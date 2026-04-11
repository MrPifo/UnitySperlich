using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Globalization;
using System.Collections;

namespace Sperlich.Monitoring {
	public class MonitorSystem : MonoBehaviour {

		private GUISkin skin;
		private NumberFormatInfo numberFormat;
		public static bool Initialized = false;
		public static bool EnableMonitoring = false;
		public static Dictionary<MonoBehaviour, (List<(string, Func<MonoBehaviour, object>)> Attributes, bool enableMonitoring)> Monitorings { get; } = new();
		private static MonitorSystem _instance;
		public static MonitorSystem Instance {
			get {
				if (_instance == null) {
					_instance = new GameObject("MonitorSystem").AddComponent<MonitorSystem>();
				}
				return _instance;
			}
		}

		[RuntimeInitializeOnLoadMethod]
		public static void Initialize() {
			DontDestroyOnLoad(Instance.gameObject);
			Instance.skin = Resources.Load<GUISkin>("Skin");

			Instance.numberFormat = new NumberFormatInfo();
			Instance.numberFormat.NumberDecimalSeparator = ".";
			Initialized = true;
		}

		void OnGUI() {
			if (EnableMonitoring == false) return;
			var horizRatio = 1f;
			var vertRatio = 1f;

			GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(horizRatio, vertRatio, 1));
			GUI.skin = skin;
			HashSet<MonoBehaviour> removeNulls = new HashSet<MonoBehaviour>();

			foreach (var monitor in Monitorings) {
				if (monitor.Key == null) {
					removeNulls.Add(monitor.Key);
					continue;
				}
				if (monitor.Value.enableMonitoring == false) {
					continue;
				}

				Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, monitor.Key.transform.position);
				Vector2 size = new Vector2(1000, Screen.height);
				pos.y = (Screen.height) - pos.y;
				pos.y -= 25;
				Rect boxRect = new Rect(pos, size);

				GUI.skin.box.fontSize = 26;
				GUI.skin.box.fontStyle = FontStyle.Bold;
				GUI.skin.box.normal.textColor = Color.yellow;
				GUILayout.BeginArea(boxRect);

				GUILayout.Box(monitor.Key.name);

				GUI.skin.box.fontSize = 20;
				GUI.skin.box.normal.textColor = Color.white;
				GUI.skin.box.alignment = TextAnchor.MiddleLeft;
				GUI.skin.box.fontStyle = FontStyle.Normal;

				foreach ((string name, Func<MonoBehaviour, object> getter) attr in monitor.Value.Attributes) {
					string content = ConvertString(attr.getter(monitor.Key));
					if (content != string.Empty) {
						GUILayout.BeginHorizontal();
						string name = attr.name;

						GUILayout.Box(attr.name + ": ");
						GUILayout.Box(content);
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.EndArea();
			}
			foreach (var monitor in removeNulls) {
				Monitorings.Remove(monitor);
			}
		}

