using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using System.Linq;

namespace Sperlich.Sequencer {
	public partial class AnimSequencer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
		
		public List<AnimSequence> sequences = new();
		
		bool _isPlayingDisableSequence;
		bool _disabled;
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
			Play(TriggerType.OnEnable);
		}

		void OnDisable() {
			if (_isPlayingDisableSequence || _internalDisable) {
				return;
			}

			foreach (var kvp in _activeSequences.ToList()) {
				foreach (var s in kvp.Value.ToList()) {
					if (s.isAlive) {
						s.Stop();
					}
				}
			}

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
			for (int i = _pollingWaits.Count - 1; i >= 0; i--) {
				var state = _pollingWaits[i];
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
		}

		public void OnPointerEnter(PointerEventData e) {
			if (!_disabled) {
				Play(TriggerType.OnPointerEnter);
			}
		}

		public void OnPointerExit(PointerEventData e) {
			if (!_disabled) {
				Play(TriggerType.OnPointerExit);
			}
		}

		public void OnPointerDown(PointerEventData e) {
			if (!_disabled) {
				Play(TriggerType.OnPointerDown);
			}
		}

		public void OnPointerUp(PointerEventData e) {
			if (!_disabled) {
				Play(TriggerType.OnPointerUp);
			}
		}

		public void OnPointerClick(PointerEventData e) {
			if (!_disabled) {
				Play(TriggerType.OnClick);
			}
		}

		public void SetEnabled(bool value) {
			_disabled = !value;
		}

		public void Play(TriggerType trigger) {
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
			if (_activeSequences.TryGetValue(sequenceLabel, out var list)) {
				foreach (var s in list) {
					if (s.isAlive) {
						Tween.SetPausedAll(true, this);
					}
				}
			}
		}

		public void Resume(string sequenceLabel) {
			if (_activeSequences.TryGetValue(sequenceLabel, out var list)) {
				foreach (var s in list) {
					if (s.isAlive) {
						Tween.SetPausedAll(false, this);
					}
				}
			}
		}

		public void PauseAll() {
			Tween.SetPausedAll(true, this);
		}

		public void ResumeAll() {
			Tween.SetPausedAll(false, this);
		}

		public void StopByLabel(string sequenceLabel) {
			if (_activeSequences.TryGetValue(sequenceLabel, out var list)) {
				foreach (var s in list.ToList()) {
					if (s.isAlive) {
						s.Stop();
					}
				}

				_activeSequences.Remove(sequenceLabel);
			}

			_pollingWaits.RemoveAll(w => w.seq.label == sequenceLabel);
		}

		void PlayDisableSequences() {
			foreach (var seq in sequences) {
				if (seq.trigger != TriggerType.OnDisable || seq.steps == null || seq.steps.Count == 0) {
					continue;
				}

				PlaySequence(seq, true);
			}
		}

		public void PlaySequence(AnimSequence seq, bool isDisable = false) {
			if (seq.steps == null || seq.steps.Count == 0) {
				return;
			}

			string label = seq.label ?? "";

			if (!string.IsNullOrEmpty(label)) {
				StopByLabel(label);
				_activeSequences[label] = new List<Sequence>();
			}

			if (seq.onStart != null) {
				seq.onStart.Invoke();
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
			string label = seq.label ?? "";

			if (!string.IsNullOrEmpty(label) && _activeSequences.ContainsKey(label)) {
				_activeSequences[label].Add(s);
			}

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

					if (triggerTime > 0.001f) {
						s.Group(Tween.Delay(triggerTime).OnComplete(() => {
							HandleSliceBreak(bType, capturedStep, seq, seqIndex, breakIndex, rAnchor, isDisable);
						}));
					} else {
						s.Group(Tween.Delay(0.001f).OnComplete(() => {
							HandleSliceBreak(bType, capturedStep, seq, seqIndex, breakIndex, rAnchor, isDisable);
						}));
					}

					maxGroupTime = Mathf.Max(maxGroupTime, triggerTime);
					i++;
					break;
				}

				float stepDuration = 0f;

				if (step.type == AnimType.Wait) {
					stepDuration = step.duration;
				} else if (IsInstantType(step.type)) {
					stepDuration = 0.001f;
				} else if (!IsLogicType(step.type)) {
					stepDuration = step.duration;
				}

				if (step.type == AnimType.TypeWriter && step.cachedText != null) {
					string targetText = "";

					if (string.IsNullOrEmpty(step.setTextValue)) {
						targetText = step.cachedText.text;
					} else {
						targetText = step.setTextValue;
					}

					float length = (targetText ?? "").Length;
					stepDuration = Mathf.Max(length, 1f) / Mathf.Max(step.typeWriterCharsPerSecond, 1f);
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
						ExecuteInstantStep(seq.steps[ci], ct);
						s.Group(Tween.Delay(0.001f));
					} else {
						s.Group(Tween.Delay(d).OnComplete(() => {
							ExecuteInstantStep(seq.steps[ci], ct);
						}));
					}
				} else if (!IsLogicType(step.type)) {
					if (!step.animateFromCurrent) {
						if (step.type == AnimType.Bounce || step.type == AnimType.PunchScale || step.type == AnimType.PunchRotate) {
							var t = step.resolvedTarget;
							var tp = step.type;
							var ip = step.initialLocalPosition;
							var ir = step.initialLocalRotation;
							var isc = step.initialLocalScale;
							float delayTime = step.delay + currentTime;

							s.Group(Tween.Delay(Mathf.Max(delayTime, 0.001f)).OnComplete(() => {
								if (t == null) {
									return;
								}

								if (tp == AnimType.Bounce) {
									t.localPosition = ip;
								} else if (tp == AnimType.PunchScale) {
									t.localScale = isc;
								} else if (tp == AnimType.PunchRotate) {
									t.localEulerAngles = ir;
								}
							}));
						}
					}

					s.Group(BuildTween(step, step.resolvedTarget, currentTime));
				}

				if (!isDisable && seqIndex >= 0) {
					int ci = i;
					int cs = seqIndex;
					float actualDur = Mathf.Max(stepDuration, 0.001f);

					s.Group(Tween.Custom(this, new TweenSettings<float>(0f, 1f, new TweenSettings(actualDur, Ease.Linear, 1, CycleMode.Restart, step.delay + currentTime)), (t, v) => {
						if (t.editorPlayingSeqIndex == cs && ci < t.editorStepProgress.Length) {
							t.editorStepProgress[ci] = v;
						}
					}));
				}
			}

			int breakEndIndex = i;
			var capturedS = s;

			s.ChainCallback(() => {
				if (!string.IsNullOrEmpty(label) && _activeSequences.ContainsKey(label)) {
					_activeSequences[label].Remove(capturedS);
				}

				if (breakEndIndex >= seq.steps.Count && !endsSliceEarly) {
					FinishSequence(seq, seqIndex, isDisable);
				}
			});
		}

		void HandleSliceBreak(AnimType bType, AnimStep step, AnimSequence seq, int seqIndex, int breakIndex, string rAnchor, bool isDisable) {
			if (bType == AnimType.WaitUntil || (bType == AnimType.Wait && step.waitMethod == WaitMethod.Frames)) {
				var waitState = new WaitState {
					seq = seq,
					seqIndex = seqIndex,
					nextIndex = breakIndex + 1,
					isDisable = isDisable,
					step = step,
					frameWaitCounter = step.waitFrames
				};

				_pollingWaits.Add(waitState);
			} else if (bType == AnimType.Repeat) {
				int anchorIdx = FindAnchorIndex(seq, rAnchor);

				int targetIndex = breakIndex + 1;

				if (anchorIdx >= 0) {
					targetIndex = anchorIdx;
				}

				PlaySequenceSlice(seq, seqIndex, targetIndex, isDisable);
			}
		}

		void FinishSequence(AnimSequence seq, int seqIndex, bool isDisable) {
			if (seq.onEnd != null) {
				seq.onEnd.Invoke();
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
		}

		int FindAnchorIndex(AnimSequence seq, string label) {
			for (int i = 0; i < seq.steps.Count; i++) {
				if (seq.steps[i].type == AnimType.Anchor && seq.steps[i].anchorLabel == label) {
					return i;
				}
			}

			return -1;
		}

		void InitStepCache(AnimStep step) {
			bool needsHeal = false;

			if (step.isInitialized) {
				if (step.resolvedTarget == null) {
					needsHeal = true;
				} else if (step.isUI) {
					if ((step.type == AnimType.ColorTint || step.type == AnimType.SetColor) && step.cachedGraphic == null) needsHeal = true;
					if ((step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || step.type == AnimType.SetText) && step.cachedText == null) needsHeal = true;
					if ((step.type == AnimType.Fade || step.type == AnimType.SetFade) && step.cachedCanvasGroup == null) needsHeal = true;
					if (step.type == AnimType.SetImage && step.cachedImage == null) needsHeal = true;
				} else {
					if ((step.type == AnimType.SetSprite || step.type == AnimType.FadeSpriteColor) && step.cachedSpriteRenderer == null) needsHeal = true;
				}

				if (!needsHeal) {
					return;
				}
			}

			if (step.target != null) {
				step.resolvedTarget = step.target;
			} else {
				step.resolvedTarget = this.transform;
			}

			step.rectTarget = step.resolvedTarget as RectTransform;
			step.isUI = step.rectTarget != null;

			if (!step.isInitialized) {
				step.initialLocalPosition = step.resolvedTarget.localPosition;
				step.initialLocalRotation = step.resolvedTarget.localEulerAngles;
				step.initialLocalScale = step.resolvedTarget.localScale;

				if (step.isUI) {
					step.initialAnchoredPosition = step.rectTarget.anchoredPosition;
				}
			}

			if (step.type == AnimType.ColorTint || step.type == AnimType.SetColor) {
				step.cachedGraphic = step.resolvedTarget.GetComponent<Graphic>();
				step.cachedText = step.resolvedTarget.GetComponent<TMP_Text>();
				step.cachedSpriteRenderer = step.resolvedTarget.GetComponent<SpriteRenderer>();
			}

			if (step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || step.type == AnimType.SetText) {
				if (step.tmpTarget != null) {
					step.cachedText = step.tmpTarget;
				} else {
					step.cachedText = step.resolvedTarget.GetComponent<TMP_Text>();
				}
			}

			if (step.type == AnimType.Fade || step.type == AnimType.SetFade) {
				step.cachedCanvasGroup = step.resolvedTarget.GetComponent<CanvasGroup>();
			}

			if (step.type == AnimType.SetImage) {
				if (step.imageTarget != null) {
					step.cachedImage = step.imageTarget;
				} else {
					step.cachedImage = step.resolvedTarget.GetComponent<Image>();
				}
			}

			if (step.type == AnimType.SetSprite || step.type == AnimType.FadeSpriteColor) {
				if (step.spriteTarget != null) {
					step.cachedSpriteRenderer = step.spriteTarget;
				} else {
					step.cachedSpriteRenderer = step.resolvedTarget.GetComponent<SpriteRenderer>();
				}
			}

			step.isInitialized = true;
		}

		static bool IsInstantType(AnimType type) {
			return type == AnimType.SetTransform ||
				   type == AnimType.SetText ||
				   type == AnimType.SetColor ||
				   type == AnimType.SetActive ||
				   type == AnimType.Trigger ||
				   type == AnimType.Event ||
				   type == AnimType.SetSprite ||
				   type == AnimType.SetImage ||
				   type == AnimType.SetFade;
		}

		static bool IsLogicType(AnimType type) {
			return type == AnimType.Anchor ||
				   type == AnimType.Repeat ||
				   type == AnimType.WaitUntil;
		}

		void ExecuteInstantStep(AnimStep step, Transform target) {
			switch (step.type) {
				case AnimType.Event:
					if (step.onEvent != null) {
						step.onEvent.Invoke();
					}
					break;

				case AnimType.Trigger:
					if (!string.IsNullOrEmpty(step.triggerSequenceLabel)) {
						AnimSequencer seqTarget = null;

						if (step.triggerSequencer != null) {
							seqTarget = step.triggerSequencer;
						} else {
							seqTarget = this;
						}

						Tween.Delay(0.001f, useUnscaledTime: true).OnComplete(() => {
							if (seqTarget != null && seqTarget.gameObject.activeInHierarchy) {
								seqTarget.PlayByLabel(step.triggerSequenceLabel);
							}
						});
					}
					break;

				case AnimType.SetActive:
					if (target != null) {
						if (!step.setActiveValue) {
							try {
								_internalDisable = true;
								target.gameObject.SetActive(false);
							} finally {
								_internalDisable = false;
							}
						} else {
							target.gameObject.SetActive(true);
						}
					}
					break;

				case AnimType.SetTransform:
					switch (step.transformSubType) {
						case TransformSubType.LocalPosition:
							if (step.relativeOffset) {
								target.localPosition += step.setTransformValue;
							} else {
								target.localPosition = step.setTransformValue;
							}
							break;
						case TransformSubType.LocalRotation:
							if (step.relativeOffset) {
								target.localEulerAngles += step.setTransformValue;
							} else {
								target.localEulerAngles = step.setTransformValue;
							}
							break;
						case TransformSubType.LocalScale:
							if (step.relativeOffset) {
								target.localScale += step.setTransformValue;
							} else {
								target.localScale = step.setTransformValue;
							}
							break;
					}
					break;

				case AnimType.SetText:
					if (step.cachedText != null) {
						step.cachedText.text = step.setTextValue;
					}
					break;

				case AnimType.SetColor:
					if (step.cachedGraphic != null) {
						step.cachedGraphic.color = step.colorTo;
					} else if (step.cachedText != null) {
						step.cachedText.color = step.colorTo;
					} else if (step.cachedSpriteRenderer != null) {
						step.cachedSpriteRenderer.color = step.colorTo;
					}
					break;

				case AnimType.SetSprite:
					if (step.cachedSpriteRenderer != null) {
						step.cachedSpriteRenderer.sprite = step.setSpriteValue;
					}
					break;

				case AnimType.SetImage:
					if (step.cachedImage != null) {
						step.cachedImage.sprite = step.setSpriteValue;
					}
					break;

				case AnimType.SetFade:
					if (step.cachedCanvasGroup != null) {
						step.cachedCanvasGroup.alpha = step.setFadeValue;
					}
					break;
			}
		}

		Tween BuildTween(AnimStep step, Transform target, float absoluteDelay) {
			var settings = MakeSettings(step, absoluteDelay);

			switch (step.type) {
				case AnimType.Fade:
					if (step.cachedCanvasGroup != null) {
						if (step.animateFromCurrent) {
							return Tween.Alpha(step.cachedCanvasGroup, step.fadeTo, settings);
						} else {
							return Tween.Alpha(step.cachedCanvasGroup, new TweenSettings<float>(step.fadeFrom, step.fadeTo, settings));
						}
					}
					return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));

				case AnimType.Scale:
					if (step.relativeOffset) {
						if (step.isUI) {
							Vector3 start = default;
							bool init = false;
							return Tween.Custom(step.rectTarget, new TweenSettings<float>(0f, 1f, settings), (t, v) => {
								if (!init) { start = t.localScale; init = true; }
								if (step.animateFromCurrent) t.localScale = Vector3.LerpUnclamped(start, start + step.scaleTo, v);
								else t.localScale = Vector3.LerpUnclamped(start + step.scaleFrom, start + step.scaleTo, v);
							});
						} else {
							Vector3 start = default;
							bool init = false;
							return Tween.Custom(target, new TweenSettings<float>(0f, 1f, settings), (t, v) => {
								if (!init) { start = t.localScale; init = true; }
								if (step.animateFromCurrent) t.localScale = Vector3.LerpUnclamped(start, start + step.scaleTo3D, v);
								else t.localScale = Vector3.LerpUnclamped(start + step.scaleFrom3D, start + step.scaleTo3D, v);
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
								return Tween.Scale(target, step.scaleTo3D, settings);
							} else {
								return Tween.Scale(target, new TweenSettings<Vector3>(step.scaleFrom3D, step.scaleTo3D, settings));
							}
						}
					}

				case AnimType.Slide:
					if (step.relativeOffset) {
						if (step.isUI) {
							Vector2 start = default;
							bool init = false;
							return Tween.Custom(step.rectTarget, new TweenSettings<float>(0f, 1f, settings), (t, v) => {
								if (!init) { start = t.anchoredPosition; init = true; }
								if (step.animateFromCurrent) t.anchoredPosition = Vector2.LerpUnclamped(start, start + step.slideTo, v);
								else t.anchoredPosition = Vector2.LerpUnclamped(start + step.slideFrom, start + step.slideTo, v);
							});
						} else {
							Vector3 start = default;
							bool init = false;
							return Tween.Custom(target, new TweenSettings<float>(0f, 1f, settings), (t, v) => {
								if (!init) { start = t.localPosition; init = true; }
								if (step.animateFromCurrent) t.localPosition = Vector3.LerpUnclamped(start, start + (Vector3)step.slideTo, v);
								else t.localPosition = Vector3.LerpUnclamped(start + (Vector3)step.slideFrom, start + (Vector3)step.slideTo, v);
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
								return Tween.LocalPosition(target, (Vector3)step.slideTo, settings);
							} else {
								return Tween.LocalPosition(target, new TweenSettings<Vector3>((Vector3)step.slideFrom, (Vector3)step.slideTo, settings));
							}
						}
					}

				case AnimType.Rotate:
					if (step.relativeOffset) {
						Vector3 start = default;
						bool init = false;
						return Tween.Custom(target, new TweenSettings<float>(0f, 1f, settings), (t, v) => {
							if (!init) { start = t.localEulerAngles; init = true; }
							if (step.animateFromCurrent) t.localEulerAngles = Vector3.LerpUnclamped(start, start + new Vector3(0, 0, step.rotateTo), v);
							else t.localEulerAngles = Vector3.LerpUnclamped(start + new Vector3(0, 0, step.rotateFrom), start + new Vector3(0, 0, step.rotateTo), v);
						});
					} else {
						var toVec = new Vector3(step.initialLocalRotation.x, step.initialLocalRotation.y, step.rotateTo);

						if (step.animateFromCurrent) {
							return Tween.LocalRotation(target, toVec, settings);
						} else {
							var fromVec = new Vector3(step.initialLocalRotation.x, step.initialLocalRotation.y, step.rotateFrom);
							return Tween.LocalRotation(target, new TweenSettings<Vector3>(fromVec, toVec, settings));
						}
					}

				case AnimType.Bounce:
					if (step.isUI) {
						return Tween.PunchLocalPosition(step.rectTarget, new ShakeSettings(new Vector3(0f, step.bounceIntensity, 0f), Mathf.Max(step.duration, 0.001f), step.bounceCount, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					} else {
						return Tween.PunchLocalPosition(target, new ShakeSettings(step.bounce3D, Mathf.Max(step.duration, 0.001f), step.bounceCount, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					}

				case AnimType.PunchRotate:
					if (step.isUI) {
						float angle = step.punchRotateAngle;

						if (step.punchRotateRandom) {
							if (Random.value > 0.5f) {
								angle = step.punchRotateAngle1;
							} else {
								angle = step.punchRotateAngle2;
							}
						}

						return Tween.PunchLocalRotation(step.rectTarget, new ShakeSettings(new Vector3(0f, 0f, angle), Mathf.Max(step.duration, 0.001f), step.punchRotateFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					} else {
						return Tween.PunchLocalRotation(target, new ShakeSettings(step.punchRotate3D, Mathf.Max(step.duration, 0.001f), step.punchRotateFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					}

				case AnimType.PunchScale:
					if (step.isUI) {
						return Tween.PunchScale(step.rectTarget, new ShakeSettings(Vector3.one * step.punchScaleIntensity, Mathf.Max(step.duration, 0.001f), step.punchScaleFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					} else {
						return Tween.PunchScale(target, new ShakeSettings(step.punchScale3D, Mathf.Max(step.duration, 0.001f), step.punchScaleFrequency, enableFalloff: true, startDelay: step.delay + absoluteDelay));
					}

				case AnimType.ColorTint:
					if (step.cachedGraphic != null) {
						if (step.animateFromCurrent) {
							return Tween.Color(step.cachedGraphic, step.colorTo, settings);
						} else {
							return Tween.Color(step.cachedGraphic, new TweenSettings<Color>(step.colorFrom, step.colorTo, settings));
						}
					}
					return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));

				case AnimType.FadeSpriteColor:
					if (step.cachedSpriteRenderer != null) {
						Color fromColor = step.colorFrom;

						if (step.animateFromCurrent) {
							fromColor = step.cachedSpriteRenderer.color;
						}

						return Tween.Color(step.cachedSpriteRenderer, new TweenSettings<Color>(fromColor, step.colorTo, settings));
					}
					return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));

				case AnimType.TypeWriter:
					if (step.cachedText == null) {
						return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));
					}

					string targetText = "";

					if (string.IsNullOrEmpty(step.setTextValue)) {
						targetText = step.cachedText.text;
					} else {
						targetText = step.setTextValue;
					}

					float length = (targetText ?? "").Length;
					float estimatedDur = Mathf.Max(length, 1f) / Mathf.Max(step.typeWriterCharsPerSecond, 1f);
					bool isInit = false;
					int cachedCharCount = 0;

					return Tween.Custom(step.cachedText, new TweenSettings<float>(0f, 1f, MakeSettings(step, absoluteDelay, estimatedDur)), (t, v) => {
						if (!isInit || v == 0f) {
							if (!string.IsNullOrEmpty(step.setTextValue)) {
								t.SetText(step.setTextValue);
							}

							t.maxVisibleCharacters = 0;
							t.ForceMeshUpdate(true);
							cachedCharCount = t.textInfo.characterCount;
							isInit = true;
						}

						t.maxVisibleCharacters = Mathf.RoundToInt(Mathf.Lerp(0, cachedCharCount, v));
					});

				case AnimType.TextCounter:
					if (step.cachedText == null) {
						return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));
					}

					string fmt = step.textCounterFormat;
					bool round = step.textCounterRoundToInt;
					float fromNum = step.textCounterFrom;

					if (step.animateFromCurrent) {
						fromNum = step.textCounterCurrentValue;
					}

					return Tween.Custom(step.cachedText, new TweenSettings<float>(fromNum, step.textCounterTo, settings), (t, v) => {
						step.textCounterCurrentValue = v;
						float valToFormat = v;

						if (round) {
							valToFormat = Mathf.RoundToInt(v);
						}

						t.text = string.Format(fmt, valToFormat);
					});

				case AnimType.Wait:
					return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));

				default:
					return Tween.Delay(Mathf.Max(step.duration + step.delay + absoluteDelay, 0.001f));
			}
		}

		static TweenSettings MakeSettings(AnimStep step, float absoluteDelay, float overrideDuration = -1f) {
			float targetDuration = step.duration;

			if (overrideDuration >= 0f) {
				targetDuration = overrideDuration;
			}

			float finalDuration = Mathf.Max(targetDuration, 0.001f);

			if (step.ease == Ease.Custom) {
				AnimationCurve curve = step.customCurve;

				if (curve == null) {
					curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				}

				return new TweenSettings(finalDuration, curve, 1, CycleMode.Restart, step.delay + absoluteDelay);
			}

			return new TweenSettings(finalDuration, step.ease, 1, CycleMode.Restart, step.delay + absoluteDelay);
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

			[System.NonSerialized] public System.Func<bool> waitConditionLambda;
			[System.NonSerialized] public Vector3 initialLocalPosition;
			[System.NonSerialized] public Vector3 initialLocalRotation;
			[System.NonSerialized] public Vector3 initialLocalScale;
			[System.NonSerialized] public Vector2 initialAnchoredPosition;
			[System.NonSerialized] public bool isInitialized;
			[System.NonSerialized] public bool isUI;
			[System.NonSerialized] public Transform resolvedTarget;
			[System.NonSerialized] public RectTransform rectTarget;
			[System.NonSerialized] public CanvasGroup cachedCanvasGroup;
			[System.NonSerialized] public TMP_Text cachedText;
			[System.NonSerialized] public Graphic cachedGraphic;
			[System.NonSerialized] public SpriteRenderer cachedSpriteRenderer;
			[System.NonSerialized] public Image cachedImage;

#if UNITY_EDITOR
			public bool isExpanded = true;
#endif
		}
	}
}