using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Pathfinding {
	[System.Serializable]
	public struct SPoint {

		public Vector3 position;
		public Vector3 normal;
		public string tag;
		public float x => position.x;
		public float y => position.y;
		public float z => position.z;
		public bool isValid;
		public float isoValue;
		public Vector2 Vector2 => new Vector2(x, z);

		public SPoint(Vector3 position, string tag) {
			this.position = position;
			this.tag = tag;
			this.normal = Vector3.zero;
			isValid = true;
			isoValue = 0;
		}
		public SPoint(Vector3 position, float isoValue, bool isValid) {
			this.position = position;
			this.tag = "";
			this.isoValue = isoValue;
			this.isValid = isValid;
			this.normal = Vector3.zero;
		}
		public SPoint(Vector3 position, Vector3 normal, float isoValue, bool isValid) {
			this.position = position;
			this.tag = "";
			this.isoValue = isoValue;
			this.isValid = isValid;
			this.normal = normal;
		}

		public static bool operator ==(SPoint a, SPoint b) {
			return a.Equals(b);
		}
		public static bool operator !=(SPoint a, SPoint b) {
			return !a.Equals(b);
		}
		public static Vector3 operator -(SPoint a, SPoint b) => a.position - b.position;
		public static Vector3 operator +(SPoint a, SPoint b) => a.position + b.position;
		public override bool Equals(object obj) => obj is SPoint point && this.Equals(point);
		public bool Equals(SPoint other) => this.position.x == other.position.x && this.position.y == other.position.y && this.position.z == other.position.z;
		public override int GetHashCode() => -1209761766 + this.position.x.GetHashCode() + this.position.y.GetHashCode() + this.position.z.GetHashCode();
	}
}