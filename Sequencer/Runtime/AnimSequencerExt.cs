using System.Threading.Tasks;
using UnityEngine;
using PrimeTween;

namespace Sperlich.Sequencer {
	public static class AnimSequencerExt {

		#region Sequence Setup & Chaining
		public static AnimSequencer.AnimStep AppendStep<T>(this AnimSequencer.AnimSequence seq, T config) where T : AnimConfig {
			if (seq == null) {
				Debug.LogError("[AnimSequencer] Cannot AppendStep. Sequence is null.");
				return null;
			}
			var step = new AnimSequencer.AnimStep {
				type = config.GetAnimType()
			};
			config.ApplyTo(step);
			seq.steps.Add(step);
			return step;
		}
		public static AnimSequencer.AnimSequence AttachTo(this AnimSequencer.AnimSequence seq, GameObject target) {
			if (seq == null) {
				Debug.LogError("[AnimSequencer] Cannot AttachTo. Sequence is null.");
				return null;
			}
			if (target == null) {
				Debug.LogError("[AnimSequencer] Cannot AttachTo. Target GameObject is null.");
				return seq;
			}
			if (seq.owner != null) {
				seq.owner.sequences.Remove(seq);
			}
			var targetSequencer = target.GetComponent<AnimSequencer>();
			if (targetSequencer == null) {
				targetSequencer = target.AddComponent<AnimSequencer>();
			}
			seq.owner = targetSequencer;
			seq.isTemporary = false;
			targetSequencer.sequences.Add(seq);
			return seq;
		}
		#endregion

		#region Playback & Lifecycle
		public static void Play(this AnimSequencer.AnimSequence seq) {
			if (seq != null) {
				if (seq.owner != null) {
					seq.owner.PlaySequence(seq);
				} else {
					Debug.LogError($"[AnimSequencer] Cannot play. Sequence '{seq.label}' has no valid owner.");
				}
			} else {
				Debug.LogError("[AnimSequencer] Cannot play. Sequence is null.");
			}
		}
		public static void Pause(this AnimSequencer.AnimSequence seq) {
			if (seq != null) {
				if (seq.owner != null) {
					seq.owner.Pause(seq.label);
				} else {
					Debug.LogError($"[AnimSequencer] Cannot pause. Sequence '{seq.label}' has no valid owner.");
				}
			} else {
				Debug.LogError("[AnimSequencer] Cannot pause. Sequence is null.");
			}
		}
		public static void Resume(this AnimSequencer.AnimSequence seq) {
			if (seq != null) {
				if (seq.owner != null) {
					seq.owner.Resume(seq.label);
				} else {
					Debug.LogError($"[AnimSequencer] Cannot resume. Sequence '{seq.label}' has no valid owner.");
				}
			} else {
				Debug.LogError("[AnimSequencer] Cannot resume. Sequence is null.");
			}
		}
		public static AnimSequencer.AnimSequence OnComplete(this AnimSequencer.AnimSequence seq, System.Action action) {
			if (seq == null) {
				Debug.LogError("[AnimSequencer] Cannot set OnComplete. Sequence is null.");
				return null;
			}
			seq.onCompleteAction += action;
			return seq;
		}
		public static AnimSequencer.AnimSequence OnStart(this AnimSequencer.AnimSequence seq, System.Action action) {
			if (seq == null) {
				Debug.LogError("[AnimSequencer] Cannot set OnStart. Sequence is null.");
				return null;
			}
			seq.onStartAction += action;
			return seq;
		}
		public static async Task PlayAsync(this AnimSequencer.AnimSequence seq) {
			if (seq == null) {
				Debug.LogError("[AnimSequencer] Cannot PlayAsync. Sequence is null.");
				return;
			}
			var tcs = new TaskCompletionSource<bool>();
			System.Action completeAction = null;
			completeAction = () => {
				seq.onCompleteAction -= completeAction;
				tcs.TrySetResult(true);
			};
			seq.onCompleteAction += completeAction;
			seq.Play();
			await tcs.Task;
		}
		public static CustomYieldInstruction WaitForCompletion(this AnimSequencer.AnimSequence seq) {
			if (seq == null) {
				Debug.LogError("[AnimSequencer] Cannot WaitForCompletion. Sequence is null.");
				return null;
			}
			return new WaitWhile(() => seq.isPlaying);
		}
		public static async Task PlayAsync(this AnimSequencer sequencer, string sequenceLabel) {
			if (sequencer == null) {
				Debug.LogError("[AnimSequencer] Cannot PlayAsync. Sequencer is null.");
				return;
			}
			var seq = sequencer.GetSequence(sequenceLabel);
			if (seq != null) {
				await seq.PlayAsync();
			} else {
				Debug.LogError($"[AnimSequencer] Cannot PlayAsync. Sequence '{sequenceLabel}' not found.");
			}
		}
		public static CustomYieldInstruction WaitForCompletion(this AnimSequencer sequencer, string sequenceLabel) {
			if (sequencer == null) {
				Debug.LogError("[AnimSequencer] Cannot WaitForCompletion. Sequencer is null.");
				return null;
			}
			var seq = sequencer.GetSequence(sequenceLabel);
			if (seq != null) {
				return seq.WaitForCompletion();
			} else {
				Debug.LogError($"[AnimSequencer] Cannot WaitForCompletion. Sequence '{sequenceLabel}' not found.");
			}
			return null;
		}
		#endregion

