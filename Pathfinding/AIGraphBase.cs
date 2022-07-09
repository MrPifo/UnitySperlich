using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Paths;
using Sperlich.Pathfinding;
using UnityEngine;
using static Sperlich.Debug.Draw.Draw;

public class AIGraphBase : MonoBehaviour {

    public enum GenerationStep { Generate, Merge, Reduce, Connect, Finish }
    public enum MergeMethod { First, Last, Average }
    public enum MergeMoment { DontMerge, Before, After, Both }
    public enum PointsMethod { Grid, Triangle, RandomGrid, RandomTriangle }

    public bool showGizmos;
    public bool CancelGeneration { get; set; }
    public bool IsGenerating { get; set; }
    public SGraph Graph { get; set; }
    public GenerationStep CurrentStep { get; set; }

	public List<Vector3> FindPath(Vector3 from, Vector3 to, float minDist = Mathf.Infinity, float maxDist = Mathf.Infinity) {
		if (Graph == null || Graph.nodes.Count <= 1) return new List<Vector3>();
		var path = new PathFinder().FindPath(Graph.GetNearest(from), Graph.GetNearest(to), new Roy_T.AStar.Primitives.Velocity(100));
		List<Vector3> points = new List<Vector3>();
		float totalDistance = 0;

		foreach (var e in path.Edges) {
			float dist = Vector3.Distance(from, e.Start.Position.Vector3);
			totalDistance += dist;
			if (totalDistance > minDist && totalDistance < maxDist) {
				points.Add(e.Start.Position.Vector3);
				points.Add(e.End.Position.Vector3);
			}
		}
		return points;
	}
	public List<Vector3> SmoothPath(List<Vector3> points, int smoothing) {
		if (points != null && points.Count > 1) {
			return SmoothCurve(points, smoothing).ToList();
		}
		return points;
	}
	public static void PaintPath(IReadOnlyList<IEdge> edges) {
		if (edges != null && edges.Count > 1) {
			foreach (var edge in edges) {
				Line(edge.Start.Position.Vector3 + Vector3.up, edge.End.Position.Vector3 + Vector3.up, 4, Color.cyan, false);
			}
		}
	}
	public static void PaintPath(List<Vector3> points, Color color) {
		if (points != null && points.Count > 1) {
			Vector3 lastPos = points[0];
			for (int i = 1; i < points.Count; i++) {
				Line(lastPos, points[i], 4, color, false);
				Sphere(lastPos, 0.3f, Color.blue, false);
				lastPos = points[i];
			}
		}
	}
	public static List<(Vector3, float)> GetSimiliarVectors(Vector3 origin, Vector3 normal, List<Vector3> dirs, float treshold) {
		List<(Vector3, float)> list = new List<(Vector3, float)>();
		for (int i = 0; i < dirs.Count; i++) {
			Vector3 dir = ((origin + dirs[i]).normalized - origin).normalized;
			float dot = 180 - Mathf.Acos(Vector3.Dot(dir, normal)) * Mathf.Rad2Deg;
			if (dot <= treshold) {
				list.Add((dirs[i], dot));
			}
		}
		return list;
	}
	public static List<(Vector2, float)> GetSimiliarVectors(Vector2 origin, Vector2 normal, List<Vector2> dirs, float treshold) {
		List<(Vector2, float)> list = new List<(Vector2, float)>();
		for (int i = 0; i < dirs.Count; i++) {
			Vector2 dir = ((origin + dirs[i]).normalized - origin).normalized;
			float dot = Mathf.Acos(Vector2.Dot(dir, normal)) * Mathf.Rad2Deg;
			if (dot <= treshold) {
				list.Add((dirs[i], dot));
			}
		}
		return list;
	}
	public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints) {
		//To avoid floating point precision issues we can add a small value
		float epsilon = 0.00001f;

		bool isIntersecting = false;

		float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

		//Make sure the denominator is > 0, if not the lines are parallel
		if (denominator != 0f) {
			float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
			float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

			//Are the line segments intersecting if the end points are the same
			if (shouldIncludeEndPoints) {
				//Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
				if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon) {
					isIntersecting = true;
				}
			} else {
				//Is intersecting if u_a and u_b are between 0 and 1
				if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon) {
					isIntersecting = true;
				}
			}
		}

		return isIntersecting;
	}
	public static List<Vector3> SmoothCurve(List<Vector3> pathToCurve, int interpolations) {
		if (pathToCurve.Count <= 1) return pathToCurve;
		List<Vector3> tempPoints;
		List<Vector3> curvedPoints;
		int pointsLength;
		int curvedLength;

		if (interpolations < 1)
			interpolations = 1;

		pointsLength = pathToCurve.Count;
		curvedLength = (pointsLength * Mathf.RoundToInt(interpolations)) - 1;
		curvedPoints = new List<Vector3>(curvedLength);

		float t = 0.0f;
		for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++) {
			t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);
			tempPoints = new List<Vector3>(pathToCurve);
			for (int j = pointsLength - 1; j > 0; j--) {
				for (int i = 0; i < j; i++) {
					tempPoints[i] = (1 - t) * tempPoints[i] + t * tempPoints[i + 1];
				}
			}
			curvedPoints.Add(tempPoints[0]);
		}

		return curvedPoints;
	}
	public static List<Vector3> GetPointsWithinBounds(List<Vector3> points, Vector2 origin, Vector2 bounds) {
		origin += bounds / 2f;
		List<Vector3> list = new List<Vector3>();
		foreach (var p in points) {
			if (p.x <= origin.x + bounds.x / 2f && p.x >= origin.x - bounds.x / 2f) {
				if (p.z <= origin.y + bounds.y / 2f && p.z >= origin.y - bounds.y / 2f) {
					list.Add(p);
				}
			}
		}
		return list;
	}
	public static List<SPoint> GetPointsWithinBounds(List<SPoint> points, Vector2 origin, Vector2 bounds) {
		origin += bounds / 2f;
		List<SPoint> list = new List<SPoint>();
		foreach (var p in points) {
			if (p.x <= origin.x + bounds.x / 2f && p.x >= origin.x - bounds.x / 2f) {
				if (p.z <= origin.y + bounds.y / 2f && p.z >= origin.y - bounds.y / 2f) {
					list.Add(p);
				}
			}
		}
		return list;
	}

}
