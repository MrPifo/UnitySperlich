using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;


namespace Sperlich.GameSettings {
	/// <summary>
	/// Enables getting/setting URP graphics settings properties that don't have built-in getters and setters.
	/// </summary>
	public static class PipelineExtensions {

		private static readonly Type pipelineAssetType = typeof(UniversalRenderPipelineAsset);
		private static readonly Type aoType = Type.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusionSettings, Unity.RenderPipelines.Universal.Runtime", true, true);
		private static readonly Type aoSettings = Type.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion, Unity.RenderPipelines.Universal.Runtime", true, true);
		private static readonly Type decalType = Type.GetType("UnityEngine.Rendering.Universal.DecalRendererFeature, Unity.RenderPipelines.Universal.Runtime", true, true);
		private static readonly Type decalSettings = Type.GetType("UnityEngine.Rendering.Universal.DecalSettings, Unity.RenderPipelines.Universal.Runtime", true, true);
		private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

		public static readonly FieldInfo MainLightCastShadows_FieldInfo;
		public static readonly FieldInfo AdditionalLightCastShadows_FieldInfo;
		public static readonly FieldInfo MainLightShadowmapResolution_FieldInfo;
		public static readonly FieldInfo AdditionalLightShadowmapResolution_FieldInfo;
		public static readonly FieldInfo Cascade2Split_FieldInfo;
		public static readonly FieldInfo Cascade4Split_FieldInfo;
		public static readonly FieldInfo SoftShadowsEnabled_FieldInfo;

		public static readonly FieldInfo AOSettings_FieldInfo;
		public static readonly FieldInfo AOIntensity_FieldInfo;
		public static readonly FieldInfo AOSampleCount_FieldInfo;
		public static readonly FieldInfo AOQuality_FieldInfo;
		public static readonly FieldInfo AOBlurQuality_FieldInfo;
		public static readonly FieldInfo AODownsample_FieldInfo;
		public static readonly FieldInfo AOAfterOpaque_FieldInfo;
		public static readonly FieldInfo AORadius_FieldInfo;

		public static readonly FieldInfo DecalSettings_FieldInfo;
		public static readonly FieldInfo DecalDrawDistance_FieldInfo;

		public static float AO_Intensity {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (float)AOIntensity_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AOIntensity_FieldInfo.SetValue(field, value);
			}
		}
		public static int AO_SampleRate {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (int)AOSampleCount_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AOSampleCount_FieldInfo.SetValue(field, value);
			}
		}
		public static int AO_Quality {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (int)AOQuality_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AOQuality_FieldInfo.SetValue(field, value);
			}
		}
		public static int AO_BlurQuality {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (int)AOBlurQuality_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AOBlurQuality_FieldInfo.SetValue(field, value);
			}
		}
		public static float AO_Radius {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (float)AORadius_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AORadius_FieldInfo.SetValue(field, value);
			}
		}
		public static bool AO_Downsample {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (bool)AODownsample_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AODownsample_FieldInfo.SetValue(field, value);
			}
		}
		/// <summary>
		/// Improves performance
		/// </summary>
		public static bool AO_AfterOpaque {
			get {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				return (bool)AOAfterOpaque_FieldInfo.GetValue(field);
			}
			set {
				object field = AOSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetAORendererFeature());
				AOAfterOpaque_FieldInfo.SetValue(field, value);
			}
		}
		public static float Decal_DrawDistance {
			get {
				object field = DecalSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetDecalRendererFeature());
				return (float)DecalDrawDistance_FieldInfo.GetValue(field);
			}
			set {
				object field = DecalSettings_FieldInfo.GetValue(GameSettings.RenderFeatures.GetDecalRendererFeature());
				DecalDrawDistance_FieldInfo.SetValue(field, value);
			}
		}
		public static bool MainLightCastShadows {
			get => (bool)MainLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => MainLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}
		public static bool AdditionalLightCastShadows {
			get => (bool)AdditionalLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => AdditionalLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}
		public static ShadowResolution MainLightShadowResolution {
			get => (ShadowResolution)MainLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => MainLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}
		public static ShadowResolution AdditionalLightShadowResolution {
			get => (ShadowResolution)AdditionalLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => AdditionalLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}
		public static float Cascade2Split {
			get => (float)Cascade2Split_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => Cascade2Split_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}
		public static Vector3 Cascade4Split {
			get => (Vector3)Cascade4Split_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => Cascade4Split_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}
		public static bool SoftShadowsEnabled {
			get => (bool)SoftShadowsEnabled_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
			set => SoftShadowsEnabled_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
		}

		static PipelineExtensions() {
			// Main light and shadow settings
			MainLightCastShadows_FieldInfo = TryGetField(pipelineAssetType, "m_MainLightShadowsSupported");
			AdditionalLightCastShadows_FieldInfo = TryGetField(pipelineAssetType, "m_AdditionalLightShadowsSupported");
			MainLightShadowmapResolution_FieldInfo = TryGetField(pipelineAssetType, "m_MainLightShadowmapResolution");
			AdditionalLightShadowmapResolution_FieldInfo = TryGetField(pipelineAssetType, "m_AdditionalLightsShadowmapResolution");
			Cascade2Split_FieldInfo = TryGetField(pipelineAssetType, "m_Cascade2Split");
			Cascade4Split_FieldInfo = TryGetField(pipelineAssetType, "m_Cascade4Split");
			SoftShadowsEnabled_FieldInfo = TryGetField(pipelineAssetType, "m_SoftShadowsSupported");

			// Ambient Occlusion settings
			AOSettings_FieldInfo = TryGetField(aoSettings, "m_Settings");
			AOIntensity_FieldInfo = TryGetField(aoType, "Intensity");
			AOSampleCount_FieldInfo = TryGetField(aoType, "Samples");
			AOQuality_FieldInfo = TryGetField(aoType, "NormalSamples");
			AOBlurQuality_FieldInfo = TryGetField(aoType, "BlurQuality");
			AODownsample_FieldInfo = TryGetField(aoType, "Downsample");
			AOAfterOpaque_FieldInfo = TryGetField(aoType, "AfterOpaque");
			AORadius_FieldInfo = TryGetField(aoType, "Radius");

			// Decal settings
			//DecalSettings_FieldInfo = TryGetField(decalSettings, "m_Settings");
			//DecalDrawDistance_FieldInfo = TryGetField(decalType, "maxDrawDistance");
		}

		public static ScriptableRendererFeature GetRendererFeature(this List<ScriptableRendererFeature> data, string rendererFeatureName) {
			return data.Find(r => r.name.Replace(" ", "").ToLower() == rendererFeatureName.Replace(" ", "").ToLower());
		}
		public static ScriptableRendererFeature GetAORendererFeature(this List<ScriptableRendererFeature> data) {
			return data.Find(r => r.name == "Ambient Occlusion");
		}
		public static ScriptableRendererFeature GetDecalRendererFeature(this List<ScriptableRendererFeature> data) {
			return data.Find(r => r.name == "DecalRendererFeature");
		}

		private static FieldInfo TryGetField(Type type, string fieldName) {
			FieldInfo field = type.GetField(fieldName, Flags);
			if (field == null) {
				throw new MissingFieldException($"Field '{fieldName}' not found in type '{type.FullName}'.");
			}
			return field;
		}
	}
}