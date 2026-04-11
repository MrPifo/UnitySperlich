//using HorizonBasedAmbientOcclusion.Universal;
using Sperlich.GameSettings;
using UnityEngine;
using UnityEngine.Rendering;
using static Sperlich.GameSettings.Settings;

[RequireComponent(typeof(Volume))]
public class VolumeSettingsReceiver : MonoBehaviour, IGraphicsReceiver {

	private Volume volume;
	private VolumeProfile profile;
	//private HBAO hbao;

	public object AppliedValue { get; set; }

	void Awake() {
		volume = GetComponent<Volume>();
		profile = volume.profile;
		//profile.TryGet(out hbao);

		GameSettings.AddGraphicsListener(this);
		GameSettings.TriggerRefresh(GameSetting.AmbientOcclusion);
	}

	void OnDestroy() {
		GameSettings.RemoveGraphicsListener(this);
	}

	public void OnGameSettingsApplied(GameSetting setting, object value) {
		if (setting == GameSetting.AmbientOcclusion) {
			int level = (int)value;
			AppliedValue = level;

			/*if (hbao != null) {
				switch (level) {
					case 0:
					default:
						hbao.active = false;
						break;
					case 1:
						hbao.active = true;
						hbao.SetQuality(HBAO.Quality.Low);
						hbao.resolution = new HBAO.ResolutionParameter(HBAO.Resolution.Half, true);
						hbao.perPixelNormals = new HBAO.PerPixelNormalsParameter(HBAO.PerPixelNormals.Reconstruct2Samples);
						break;
					case 2:
						hbao.active = true;
						hbao.SetQuality(HBAO.Quality.High);
						hbao.resolution = new HBAO.ResolutionParameter(HBAO.Resolution.Full, true);
						hbao.perPixelNormals = new HBAO.PerPixelNormalsParameter(HBAO.PerPixelNormals.Reconstruct4Samples);
						break;
				}
			}*/
		}
	}
}
