using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Extensions.MatPropertyBlock {
	/// <summary>
	/// Use this class for fast Material manipulation, through the automatic caching of the Shader properties, they will be directly accessed via their ID.
	/// This class mostly does the job automaticially and avoids clutter in MonoBehaviours.
	/// </summary>
	[System.Serializable]
	public class ShaderAccessor {

		[SerializeField]
		private Material _material;
		public Shader Shader => _material.shader;
		public Material Material => _material;

		private Dictionary<string, int> _propIDs = new();

		public ShaderAccessor(Material _material) {
			this._material = _material;
		}

		public void SetFloat(string propName, float value) {
			if(_propIDs.ContainsKey(propName) == false) {
				Cache(propName);
			}

			_material.SetFloat(_propIDs[propName], value);
		}
		public float GetFloat(string propName) {
			if (_propIDs.ContainsKey(propName) == false) {
				Cache(propName);
			}

			return _material.GetFloat(_propIDs[propName]);
		}

		private void Cache(string propName) {
			if(_propIDs.ContainsKey(propName) == false) {
				_propIDs.Add(propName, Shader.PropertyToID(propName));
			}
		}
	}
}