		public string ConvertString(object obj) {
			string result = string.Empty;

			if (obj is null) {
				return string.Empty;
			}
			if (obj is bool) {
				if (obj is true) {
					result = "<color=lime>TRUE</color>";
				} else {
					result = "<color=red>FALSE</color>";
				}
			} else if (obj is Vector3) {
				result += "[";
				Vector3 vec = (Vector3)obj;
				float x = Mathf.Round(vec.x * 100f) / 100f;
				float y = Mathf.Round(vec.y * 100f) / 100f;
				float z = Mathf.Round(vec.z * 100f) / 100f;
				if (x == 0) {
					result += ((int)x).ToString(numberFormat);
				} else {
					result += x.ToString(numberFormat);
				}
				result += ", ";
				if (y == 0) {
					result += ((int)y).ToString(numberFormat);
				} else {
					result += y.ToString(numberFormat);
				}
				result += ", ";
				if (z == 0) {
					result += ((int)z).ToString(numberFormat);
				} else {
					result += z.ToString(numberFormat);
				}
				result += "]";
			} else if (obj is Vector2) {
				result += "[";
				Vector2 vec = (Vector2)obj;
				float x = Mathf.Round(vec.x * 100f) / 100f;
				float y = Mathf.Round(vec.y * 100f) / 100f;
				if (x == 0) {
					result += ((int)x).ToString(numberFormat);
				} else {
					result += x.ToString(numberFormat);
				}
				result += ", ";
				if (y == 0) {
					result += ((int)y).ToString(numberFormat);
				} else {
					result += y.ToString(numberFormat);
				}
				result += "]";
			} else if (obj is float) {
				result = (Mathf.Round(((float)obj) * 100f) / 100f).ToString(numberFormat);
			} else if (obj is IList) {
				foreach (var e in obj as IList) {
					result += $"{e.ToString()} \n";
				}
			} else if (obj is IDictionary) {
				IDictionary dic = obj as IDictionary;

				foreach (DictionaryEntry pair in dic) {
					result += $"{pair.Key}: {pair.Value} \n";
				}
			} else if (obj is Dictionary<Enum, float> dic) {
				foreach (KeyValuePair<Enum, float> pair in dic) {
					result += $"{pair.Key}: {pair.Value} \n";
				}
			} else {
				result = obj.ToString();
			}
			return result;
		}
		public static void Monitor(MonoBehaviour monoBehaviour, bool pauseImmediately) {
			if (Initialized && Monitorings.ContainsKey(monoBehaviour) == false) {
				List<(string, Func<MonoBehaviour, object>)> attributes = new List<(string, Func<MonoBehaviour, object>)>();

				Type monoType = monoBehaviour.GetType();
				IEnumerable<(PropertyInfo, string)> props = monoType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).
					Where(p => p.GetCustomAttribute<MonitorAttribute>(true) is not null).
					Select(p => (p, p.GetCustomAttribute<MonitorAttribute>(true).CustomFieldName));

				IEnumerable<(FieldInfo, string)> fields = monoType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).
					Where(f => f.GetCustomAttribute<MonitorAttribute>(true) is not null).
					Select(f => (f, f.GetCustomAttribute<MonitorAttribute>(true).CustomFieldName));

				ParameterExpression param = Expression.Parameter(typeof(MonoBehaviour));
				foreach ((PropertyInfo property, string customPropName) p in props) {
					MemberExpression memb = Expression.Property(Expression.Convert(param, monoType), p.property);
					attributes.Add((p.customPropName ?? p.property.Name, Expression.Lambda<Func<MonoBehaviour, object>>(Expression.Convert(memb, typeof(object)), param).Compile()));
				}
				foreach ((FieldInfo field, string customFieldName) f in fields) {
					MemberExpression memb = Expression.Field(Expression.Convert(param, monoType), f.field);
					attributes.Add((f.customFieldName ?? f.field.Name, Expression.Lambda<Func<MonoBehaviour, object>>(Expression.Convert(memb, typeof(object)), param).Compile()));
				}

				Monitorings.Add(monoBehaviour, (attributes, true));

				if (pauseImmediately) {
					monoBehaviour.PauseMonitoring();
				}
			}
		}
		public static void NoMonitor(MonoBehaviour monoBehaviour) {
			if (Monitorings.ContainsKey(monoBehaviour)) {
				Monitorings.Remove(monoBehaviour);
			}
		}
		public static bool IsMonitored(MonoBehaviour monoBehaviour) {
			return Monitorings.ContainsKey(monoBehaviour);
		}
	}
	public static class MonitorSystemExt {
		public static void NoMonitor(this MonoBehaviour b) => MonitorSystem.NoMonitor(b);
		public static void Monitor(this MonoBehaviour b) => MonitorSystem.Monitor(b, false);
		public static void Monitor(this MonoBehaviour b, bool pauseImmediately) => MonitorSystem.Monitor(b, pauseImmediately);
		public static void PauseMonitoring(this MonoBehaviour b) {
			if (MonitorSystem.Monitorings.ContainsKey(b)) {
				MonitorSystem.Monitorings[b] = (MonitorSystem.Monitorings[b].Attributes, false);
			}
		}
		public static void ResumeMonitoring(this MonoBehaviour b) {
			if (MonitorSystem.Monitorings.ContainsKey(b)) {
				MonitorSystem.Monitorings[b] = (MonitorSystem.Monitorings[b].Attributes, true);
			}
		}
		public static void IsMonitored(this MonoBehaviour b) => MonitorSystem.IsMonitored(b);
	}
}