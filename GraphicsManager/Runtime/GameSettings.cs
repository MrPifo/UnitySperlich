using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Tomlet;
using Tomlet.Models;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Sperlich.GameSettings.Settings;

namespace Sperlich.GameSettings {
	public static class GameSettings {

		private static HashSet<IGraphicsReceiver> GraphicsListeners { get; set; } = new();

		public static bool IsInitialized { get; set; }
		public static string FilePath => Path.Combine(Application.persistentDataPath, "graphics.ini");
		public static Settings Settings { get; set; } = new();
		public static GameConfig DefaultConfig { get; set; }
		public static UniversalRenderPipelineAsset URPAsset { get; set; }
		public static List<ScriptableRendererFeature> RenderFeatures { get; private set; }
		public static UnityEvent<GameSetting, object> OnGameSettingChanged { get; set; }
		public static UnityEvent<List<GameSetting>> OnMissingSettings { get; set; }
		public readonly static IReadOnlyList<Vector2Int> AllowedResolutions = new List<Vector2Int>() {
			new Vector2Int(640, 360),
			new Vector2Int(896, 504),
			new Vector2Int(960, 540),
			new Vector2Int(1024, 576),
			new Vector2Int(1360, 765),
			new Vector2Int(1600, 900),
			new Vector2Int(1920, 1080),
			new Vector2Int(2048, 1152),
			new Vector2Int(2560, 1440),
			new Vector2Int(3072, 1728),
			new Vector2Int(3200, 1800),
			new Vector2Int(3840, 2160),
			new Vector2Int(4096, 2304),
			new Vector2Int(5120, 2880),
			new Vector2Int(7680, 4320),
			new Vector2Int(8192, 4608),
			new Vector2Int(15360, 8640)
		};
		public static IReadOnlyList<Vector2Int> AvailableUserResolutions = new List<Vector2Int>();
		public static Dictionary<GameSetting, (object oldValue, object newValue)> AppliedChanges { get; set; } = new();
		public static bool IsDirty => AppliedChanges.Count > 0;

		public static void Initialize(UniversalRenderPipelineAsset urpAsset) {
			if (IsInitialized == false) {
				IsInitialized = true;
				OnMissingSettings = new();
				OnGameSettingChanged = new();
				URPAsset = urpAsset;
				DefaultConfig = Resources.Load<GameConfig>("DefaultGameSettings");
				RenderFeatures = typeof(ScriptableRenderer).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(URPAsset.scriptableRenderer) as List<ScriptableRendererFeature>;

				ComputeAvailableSettings();
				LoadSettings();
				ApplySettings();
			}
		}
		public static void ComputeAvailableSettings() {
			List<Vector2Int> userResolutions = new();

			if (DefaultConfig.TryGetEntry("ResolutionX", out ConfigEntry resX) && DefaultConfig.TryGetEntry("ResolutionY", out ConfigEntry resY)) {
				Vector2Int monitorRes = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
				resX.stringList.Clear();
				resY.stringList.Clear();

				for (int i = 0; i < AllowedResolutions.Count; i++) {
					Vector2Int res = AllowedResolutions[i];

					if (res.x <= monitorRes.x && res.y <= monitorRes.y) {
						userResolutions.Add(res);

						resX.stringList.Add(res.x.ToString());
						resY.stringList.Add(res.y.ToString());
					} else {
						if(userResolutions.Contains(monitorRes) == false) {
							userResolutions.Add(monitorRes);
						}
						break;
					}
				}

				AvailableUserResolutions = userResolutions;
			}
		}