		#region Locators & Central Evaluators
		public static int FindStepIndex(this AnimSequencer.AnimSequence seq, string tag) {
			if (seq == null) {
				return -1;
			}
			return seq.steps.FindIndex(s => s.tag == tag);
		}
		private static void ApplyToStep(AnimSequencer.AnimSequence seq, int index, System.Action<AnimSequencer.AnimStep> action, string propertyName) {
			if (seq == null) {
				Debug.LogError($"[AnimSequencer] Cannot set {propertyName}. Sequence is null.");
				return;
			}
			if (index >= 0) {
				if (index < seq.steps.Count) {
					var step = seq.steps[index];
					if (step != null) {
						action(step);
					}
				} else {
					Debug.LogError($"[AnimSequencer] Cannot set {propertyName}. Step at index {index} is out of bounds in sequence '{seq.label}'.");
				}
			} else {
				Debug.LogError($"[AnimSequencer] Cannot set {propertyName}. Invalid step index ({index}) in sequence '{seq.label}'. Check if your step tag exists.");
			}
		}
		private static AnimSequencer ApplyToSequence(AnimSequencer sequencer, string seqLabel, System.Action<AnimSequencer.AnimSequence> action) {
			if (sequencer == null) {
				Debug.LogError($"[AnimSequencer] Cannot modify sequence '{seqLabel}'. Sequencer component is null.");
				return null;
			}
			var seq = sequencer.GetSequence(seqLabel);
			if (seq != null) {
				action(seq);
			} else {
				Debug.LogError($"[AnimSequencer] Sequence '{seqLabel}' not found on '{sequencer.gameObject.name}'.");
			}
			return sequencer;
		}
		#endregion

