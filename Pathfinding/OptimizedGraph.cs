using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Sperlich.Extensions;
using Sperlich.Pathfinding;
using UnityEngine;
using DelaunayTriangulation;
using static Sperlich.Debug.Draw.Draw;

public class OptimizedGraph : AIGraphBase {

	public Vector2 size;
	public float gridPrecision;
	public float obscureRadius;
	public float maxStepAngle;
	public float levelCastHeight;
	public float mergePointsDistance;
	public float maxRandomPoints;
	public int randomSeed;
	public PointsMethod gridMethod;
	[Header("Triangulation")]
	public int maximumNeighbours;
	public float maxNeighbourHeightDiff;
	public float neighbourHoodSize;
	public float neighbourDistance;
	[Header("Pathfinding")]
	public Transform pathFindTestStart;
	public Transform pathFindTestEnd;
	public LayerMask generateMask;
	public LayerMask obscureCeilLayer;
	public LayerMask obscureLayer;
	private List<Triangle> triangles;
	public class Triangle {
		public Vertex pointA;
		public Vertex pointB;
		public Vertex pointC;
		public Edge Edge1 => new Edge(pointA, pointB);
		public Edge Edge2 => new Edge(pointB, pointC);

		public Triangle() { }
		public Triangle(Vertex a, Vertex b, Vertex c) {
			pointA = a;
			pointB = b;
			pointC = c;
		}
	}
	public class Vertex {
		public Vector3 position;
		public Vector3 normal;
		public float x => position.x;
		public float y => position.y;
		public float z => position.z;
		public bool isValid;
		public float isoValue;

		public Vertex(Vector3 position, string tag) {
			this.position = position;
			this.normal = Vector3.zero;
			isValid = true;
			isoValue = 0;
		}
		public Vertex(Vector3 position, float isoValue, bool isValid) {
			this.position = position;
			this.isoValue = isoValue;
			this.isValid = isValid;
			this.normal = Vector3.zero;
		}
		public Vertex(Vector3 position, Vector3 normal, float isoValue, bool isValid) {
			this.position = position;
			this.isoValue = isoValue;
			this.isValid = isValid;
			this.normal = normal;
		}

		public static bool operator ==(Vertex a, Vertex b) {
			return a.Equals(b);
		}
		public static bool operator !=(Vertex a, Vertex b) {
			return !a.Equals(b);
		}
		public static Vector3 operator -(Vertex a, Vertex b) => a.position - b.position;
		public static Vector3 operator +(Vertex a, Vertex b) => a.position + b.position;
		public override bool Equals(object obj) => obj is Vertex point && this.Equals(point);
		public bool Equals(Vertex other) => this.position.x == other.position.x && this.position.y == other.position.y && this.position.z == other.position.z;
		public override int GetHashCode() => -1209761766 + this.position.x.GetHashCode() + this.position.y.GetHashCode() + this.position.z.GetHashCode();
	}
	public class Edge {

		public Vertex from;
		public Vertex to;
		public Vector3 MidPoint => (from.position + to.position) / 2;

		public Edge(Vertex from, Vertex to) {
			this.from = from;
			this.to = to;
		}

		public static bool operator ==(Edge a, Edge b) {
			return a.Equals(b);
		}
		public static bool operator !=(Edge a, Edge b) {
			return !a.Equals(b);
		}
		public override bool Equals(object obj) => obj is Vertex edge && this.Equals(edge);
		public bool Equals(Edge other) => this.from.position == other.from.position && this.to.position == other.to.position;
		public override int GetHashCode() => -1009761766 + this.from.GetHashCode() + this.to.GetHashCode();
	}

