using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using PrimeTween;

namespace Sperlich.Sequencer {
	public partial class AnimSequencer {
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

		public T GetConfig<T>(string sequenceLabel, string stepTag) where T : AnimConfig, new() {
			var step = FindStep(sequenceLabel, stepTag);

			if (step == null) {
				return default;
			}

			T config = new T();
			config.ReadFrom(step);
			return config;
		}

		public T GetConfig<T>(string sequenceLabel, int stepIndex) where T : AnimConfig, new() {
			var step = FindStep(sequenceLabel, stepIndex);

			if (step == null) {
				return default;
			}

			T config = new T();
			config.ReadFrom(step);
			return config;
		}

		public void SetConfig<T>(string sequenceLabel, string stepTag, T config) where T : AnimConfig {
			var step = FindStep(sequenceLabel, stepTag);

			if (step != null) {
				config.ApplyTo(step);
			}
		}

		public void SetConfig<T>(string sequenceLabel, int stepIndex, T config) where T : AnimConfig {
			var step = FindStep(sequenceLabel, stepIndex);

			if (step != null) {
				config.ApplyTo(step);
			}
		}

		public bool RemoveStep(string sequenceLabel, string stepTag) {
			var seq = GetSequence(sequenceLabel);
			return seq != null && seq.steps.RemoveAll(s => s.tag == stepTag) > 0;
		}

		public bool RemoveStep(string sequenceLabel, int stepIndex) {
			var seq = GetSequence(sequenceLabel);

			if (seq != null && stepIndex >= 0 && stepIndex < seq.steps.Count) {
				seq.steps.RemoveAt(stepIndex);
				return true;
			}

			return false;
		}

		public void ClearSteps(string sequenceLabel) {
			var seq = GetSequence(sequenceLabel);

			if (seq != null) {
				seq.steps.Clear();
			}
		}

		public void SetWaitCondition(string seqLabel, string tag, System.Func<bool> lambda) { var s = FindStep(seqLabel, tag); if (s != null) { s.waitConditionLambda = lambda; } }
		public void SetWaitReady(string seqLabel, string tag) { var s = FindStep(seqLabel, tag); if (s != null) { s.waitUntilValue = true; } }
		public void SetTypeWriterText(string seqLabel, string tag, string text) { var s = FindStep(seqLabel, tag); if (s != null) { s.setTextValue = text; } }
		public void SetTextValue(string seqLabel, string tag, string text) { var s = FindStep(seqLabel, tag); if (s != null) { s.setTextValue = text; } }
		public void SetTarget(string seqLabel, string tag, Transform target) { var s = FindStep(seqLabel, tag); if (s != null) { s.target = target; } }
		public void SetTMPTarget(string seqLabel, string tag, TMP_Text tmp) { var s = FindStep(seqLabel, tag); if (s != null) { s.tmpTarget = tmp; } }
		public void SetImageTarget(string seqLabel, string tag, Image img) { var s = FindStep(seqLabel, tag); if (s != null) { s.imageTarget = img; } }
		public void SetImageSprite(string seqLabel, string tag, Sprite sprite) { var s = FindStep(seqLabel, tag); if (s != null) { s.setSpriteValue = sprite; } }
		public void SetColorColor(string seqLabel, string tag, Color color) { var s = FindStep(seqLabel, tag); if (s != null) { s.colorTo = color; } }
		public void SetDuration(string seqLabel, string tag, float duration) { var s = FindStep(seqLabel, tag); if (s != null) { s.duration = duration; } }
		public void SetDelay(string seqLabel, string tag, float delay) { var s = FindStep(seqLabel, tag); if (s != null) { s.delay = delay; } }
		public void SetFrameCount(string seqLabel, string tag, int frames) { var s = FindStep(seqLabel, tag); if (s != null) { s.waitFrames = frames; } }
		public void SetWaitMethod(string seqLabel, string tag, WaitMethod method) { var s = FindStep(seqLabel, tag); if (s != null) { s.waitMethod = method; } }
		public void SetFadeAlpha(string seqLabel, string tag, float alpha) { var s = FindStep(seqLabel, tag); if (s != null) { s.setFadeValue = alpha; } }

