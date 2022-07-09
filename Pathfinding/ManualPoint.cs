using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Sperlich.Debug.Draw.Draw;

public class ManualPoint : MonoBehaviour {
	
	public List<int> connections = new List<int>();
	private Vector3 _pos;
	private CustomAIGraph _assignedGraph;
	public CustomAIGraph AssignedGraph {
		get {
			if(_assignedGraph == null) {
				_assignedGraph = transform.GetComponentInParent<CustomAIGraph>();
			}
			return _assignedGraph;
		}
	}
	public Vector3 Pos => _pos;
	public int index => transform.GetSiblingIndex();

	public ManualPoint GetNeighbour(int index) => transform.parent.GetChild(index).GetComponent<ManualPoint>();

	private void OnDrawGizmos() {
		if (connections.Contains(index)) {
			Debug.LogError("Warning: Point cannot connect to itself!");
		}
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit)) {
			_pos = hit.point + Vector3.up * 0.25f;
		} else {
			_pos = transform.position;
		}
#if UNITY_EDITOR
		if (AssignedGraph != null && AssignedGraph.showGizmos) {
			Sphere(Pos, 2f, Color.blue, false);
			Ray(Pos, Vector3.up, 15, Color.blue);
		}
		if (AssignedGraph != null && AssignedGraph.showNumbers) {
			Handles.Label(Pos, transform.GetSiblingIndex() + "", new GUIStyle() { fontSize = 30 });
		}
#endif
	}

	private void OnDrawGizmosSelected() {
		if (AssignedGraph != null && AssignedGraph.showGizmos) {

			foreach (int c in connections) {
				var neigh = GetNeighbour(c);
				Line(Pos, neigh.Pos, 4, Color.black, false);
			}
		}
	}
}
