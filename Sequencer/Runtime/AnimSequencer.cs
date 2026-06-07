using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using System.Linq;

namespace Sperlich.Sequencer {
	public partial class AnimSequencer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler {

		public List<AnimSequence> sequences = new();

		bool _isPlayingDisableSequence;
		bool _disabled;
		bool _pendingOnEnable;
		bool _internalDisable;

		readonly Dictionary<string, List<Sequence>> _activeSequences = new();
		readonly Dictionary<AnimSequence, bool> _lastInteractableState = new();
		readonly List<WaitState> _pollingWaits = new();

		[HideInInspector] public bool editorIsPlaying;
		[HideInInspector] public int editorPlayingSeqIndex = -1;
		[HideInInspector] public float[] editorStepProgress = new float[0];

		void Awake() {
			foreach (var seq in sequences) {
				seq.owner = this;
			}
		}
		void OnEnable() {
			_pendingOnEnable = true;

			foreach (var seq in sequences) {
				if (seq.trigger != TriggerType.OnEnable) {
					continue;
				}
				if (seq.steps == null || seq.steps.Count == 0) {
					continue;
				}
				SnapStepsToStart(seq);
			}
		}
		void OnDisable() {
			if (_isPlayingDisableSequence || _internalDisable) {
				return;
			}

			// ==========================================
			// STANDALONE FIX: Erkennen von Selection-Loss
			// ==========================================
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject) {
				// Sequencer erkennt selbst, dass er deaktiviert wurde, während er selektiert war.
				// Wir räumen das EventSystem auf, damit die Selektion global nicht "stecken" bleibt.
				EventSystem.current.SetSelectedGameObject(null);
			}

			// Stoppe alle laufenden Tweens
			foreach (var kvp in _activeSequences.ToList()) {
				foreach (var s in kvp.Value.ToList()) {
					if (s.isAlive) {
						s.Stop();
					}
				}
			}

			// Setze alle Visuals hart auf ihren Ursprung zurück
			RestoreInitialState();

			_activeSequences.Clear();
			_pollingWaits.Clear();
			editorPlayingSeqIndex = -1;

			if (!sequences.Any(s => s.trigger == TriggerType.OnDisable)) {
				return;
			}

			gameObject.SetActive(true);
			_isPlayingDisableSequence = true;
			PlayDisableSequences();
		}
		void OnDestroy() {
			foreach (var kvp in _activeSequences) {
				foreach (var s in kvp.Value.ToList()) {
					if (s.isAlive) {
						s.Stop();
					}
				}
			}

			_activeSequences.Clear();
			_pollingWaits.Clear();
		}
		void Update() {
			if (_pendingOnEnable) {
				_pendingOnEnable = false;
				Play(TriggerType.OnEnable);
			}
			for (int i = _pollingWaits.Count - 1; i >= 0; i--) {
				var state = _pollingWaits[i];

				if (state.seq.IsPaused) {
					continue;
				}

				bool conditionMet = false;

				if (!state.step.enabled) {
					conditionMet = true;
				} else if (state.step.type == AnimType.WaitUntil) {
					if (state.step.waitUntilValue || (state.step.waitConditionLambda != null && state.step.waitConditionLambda.Invoke())) {
						conditionMet = true;
					}
				} else if (state.step.type == AnimType.Wait && state.step.waitMethod == WaitMethod.Frames) {
					state.frameWaitCounter--;
					_pollingWaits[i] = state;

					if (state.frameWaitCounter <= 0) {
						conditionMet = true;
					}
				}

				if (conditionMet) {
					_pollingWaits.RemoveAt(i);
					PlaySequenceSlice(state.seq, state.seqIndex, state.nextIndex, state.isDisable);
				}
			}

			foreach (var seq in sequences) {
				if (seq.trigger != TriggerType.OnBecameInteractable && seq.trigger != TriggerType.OnBecameNonInteractable) {
					continue;
				}

				Selectable selectable = seq.selectableTarget != null ? seq.selectableTarget : GetComponent<Selectable>();

				if (selectable == null) {
					continue;
				}

				bool current = selectable.interactable;

				if (!_lastInteractableState.TryGetValue(seq, out bool last)) {
					_lastInteractableState[seq] = current;
					continue;
				}

				if (current == last) {
					continue;
				}

				_lastInteractableState[seq] = current;

				if (current && seq.trigger == TriggerType.OnBecameInteractable) {
					PlaySequence(seq);
				} else if (!current && seq.trigger == TriggerType.OnBecameNonInteractable) {
					PlaySequence(seq);
				}
			}

			for (int i = sequences.Count - 1; i >= 0; i--) {
				var seq = sequences[i];

				if (!seq.IsPlaying) {
					continue;
				}

				if (seq.activeTweens.Count == 0) {
					continue;
				}

				bool allDead = true;

				foreach (var tween in seq.activeTweens) {
					if (tween.isAlive) {
						allDead = false;
						break;
					}
				}

				if (allDead) {
					seq.activeTweens.Clear();
					_pollingWaits.RemoveAll(w => w.seq == seq);
					seq.SetPlaying(false);
					seq.SetPaused(false);

					if (editorPlayingSeqIndex == i) {
						editorPlayingSeqIndex = -1;
					}
				}
			}
		}

		public void OnPointerEnter(PointerEventData e) { if (!_disabled) Play(TriggerType.OnPointerEnter); }
		public void OnPointerExit(PointerEventData e) { if (!_disabled) Play(TriggerType.OnPointerExit); }
		public void OnPointerDown(PointerEventData e) { if (!_disabled) Play(TriggerType.OnPointerDown); }
		public void OnPointerUp(PointerEventData e) { if (!_disabled) Play(TriggerType.OnPointerUp); }
		public void OnPointerClick(PointerEventData e) { if (!_disabled) Play(TriggerType.OnClick); }
		public void OnSelect(BaseEventData e) { if (!_disabled) Play(TriggerType.OnSelect); }
		public void OnDeselect(BaseEventData e) { if (!_disabled) Play(TriggerType.OnDeselect); }

		public void SetEnabled(bool value) {
			_disabled = !value;
		}

		public void Play(TriggerType trigger) {
			if (trigger == TriggerType.OnEnable) {
				_pendingOnEnable = false;
			}
			if (_disabled) {
				return;
			}

			foreach (var seq in sequences) {
				if (seq.trigger == trigger) {
					PlaySequence(seq);
				}
			}
		}
		public void PlayByLabel(string sequenceLabel) {
			if (!gameObject.activeInHierarchy) {
				Debug.LogWarning($"[AnimSequencer] Cannot play sequence '{sequenceLabel}' because the GameObject '{gameObject.name}' is disabled. Please enable it first.");
				return;
			}

			foreach (var seq in sequences) {
				if (seq.label == sequenceLabel) {
					PlaySequence(seq);
					return;
				}
			}
		}

		public void Pause(string sequenceLabel) {
			var seq = sequences.Find(s => s.label == sequenceLabel);
			if (seq != null) {
				SetPausedInternal(seq, true);
			}
		}
		public void Resume(string sequenceLabel) {
			var seq = sequences.Find(s => s.label == sequenceLabel);
			if (seq != null) {
				SetPausedInternal(seq, false);
			}
		}