		public void SetWaitCondition(string seqLabel, int index, System.Func<bool> lambda) { var s = FindStep(seqLabel, index); if (s != null) { s.waitConditionLambda = lambda; } }
		public void SetWaitReady(string seqLabel, int index) { var s = FindStep(seqLabel, index); if (s != null) { s.waitUntilValue = true; } }
		public void SetTypeWriterText(string seqLabel, int index, string text) { var s = FindStep(seqLabel, index); if (s != null) { s.setTextValue = text; } }
		public void SetTextValue(string seqLabel, int index, string text) { var s = FindStep(seqLabel, index); if (s != null) { s.setTextValue = text; } }
		public void SetTarget(string seqLabel, int index, Transform target) { var s = FindStep(seqLabel, index); if (s != null) { s.target = target; } }
		public void SetTMPTarget(string seqLabel, int index, TMP_Text tmp) { var s = FindStep(seqLabel, index); if (s != null) { s.tmpTarget = tmp; } }
		public void SetImageTarget(string seqLabel, int index, Image img) { var s = FindStep(seqLabel, index); if (s != null) { s.imageTarget = img; } }
		public void SetImageSprite(string seqLabel, int index, Sprite sprite) { var s = FindStep(seqLabel, index); if (s != null) { s.setSpriteValue = sprite; } }
		public void SetColorColor(string seqLabel, int index, Color color) { var s = FindStep(seqLabel, index); if (s != null) { s.colorTo = color; } }
		public void SetDuration(string seqLabel, int index, float duration) { var s = FindStep(seqLabel, index); if (s != null) { s.duration = duration; } }
		public void SetDelay(string seqLabel, int index, float delay) { var s = FindStep(seqLabel, index); if (s != null) { s.delay = delay; } }
		public void SetFrameCount(string seqLabel, int index, int frames) { var s = FindStep(seqLabel, index); if (s != null) { s.waitFrames = frames; } }
		public void SetWaitMethod(string seqLabel, int index, WaitMethod method) { var s = FindStep(seqLabel, index); if (s != null) { s.waitMethod = method; } }
		public void SetFadeAlpha(string seqLabel, int index, float alpha) { var s = FindStep(seqLabel, index); if (s != null) { s.setFadeValue = alpha; } }

		public void SetTextCounterTarget(string seqLabel, string tag, float to) {
			var s = FindStep(seqLabel, tag);
			if (s != null) {
				s.animateFromCurrent = true;
				s.textCounterTo = to;
			}
		}

		public void SetTextCounterTarget(string seqLabel, string tag, float from, float to) {
			var s = FindStep(seqLabel, tag);
			if (s != null) {
				s.animateFromCurrent = false;
				s.textCounterFrom = from;
				s.textCounterTo = to;
			}
		}

		public void SetTextCounterTarget(string seqLabel, int stepIndex, float to) {
			var s = FindStep(seqLabel, stepIndex);
			if (s != null) {
				s.animateFromCurrent = true;
				s.textCounterTo = to;
			}
		}

		public void SetTextCounterTarget(string seqLabel, int stepIndex, float from, float to) {
			var s = FindStep(seqLabel, stepIndex);
			if (s != null) {
				s.animateFromCurrent = false;
				s.textCounterFrom = from;
				s.textCounterTo = to;
			}
		}

		AnimStep FindStep(string sequenceLabel, string tag) {
			foreach (var seq in sequences) {
				if (seq.label != sequenceLabel) {
					continue;
				}

				foreach (var step in seq.steps) {
					if (!string.IsNullOrEmpty(step.tag) && step.tag == tag) {
						return step;
					}
				}
			}

			return null;
		}

		AnimStep FindStep(string sequenceLabel, int stepIndex) {
			foreach (var seq in sequences) {
				if (seq.label != sequenceLabel) {
					continue;
				}

				if (stepIndex >= 0 && stepIndex < seq.steps.Count) {
					return seq.steps[stepIndex];
				}
			}

			return null;
		}

		public string CopyToJson() {
			var wrapper = new SequenceWrapper {
				sequences = this.sequences
			};

			return JsonUtility.ToJson(wrapper, true);
		}

		public void PasteFromJson(string json) {
			if (string.IsNullOrEmpty(json)) {
				return;
			}

			try {
				var wrapper = JsonUtility.FromJson<SequenceWrapper>(json);
				if (wrapper != null && wrapper.sequences != null) {
					this.sequences = wrapper.sequences;
				}
			} catch (System.Exception e) {
				Debug.LogError($"[AnimSequencer] PasteFromJson fehlgeschlagen: {e.Message}");
			}
		}
	}

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