		#region Functional
		public static void AddGraphicsListener(IGraphicsReceiver receiver) {
			if(GraphicsListeners.Contains(receiver) == false) {
				GraphicsListeners.Add(receiver);
			}
		}
		public static void RemoveGraphicsListener(IGraphicsReceiver receiver) {
			if(GraphicsListeners.Contains(receiver)) {
				GraphicsListeners.Remove(receiver);
			}
		}
		public static void SaveSettings() {
			try {
				CheckMissingSettings();
				string content = Settings.BuildTomlet().SerializedValue;

				File.WriteAllText(FilePath, content);
			} catch (System.Exception e) {
				Debug.LogError("An error occured while trying to save Graphic settings. \n" + e.Message + " \n " + e.StackTrace);
			}
		}
		public static void LoadSettings() {
			Debug.Log("Loading Settings.");

			try {
				if (File.Exists(FilePath)) {
					if(Settings.ParseToml(File.ReadAllText(FilePath))) {
						SaveSettings();
					} else {
						ResetSettings();
					}
				} else {
					var stream = File.Create(FilePath);
					stream.Close();

					ResetSettings();
				}

				ComputeAvailableSettings();
			} catch (System.Exception e) {
				ResetSettings();
				Debug.LogError("An error occured while trying to read the Graphics settings file. \n " + e.Message + " \n " + e.StackTrace);
			}
		}
		public static void ResetSettings() {
			Debug.Log("Resettings Graphic-Settings.");
			Settings = new Settings();
			SetValue(Categories.WindowSettings, "WindowSizeX", Screen.width);
			SetValue(Categories.WindowSettings, "WindowSizeY", Screen.height);
			SetValue(Categories.WindowSettings, "FullscreenMode", 1);
			CheckMissingSettings();

			SaveSettings();
		}
		public static void ApplySettings() {
			var specialCases = new List<GameSetting>() {
				GameSetting.ResolutionX,
				GameSetting.ResolutionY
			};
			var entries = System.Enum.GetValues(typeof(GameSetting)).OfType<GameSetting>().Except(specialCases);

			foreach (GameSetting setting in entries) {
				ApplySetting(setting, GetValue<object>(setting));
			}

			// Resolution
			ApplySetting(GameSetting.ResolutionX, GetCurrentSavedResolution());
			
			Debug.Log("Graphics applied for Desktop");
		}
		public static void CheckMissingSettings() {
			List<GameSetting> missing = new();

			foreach (GameSetting setting in System.Enum.GetValues(typeof(GameSetting))) {
				bool isMissing = false;
				if (Settings.TryGetCategoryBySetting(setting, out Categories category)) {
					if (Settings.HasValue(category, setting) == false) {
						isMissing = true;
					}
				} else {
					isMissing = true;
				}

				if (isMissing) {
					if (DefaultConfig.TryGetEntry(setting, out ConfigEntry value)) {
						Settings.SetValue(value.Category, value.Setting, value.Value);
					}

					missing.Add(setting);
				}
			}

			OnMissingSettings.Invoke(missing);
		}
		public static void ApplyUserChanges() {
			foreach(var pair in AppliedChanges) {
				try {
					if(pair.Key == GameSetting.ResolutionX || pair.Key == GameSetting.ResolutionY) {
						Vector2Int resolution = (Vector2Int)pair.Value.newValue;

						Settings.SetValue(GameSetting.ResolutionX, resolution.x);
						Settings.SetValue(GameSetting.ResolutionY, resolution.y);
						ApplySetting(GameSetting.ResolutionX, (Vector2Int)pair.Value.newValue);
					} else {
						Settings.SetValue(pair.Key, pair.Value.newValue);
						ApplySetting(pair.Key, pair.Value.newValue);
					}
				} catch(System.Exception e) {
					Debug.LogException(e);
				}
			}

			AppliedChanges = new();
		}
		public static void TriggerRefresh(GameSetting setting) {
			if (IsInitialized == false) return;

			foreach (var receiver in GraphicsListeners) {
				if (receiver != null) {

					receiver.OnGameSettingsApplied(setting, ConvertSettingToQualityLevel<object>(setting));
				}
			}
		}
		#endregion

