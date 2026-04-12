using System.Threading.Tasks;
using UnityEngine;
using PrimeTween;

namespace Sperlich.Sequencer {
	public static class AnimSequencerExt {

		#region Sequence Setup & Chaining
		public static AnimSequencer.AnimStep AppendStep<T>(this AnimSequencer.AnimSequence seq, T config) where T : AnimConfig {
			var step = new AnimSequencer.AnimStep {
				type = config.GetAnimType()
			};

			config.ApplyTo(step);
			seq.steps.Add(step);

			return step;
		}

		public static AnimSequencer.AnimSequence AttachTo(this AnimSequencer.AnimSequence seq, GameObject target) {
			if (target == null) return seq;
			if (seq.owner != null) seq.owner.sequences.Remove(seq);

			var targetSequencer = target.GetComponent<AnimSequencer>();
			if (targetSequencer == null) targetSequencer = target.AddComponent<AnimSequencer>();

			seq.owner = targetSequencer;
			seq.isTemporary = false;
			targetSequencer.sequences.Add(seq);

			return seq;
		}
		#endregion

		#region Playback & Lifecycle
		public static void Play(this AnimSequencer.AnimSequence seq) {
			if (seq.owner != null) seq.owner.PlaySequence(seq);
			else Debug.LogWarning($"[AnimSequencer] Sequence '{seq.label}' has no owner.");
		}

		public static void Pause(this AnimSequencer.AnimSequence seq) { if (seq.owner != null) seq.owner.Pause(seq.label); }
		public static void Resume(this AnimSequencer.AnimSequence seq) { if (seq.owner != null) seq.owner.Resume(seq.label); }

		public static AnimSequencer.AnimSequence OnComplete(this AnimSequencer.AnimSequence seq, System.Action action) { seq.onCompleteAction += action; return seq; }
		public static AnimSequencer.AnimSequence OnStart(this AnimSequencer.AnimSequence seq, System.Action action) { seq.onStartAction += action; return seq; }

		public static async Task PlayAsync(this AnimSequencer.AnimSequence seq) {
			var tcs = new TaskCompletionSource<bool>();
			System.Action completeAction = null;

			completeAction = () => { seq.onCompleteAction -= completeAction; tcs.TrySetResult(true); };
			seq.onCompleteAction += completeAction;
			seq.Play();

			await tcs.Task;
		}

		public static CustomYieldInstruction WaitForCompletion(this AnimSequencer.AnimSequence seq) { return new WaitWhile(() => seq.isPlaying); }

		// --- Sequencer Level Playback ---
		public static async Task PlayAsync(this AnimSequencer sequencer, string sequenceLabel) {
			var seq = sequencer.GetSequence(sequenceLabel);
			if (seq != null) await seq.PlayAsync();
		}

		public static CustomYieldInstruction WaitForCompletion(this AnimSequencer sequencer, string sequenceLabel) {
			var seq = sequencer.GetSequence(sequenceLabel);
			return seq != null ? seq.WaitForCompletion() : null;
		}
		#endregion

		#region Locators (Sequence & Step)
		public static AnimSequencer.AnimSequence GetLastSequence(this AnimSequencer sequencer) {
			if (sequencer != null && sequencer.sequences.Count > 0) return sequencer.sequences[sequencer.sequences.Count - 1];
			return null;
		}

		public static AnimSequencer.AnimStep FindStep(this AnimSequencer.AnimSequence seq, string tag) { return seq.steps.Find(s => s.tag == tag); }
		public static AnimSequencer.AnimStep FindStep(this AnimSequencer.AnimSequence seq, int index) { return (index >= 0 && index < seq.steps.Count) ? seq.steps[index] : null; }
		public static AnimSequencer.AnimStep GetLastStep(this AnimSequencer.AnimSequence seq) { return seq.steps.Count > 0 ? seq.steps[seq.steps.Count - 1] : null; }
		#endregion

