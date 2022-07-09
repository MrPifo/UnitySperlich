using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sperlich.Pathfinding;

public class AIPathfinder : MonoBehaviour {
	
	//public enum DetailMode { Low, Medium, High, Rough }

	public Transform from;
	public Transform to;
	public float lowCalcTime;
	public float mediumCalcTime;
	public float highCalcTime;
	public float totalTime;
	public bool showSmoothed;
	public bool showGizmos;
	//public List<DetailGraph> graphs;
	public DetailGraph LowGraph;
	public DetailGraph MediumGraph;
	public DetailGraph HighGraph;
	[System.Serializable]
	public class DetailGraph {
		public AIGraphBase graph;
		public float minRange;
	}

	public void FindPath(Vector3 start, Vector3 end) {
		if(LowGraph == null) return;
		if(MediumGraph == null) return;
		if(HighGraph == null) return;

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		watch.Start();
		List<Vector3> final = new List<Vector3>();

		List<Vector3> lowPath = LowGraph.graph.FindPath(start, end, LowGraph.minRange);
		lowCalcTime = (float)watch.Elapsed.TotalMilliseconds;
		watch.Restart();

		List<Vector3> mediumPath = MediumGraph.graph.FindPath(start, lowPath.Count == 0 ? end : lowPath[0], MediumGraph.minRange);
		mediumCalcTime = (float)watch.Elapsed.TotalMilliseconds;
		watch.Restart();

		List<Vector3> highPath = HighGraph.graph.FindPath(start, mediumPath.Count == 0 ? end : mediumPath[0], HighGraph.minRange);
		watch.Stop();
		highCalcTime = (float)watch.Elapsed.TotalMilliseconds;

		totalTime = lowCalcTime + mediumCalcTime + highCalcTime;
		
		final.AddRange(highPath);
		final.AddRange(mediumPath);
		final.AddRange(lowPath);
		final = AIGraph.SmoothCurve(final, 1);

		if (showGizmos) {
			if (showSmoothed) {
				AIGraph.PaintPath(final, Color.black);
			} else {
				AIGraph.PaintPath(lowPath, Color.blue);
				AIGraph.PaintPath(mediumPath, Color.yellow);
				AIGraph.PaintPath(highPath, Color.red);
			}
		}
	}

	void LateUpdate() {
		FindPath(from.position, to.position);
	}
}