		#region Generic Settings
		static void ApplySetting(GameSetting setting, object value) {
			//Debug.Log(setting + " = " + value.ToString() + " : " + value.GetType().Name);
			object convertedValue = ConvertSettingToQualityLevel<object>(setting, value.ToString());

			switch (setting) {
				case GameSetting.Antialiasing:
					break;
				case GameSetting.ControllerScheme:
					break;
				case GameSetting.ControllerSensitivity:
					break;
				case GameSetting.EffectVolume:
					break;
				case GameSetting.MainVolume:
					break;
				case GameSetting.MusicVolume:
					break;
				case GameSetting.PerformanceMode:
					ApplyPerformanceMode((bool)convertedValue);
					break;
				case GameSetting.ResolutionY:
				case GameSetting.ResolutionX:
					Vector2Int resolution = (Vector2Int)value;

					ApplyWindowResolution(resolution);
					break;
				case GameSetting.ShadowQuality:
					ApplyShadowResolution((int)convertedValue);
					break;
				case GameSetting.ShowTracks:
					break;
				case GameSetting.TextureQuality:
					ApplyTextureLevel((int)convertedValue);
					break;
				case GameSetting.Vsync:
					ApplyVsync((bool)convertedValue);
					break;
				case GameSetting.WindowMode:
					ApplyWindowMode((int)convertedValue);
					break;
				default:
					break;
			}

			if (value != null) {
				foreach (var receiver in GraphicsListeners) {
					if (receiver != null) {
						receiver.AppliedValue = convertedValue;
						receiver.OnGameSettingsApplied(setting, convertedValue);
					}
				}
			}
		}
		public static T ConvertSettingToQualityLevel<T>(GameSetting setting) {
			return ConvertSettingToQualityLevel<T>(setting, GetValue<T>(setting).ToString().ToLower());
		}
		public static T ConvertSettingToQualityLevel<T>(GameSetting setting, string stringValue) {
			stringValue = stringValue.ToLower();
			object value = default;
			DefaultConfig.TryGetEntry(setting, out ConfigEntry entry);

			if(entry.IsValueList) {
				if (entry.TryGetIndex(stringValue, out int index)) {
					value = index;
				} else if (TryParse(stringValue, out index)) {
					value = index;
				} else {
					value = 0;
				}
			} else {
				switch (entry.Type) {
					case Type.String:
						break;
					case Type.Integer:
						if (TryParse(stringValue, out int intValue) == false) {
							intValue = 0;
						} else {
							if(entry.TryGetIndex(stringValue, out intValue) == false) {
								intValue = 0;
							}
						}

						value = intValue;

						break;
					case Type.Float:
						if(TryParse(stringValue, out float floatValue) == false) {
							floatValue = 0.5f;
						}

						value = floatValue;
						break;
					case Type.Boolean:
						if(TryParse(stringValue, out bool boolValue) == false) {
							boolValue = false;
						}

						value = boolValue;
						break;
					default:
						break;
				}
				
			}

			return (T)value;
		}
		static void ApplyShadowResolution(int level) {
			switch (level) {
				case 3:
					PipelineExtensions.MainLightCastShadows = true;
					PipelineExtensions.AdditionalLightCastShadows = true;
					PipelineExtensions.SoftShadowsEnabled = true;
					PipelineExtensions.MainLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._4096;
					PipelineExtensions.AdditionalLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
					break;
				case 2:
					PipelineExtensions.MainLightCastShadows = true;
					PipelineExtensions.AdditionalLightCastShadows = true;
					PipelineExtensions.SoftShadowsEnabled = true;
					PipelineExtensions.MainLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
					PipelineExtensions.AdditionalLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
					break;
				case 1:
					PipelineExtensions.MainLightCastShadows = true;
					PipelineExtensions.AdditionalLightCastShadows = false;
					PipelineExtensions.SoftShadowsEnabled = true;
					PipelineExtensions.MainLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
					PipelineExtensions.AdditionalLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
					break;
				case 0:
					PipelineExtensions.MainLightCastShadows = false;
					PipelineExtensions.AdditionalLightCastShadows = false;
					PipelineExtensions.SoftShadowsEnabled = false;
					PipelineExtensions.MainLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._256;
					PipelineExtensions.AdditionalLightShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._256;
					break;
			}

			Debug.Log("<color=magenta>Applied Shadow-Quality level</color>: " + level);
		}
		static void ApplyTextureLevel(int level) {
			QualitySettings.globalTextureMipmapLimit = 3 - Mathf.Clamp(level, 0, 3);

			Debug.Log("<color=magenta>Applied texture level</color>: " + level);
		}
		static void ApplyWindowResolution(Vector2Int resolution) {
			Screen.SetResolution(resolution.x, resolution.y, GetCurrentFullscreenMode());

			Debug.Log("<color=magenta>Applied Resolution level</color>: " + resolution.x + "x" + resolution.y);
		}
		static void ApplyWindowMode(int mode) {
			Vector2Int currentRes = GetCurrentSavedResolution();

			switch (mode) {
				case 0:
					Screen.fullScreen = false;
					Screen.SetResolution(currentRes.x, currentRes.y, FullScreenMode.Windowed);
					break;
				case 1:
					Screen.fullScreen = true;
					Screen.SetResolution(currentRes.x, currentRes.y, FullScreenMode.FullScreenWindow);
					break;
			}

			Debug.Log("<color=magenta>Applied Window-Mode</color>: " + Screen.fullScreenMode);
		}
		static void ApplyPerformanceMode(bool state) {
			if(state) {
				URPAsset.renderScale = 0.6f;

				if(TryGetRenderFeature("DecalRendererFeature", out ScriptableRendererFeature decalRender)) {
					//decalRender.SetActive(true);
				}
			} else {
				URPAsset.renderScale = 1f;

				if (TryGetRenderFeature("DecalRendererFeature", out ScriptableRendererFeature decalRender)) {
					//decalRender.SetActive(false);
				}
			}
		}
		static void ApplyVsync(bool state) {
			if(state) {
				QualitySettings.vSyncCount = 1;
			} else {
				QualitySettings.vSyncCount = 0;
			}
		}
		#endregion

