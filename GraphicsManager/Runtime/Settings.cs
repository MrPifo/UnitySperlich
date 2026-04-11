using System;
using System.Collections.Generic;
using System.Globalization;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;
using UnityEngine;

namespace Sperlich.GameSettings {
	[TomlDoNotInlineObject]
	public class Settings {

		public enum Type {
			String,
			Integer,
			Float,
			Boolean,
			Vector2Int
		}
		public enum Categories {
			Graphics,
			Audio,
			Controls,
			WindowSettings
		}
		public enum GameSetting {
			AmbientOcclusion,
			Antialiasing,
			ControllerScheme,
			ControllerSensitivity,
			EffectVolume,
			MainVolume,
			MusicVolume,
			PerformanceMode,
			ResolutionX,
			ShadowQuality,
			ShowTracks,
			TextureQuality,
			Vsync,
			WindowMode,
			ResolutionY,
		}

		public Dictionary<string, Dictionary<string, object>> Values { get; private set; }

		public Settings() {
			Values = new();

			foreach(Categories cat in Enum.GetValues(typeof(Categories))) {
				Values.Add(cat.ToString(), new Dictionary<string, object>());
			}
		}

		public bool ParseToml(string src) {
			try {
				Values = new();
				TomlParser parser = new TomlParser();
				TomlDocument doc = parser.Parse(src);

				foreach (var cat in doc.Entries) {
					TomlTable catVal = doc.GetSubTable(cat.Key);

					foreach (var key in catVal.Keys) {
						if (Enum.TryParse(cat.Key, true, out Categories category) && Enum.TryParse(key, true, out GameSetting setting)) {
							object value;

							if (GameSettings.DefaultConfig.TryGetEntry(setting, out ConfigEntry entry)) {
								// If the Default-Config exists, try parsing with the preset type instead
								value = ParseAndSerialize(entry.Type, catVal.GetValue(key).SerializedValue);
							} else {
								value = ParseAndSerialize(catVal.GetValue(key).SerializedValue);
							}

							if (value is float floatValue) {
								SetValue(category, setting, floatValue);
							} else {
								SetValue(category, setting, value);
							}
						}
					}
				}

				return true;
			} catch {
				Debug.LogError("An unexpected error occurred while parsing the GameSettings.ini file.");
			}

			return false;
		}
		public TomlDocument BuildTomlet() {
			var doc = TomlDocument.CreateEmpty();

			foreach ((string category, Dictionary<string, object> values) in Values) {
				var table = new TomlTable();
				table.ForceNoInline = true;

				foreach ((string fieldName, object fieldValue) in values) {
					TomlValue value;

					switch (fieldValue) {
						case bool boolVal:
							value = boolVal ? TomlBoolean.True : TomlBoolean.False;
							break;
						case double doubleVal:
							value = new TomlDouble(Mathf.Round((float)doubleVal * 100f) / 100f);
							break;
						case long longVal:
							value = new TomlLong(longVal);
							break;
						case int intVal:
							value = new TomlLong(intVal);
							break;
						case float floatVal:
							value = new TomlDouble((float)Mathf.Round((float)floatVal * 100f) / 100f);
							break;
						case string stringVal:
							value = new TomlString(stringVal);
							break;
						case Enum en:
							value = new TomlString(en.ToString());
							break;
						default:
							throw new NotSupportedException($"Value type of {fieldValue.GetType().Name} is not supported!");
					}

					table.PutValue(fieldName, value);
				}

				doc.Put(category.ToString(), table);
			}

			return doc;
		}
		object ParseAndSerialize(Type type, string input) {
			input = input.Replace("'", "").Replace("\"", "");

			switch (type) {
				default:
				case Type.String:
					return input;
				case Type.Integer:
					return int.Parse(input, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
				case Type.Float:
					float.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out float floatValue);
					return Mathf.Round(floatValue * 100f) / 100f;
				case Type.Boolean:
					return bool.Parse(input);
			}
		}
		object ParseAndSerialize(string input) {
			input = input.Replace("'", "").Replace("\"", "");

			if ((input.Contains('.') || input.Contains(',')) && float.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out float doubleValue)) {
				return Mathf.Round(doubleValue * 100f) / 100f;
			} else if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int floatValue)) {
				return floatValue;
			} else if (bool.TryParse(input, out bool boolValue)) {
				return boolValue;
			} else {
				return input;
			}
		}

		public void SetValue(GameSetting setting, object value) {
			if(TryGetCategoryBySetting(setting, out Categories category)) {
				//Debug.Log($"Set {setting} to {value}");
				SetValue(category, setting, value);
			}
		}
		public void SetValue(Categories category, GameSetting setting, object value) => SetValue(category.ToString(), setting.ToString(), value);
		public void SetValue(Categories category, string setting, object value) => SetValue(category.ToString(), setting, value);
		public void SetValue(string category, string fieldName, object value) {
			if(Values.ContainsKey(category) == false) {
				Values.Add(category, new Dictionary<string, object>());
			}
			if (Values[category].ContainsKey(fieldName) == false) {
				Values[category].Add(fieldName, null);
			}

			Values[category][fieldName] = value;
		}
		public T GetValue<T>(Categories category, GameSetting setting) => GetValue<T>(category.ToString(), setting.ToString());
		public T GetValue<T>(Categories category, string fieldName) => GetValue<T>(category.ToString(), fieldName);
		public T GetValue<T>(string category, string fieldName) {
			if (Values[category].ContainsKey(fieldName)) {
				try {
					return (T)Values[category][fieldName];
				} catch {
					Debug.Log($"Invalid cast from {fieldName} to {typeof(T).Name}");
				}

				return default;
			} else {
				throw new KeyNotFoundException($"Field {fieldName} in {category} not found!");
			}
		}
		public bool TryGetCategoryBySetting(GameSetting setting, out Categories category) {
			foreach(var catPair in Values) {
				foreach(var settingPair in catPair.Value) {
					if(settingPair.Key == setting.ToString()) {
						category = (Categories)Enum.Parse(typeof(Categories), catPair.Key, true);
						return true;
					}
				}
			}

			category = default;
			return false;
		}
		public bool HasValue(Categories category, GameSetting setting) {
			if(Values.ContainsKey(category.ToString())) {
				if (Values[category.ToString()].ContainsKey(setting.ToString())) {
					return true;
				}
			}

			return false;
		}
	}
}