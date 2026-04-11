using Sperlich.GameSettings;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Sperlich.GameSettings.Settings;

[RequireComponent(typeof(Camera))]
public class CameraSettingsReceiver : MonoBehaviour, IGraphicsReceiver {

	private Camera cam;
	private UniversalAdditionalCameraData camData;
	public object AppliedValue { get; set; }

	void Awake() {
		cam = GetComponent<Camera>();
		camData = GetComponent<UniversalAdditionalCameraData>();

		GameSettings.AddGraphicsListener(this);
		GameSettings.TriggerRefresh(GameSetting.Antialiasing);
	}

	void OnDestroy() {
		GameSettings.RemoveGraphicsListener(this);
	}

	public void OnGameSettingsApplied(GameSetting setting, object value) {
		if (setting == GameSetting.Antialiasing) {
			bool state = (bool)value;

			if(state) {
				camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
			} else {
				camData.antialiasing = AntialiasingMode.None;
			}
		}
	}

}