		public virtual void ReadFrom(AnimSequencer.AnimStep s) {
			label = s.tag;
			enabled = s.enabled;
			mode = s.mode;
			delay = s.delay;
		}
	}

	public abstract class TweenConfig : AnimConfig {
		public float duration = 0.3f;
		public Ease ease = Ease.InOutSine;
		public AnimationCurve customCurve;

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.duration = duration;
			s.ease = ease;
			s.customCurve = customCurve;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			duration = s.duration;
			ease = s.ease;
			customCurve = s.customCurve;
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

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			animateFromCurrent = s.animateFromCurrent;
			relativeOffset = s.relativeOffset;
		}
	}

	public class FadeConfig : TargetTweenConfig {
		public float from = 0f;
		public float to = 1f;

		public override AnimType GetAnimType() {
			return AnimType.Fade;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.fadeFrom = from;
			s.fadeTo = to;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			from = s.fadeFrom;
			to = s.fadeTo;
		}
	}

	public class ScaleConfig : TargetTweenConfig {
		public Vector3 from = Vector3.zero;
		public Vector3 to = Vector3.one;
		public Vector3 from3D = Vector3.zero;
		public Vector3 to3D = Vector3.one;

		public override AnimType GetAnimType() {
			return AnimType.Scale;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.scaleFrom = from;
			s.scaleTo = to;
			s.scaleFrom3D = from3D;
			s.scaleTo3D = to3D;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			from = s.scaleFrom;
			to = s.scaleTo;
			from3D = s.scaleFrom3D;
			to3D = s.scaleTo3D;
		}
	}

	public class SlideConfig : TargetTweenConfig {
		public Vector2 from = Vector2.zero;
		public Vector2 to = Vector2.zero;

		public override AnimType GetAnimType() {
			return AnimType.Slide;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.slideFrom = from;
			s.slideTo = to;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			from = s.slideFrom;
			to = s.slideTo;
		}
	}

	public class RotateConfig : TargetTweenConfig {
		public float from = 0f;
		public float to = 360f;

		public override AnimType GetAnimType() {
			return AnimType.Rotate;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.rotateFrom = from;
			s.rotateTo = to;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			from = s.rotateFrom;
			to = s.rotateTo;
		}
	}

	public class BounceConfig : TweenConfig {
		public Transform target;
		public float intensity = 30f;
		public Vector3 bounce3D = new Vector3(0, 1f, 0);
		public float count = 3f;

		public override AnimType GetAnimType() {
			return AnimType.Bounce;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.bounceIntensity = intensity;
			s.bounce3D = bounce3D;
			s.bounceCount = count;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			intensity = s.bounceIntensity;
			bounce3D = s.bounce3D;
			count = s.bounceCount;
		}
	}

	public class PunchRotateConfig : TweenConfig {
		public Transform target;
		public bool randomAngle = false;
		public float angle = 15f;
		public float angle1 = 15f;
		public float angle2 = -15f;
		public Vector3 punch3D = new Vector3(0, 0, 15f);
		public float frequency = 10f;

		public override AnimType GetAnimType() {
			return AnimType.PunchRotate;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.punchRotateRandom = randomAngle;
			s.punchRotateAngle = angle;
			s.punchRotateAngle1 = angle1;
			s.punchRotateAngle2 = angle2;
			s.punchRotate3D = punch3D;
			s.punchRotateFrequency = frequency;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			randomAngle = s.punchRotateRandom;
			angle = s.punchRotateAngle;
			angle1 = s.punchRotateAngle1;
			angle2 = s.punchRotateAngle2;
			punch3D = s.punchRotate3D;
			frequency = s.punchRotateFrequency;
		}
	}

	public class PunchScaleConfig : TweenConfig {
		public Transform target;
		public float intensity = 0.2f;
		public Vector3 punch3D = new Vector3(0.2f, 0.2f, 0.2f);
		public float frequency = 10f;

		public override AnimType GetAnimType() {
			return AnimType.PunchScale;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.punchScaleIntensity = intensity;
			s.punchScale3D = punch3D;
			s.punchScaleFrequency = frequency;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			intensity = s.punchScaleIntensity;
			punch3D = s.punchScale3D;
			frequency = s.punchScaleFrequency;
		}
	}

	public class ColorTintConfig : TargetTweenConfig {
		public Color from = Color.white;
		public Color to = Color.white;
		public ColorTargetType colorTarget = ColorTargetType.Image;

		public override AnimType GetAnimType() {
			return AnimType.ColorTint;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.colorFrom = from;
			s.colorTo = to;
			s.colorTarget = colorTarget;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			from = s.colorFrom;
			to = s.colorTo;
			colorTarget = s.colorTarget;
		}
	}

	public class FadeSpriteColorConfig : TweenConfig {
		public SpriteRenderer spriteTarget;
		public bool animateFromCurrent = false;
		public Color from = Color.white;
		public Color to = Color.white;

		public override AnimType GetAnimType() {
			return AnimType.FadeSpriteColor;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.spriteTarget = spriteTarget;
			s.animateFromCurrent = animateFromCurrent;
			s.colorFrom = from;
			s.colorTo = to;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			spriteTarget = s.spriteTarget;
			animateFromCurrent = s.animateFromCurrent;
			from = s.colorFrom;
			to = s.colorTo;
		}
	}

	public class TextCounterConfig : TweenConfig {
		public TMP_Text tmpTarget;
		public bool animateFromCurrent = false;
		public float from = 0f;
		public float to = 100f;
		public string format = "{0}";
		public bool roundToInt = true;

		public override AnimType GetAnimType() {
			return AnimType.TextCounter;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.tmpTarget = tmpTarget;
			s.animateFromCurrent = animateFromCurrent;
			s.textCounterFrom = from;
			s.textCounterTo = to;
			s.textCounterFormat = format;
			s.textCounterRoundToInt = roundToInt;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			tmpTarget = s.tmpTarget;
			animateFromCurrent = s.animateFromCurrent;
			from = s.textCounterFrom;
			to = s.textCounterTo;
			format = s.textCounterFormat;
			roundToInt = s.textCounterRoundToInt;
		}
	}

	public class TypeWriterConfig : AnimConfig {
		public TMP_Text tmpTarget;
		public string text = "";
		public float charsPerSecond = 20f;

		public override AnimType GetAnimType() {
			return AnimType.TypeWriter;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.tmpTarget = tmpTarget;
			s.setTextValue = text;
			s.typeWriterCharsPerSecond = charsPerSecond;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			tmpTarget = s.tmpTarget;
			text = s.setTextValue;
			charsPerSecond = s.typeWriterCharsPerSecond;
		}
	}

	public class WaitConfig : AnimConfig {
		public float duration = 0.3f;
		public WaitMethod waitMethod = WaitMethod.Seconds;
		public int frameCount = 1;

		public override AnimType GetAnimType() {
			return AnimType.Wait;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.duration = duration;
			s.waitMethod = waitMethod;
			s.waitFrames = frameCount;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			duration = s.duration;
			waitMethod = s.waitMethod;
			frameCount = s.waitFrames;
		}
	}

	public class SetColorConfig : AnimConfig {
		public Transform target;
		public Color color = Color.white;
		public ColorTargetType colorTarget = ColorTargetType.Image;

		public override AnimType GetAnimType() {
			return AnimType.SetColor;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.colorTo = color;
			s.colorTarget = colorTarget;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			color = s.colorTo;
			colorTarget = s.colorTarget;
		}
	}

	public class SetTextConfig : AnimConfig {
		public TMP_Text tmpTarget;
		public string text = "";

		public override AnimType GetAnimType() {
			return AnimType.SetText;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.tmpTarget = tmpTarget;
			s.setTextValue = text;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			tmpTarget = s.tmpTarget;
			text = s.setTextValue;
		}
	}

	public class SetSpriteConfig : AnimConfig {
		public SpriteRenderer spriteTarget;
		public Sprite sprite;

		public override AnimType GetAnimType() {
			return AnimType.SetSprite;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.spriteTarget = spriteTarget;
			s.setSpriteValue = sprite;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			spriteTarget = s.spriteTarget;
			sprite = s.setSpriteValue;
		}
	}

	public class SetImageConfig : AnimConfig {
		public Image imageTarget;
		public Sprite sprite;

		public override AnimType GetAnimType() {
			return AnimType.SetImage;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.imageTarget = imageTarget;
			s.setSpriteValue = sprite;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			imageTarget = s.imageTarget;
			sprite = s.setSpriteValue;
		}
	}

	public class SetTransformConfig : AnimConfig {
		public Transform target;
		public TransformSubType subType = TransformSubType.LocalPosition;
		public Vector3 value = Vector3.zero;
		public bool relativeOffset = false;

		public override AnimType GetAnimType() {
			return AnimType.SetTransform;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.transformSubType = subType;
			s.setTransformValue = value;
			s.relativeOffset = relativeOffset;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			subType = s.transformSubType;
			value = s.setTransformValue;
			relativeOffset = s.relativeOffset;
		}
	}

	public class SetActiveConfig : AnimConfig {
		public Transform target;
		public bool active = true;

		public override AnimType GetAnimType() {
			return AnimType.SetActive;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.setActiveValue = active;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			active = s.setActiveValue;
		}
	}

	public class TriggerConfig : AnimConfig {
		public AnimSequencer targetSequencer;
		public string targetSequenceLabel = "";

		public override AnimType GetAnimType() {
			return AnimType.Trigger;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.triggerSequencer = targetSequencer;
			s.triggerSequenceLabel = targetSequenceLabel;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			targetSequencer = s.triggerSequencer;
			targetSequenceLabel = s.triggerSequenceLabel;
		}
	}

	public class EventConfig : AnimConfig {
		public UnityEvent onEvent = new UnityEvent();

		public override AnimType GetAnimType() {
			return AnimType.Event;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.onEvent = onEvent;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			onEvent = s.onEvent;
		}
	}

	public class AnchorConfig : AnimConfig {
		public string anchorName = "";

		public AnchorConfig() {
		}

		public AnchorConfig(string anchorName) {
			this.anchorName = anchorName;
		}

		public override AnimType GetAnimType() {
			return AnimType.Anchor;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.anchorLabel = anchorName;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			anchorName = s.anchorLabel;
		}
	}

	public class RepeatConfig : AnimConfig {
		public string targetAnchor = "";

		public RepeatConfig() {
		}

		public RepeatConfig(string targetAnchor) {
			this.targetAnchor = targetAnchor;
		}

		public override AnimType GetAnimType() {
			return AnimType.Repeat;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.repeatAnchorLabel = targetAnchor;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			targetAnchor = s.repeatAnchorLabel;
		}
	}

	public class WaitUntilConfig : AnimConfig {
		public bool conditionValue = false;
		public System.Func<bool> conditionLambda = null;

		public override AnimType GetAnimType() {
			return AnimType.WaitUntil;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.waitUntilValue = conditionValue;
			s.waitConditionLambda = conditionLambda;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			conditionValue = s.waitUntilValue;
			conditionLambda = s.waitConditionLambda;
		}
	}

	public class SetFadeConfig : AnimConfig {
		public Transform target;
		public float alpha = 1f;

		public override AnimType GetAnimType() {
			return AnimType.SetFade;
		}

		public override void ApplyTo(AnimSequencer.AnimStep s) {
			base.ApplyTo(s);
			s.target = target;
			s.setFadeValue = alpha;
		}

		public override void ReadFrom(AnimSequencer.AnimStep s) {
			base.ReadFrom(s);
			target = s.target;
			alpha = s.setFadeValue;
		}
	}

	public static class AnimSequenceExtensions {
		public static AnimSequencer.AnimStep AppendStep<T>(this AnimSequencer.AnimSequence seq, T config) where T : AnimConfig {
			var step = new AnimSequencer.AnimStep {
				type = config.GetAnimType()
			};

			config.ApplyTo(step);
			seq.steps.Add(step);
			return step;
		}

		public static AnimSequencer.AnimStep AppendStep<T>(this AnimSequencer.AnimSequence seq, string label, T config) where T : AnimConfig {
			config.label = label;
			return AppendStep(seq, config);
		}

		public static AnimSequencer.AnimStep InsertStep<T>(this AnimSequencer.AnimSequence seq, int index, T config) where T : AnimConfig {
			var step = new AnimSequencer.AnimStep {
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

		public static AnimSequencer.AnimStep InsertStep<T>(this AnimSequencer.AnimSequence seq, int index, string label, T config) where T : AnimConfig {
			config.label = label;
			return InsertStep(seq, index, config);
		}

		private static AnimSequencer.AnimStep FindStep(AnimSequencer.AnimSequence seq, string tag) {
			return seq.steps.Find(s => s.tag == tag);
		}

		private static AnimSequencer.AnimStep FindStep(AnimSequencer.AnimSequence seq, int index) {
			if (index >= 0 && index < seq.steps.Count) {
				return seq.steps[index];
			}

			return null;
		}

		private static AnimSequencer.AnimStep GetLastStep(AnimSequencer.AnimSequence seq) {
			if (seq.steps.Count > 0) {
				return seq.steps[seq.steps.Count - 1];
			}

			return null;
		}

		public static T GetConfig<T>(this AnimSequencer.AnimSequence seq, string stepTag) where T : AnimConfig, new() {
			var step = FindStep(seq, stepTag);

			if (step == null) {
				return default;
			}

			T config = new T();
			config.ReadFrom(step);
			return config;
		}

		public static T GetConfig<T>(this AnimSequencer.AnimSequence seq, int stepIndex) where T : AnimConfig, new() {
			var step = FindStep(seq, stepIndex);

			if (step == null) {
				return default;
			}

			T config = new T();
			config.ReadFrom(step);
			return config;
		}

		public static AnimSequencer.AnimSequence SetConfig<T>(this AnimSequencer.AnimSequence seq, string stepTag, T config) where T : AnimConfig {
			var step = FindStep(seq, stepTag);

			if (step != null) {
				config.ApplyTo(step);
			}

			return seq;
		}

		public static AnimSequencer.AnimSequence SetConfig<T>(this AnimSequencer.AnimSequence seq, int stepIndex, T config) where T : AnimConfig {
			var step = FindStep(seq, stepIndex);

			if (step != null) {
				config.ApplyTo(step);
			}

			return seq;
		}

		public static bool RemoveStep(this AnimSequencer.AnimSequence seq, string stepTag) {
			return seq.steps.RemoveAll(s => s.tag == stepTag) > 0;
		}

		public static bool RemoveStep(this AnimSequencer.AnimSequence seq, int stepIndex) {
			if (stepIndex >= 0 && stepIndex < seq.steps.Count) {
				seq.steps.RemoveAt(stepIndex);
				return true;
			}

			return false;
		}

		public static AnimSequencer.AnimSequence ClearSteps(this AnimSequencer.AnimSequence seq) {
			seq.steps.Clear();
			return seq;
		}

		public static AnimSequencer.AnimStep SetRelative(this AnimSequencer.AnimStep s, bool relative = true) { if (s != null) { s.relativeOffset = relative; } return s; }
		public static AnimSequencer.AnimSequence SetRelative(this AnimSequencer.AnimSequence seq, bool relative = true) { GetLastStep(seq)?.SetRelative(relative); return seq; }
		public static AnimSequencer.AnimSequence SetRelative(this AnimSequencer.AnimSequence seq, string tag, bool relative = true) { FindStep(seq, tag)?.SetRelative(relative); return seq; }
		public static AnimSequencer.AnimSequence SetRelative(this AnimSequencer.AnimSequence seq, int index, bool relative = true) { FindStep(seq, index)?.SetRelative(relative); return seq; }

		public static AnimSequencer.AnimStep SetWaitCondition(this AnimSequencer.AnimStep s, System.Func<bool> lambda) { if (s != null) { s.waitConditionLambda = lambda; } return s; }
		public static AnimSequencer.AnimSequence SetWaitCondition(this AnimSequencer.AnimSequence seq, System.Func<bool> lambda) { GetLastStep(seq)?.SetWaitCondition(lambda); return seq; }
		public static AnimSequencer.AnimSequence SetWaitCondition(this AnimSequencer.AnimSequence seq, string tag, System.Func<bool> lambda) { FindStep(seq, tag)?.SetWaitCondition(lambda); return seq; }
		public static AnimSequencer.AnimSequence SetWaitCondition(this AnimSequencer.AnimSequence seq, int index, System.Func<bool> lambda) { FindStep(seq, index)?.SetWaitCondition(lambda); return seq; }

		public static AnimSequencer.AnimStep SetWaitReady(this AnimSequencer.AnimStep s) { if (s != null) { s.waitUntilValue = true; } return s; }
		public static AnimSequencer.AnimSequence SetWaitReady(this AnimSequencer.AnimSequence seq) { GetLastStep(seq)?.SetWaitReady(); return seq; }
		public static AnimSequencer.AnimSequence SetWaitReady(this AnimSequencer.AnimSequence seq, string tag) { FindStep(seq, tag)?.SetWaitReady(); return seq; }
		public static AnimSequencer.AnimSequence SetWaitReady(this AnimSequencer.AnimSequence seq, int index) { FindStep(seq, index)?.SetWaitReady(); return seq; }

		public static AnimSequencer.AnimStep SetTypeWriterText(this AnimSequencer.AnimStep s, string text) { if (s != null) { s.setTextValue = text; } return s; }
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, string text) { GetLastStep(seq)?.SetTypeWriterText(text); return seq; }
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, string tag, string text) { FindStep(seq, tag)?.SetTypeWriterText(text); return seq; }
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, int index, string text) { FindStep(seq, index)?.SetTypeWriterText(text); return seq; }

		public static AnimSequencer.AnimStep SetTextValue(this AnimSequencer.AnimStep s, string text) { if (s != null) { s.setTextValue = text; } return s; }
		public static AnimSequencer.AnimSequence SetTextValue(this AnimSequencer.AnimSequence seq, string text) { GetLastStep(seq)?.SetTextValue(text); return seq; }
		public static AnimSequencer.AnimSequence SetTextValue(this AnimSequencer.AnimSequence seq, string tag, string text) { FindStep(seq, tag)?.SetTextValue(text); return seq; }
		public static AnimSequencer.AnimSequence SetTextValue(this AnimSequencer.AnimSequence seq, int index, string text) { FindStep(seq, index)?.SetTextValue(text); return seq; }

		public static AnimSequencer.AnimStep SetTextCounterTarget(this AnimSequencer.AnimStep s, float to) { if (s != null) { s.animateFromCurrent = true; s.textCounterTo = to; } return s; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, float to) { GetLastStep(seq)?.SetTextCounterTarget(to); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, string tag, float to) { FindStep(seq, tag)?.SetTextCounterTarget(to); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, int index, float to) { FindStep(seq, index)?.SetTextCounterTarget(to); return seq; }

		public static AnimSequencer.AnimStep SetTextCounterTarget(this AnimSequencer.AnimStep s, float from, float to) { if (s != null) { s.animateFromCurrent = false; s.textCounterFrom = from; s.textCounterTo = to; } return s; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, float from, float to) { GetLastStep(seq)?.SetTextCounterTarget(from, to); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, string tag, float from, float to) { FindStep(seq, tag)?.SetTextCounterTarget(from, to); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, int index, float from, float to) { FindStep(seq, index)?.SetTextCounterTarget(from, to); return seq; }

		public static AnimSequencer.AnimStep SetTarget(this AnimSequencer.AnimStep s, Transform target) { if (s != null) { s.target = target; } return s; }
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, Transform target) { GetLastStep(seq)?.SetTarget(target); return seq; }
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, string tag, Transform target) { FindStep(seq, tag)?.SetTarget(target); return seq; }
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, int index, Transform target) { FindStep(seq, index)?.SetTarget(target); return seq; }

		public static AnimSequencer.AnimStep SetTMPTarget(this AnimSequencer.AnimStep s, TMPro.TMP_Text tmp) { if (s != null) { s.tmpTarget = tmp; } return s; }
		public static AnimSequencer.AnimSequence SetTMPTarget(this AnimSequencer.AnimSequence seq, TMPro.TMP_Text tmp) { GetLastStep(seq)?.SetTMPTarget(tmp); return seq; }
		public static AnimSequencer.AnimSequence SetTMPTarget(this AnimSequencer.AnimSequence seq, string tag, TMPro.TMP_Text tmp) { FindStep(seq, tag)?.SetTMPTarget(tmp); return seq; }
		public static AnimSequencer.AnimSequence SetTMPTarget(this AnimSequencer.AnimSequence seq, int index, TMPro.TMP_Text tmp) { FindStep(seq, index)?.SetTMPTarget(tmp); return seq; }

		public static AnimSequencer.AnimStep SetImageTarget(this AnimSequencer.AnimStep s, UnityEngine.UI.Image img) { if (s != null) { s.imageTarget = img; } return s; }
		public static AnimSequencer.AnimSequence SetImageTarget(this AnimSequencer.AnimSequence seq, UnityEngine.UI.Image img) { GetLastStep(seq)?.SetImageTarget(img); return seq; }
		public static AnimSequencer.AnimSequence SetImageTarget(this AnimSequencer.AnimSequence seq, string tag, UnityEngine.UI.Image img) { FindStep(seq, tag)?.SetImageTarget(img); return seq; }
		public static AnimSequencer.AnimSequence SetImageTarget(this AnimSequencer.AnimSequence seq, int index, UnityEngine.UI.Image img) { FindStep(seq, index)?.SetImageTarget(img); return seq; }

		public static AnimSequencer.AnimStep SetImageSprite(this AnimSequencer.AnimStep s, Sprite sprite) { if (s != null) { s.setSpriteValue = sprite; } return s; }
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, Sprite sprite) { GetLastStep(seq)?.SetImageSprite(sprite); return seq; }
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, string tag, Sprite sprite) { FindStep(seq, tag)?.SetImageSprite(sprite); return seq; }
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, int index, Sprite sprite) { FindStep(seq, index)?.SetImageSprite(sprite); return seq; }

		public static AnimSequencer.AnimStep SetColorColor(this AnimSequencer.AnimStep s, Color color) { if (s != null) { s.colorTo = color; } return s; }
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, Color color) { GetLastStep(seq)?.SetColorColor(color); return seq; }
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, string tag, Color color) { FindStep(seq, tag)?.SetColorColor(color); return seq; }
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, int index, Color color) { FindStep(seq, index)?.SetColorColor(color); return seq; }

		public static AnimSequencer.AnimStep SetDuration(this AnimSequencer.AnimStep s, float duration) { if (s != null) { s.duration = duration; } return s; }
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, float duration) { GetLastStep(seq)?.SetDuration(duration); return seq; }
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, string tag, float duration) { FindStep(seq, tag)?.SetDuration(duration); return seq; }
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, int index, float duration) { FindStep(seq, index)?.SetDuration(duration); return seq; }

		public static AnimSequencer.AnimStep SetDelay(this AnimSequencer.AnimStep s, float delay) { if (s != null) { s.delay = delay; } return s; }
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, float delay) { GetLastStep(seq)?.SetDelay(delay); return seq; }
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, string tag, float delay) { FindStep(seq, tag)?.SetDelay(delay); return seq; }
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, int index, float delay) { FindStep(seq, index)?.SetDelay(delay); return seq; }

		public static AnimSequencer.AnimStep SetFrameCount(this AnimSequencer.AnimStep s, int frames) { if (s != null) { s.waitFrames = frames; } return s; }
		public static AnimSequencer.AnimSequence SetFrameCount(this AnimSequencer.AnimSequence seq, int frames) { GetLastStep(seq)?.SetFrameCount(frames); return seq; }
		public static AnimSequencer.AnimSequence SetFrameCount(this AnimSequencer.AnimSequence seq, string tag, int frames) { FindStep(seq, tag)?.SetFrameCount(frames); return seq; }
		public static AnimSequencer.AnimSequence SetFrameCount(this AnimSequencer.AnimSequence seq, int index, int frames) { FindStep(seq, index)?.SetFrameCount(frames); return seq; }

		public static AnimSequencer.AnimStep SetWaitMethod(this AnimSequencer.AnimStep s, WaitMethod method) { if (s != null) { s.waitMethod = method; } return s; }
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, WaitMethod method) { GetLastStep(seq)?.SetWaitMethod(method); return seq; }
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, string tag, WaitMethod method) { FindStep(seq, tag)?.SetWaitMethod(method); return seq; }
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, int index, WaitMethod method) { FindStep(seq, index)?.SetWaitMethod(method); return seq; }

		public static AnimSequencer.AnimStep SetFadeAlpha(this AnimSequencer.AnimStep s, float alpha) { if (s != null) { s.setFadeValue = alpha; } return s; }
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, float alpha) { GetLastStep(seq)?.SetFadeAlpha(alpha); return seq; }
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, string tag, float alpha) { FindStep(seq, tag)?.SetFadeAlpha(alpha); return seq; }
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, int index, float alpha) { FindStep(seq, index)?.SetFadeAlpha(alpha); return seq; }

		public static void Play(this AnimSequencer.AnimSequence seq) {
			if (seq.owner != null) {
				seq.owner.PlaySequence(seq);
			} else {
				Debug.LogWarning($"[AnimSequencer] Sequence '{seq.label}' has no owner. Did you create it without CreateSequence()?");
			}
		}

		public static void Pause(this AnimSequencer.AnimSequence seq) {
			if (seq.owner != null) {
				seq.owner.Pause(seq.label);
			}
		}

		public static void Resume(this AnimSequencer.AnimSequence seq) {
			if (seq.owner != null) {
				seq.owner.Resume(seq.label);
			}
		}

		public static void Stop(this AnimSequencer.AnimSequence seq) {
			if (seq.owner != null) {
				seq.owner.StopByLabel(seq.label);
			}
		}
	}
}