	[Button]
    public void Generate() => GenerateGraph().Forget();
	[Button]
	public void Triangulation() => Triangulate().Forget();
    async UniTaskVoid GenerateGraph() {
		KillAll();
		Random.InitState(randomSeed);
		List<Vertex> points = new List<Vertex>();
		triangles = new List<Triangle>();
		List<int> tris = new List<int>();

		int current = 0;
		// Generate Points
		List<Vector3> genPoints = new List<Vector3>();
		switch (gridMethod) {
			case PointsMethod.Grid:
				genPoints = GetGridPoints();
				break;
			case PointsMethod.Triangle:
				genPoints = GetTrianglePoints(size, 999);
				break;
			case PointsMethod.RandomGrid:
				genPoints = GetRandomGridPoints();
				break;
			case PointsMethod.RandomTriangle:
				genPoints = GetRandomTrianglePoints(size, 999);
				break;
		}
		foreach (var pointXZ in genPoints) {
			// Raycast down to get all hit points
			RaycastHit[] hits = Physics.RaycastAll(pointXZ, Vector3.down, Mathf.Infinity, generateMask);

			// Loop the hits beginning from the highest point
			foreach (var hit in hits.OrderByDescending(p => p.point.y)) {
				// Limit the Angle a hit is allowed to have
				// Limit to a desired height, if there is not enough height above the point, it will be discarded.
				bool hasCeiling = Physics.Raycast(hit.point, Vector3.up, out RaycastHit ceilHeit, levelCastHeight, generateMask);
				if(hasCeiling) {
					Line(hit.point, ceilHeit.point, 2, Color.red, 0.5f, true);
				}
				if (Vector3.Angle(hit.normal, Vector3.up) < maxStepAngle && hasCeiling == false) {
					// Is the hit object within the ObscureCeilLayer ?
					if (hit.transform.gameObject.IsInLayerMask(obscureCeilLayer)) {
						//Sphere(hit.point, 1f, Color.magenta, 2f);
						points.Add(new Vertex(hit.point, hit.normal, 0, false));
						break;
					}
					// Is the point overlapping with objects from the ObscureLayer ?
					if (Physics.OverlapSphere(hit.point, obscureRadius, obscureLayer).Length == 0) {
						points.Add(new Vertex(hit.point, hit.normal, hit.point.y.Remap(-5.5f, 16, 0f, 1f), true));
						Sphere(hit.point, 1f, Color.green, 0.5f, true);
					} else {
						points.Add(new Vertex(hit.point, hit.normal, 0, false));
						Sphere(hit.point, 1f, Color.red, 0.5f, true);
					}
				} else {
					Sphere(hit.point, 1f, Color.yellow, 0.5f, true);
				}
			}
			if(current % 60 == 0) {
				await UniTask.Delay(1);
			}
			current++;
		}
		// Remove duplicates
		current = 0;
		points = await RemoveDuplicates(points);
		points = await MergePoints(points, MergeMethod.First, mergePointsDistance);

		// 
		float sqrNeighbourHoodSize = neighbourHoodSize * neighbourHoodSize;
		int total;
		foreach(var point in points) {
			List<(Vertex point, float dist)> tmpNeighs = new List<(Vertex, float)>();
			List<Vertex> finalNeighs = new List<Vertex>();
			Plane plane = new Plane(point.normal, point.position);
			Color color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
			var neighs = points.Where(p => p != point && (point.position - p.position).sqrMagnitude < sqrNeighbourHoodSize).ToArray();
			//Sphere(point.position, 0.7f, color, 15, true);
			//Rectangle(point.position, point.normal, new Vector2(5, 5), color, 15, true);

			for (int i = 0; i < neighs.Length; i++) {
				float heightDiff = Mathf.Abs(plane.GetDistanceToPoint(neighs[i].position));
				if (heightDiff < maxNeighbourHeightDiff) {
					Vector3 projectedPos = neighs[i].position + Vector3.up * heightDiff;
					float dist = Vector3.Distance(projectedPos, point.position);
					if (dist < neighbourDistance) {
						//Line(projectedPos, point.position, 3, color, 15, true);
						//tris.Add(points.IndexOf(neighs[i]));
						neighs[i].position = projectedPos;
						tmpNeighs.Add((neighs[i], dist));
					}
				}
			}
			finalNeighs = tmpNeighs.OrderBy(p => p.dist).Select(p => p.point).Take(maximumNeighbours).ToList();

			for(int i = 0; i < finalNeighs.Count && finalNeighs.Count == 3; i++) {
				tris.Add(points.FindIndex(e => e.Equals(finalNeighs[i] as object)));
			}
			//finalNeighs.ForEach(p => Line(point.position, p.position, 3, color, 15, true));


			if (current % 2 == 0) {
				await UniTask.Delay(5);
			}
			current++;
		}

		Debug.Log(tris.Count + " : " + points.Count);
		for(int i = 3; i < tris.Count - 3; i += 3) {
			//triangles.Add(new Triangle(points[tris[i]], points[tris[i - 1]], points[tris[i - 2]]));
		}

		foreach(var tri in triangles) {
			Color color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
			//Line(tri.Edge1.from.position, tri.Edge1.to.position, 4, color, 15, true);
			//Line(tri.Edge1.to.position, tri.Edge2.from.position, 4, color, 15, true);
			//Line(tri.Edge2.from.position, tri.Edge2.to.position, 4, color, 15, true);
		}

		return;
		foreach(var p in points) {
			Line(p.position, p.position + p.normal * 2, Color.black, 15, true);
			Sphere(p.position, 1f, p.isValid ? Color.Lerp(Color.green, Color.red, p.isoValue) : Color.black, 15, true);
		}
	}
	async UniTaskVoid Triangulate() {
		KillAll();
		Random.InitState(randomSeed);
		List<Vertex> points = new List<Vertex>();
		triangles = new List<Triangle>();
		List<int> tris = new List<int>();

		int current = 0;
		// Generate Points
		List<Vector3> genPoints = new List<Vector3>();
		switch (gridMethod) {
			case PointsMethod.Grid:
				genPoints = GetGridPoints();
				break;
			case PointsMethod.Triangle:
				genPoints = GetTrianglePoints(size, 999);
				break;
			case PointsMethod.RandomGrid:
				genPoints = GetRandomGridPoints();
				break;
			case PointsMethod.RandomTriangle:
				genPoints = GetRandomTrianglePoints(size, 999);
				break;
		}
		foreach (var pointXZ in genPoints) {
			// Raycast down to get all hit points
			RaycastHit[] hits = Physics.RaycastAll(pointXZ, Vector3.down, Mathf.Infinity, generateMask);

			// Loop the hits beginning from the highest point
			foreach (var hit in hits.OrderByDescending(p => p.point.y)) {
				// Limit the Angle a hit is allowed to have
				// Limit to a desired height, if there is not enough height above the point, it will be discarded.
				bool hasCeiling = Physics.Raycast(hit.point, Vector3.up, out RaycastHit ceilHeit, levelCastHeight, generateMask);
				if (hasCeiling) {
					Line(hit.point, ceilHeit.point, 2, Color.red, 0.5f, true);
				}
				if (Vector3.Angle(hit.normal, Vector3.up) < maxStepAngle && hasCeiling == false) {
					// Is the hit object within the ObscureCeilLayer ?
					if (hit.transform.gameObject.IsInLayerMask(obscureCeilLayer)) {
						//Sphere(hit.point, 1f, Color.magenta, 2f);
						points.Add(new Vertex(hit.point, hit.normal, 0, false));
						break;
					}
					// Is the point overlapping with objects from the ObscureLayer ?
					if (Physics.OverlapSphere(hit.point, obscureRadius, obscureLayer).Length == 0) {
						points.Add(new Vertex(hit.point, hit.normal, hit.point.y.Remap(-5.5f, 16, 0f, 1f), true));
						Sphere(hit.point, 1f, Color.green, 0.5f, true);
					} else {
						points.Add(new Vertex(hit.point, hit.normal, 0, false));
						Sphere(hit.point, 1f, Color.red, 0.5f, true);
					}
				} else {
					Sphere(hit.point, 1f, Color.yellow, 0.5f, true);
				}
			}
			if (current % 60 == 0) {
				await UniTask.Delay(1);
			}
			current++;
		}
		// Remove duplicates
		current = 0;
		points = await RemoveDuplicates(points);
		points = await MergePoints(points, MergeMethod.First, mergePointsDistance);

		List<DelaunayTriangulation.Vertex> vertexes = new List<DelaunayTriangulation.Vertex>();
		for(int i = 0; i < points.Count; i++) {
			vertexes.Add(new DelaunayTriangulation.Vertex(new Vector2(points[i].position.x, points[i].position.z), i));
		}
		Triangulation triang = new Triangulation(vertexes);

		foreach (DelaunayTriangulation.Triangle tri in triang.triangles) {
			Color color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
			Line(tri.edge0.point0.Pos3D, tri.edge0.point1.Pos3D, 4, color, 15, false);
			Line(tri.edge1.point0.Pos3D, tri.edge1.point1.Pos3D, 4, color, 15, false);
			Line(tri.edge2.point0.Pos3D, tri.edge2.point1.Pos3D, 4, color, 15, false);
		}
	}
	async UniTask<List<Vertex>> RemoveDuplicates(List<Vertex> points) {
		List<Vertex> duplicates = new List<Vertex>();
		int current = 0;

		foreach (var point in points) {
			var similiar = points.Where(p => p.x == point.x && p.z == point.z).ToList();
			if (similiar.Count > 1) {
				duplicates.AddRange(similiar.Where(p => p != point && p.isValid == false));
			} else if (point.isValid == false) {
				duplicates.Add(point);
			} else {
				Sphere(point.position, 1.5f, Color.red, 0.1f, true);
			}

			if (current % 200 == 0) {
				await UniTask.Delay(1);
			}
			current++;
		}
		return points.Except(duplicates).ToList();
	}
	List<Vector3> GetGridPoints() {
		List<Vector3> points = new List<Vector3>();
		for (float appX = -size.x; appX < size.x; appX += gridPrecision) {
			for (float appZ = -size.y; appZ < size.y; appZ += gridPrecision) {
				points.Add(new Vector3(appX, 999, appZ));
			}
		}
		return points;
	}
	List<Vector3> GetRandomGridPoints() {
		List<Vector3> points = new List<Vector3>();
		for (float appX = -size.x; appX < size.x; appX += gridPrecision) {
			for (float appZ = -size.y; appZ < size.y; appZ += gridPrecision) {
				points.Add(new Vector3(appX + Random.Range(-maxRandomPoints, maxRandomPoints), 999, appZ + Random.Range(-maxRandomPoints, maxRandomPoints)));
			}
		}
		return points;
	}
	List<Vector3> GetTrianglePoints(Vector2 size, float height) {
		List<Vector3> points = new List<Vector3>();
		bool inverse = false;
		for (float appX = -size.x; appX < size.x; appX += gridPrecision) {
			for (float appZ = -size.y; appZ < size.y; appZ += gridPrecision) {
				Vector3 point = new Vector3(appX, height, appZ);
				if (inverse) {
					point.z += gridPrecision / 2f;
				}
				points.Add(point);
			}
			inverse = !inverse;
		}
		return points;
	}
	List<Vector3> GetRandomTrianglePoints(Vector2 size, float height) {
		List<Vector3> points = new List<Vector3>();
		bool inverse = false;
		for (float appX = -size.x; appX < size.x; appX += gridPrecision) {
			for (float appZ = -size.y; appZ < size.y; appZ += gridPrecision) {
				Vector3 point = new Vector3(appX + Random.Range(-maxRandomPoints, maxRandomPoints), height, appZ + Random.Range(-maxRandomPoints, maxRandomPoints));
				if (inverse) {
					point.z += gridPrecision / 2f;
				}
				points.Add(point);
			}
			inverse = !inverse;
		}
		return points;
	}
	async UniTask<List<Vertex>> MergePoints(List<Vertex> points, MergeMethod method, float mergeDistance) {
		List<Vertex> mergedPoints = new List<Vertex>();
		List<Vertex> newPoints = new List<Vertex>();
		float step = 1f / points.Count;

		for (int i = 0; i < points.Count; i++) {
			var point = points[i];
			if (mergedPoints.Contains(point)) {
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
						newPoints.Add(new Vertex(average, ""));
					} else if (nearby.Count == 0) {
						newPoints.Add(point);
					}
					break;
			}
			//Sphere(point.position, 1f, Color.blue, 0.25f, false);
			//foreach (var e in nearby) {
				//Sphere(e.position, 1f, Color.magenta, 0.25f, false);
			//}

			if (i % 40 == 0) {
				await UniTask.Delay(1);
			}
			//AddProgress(step, GenerationStep.Merge);
		}
		Debug.Log(mergedPoints.Count + " Points have been removed.");
		return newPoints;
	}

	void OnDrawGizmos() {
		if (showGizmos) {
			if (Graph != null && Graph.points != null && Graph.edges != null && Graph.nodes.Count > 0) {
				/*PathfindHelper.Refresh();

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
				FindPath(pathFindTestFrom.position, pathFindTestGoal.position);*/
			}

			if (Graph != null && Graph.points != null && Graph.points.Count > 0) {
				
				//if (showRawPath) {
					//PaintPath(FindPath(pathFindTestFrom.position, pathFindTestGoal.position), Color.blue);
				//}
				//PaintPath(SmoothPath(FindPath(pathFindTestFrom.position, pathFindTestGoal.position), pathInterpolateSteps), Color.red);
			}
			Line(transform.position + new Vector3(-size.x, 0, size.y), transform.position + new Vector3(size.x, 0, size.y), 5, Color.black, false);
			Line(transform.position + new Vector3(-size.x, 0, -size.y), transform.position + new Vector3(size.x, 0, -size.y), 5, Color.black, false);
			Line(transform.position + new Vector3(-size.x, 0, size.y), transform.position + new Vector3(-size.x, 0, -size.y), 5, Color.black, false);
			Line(transform.position + new Vector3(size.x, 0, size.y), transform.position + new Vector3(size.x, 0, -size.y), 5, Color.black, false);
		}
	}
}
