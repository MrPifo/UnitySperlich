using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Sperlich.Sequencer {
	public partial class AnimSequencer {

		#region Global Runner (Autonomous Sequences)
		private static AnimSequencer _globalRunner;
		public static AnimSequencer Global {
			get {
				if (_globalRunner == null) {
					var existing = GameObject.Find("[AnimSequencer_GlobalRunner]");

					if (existing != null) {
						_globalRunner = existing.GetComponent<AnimSequencer>();
					}

					if (_globalRunner == null) {
						var go = new GameObject("[AnimSequencer_GlobalRunner]");
						GameObject.DontDestroyOnLoad(go);
						go.hideFlags = HideFlags.HideInHierarchy;
						_globalRunner = go.AddComponent<AnimSequencer>();
					}
				}

				return _globalRunner;
			}
		}
		public static AnimSequence Create(string label = "") {
			var seq = Global.CreateSequence(label, TriggerType.Manual);
			seq.isTemporary = true;
			return seq;
		}
		#endregion

		#region Sequencer Setup API
		public AnimSequence CreateSequence(string label = "", TriggerType trigger = TriggerType.Manual) {
			var seq = new AnimSequence {
				label = label,
				trigger = trigger,
				owner = this
			};
			sequences.Add(seq);
			return seq;
		}
		public AnimSequence GetSequence(string label) {
			return sequences.Find(s => s.label == label);
		}

		public bool RemoveSequence(string label) {
			return sequences.RemoveAll(s => s.label == label) > 0;
		}
		public void ClearSequences() {
			sequences.Clear();
		}

		public AnimStep AppendStep<T>(string sequenceLabel, T config) where T : AnimConfig {
			var seq = GetSequence(sequenceLabel);
			if (seq == null) {
				return null;
			}

			var step = new AnimStep {
				type = config.GetAnimType()
			};

			config.ApplyTo(step);
			seq.steps.Add(step);
			return step;
		}
		public AnimStep InsertStep<T>(string sequenceLabel, int index, T config) where T : AnimConfig {
			var seq = GetSequence(sequenceLabel);
			if (seq == null) {
				return null;
			}

			var step = new AnimStep {
				type = config.GetAnimType()
			};

			config.ApplyTo(step);

			if (index < 0 || index >= seq.steps.Count) {
				seq.steps.Add(step);
			} else {
				seq.steps.Insert(index, step);
			}

			return step;
		}
		public bool RemoveStep(string sequenceLabel, string stepTag) {
			var seq = GetSequence(sequenceLabel);
			if (seq != null) {
				return seq.steps.RemoveAll(s => s.tag == stepTag) > 0;
			}
			return false;
		}

		public string CopyToJson() {
			return JsonUtility.ToJson(new SequenceWrapper { sequences = this.sequences }, true);
		}
		public void PasteFromJson(string json) {
			try {
				var w = JsonUtility.FromJson<SequenceWrapper>(json);

				if (w != null && w.sequences != null) {
					this.sequences = w.sequences;

					foreach (var seq in this.sequences) {
						seq.owner = this;
					}
				}
			} catch (System.Exception e) {
				Debug.LogError($"[AnimSequencer] PasteFromJson failed: {e.Message}");
			}
		}
		#endregion

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void OnDomainReload() {
			_globalRunner = null;
		}
#endif
	}

	#region Base Configurations
	public abstract class AnimConfig {
		public string label = "";
		public bool enabled = true;
		public StepMode mode = StepMode.Sequential;
		public float delay = 0f;

		public abstract AnimType GetAnimType();

		public virtual void ApplyTo(AnimSequencer.AnimStep s) {
			s.tag = label;
			s.enabled = enabled;
			s.mode = mode;
			s.delay = delay;
		}
	}

	public abstract class TweenConfig : AnimConfig {
		public float duration = 0.3f;
		public PrimeTween.Ease ease = PrimeTween.Ease.InOutSine;
		public AnimationCurve customCurve;

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.duration = duration;
			s.ease = ease;
			s.customCurve = customCurve;
		}
	}

	public abstract class TargetTweenConfig : TweenConfig {
		public Transform target;
		public bool animateFromCurrent = false;
		public bool relativeOffset = false;

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.animateFromCurrent = animateFromCurrent;
			s.relativeOffset = relativeOffset;
		}
	}
	#endregion

	#region Transform & UI Tweens
	public class FadeConfig : TargetTweenConfig {
		public float from = 0f;
		public float to = 1f;
		public override AnimType GetAnimType() { return AnimType.Fade; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.fadeFrom = from; s.fadeTo = to; }
	}

	public class ScaleConfig : TargetTweenConfig {
		public Vector3 from = Vector3.zero;
		public Vector3 to = Vector3.one;
		public Vector3 from3D = Vector3.zero;
		public Vector3 to3D = Vector3.one;
		public override AnimType GetAnimType() { return AnimType.Scale; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.scaleFrom = from; s.scaleTo = to; s.scaleFrom3D = from3D; s.scaleTo3D = to3D; }
	}

	public class SlideConfig : TargetTweenConfig {
		public Vector2 from = Vector2.zero;
		public Vector2 to = Vector2.zero;
		public override AnimType GetAnimType() { return AnimType.Slide; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.slideFrom = from; s.slideTo = to; }
	}

	public class RotateConfig : TargetTweenConfig {
		public float from = 0f;
		public float to = 360f;
		public override AnimType GetAnimType() { return AnimType.Rotate; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.rotateFrom = from; s.rotateTo = to; }
	}

	public class SizeDeltaConfig : TargetTweenConfig {
		public Vector2 from = Vector2.zero;
		public Vector2 to = Vector2.zero;
		public override AnimType GetAnimType() { return AnimType.SizeDelta; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.sizeDeltaFrom = from; s.sizeDeltaTo = to; }
	}

	public class FillAmountConfig : TweenConfig {
		public Image imageTarget;
		public bool animateFromCurrent = false;
		public float from = 0f;
		public float to = 1f;
		public override AnimType GetAnimType() { return AnimType.FillAmount; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.imageTarget = imageTarget; s.animateFromCurrent = animateFromCurrent; s.fillAmountFrom = from; s.fillAmountTo = to; }
	}
	#endregion

	#region Juice & Feedback Tweens
	public class BounceConfig : TweenConfig {
		public Transform target;
		public float intensity = 30f;
		public Vector3 bounce3D = new Vector3(0, 1f, 0);
		public float count = 3f;
		public override AnimType GetAnimType() { return AnimType.Bounce; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.target = target; s.bounceIntensity = intensity; s.bounce3D = bounce3D; s.bounceCount = count; }
	}

	public class PunchRotateConfig : TweenConfig {
		public Transform target;
		public bool randomAngle = false;
		public float angle = 15f;
		public float angle1 = 15f;
		public float angle2 = -15f;
		public Vector3 punch3D = new Vector3(0, 0, 15f);
		public float frequency = 10f;
		public override AnimType GetAnimType() { return AnimType.PunchRotate; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.target = target; s.punchRotateRandom = randomAngle; s.punchRotateAngle = angle; s.punchRotateAngle1 = angle1; s.punchRotateAngle2 = angle2; s.punchRotate3D = punch3D; s.punchRotateFrequency = frequency; }
	}

	public class PunchScaleConfig : TweenConfig {
		public Transform target;
		public bool useVector3 = false;
		public float intensity = 0.2f;
		public Vector3 punch3D = new Vector3(0.2f, 0.2f, 0.2f);
		public float frequency = 10f;
		public override AnimType GetAnimType() { return AnimType.PunchScale; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { 
			base.ApplyTo(s); 
			s.target = target; 
			s.punchScaleUseVector3 = useVector3;
			s.punchScaleIntensity = intensity; 
			s.punchScale3D = punch3D; 
			s.punchScaleFrequency = frequency; 
		}
	}

	public class ShakePositionConfig : TargetTweenConfig {
		public Vector3 strength = new Vector3(10f, 10f, 0f);
		public float frequency = 10f;
		public bool falloff = true;
		public override AnimType GetAnimType() { return AnimType.ShakePosition; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.shakeStrength = strength; s.shakeFrequency = frequency; s.shakeFalloff = falloff; }
	}

	public class ShakeRotationConfig : TargetTweenConfig {
		public Vector3 strength = new Vector3(0f, 0f, 10f);
		public float frequency = 10f;
		public bool falloff = true;
		public override AnimType GetAnimType() { return AnimType.ShakeRotation; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.shakeStrength = strength; s.shakeFrequency = frequency; s.shakeFalloff = falloff; }
	}
	#endregion

	#region Color & Material Tweens
	public class ColorTintConfig : TargetTweenConfig {
		public Color from = Color.white;
		public Color to = Color.white;
		public ColorTargetType colorTarget = ColorTargetType.Image;
		public ColorTintMode colorMode = ColorTintMode.RGBA;
		public override AnimType GetAnimType() { return AnimType.ColorTint; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.colorFrom = from; s.colorTo = to; s.colorTarget = colorTarget; s.colorTintMode = colorMode; }
	}

	public class FadeSpriteColorConfig : TweenConfig {
		public SpriteRenderer spriteTarget;
		public bool animateFromCurrent = false;
		public Color from = Color.white;
		public Color to = Color.white;
		public override AnimType GetAnimType() { return AnimType.FadeSpriteColor; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.spriteTarget = spriteTarget; s.animateFromCurrent = animateFromCurrent; s.colorFrom = from; s.colorTo = to; }
	}

	public class MaterialFloatConfig : TweenConfig {
		public Renderer rendererTarget;
		public UnityEngine.UI.Graphic graphicTarget;
		public Material materialTarget;
		public int materialIndex = 0;
		public string propertyName = "_BaseColor";
		public bool animateFromCurrent = false;
		public float from = 0f;
		public float to = 1f;

		// Zeigt jetzt auf den NEUEN Master-Typen!
		public override AnimType GetAnimType() { return AnimType.MaterialProperty; }

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rendererTarget = rendererTarget;
			s.graphicTarget = graphicTarget;
			s.materialTarget = materialTarget;
			s.materialIndex = materialIndex;
			s.materialPropertyName = propertyName;
			s.materialPropertyType = MaterialPropertyType.Float;
			s.animateFromCurrent = animateFromCurrent;
			s.materialFloatFrom = from;
			s.materialFloatTo = to;
		}
	}

	public class MaterialColorConfig : TweenConfig {
		public Renderer rendererTarget;
		public UnityEngine.UI.Graphic graphicTarget;
		public Material materialTarget;
		public int materialIndex = 0;
		public string propertyName = "_BaseColor";
		public bool animateFromCurrent = false;
		public Color from = Color.white;
		public Color to = Color.white;

		public override AnimType GetAnimType() { return AnimType.MaterialProperty; }

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rendererTarget = rendererTarget;
			s.graphicTarget = graphicTarget;
			s.materialTarget = materialTarget;
			s.materialIndex = materialIndex;
			s.materialPropertyName = propertyName;
			s.materialPropertyType = MaterialPropertyType.Color;
			s.animateFromCurrent = animateFromCurrent;
			s.materialColorFrom = from;
			s.materialColorTo = to;
		}
	}
	#endregion

	#region Text & Data Tweens
	public class TextCounterConfig : TweenConfig {
		public TMP_Text tmpTarget;
		public bool animateFromCurrent = false;
		public float from = 0f;
		public float to = 100f;
		public string format = "{0}";
		public bool roundToInt = true;
		public override AnimType GetAnimType() { return AnimType.TextCounter; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.tmpTarget = tmpTarget; s.animateFromCurrent = animateFromCurrent; s.textCounterFrom = from; s.textCounterTo = to; s.textCounterFormat = format; s.textCounterRoundToInt = roundToInt; }
	}

	public class TypeWriterConfig : AnimConfig {
		public TMP_Text tmpTarget;
		public string text = "";
		public float charsPerSecond = 20f;
		public override AnimType GetAnimType() { return AnimType.TypeWriter; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.tmpTarget = tmpTarget; s.setTextValue = text; s.typeWriterCharsPerSecond = charsPerSecond; }
	}
	#endregion

	#region Audio & Time Tweens
	public class PlayAudioConfig : AnimConfig {
		public AudioSource audioTarget;
		public AudioClip audioClip;
		public Vector2 volume = new Vector2(1f, 1f);
		public Vector2 pitch = new Vector2(1f, 1f);
		public float spatialBlend = 0f;
		public override AnimType GetAnimType() { return AnimType.PlayAudio; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.audioTarget = audioTarget; s.audioClip = audioClip; s.audioVolume = volume; s.audioPitch = pitch; s.audioSpatialBlend = spatialBlend; }
	}

	public class FadeAudioConfig : TweenConfig {
		public AudioSource audioTarget;
		public bool animateFromCurrent = false;
		public float from = 0f;
		public float to = 1f;
		public override AnimType GetAnimType() { return AnimType.FadeAudio; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.audioTarget = audioTarget; s.animateFromCurrent = animateFromCurrent; s.fadeAudioFrom = from; s.fadeAudioTo = to; }
	}

	public class TimeScaleConfig : TweenConfig {
		public bool animateFromCurrent = false;
		public float from = 1f;
		public float to = 1f;
		public override AnimType GetAnimType() { return AnimType.TimeScale; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.animateFromCurrent = animateFromCurrent; s.timeScaleFrom = from; s.timeScaleTo = to; }
	}
	#endregion

	#region Instant Setters
	public class SetTransformConfig : AnimConfig {
		public Transform target;
		public TransformSubType subType = TransformSubType.LocalPosition;
		public Vector3 value = Vector3.zero;
		public bool relativeOffset = false;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Transform; s.target = target; s.transformSubType = subType; s.setTransformValue = value; s.relativeOffset = relativeOffset; }
	}

	public class SetActiveConfig : AnimConfig {
		public Transform target;
		public bool active = true;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Active; s.target = target; s.setActiveValue = active; }
	}

	public class SetCanvasGroupStateConfig : AnimConfig {
		public Transform target;
		public OptionalBool interactable = OptionalBool.Unchanged;
		public OptionalBool blocksRaycasts = OptionalBool.Unchanged;
		public OptionalBool ignoreParentGroups = OptionalBool.Unchanged;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.CanvasGroupState; s.target = target; s.cgInteractable = interactable; s.cgBlocksRaycasts = blocksRaycasts; s.cgIgnoreParentGroups = ignoreParentGroups; }
	}

	public class SetColorConfig : AnimConfig {
		public Transform target;
		public Color color = Color.white;
		public ColorTargetType colorTarget = ColorTargetType.Image;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Color; s.target = target; s.colorTo = color; s.colorTarget = colorTarget; }
	}

	public class SetTextConfig : AnimConfig {
		public TMP_Text tmpTarget;
		public string text = "";
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Text; s.tmpTarget = tmpTarget; s.setTextValue = text; }
	}

	public class SetSpriteConfig : AnimConfig {
		public SpriteRenderer spriteTarget;
		public Sprite sprite;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Sprite; s.spriteTarget = spriteTarget; s.setSpriteValue = sprite; }
	}

	public class SetImageConfig : AnimConfig {
		public Image imageTarget;
		public Sprite sprite;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Image; s.imageTarget = imageTarget; s.setSpriteValue = sprite; }
	}

	public class SetFadeConfig : AnimConfig {
		public Transform target;
		public float alpha = 1f;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Fade; s.target = target; s.setFadeValue = alpha; }
	}

	public class SetTimeScaleConfig : AnimConfig {
		public float timeScale = 1f;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.TimeScale; s.timeScaleTo = timeScale; }
	}

	public class SetSizeDeltaConfig : AnimConfig {
		public Transform target;
		public Vector2 size = Vector2.zero;
		public bool relativeOffset = false;
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.SizeDelta; s.target = target; s.setSizeDeltaValue = size; s.relativeOffset = relativeOffset; }
	}

	public class SetPivotConfig : AnimConfig {
		public Transform target;
		public Vector2 pivot = new Vector2(0.5f, 0.5f);
		public override AnimType GetAnimType() { return AnimType.SetProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.setPropertyType = SetPropertyType.Pivot; s.target = target; s.setPivotValue = pivot; }
	}

	public class SetMaterialFloatConfig : AnimConfig {
		public Renderer rendererTarget;
		public Graphic graphicTarget;
		public Material materialTarget;
		public int materialIndex = 0;
		public string propertyName = "_BaseColor";
		public float value = 1f;
		public override AnimType GetAnimType() { return AnimType.SetMaterialProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rendererTarget = rendererTarget;
			s.graphicTarget = graphicTarget;
			s.materialTarget = materialTarget;
			s.materialIndex = materialIndex;
			s.materialPropertyName = propertyName;
			s.materialPropertyType = MaterialPropertyType.Float;
			s.materialFloatTo = value;
		}
	}

	public class SetMaterialColorConfig : AnimConfig {
		public Renderer rendererTarget;
		public Graphic graphicTarget;
		public Material materialTarget;
		public int materialIndex = 0;
		public string propertyName = "_BaseColor";
		public Color value = Color.white;
		public override AnimType GetAnimType() { return AnimType.SetMaterialProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rendererTarget = rendererTarget;
			s.graphicTarget = graphicTarget;
			s.materialTarget = materialTarget;
			s.materialIndex = materialIndex;
			s.materialPropertyName = propertyName;
			s.materialPropertyType = MaterialPropertyType.Color;
			s.materialColorTo = value;
		}
	}
	#endregion

	#region Logic & Flow
	public class TriggerConfig : AnimConfig {
		public AnimSequencer targetSequencer;
		public string targetSequenceLabel = "";
		public override AnimType GetAnimType() { return AnimType.Trigger; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.triggerSequencer = targetSequencer; s.triggerSequenceLabel = targetSequenceLabel; }
	}

	public class EventConfig : AnimConfig {
		public UnityEvent onEvent = new UnityEvent();
		public override AnimType GetAnimType() { return AnimType.Event; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.onEvent = onEvent; }
	}

	public class WaitConfig : AnimConfig {
		public float duration = 0.3f;
		public WaitMethod waitMethod = WaitMethod.Seconds;
		public int frameCount = 1;
		public override AnimType GetAnimType() { return AnimType.Wait; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.duration = duration; s.waitMethod = waitMethod; s.waitFrames = frameCount; }
	}

	public class WaitUntilConfig : AnimConfig {
		public bool conditionValue = false;
		public System.Func<bool> conditionLambda = null;
		public override AnimType GetAnimType() { return AnimType.WaitUntil; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.waitUntilValue = conditionValue; s.waitConditionLambda = conditionLambda; }
	}

	public class AnchorConfig : AnimConfig {
		public string anchorName = "";
		public AnchorConfig(string anchorName = "") { this.anchorName = anchorName; }
		public override AnimType GetAnimType() { return AnimType.Anchor; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.anchorLabel = anchorName; }
	}

	public class RepeatConfig : AnimConfig {
		public string targetAnchor = "";
		public RepeatConfig(string targetAnchor = "") { this.targetAnchor = targetAnchor; }
		public override AnimType GetAnimType() { return AnimType.Repeat; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.repeatAnchorLabel = targetAnchor; }
	}

	public class MaterialPropertyConfig : TweenConfig {
		public Renderer rendererTarget;
		public Material materialTarget;
		public Graphic graphicTarget;
		public int materialIndex = 0;
		public string propertyName = "_BaseColor";
		public MaterialPropertyType propertyType = MaterialPropertyType.Float;
		public bool animateFromCurrent = false;
		public float floatFrom = 0f;
		public float floatTo = 1f;
		public Color colorFrom = Color.white;
		public Color colorTo = Color.white;
		public override AnimType GetAnimType() { return AnimType.MaterialProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rendererTarget = rendererTarget;
			s.materialTarget = materialTarget;
			s.materialIndex = materialIndex;
			s.materialPropertyName = propertyName;
			s.materialPropertyType = propertyType;
			s.animateFromCurrent = animateFromCurrent;
			s.materialFloatFrom = floatFrom;
			s.materialFloatTo = floatTo;
			s.materialColorFrom = colorFrom;
			s.materialColorTo = colorTo;
		}
	}

	public class SetMaterialPropertyConfig : AnimConfig {
		public Renderer rendererTarget;
		public Material materialTarget;
		public Graphic graphicTarget;
		public int materialIndex = 0;
		public string propertyName = "_BaseColor";
		public MaterialPropertyType propertyType = MaterialPropertyType.Float;
		public float floatValue = 1f;
		public Color colorValue = Color.white;
		public override AnimType GetAnimType() { return AnimType.SetMaterialProperty; }
		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rendererTarget = rendererTarget;
			s.materialTarget = materialTarget;
			s.materialIndex = materialIndex;
			s.materialPropertyName = propertyName;
			s.materialPropertyType = propertyType;
			s.materialFloatTo = floatValue;
			s.materialColorTo = colorValue;
		}
	}

	public class DestroyConfig : AnimConfig {
		/// <summary>The GameObject to destroy. Leave null to destroy the AnimSequencer's own GameObject.</summary>
		public Transform target;
		public override AnimType GetAnimType() { return AnimType.Destroy; }
		public override void ApplyTo(AnimSequencer.AnimStep s) { base.ApplyTo(s); s.target = target; }
	}

	public class ControlSequenceConfig : AnimConfig {
		public SequenceControlType action = SequenceControlType.Stop;
		public SequenceControlTarget targetScope = SequenceControlTarget.Self;
		public AnimSequencer targetSequencer;
		public string targetLabel = "";

		public override AnimType GetAnimType() { return AnimType.ControlSequence; }

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.sequenceControlType = action;
			s.sequenceControlTarget = targetScope;
			s.controlSequencerTarget = targetSequencer;
			s.controlSequenceLabel = targetLabel;
		}
	}
	#endregion


}