		#region Sequence Control API
		public void StopByLabel(string label) {
			var seq = sequences.Find(s => s.label == label);
			if (seq != null) {
				StopSequenceInternal(seq);
			}
		}
		public void StopAll() {
			var all = new List<AnimSequence>(sequences);
			foreach (var seq in all) {
				StopSequenceInternal(seq);
			}
		}
		public void CompleteByLabel(string label) {
			var seq = sequences.Find(s => s.label == label);
			if (seq != null) {
				CompleteSequenceInternal(seq);
			}
		}
		public void CompleteAll() {
			var all = new List<AnimSequence>(sequences);
			foreach (var seq in all) {
				CompleteSequenceInternal(seq);
			}
		}
		public void PauseByLabel(string label) {
			var seq = sequences.Find(s => s.label == label);
			if (seq != null) {
				SetPausedInternal(seq, true);
			}
		}
		public void PauseAll() {
			foreach (var seq in sequences) {
				SetPausedInternal(seq, true);
			}
		}
		public void ResumeByLabel(string label) {
			var seq = sequences.Find(s => s.label == label);
			if (seq != null) {
				SetPausedInternal(seq, false);
			}
		}
		public void ResumeAll() {
			foreach (var seq in sequences) {
				SetPausedInternal(seq, false);
			}
		}

		void StopSequenceInternal(AnimSequence seq) {
			if (seq == null) {
				return;
			}

			foreach (var tween in seq.activeTweens) {
				if (tween.isAlive) {
					tween.Stop();
				}
			}

			seq.activeTweens.Clear();
			_pollingWaits.RemoveAll(w => w.seq == seq);
			seq.SetPlaying(false);

			if (editorPlayingSeqIndex == sequences.IndexOf(seq)) {
				editorPlayingSeqIndex = -1;
			}
		}
		void CompleteSequenceInternal(AnimSequence seq) {
			if (seq == null) return;

			foreach (var tween in seq.activeTweens.ToList()) {
				if (tween.isAlive) {
					tween.Complete();
				}
			}
			seq.activeTweens.Clear();

			_pollingWaits.RemoveAll(w => w.seq == seq);
			FinishSequence(seq, sequences.IndexOf(seq), false);
		}
		void SetPausedInternal(AnimSequence seq, bool paused) {
			if (seq == null) {
				return;
			}

			seq.SetPaused(paused);

			foreach (var tween in seq.activeTweens) {
				if (tween.isAlive) {
					tween.isPaused = paused;
				}
			}
		}
		public void RestoreInitialState() {
			foreach (var seq in sequences) {
				foreach (var step in seq.steps) {
					if (!step.enabled || !step.isInitialized || step.resolvedTarget == null) continue;

					// Setze Transforms und Farben auf die initial gecachten Werte zurück
					switch (step.type) {
						case AnimType.Scale:
						case AnimType.PunchScale:
							if (step.isUI && step.rectTarget != null) step.rectTarget.localScale = step.initialLocalScale;
							else step.resolvedTarget.localScale = step.initialLocalScale;
							break;
						case AnimType.Slide:
						case AnimType.Bounce:
						case AnimType.ShakePosition:
							if (step.isUI && step.rectTarget != null)
								step.rectTarget.anchoredPosition = step.initialAnchoredPosition;
							else
								step.resolvedTarget.localPosition = step.initialLocalPosition;
							break;
						case AnimType.Rotate:
						case AnimType.PunchRotate:
						case AnimType.ShakeRotation:
							step.resolvedTarget.localEulerAngles = step.initialLocalRotation;
							break;
						case AnimType.SizeDelta:
							if (step.isUI && step.rectTarget != null)
								step.rectTarget.sizeDelta = step.initialSizeDelta;
							break;
						case AnimType.ColorTint:
							// Farben nutzen als Fallback den "From"-Wert, wenn nicht "FromCurrent" aktiv ist
							if (step.cachedGraphic != null && !step.animateFromCurrent)
								step.cachedGraphic.color = step.colorFrom;
							break;
						case AnimType.Fade:
							if (step.cachedCanvasGroup != null && !step.animateFromCurrent)
								step.cachedCanvasGroup.alpha = step.fadeFrom;
							break;
						case AnimType.FadeSpriteColor:
							if (step.cachedSpriteRenderer != null && !step.animateFromCurrent)
								step.cachedSpriteRenderer.color = step.colorFrom;
							break;
					}
				}
			}
		}
		#endregion

		void PlayDisableSequences() {
			foreach (var seq in sequences) {
				if (seq.trigger == TriggerType.OnDisable && seq.steps != null && seq.steps.Count > 0) {
					PlaySequence(seq, true);
				}
			}
		}

