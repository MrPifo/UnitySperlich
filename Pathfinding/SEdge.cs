using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Pathfinding {
	[System.Serializable]
	public struct SEdge {

		public SPoint from;
		public SPoint to;
		public Vector3 MidPoint => (from.position + to.position) / 2;

		public SEdge(SPoint from, SPoint to) {
			this.from = from;
			this.to = to;
		}

		public static bool operator ==(SEdge a, SEdge b) {
			return a.Equals(b);
		}
		public static bool operator !=(SEdge a, SEdge b) {
			return !a.Equals(b);
		}
		public override bool Equals(object obj) => obj is SEdge edge && this.Equals(edge);
		public bool Equals(SEdge other) => this.from.position == other.from.position && this.to.position == other.to.position;
		public override int GetHashCode() => -1009761766 + this.from.GetHashCode() + this.to.GetHashCode();
	}
}
