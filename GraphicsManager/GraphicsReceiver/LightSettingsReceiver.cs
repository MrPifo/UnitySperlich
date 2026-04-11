using Sperlich.GameSettings;
using System.Collections;
using System.Collections.Generic;
//using Umbra;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Sperlich.GameSettings.Settings;

[RequireComponent(typeof(Light))]
public class LightSettingsReceiver : MonoBehaviour, IGraphicsReceiver {

	Light srcLight;
    //UmbraSoftShadows umbraLightSrc;
	UniversalAdditionalLightData lightDataSrc;
	public object AppliedValue { get; set; }

	void Awake() {
		srcLight = GetComponent<Light>();
        //umbraLightSrc = GetComponent<UmbraSoftShadows>();
		lightDataSrc = GetComponent<UniversalAdditionalLightData>();

		GameSettings.AddGraphicsListener(this);
		GameSettings.TriggerRefresh(GameSetting.ShadowQuality);
	}

	void OnDestroy() {
		GameSettings.RemoveGraphicsListener(this);
	}

	public void OnGameSettingsApplied(GameSetting setting, object value) {
		if(setting == GameSetting.ShadowQuality) {
			int level = (int)value;
			AppliedValue = level;

			/*if (umbraLightSrc != null && srcLight != null) {
				switch (level) {
					case 0:
					default:
						umbraLightSrc.profile.downsample = false;
						srcLight.shadows = LightShadows.None;
						umbraLightSrc.profile.frameSkipOptimization = true;
						umbraLightSrc.profile.contactShadows = false;
						break;
					case 1:
						umbraLightSrc.profile.downsample = true;
						srcLight.shadows = LightShadows.Hard;
						umbraLightSrc.profile.loopStepOptimization = LoopStep.x3;
						umbraLightSrc.profile.frameSkipOptimization = true;
						umbraLightSrc.profile.contactShadows = false;
						break;
					case 2:
						umbraLightSrc.profile.downsample = false;
						srcLight.shadows = LightShadows.Soft;
						umbraLightSrc.profile.loopStepOptimization = LoopStep.x2;
						umbraLightSrc.profile.frameSkipOptimization = true;
						umbraLightSrc.profile.contactShadows = false;
						break;
					case 3:
						umbraLightSrc.profile.downsample = false;
						srcLight.shadows = LightShadows.Soft;
						umbraLightSrc.profile.loopStepOptimization = LoopStep.Default;
						umbraLightSrc.profile.frameSkipOptimization = false;
						umbraLightSrc.profile.contactShadows = true;
						break;
				}
			}*/
		}
	}
}
