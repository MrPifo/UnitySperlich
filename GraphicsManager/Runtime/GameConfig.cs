using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Sperlich.GameSettings.Settings;

namespace Sperlich.GameSettings {
	[CreateAssetMenu(fileName = "Asset", menuName = "GameSettings/Config", order = 1)]
	public class GameConfig : ScriptableObject {

		public List<ConfigEntry> settings = new();

		public ConfigEntry GetEntry(GameSetting setting) {
			return settings.Where(s => s.Setting.ToString().ToLower() == setting.ToString().ToLower()).First();
		}
		public bool TryGetEntry(GameSetting setting, out ConfigEntry entry) => TryGetEntry(setting.ToString(), out entry);
		public bool TryGetEntry(string setting, out ConfigEntry entry) {
			entry = settings.Where(s => s.Setting.ToString().ToLower() == setting.ToLower()).FirstOrDefault();

			if (entry != null) {
				return true;
			} else {
				return false;
			}
		}
		public bool TryGetValue(GameSetting setting, out object value) {
			var entry = settings.Where(s => s.Setting == setting).FirstOrDefault();

			if (entry != null) {
				value = entry.Value;
				return true;
			} else {
				value = null;
				return false;
			}
		}
		public object GetValue(GameSetting setting) {
			return settings.Where(s => s.Setting == setting).First().Value;
		}
		public string[] GetSelectValues(GameSetting setting) {
			if(TryGetEntry(setting, out ConfigEntry entry)) {
				if(entry.Type == Type.String || entry.Type == Type.Integer) {
					return entry.StringList.ToArray();
				}

				throw new KeyNotFoundException($"GameSetting {setting} must be of type String.");
			}

			throw new System.NullReferenceException($"Entry for GameSetting {setting} not found.");
		}
	}
}