		#region Duration
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, int index, float duration) {
			ApplyToStep(seq, index, s => s.duration = duration, "Duration");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, string tag, float duration) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetDuration(index, duration);
		}
		public static AnimSequencer SetDuration(this AnimSequencer sequencer, string seqLabel, int stepIndex, float duration) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetDuration(stepIndex, duration));
		}
		public static AnimSequencer SetDuration(this AnimSequencer sequencer, string seqLabel, string stepTag, float duration) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetDuration(stepTag, duration));
		}
		#endregion

		#region Delay
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, int index, float delay) {
			ApplyToStep(seq, index, s => s.delay = delay, "Delay");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, string tag, float delay) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetDelay(index, delay);
		}
		public static AnimSequencer SetDelay(this AnimSequencer sequencer, string seqLabel, int stepIndex, float delay) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetDelay(stepIndex, delay));
		}
		public static AnimSequencer SetDelay(this AnimSequencer sequencer, string seqLabel, string stepTag, float delay) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetDelay(stepTag, delay));
		}
		#endregion

		#region Ease
		public static AnimSequencer.AnimSequence SetEase(this AnimSequencer.AnimSequence seq, int index, Ease ease) {
			ApplyToStep(seq, index, s => s.ease = ease, "Ease");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetEase(this AnimSequencer.AnimSequence seq, string tag, Ease ease) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetEase(index, ease);
		}
		public static AnimSequencer SetEase(this AnimSequencer sequencer, string seqLabel, int stepIndex, Ease ease) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetEase(stepIndex, ease));
		}
		public static AnimSequencer SetEase(this AnimSequencer sequencer, string seqLabel, string stepTag, Ease ease) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetEase(stepTag, ease));
		}
		#endregion

		#region Target
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, int index, Transform target) {
			ApplyToStep(seq, index, s => {
				s.target = target;
				s.isInitialized = false;
			}, "Target");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, string tag, Transform target) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetTarget(index, target);
		}
		public static AnimSequencer SetTarget(this AnimSequencer sequencer, string seqLabel, int stepIndex, Transform target) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTarget(stepIndex, target));
		}
		public static AnimSequencer SetTarget(this AnimSequencer sequencer, string seqLabel, string stepTag, Transform target) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTarget(stepTag, target));
		}
		#endregion

		#region FadeAlpha
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, int index, float alpha) {
			ApplyToStep(seq, index, s => s.setFadeValue = alpha, "FadeAlpha");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, string tag, float alpha) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetFadeAlpha(index, alpha);
		}
		public static AnimSequencer SetFadeAlpha(this AnimSequencer sequencer, string seqLabel, int stepIndex, float alpha) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetFadeAlpha(stepIndex, alpha));
		}
		public static AnimSequencer SetFadeAlpha(this AnimSequencer sequencer, string seqLabel, string stepTag, float alpha) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetFadeAlpha(stepTag, alpha));
		}
		#endregion

		#region ScaleTo
		public static AnimSequencer.AnimSequence SetScaleTo(this AnimSequencer.AnimSequence seq, int index, Vector3 scale) {
			ApplyToStep(seq, index, s => {
				s.scaleTo = scale;
				s.scaleTo3D = scale;
			}, "ScaleTo");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetScaleTo(this AnimSequencer.AnimSequence seq, string tag, Vector3 scale) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetScaleTo(index, scale);
		}
		public static AnimSequencer SetScaleTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector3 scale) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetScaleTo(stepIndex, scale));
		}
		public static AnimSequencer SetScaleTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector3 scale) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetScaleTo(stepTag, scale));
		}
		#endregion

		#region SlideTo
		public static AnimSequencer.AnimSequence SetSlideTo(this AnimSequencer.AnimSequence seq, int index, Vector2 slide) {
			ApplyToStep(seq, index, s => s.slideTo = slide, "SlideTo");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetSlideTo(this AnimSequencer.AnimSequence seq, string tag, Vector2 slide) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetSlideTo(index, slide);
		}
		public static AnimSequencer SetSlideTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 slide) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetSlideTo(stepIndex, slide));
		}
		public static AnimSequencer SetSlideTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 slide) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetSlideTo(stepTag, slide));
		}
		#endregion

		#region RotateTo
		public static AnimSequencer.AnimSequence SetRotateTo(this AnimSequencer.AnimSequence seq, int index, float rotation) {
			ApplyToStep(seq, index, s => s.rotateTo = rotation, "RotateTo");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetRotateTo(this AnimSequencer.AnimSequence seq, string tag, float rotation) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetRotateTo(index, rotation);
		}
		public static AnimSequencer SetRotateTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, float rotation) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetRotateTo(stepIndex, rotation));
		}
		public static AnimSequencer SetRotateTo(this AnimSequencer sequencer, string seqLabel, string stepTag, float rotation) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetRotateTo(stepTag, rotation));
		}
		#endregion

		#region SizeDeltaTo
		public static AnimSequencer.AnimSequence SetSizeDeltaTo(this AnimSequencer.AnimSequence seq, int index, Vector2 size) {
			ApplyToStep(seq, index, s => s.sizeDeltaTo = size, "SizeDeltaTo");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetSizeDeltaTo(this AnimSequencer.AnimSequence seq, string tag, Vector2 size) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetSizeDeltaTo(index, size);
		}
		public static AnimSequencer SetSizeDeltaTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 size) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetSizeDeltaTo(stepIndex, size));
		}
		public static AnimSequencer SetSizeDeltaTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 size) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetSizeDeltaTo(stepTag, size));
		}
		#endregion

		#region FillAmountTo
		public static AnimSequencer.AnimSequence SetFillAmountTo(this AnimSequencer.AnimSequence seq, int index, float amount) {
			ApplyToStep(seq, index, s => s.fillAmountTo = amount, "FillAmountTo");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetFillAmountTo(this AnimSequencer.AnimSequence seq, string tag, float amount) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetFillAmountTo(index, amount);
		}
		public static AnimSequencer SetFillAmountTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, float amount) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetFillAmountTo(stepIndex, amount));
		}
		public static AnimSequencer SetFillAmountTo(this AnimSequencer sequencer, string seqLabel, string stepTag, float amount) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetFillAmountTo(stepTag, amount));
		}
		#endregion

		#region ColorTo
		public static AnimSequencer.AnimSequence SetColorTo(this AnimSequencer.AnimSequence seq, int index, Color color) {
			ApplyToStep(seq, index, s => s.colorTo = color, "ColorTo");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetColorTo(this AnimSequencer.AnimSequence seq, string tag, Color color) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetColorTo(index, color);
		}
		public static AnimSequencer SetColorTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Color color) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetColorTo(stepIndex, color));
		}
		public static AnimSequencer SetColorTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Color color) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetColorTo(stepTag, color));
		}
		#endregion

		#region ColorColor Alias
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, int index, Color color) {
			return seq.SetColorTo(index, color);
		}
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, string tag, Color color) {
			return seq.SetColorTo(tag, color);
		}
		public static AnimSequencer SetColorColor(this AnimSequencer sequencer, string seqLabel, int stepIndex, Color color) {
			return sequencer.SetColorTo(seqLabel, stepIndex, color);
		}
		public static AnimSequencer SetColorColor(this AnimSequencer sequencer, string seqLabel, string stepTag, Color color) {
			return sequencer.SetColorTo(seqLabel, stepTag, color);
		}
		#endregion

		#region Active
		public static AnimSequencer.AnimSequence SetActive(this AnimSequencer.AnimSequence seq, int index, bool isActive) {
			ApplyToStep(seq, index, s => s.setActiveValue = isActive, "Active");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetActive(this AnimSequencer.AnimSequence seq, string tag, bool isActive) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetActive(index, isActive);
		}
		public static AnimSequencer SetActive(this AnimSequencer sequencer, string seqLabel, int stepIndex, bool isActive) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetActive(stepIndex, isActive));
		}
		public static AnimSequencer SetActive(this AnimSequencer sequencer, string seqLabel, string stepTag, bool isActive) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetActive(stepTag, isActive));
		}
		#endregion

		#region Sprite
		public static AnimSequencer.AnimSequence SetSprite(this AnimSequencer.AnimSequence seq, int index, Sprite sprite) {
			ApplyToStep(seq, index, s => s.setSpriteValue = sprite, "Sprite");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetSprite(this AnimSequencer.AnimSequence seq, string tag, Sprite sprite) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetSprite(index, sprite);
		}
		public static AnimSequencer SetSprite(this AnimSequencer sequencer, string seqLabel, int stepIndex, Sprite sprite) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetSprite(stepIndex, sprite));
		}
		public static AnimSequencer SetSprite(this AnimSequencer sequencer, string seqLabel, string stepTag, Sprite sprite) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetSprite(stepTag, sprite));
		}
		#endregion

		#region ImageSprite Alias
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, int index, Sprite sprite) {
			return seq.SetSprite(index, sprite);
		}
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, string tag, Sprite sprite) {
			return seq.SetSprite(tag, sprite);
		}
		public static AnimSequencer SetImageSprite(this AnimSequencer sequencer, string seqLabel, int stepIndex, Sprite sprite) {
			return sequencer.SetSprite(seqLabel, stepIndex, sprite);
		}
		public static AnimSequencer SetImageSprite(this AnimSequencer sequencer, string seqLabel, string stepTag, Sprite sprite) {
			return sequencer.SetSprite(seqLabel, stepTag, sprite);
		}
		#endregion

		#region Text
		public static AnimSequencer.AnimSequence SetText(this AnimSequencer.AnimSequence seq, int index, string text) {
			ApplyToStep(seq, index, s => s.setTextValue = text, "Text");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetText(this AnimSequencer.AnimSequence seq, string tag, string text) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetText(index, text);
		}
		public static AnimSequencer SetText(this AnimSequencer sequencer, string seqLabel, int stepIndex, string text) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetText(stepIndex, text));
		}
		public static AnimSequencer SetText(this AnimSequencer sequencer, string seqLabel, string stepTag, string text) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetText(stepTag, text));
		}
		#endregion

		#region TypeWriterText Alias
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, int index, string text) {
			return seq.SetText(index, text);
		}
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, string tag, string text) {
			return seq.SetText(tag, text);
		}
		public static AnimSequencer SetTypeWriterText(this AnimSequencer sequencer, string seqLabel, int stepIndex, string text) {
			return sequencer.SetText(seqLabel, stepIndex, text);
		}
		public static AnimSequencer SetTypeWriterText(this AnimSequencer sequencer, string seqLabel, string stepTag, string text) {
			return sequencer.SetText(seqLabel, stepTag, text);
		}
		#endregion

		#region TextCounterTarget To Only
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, int index, float to) {
			ApplyToStep(seq, index, s => {
				s.animateFromCurrent = true;
				s.textCounterTo = to;
			}, "TextCounterTarget To");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, string tag, float to) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetTextCounterTarget(index, to);
		}
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, int stepIndex, float to) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTextCounterTarget(stepIndex, to));
		}
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, string stepTag, float to) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTextCounterTarget(stepTag, to));
		}
		#endregion

		#region TextCounterTarget From To
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, int index, float from, float to) {
			ApplyToStep(seq, index, s => {
				s.animateFromCurrent = false;
				s.textCounterFrom = from;
				s.textCounterTo = to;
			}, "TextCounterTarget From To");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, string tag, float from, float to) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetTextCounterTarget(index, from, to);
		}
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, int stepIndex, float from, float to) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTextCounterTarget(stepIndex, from, to));
		}
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, string stepTag, float from, float to) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTextCounterTarget(stepTag, from, to));
		}
		#endregion

		#region WaitMethod
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, int index, WaitMethod method) {
			ApplyToStep(seq, index, s => s.waitMethod = method, "WaitMethod");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, string tag, WaitMethod method) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetWaitMethod(index, method);
		}
		public static AnimSequencer SetWaitMethod(this AnimSequencer sequencer, string seqLabel, int stepIndex, WaitMethod method) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetWaitMethod(stepIndex, method));
		}
		public static AnimSequencer SetWaitMethod(this AnimSequencer sequencer, string seqLabel, string stepTag, WaitMethod method) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetWaitMethod(stepTag, method));
		}
		#endregion

		#region Condition
		public static AnimSequencer.AnimSequence SetCondition(this AnimSequencer.AnimSequence seq, int index, System.Func<bool> condition) {
			ApplyToStep(seq, index, s => s.waitConditionLambda = condition, "Condition");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetCondition(this AnimSequencer.AnimSequence seq, string tag, System.Func<bool> condition) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetCondition(index, condition);
		}
		public static AnimSequencer SetCondition(this AnimSequencer sequencer, string seqLabel, int stepIndex, System.Func<bool> condition) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetCondition(stepIndex, condition));
		}
		public static AnimSequencer SetCondition(this AnimSequencer sequencer, string seqLabel, string stepTag, System.Func<bool> condition) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetCondition(stepTag, condition));
		}
		#endregion

		#region TimeScale
		public static AnimSequencer.AnimSequence SetTimeScale(this AnimSequencer.AnimSequence seq, int index, float timeScale) {
			ApplyToStep(seq, index, s => s.timeScaleTo = timeScale, "TimeScale");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetTimeScale(this AnimSequencer.AnimSequence seq, string tag, float timeScale) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetTimeScale(index, timeScale);
		}
		public static AnimSequencer SetTimeScale(this AnimSequencer sequencer, string seqLabel, int stepIndex, float timeScale) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTimeScale(stepIndex, timeScale));
		}
		public static AnimSequencer SetTimeScale(this AnimSequencer sequencer, string seqLabel, string stepTag, float timeScale) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTimeScale(stepTag, timeScale));
		}
		#endregion

		#region TriggerSequence
		public static AnimSequencer.AnimSequence SetTriggerSequence(this AnimSequencer.AnimSequence seq, int index, string sequenceLabel) {
			ApplyToStep(seq, index, s => s.triggerSequenceLabel = sequenceLabel, "TriggerSequence");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetTriggerSequence(this AnimSequencer.AnimSequence seq, string tag, string sequenceLabel) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetTriggerSequence(index, sequenceLabel);
		}
		public static AnimSequencer SetTriggerSequence(this AnimSequencer sequencer, string seqLabel, int stepIndex, string sequenceLabel) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTriggerSequence(stepIndex, sequenceLabel));
		}
		public static AnimSequencer SetTriggerSequence(this AnimSequencer sequencer, string seqLabel, string stepTag, string sequenceLabel) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetTriggerSequence(stepTag, sequenceLabel));
		}
		#endregion

		#region MaterialFloat
		public static AnimSequencer.AnimSequence SetMaterialFloat(this AnimSequencer.AnimSequence seq, int index, float value) {
			ApplyToStep(seq, index, s => s.materialFloatTo = value, "MaterialFloat");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetMaterialFloat(this AnimSequencer.AnimSequence seq, string tag, float value) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetMaterialFloat(index, value);
		}
		public static AnimSequencer SetMaterialFloat(this AnimSequencer sequencer, string seqLabel, int stepIndex, float value) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetMaterialFloat(stepIndex, value));
		}
		public static AnimSequencer SetMaterialFloat(this AnimSequencer sequencer, string seqLabel, string stepTag, float value) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetMaterialFloat(stepTag, value));
		}
		#endregion

		#region MaterialColor
		public static AnimSequencer.AnimSequence SetMaterialColor(this AnimSequencer.AnimSequence seq, int index, Color color) {
			ApplyToStep(seq, index, s => s.materialColorTo = color, "MaterialColor");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetMaterialColor(this AnimSequencer.AnimSequence seq, string tag, Color color) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetMaterialColor(index, color);
		}
		public static AnimSequencer SetMaterialColor(this AnimSequencer sequencer, string seqLabel, int stepIndex, Color color) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetMaterialColor(stepIndex, color));
		}
		public static AnimSequencer SetMaterialColor(this AnimSequencer sequencer, string seqLabel, string stepTag, Color color) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetMaterialColor(stepTag, color));
		}
		#endregion

		#region AudioVolume
		public static AnimSequencer.AnimSequence SetAudioVolume(this AnimSequencer.AnimSequence seq, int index, Vector2 volumeMinMax) {
			ApplyToStep(seq, index, s => s.audioVolume = volumeMinMax, "AudioVolume");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetAudioVolume(this AnimSequencer.AnimSequence seq, string tag, Vector2 volumeMinMax) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetAudioVolume(index, volumeMinMax);
		}
		public static AnimSequencer SetAudioVolume(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 volumeMinMax) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetAudioVolume(stepIndex, volumeMinMax));
		}
		public static AnimSequencer SetAudioVolume(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 volumeMinMax) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetAudioVolume(stepTag, volumeMinMax));
		}
		#endregion

		#region AudioPitch
		public static AnimSequencer.AnimSequence SetAudioPitch(this AnimSequencer.AnimSequence seq, int index, Vector2 pitchMinMax) {
			ApplyToStep(seq, index, s => s.audioPitch = pitchMinMax, "AudioPitch");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetAudioPitch(this AnimSequencer.AnimSequence seq, string tag, Vector2 pitchMinMax) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetAudioPitch(index, pitchMinMax);
		}
		public static AnimSequencer SetAudioPitch(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 pitchMinMax) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetAudioPitch(stepIndex, pitchMinMax));
		}
		public static AnimSequencer SetAudioPitch(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 pitchMinMax) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetAudioPitch(stepTag, pitchMinMax));
		}
		#endregion

		#region ShakeStrength
		public static AnimSequencer.AnimSequence SetShakeStrength(this AnimSequencer.AnimSequence seq, int index, Vector3 strength) {
			ApplyToStep(seq, index, s => s.shakeStrength = strength, "ShakeStrength");
			return seq;
		}
		public static AnimSequencer.AnimSequence SetShakeStrength(this AnimSequencer.AnimSequence seq, string tag, Vector3 strength) {
			int index = -1;
			if (seq != null) {
				index = seq.FindStepIndex(tag);
			}
			return seq.SetShakeStrength(index, strength);
		}
		public static AnimSequencer SetShakeStrength(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector3 strength) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetShakeStrength(stepIndex, strength));
		}
		public static AnimSequencer SetShakeStrength(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector3 strength) {
			return ApplyToSequence(sequencer, seqLabel, seq => seq.SetShakeStrength(stepTag, strength));
		}
		#endregion

	}
}