using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Primitives;
using UnityEngine;

namespace Sperlich.Pathfinding {
	[System.Serializable]
	public class SGraph {
		public List<SPoint> points;
		public List<SEdge> edges;
		public List<Node> nodes;
		public int discardedPointCount;

		public SGraph() {
			points = new List<SPoint>();
			edges = new List<SEdge>();
			nodes = new List<Node>();
		}
		public SGraph(List<SPoint> points, List<SEdge> edges) {
			this.points = points;
			this.edges = edges;
			this.nodes = new List<Node>();
		}

		public INode GetNearest(Vector3 pos) {
			if (nodes.Count == 0) return new Node(new Position(pos.x, pos.z));
			return nodes.OrderBy(n => (pos - n.Position.Vector3).sqrMagnitude).First();
		}
	}
}