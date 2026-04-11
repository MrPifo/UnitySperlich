using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Sperlich.PrefabManager {
	public class Prefab {

		public readonly string name;
		public readonly Prefabs type;
		public readonly GameObject prefab;
		public readonly bool isPoolPrefab;

		public Prefab(string name, GameObject gameobject) {
			this.name = PrefabManager.ToCleanString(name);
			this.type = PrefabManager.GetPrefabTypeFromName(this.name);
			this.prefab = gameobject;
			this.isPoolPrefab = this is PoolPrefab;
		}
	}
	public class PoolPrefab : Prefab {

		public int preloadAmount;

		public PoolPrefab(string name, GameObject gameObject, int preloadAmount = 0) : base(name, gameObject) {
			this.preloadAmount = preloadAmount;
		}
	}
}