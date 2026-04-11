using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Sperlich.Extensions.MatPropertyBlock {
	public class PropBlock {
		private enum Mode { Renderer, UI }
		private readonly Mode _mode;
		private readonly Renderer _renderer;
		private readonly MaterialPropertyBlock _mpb;
		private readonly Graphic _graphic;
		private readonly Material _uiMaterial;

		private static readonly Dictionary<string, int> _idCache = new();

		public PropBlock(Object target) {
			if (target is Renderer rend) {
				_mode = Mode.Renderer;
				_renderer = rend;
				_mpb = new MaterialPropertyBlock();
				_renderer.GetPropertyBlock(_mpb);
			} else if (target is Graphic gfx) {
				_mode = Mode.UI;
				_graphic = gfx;
				_uiMaterial = Object.Instantiate(gfx.material);
				_graphic.material = _uiMaterial;
			} else {
				throw new System.ArgumentException("PropBlock only supports Renderer or UI Graphic targets.");
			}
		}

		private static int IdOf(string name) {
			if (!_idCache.TryGetValue(name, out int id)) {
				id = Shader.PropertyToID(name);
				_idCache[name] = id;
			}
			return id;
		}

		public PropBlock SetFloat(string name, float value) {
			int id = IdOf(name);
			if (_mode == Mode.Renderer) {
				_mpb.SetFloat(id, value);
			} else {
				_uiMaterial.SetFloat(id, value);
			}
			return this;
		}

		public PropBlock SetInt(string name, int value) {
			int id = IdOf(name);
			if (_mode == Mode.Renderer) {
				_mpb.SetInteger(id, value);
			} else {
				_uiMaterial.SetInteger(id, value);
			}
			return this;
		}

		public PropBlock SetColor(string name, Color value) {
			int id = IdOf(name);
			if (_mode == Mode.Renderer) {
				_mpb.SetColor(id, value);
			} else {
				_uiMaterial.SetColor(id, value);
			}
			return this;
		}

		public PropBlock SetVector(string name, Vector4 value) {
			int id = IdOf(name);
			if (_mode == Mode.Renderer) {
				_mpb.SetVector(id, value);
			} else {
				_uiMaterial.SetVector(id, value);
			}
			return this;
		}

		public PropBlock SetTexture(string name, Texture value) {
			int id = IdOf(name);
			if (_mode == Mode.Renderer) {
				_mpb.SetTexture(id, value);
			} else {
				_uiMaterial.SetTexture(id, value);
			}
			return this;
		}

		public PropBlock Clear() {
			if (_mode == Mode.Renderer) {
				_mpb.Clear();
			}
			return this;
		}

		public void Apply() {
			if (_mode == Mode.Renderer) {
				_renderer.SetPropertyBlock(_mpb);
			}
			// UI braucht kein Apply – Änderungen gehen direkt ins Material
		}
	}
}