		#region Fluent Mutators - Core Properties
		// --- Duration ---
		public static AnimSequencer.AnimStep SetDuration(this AnimSequencer.AnimStep s, float duration) { if (s != null) { s.duration = duration; } return s; }
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, float duration) { seq.GetLastStep()?.SetDuration(duration); return seq; }
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, string tag, float duration) { seq.FindStep(tag)?.SetDuration(duration); return seq; }
		public static AnimSequencer.AnimSequence SetDuration(this AnimSequencer.AnimSequence seq, int index, float duration) { seq.FindStep(index)?.SetDuration(duration); return seq; }
		public static AnimSequencer SetDuration(this AnimSequencer sequencer, float duration) { sequencer.GetLastSequence()?.SetDuration(duration); return sequencer; }
		public static AnimSequencer SetDuration(this AnimSequencer sequencer, string seqLabel, float duration) { sequencer.GetSequence(seqLabel)?.SetDuration(duration); return sequencer; }
		public static AnimSequencer SetDuration(this AnimSequencer sequencer, string seqLabel, string stepTag, float duration) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetDuration(duration); return sequencer; }
		public static AnimSequencer SetDuration(this AnimSequencer sequencer, string seqLabel, int stepIndex, float duration) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetDuration(duration); return sequencer; }

		// --- Delay ---
		public static AnimSequencer.AnimStep SetDelay(this AnimSequencer.AnimStep s, float delay) { if (s != null) { s.delay = delay; } return s; }
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, float delay) { seq.GetLastStep()?.SetDelay(delay); return seq; }
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, string tag, float delay) { seq.FindStep(tag)?.SetDelay(delay); return seq; }
		public static AnimSequencer.AnimSequence SetDelay(this AnimSequencer.AnimSequence seq, int index, float delay) { seq.FindStep(index)?.SetDelay(delay); return seq; }
		public static AnimSequencer SetDelay(this AnimSequencer sequencer, float delay) { sequencer.GetLastSequence()?.SetDelay(delay); return sequencer; }
		public static AnimSequencer SetDelay(this AnimSequencer sequencer, string seqLabel, float delay) { sequencer.GetSequence(seqLabel)?.SetDelay(delay); return sequencer; }
		public static AnimSequencer SetDelay(this AnimSequencer sequencer, string seqLabel, string stepTag, float delay) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetDelay(delay); return sequencer; }
		public static AnimSequencer SetDelay(this AnimSequencer sequencer, string seqLabel, int stepIndex, float delay) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetDelay(delay); return sequencer; }

		// --- Ease ---
		public static AnimSequencer.AnimStep SetEase(this AnimSequencer.AnimStep s, Ease ease) { if (s != null) { s.ease = ease; } return s; }
		public static AnimSequencer.AnimSequence SetEase(this AnimSequencer.AnimSequence seq, Ease ease) { seq.GetLastStep()?.SetEase(ease); return seq; }
		public static AnimSequencer.AnimSequence SetEase(this AnimSequencer.AnimSequence seq, string tag, Ease ease) { seq.FindStep(tag)?.SetEase(ease); return seq; }
		public static AnimSequencer.AnimSequence SetEase(this AnimSequencer.AnimSequence seq, int index, Ease ease) { seq.FindStep(index)?.SetEase(ease); return seq; }
		public static AnimSequencer SetEase(this AnimSequencer sequencer, Ease ease) { sequencer.GetLastSequence()?.SetEase(ease); return sequencer; }
		public static AnimSequencer SetEase(this AnimSequencer sequencer, string seqLabel, Ease ease) { sequencer.GetSequence(seqLabel)?.SetEase(ease); return sequencer; }
		public static AnimSequencer SetEase(this AnimSequencer sequencer, string seqLabel, string stepTag, Ease ease) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetEase(ease); return sequencer; }
		public static AnimSequencer SetEase(this AnimSequencer sequencer, string seqLabel, int stepIndex, Ease ease) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetEase(ease); return sequencer; }

		// --- Target ---
		public static AnimSequencer.AnimStep SetTarget(this AnimSequencer.AnimStep s, Transform target) { if (s != null) { s.target = target; s.isInitialized = false; } return s; }
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, Transform target) { seq.GetLastStep()?.SetTarget(target); return seq; }
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, string tag, Transform target) { seq.FindStep(tag)?.SetTarget(target); return seq; }
		public static AnimSequencer.AnimSequence SetTarget(this AnimSequencer.AnimSequence seq, int index, Transform target) { seq.FindStep(index)?.SetTarget(target); return seq; }
		public static AnimSequencer SetTarget(this AnimSequencer sequencer, Transform target) { sequencer.GetLastSequence()?.SetTarget(target); return sequencer; }
		public static AnimSequencer SetTarget(this AnimSequencer sequencer, string seqLabel, Transform target) { sequencer.GetSequence(seqLabel)?.SetTarget(target); return sequencer; }
		public static AnimSequencer SetTarget(this AnimSequencer sequencer, string seqLabel, string stepTag, Transform target) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetTarget(target); return sequencer; }
		public static AnimSequencer SetTarget(this AnimSequencer sequencer, string seqLabel, int stepIndex, Transform target) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetTarget(target); return sequencer; }
		#endregion

		#region Fluent Mutators - Transform & UI
		// --- Fade Alpha ---
		public static AnimSequencer.AnimStep SetFadeAlpha(this AnimSequencer.AnimStep s, float alpha) { if (s != null) { s.setFadeValue = alpha; } return s; }
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, float alpha) { seq.GetLastStep()?.SetFadeAlpha(alpha); return seq; }
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, string tag, float alpha) { seq.FindStep(tag)?.SetFadeAlpha(alpha); return seq; }
		public static AnimSequencer.AnimSequence SetFadeAlpha(this AnimSequencer.AnimSequence seq, int index, float alpha) { seq.FindStep(index)?.SetFadeAlpha(alpha); return seq; }
		public static AnimSequencer SetFadeAlpha(this AnimSequencer sequencer, float alpha) { sequencer.GetLastSequence()?.SetFadeAlpha(alpha); return sequencer; }
		public static AnimSequencer SetFadeAlpha(this AnimSequencer sequencer, string seqLabel, float alpha) { sequencer.GetSequence(seqLabel)?.SetFadeAlpha(alpha); return sequencer; }
		public static AnimSequencer SetFadeAlpha(this AnimSequencer sequencer, string seqLabel, string stepTag, float alpha) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetFadeAlpha(alpha); return sequencer; }
		public static AnimSequencer SetFadeAlpha(this AnimSequencer sequencer, string seqLabel, int stepIndex, float alpha) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetFadeAlpha(alpha); return sequencer; }

		// --- Scale ---
		public static AnimSequencer.AnimStep SetScaleTo(this AnimSequencer.AnimStep s, Vector3 scale) { if (s != null) { s.scaleTo = scale; s.scaleTo3D = scale; } return s; }
		public static AnimSequencer.AnimSequence SetScaleTo(this AnimSequencer.AnimSequence seq, Vector3 scale) { seq.GetLastStep()?.SetScaleTo(scale); return seq; }
		public static AnimSequencer.AnimSequence SetScaleTo(this AnimSequencer.AnimSequence seq, string tag, Vector3 scale) { seq.FindStep(tag)?.SetScaleTo(scale); return seq; }
		public static AnimSequencer.AnimSequence SetScaleTo(this AnimSequencer.AnimSequence seq, int index, Vector3 scale) { seq.FindStep(index)?.SetScaleTo(scale); return seq; }
		public static AnimSequencer SetScaleTo(this AnimSequencer sequencer, Vector3 scale) { sequencer.GetLastSequence()?.SetScaleTo(scale); return sequencer; }
		public static AnimSequencer SetScaleTo(this AnimSequencer sequencer, string seqLabel, Vector3 scale) { sequencer.GetSequence(seqLabel)?.SetScaleTo(scale); return sequencer; }
		public static AnimSequencer SetScaleTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector3 scale) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetScaleTo(scale); return sequencer; }
		public static AnimSequencer SetScaleTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector3 scale) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetScaleTo(scale); return sequencer; }

		// --- Slide ---
		public static AnimSequencer.AnimStep SetSlideTo(this AnimSequencer.AnimStep s, Vector2 slide) { if (s != null) { s.slideTo = slide; } return s; }
		public static AnimSequencer.AnimSequence SetSlideTo(this AnimSequencer.AnimSequence seq, Vector2 slide) { seq.GetLastStep()?.SetSlideTo(slide); return seq; }
		public static AnimSequencer.AnimSequence SetSlideTo(this AnimSequencer.AnimSequence seq, string tag, Vector2 slide) { seq.FindStep(tag)?.SetSlideTo(slide); return seq; }
		public static AnimSequencer.AnimSequence SetSlideTo(this AnimSequencer.AnimSequence seq, int index, Vector2 slide) { seq.FindStep(index)?.SetSlideTo(slide); return seq; }
		public static AnimSequencer SetSlideTo(this AnimSequencer sequencer, Vector2 slide) { sequencer.GetLastSequence()?.SetSlideTo(slide); return sequencer; }
		public static AnimSequencer SetSlideTo(this AnimSequencer sequencer, string seqLabel, Vector2 slide) { sequencer.GetSequence(seqLabel)?.SetSlideTo(slide); return sequencer; }
		public static AnimSequencer SetSlideTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 slide) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetSlideTo(slide); return sequencer; }
		public static AnimSequencer SetSlideTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 slide) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetSlideTo(slide); return sequencer; }

		// --- Rotate ---
		public static AnimSequencer.AnimStep SetRotateTo(this AnimSequencer.AnimStep s, float rotation) { if (s != null) { s.rotateTo = rotation; } return s; }
		public static AnimSequencer.AnimSequence SetRotateTo(this AnimSequencer.AnimSequence seq, float rotation) { seq.GetLastStep()?.SetRotateTo(rotation); return seq; }
		public static AnimSequencer.AnimSequence SetRotateTo(this AnimSequencer.AnimSequence seq, string tag, float rotation) { seq.FindStep(tag)?.SetRotateTo(rotation); return seq; }
		public static AnimSequencer.AnimSequence SetRotateTo(this AnimSequencer.AnimSequence seq, int index, float rotation) { seq.FindStep(index)?.SetRotateTo(rotation); return seq; }
		public static AnimSequencer SetRotateTo(this AnimSequencer sequencer, float rotation) { sequencer.GetLastSequence()?.SetRotateTo(rotation); return sequencer; }
		public static AnimSequencer SetRotateTo(this AnimSequencer sequencer, string seqLabel, float rotation) { sequencer.GetSequence(seqLabel)?.SetRotateTo(rotation); return sequencer; }
		public static AnimSequencer SetRotateTo(this AnimSequencer sequencer, string seqLabel, string stepTag, float rotation) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetRotateTo(rotation); return sequencer; }
		public static AnimSequencer SetRotateTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, float rotation) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetRotateTo(rotation); return sequencer; }

		// --- ColorTo (General) ---
		public static AnimSequencer.AnimStep SetColorTo(this AnimSequencer.AnimStep s, Color color) { if (s != null) { s.colorTo = color; } return s; }
		public static AnimSequencer.AnimSequence SetColorTo(this AnimSequencer.AnimSequence seq, Color color) { seq.GetLastStep()?.SetColorTo(color); return seq; }
		public static AnimSequencer.AnimSequence SetColorTo(this AnimSequencer.AnimSequence seq, string tag, Color color) { seq.FindStep(tag)?.SetColorTo(color); return seq; }
		public static AnimSequencer.AnimSequence SetColorTo(this AnimSequencer.AnimSequence seq, int index, Color color) { seq.FindStep(index)?.SetColorTo(color); return seq; }
		public static AnimSequencer SetColorTo(this AnimSequencer sequencer, Color color) { sequencer.GetLastSequence()?.SetColorTo(color); return sequencer; }
		public static AnimSequencer SetColorTo(this AnimSequencer sequencer, string seqLabel, Color color) { sequencer.GetSequence(seqLabel)?.SetColorTo(color); return sequencer; }
		public static AnimSequencer SetColorTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Color color) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetColorTo(color); return sequencer; }
		public static AnimSequencer SetColorTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Color color) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetColorTo(color); return sequencer; }

		// --- SetColorColor (Specific) ---
		public static AnimSequencer.AnimStep SetColorColor(this AnimSequencer.AnimStep s, Color color) { if (s != null) { s.colorTo = color; } return s; }
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, Color color) { seq.GetLastStep()?.SetColorColor(color); return seq; }
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, string tag, Color color) { seq.FindStep(tag)?.SetColorColor(color); return seq; }
		public static AnimSequencer.AnimSequence SetColorColor(this AnimSequencer.AnimSequence seq, int index, Color color) { seq.FindStep(index)?.SetColorColor(color); return seq; }
		public static AnimSequencer SetColorColor(this AnimSequencer sequencer, Color color) { sequencer.GetLastSequence()?.SetColorColor(color); return sequencer; }
		public static AnimSequencer SetColorColor(this AnimSequencer sequencer, string seqLabel, Color color) { sequencer.GetSequence(seqLabel)?.SetColorColor(color); return sequencer; }
		public static AnimSequencer SetColorColor(this AnimSequencer sequencer, string seqLabel, string stepTag, Color color) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetColorColor(color); return sequencer; }
		public static AnimSequencer SetColorColor(this AnimSequencer sequencer, string seqLabel, int stepIndex, Color color) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetColorColor(color); return sequencer; }

		// --- SizeDelta ---
		public static AnimSequencer.AnimStep SetSizeDeltaTo(this AnimSequencer.AnimStep s, Vector2 size) { if (s != null) { s.sizeDeltaTo = size; } return s; }
		public static AnimSequencer.AnimSequence SetSizeDeltaTo(this AnimSequencer.AnimSequence seq, Vector2 size) { seq.GetLastStep()?.SetSizeDeltaTo(size); return seq; }
		public static AnimSequencer.AnimSequence SetSizeDeltaTo(this AnimSequencer.AnimSequence seq, string tag, Vector2 size) { seq.FindStep(tag)?.SetSizeDeltaTo(size); return seq; }
		public static AnimSequencer.AnimSequence SetSizeDeltaTo(this AnimSequencer.AnimSequence seq, int index, Vector2 size) { seq.FindStep(index)?.SetSizeDeltaTo(size); return seq; }
		public static AnimSequencer SetSizeDeltaTo(this AnimSequencer sequencer, Vector2 size) { sequencer.GetLastSequence()?.SetSizeDeltaTo(size); return sequencer; }
		public static AnimSequencer SetSizeDeltaTo(this AnimSequencer sequencer, string seqLabel, Vector2 size) { sequencer.GetSequence(seqLabel)?.SetSizeDeltaTo(size); return sequencer; }
		public static AnimSequencer SetSizeDeltaTo(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 size) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetSizeDeltaTo(size); return sequencer; }
		public static AnimSequencer SetSizeDeltaTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 size) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetSizeDeltaTo(size); return sequencer; }

		// --- FillAmount ---
		public static AnimSequencer.AnimStep SetFillAmountTo(this AnimSequencer.AnimStep s, float amount) { if (s != null) { s.fillAmountTo = amount; } return s; }
		public static AnimSequencer.AnimSequence SetFillAmountTo(this AnimSequencer.AnimSequence seq, float amount) { seq.GetLastStep()?.SetFillAmountTo(amount); return seq; }
		public static AnimSequencer.AnimSequence SetFillAmountTo(this AnimSequencer.AnimSequence seq, string tag, float amount) { seq.FindStep(tag)?.SetFillAmountTo(amount); return seq; }
		public static AnimSequencer.AnimSequence SetFillAmountTo(this AnimSequencer.AnimSequence seq, int index, float amount) { seq.FindStep(index)?.SetFillAmountTo(amount); return seq; }
		public static AnimSequencer SetFillAmountTo(this AnimSequencer sequencer, float amount) { sequencer.GetLastSequence()?.SetFillAmountTo(amount); return sequencer; }
		public static AnimSequencer SetFillAmountTo(this AnimSequencer sequencer, string seqLabel, float amount) { sequencer.GetSequence(seqLabel)?.SetFillAmountTo(amount); return sequencer; }
		public static AnimSequencer SetFillAmountTo(this AnimSequencer sequencer, string seqLabel, string stepTag, float amount) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetFillAmountTo(amount); return sequencer; }
		public static AnimSequencer SetFillAmountTo(this AnimSequencer sequencer, string seqLabel, int stepIndex, float amount) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetFillAmountTo(amount); return sequencer; }
		#endregion

		#region Fluent Mutators - Instant Data & Text Setters
		// --- SetText (General) ---
		public static AnimSequencer.AnimStep SetText(this AnimSequencer.AnimStep s, string text) { if (s != null) { s.setTextValue = text; } return s; }
		public static AnimSequencer.AnimSequence SetText(this AnimSequencer.AnimSequence seq, string text) { seq.GetLastStep()?.SetText(text); return seq; }
		public static AnimSequencer.AnimSequence SetText(this AnimSequencer.AnimSequence seq, string tag, string text) { seq.FindStep(tag)?.SetText(text); return seq; }
		public static AnimSequencer.AnimSequence SetText(this AnimSequencer.AnimSequence seq, int index, string text) { seq.FindStep(index)?.SetText(text); return seq; }
		public static AnimSequencer SetText(this AnimSequencer sequencer, string text) { sequencer.GetLastSequence()?.SetText(text); return sequencer; }
		public static AnimSequencer SetText(this AnimSequencer sequencer, string seqLabel, string text) { sequencer.GetSequence(seqLabel)?.SetText(text); return sequencer; }
		public static AnimSequencer SetText(this AnimSequencer sequencer, string seqLabel, string stepTag, string text) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetText(text); return sequencer; }
		public static AnimSequencer SetText(this AnimSequencer sequencer, string seqLabel, int stepIndex, string text) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetText(text); return sequencer; }

		// --- SetTypeWriterText (Specific) ---
		public static AnimSequencer.AnimStep SetTypeWriterText(this AnimSequencer.AnimStep s, string text) { if (s != null) { s.setTextValue = text; } return s; }
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, string text) { seq.GetLastStep()?.SetTypeWriterText(text); return seq; }
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, string tag, string text) { seq.FindStep(tag)?.SetTypeWriterText(text); return seq; }
		public static AnimSequencer.AnimSequence SetTypeWriterText(this AnimSequencer.AnimSequence seq, int index, string text) { seq.FindStep(index)?.SetTypeWriterText(text); return seq; }
		public static AnimSequencer SetTypeWriterText(this AnimSequencer sequencer, string text) { sequencer.GetLastSequence()?.SetTypeWriterText(text); return sequencer; }
		public static AnimSequencer SetTypeWriterText(this AnimSequencer sequencer, string seqLabel, string text) { sequencer.GetSequence(seqLabel)?.SetTypeWriterText(text); return sequencer; }
		public static AnimSequencer SetTypeWriterText(this AnimSequencer sequencer, string seqLabel, string stepTag, string text) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetTypeWriterText(text); return sequencer; }
		public static AnimSequencer SetTypeWriterText(this AnimSequencer sequencer, string seqLabel, int stepIndex, string text) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetTypeWriterText(text); return sequencer; }

		// --- SetTextCounterTarget (To Only) ---
		public static AnimSequencer.AnimStep SetTextCounterTarget(this AnimSequencer.AnimStep s, float target) { if (s != null) { s.textCounterTo = target; } return s; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, float target) { seq.GetLastStep()?.SetTextCounterTarget(target); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, string tag, float target) { seq.FindStep(tag)?.SetTextCounterTarget(target); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, int index, float target) { seq.FindStep(index)?.SetTextCounterTarget(target); return seq; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, float target) { sequencer.GetLastSequence()?.SetTextCounterTarget(target); return sequencer; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, float target) { sequencer.GetSequence(seqLabel)?.SetTextCounterTarget(target); return sequencer; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, string stepTag, float target) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetTextCounterTarget(target); return sequencer; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, int stepIndex, float target) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetTextCounterTarget(target); return sequencer; }

		// --- SetTextCounterTarget (From & To) ---
		public static AnimSequencer.AnimStep SetTextCounterTarget(this AnimSequencer.AnimStep s, float from, float to) { if (s != null) { s.textCounterFrom = from; s.textCounterTo = to; s.animateFromCurrent = false; } return s; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, float from, float to) { seq.GetLastStep()?.SetTextCounterTarget(from, to); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, string tag, float from, float to) { seq.FindStep(tag)?.SetTextCounterTarget(from, to); return seq; }
		public static AnimSequencer.AnimSequence SetTextCounterTarget(this AnimSequencer.AnimSequence seq, int index, float from, float to) { seq.FindStep(index)?.SetTextCounterTarget(from, to); return seq; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, float from, float to) { sequencer.GetLastSequence()?.SetTextCounterTarget(from, to); return sequencer; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, float from, float to) { sequencer.GetSequence(seqLabel)?.SetTextCounterTarget(from, to); return sequencer; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, string stepTag, float from, float to) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetTextCounterTarget(from, to); return sequencer; }
		public static AnimSequencer SetTextCounterTarget(this AnimSequencer sequencer, string seqLabel, int stepIndex, float from, float to) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetTextCounterTarget(from, to); return sequencer; }

		// --- SetActive ---
		public static AnimSequencer.AnimStep SetActive(this AnimSequencer.AnimStep s, bool isActive) { if (s != null) { s.setActiveValue = isActive; } return s; }
		public static AnimSequencer.AnimSequence SetActive(this AnimSequencer.AnimSequence seq, bool isActive) { seq.GetLastStep()?.SetActive(isActive); return seq; }
		public static AnimSequencer.AnimSequence SetActive(this AnimSequencer.AnimSequence seq, string tag, bool isActive) { seq.FindStep(tag)?.SetActive(isActive); return seq; }
		public static AnimSequencer.AnimSequence SetActive(this AnimSequencer.AnimSequence seq, int index, bool isActive) { seq.FindStep(index)?.SetActive(isActive); return seq; }
		public static AnimSequencer SetActive(this AnimSequencer sequencer, bool isActive) { sequencer.GetLastSequence()?.SetActive(isActive); return sequencer; }
		public static AnimSequencer SetActive(this AnimSequencer sequencer, string seqLabel, bool isActive) { sequencer.GetSequence(seqLabel)?.SetActive(isActive); return sequencer; }
		public static AnimSequencer SetActive(this AnimSequencer sequencer, string seqLabel, string stepTag, bool isActive) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetActive(isActive); return sequencer; }
		public static AnimSequencer SetActive(this AnimSequencer sequencer, string seqLabel, int stepIndex, bool isActive) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetActive(isActive); return sequencer; }

		// --- SetSprite (General) ---
		public static AnimSequencer.AnimStep SetSprite(this AnimSequencer.AnimStep s, Sprite sprite) { if (s != null) { s.setSpriteValue = sprite; } return s; }
		public static AnimSequencer.AnimSequence SetSprite(this AnimSequencer.AnimSequence seq, Sprite sprite) { seq.GetLastStep()?.SetSprite(sprite); return seq; }
		public static AnimSequencer.AnimSequence SetSprite(this AnimSequencer.AnimSequence seq, string tag, Sprite sprite) { seq.FindStep(tag)?.SetSprite(sprite); return seq; }
		public static AnimSequencer.AnimSequence SetSprite(this AnimSequencer.AnimSequence seq, int index, Sprite sprite) { seq.FindStep(index)?.SetSprite(sprite); return seq; }
		public static AnimSequencer SetSprite(this AnimSequencer sequencer, Sprite sprite) { sequencer.GetLastSequence()?.SetSprite(sprite); return sequencer; }
		public static AnimSequencer SetSprite(this AnimSequencer sequencer, string seqLabel, Sprite sprite) { sequencer.GetSequence(seqLabel)?.SetSprite(sprite); return sequencer; }
		public static AnimSequencer SetSprite(this AnimSequencer sequencer, string seqLabel, string stepTag, Sprite sprite) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetSprite(sprite); return sequencer; }
		public static AnimSequencer SetSprite(this AnimSequencer sequencer, string seqLabel, int stepIndex, Sprite sprite) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetSprite(sprite); return sequencer; }

		// --- SetImageSprite (Specific) ---
		public static AnimSequencer.AnimStep SetImageSprite(this AnimSequencer.AnimStep s, Sprite sprite) { if (s != null) { s.setSpriteValue = sprite; } return s; }
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, Sprite sprite) { seq.GetLastStep()?.SetImageSprite(sprite); return seq; }
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, string tag, Sprite sprite) { seq.FindStep(tag)?.SetImageSprite(sprite); return seq; }
		public static AnimSequencer.AnimSequence SetImageSprite(this AnimSequencer.AnimSequence seq, int index, Sprite sprite) { seq.FindStep(index)?.SetImageSprite(sprite); return seq; }
		public static AnimSequencer SetImageSprite(this AnimSequencer sequencer, Sprite sprite) { sequencer.GetLastSequence()?.SetImageSprite(sprite); return sequencer; }
		public static AnimSequencer SetImageSprite(this AnimSequencer sequencer, string seqLabel, Sprite sprite) { sequencer.GetSequence(seqLabel)?.SetImageSprite(sprite); return sequencer; }
		public static AnimSequencer SetImageSprite(this AnimSequencer sequencer, string seqLabel, string stepTag, Sprite sprite) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetImageSprite(sprite); return sequencer; }
		public static AnimSequencer SetImageSprite(this AnimSequencer sequencer, string seqLabel, int stepIndex, Sprite sprite) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetImageSprite(sprite); return sequencer; }
		#endregion

		#region Fluent Mutators - Specific Features
		// --- Wait Method ---
		public static AnimSequencer.AnimStep SetWaitMethod(this AnimSequencer.AnimStep s, WaitMethod method) { if (s != null) { s.waitMethod = method; } return s; }
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, WaitMethod method) { seq.GetLastStep()?.SetWaitMethod(method); return seq; }
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, string tag, WaitMethod method) { seq.FindStep(tag)?.SetWaitMethod(method); return seq; }
		public static AnimSequencer.AnimSequence SetWaitMethod(this AnimSequencer.AnimSequence seq, int index, WaitMethod method) { seq.FindStep(index)?.SetWaitMethod(method); return seq; }
		public static AnimSequencer SetWaitMethod(this AnimSequencer sequencer, WaitMethod method) { sequencer.GetLastSequence()?.SetWaitMethod(method); return sequencer; }
		public static AnimSequencer SetWaitMethod(this AnimSequencer sequencer, string seqLabel, WaitMethod method) { sequencer.GetSequence(seqLabel)?.SetWaitMethod(method); return sequencer; }
		public static AnimSequencer SetWaitMethod(this AnimSequencer sequencer, string seqLabel, string stepTag, WaitMethod method) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetWaitMethod(method); return sequencer; }
		public static AnimSequencer SetWaitMethod(this AnimSequencer sequencer, string seqLabel, int stepIndex, WaitMethod method) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetWaitMethod(method); return sequencer; }

		// --- WaitUntil Condition ---
		public static AnimSequencer.AnimStep SetCondition(this AnimSequencer.AnimStep s, System.Func<bool> condition) { if (s != null) { s.waitConditionLambda = condition; } return s; }
		public static AnimSequencer.AnimSequence SetCondition(this AnimSequencer.AnimSequence seq, System.Func<bool> condition) { seq.GetLastStep()?.SetCondition(condition); return seq; }
		public static AnimSequencer SetCondition(this AnimSequencer sequencer, System.Func<bool> condition) { sequencer.GetLastSequence()?.SetCondition(condition); return sequencer; }
		public static AnimSequencer SetCondition(this AnimSequencer sequencer, string seqLabel, System.Func<bool> condition) { sequencer.GetSequence(seqLabel)?.SetCondition(condition); return sequencer; }
		public static AnimSequencer SetCondition(this AnimSequencer sequencer, string seqLabel, string stepTag, System.Func<bool> condition) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetCondition(condition); return sequencer; }
		public static AnimSequencer SetCondition(this AnimSequencer sequencer, string seqLabel, int stepIndex, System.Func<bool> condition) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetCondition(condition); return sequencer; }

		// --- TimeScale ---
		public static AnimSequencer.AnimStep SetTimeScale(this AnimSequencer.AnimStep s, float timeScale) { if (s != null) { s.timeScaleTo = timeScale; } return s; }
		public static AnimSequencer.AnimSequence SetTimeScale(this AnimSequencer.AnimSequence seq, float timeScale) { seq.GetLastStep()?.SetTimeScale(timeScale); return seq; }
		public static AnimSequencer SetTimeScale(this AnimSequencer sequencer, float timeScale) { sequencer.GetLastSequence()?.SetTimeScale(timeScale); return sequencer; }
		public static AnimSequencer SetTimeScale(this AnimSequencer sequencer, string seqLabel, float timeScale) { sequencer.GetSequence(seqLabel)?.SetTimeScale(timeScale); return sequencer; }
		public static AnimSequencer SetTimeScale(this AnimSequencer sequencer, string seqLabel, string stepTag, float timeScale) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetTimeScale(timeScale); return sequencer; }
		public static AnimSequencer SetTimeScale(this AnimSequencer sequencer, string seqLabel, int stepIndex, float timeScale) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetTimeScale(timeScale); return sequencer; }

		// --- Trigger Sequence ---
		public static AnimSequencer.AnimStep SetTriggerSequence(this AnimSequencer.AnimStep s, string sequenceLabel) { if (s != null) { s.triggerSequenceLabel = sequenceLabel; } return s; }
		public static AnimSequencer.AnimSequence SetTriggerSequence(this AnimSequencer.AnimSequence seq, string sequenceLabel) { seq.GetLastStep()?.SetTriggerSequence(sequenceLabel); return seq; }
		public static AnimSequencer SetTriggerSequence(this AnimSequencer sequencer, string sequenceLabel) { sequencer.GetLastSequence()?.SetTriggerSequence(sequenceLabel); return sequencer; }
		public static AnimSequencer SetTriggerSequence(this AnimSequencer sequencer, string seqLabel, string sequenceLabel) { sequencer.GetSequence(seqLabel)?.SetTriggerSequence(sequenceLabel); return sequencer; }
		public static AnimSequencer SetTriggerSequence(this AnimSequencer sequencer, string seqLabel, string stepTag, string sequenceLabel) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetTriggerSequence(sequenceLabel); return sequencer; }
		public static AnimSequencer SetTriggerSequence(this AnimSequencer sequencer, string seqLabel, int stepIndex, string sequenceLabel) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetTriggerSequence(sequenceLabel); return sequencer; }

		// --- Material Float ---
		public static AnimSequencer.AnimStep SetMaterialFloat(this AnimSequencer.AnimStep s, float value) { if (s != null) { s.materialFloatTo = value; } return s; }
		public static AnimSequencer.AnimSequence SetMaterialFloat(this AnimSequencer.AnimSequence seq, float value) { seq.GetLastStep()?.SetMaterialFloat(value); return seq; }
		public static AnimSequencer SetMaterialFloat(this AnimSequencer sequencer, float value) { sequencer.GetLastSequence()?.SetMaterialFloat(value); return sequencer; }
		public static AnimSequencer SetMaterialFloat(this AnimSequencer sequencer, string seqLabel, float value) { sequencer.GetSequence(seqLabel)?.SetMaterialFloat(value); return sequencer; }
		public static AnimSequencer SetMaterialFloat(this AnimSequencer sequencer, string seqLabel, string stepTag, float value) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetMaterialFloat(value); return sequencer; }
		public static AnimSequencer SetMaterialFloat(this AnimSequencer sequencer, string seqLabel, int stepIndex, float value) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetMaterialFloat(value); return sequencer; }

		// --- Material Color ---
		public static AnimSequencer.AnimStep SetMaterialColor(this AnimSequencer.AnimStep s, Color color) { if (s != null) { s.materialColorTo = color; } return s; }
		public static AnimSequencer.AnimSequence SetMaterialColor(this AnimSequencer.AnimSequence seq, Color color) { seq.GetLastStep()?.SetMaterialColor(color); return seq; }
		public static AnimSequencer SetMaterialColor(this AnimSequencer sequencer, Color color) { sequencer.GetLastSequence()?.SetMaterialColor(color); return sequencer; }
		public static AnimSequencer SetMaterialColor(this AnimSequencer sequencer, string seqLabel, Color color) { sequencer.GetSequence(seqLabel)?.SetMaterialColor(color); return sequencer; }
		public static AnimSequencer SetMaterialColor(this AnimSequencer sequencer, string seqLabel, string stepTag, Color color) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetMaterialColor(color); return sequencer; }
		public static AnimSequencer SetMaterialColor(this AnimSequencer sequencer, string seqLabel, int stepIndex, Color color) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetMaterialColor(color); return sequencer; }

		// --- Audio Volume ---
		public static AnimSequencer.AnimStep SetAudioVolume(this AnimSequencer.AnimStep s, Vector2 volumeMinMax) { if (s != null) { s.audioVolume = volumeMinMax; } return s; }
		public static AnimSequencer.AnimSequence SetAudioVolume(this AnimSequencer.AnimSequence seq, Vector2 volumeMinMax) { seq.GetLastStep()?.SetAudioVolume(volumeMinMax); return seq; }
		public static AnimSequencer SetAudioVolume(this AnimSequencer sequencer, Vector2 volumeMinMax) { sequencer.GetLastSequence()?.SetAudioVolume(volumeMinMax); return sequencer; }
		public static AnimSequencer SetAudioVolume(this AnimSequencer sequencer, string seqLabel, Vector2 volumeMinMax) { sequencer.GetSequence(seqLabel)?.SetAudioVolume(volumeMinMax); return sequencer; }
		public static AnimSequencer SetAudioVolume(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 volumeMinMax) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetAudioVolume(volumeMinMax); return sequencer; }
		public static AnimSequencer SetAudioVolume(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 volumeMinMax) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetAudioVolume(volumeMinMax); return sequencer; }

		// --- Audio Pitch ---
		public static AnimSequencer.AnimStep SetAudioPitch(this AnimSequencer.AnimStep s, Vector2 pitchMinMax) { if (s != null) { s.audioPitch = pitchMinMax; } return s; }
		public static AnimSequencer.AnimSequence SetAudioPitch(this AnimSequencer.AnimSequence seq, Vector2 pitchMinMax) { seq.GetLastStep()?.SetAudioPitch(pitchMinMax); return seq; }
		public static AnimSequencer SetAudioPitch(this AnimSequencer sequencer, Vector2 pitchMinMax) { sequencer.GetLastSequence()?.SetAudioPitch(pitchMinMax); return sequencer; }
		public static AnimSequencer SetAudioPitch(this AnimSequencer sequencer, string seqLabel, Vector2 pitchMinMax) { sequencer.GetSequence(seqLabel)?.SetAudioPitch(pitchMinMax); return sequencer; }
		public static AnimSequencer SetAudioPitch(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector2 pitchMinMax) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetAudioPitch(pitchMinMax); return sequencer; }
		public static AnimSequencer SetAudioPitch(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector2 pitchMinMax) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetAudioPitch(pitchMinMax); return sequencer; }

		// --- Shake Strength ---
		public static AnimSequencer.AnimStep SetShakeStrength(this AnimSequencer.AnimStep s, Vector3 strength) { if (s != null) { s.shakeStrength = strength; } return s; }
		public static AnimSequencer.AnimSequence SetShakeStrength(this AnimSequencer.AnimSequence seq, Vector3 strength) { seq.GetLastStep()?.SetShakeStrength(strength); return seq; }
		public static AnimSequencer SetShakeStrength(this AnimSequencer sequencer, Vector3 strength) { sequencer.GetLastSequence()?.SetShakeStrength(strength); return sequencer; }
		public static AnimSequencer SetShakeStrength(this AnimSequencer sequencer, string seqLabel, Vector3 strength) { sequencer.GetSequence(seqLabel)?.SetShakeStrength(strength); return sequencer; }
		public static AnimSequencer SetShakeStrength(this AnimSequencer sequencer, string seqLabel, string stepTag, Vector3 strength) { sequencer.GetSequence(seqLabel)?.FindStep(stepTag)?.SetShakeStrength(strength); return sequencer; }
		public static AnimSequencer SetShakeStrength(this AnimSequencer sequencer, string seqLabel, int stepIndex, Vector3 strength) { sequencer.GetSequence(seqLabel)?.FindStep(stepIndex)?.SetShakeStrength(strength); return sequencer; }
		#endregion
	}
}