		#region API
		public static void SetUserValue(GameSetting setting, object value) {
			var currentValue = GetValue<object>(setting);

			if(currentValue is float currentFloatValue && value is float newFloatValue) {
				currentValue = Mathf.Round(currentFloatValue * 1000f) / 1000f;
				value = Mathf.Round(newFloatValue * 1000f) / 1000f;
			}

			if (value.ToString().ToLower() == currentValue.ToString().ToLower()) {
				if (AppliedChanges.ContainsKey(setting)) {
					AppliedChanges.Remove(setting);
				}

				OnGameSettingChanged.Invoke(setting, value);
				return;
			}

			if(AppliedChanges.ContainsKey(setting) == false) {
				AppliedChanges.Add(setting, (currentValue, value));
			} else {
				if (AppliedChanges[setting].newValue.ToString() == currentValue.ToString()) {
					AppliedChanges.Remove(setting);
					OnGameSettingChanged.Invoke(setting, value);
					return;
				} else {
					AppliedChanges[setting] = (AppliedChanges[setting].oldValue, value);
				}
			}

			OnGameSettingChanged.Invoke(setting, value);
		}
		public static void SetValue(GameSetting setting, object value) {
			Settings.SetValue(setting, value);

			OnGameSettingChanged.Invoke(setting, value);
		}
		public static void SetValue(Categories category, string field, object value) {
			Settings.SetValue(category, field, value);

			OnGameSettingChanged.Invoke((GameSetting)System.Enum.Parse(typeof(GameSetting), field), value);
		}
		public static T GetValue<T>(Categories category, string fieldName) {
			return Settings.GetValue<T>(category, fieldName);
		}
		public static T GetValue<T>(GameSetting setting) {
			if (Settings.TryGetCategoryBySetting(setting, out Categories category)) {
				return Settings.GetValue<T>(category, setting);
			}

			throw new KeyNotFoundException($"Couldnt find a Category for GameSetting {setting}.");
		}
		#endregion

		#region Helpers
		public static bool TryGetRenderFeature<T>(string name, out T feature) where T : ScriptableRendererFeature {
			feature = null;

			foreach(var f in RenderFeatures) {
				if(f.name.ToLower() == name.ToLower()) {
					feature = (T)f;
					return true;
				}
			}

			return false;
		}
		public static bool TryParse(string input, out bool boolValue) {
			input = input.ToString().ToLower();

			if (input == "true" || input == "1" || input == "yes" || input == "on") {
				boolValue = true;
				return true;
			}

			boolValue = false;
			return false;
		}
		public static bool TryParse(string input, out int intValue) {
			if(int.TryParse(input, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out intValue)) {
				return true;
			}

			return false;
		}
		public static bool TryParse(string input, out float floatValue) {
			if (float.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out floatValue)) {
				return true;
			}

			return false;
		}
		public static Vector2Int GetCurrentSavedResolution() {
			string currentResX = GetValue<int>(GameSetting.ResolutionX).ToString().ToLower();
			string currentResY = GetValue<int>(GameSetting.ResolutionY).ToString().ToLower();
			TryParse(currentResX, out int resX);
			TryParse(currentResY, out int resY);

			return new Vector2Int(resX, resY);
		}
		public static FullScreenMode GetCurrentFullscreenMode() {
			int mode = GetValue<int>(GameSetting.WindowMode);

			return mode == 0 ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
		}
		#endregion
	}
}