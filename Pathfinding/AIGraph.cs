using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sperlich.Extensions;
using Sperlich.Types;
using UnityEngine;
using NaughtyAttributes;
using Shapes;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Primitives;
using static Sperlich.Debug.Draw.Draw;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sperlich.Pathfinding {
	[ExecuteInEditMode]
	public class AIGraph : AIGraphBase {

		public Int2 size;
		[Range(2, 16)]
		public float appPrecision;
		[Range(0, 30)]
		public float reduceRadius;
		[Range(0f, 90)]
		public float maxStepAngle = 45;
		[Range(2, 50)]
		public float maxConnectDistance = 10;
		[Range(0f, 10)]
		public float maxConnectHeight = 4;
		[Range(0f, 40)]
		public float mergeDistance;
		public float overlapDistance;
		public int cutAtOverlapCount;
		public float obscureRadius;
		public float obscureHeight;
		public float levelCastHeight;
		public float connectChunkRadius;
		public int maxConnections;
		public int pathInterpolateSteps;
		[Range(1, 16)]
		public int splitChunks;
		public MergeMethod reduceMethod;
		public MergeMethod mergeMethod;
		public MergeMoment mergeMoment;
		public PointsMethod pointsMethod;
		public bool reduce;
		public bool checkOverlaps;
		public bool showRawPath;
		[Range(0, 1000)]
		public float viewDistance;
		public Transform pathFindTestFrom;
		public Transform pathFindTestGoal;
		public LayerMask connectMask;
		public LayerMask generateMask;
		public LayerMask obscureLayer;
		public LayerMask obscureCeilLayer;
		private float _progress;
		public float CurrentProgress {
			get => _progress;
			set => _progress = Mathf.Clamp01(value);
		}
		public double PathFindGenerationTime { get; set; }
		private List<Sphere> gizmoSpheres = new List<Sphere>();
		public int TotalChunks => (int)Mathf.Pow(splitChunks * 2, 2);

		[Button]
		public void GenerateGrid() => Generate().Forget();
		async UniTaskVoid Generate() {
			IsGenerating = true;
			CancelGeneration = false;
			CurrentProgress = 0;

			Graph = new SGraph();
			// Reset Gizmos and Progressions
			GameObject parent = GameObject.Find("PathfindGizmoHolder");
			if (parent != null) {
				DestroyImmediate(parent);
			}
			gizmoSpheres = new List<Sphere>();

			// Generate the Base-Grid
			List<SPoint> points = new List<SPoint>();
			List<Vector3> genPoints = new List<Vector3>();

			switch (pointsMethod) {
				case PointsMethod.Grid:
					genPoints = GetGridPoints(size);
					break;
				case PointsMethod.Triangle:
					genPoints = GetTrianglePoints(size, 999);
					break;
			}
			float step = 1f / genPoints.Count;
			int currentPoint = 0;
			foreach (var pointXZ in genPoints) {
				// Raycast down to get all hit points
				RaycastHit[] hits = Physics.RaycastAll(pointXZ, Vector3.down, Mathf.Infinity, generateMask);

				// Loop the hits beginning from the highest point
				foreach (var hit in hits.OrderByDescending(p => p.point.y)) {
					// Limit the Angle a hit is allowed to have
					// Limit to a desired height, if there is not enough height above the point, it will be discarded.
					if (Vector3.Angle(hit.normal, Vector3.up) < maxStepAngle && Physics.Raycast(hit.point, Vector3.up, out RaycastHit ceilHeit, levelCastHeight, generateMask) == false) {
						// Is the hit object within the ObscureCeilLayer ?
						if (hit.transform.gameObject.IsInLayerMask(obscureCeilLayer)) {
							Sphere(hit.point, 1f, Color.black, 0.5f);
							break;
						}
						// Is the point overlapping with objects from the ObscureLayer ?
						if (Physics.OverlapSphere(hit.point, obscureRadius, obscureLayer).Length == 0) {
							points.Add(new SPoint(hit.point, ""));
							Sphere(hit.point, 1f, Color.green, 0.5f, true);
						} else {
							Sphere(hit.point, 1f, Color.red, 0.5f, true);
						}
					}
				}

				// Cancel & Feedback-Progression
				if (currentPoint % 40 == 0) {
					await UniTask.Delay(1);
				}
				if (CancelGeneration) {
					return;
				}
				currentPoint++;
				AddProgress(step, GenerationStep.Generate);
			}


			HashSet<SEdge> allEdges = new HashSet<SEdge>();
			List<SPoint> allPoints = new List<SPoint>();

			List<(List<SPoint> points, List<SEdge> edges, Vector2Int chunkIndex)> chunks = new List<(List<SPoint>, List<SEdge>, Vector2Int)>();
			for (int x = -splitChunks; x < splitChunks; x++) {
				for (int z = -splitChunks; z < splitChunks; z++) {
					// Collect Chunk-Points
					var chunkEdges = new List<SEdge>();
					List<SPoint> chunkPoints = GetPointsWithinBounds(points, new Vector2(size.x / splitChunks * x, size.y / splitChunks * z), new Vector2(size.x / splitChunks, size.y / splitChunks));

					// Reduce points and create Connections
					if (mergeMoment != MergeMoment.DontMerge && (mergeMoment == MergeMoment.Before || mergeMoment == MergeMoment.Both)) {
						var result = await MergePoints(chunkPoints, mergeMethod, mergeDistance);
						chunkPoints = result.mergedPoints;
					}


					List<SPoint> chunkMinPoints = new List<SPoint>(chunkPoints);
					if (reduce) {
						chunkMinPoints = await Simplify(chunkPoints, reduceMethod);
						if (mergeMoment != MergeMoment.DontMerge && (mergeMoment == MergeMoment.After || mergeMoment == MergeMoment.Both)) {
							var result = await MergePoints(chunkMinPoints, mergeMethod, mergeDistance);
							chunkMinPoints = result.mergedPoints;
						}

					}

					//allEdges.AddRange(chunkEdges);
					allPoints.AddRange(chunkMinPoints);
					chunks.Add((new List<SPoint>(chunkMinPoints), new List<SEdge>(chunkEdges), new Vector2Int(x, z)));
				}
			}
			for (int i = 0; i < chunks.Count; i++) {
				var chunk = chunks[i];
				Vector2 origin = new Vector2(size.x / splitChunks * chunk.chunkIndex.x, size.y / splitChunks * chunk.chunkIndex.y);
				Vector2 bounds = new Vector2(size.x / splitChunks, size.y / splitChunks);
				bounds += new Vector2(connectChunkRadius, connectChunkRadius);
				List<SPoint> boundPoints = GetPointsWithinBounds(allPoints, origin, bounds);
				var chunkEdges = await MakeConnections(boundPoints, checkOverlaps);

				for (int m = 0; m < chunkEdges.Count; m++) {
					if (allEdges.Contains(new SEdge(chunkEdges[m].from, chunkEdges[m].to)) == false) {
						allEdges.Add(chunkEdges[m]);
					}
				}
			}

			// Generate the Nodes from the simplified Points
			List<Node> graphNodes = new List<Node>();
			for (int i = 0; i < allPoints.Count; i++) {
				graphNodes.Add(new Node(new Position(allPoints[i].x, allPoints[i].y, allPoints[i].z)));
			}

			// Connect the Nodes with the help of the generated Connections
			Graph = new SGraph(allPoints, allEdges.ToList());
			step = 1f / graphNodes.Count;
			CurrentProgress = 0;

			for (int i = 0; i < graphNodes.Count; i++) {
				foreach (var cns in allEdges.Where(e => e.from.position == allPoints[i].position)) {
					var connectPoint = graphNodes.Where(n => n.Position.Vector3 == cns.to.position).FirstOrDefault();
					if (connectPoint != null) {
						float dist = Vector3.Distance(graphNodes[i].Position.Vector3XZ, connectPoint.Position.Vector3XZ);
						graphNodes[i].Connect(connectPoint, dist);
					}
				}
				Graph.nodes.Add(graphNodes[i]);

				// Cancel & Feedback-Progression
				if (i % 20 == 0) {
					await UniTask.Delay(1);
				}
				if (CancelGeneration) {
					return;
				}
				AddProgress(step, GenerationStep.Finish);
			}
			/*List<Node> rejectedNodes = new List<Node>();
			foreach(var n in Graph.nodes) {
				if(n.Outgoing.Count <= 1) {
					rejectedNodes.Add(n);
					while(n.Outgoing.Count > 0) {
						n.Disconnect(n.Outgoing[0].End);
					}
				}
			}
			Graph.nodes = Graph.nodes.Except(rejectedNodes).ToList();*/

			// Create Sphere Gizmos
			if (showGizmos) {
				gizmoSpheres = new List<Sphere>();
				parent = new GameObject("PathfindGizmoHolder");

				/*
				foreach (var n in allPoints) {
					var s = new GameObject().AddComponent<Sphere>();
					s.transform.SetParent(parent.transform);
					s.transform.position = n.position + Vector3.up * 2;
					s.Radius = 1f;
					s.Color = Color.blue;
					s.BlendMode = ShapesBlendMode.Transparent;
					s.DetailLevel = DetailLevel.Minimal;
					gizmoSpheres.Add(s);
				}*/
				UnityEngine.Debug.Log("MAX Outgoing: " + Graph.nodes.SelectMany(n => n.Outgoing).Max(e => e.Distance));
				UnityEngine.Debug.Log("MAX Incoming: " + Graph.nodes.SelectMany(n => n.Incoming).Max(e => e.Distance));
			}
			IsGenerating = false;
			CancelGeneration = false;
		}
		List<Vector3> GetGridPoints(Vector2 size) {
			List<Vector3> points = new List<Vector3>();
			for (float appX = -size.x; appX < size.x; appX += appPrecision) {
				for (float appZ = -size.y; appZ < size.y; appZ += appPrecision) {
					points.Add(new Vector3(appX, 999, appZ));
				}
			}
			return points;
		}
		List<Vector3> GetTrianglePoints(Vector2 size, float height) {
			List<Vector3> points = new List<Vector3>();
			bool inverse = false;
			for (float appX = -size.x; appX < size.x; appX += appPrecision) {
				for (float appZ = -size.y; appZ < size.y; appZ += appPrecision) {
					Vector3 point = new Vector3(appX, height, appZ);
					if (inverse) {
						point.z += appPrecision / 2f;
					}
					points.Add(point);
				}
				inverse = !inverse;
			}
			return points;
		}
		async UniTask<List<SPoint>> Simplify(List<SPoint> points, MergeMethod method) {
			CurrentProgress = 0;
			List<SPoint> reduceExcludePoints = new List<SPoint>();
			float step = 1f / points.Count;
			var minifiedPoints = new List<SPoint>();

			for (int i = 0; i < points.Count; i++) {
				var p = points[i];
				if (reduceExcludePoints.Contains(p) == false) {
					var ps = points.Where(cp => (cp - p).magnitude < reduceRadius).Except(reduceExcludePoints).ToArray();

					switch (method) {
						case MergeMethod.First:
							break;
						case MergeMethod.Last:
							break;
						case MergeMethod.Average:
							break;
					}

					Vector3 averages = new Vector3(ps.Average(p => p.x), ps.Average(p => p.y), ps.Average(p => p.z));
					minifiedPoints.Add(new SPoint(averages, ""));
					reduceExcludePoints.AddRange(ps);
					Sphere(averages, 2f, Color.yellow, 0.5f, false);

					for (int x = 0; x < ps.Count(); x++) {
						Line(ps[x].position, averages, 1f, Color.blue, 0.5f, false);
					}

					minifiedPoints.Add(p);
				}
				if (i % 100 == 0) {
					await UniTask.Delay(1);
				}
				if (CancelGeneration) {
					return points;
				}
				AddProgress(step, GenerationStep.Reduce);
			}
			return minifiedPoints;
		}
		async UniTask<(List<SPoint> mergedPoints, int removedPointsCount)> MergePoints(List<SPoint> points, MergeMethod method, float mergeDistance) {
			CurrentProgress = 0;
			List<SPoint> mergedPoints = new List<SPoint>();
			List<SPoint> newPoints = new List<SPoint>();
			float step = 1f / points.Count;

			for (int i = 0; i < points.Count; i++) {
				var point = points[i];
				if (mergedPoints.Contains(point)) {
					CurrentProgress += step;
					continue;
				}
				var nearby = points.Except(mergedPoints).Where(p => p != point && (point - p).magnitude < mergeDistance).ToList();

				switch (method) {
					case MergeMethod.First:
						mergedPoints.AddRange(nearby);
						newPoints.Add(point);
						break;
					case MergeMethod.Last:
						if (nearby.Count > 0) {
							var lastPoint = nearby[nearby.Count - 1];
							nearby = nearby.Take(nearby.Count - 1).ToList();
							mergedPoints.Add(point);
							mergedPoints.AddRange(nearby);
							newPoints.Add(lastPoint);
						} else if (nearby.Count == 0) {
							newPoints.Add(point);
						}
						break;
					case MergeMethod.Average:
						if (nearby.Count > 1) {
							mergedPoints.Add(point);
							mergedPoints.AddRange(nearby);
							nearby.Add(point);
							Vector3 average = new Vector3(nearby.Average(p => p.x), nearby.Average(p => p.y), nearby.Average(p => p.z));
							newPoints.Add(new SPoint(average, ""));
						} else if (nearby.Count == 0) {
							newPoints.Add(point);
						}
						break;
				}
				Sphere(point.position, 1f, Color.blue, 0.25f, false);
				foreach (var e in nearby) {
					Sphere(e.position, 1f, Color.magenta, 0.25f, false);
				}

				if (i % 40 == 0) {
					await UniTask.Delay(1);
				}
				if (CancelGeneration) {
					return (newPoints, mergedPoints.Count);
				}
				AddProgress(step, GenerationStep.Merge);
			}
			UnityEngine.Debug.Log(mergedPoints.Count + " Points have been removed.");
			return (newPoints, mergedPoints.Count);
		}
		async UniTask<List<SEdge>> MakeConnections(List<SPoint> points, bool checkOverlaps = true) {
			CurrentProgress = 0;
			float step = 1f / points.Count;
			float sqrMaxConnectDistance = maxConnectDistance * maxConnectDistance;
			List<SEdge> connects = new List<SEdge>();

			for (int i = 0; i < points.Count; i++) {
				var point = points[i];
				List<SEdge> tmpEdges = new List<SEdge>();

				// Get Points within Reach and Height
				// Height is seperated as a value
				foreach (var n in points.Where(p => (point.Vector2 - p.Vector2).sqrMagnitude < sqrMaxConnectDistance)) {
					if (Mathf.Abs(n.y - point.y) < maxConnectHeight) {
						tmpEdges.Add(new SEdge(point, n));
					}
				}

				tmpEdges = tmpEdges.OrderBy(p => (p.from - p.to).sqrMagnitude).ToList();
				Sphere(point.position, 1f, Color.black, 0.15f, false);

				for (int m = 0; m < maxConnections && m < tmpEdges.Count; m++) {
					connects.Add(tmpEdges[m]);
				}

				// Cancel & Progression-Feedback
				if (i % 100 == 0) {
					await UniTask.Delay(1);
				}
				if (CancelGeneration) {
					return connects;
				}
				AddProgress(step, GenerationStep.Connect);
			}

			if (checkOverlaps) {
				List<SEdge> rejectedEdges = new List<SEdge>();
				CurrentProgress = 0;
				int count = 0;
				float sqrOverlapDistance = overlapDistance * overlapDistance;
				step = 1f / connects.Count;

				foreach (var edge in connects) {
					List<SEdge> filtered = new List<SEdge>();
					List<SEdge> nearbyConnects = connects.Except(rejectedEdges).Where(e => Mathf.Abs(e.MidPoint.y - edge.MidPoint.y) < 1 && e != edge && (e.MidPoint.Vector2XZ() - edge.MidPoint.Vector2XZ()).sqrMagnitude < sqrOverlapDistance).ToList();

					int overlaps = 0;

					foreach (var neighbour in nearbyConnects) {
						float sub = -0.5f;
						Vector2 l1_p1 = edge.from.position.Vector2XZ();
						Vector2 l1_p2 = edge.to.position.Vector2XZ();
						Vector2 l1_dir = (l1_p2 - l1_p1).normalized;

						Vector2 l2_p1 = neighbour.from.position.Vector2XZ();
						Vector2 l2_p2 = neighbour.to.position.Vector2XZ();
						Vector2 l2_dir = (l2_p2 - l2_p1).normalized;

						l1_p1 -= l1_dir * sub;
						l1_p2 += l1_dir * sub;

						l2_p1 -= l2_dir * sub;
						l2_p2 += l2_dir * sub;

						if (AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, false)) {
							Line(edge.MidPoint, neighbour.MidPoint, 4, Color.cyan, 0.2f, false);
							Line(new Vector3(l2_p1.x, neighbour.from.y, l2_p1.y), new Vector3(l2_p2.x, neighbour.from.y, l2_p2.y), 4, Color.red, 0.2f, false);
							overlaps++;
							if (overlaps >= cutAtOverlapCount) {
								continue;
							}
						}
					}
					if (overlaps >= cutAtOverlapCount) {
						rejectedEdges.Add(edge);
					}

					if (count % 80 == 0) {
						await UniTask.Delay(1);
					}
					if (CancelGeneration) {
						return connects;
					}
					AddProgress(step, GenerationStep.Connect);
					count++;
				}

				connects = connects.Except(rejectedEdges).ToList();
				UnityEngine.Debug.Log(rejectedEdges.Count + " Connections removed.");
			}

			return connects;
		}
		void AddProgress(float amount, GenerationStep currentStep) {
			CurrentStep = currentStep;
			CurrentProgress += amount;
		}
		public static void RepaintInspector(System.Type t) {
			Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
			for (int i = 0; i < ed.Length; i++) {
				if (ed[i].GetType() == t) {
					ed[i].Repaint();
					return;
				}
			}
		}

		[Button]
		public void Cancel() {
			CancelGeneration = true;
			IsGenerating = false;
			gizmoSpheres = new List<Sphere>();
		}

		void OnDrawGizmos() {
			if (showGizmos) {
				if (Graph != null && Graph.points != null && Graph.edges != null && Graph.nodes.Count > 0) {
					PathfindHelper.Refresh();

					foreach (var n in gizmoSpheres) {
						if ((n.transform.position - PathfindHelper.camPos).Vector2XZ().magnitude < viewDistance) {
							n.Show();
						} else {
							n.Hide();
						}
					}
					foreach (var n in Graph.nodes.Where(n => (n.Position.Vector3.Vector2XZ() - PathfindHelper.camPos.Vector2XZ()).magnitude < viewDistance)) {
						foreach (var e in n.Outgoing) {
							Line(e.Start.Position.Vector3 + Vector3.up * 2, e.End.Position.Vector3 + Vector3.up * 2, 4, Color.Lerp(Color.black, Color.red, e.Distance.Meters.Remap(0, 16, 0f, 1f)), true);
						}
					}
					FindPath(pathFindTestFrom.position, pathFindTestGoal.position);
				}
				if (IsGenerating && CancelGeneration == false) {
					RepaintInspector(typeof(AIGraphEditor));
				}

				if (Graph != null && Graph.points != null && Graph.points.Count > 0) {
					Line(transform.position + new Vector3(-size.x, 0, size.y), transform.position + new Vector3(size.x, 0, size.y), 5, Color.black, false);
					Line(transform.position + new Vector3(-size.x, 0, -size.y), transform.position + new Vector3(size.x, 0, -size.y), 5, Color.black, false);
					Line(transform.position + new Vector3(-size.x, 0, size.y), transform.position + new Vector3(-size.x, 0, -size.y), 5, Color.black, false);
					Line(transform.position + new Vector3(size.x, 0, size.y), transform.position + new Vector3(size.x, 0, -size.y), 5, Color.black, false);
					if (showRawPath) {
						PaintPath(FindPath(pathFindTestFrom.position, pathFindTestGoal.position), Color.blue);
					}
					PaintPath(SmoothPath(FindPath(pathFindTestFrom.position, pathFindTestGoal.position), pathInterpolateSteps), Color.red);
				}
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(AIGraph))]
	public class AIGraphEditor : Editor {
		public override void OnInspectorGUI() {
			base.DrawDefaultInspector();
			var graph = (AIGraph)target;

			if (graph.Graph != null && graph.Graph.nodes != null) {
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
				GUILayout.BeginVertical();
				GUILayout.Label("Points: " + graph.Graph.nodes.Count);
				GUILayout.Label("Edges: " + graph.Graph.edges.Count);
				GUILayout.Label("Path Time: " + graph.PathFindGenerationTime + "ms");
				GUILayout.EndVertical();
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			}

			Color col = Color.white;
			if (graph.IsGenerating) {
				switch (graph.CurrentStep) {
					case AIGraph.GenerationStep.Generate:
						col = Color.green;
						GUI.contentColor = Color.black;
						break;
					case AIGraph.GenerationStep.Merge:
						col = Color.yellow;
						GUI.contentColor = Color.white;
						break;
					case AIGraph.GenerationStep.Reduce:
						col = Color.red;
						GUI.contentColor = Color.white;
						break;
					case AIGraph.GenerationStep.Connect:
						col = Color.white;
						GUI.contentColor = Color.black;
						break;
					case AIGraph.GenerationStep.Finish:
						col = Color.blue;
						GUI.contentColor = Color.white;
						break;
				}

				var progressRect = GUILayoutUtility.GetRect(0, 20);
				var labelRect = new Rect(progressRect);
				float fullLength = progressRect.width;
				progressRect.width = graph.CurrentProgress.Remap(0f, 1f, 0f, fullLength);
				var progressBarLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
				EditorGUI.DrawRect(labelRect, new Color(0.15f, 0.15f, 0.15f, 1f));
				EditorGUI.DrawRect(progressRect, col);
				EditorGUI.LabelField(labelRect, graph.CurrentStep + " " + Mathf.RoundToInt(graph.CurrentProgress * 100) + "%", progressBarLabel);
			}

			if (graph.IsGenerating == false && GUILayout.Button("Generate")) {
				graph.GenerateGrid();
			}
			if (graph.IsGenerating && GUILayout.Button("Cancel")) {
				graph.Cancel();
			}
		}
	}
#endif
}