		public void PlaySequence(AnimSequence seq, bool isDisable = false) {
			if (seq.steps == null || seq.steps.Count == 0) {
				if (seq.onCompleteAction != null) {
					seq.onCompleteAction.Invoke();
				}

				if (seq.isTemporary) {
					sequences.Remove(seq);
				}

				return;
			}

			StopSequenceInternal(seq);

			seq.SetPlaying(true);
			seq.SetPaused(false);

			if (seq.onStart != null) {
				seq.onStart.Invoke();
			}

			if (seq.onStartAction != null) {
				seq.onStartAction.Invoke();
			}

			int seqIndex = sequences.IndexOf(seq);

			if (!isDisable) {
				editorStepProgress = new float[seq.steps.Count];
				editorPlayingSeqIndex = seqIndex;
			}

			PlaySequenceSlice(seq, seqIndex, 0, isDisable);
		}
		void PlaySequenceSlice(AnimSequence seq, int seqIndex, int startIndex, bool isDisable) {
			if (startIndex >= seq.steps.Count) {
				FinishSequence(seq, seqIndex, isDisable);
				return;
			}

			var s = Sequence.Create();

			seq.activeTweens.Add(s);

			float maxGroupTime = 0f;
			float groupStartTime = 0f;
			int i = startIndex;
			bool endsSliceEarly = false;

			for (; i < seq.steps.Count; i++) {
				var step = seq.steps[i];

				if (!step.enabled) {
					continue;
				}

				InitStepCache(step);

				float currentTime = 0f;
				if (step.mode == StepMode.Sequential) {
					currentTime = maxGroupTime;
				} else {
					currentTime = groupStartTime;
				}

				if (step.mode == StepMode.Sequential) {
					groupStartTime = currentTime;
				}

				if (step.type == AnimType.WaitUntil || step.type == AnimType.Repeat || (step.type == AnimType.Wait && step.waitMethod == WaitMethod.Frames)) {
					float triggerTime = currentTime + step.delay;
					int breakIndex = i;
					string rAnchor = step.repeatAnchorLabel;
					AnimType bType = step.type;
					var capturedStep = step;
					endsSliceEarly = true;

					s.Group(Tween.Delay(Mathf.Max(triggerTime, 0.001f)).OnComplete(() => {
						if (!seq.IsPlaying) return;
						HandleSliceBreak(bType, capturedStep, seq, seqIndex, breakIndex, rAnchor, isDisable);
					}, false));

					maxGroupTime = Mathf.Max(maxGroupTime, triggerTime);
					i++;
					break;
				}

				float stepDuration = step.duration;
				if (IsInstantType(step.type)) stepDuration = 0.001f;
				else if (IsLogicType(step.type)) stepDuration = 0f;
				else if (step.type == AnimType.TypeWriter && step.cachedText != null) {
					string targetText = string.IsNullOrEmpty(step.setTextValue) ? step.cachedText.text : step.setTextValue;
					stepDuration = Mathf.Max((targetText ?? "").Length, 1f) / Mathf.Max(step.typeWriterCharsPerSecond, 1f);
				}

				float stepEndTime = currentTime + step.delay + stepDuration;

				if (step.type == AnimType.Wait || step.type == AnimType.Anchor) {
					maxGroupTime = stepEndTime;
					groupStartTime = stepEndTime;
				} else {
					maxGroupTime = Mathf.Max(maxGroupTime, stepEndTime);
				}

				if (IsInstantType(step.type)) {
					int ci = i;
					Transform ct = step.resolvedTarget;
					float d = step.delay + currentTime;

					if (d <= 0f) {
						ExecuteInstantStep(seq, seq.steps[ci], ct);
						if (!seq.IsPlaying) return;
						s.Group(Tween.Delay(0.001f));
					} else {
						s.Group(Tween.Delay(d).OnComplete(() => {
							if (!seq.IsPlaying) return;
							ExecuteInstantStep(seq, seq.steps[ci], ct);
						}, false));
					}
				} else if (!IsLogicType(step.type)) {
					if (!step.animateFromCurrent) {
						if (step.type == AnimType.Bounce || step.type == AnimType.PunchScale || step.type == AnimType.PunchRotate || step.type == AnimType.ShakePosition || step.type == AnimType.ShakeRotation) {
							var t = step.resolvedTarget;
							var tp = step.type;
							var ip = t != null ? t.localPosition : step.initialLocalPosition;
							var ir = t != null ? t.localEulerAngles : step.initialLocalRotation;
							var isc = t != null ? t.localScale : step.initialLocalScale;
							float delayTime = step.delay + currentTime;

							s.Group(Tween.Delay(Mathf.Max(delayTime, 0.001f)).OnComplete(() => {
								if (!seq.IsPlaying || t == null) return;
								if (tp == AnimType.Bounce || tp == AnimType.ShakePosition) t.localPosition = ip;
								else if (tp == AnimType.PunchScale) t.localScale = isc;
								else if (tp == AnimType.PunchRotate || tp == AnimType.ShakeRotation) t.localEulerAngles = ir;
							}, false));
						}
					}
					s.Group(BuildTween(step, step.resolvedTarget, currentTime));
				}

				// EDITOR PROGRESS LOGIK (wieder da)
				if (!isDisable && seqIndex >= 0) {
					int ci = i;
					int cs = seqIndex;
					float actualDur = Mathf.Max(stepDuration, 0.001f);
					bool unscaled = step.type == AnimType.TimeScale;

					s.Group(Tween.Custom(this, new TweenSettings<float>(0f, 1f, new TweenSettings(actualDur, Ease.Linear, 1, CycleMode.Restart, step.delay + currentTime, useUnscaledTime: unscaled)), (t, v) => {
						if (t.editorPlayingSeqIndex == cs && ci < t.editorStepProgress.Length) {
							t.editorStepProgress[ci] = v;
						}
					}));
				}
			}

			int breakEndIndex = i;

			s.ChainCallback(() => {
				seq.activeTweens.Remove(s);
				if (breakEndIndex >= seq.steps.Count && !endsSliceEarly) {
					FinishSequence(seq, seqIndex, isDisable);
				}
			}, false);
		}
		void HandleSliceBreak(AnimType bType, AnimStep step, AnimSequence seq, int seqIndex, int breakIndex, string rAnchor, bool isDisable) {
			if (bType == AnimType.WaitUntil || (bType == AnimType.Wait && step.waitMethod == WaitMethod.Frames)) {
				_pollingWaits.Add(new WaitState {
					seq = seq,
					seqIndex = seqIndex,
					nextIndex = breakIndex + 1,
					isDisable = isDisable,
					step = step,
					frameWaitCounter = step.waitFrames
				});
			} else if (bType == AnimType.Repeat) {
				int anchorIdx = FindAnchorIndex(seq, rAnchor);
				PlaySequenceSlice(seq, seqIndex, anchorIdx >= 0 ? anchorIdx : breakIndex + 1, isDisable);
			}
		}
		void FinishSequence(AnimSequence seq, int seqIndex, bool isDisable) {
			if (!seq.IsPlaying) {
				return;
			}

			seq.SetPlaying(false);
			seq.SetPaused(false);

			if (seq.onEnd != null) {
				seq.onEnd.Invoke();
			}

			if (seq.onCompleteAction != null) {
				seq.onCompleteAction.Invoke();
			}

			string label = seq.label ?? "";

			if (!string.IsNullOrEmpty(label) && _activeSequences.ContainsKey(label) && _activeSequences[label].Count == 0) {
				_activeSequences.Remove(label);
			}

			if (editorPlayingSeqIndex == seqIndex) {
				editorPlayingSeqIndex = -1;
			}

			if (isDisable) {
				_isPlayingDisableSequence = false;

				if (seq.deactivateAfter) {
					try {
						_internalDisable = true;
						gameObject.SetActive(false);
					} finally {
						_internalDisable = false;
					}
				}
			}

			if (seq.isTemporary) {
				sequences.Remove(seq);
			}
		}
		int FindAnchorIndex(AnimSequence seq, string label) {
			for (int i = 0; i < seq.steps.Count; i++) {
				if (seq.steps[i].type == AnimType.Anchor && seq.steps[i].anchorLabel == label) return i;
			}
			return -1;
		}
		void InitStepCache(AnimStep step) {
			if (step.type == AnimType.Trigger || step.type == AnimType.Event || step.type == AnimType.PlayAudio ||
				step.type == AnimType.SetProperty || step.type == AnimType.SetMaterialProperty || step.type == AnimType.ControlSequence ||
				step.type == AnimType.Destroy) {
				step.duration = 0f;
			}

			bool needsHeal = false;

			// Runtime Target Healing Check
			Transform intendedTarget = step.target != null ? step.target : this.transform;
			if (step.isInitialized && step.resolvedTarget != intendedTarget) {
				needsHeal = true;
			}

			if (step.isInitialized) {
				if (step.resolvedTarget == null) {
					needsHeal = true;
				} else if (step.isUI) {
					if ((step.type == AnimType.ColorTint || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Color)) && step.cachedGraphic == null) {
						needsHeal = true;
					}
					if ((step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Text)) && step.cachedText == null) {
						needsHeal = true;
					}
					if ((step.type == AnimType.Fade || (step.type == AnimType.SetProperty && (step.setPropertyType == SetPropertyType.Fade || step.setPropertyType == SetPropertyType.CanvasGroupState))) && step.cachedCanvasGroup == null) {
						needsHeal = true;
					}
					if ((step.type == AnimType.FillAmount || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Image)) && step.cachedImage == null) {
						needsHeal = true;
					}
				} else {
					if ((step.type == AnimType.FadeSpriteColor || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Sprite)) && step.cachedSpriteRenderer == null) {
						needsHeal = true;
					}
				}
				if ((step.type == AnimType.PlayAudio || step.type == AnimType.FadeAudio) && step.cachedAudioSource == null) {
					needsHeal = true;
				}
				if ((step.type == AnimType.MaterialProperty || step.type == AnimType.SetMaterialProperty) && step.cachedMaterial == null) {
					needsHeal = true;
				}

				if (!needsHeal) {
					return;
				}
			}

			step.resolvedTarget = intendedTarget;
			step.rectTarget = step.resolvedTarget as RectTransform;
			step.isUI = step.rectTarget != null;

			if (!step.isInitialized || needsHeal) {
				step.initialLocalPosition = step.resolvedTarget.localPosition;
				step.initialLocalRotation = step.resolvedTarget.localEulerAngles;
				step.initialLocalScale = step.resolvedTarget.localScale;
				if (step.isUI) {
					step.initialAnchoredPosition = step.rectTarget.anchoredPosition;
					step.initialSizeDelta = step.rectTarget.sizeDelta;
					step.initialPivot = step.rectTarget.pivot;
				}
			}

			if (step.type == AnimType.ColorTint || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Color)) {
				step.cachedGraphic = step.resolvedTarget.GetComponent<Graphic>();
				step.cachedText = step.resolvedTarget.GetComponent<TMP_Text>();
				step.cachedSpriteRenderer = step.resolvedTarget.GetComponent<SpriteRenderer>();
			}

			if (step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Text)) {
				step.cachedText = step.tmpTarget != null ? step.tmpTarget : step.resolvedTarget.GetComponent<TMP_Text>();
			}

			if (step.type == AnimType.Fade || (step.type == AnimType.SetProperty && (step.setPropertyType == SetPropertyType.Fade || step.setPropertyType == SetPropertyType.CanvasGroupState))) {
				step.cachedCanvasGroup = step.resolvedTarget.GetComponent<CanvasGroup>();
			}

			if (step.type == AnimType.FillAmount || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Image)) {
				step.cachedImage = step.imageTarget != null ? step.imageTarget : step.resolvedTarget.GetComponent<Image>();
			}

			if (step.type == AnimType.FadeSpriteColor || (step.type == AnimType.SetProperty && step.setPropertyType == SetPropertyType.Sprite)) {
				step.cachedSpriteRenderer = step.spriteTarget != null ? step.spriteTarget : step.resolvedTarget.GetComponent<SpriteRenderer>();
			}

			if (step.type == AnimType.PlayAudio || step.type == AnimType.FadeAudio) {
				step.cachedAudioSource = step.audioTarget != null ? step.audioTarget : step.resolvedTarget.GetComponent<AudioSource>();
			}

			if (step.type == AnimType.MaterialProperty || step.type == AnimType.SetMaterialProperty) {
				if (step.materialTarget != null) {
					step.cachedMaterial = step.materialTarget;
				} else {
					step.cachedRenderer = step.rendererTarget != null ? step.rendererTarget : step.resolvedTarget.GetComponent<Renderer>();
					if (step.cachedRenderer != null) {
						if (step.materialIndex >= 0 && step.materialIndex < step.cachedRenderer.sharedMaterials.Length) {
							step.cachedMaterial = step.cachedRenderer.materials[step.materialIndex];
						}
					} else {
						var graphic = step.graphicTarget != null ? step.graphicTarget : step.resolvedTarget.GetComponent<Graphic>();
						if (graphic != null && graphic.material != null) {
							step.cachedMaterial = graphic.material;
						}
					}
				}
				if (!string.IsNullOrEmpty(step.materialPropertyName)) {
					step.cachedMaterialPropertyId = Shader.PropertyToID(step.materialPropertyName);
				}
			}

			step.isInitialized = true;
		}
		void SnapStepsToStart(AnimSequence seq) {
			bool sequentialBranchSnapped = false;

			for (int i = 0; i < seq.steps.Count; i++) {
				var step = seq.steps[i];

				if (!step.enabled) {
					continue;
				}
				if (step.animateFromCurrent) {
					continue;
				}
				if (step.delay > 0f) {
					continue;
				}
				if (IsLogicType(step.type) || IsInstantType(step.type)) {
					continue;
				}
				if (step.mode == StepMode.Sequential && sequentialBranchSnapped) {
					continue;
				}

				InitStepCache(step);

				Transform t = step.resolvedTarget;
				if (t == null) {
					continue;
				}

				switch (step.type) {
					case AnimType.Fade:
						if (step.cachedCanvasGroup != null) {
							step.cachedCanvasGroup.alpha = step.fadeFrom;
						}
						break;
					case AnimType.Scale:
						if (step.isUI && step.rectTarget != null) {
							step.rectTarget.localScale = step.relativeOffset
								? step.initialLocalScale + step.scaleFrom
								: step.scaleFrom;
						} else {
							t.localScale = step.relativeOffset
								? step.initialLocalScale + step.scaleFrom3D
								: step.scaleFrom3D;
						}
						break;
					case AnimType.Slide:
						if (step.isUI && step.rectTarget != null) {
							step.rectTarget.anchoredPosition = step.relativeOffset
								? step.initialAnchoredPosition + step.slideFrom
								: step.slideFrom;
						} else {
							t.localPosition = step.relativeOffset
								? step.initialLocalPosition + (Vector3)step.slideFrom
								: (Vector3)step.slideFrom;
						}
						break;
					case AnimType.Rotate:
						if (step.relativeOffset) {
							t.localEulerAngles = step.initialLocalRotation + new Vector3(0, 0, step.rotateFrom);
						} else {
							t.localEulerAngles = new Vector3(step.initialLocalRotation.x, step.initialLocalRotation.y, step.rotateFrom);
						}
						break;
					case AnimType.SizeDelta:
						if (step.isUI && step.rectTarget != null) {
							step.rectTarget.sizeDelta = step.relativeOffset
								? step.initialSizeDelta + step.sizeDeltaFrom
								: step.sizeDeltaFrom;
						}
						break;
					case AnimType.FillAmount:
						if (step.cachedImage != null) {
							step.cachedImage.fillAmount = step.fillAmountFrom;
						}
						break;
					case AnimType.ColorTint:
						if (step.cachedGraphic != null) {
							step.cachedGraphic.color = step.colorFrom;
						}
						break;
					case AnimType.FadeSpriteColor:
						if (step.cachedSpriteRenderer != null) {
							step.cachedSpriteRenderer.color = step.colorFrom;
						}
						break;
					case AnimType.TypeWriter:
						if (step.cachedText != null) {
							if (!string.IsNullOrEmpty(step.setTextValue)) {
								step.cachedText.SetText(step.setTextValue);
							}
							step.cachedText.maxVisibleCharacters = 0;
							step.cachedText.ForceMeshUpdate(true);
						}
						break;
					case AnimType.TextCounter:
						if (step.cachedText != null) {
							step.textCounterCurrentValue = step.textCounterFrom;
							step.cachedText.text = string.Format(
								step.textCounterFormat,
								step.textCounterRoundToInt ? Mathf.RoundToInt(step.textCounterFrom) : step.textCounterFrom
							);
						}
						break;
					case AnimType.FadeAudio:
						if (step.cachedAudioSource != null) {
							step.cachedAudioSource.volume = step.fadeAudioFrom;
						}
						break;
				}

				if (step.mode == StepMode.Sequential) {
					sequentialBranchSnapped = true;
				}
			}
		}

		static bool IsInstantType(AnimType t) {
			return t == AnimType.Trigger ||
				   t == AnimType.Event ||
				   t == AnimType.PlayAudio ||
				   t == AnimType.SetProperty ||
				   t == AnimType.SetMaterialProperty ||
				   t == AnimType.ControlSequence ||
				   t == AnimType.Destroy;
		}
		static bool IsLogicType(AnimType type) {
			return type == AnimType.Anchor || type == AnimType.Repeat || type == AnimType.WaitUntil;
		}

		void ExecuteInstantStep(AnimSequence parentSeq, AnimStep step, Transform target) {
			Transform t = step.resolvedTarget;

			switch (step.type) {
				case AnimType.Event:
					if (step.onEvent != null) {
						step.onEvent.Invoke();
					}
					break;
				case AnimType.Trigger:
					if (!string.IsNullOrEmpty(step.triggerSequenceLabel)) {
						AnimSequencer seqTarget = step.triggerSequencer != null ? step.triggerSequencer : this;
						Tween.Delay(0.001f, useUnscaledTime: true).OnComplete(() => {
							if (seqTarget != null && seqTarget.gameObject.activeInHierarchy) {
								seqTarget.PlayByLabel(step.triggerSequenceLabel);
							}
						});
					}
					break;
				case AnimType.SetMaterialProperty:
					if (step.cachedMaterial != null) {
						if (step.materialPropertyType == MaterialPropertyType.Float) {
							step.cachedMaterial.SetFloat(step.cachedMaterialPropertyId, step.materialFloatTo);
						} else if (step.materialPropertyType == MaterialPropertyType.Color) {
							step.cachedMaterial.SetColor(step.cachedMaterialPropertyId, step.materialColorTo);
						}
					}
					break;
				case AnimType.SetProperty:
					switch (step.setPropertyType) {
						case SetPropertyType.Active:
							if (t != null) {
								if (!step.setActiveValue) {
									try {
										_internalDisable = true;
										t.gameObject.SetActive(false);
									} finally {
										_internalDisable = false;
									}
								} else {
									t.gameObject.SetActive(true);
								}
							}
							break;
						case SetPropertyType.Transform:
							switch (step.transformSubType) {
								case TransformSubType.LocalPosition:
									if (step.relativeOffset) {
										t.localPosition += step.setTransformValue;
									} else {
										t.localPosition = step.setTransformValue;
									}
									break;
								case TransformSubType.LocalRotation:
									if (step.relativeOffset) {
										t.localEulerAngles += step.setTransformValue;
									} else {
										t.localEulerAngles = step.setTransformValue;
									}
									break;
								case TransformSubType.LocalScale:
									if (step.relativeOffset) {
										t.localScale += step.setTransformValue;
									} else {
										t.localScale = step.setTransformValue;
									}
									break;
							}
							break;
						case SetPropertyType.Text:
							if (step.cachedText != null) {
								step.cachedText.text = step.setTextValue;
							}
							break;
						case SetPropertyType.Color:
							if (step.cachedGraphic != null) {
								step.cachedGraphic.color = step.colorTo;
							} else if (step.cachedText != null) {
								step.cachedText.color = step.colorTo;
							} else if (step.cachedSpriteRenderer != null) {
								step.cachedSpriteRenderer.color = step.colorTo;
							}
							break;
						case SetPropertyType.Sprite:
							if (step.cachedSpriteRenderer != null) {
								step.cachedSpriteRenderer.sprite = step.setSpriteValue;
							}
							break;
						case SetPropertyType.Image:
							if (step.cachedImage != null) {
								step.cachedImage.sprite = step.setSpriteValue;
							}
							break;
						case SetPropertyType.Fade:
							if (step.cachedCanvasGroup != null) {
								step.cachedCanvasGroup.alpha = step.setFadeValue;
							}
							break;
						case SetPropertyType.CanvasGroupState:
							if (step.cachedCanvasGroup != null) {
								if (step.cgInteractable != OptionalBool.Unchanged) {
									step.cachedCanvasGroup.interactable = step.cgInteractable == OptionalBool.True;
								}
								if (step.cgBlocksRaycasts != OptionalBool.Unchanged) {
									step.cachedCanvasGroup.blocksRaycasts = step.cgBlocksRaycasts == OptionalBool.True;
								}
								if (step.cgIgnoreParentGroups != OptionalBool.Unchanged) {
									step.cachedCanvasGroup.ignoreParentGroups = step.cgIgnoreParentGroups == OptionalBool.True;
								}
							}
							break;
						case SetPropertyType.TimeScale:
							Time.timeScale = step.timeScaleTo;
							break;
						case SetPropertyType.SizeDelta:
							if (step.isUI && step.rectTarget != null) {
								if (step.relativeOffset) {
									step.rectTarget.sizeDelta = step.initialSizeDelta + step.setSizeDeltaValue;
								} else {
									step.rectTarget.sizeDelta = step.setSizeDeltaValue;
								}
							}
							break;
						case SetPropertyType.Pivot:
							if (step.isUI && step.rectTarget != null) {
								step.rectTarget.pivot = step.setPivotValue;
							}
							break;
					}
					break;
				case AnimType.PlayAudio:
					if (step.cachedAudioSource != null && step.audioClip != null) {
						step.cachedAudioSource.pitch = Random.Range(step.audioPitch.x, step.audioPitch.y);
						step.cachedAudioSource.spatialBlend = step.audioSpatialBlend;
						step.cachedAudioSource.PlayOneShot(step.audioClip, Random.Range(step.audioVolume.x, step.audioVolume.y));
					}
					break;
				case AnimType.ControlSequence:
					AnimSequencer ctrlSeq = step.controlSequencerTarget != null ? step.controlSequencerTarget : this;
					if (ctrlSeq != null) {
						List<AnimSequence> targets = new List<AnimSequence>();

						if (step.sequenceControlTarget == SequenceControlTarget.Self) {
							if (parentSeq != null) {
								targets.Add(parentSeq);
							}
						} else if (step.sequenceControlTarget == SequenceControlTarget.Specific && !string.IsNullOrEmpty(step.controlSequenceLabel)) {
							var found = ctrlSeq.sequences.Find(s => s.label == step.controlSequenceLabel);
							if (found != null) {
								targets.Add(found);
							}
						} else if (step.sequenceControlTarget == SequenceControlTarget.All) {
							targets.AddRange(ctrlSeq.sequences);
						}

						foreach (var tSeq in targets) {
							switch (step.sequenceControlType) {
								case SequenceControlType.Stop: ctrlSeq.StopSequenceInternal(tSeq); break;
								case SequenceControlType.Complete: ctrlSeq.CompleteSequenceInternal(tSeq); break;
								case SequenceControlType.Pause: ctrlSeq.SetPausedInternal(tSeq, true); break;
								case SequenceControlType.Resume: ctrlSeq.SetPausedInternal(tSeq, false); break;
							}
						}
					}
					break;
				case AnimType.Destroy:
					if (step.resolvedTarget != null && step.resolvedTarget != this.transform) {
						// Destroy a specific target GameObject
						Object.Destroy(step.resolvedTarget.gameObject);
					} else {
						// Self-destroy: stop all tweens first, suppress OnDisable sequences, then destroy
						StopAll();
						_internalDisable = true;
						Object.Destroy(gameObject);
					}
					break;
			}
		}

		Tween BuildTween(AnimStep step, Transform target, float absoluteDelay) {
			var settings = MakeSettings(step, absoluteDelay);
			var unscaledSettings = MakeSettings(step, absoluteDelay, unscaledTime: true);
			Transform t = step.resolvedTarget;

			switch (step.type) {
				case AnimType.Fade:
					if (step.cachedCanvasGroup != null) {
						if (step.animateFromCurrent) {
							return Tween.Alpha(step.cachedCanvasGroup, step.fadeTo, settings);
						} else {
							return Tween.Alpha(step.cachedCanvasGroup, new TweenSettings<float>(step.fadeFrom, step.fadeTo, settings));
						}
					}
					break;
				case AnimType.Scale:
					if (step.relativeOffset) {
						if (step.isUI) {
							Vector3 baseScale = step.initialLocalScale;
							return Tween.Custom(step.rectTarget, new TweenSettings<float>(0f, 1f, settings), (obj, v) => {
								if (step.animateFromCurrent) {
									obj.localScale = Vector3.LerpUnclamped(baseScale, baseScale + step.scaleTo, v);
								} else {
									obj.localScale = Vector3.LerpUnclamped(baseScale + step.scaleFrom, baseScale + step.scaleTo, v);
								}
							});
						} else {
							Vector3 baseScale = step.initialLocalScale;
							return Tween.Custom(t, new TweenSettings<float>(0f, 1f, settings), (obj, v) => {
								if (step.animateFromCurrent) {
									obj.localScale = Vector3.LerpUnclamped(baseScale, baseScale + step.scaleTo3D, v);
								} else {
									obj.localScale = Vector3.LerpUnclamped(baseScale + step.scaleFrom3D, baseScale + step.scaleTo3D, v);
								}
							});
						}
					} else {
						if (step.isUI) {
							if (step.animateFromCurrent) {
								return Tween.Scale(step.rectTarget, step.scaleTo, settings);
							} else {
								return Tween.Scale(step.rectTarget, new TweenSettings<Vector3>(step.scaleFrom, step.scaleTo, settings));
							}
						} else {
							if (step.animateFromCurrent) {
								return Tween.Scale(t, step.scaleTo3D, settings);
							} else {
								return Tween.Scale(t, new TweenSettings<Vector3>(step.scaleFrom3D, step.scaleTo3D, settings));
							}
						}
					}
				case AnimType.Slide:
					if (step.relativeOffset) {
						if (step.isUI) {
							Vector2 basePos = step.initialAnchoredPosition;
							return Tween.Custom(step.rectTarget, new TweenSettings<float>(0f, 1f, settings), (obj, v) => {
								if (step.animateFromCurrent) {
									obj.anchoredPosition = Vector2.LerpUnclamped(basePos, basePos + step.slideTo, v);
								} else {
									obj.anchoredPosition = Vector2.LerpUnclamped(basePos + step.slideFrom, basePos + step.slideTo, v);
								}
							});
						} else {
							Vector3 basePos = step.initialLocalPosition;
							return Tween.Custom(t, new TweenSettings<float>(0f, 1f, settings), (obj, v) => {
								if (step.animateFromCurrent) {
									obj.localPosition = Vector3.LerpUnclamped(basePos, basePos + (Vector3)step.slideTo, v);
								} else {
									obj.localPosition = Vector3.LerpUnclamped(basePos + (Vector3)step.slideFrom, basePos + (Vector3)step.slideTo, v);
								}
							});
						}
					} else {
						if (step.isUI) {
							if (step.animateFromCurrent) {
								return Tween.UIAnchoredPosition(step.rectTarget, step.slideTo, settings);
							} else {
								return Tween.UIAnchoredPosition(step.rectTarget, new TweenSettings<Vector2>(step.slideFrom, step.slideTo, settings));
							}
						} else {
							if (step.animateFromCurrent) {
								return Tween.LocalPosition(t, (Vector3)step.slideTo, settings);
							} else {
								return Tween.LocalPosition(t, new TweenSettings<Vector3>((Vector3)step.slideFrom, (Vector3)step.slideTo, settings));
							}
						}
					}
				case AnimType.Rotate:
					if (step.relativeOffset) {
						Vector3 baseRot = step.initialLocalRotation;
						return Tween.Custom(t, new TweenSettings<float>(0f, 1f, settings), (obj, v) => {
							if (step.animateFromCurrent) {
								obj.localEulerAngles = Vector3.LerpUnclamped(baseRot, baseRot + new Vector3(0, 0, step.rotateTo), v);
							} else {
								obj.localEulerAngles = Vector3.LerpUnclamped(baseRot + new Vector3(0, 0, step.rotateFrom), baseRot + new Vector3(0, 0, step.rotateTo), v);
							}
						});
					} else {
						var toVec = new Vector3(step.initialLocalRotation.x, step.initialLocalRotation.y, step.rotateTo);
						if (step.animateFromCurrent) {
							return Tween.LocalRotation(t, toVec, settings);
						} else {
							return Tween.LocalRotation(t, new TweenSettings<Vector3>(new Vector3(step.initialLocalRotation.x, step.initialLocalRotation.y, step.rotateFrom), toVec, settings));
						}
					}
				case AnimType.SizeDelta:
					if (step.isUI) {
						if (step.relativeOffset) {
							Vector2 baseSize = step.initialSizeDelta;
							return Tween.Custom(step.rectTarget, new TweenSettings<float>(0f, 1f, settings), (obj, v) => {
								if (step.animateFromCurrent) {
									obj.sizeDelta = Vector2.LerpUnclamped(baseSize, baseSize + step.sizeDeltaTo, v);
								} else {
									obj.sizeDelta = Vector2.LerpUnclamped(baseSize + step.sizeDeltaFrom, baseSize + step.sizeDeltaTo, v);
								}
							});
						} else {
							if (step.animateFromCurrent) {
								return Tween.UISizeDelta(step.rectTarget, step.sizeDeltaTo, settings);
							} else {
								return Tween.UISizeDelta(step.rectTarget, new TweenSettings<Vector2>(step.sizeDeltaFrom, step.sizeDeltaTo, settings));
							}
						}
					}
					break;
				case AnimType.FillAmount:
					if (step.cachedImage != null) {
						if (step.animateFromCurrent) {
							return Tween.UIFillAmount(step.cachedImage, step.fillAmountTo, settings);
						} else {
							return Tween.UIFillAmount(step.cachedImage, new TweenSettings<float>(step.fillAmountFrom, step.fillAmountTo, settings));
						}
					}
					break;
				case AnimType.Bounce:
					if (step.isUI) {
						return Tween.PunchLocalPosition(step.rectTarget, new ShakeSettings(new Vector3(0f, step.bounceIntensity, 0f), Mathf.Max(step.duration, 0.001f), step.bounceCount, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					} else {
						return Tween.PunchLocalPosition(t, new ShakeSettings(step.bounce3D, Mathf.Max(step.duration, 0.001f), step.bounceCount, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					}
				case AnimType.PunchRotate:
					if (step.isUI) {
						float angle = step.punchRotateRandom ? (Random.value > 0.5f ? step.punchRotateAngle1 : step.punchRotateAngle2) : step.punchRotateAngle;
						return Tween.PunchLocalRotation(step.rectTarget, new ShakeSettings(new Vector3(0f, 0f, angle), Mathf.Max(step.duration, 0.001f), step.punchRotateFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					} else {
						return Tween.PunchLocalRotation(t, new ShakeSettings(step.punchRotate3D, Mathf.Max(step.duration, 0.001f), step.punchRotateFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					}
				case AnimType.PunchScale:
					Vector3 pStrength = step.punchScaleUseVector3 ? step.punchScale3D : Vector3.one * step.punchScaleIntensity;
					if (step.isUI) {
						return Tween.PunchScale(step.rectTarget, new ShakeSettings(pStrength, Mathf.Max(step.duration, 0.001f), step.punchScaleFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					} else {
						return Tween.PunchScale(t, new ShakeSettings(pStrength, Mathf.Max(step.duration, 0.001f), step.punchScaleFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					}
				case AnimType.ShakePosition:
					return Tween.ShakeLocalPosition(t, new ShakeSettings(step.shakeStrength, Mathf.Max(step.duration, 0.001f), step.shakeFrequency, enableFalloff: step.shakeFalloff, startDelay: step.delay + absoluteDelay));
				case AnimType.ShakeRotation:
					return Tween.ShakeLocalRotation(t, new ShakeSettings(step.shakeStrength, Mathf.Max(step.duration, 0.001f), step.shakeFrequency, enableFalloff: step.shakeFalloff, startDelay: step.delay + absoluteDelay));
				case AnimType.ColorTint:
					if (step.cachedGraphic != null) {
						if (step.animateFromCurrent) {
							return Tween.Color(step.cachedGraphic, step.colorTo, settings);
						} else {
							return Tween.Color(step.cachedGraphic, new TweenSettings<Color>(step.colorFrom, step.colorTo, settings));
						}
					}
					break;
				case AnimType.FadeSpriteColor:
					if (step.cachedSpriteRenderer != null) {
						Color fromColor = step.animateFromCurrent ? step.cachedSpriteRenderer.color : step.colorFrom;
						return Tween.Color(step.cachedSpriteRenderer, new TweenSettings<Color>(fromColor, step.colorTo, settings));
					}
					break;
				case AnimType.TypeWriter:
					if (step.cachedText != null) {
						string targetText = string.IsNullOrEmpty(step.setTextValue) ? step.cachedText.text : step.setTextValue;

						if (!step.animateFromCurrent) {
							if (!string.IsNullOrEmpty(step.setTextValue)) {
								step.cachedText.SetText(step.setTextValue);
							}
							step.cachedText.maxVisibleCharacters = 0;
							step.cachedText.ForceMeshUpdate(true);
						}

						float estimatedDur = Mathf.Max((targetText ?? "").Length, 1f) / Mathf.Max(step.typeWriterCharsPerSecond, 1f);
						bool isInit = false;
						int cachedCharCount = 0;
						return Tween.Custom(step.cachedText, new TweenSettings<float>(0f, 1f, MakeSettings(step, absoluteDelay, estimatedDur)), (obj, v) => {
							if (!isInit || v == 0f) {
								if (!string.IsNullOrEmpty(step.setTextValue)) {
									obj.SetText(step.setTextValue);
								}
								obj.maxVisibleCharacters = 0;
								obj.ForceMeshUpdate(true);
								cachedCharCount = obj.textInfo.characterCount;
								isInit = true;
							}
							obj.maxVisibleCharacters = Mathf.RoundToInt(Mathf.Lerp(0, cachedCharCount, v));
						});
					}
					break;
				case AnimType.TextCounter:
					if (step.cachedText != null) {
						float fromNum = step.animateFromCurrent ? step.textCounterCurrentValue : step.textCounterFrom;

						if (!step.animateFromCurrent) {
							step.textCounterCurrentValue = fromNum;
							step.cachedText.text = string.Format(step.textCounterFormat, step.textCounterRoundToInt ? Mathf.RoundToInt(fromNum) : fromNum);
						}

						return Tween.Custom(step.cachedText, new TweenSettings<float>(fromNum, step.textCounterTo, settings), (obj, v) => {
							step.textCounterCurrentValue = v;
							obj.text = string.Format(step.textCounterFormat, step.textCounterRoundToInt ? Mathf.RoundToInt(v) : v);
						});
					}
					break;
				case AnimType.FadeAudio:
					if (step.cachedAudioSource != null) {
						if (step.animateFromCurrent) {
							return Tween.AudioVolume(step.cachedAudioSource, step.fadeAudioTo, settings);
						} else {
							return Tween.AudioVolume(step.cachedAudioSource, new TweenSettings<float>(step.fadeAudioFrom, step.fadeAudioTo, settings));
						}
					}
					break;
				case AnimType.TimeScale:
					float startTS = step.animateFromCurrent ? Time.timeScale : step.timeScaleFrom;
					return Tween.Custom(this, new TweenSettings<float>(startTS, step.timeScaleTo, unscaledSettings), (obj, v) => {
						Time.timeScale = v;
					});
				case AnimType.MaterialProperty:
					if (step.cachedMaterial != null) {
						if (step.materialPropertyType == MaterialPropertyType.Float) {
							if (step.animateFromCurrent) {
								return Tween.MaterialProperty(step.cachedMaterial, step.cachedMaterialPropertyId, step.materialFloatTo, settings);
							} else {
								return Tween.MaterialProperty(step.cachedMaterial, step.cachedMaterialPropertyId, new TweenSettings<float>(step.materialFloatFrom, step.materialFloatTo, settings));
							}
						} else if (step.materialPropertyType == MaterialPropertyType.Color) {
							if (step.animateFromCurrent) {
								return Tween.MaterialProperty(step.cachedMaterial, step.cachedMaterialPropertyId, step.materialColorTo, settings);
							} else {
								return Tween.MaterialProperty(step.cachedMaterial, step.cachedMaterialPropertyId, new TweenSettings<Vector4>(step.materialColorFrom, step.materialColorTo, settings));
							}
						}
					}
					break;
			}

			return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0f));
		}

		static TweenSettings MakeSettings(AnimStep step, float absoluteDelay, float overrideDuration = -1f, bool unscaledTime = false) {
			float targetDuration = overrideDuration >= 0f ? overrideDuration : step.duration;
			float finalDuration = Mathf.Max(targetDuration, 0.001f);

			if (step.ease == Ease.Custom) {
				return new TweenSettings(finalDuration, step.customCurve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), 1, CycleMode.Restart, step.delay + absoluteDelay, useUnscaledTime: unscaledTime);
			}
			return new TweenSettings(finalDuration, step.ease, 1, CycleMode.Restart, step.delay + absoluteDelay, useUnscaledTime: unscaledTime);
		}

		struct WaitState {
			public AnimSequence seq;
			public int seqIndex;
			public int nextIndex;
			public bool isDisable;
			public AnimStep step;
			public int frameWaitCounter;
		}

		[System.Serializable]
		class SequenceWrapper {
			public List<AnimSequence> sequences;
		}

		[System.Serializable]
		public class AnimSequence {

			[System.NonSerialized] public AnimSequencer owner;
			public string label;
			public TriggerType trigger;
			public bool deactivateAfter = true;
			public Selectable selectableTarget;
			public List<AnimStep> steps = new();
			public UnityEvent onStart = new();
			public UnityEvent onEnd = new();

			[System.NonSerialized] public System.Action onStartAction;
			[System.NonSerialized] public System.Action onCompleteAction;
			[System.NonSerialized] public bool isTemporary;
			[System.NonSerialized] internal List<Sequence> activeTweens = new();
			[System.NonSerialized] private bool _isPlaying;
			[System.NonSerialized] private bool _isPaused;

			public bool IsPlaying => _isPlaying;
			public bool IsPaused => _isPaused;
			internal void SetPlaying(bool value) { _isPlaying = value; }
			internal void SetPaused(bool value) { _isPaused = value; }

#if UNITY_EDITOR
			public bool isExpanded = true;
			public bool eventsExpanded = false;
#endif
		}

		[System.Serializable]
		public class AnimStep {
			public bool enabled = true;
			public StepMode mode = StepMode.Sequential;
			public AnimType type = AnimType.Fade;
			public string tag = "";

			public Transform target;
			public TMP_Text tmpTarget;
			public SpriteRenderer spriteTarget;
			public Image imageTarget;
			public AudioSource audioTarget;
			public Renderer rendererTarget;

			[Min(0f)] public float duration = 0.3f;
			[Min(0f)] public float delay = 0f;
			public Ease ease = Ease.OutCubic;
			public AnimationCurve customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			public bool animateFromCurrent = false;
			public bool relativeOffset = false;

			[Range(0f, 1f)] public float fadeFrom = 0f;
			[Range(0f, 1f)] public float fadeTo = 1f;
			public Vector3 scaleFrom = Vector3.zero;
			public Vector3 scaleTo = Vector3.one;
			public Vector2 slideFrom = Vector2.zero;
			public Vector2 slideTo = Vector2.zero;
			public float rotateFrom = 0f;
			public float rotateTo = 360f;
			public float bounceIntensity = 30f;
			public float bounceCount = 3f;
			public float punchRotateAngle = 15f;
			public bool punchRotateRandom = false;
			public float punchRotateAngle1 = 15f;
			public float punchRotateAngle2 = -15f;
			public float punchRotateFrequency = 10f;
			public float punchScaleIntensity = 0.2f;
			public float punchScaleFrequency = 10f;
			public float typeWriterCharsPerSecond = 20f;
			public float textCounterFrom = 0f;
			public float textCounterTo = 100f;
			public string textCounterFormat = "{0}";
			public bool textCounterRoundToInt = true;
			public bool punchScaleUseVector3 = false;
			[HideInInspector] public float textCounterCurrentValue = 0f;
			public Color colorFrom = Color.white;
			public Color colorTo = Color.white;
			public ColorTargetType colorTarget = ColorTargetType.Image;
			public TransformSubType transformSubType = TransformSubType.LocalPosition;
			public Vector3 setTransformValue = Vector3.zero;
			public string setTextValue = "";
			public bool setActiveValue = true;
			public AnimSequencer triggerSequencer;
			public string triggerSequenceLabel = "";
			public UnityEvent onEvent = new UnityEvent();

			public Vector3 scaleFrom3D = Vector3.zero;
			public Vector3 scaleTo3D = Vector3.one;
			public Vector3 punchRotate3D = new Vector3(0, 0, 15f);
			public Vector3 punchScale3D = new Vector3(0.2f, 0.2f, 0.2f);
			public Vector3 bounce3D = new Vector3(0, 1f, 0);
			public Sprite setSpriteValue;

			public string anchorLabel = "";
			public string repeatAnchorLabel = "";
			public bool waitUntilValue = false;
			public WaitMethod waitMethod = WaitMethod.Seconds;
			public int waitFrames = 1;
			public float setFadeValue = 1f;

			[Range(0f, 1f)] public float fillAmountFrom = 0f;
			[Range(0f, 1f)] public float fillAmountTo = 1f;
			public Vector2 sizeDeltaFrom = Vector2.zero;
			public Vector2 sizeDeltaTo = Vector2.zero;
			public Vector3 shakeStrength = new Vector3(10f, 10f, 0f);
			public float shakeFrequency = 10f;
			public bool shakeFalloff = true;

			public AudioClip audioClip;
			public Vector2 audioVolume = new Vector2(1f, 1f);
			public Vector2 audioPitch = new Vector2(1f, 1f);
			[Range(0f, 1f)] public float audioSpatialBlend = 0f;
			public float fadeAudioFrom = 0f;
			public float fadeAudioTo = 1f;

			public OptionalBool cgInteractable = OptionalBool.Unchanged;
			public OptionalBool cgBlocksRaycasts = OptionalBool.Unchanged;
			public OptionalBool cgIgnoreParentGroups = OptionalBool.Unchanged;

			public float timeScaleFrom = 1f;
			public float timeScaleTo = 1f;

			public string materialPropertyName = "_BaseColor";
			public float materialFloatFrom = 0f;
			public float materialFloatTo = 1f;
			public Color materialColorFrom = Color.white;
			public Color materialColorTo = Color.white;

			public SetPropertyType setPropertyType = SetPropertyType.Active;
			public MaterialPropertyType materialPropertyType = MaterialPropertyType.Float;
			public Vector2 setSizeDeltaValue = Vector2.zero;
			public Vector2 setPivotValue = new Vector2(0.5f, 0.5f);

			public Material materialTarget;
			public Graphic graphicTarget;
			public int materialIndex = 0;

			public SequenceControlType sequenceControlType = SequenceControlType.Stop;
			public SequenceControlTarget sequenceControlTarget = SequenceControlTarget.Specific;
			public AnimSequencer controlSequencerTarget;
			public string controlSequenceLabel = "";

			[System.NonSerialized] public Material cachedMaterial;
			[System.NonSerialized] public Vector2 initialPivot;
			[System.NonSerialized] public System.Func<bool> waitConditionLambda;
			[System.NonSerialized] public Vector3 initialLocalPosition;
			[System.NonSerialized] public Vector3 initialLocalRotation;
			[System.NonSerialized] public Vector3 initialLocalScale;
			[System.NonSerialized] public Vector2 initialAnchoredPosition;
			[System.NonSerialized] public Vector2 initialSizeDelta;
			[System.NonSerialized] public bool isInitialized;
			[System.NonSerialized] public bool isUI;
			[System.NonSerialized] public Transform resolvedTarget;
			[System.NonSerialized] public RectTransform rectTarget;
			[System.NonSerialized] public CanvasGroup cachedCanvasGroup;
			[System.NonSerialized] public TMP_Text cachedText;
			[System.NonSerialized] public Graphic cachedGraphic;
			[System.NonSerialized] public SpriteRenderer cachedSpriteRenderer;
			[System.NonSerialized] public Image cachedImage;
			[System.NonSerialized] public AudioSource cachedAudioSource;
			[System.NonSerialized] public Renderer cachedRenderer;
			[System.NonSerialized] public int cachedMaterialPropertyId;

#if UNITY_EDITOR
			public bool isExpanded = true;
#endif
		}
	}
}