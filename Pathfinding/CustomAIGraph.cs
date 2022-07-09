using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Primitives;
using Sperlich.Extensions;
using Sperlich.Pathfinding;
using UnityEngine;
using static Sperlich.Debug.Draw.Draw;

public class CustomAIGraph : AIGraphBase {

	//public float connectRadius;
	public bool updateInstantly;
	public bool showNumbers;
	public LayerMask castMask;

	[Button]
	public void Generate() => GenerateFromPoints();
	void GenerateFromPoints() {
		List<(SPoint spoint, ManualPoint mpoint)> points = new List<(SPoint, ManualPoint)>();
		List<SEdge> edges = new List<SEdge>();
		int count = 0;
		foreach (ManualPoint t in transform.GetComponentsInChildren<ManualPoint>()) {
			if (Physics.Raycast(t.Pos, Vector3.down, out RaycastHit hit, Mathf.Infinity, castMask)) {
				points.Add((new SPoint(new Vector3(t.Pos.x, hit.point.y, t.Pos.z), ""), t));
			}
			t.name = count + "";
			count++;
		}

		foreach(var point in points) {
			//var nearby = points.Where(p => p != point && (point.position - p.position).magnitude < connectRadius).ToList();
			var nearby = points.Where(p => point.mpoint.connections.Contains(p.mpoint.index));
			foreach (var n in nearby) {
				edges.Add(new SEdge(point.spoint, n.spoint));
			}
		}

		List<Node> graphNodes = new List<Node>();
		foreach(var p in points.Select(p => p.spoint)) {
			graphNodes.Add(new Node(new Position(p.x, p.y, p.z)));
		}
		
		Graph = new SGraph(points.Select(p => p.spoint).ToList(), edges);

		for (int i = 0; i < graphNodes.Count; i++) {
			foreach (var cns in edges.Where(e => e.from.position == points[i].spoint.position)) {
				var connectPoint = graphNodes.Where(n => n.Position.Vector3 == cns.to.position).FirstOrDefault();
				if (connectPoint != null) {
					float dist = Vector2.Distance(graphNodes[i].Position.Vector2, connectPoint.Position.Vector2);
					graphNodes[i].Connect(connectPoint, dist);
				}
			}
			Graph.nodes.Add(graphNodes[i]);
		}
	}

	private void OnDrawGizmos() {
		if (updateInstantly) {
			GenerateFromPoints();
		}
		if(showGizmos) {
			if (Graph != null && Graph.points != null && Graph.edges != null && Graph.nodes.Count > 0) {
				PathfindHelper.Refresh();
				foreach (var n in Graph.nodes) {
					foreach (var e in n.Outgoing) {
						//Line(e.Start.Position.Vector3 + Vector3.up * 2, e.End.Position.Vector3 + Vector3.up * 2, 4, Color.Lerp(Color.black, Color.red, e.Distance.Meters.Remap(0, 16, 0f, 1f)), true);
					}
				}
				foreach(var p in Graph.points) {
					//Sphere(p.position, 2f, Color.yellow, true);
				}
			}
		}
	}
}
