#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Globalization;
using System.Collections.Generic;
using static Sperlich.Sequencer.AnimSequencer;

namespace Sperlich.Sequencer.Editor {
	[CustomEditor(typeof(AnimSequencer))]
	public class AnimSequencerEditor : UnityEditor.Editor {
		AnimSequencer _sequencer;

		static readonly Color BgDark = new Color(0.12f, 0.13f, 0.16f);
		static readonly Color BgStep = new Color(0.17f, 0.18f, 0.22f);
		static readonly Color BgStepBody = new Color(0.14f, 0.15f, 0.18f);

		static readonly Color ColorSlide = new Color(0.20f, 0.75f, 0.95f);
		static readonly Color ColorScale = new Color(0.30f, 0.90f, 0.50f);
		static readonly Color ColorRotate = new Color(0.70f, 0.50f, 0.95f);
		static readonly Color ColorBounce = new Color(0.95f, 0.90f, 0.30f);
		static readonly Color ColorPunchRotate = new Color(0.95f, 0.45f, 0.20f);
		static readonly Color ColorPunchScale = new Color(0.95f, 0.30f, 0.60f);

		static readonly Color ColorFade = new Color(0.50f, 0.60f, 0.95f);
		static readonly Color ColorColorTint = new Color(0.90f, 0.20f, 0.50f);
		static readonly Color ColorSprite = new Color(0.40f, 0.85f, 0.60f);

		static readonly Color ColorTypeWriter = new Color(0.10f, 0.60f, 0.95f);
		static readonly Color ColorTextCounter = new Color(0.90f, 0.70f, 0.10f);

		static readonly Color ColorSetTransform = new Color(0.85f, 0.15f, 0.25f);
		static readonly Color ColorSetText = new Color(0.00f, 0.65f, 0.75f);
		static readonly Color ColorSetColor = new Color(0.65f, 0.10f, 0.45f);
		static readonly Color ColorSetFade = new Color(0.35f, 0.40f, 0.85f);
		static readonly Color ColorSetActive = new Color(0.12f, 0.12f, 0.12f);
		static readonly Color ColorTrigger = new Color(0.85f, 0.85f, 0.35f);
		static readonly Color ColorEvent = new Color(0.2f, 0.8f, 0.8f);
		static readonly Color ColorWait = new Color(0.98f, 0.92f, 0.84f);

		static readonly Color ColorAnchor = new Color(0.85f, 0.25f, 0.25f);
		static readonly Color ColorRepeat = new Color(0.95f, 0.35f, 0.20f);
		static readonly Color ColorWaitUntil = new Color(0.90f, 0.80f, 0.20f);

		static readonly Color ColorSeq = new Color(0.30f, 0.85f, 0.40f);
		static readonly Color ColorPar = new Color(0.95f, 0.55f, 0.15f);

		static readonly Color ToggleOnBg = new Color(0.25f, 0.75f, 0.65f);
		static readonly Color ToggleOffBg = new Color(0.25f, 0.25f, 0.30f);

		static readonly Color ButtonBg = new Color(0.22f, 0.23f, 0.27f);
		static readonly Color ButtonHoverBg = new Color(0.28f, 0.30f, 0.38f);
		static readonly Color ButtonBorder = new Color(0.35f, 0.38f, 0.45f, 0.3f);
		static readonly Color ButtonAccent = new Color(0.30f, 0.90f, 0.50f);

		VisualElement _root;
		double _pasteSuccessTime = 0;

		void OnEnable() {
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		void OnDisable() {
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
		}

		void OnUndoRedoPerformed() {
			if (_root == null || _sequencer == null) {
				return;
			}

			serializedObject.Update();
			BuildUI(_root);
		}

		bool IsSequencerUI() {
			return _sequencer != null && _sequencer.GetComponent<RectTransform>() != null;
		}

		bool IsStepUI(AnimStep step) {
			if (step.target != null) {
				return step.target is RectTransform;
			}

			return IsSequencerUI();
		}

		public override VisualElement CreateInspectorGUI() {
			_sequencer = (AnimSequencer)target;
			_root = new VisualElement();
			_root.style.paddingBottom = 4;
			BuildUI(_root);
			return _root;
		}

		void BuildUI(VisualElement root) {
			root.Clear();
			root.Add(MakeHeader());
			root.Add(MakeCopyPasteRow(root));

			for (int i = 0; i < _sequencer.sequences.Count; i++) {
				root.Add(BuildSequenceElement(i, root));
			}

			root.Add(MakeAddSequenceButton(root));
		}

		VisualElement MakeHeader() {
			var container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			container.style.justifyContent = Justify.SpaceBetween;
			container.style.alignItems = Align.Center;
			container.style.backgroundColor = new StyleColor(BgDark);
			container.style.paddingTop = 6;
			container.style.paddingBottom = 6;
			container.style.paddingLeft = 8;
			container.style.paddingRight = 8;
			container.style.marginBottom = 4;

			var label = new Label("Anim Sequencer");
			label.style.fontSize = 13;
			label.style.unityFontStyleAndWeight = FontStyle.Bold;
			label.style.color = Color.white;

			bool isUI = IsSequencerUI();
			var badge = new Label(isUI ? "UI [RectTransform]" : "World [Transform]");
			badge.style.fontSize = 10;
			badge.style.unityFontStyleAndWeight = FontStyle.Bold;
			badge.style.color = Color.white;

			Color badgeColor;
			if (isUI) {
				badgeColor = new Color(0.15f, 0.45f, 0.85f);
			} else {
				badgeColor = new Color(0.85f, 0.2f, 0.2f);
			}

			badge.style.backgroundColor = new StyleColor(badgeColor);

			badge.style.paddingTop = 2;
			badge.style.paddingBottom = 2;
			badge.style.paddingLeft = 6;
			badge.style.paddingRight = 6;
			badge.style.borderTopLeftRadius = 3;
			badge.style.borderTopRightRadius = 3;
			badge.style.borderBottomLeftRadius = 3;
			badge.style.borderBottomRightRadius = 3;

			container.Add(label);
			container.Add(badge);
			return container;
		}

		VisualElement MakeCopyPasteRow(VisualElement root) {
			var row = new VisualElement();
			row.style.flexDirection = FlexDirection.Row;
			row.style.marginBottom = 4;

			var copy = new Button();
			copy.text = "Copy All";
			copy.style.flexGrow = 1;
			copy.style.height = 22;
			ApplyNeonButtonStyle(copy);

			copy.clicked += () => {
				EditorGUIUtility.systemCopyBuffer = _sequencer.CopyToJson();
				copy.text = "✓ Copied!";
				copy.schedule.Execute(() => copy.text = "Copy All").StartingIn(1200);
			};

			var paste = new Button();
			bool justPasted = (EditorApplication.timeSinceStartup - _pasteSuccessTime) < 1.2;

			if (justPasted) {
				paste.text = "✓ Pasted!";
				long delay = (long)((1.2 - (EditorApplication.timeSinceStartup - _pasteSuccessTime)) * 1000);
				paste.schedule.Execute(() => paste.text = "Paste All").StartingIn(delay);
			} else {
				paste.text = "Paste All";
			}

			paste.style.flexGrow = 1;
			paste.style.height = 22;
			ApplyNeonButtonStyle(paste);

			paste.clicked += () => {
				string json = EditorGUIUtility.systemCopyBuffer;

				if (string.IsNullOrEmpty(json)) {
					paste.text = "⚠ Clipboard Empty";
					paste.schedule.Execute(() => paste.text = "Paste All").StartingIn(1200);
					return;
				}

				try {
					Undo.RecordObject(_sequencer, "Paste Sequences");
					_sequencer.PasteFromJson(json);
					EditorUtility.SetDirty(_sequencer);
					_pasteSuccessTime = EditorApplication.timeSinceStartup;
					BuildUI(root);
				} catch {
					paste.text = "⚠ Invalid Data";
					paste.schedule.Execute(() => paste.text = "Paste All").StartingIn(1200);
				}
			};

			row.Add(copy);
			row.Add(paste);
			return row;
		}

		Button MakeAddSequenceButton(VisualElement root) {
			var btn = new Button(() => {
				Undo.RecordObject(_sequencer, "Add Sequence");
				_sequencer.sequences.Add(new AnimSequence());
				EditorUtility.SetDirty(_sequencer);
				BuildUI(root);
			});

			btn.text = "+ Add Sequence";
			btn.style.height = 30;
			btn.style.marginTop = 6;
			ApplyNeonButtonStyle(btn, true);

			return btn;
		}

		SerializedProperty GetSeqProp(int seqIndex) {
			serializedObject.Update();
			return serializedObject.FindProperty("sequences").GetArrayElementAtIndex(seqIndex);
		}

		SerializedProperty GetStepProp(int seqIndex, int stepIndex) {
			return GetSeqProp(seqIndex).FindPropertyRelative("steps").GetArrayElementAtIndex(stepIndex);
		}

		VisualElement BuildSequenceElement(int seqIndex, VisualElement root) {
			var seq = _sequencer.sequences[seqIndex];
			var box = CreateBox(4, new Color(0.3f, 0.3f, 0.3f));
			box.style.marginBottom = 6;

			var (headerRow, arrowLabel, titleLabel) = MakeSequenceHeader(seq, seqIndex, root);
			box.Add(headerRow);

			var body = new VisualElement();
			body.style.paddingLeft = 8;
			body.style.paddingRight = 8;
			body.style.paddingTop = 6;
			body.style.paddingBottom = 6;

			if (seq.isExpanded) {
				body.style.display = DisplayStyle.Flex;
			} else {
				body.style.display = DisplayStyle.None;
			}

			headerRow.RegisterCallback<ClickEvent>(evt => {
				if (evt.button != 0) {
					return;
				}

				var ve = evt.target as VisualElement;

				if (ve is Button || ve?.parent is Button) {
					return;
				}

				seq.isExpanded = !seq.isExpanded;

				if (seq.isExpanded) {
					body.style.display = DisplayStyle.Flex;
					arrowLabel.text = "▼";
				} else {
					body.style.display = DisplayStyle.None;
					arrowLabel.text = "▶";
				}

				EditorUtility.SetDirty(_sequencer);
			});

			BuildSequenceBody(body, seq, seqIndex, root, titleLabel);
			box.Add(body);
			return box;
		}

		(VisualElement row, Label arrow, Label title) MakeSequenceHeader(AnimSequence seq, int seqIndex, VisualElement root) {
			var row = new VisualElement();
			row.style.flexDirection = FlexDirection.Row;
			row.style.alignItems = Align.Center;
			row.style.backgroundColor = new StyleColor(BgDark);
			row.style.paddingLeft = 4;
			row.style.paddingRight = 4;
			row.style.paddingTop = 3;
			row.style.paddingBottom = 3;

			row.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Sequence", a => {
					EditorGUIUtility.systemCopyBuffer = "ANIMSEQ_SEQ:" + JsonUtility.ToJson(seq, true);
				});

				evt.menu.AppendAction("Paste Sequence", a => {
					string clip = EditorGUIUtility.systemCopyBuffer;

					if (clip != null && clip.StartsWith("ANIMSEQ_SEQ:")) {
						Undo.RecordObject(_sequencer, "Paste Sequence");
						_sequencer.sequences[seqIndex] = JsonUtility.FromJson<AnimSequence>(clip.Substring(12));
						EditorUtility.SetDirty(_sequencer);
						BuildUI(root);
					}
				}, a => EditorGUIUtility.systemCopyBuffer != null && EditorGUIUtility.systemCopyBuffer.StartsWith("ANIMSEQ_SEQ:") ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));

			var arrow = new Label(seq.isExpanded ? "▼" : "▶");
			arrow.style.marginLeft = 4;
			arrow.style.marginRight = 4;
			arrow.style.fontSize = 9;
			arrow.style.width = 12;
			arrow.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
			arrow.style.unityTextAlign = TextAnchor.MiddleLeft;

			string labelText = seq.label;

			if (string.IsNullOrEmpty(seq.label)) {
				labelText = $"Sequence {seqIndex}";
			}

			var title = new Label(labelText);
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			title.style.color = Color.white;
			title.style.flexGrow = 1;
			title.style.unityTextAlign = TextAnchor.MiddleLeft;

			var playBtn = MakeSmallButton("▶", 22, () => {
				if (Application.isPlaying) {
					_sequencer.PlayByLabel(seq.label);
				}
			});

			var upBtn = MakeSmallButton("↑", 22, () => {
				if (seqIndex <= 0) {
					return;
				}

				Undo.RecordObject(_sequencer, "Move Sequence Up");
				var temp = _sequencer.sequences[seqIndex - 1];
				_sequencer.sequences[seqIndex - 1] = _sequencer.sequences[seqIndex];
				_sequencer.sequences[seqIndex] = temp;
				EditorUtility.SetDirty(_sequencer);
				BuildUI(root);
			});

			upBtn.SetEnabled(seqIndex > 0);

			var downBtn = MakeSmallButton("↓", 22, () => {
				if (seqIndex >= _sequencer.sequences.Count - 1) {
					return;
				}

				Undo.RecordObject(_sequencer, "Move Sequence Down");
				var temp = _sequencer.sequences[seqIndex + 1];
				_sequencer.sequences[seqIndex + 1] = _sequencer.sequences[seqIndex];
				_sequencer.sequences[seqIndex] = temp;
				EditorUtility.SetDirty(_sequencer);
				BuildUI(root);
			});

			downBtn.SetEnabled(seqIndex < _sequencer.sequences.Count - 1);

			var removeBtn = MakeSmallButton("✕", 22, () => {
				Undo.RecordObject(_sequencer, "Remove Sequence");
				_sequencer.sequences.RemoveAt(seqIndex);
				EditorUtility.SetDirty(_sequencer);
				BuildUI(root);
			});

			row.Add(arrow);
			row.Add(title);
			row.Add(playBtn);
			row.Add(upBtn);
			row.Add(downBtn);
			row.Add(removeBtn);

			return (row, arrow, title);
		}

		void BuildSequenceBody(VisualElement body, AnimSequence seq, int seqIndex, VisualElement root, Label titleLabel) {
			var labelField = new PropertyField(GetSeqProp(seqIndex).FindPropertyRelative("label"), "Label");
			labelField.Bind(serializedObject);

			labelField.RegisterValueChangeCallback(_ => {
				if (string.IsNullOrEmpty(seq.label)) {
					titleLabel.text = $"Sequence {seqIndex}";
				} else {
					titleLabel.text = seq.label;
				}
			});

			body.Add(labelField);

			var deactivateProp = GetSeqProp(seqIndex).FindPropertyRelative("deactivateAfter");
			var (deactivateField, _) = MakeToggleField(deactivateProp, "Deactivate After", () => seq.deactivateAfter);

			if (seq.trigger == TriggerType.OnDisable) {
				deactivateField.style.display = DisplayStyle.Flex;
			} else {
				deactivateField.style.display = DisplayStyle.None;
			}

			body.Add(deactivateField);

			var selectableField = MakeTargetField(GetSeqProp(seqIndex).FindPropertyRelative("selectableTarget"), "Selectable");

			if (IsInteractableTrigger(seq.trigger)) {
				selectableField.style.display = DisplayStyle.Flex;
			} else {
				selectableField.style.display = DisplayStyle.None;
			}

			var triggerField = new PropertyField(GetSeqProp(seqIndex).FindPropertyRelative("trigger"), "Trigger");
			triggerField.Bind(serializedObject);

			bool seqIsUI = IsSequencerUI();
			var triggerWarning = new HelpBox("", HelpBoxMessageType.Warning);
			triggerWarning.style.display = DisplayStyle.None;
			triggerWarning.style.marginTop = 4;

			void ValidateTrigger(TriggerType t) {
				if (!seqIsUI && (t == TriggerType.OnBecameInteractable || t == TriggerType.OnBecameNonInteractable)) {
					triggerWarning.text = "Interactable Triggers require a UI Selectable component. This will not fire on World objects.";
					triggerWarning.style.display = DisplayStyle.Flex;
				} else if (!seqIsUI && (t == TriggerType.OnClick || t == TriggerType.OnPointerEnter || t == TriggerType.OnPointerExit || t == TriggerType.OnPointerDown || t == TriggerType.OnPointerUp)) {
					triggerWarning.text = "Pointer Events on World objects require a Collider and a PhysicsRaycaster on the Main Camera.";
					triggerWarning.style.display = DisplayStyle.Flex;
				} else {
					triggerWarning.style.display = DisplayStyle.None;
				}
			}

			ValidateTrigger(seq.trigger);

			triggerField.RegisterValueChangeCallback(evt => {
				var newVal = (TriggerType)evt.changedProperty.enumValueIndex;

				if (newVal == TriggerType.OnDisable) {
					deactivateField.style.display = DisplayStyle.Flex;
				} else {
					deactivateField.style.display = DisplayStyle.None;
				}

				if (IsInteractableTrigger(newVal)) {
					selectableField.style.display = DisplayStyle.Flex;
				} else {
					selectableField.style.display = DisplayStyle.None;
				}

				ValidateTrigger(newVal);
				AutoLabel(seq, seqIndex, titleLabel);
			});

			body.Add(triggerField);
			body.Add(triggerWarning);
			body.Add(deactivateField);
			body.Add(selectableField);
			body.Add(Spacer(6));

			var stepsContainer = new VisualElement();
			body.Add(stepsContainer);

			void RebuildSteps() {
				serializedObject.Update();
				stepsContainer.Clear();

				for (int i = 0; i < seq.steps.Count; i++) {
					stepsContainer.Add(BuildStepElement(seqIndex, i, RebuildSteps));
				}
			}

			RebuildSteps();

			var addStepBtn = new Button(() => {
				Undo.RecordObject(_sequencer, "Add Step");
				seq.steps.Add(new AnimStep());
				EditorUtility.SetDirty(_sequencer);
				RebuildSteps();
			});

			addStepBtn.text = "+ Add Step";
			addStepBtn.style.height = 22;
			addStepBtn.style.marginTop = 4;
			ApplyNeonButtonStyle(addStepBtn);

			body.Add(addStepBtn);
			body.Add(Spacer(6));
			body.Add(MakeEventsSection(seq, seqIndex));
		}

		VisualElement MakeEventsSection(AnimSequence seq, int seqIndex) {
			var container = new VisualElement();
			var headerRow = new VisualElement();
			headerRow.style.flexDirection = FlexDirection.Row;
			headerRow.style.alignItems = Align.Center;
			headerRow.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
			headerRow.style.paddingLeft = 4;
			headerRow.style.paddingTop = 3;
			headerRow.style.paddingBottom = 3;
			headerRow.style.borderTopLeftRadius = 3;
			headerRow.style.borderTopRightRadius = 3;

			var arrow = new Label(seq.eventsExpanded ? "▼" : "▶");
			arrow.style.fontSize = 9;
			arrow.style.width = 12;
			arrow.style.marginRight = 4;
			arrow.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));

			var title = new Label("Events");
			title.style.color = new StyleColor(new Color(0.75f, 0.75f, 0.75f));
			title.style.fontSize = 11;
			title.style.unityFontStyleAndWeight = FontStyle.Bold;

			headerRow.Add(arrow);
			headerRow.Add(title);

			var eventsBody = new VisualElement();
			eventsBody.style.paddingTop = 4;
			eventsBody.style.paddingBottom = 4;

			if (seq.eventsExpanded) {
				eventsBody.style.display = DisplayStyle.Flex;
			} else {
				eventsBody.style.display = DisplayStyle.None;
			}

			eventsBody.Add(MakeBoundField(GetSeqProp(seqIndex).FindPropertyRelative("onStart"), "On Start"));
			eventsBody.Add(MakeBoundField(GetSeqProp(seqIndex).FindPropertyRelative("onEnd"), "On End"));

			headerRow.RegisterCallback<ClickEvent>(_ => {
				seq.eventsExpanded = !seq.eventsExpanded;

				if (seq.eventsExpanded) {
					eventsBody.style.display = DisplayStyle.Flex;
					arrow.text = "▼";
				} else {
					eventsBody.style.display = DisplayStyle.None;
					arrow.text = "▶";
				}

				EditorUtility.SetDirty(_sequencer);
			});

			container.Add(headerRow);
			container.Add(eventsBody);
			return container;
		}

		void AutoLabel(AnimSequence seq, int seqIndex, Label titleLabel) {
			bool matchesTrigger = false;

			foreach (TriggerType t in System.Enum.GetValues(typeof(TriggerType))) {
				if (seq.label == t.ToString()) {
					matchesTrigger = true;
					break;
				}
			}

			if (!string.IsNullOrEmpty(seq.label) && !matchesTrigger) {
				return;
			}

			Undo.RecordObject(_sequencer, "Auto Label");
			seq.label = seq.trigger.ToString();

			serializedObject.FindProperty("sequences").GetArrayElementAtIndex(seqIndex).FindPropertyRelative("label").stringValue = seq.label;
			serializedObject.ApplyModifiedProperties();

			titleLabel.text = seq.label;
		}

		VisualElement BuildStepElement(int seqIndex, int stepIndex, System.Action rebuild) {
			var seq = _sequencer.sequences[seqIndex];
			var step = seq.steps[stepIndex];
			Color typeColor = GetAnimTypeColor(step.type);

			var stepBox = CreateBox(3, new Color(0.28f, 0.28f, 0.28f));
			stepBox.style.marginBottom = 4;

			var (stepHeader, colorBar, arrowLabel, infoLabel, tagLabel, modeEl, iconLabel, warningIcon) = MakeStepHeader(step, seqIndex, stepIndex, typeColor, seq, rebuild);
			stepBox.Add(stepHeader);
			stepBox.Add(MakeProgressBar(seqIndex, stepIndex, typeColor));

			var stepBody = new VisualElement();
			stepBody.style.paddingLeft = 8;
			stepBody.style.paddingRight = 8;
			stepBody.style.paddingTop = 6;
			stepBody.style.paddingBottom = 6;
			stepBody.style.backgroundColor = new StyleColor(BgStepBody);

			if (step.isExpanded) {
				stepBody.style.display = DisplayStyle.Flex;
			} else {
				stepBody.style.display = DisplayStyle.None;
			}

			stepHeader.RegisterCallback<ClickEvent>(evt => {
				if (evt.button != 0) {
					return;
				}

				var ve = evt.target as VisualElement;

				if (ve is Button || ve?.parent is Button || ve is Toggle || ve?.parent is Toggle) {
					return;
				}

				step.isExpanded = !step.isExpanded;

				if (step.isExpanded) {
					stepBody.style.display = DisplayStyle.Flex;
					arrowLabel.text = "▼";
				} else {
					stepBody.style.display = DisplayStyle.None;
					arrowLabel.text = "▶";
				}

				EditorUtility.SetDirty(_sequencer);
			});

			BuildStepBody(stepBody, seqIndex, stepIndex, colorBar, infoLabel, tagLabel, modeEl, iconLabel, warningIcon, step);
			stepBox.Add(stepBody);

			return stepBox;
		}

		(VisualElement header, VisualElement colorBar, Label arrow, Label info, Label tagLabel, Label modeEl, Label iconLabel, Image warningIcon)
		MakeStepHeader(AnimStep step, int seqIndex, int stepIndex, Color typeColor, AnimSequence seq, System.Action rebuild) {
			var header = new VisualElement();
			header.style.flexDirection = FlexDirection.Row;
			header.style.alignItems = Align.Center;

			if (step.type == AnimType.Anchor) {
				header.style.backgroundColor = new StyleColor(new Color(ColorAnchor.r, ColorAnchor.g, ColorAnchor.b, 0.4f));
			} else {
				header.style.backgroundColor = new StyleColor(BgStep);
			}

			header.style.minHeight = 24;

			header.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Step", a => EditorGUIUtility.systemCopyBuffer = "ANIMSEQ_STEP:" + JsonUtility.ToJson(step, true));

				evt.menu.AppendAction("Paste Step", a => {
					string clip = EditorGUIUtility.systemCopyBuffer;

					if (clip != null && clip.StartsWith("ANIMSEQ_STEP:")) {
						Undo.RecordObject(_sequencer, "Paste Step");
						seq.steps[stepIndex] = JsonUtility.FromJson<AnimStep>(clip.Substring(13));
						EditorUtility.SetDirty(_sequencer);
						rebuild();
					}
				}, a => EditorGUIUtility.systemCopyBuffer != null && EditorGUIUtility.systemCopyBuffer.StartsWith("ANIMSEQ_STEP:") ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));

			var colorBar = new VisualElement();
			colorBar.style.width = 5;
			colorBar.style.alignSelf = Align.Stretch;

			if (step.enabled) {
				colorBar.style.backgroundColor = new StyleColor(typeColor);
			} else {
				colorBar.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f));
			}

			var arrow = new Label(step.isExpanded ? "▼" : "▶");
			arrow.style.marginLeft = 6;
			arrow.style.marginRight = 4;
			arrow.style.fontSize = 9;
			arrow.style.width = 12;
			arrow.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));

			var enableToggle = new Toggle();
			enableToggle.value = step.enabled;
			enableToggle.style.marginRight = 6;

			enableToggle.RegisterCallback<ClickEvent>(evt => {
				evt.StopPropagation();
			});

			void ApplyCheckmarkStyle(bool isChecked) {
				var checkmark = enableToggle.Q<VisualElement>(className: "unity-toggle__checkmark");

				if (checkmark != null) {
					checkmark.style.backgroundColor = new StyleColor(new Color(0.12f, 0.13f, 0.16f));
					checkmark.style.borderTopColor = new StyleColor(ButtonBorder);
					checkmark.style.borderBottomColor = new StyleColor(ButtonBorder);
					checkmark.style.borderLeftColor = new StyleColor(ButtonBorder);
					checkmark.style.borderRightColor = new StyleColor(ButtonBorder);

					if (isChecked) {
						checkmark.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
					}
				}
			}

			enableToggle.schedule.Execute(() => ApplyCheckmarkStyle(enableToggle.value));

			enableToggle.RegisterValueChangedCallback(evt => {
				Undo.RecordObject(_sequencer, "Toggle Step Enabled");
				step.enabled = evt.newValue;

				if (step.enabled) {
					colorBar.style.backgroundColor = new StyleColor(GetAnimTypeColor(step.type));
				} else {
					colorBar.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f));
				}

				ApplyCheckmarkStyle(evt.newValue);
				EditorUtility.SetDirty(_sequencer);
			});

			var modeEl = new Label(step.mode == StepMode.Sequential ? "SEQ" : "PAR");

			if (step.mode == StepMode.Sequential) {
				modeEl.style.color = new StyleColor(ColorSeq);
			} else {
				modeEl.style.color = new StyleColor(ColorPar);
			}

			modeEl.style.unityFontStyleAndWeight = FontStyle.Bold;
			modeEl.style.fontSize = 10;
			modeEl.style.width = 28;

			if (IsModeHidden(step.type)) {
				modeEl.style.display = DisplayStyle.None;
			} else {
				modeEl.style.display = DisplayStyle.Flex;
			}

			var info = new Label(BuildStepTypeInfo(step));
			info.enableRichText = true;
			info.style.fontSize = 11;
			info.style.flexGrow = 1;

			if (step.enabled) {
				info.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
			} else {
				info.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
			}

			string tagText = "";

			if (!string.IsNullOrEmpty(step.tag)) {
				tagText = $"[{step.tag}]";
			}

			var tagLabel = new Label(tagText);
			tagLabel.style.fontSize = 11;
			tagLabel.style.color = new StyleColor(new Color(0.75f, 0.75f, 0.75f));
			tagLabel.style.marginRight = 4;

			if (string.IsNullOrEmpty(step.tag)) {
				tagLabel.style.display = DisplayStyle.None;
			} else {
				tagLabel.style.display = DisplayStyle.Flex;
			}

			var iconLabel = new Label("");
			iconLabel.style.fontSize = 14;
			iconLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
			iconLabel.style.marginLeft = 4;
			iconLabel.style.marginRight = 8;
			iconLabel.style.display = DisplayStyle.None;

			var warningIcon = new Image();
			warningIcon.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
			warningIcon.style.width = 14;
			warningIcon.style.height = 14;
			warningIcon.style.marginRight = 4;
			warningIcon.style.display = DisplayStyle.None;

			var upBtn = MakeSmallButton("↑", 18, () => {
				if (stepIndex <= 0) {
					return;
				}

				Undo.RecordObject(_sequencer, "Move Step Up");
				var temp = seq.steps[stepIndex - 1];
				seq.steps[stepIndex - 1] = seq.steps[stepIndex];
				seq.steps[stepIndex] = temp;
				EditorUtility.SetDirty(_sequencer);
				rebuild();
			});

			upBtn.SetEnabled(stepIndex > 0);

			var downBtn = MakeSmallButton("↓", 18, () => {
				if (stepIndex >= seq.steps.Count - 1) {
					return;
				}

				Undo.RecordObject(_sequencer, "Move Step Down");
				var temp = seq.steps[stepIndex + 1];
				seq.steps[stepIndex + 1] = seq.steps[stepIndex];
				seq.steps[stepIndex] = temp;
				EditorUtility.SetDirty(_sequencer);
				rebuild();
			});

			downBtn.SetEnabled(stepIndex < seq.steps.Count - 1);

			var removeBtn = MakeSmallButton("✕", 22, () => {
				Undo.RecordObject(_sequencer, "Remove Step");
				seq.steps.RemoveAt(stepIndex);
				EditorUtility.SetDirty(_sequencer);
				rebuild();
			});

			removeBtn.style.marginRight = 4;

			header.Add(colorBar);
			header.Add(arrow);
			header.Add(enableToggle);
			header.Add(modeEl);
			header.Add(info);
			header.Add(tagLabel);
			header.Add(iconLabel);
			header.Add(warningIcon);
			header.Add(upBtn);
			header.Add(downBtn);
			header.Add(removeBtn);

			return (header, colorBar, arrow, info, tagLabel, modeEl, iconLabel, warningIcon);
		}

		VisualElement MakeProgressBar(int seqIndex, int stepIndex, Color typeColor) {
			var bg = new VisualElement();
			bg.style.height = 3;
			bg.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));

			var fill = new VisualElement();
			fill.style.height = 3;
			fill.style.width = Length.Percent(0);
			fill.style.backgroundColor = new StyleColor(new Color(typeColor.r, typeColor.g, typeColor.b, 0f));
			bg.Add(fill);

			int cs = seqIndex;
			int ci = stepIndex;

			bg.schedule.Execute(() => {
				if (_sequencer == null) {
					return;
				}

				bool active = _sequencer.editorPlayingSeqIndex == cs && _sequencer.editorStepProgress != null && ci < _sequencer.editorStepProgress.Length;

				if (active) {
					fill.style.backgroundColor = new StyleColor(new Color(typeColor.r, typeColor.g, typeColor.b, 1f));
					fill.style.width = Length.Percent(_sequencer.editorStepProgress[ci] * 100f);
				} else {
					fill.style.backgroundColor = new StyleColor(new Color(typeColor.r, typeColor.g, typeColor.b, 0f));
					fill.style.width = Length.Percent(0f);
				}
			}).Every(16);

			return bg;
		}

		List<string> GetValidAnimTypes(bool isUI) {
			var valid = new List<string>();

			foreach (AnimType t in System.Enum.GetValues(typeof(AnimType))) {
				bool uiOnly = t == AnimType.Fade ||
							  t == AnimType.SetFade ||
							  t == AnimType.ColorTint ||
							  t == AnimType.SetColor ||
							  t == AnimType.TypeWriter ||
							  t == AnimType.TextCounter ||
							  t == AnimType.SetText ||
							  t == AnimType.SetImage;

				bool worldOnly = t == AnimType.SetSprite ||
								 t == AnimType.FadeSpriteColor;

				if (isUI && !worldOnly) {
					valid.Add(t.ToString());
				} else if (!isUI && !uiOnly) {
					valid.Add(t.ToString());
				}
			}

			valid.Sort();
			return valid;
		}

		void BuildStepBody(VisualElement body, int seqIndex, int stepIndex, VisualElement colorBar, Label infoLabel, Label tagLabel, Label modeEl, Label iconLabel, Image warningIcon, AnimStep step) {
			var stepProp = GetStepProp(seqIndex, stepIndex);

			bool GetCurrentIsUI() {
				var t = stepProp.FindPropertyRelative("target").objectReferenceValue as Transform;

				if (t != null) {
					return t is RectTransform;
				}

				return IsSequencerUI();
			}

			var tagField = new PropertyField(stepProp.FindPropertyRelative("tag"), "Tag");
			tagField.Bind(serializedObject);

			tagField.RegisterValueChangeCallback(_ => {
				if (string.IsNullOrEmpty(step.tag)) {
					tagLabel.text = "";
					tagLabel.style.display = DisplayStyle.None;
				} else {
					tagLabel.text = $"[{step.tag}]";
					tagLabel.style.display = DisplayStyle.Flex;
				}
			});

			body.Add(tagField);

			var modeField = new PropertyField(stepProp.FindPropertyRelative("mode"), "Mode");
			modeField.Bind(serializedObject);

			if (IsModeHidden(step.type)) {
				modeField.style.display = DisplayStyle.None;
			} else {
				modeField.style.display = DisplayStyle.Flex;
			}

			modeField.name = "modeField";
			body.Add(modeField);

			var typeFieldsContainer = new VisualElement();
			var contextWarning = new HelpBox("", HelpBoxMessageType.Error);
			contextWarning.style.display = DisplayStyle.None;

			void UpdateContextWarning() {
				if (step == null) {
					return;
				}

				bool isUI = GetCurrentIsUI();
				bool isCompatible = true;
				string msg = "";
				HelpBoxMessageType msgType = HelpBoxMessageType.Error;

				if (!isUI) {
					if (step.type == AnimType.Fade ||
						step.type == AnimType.SetFade ||
						step.type == AnimType.ColorTint ||
						step.type == AnimType.SetColor ||
						step.type == AnimType.TypeWriter ||
						step.type == AnimType.TextCounter ||
						step.type == AnimType.SetText ||
						step.type == AnimType.SetImage) {
						isCompatible = false;
						msg = $"Type '{step.type}' is strictly for UI elements. Use a RectTransform target.";
					}
				} else {
					if (step.type == AnimType.SetSprite || step.type == AnimType.FadeSpriteColor) {
						isCompatible = false;
						msg = $"Type '{step.type}' is for World 2D Sprites only.";
					}
				}

				if (isCompatible && step.type == AnimType.Repeat) {
					bool anchorExists = _sequencer.sequences[seqIndex].steps.Exists(s => s.type == AnimType.Anchor && s.anchorLabel == step.repeatAnchorLabel);

					if (!anchorExists) {
						isCompatible = false;
						msg = $"Target Anchor '#{step.repeatAnchorLabel}' does not exist in this sequence.";
					}
				}

				if (isCompatible) {
					Transform tTarget = stepProp.FindPropertyRelative("target").objectReferenceValue as Transform;
					Transform effTarget = tTarget;

					if (effTarget == null) {
						effTarget = _sequencer.transform;
					}

					bool missing = false;
					string comp = "";

					if (effTarget != null) {
						if ((step.type == AnimType.Fade || step.type == AnimType.SetFade) && isUI) {
							if (effTarget.GetComponent<CanvasGroup>() == null) {
								missing = true;
								comp = "CanvasGroup";
							}
						} else if (step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || step.type == AnimType.SetText) {
							var tmp = stepProp.FindPropertyRelative("tmpTarget").objectReferenceValue;

							if (tmp == null && effTarget.GetComponent<TMPro.TMP_Text>() == null) {
								missing = true;
								comp = "TMP_Text";
							}
						} else if (step.type == AnimType.SetSprite || step.type == AnimType.FadeSpriteColor) {
							var spr = stepProp.FindPropertyRelative("spriteTarget").objectReferenceValue;

							if (spr == null && effTarget.GetComponent<SpriteRenderer>() == null) {
								missing = true;
								comp = "SpriteRenderer";
							}
						} else if (step.type == AnimType.SetImage) {
							var img = stepProp.FindPropertyRelative("imageTarget").objectReferenceValue;

							if (img == null && effTarget.GetComponent<UnityEngine.UI.Image>() == null) {
								missing = true;
								comp = "Image";
							}
						} else if (step.type == AnimType.ColorTint || step.type == AnimType.SetColor) {
							if (effTarget.GetComponent<UnityEngine.UI.Graphic>() == null) {
								missing = true;
								comp = "Graphic (Image or Text)";
							}
						}
					}

					if (missing) {
						isCompatible = false;
						msg = $"Missing Component: Target '{effTarget.name}' needs a {comp} component to perform {step.type}!";
						msgType = HelpBoxMessageType.Warning;
					}
				}

				if (!isCompatible) {
					if (contextWarning.text != msg) {
						contextWarning.text = msg;
					}

					if (contextWarning.messageType != msgType) {
						contextWarning.messageType = msgType;
					}

					Texture expectedIcon;

					if (msgType == HelpBoxMessageType.Error) {
						expectedIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image;
					} else {
						expectedIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
					}

					if (warningIcon.image != expectedIcon) {
						warningIcon.image = expectedIcon;
					}

					contextWarning.style.display = DisplayStyle.Flex;
					warningIcon.style.display = DisplayStyle.Flex;
				} else {
					contextWarning.style.display = DisplayStyle.None;
					warningIcon.style.display = DisplayStyle.None;
				}
			}

			bool currentIsUI = GetCurrentIsUI();
			var validChoices = GetValidAnimTypes(currentIsUI);
			string currentTypeStr = step.type.ToString();

			if (!validChoices.Contains(currentTypeStr)) {
				validChoices.Add(currentTypeStr);
			}

			var typeDropdown = new DropdownField("Type", validChoices, currentTypeStr);
			typeDropdown.AddToClassList("unity-base-field");
			typeDropdown.AddToClassList("unity-base-field__aligned");

			typeDropdown.RegisterValueChangedCallback(evt => {
				if (System.Enum.TryParse<AnimType>(evt.newValue, out var newType)) {
					Undo.RecordObject(_sequencer, "Change Step Type");
					step.type = newType;

					stepProp.FindPropertyRelative("type").enumValueIndex = (int)newType;
					serializedObject.ApplyModifiedProperties();

					Color c = GetAnimTypeColor(step.type);

					if (step.enabled) {
						colorBar.style.backgroundColor = new StyleColor(c);
					} else {
						colorBar.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f));
					}

					infoLabel.text = BuildStepTypeInfo(step);

					if (step.type == AnimType.Anchor) {
						colorBar.parent.style.backgroundColor = new StyleColor(new Color(ColorAnchor.r, ColorAnchor.g, ColorAnchor.b, 0.4f));
					} else {
						colorBar.parent.style.backgroundColor = new StyleColor(BgStep);
					}

					if (IsModeHidden(step.type)) {
						modeEl.style.display = DisplayStyle.None;
					} else {
						modeEl.style.display = DisplayStyle.Flex;
					}

					RefreshStepBodyVisibility(body, step);
					BuildTypeFields(typeFieldsContainer, seqIndex, stepIndex, infoLabel, body);
					UpdateContextWarning();
				}
			});

			body.Add(typeDropdown);

			var durationField = MakeBoundField(stepProp.FindPropertyRelative("duration"), "Duration");
			var delayField = MakeBoundField(stepProp.FindPropertyRelative("delay"), "Delay");
			var easeField = MakeBoundField(stepProp.FindPropertyRelative("ease"), "Ease");
			var customCurveField = MakeBoundField(stepProp.FindPropertyRelative("customCurve"), "Custom Curve");

			var fromCurrentProp = stepProp.FindPropertyRelative("animateFromCurrent");
			var (fromCurrentField, fromCurrentPill) = MakeToggleField(fromCurrentProp, "From Current", () => step.animateFromCurrent);
			fromCurrentField.name = "fromCurrentField";

			fromCurrentPill.onValueChanged += () => {
				UpdateFromCurrentVisibility(body, step);
			};

			var targetField = MakeTargetField(stepProp.FindPropertyRelative("target"), "Target Transform");

			durationField.name = "durationField";
			delayField.name = "delayField";
			easeField.name = "easeField";
			customCurveField.name = "customCurveField";
			targetField.name = "targetField";

			durationField.RegisterValueChangeCallback(evt => {
				step.duration = evt.changedProperty.floatValue;
				infoLabel.text = BuildStepTypeInfo(step);
			});

			delayField.RegisterValueChangeCallback(evt => {
				step.delay = evt.changedProperty.floatValue;
				infoLabel.text = BuildStepTypeInfo(step);
			});

			easeField.RegisterValueChangeCallback(evt => {
				step.ease = (PrimeTween.Ease)evt.changedProperty.intValue;
				RefreshStepBodyVisibility(body, step);
			});

			targetField.RegisterValueChangeCallback(_ => {
				bool newIsUI = GetCurrentIsUI();
				var newChoices = GetValidAnimTypes(newIsUI);
				string typeStr = step.type.ToString();

				if (!newChoices.Contains(typeStr)) {
					newChoices.Add(typeStr);
				}

				typeDropdown.choices = newChoices;
				typeDropdown.value = typeStr;

				UpdateContextWarning();
				BuildTypeFields(typeFieldsContainer, seqIndex, stepIndex, infoLabel, body);
			});

			body.Add(durationField);
			body.Add(delayField);
			body.Add(easeField);
			body.Add(customCurveField);
			body.Add(fromCurrentField);
			body.Add(targetField);
			body.Add(Spacer(4));

			BuildTypeFields(typeFieldsContainer, seqIndex, stepIndex, infoLabel, body);
			body.Add(typeFieldsContainer);

			UpdateContextWarning();
			body.Add(Spacer(4));
			body.Add(contextWarning);

			RefreshStepBodyVisibility(body, step);

			body.schedule.Execute(() => {
				if (step == null) {
					return;
				}

				string newInfo = BuildStepTypeInfo(step);

				if (infoLabel.text != newInfo) {
					infoLabel.text = newInfo;
				}

				string newMode = "PAR";

				if (step.mode == StepMode.Sequential) {
					newMode = "SEQ";
				}

				if (modeEl.text != newMode) {
					modeEl.text = newMode;

					if (step.mode == StepMode.Sequential) {
						modeEl.style.color = new StyleColor(ColorSeq);
					} else {
						modeEl.style.color = new StyleColor(ColorPar);
					}
				}

				UpdateContextWarning();
			}).Every(100);
		}

		void UpdateFromCurrentVisibility(VisualElement body, AnimStep step) {
			var fromField = body.Q<PropertyField>("fromField");

			if (fromField != null) {
				if (step.animateFromCurrent) {
					fromField.style.display = DisplayStyle.None;
				} else {
					fromField.style.display = DisplayStyle.Flex;
				}
			}
		}

		void UpdateToLabelVisibility(VisualElement body, AnimStep step) {
			body.Query<PropertyField>("toField").ForEach(f => {
				if (step.relativeOffset) {
					f.label = "To Offset";
				} else {
					f.label = "To";
				}
			});

			body.Query<PropertyField>("fromField").ForEach(f => {
				if (step.relativeOffset) {
					f.label = "From Offset";
				} else {
					f.label = "From";
				}
			});
		}

		void RefreshStepBodyVisibility(VisualElement body, AnimStep step) {
			SetVisible(body, "durationField", !IsDurationHidden(step.type));
			SetVisible(body, "delayField", !IsDelayHidden(step.type));
			SetVisible(body, "easeField", !IsEaseHidden(step.type));
			SetVisible(body, "customCurveField", !IsEaseHidden(step.type) && step.ease == PrimeTween.Ease.Custom);
			SetVisible(body, "fromCurrentField", !IsFromCurrentHidden(step));
			SetVisible(body, "targetField", !IsTargetHidden(step.type));
			SetVisible(body, "modeField", !IsModeHidden(step.type));

			UpdateFromCurrentVisibility(body, step);
			UpdateToLabelVisibility(body, step);
		}

		VisualElement MakeRelativeToggle(SerializedProperty sp, AnimStep step, Label infoLabel, VisualElement body) {
			var (relRow, relPill) = MakeToggleField(sp.FindPropertyRelative("relativeOffset"), "Relative Offset", () => step.relativeOffset);

			relPill.onValueChanged += () => {
				RefreshStepBodyVisibility(body, step);
				infoLabel.text = BuildStepTypeInfo(step);
			};

			return relRow;
		}

		void BuildTypeFields(VisualElement c, int seqIndex, int stepIndex, Label infoLabel, VisualElement body) {
			c.Clear();
			var step = _sequencer.sequences[seqIndex].steps[stepIndex];
			var sp = GetStepProp(seqIndex, stepIndex);
			bool fc = step.animateFromCurrent;
			bool isUI = IsStepUI(step);

			void Add(SerializedProperty prop, string label, bool isFromField = false, bool isToField = false) {
				bool isRef = prop.propertyType == SerializedPropertyType.ObjectReference;
				string initLabel = label;

				if (isRef && prop.objectReferenceValue == null) {
					initLabel = $"{label} [Self]";
				}

				var f = new PropertyField(prop, initLabel);
				f.Bind(serializedObject);

				if (isRef) {
					f.RegisterValueChangeCallback(evt => {
						if (evt.changedProperty.objectReferenceValue == null) {
							f.label = $"{label} [Self]";
						} else {
							f.label = label;
						}
					});
				}

				if (isFromField) {
					f.name = "fromField";

					// GEÄNDERT: step.relativeOffset entfernt!
					if (fc) {
						f.style.display = DisplayStyle.None;
					} else {
						f.style.display = DisplayStyle.Flex;
					}
				}

				if (isToField) {
					f.name = "toField";
				}

				c.Add(f);
			}

			switch (step.type) {
				case AnimType.Fade:
					Add(sp.FindPropertyRelative("fadeFrom"), "From", true);
					Add(sp.FindPropertyRelative("fadeTo"), "To");
					break;
				case AnimType.Scale:
					c.Add(MakeRelativeToggle(sp, step, infoLabel, body));
					if (isUI) {
						Add(sp.FindPropertyRelative("scaleFrom"), "From", true);
						Add(sp.FindPropertyRelative("scaleTo"), "To", false, true);
					} else {
						Add(sp.FindPropertyRelative("scaleFrom3D"), "From", true);
						Add(sp.FindPropertyRelative("scaleTo3D"), "To", false, true);
					}
					break;
				case AnimType.Slide:
					c.Add(MakeRelativeToggle(sp, step, infoLabel, body));
					Add(sp.FindPropertyRelative("slideFrom"), "From", true);
					Add(sp.FindPropertyRelative("slideTo"), "To", false, true);
					break;
				case AnimType.Rotate:
					c.Add(MakeRelativeToggle(sp, step, infoLabel, body));
					Add(sp.FindPropertyRelative("rotateFrom"), "From", true);
					Add(sp.FindPropertyRelative("rotateTo"), "To", false, true);
					break;
				case AnimType.Bounce:
					if (isUI) {
						Add(sp.FindPropertyRelative("bounceIntensity"), "Intensity");
					} else {
						Add(sp.FindPropertyRelative("bounce3D"), "Bounce Vector");
					}
					Add(sp.FindPropertyRelative("bounceCount"), "Count");
					break;
				case AnimType.PunchRotate:
					if (isUI) {
						BuildPunchRotateFields(c, sp, step);
					} else {
						Add(sp.FindPropertyRelative("punchRotate3D"), "Punch Vector");
					}
					break;
				case AnimType.PunchScale:
					if (isUI) {
						Add(sp.FindPropertyRelative("punchScaleIntensity"), "Intensity");
					} else {
						Add(sp.FindPropertyRelative("punchScale3D"), "Punch Vector");
					}
					Add(sp.FindPropertyRelative("punchScaleFrequency"), "Frequency");
					break;
				case AnimType.ColorTint:
					Add(sp.FindPropertyRelative("colorTarget"), "Color Target");
					Add(sp.FindPropertyRelative("colorFrom"), "From", true);
					Add(sp.FindPropertyRelative("colorTo"), "To");
					break;
				case AnimType.FadeSpriteColor:
					Add(sp.FindPropertyRelative("spriteTarget"), "Sprite Target");
					Add(sp.FindPropertyRelative("colorFrom"), "From", true);
					Add(sp.FindPropertyRelative("colorTo"), "To");
					break;
				case AnimType.SetColor:
					Add(sp.FindPropertyRelative("colorTarget"), "Color Target");
					Add(sp.FindPropertyRelative("colorTo"), "Color");
					break;
				case AnimType.SetFade:
					Add(sp.FindPropertyRelative("setFadeValue"), "Alpha");
					break;
				case AnimType.SetSprite:
					Add(sp.FindPropertyRelative("spriteTarget"), "Sprite Target");
					Add(sp.FindPropertyRelative("setSpriteValue"), "New Sprite");
					break;
				case AnimType.SetImage:
					Add(sp.FindPropertyRelative("imageTarget"), "Image Target");
					Add(sp.FindPropertyRelative("setSpriteValue"), "New Sprite");
					break;
				case AnimType.TypeWriter:
					Add(sp.FindPropertyRelative("tmpTarget"), "TMP Target");
					Add(sp.FindPropertyRelative("setTextValue"), "Text String");
					Add(sp.FindPropertyRelative("typeWriterCharsPerSecond"), "Chars Per Second");
					break;
				case AnimType.TextCounter:
					BuildTextCounterFields(c, sp, step, seqIndex, stepIndex);
					break;
				case AnimType.SetTransform:
					BuildSetTransformFields(c, sp, step, infoLabel, body);
					break;
				case AnimType.SetText:
					Add(sp.FindPropertyRelative("tmpTarget"), "TMP Target");
					Add(sp.FindPropertyRelative("setTextValue"), "Text Value");
					break;
				case AnimType.SetActive:
					var (actRow, actPill) = MakeToggleField(sp.FindPropertyRelative("setActiveValue"), "Set Active", () => step.setActiveValue);

					actPill.onValueChanged += () => {
						infoLabel.text = BuildStepTypeInfo(step);
					};

					c.Add(actRow);
					break;
				case AnimType.Trigger:
					var seqField = MakeTargetField(sp.FindPropertyRelative("triggerSequencer"), "Sequencer");

					seqField.RegisterValueChangeCallback(evt => {
						step.triggerSequencer = evt.changedProperty.objectReferenceValue as AnimSequencer;
						infoLabel.text = BuildStepTypeInfo(step);
					});

					c.Add(seqField);

					var lblField = new PropertyField(sp.FindPropertyRelative("triggerSequenceLabel"), "Sequence Label");
					lblField.Bind(serializedObject);

					lblField.RegisterValueChangeCallback(evt => {
						step.triggerSequenceLabel = evt.changedProperty.stringValue;
						infoLabel.text = BuildStepTypeInfo(step);
					});

					c.Add(lblField);
					break;
				case AnimType.Event:
					var eventField = new PropertyField(sp.FindPropertyRelative("onEvent"), "On Event");
					eventField.Bind(serializedObject);
					c.Add(eventField);
					break;
				case AnimType.Anchor:
					var anchorField = new PropertyField(sp.FindPropertyRelative("anchorLabel"), "Anchor Name");
					anchorField.Bind(serializedObject);

					anchorField.RegisterValueChangeCallback(evt => {
						step.anchorLabel = evt.changedProperty.stringValue;
						infoLabel.text = BuildStepTypeInfo(step);
					});

					c.Add(anchorField);
					break;
				case AnimType.Repeat:
					var repField = new PropertyField(sp.FindPropertyRelative("repeatAnchorLabel"), "To Anchor");
					repField.Bind(serializedObject);

					repField.RegisterValueChangeCallback(evt => {
						step.repeatAnchorLabel = evt.changedProperty.stringValue;
						infoLabel.text = BuildStepTypeInfo(step);
					});

					c.Add(repField);
					break;
				case AnimType.WaitUntil:
					var (waitRow, waitPill) = MakeToggleField(sp.FindPropertyRelative("waitUntilValue"), "Condition Met", () => step.waitUntilValue);

					waitPill.onValueChanged += () => {
						infoLabel.text = BuildStepTypeInfo(step);
					};

					c.Add(waitRow);

					bool currentPillVisual = step.waitUntilValue;

					waitRow.schedule.Execute(() => {
						if (!Application.isPlaying || step == null) {
							return;
						}

						bool isMet = step.waitUntilValue;

						if (step.waitConditionLambda != null) {
							isMet = isMet || step.waitConditionLambda.Invoke();
						}

						if (currentPillVisual != isMet) {
							currentPillVisual = isMet;
							waitPill.SetValue(isMet);
						}
					}).Every(50);
					break;
				case AnimType.Wait:
					var waitMethodField = new PropertyField(sp.FindPropertyRelative("waitMethod"), "Wait Method");
					waitMethodField.Bind(serializedObject);
					c.Add(waitMethodField);

					var durField = new PropertyField(sp.FindPropertyRelative("duration"), "Duration (Seconds)");
					durField.Bind(serializedObject);
					c.Add(durField);

					var framesField = new PropertyField(sp.FindPropertyRelative("waitFrames"), "Frames");
					framesField.Bind(serializedObject);
					c.Add(framesField);

					void RefreshWait() {
						bool isFrames = step.waitMethod == WaitMethod.Frames;

						if (isFrames) {
							durField.style.display = DisplayStyle.None;
							framesField.style.display = DisplayStyle.Flex;
						} else {
							durField.style.display = DisplayStyle.Flex;
							framesField.style.display = DisplayStyle.None;
						}
					}

					RefreshWait();

					waitMethodField.RegisterValueChangeCallback(evt => {
						step.waitMethod = (WaitMethod)evt.changedProperty.enumValueIndex;
						RefreshWait();
						infoLabel.text = BuildStepTypeInfo(step);
					});

					break;
			}

			UpdateFromCurrentVisibility(body, step);
			UpdateToLabelVisibility(body, step);
		}

		void BuildPunchRotateFields(VisualElement c, SerializedProperty sp, AnimStep step) {
			var (randomRow, randomToggle) = MakeToggleField(sp.FindPropertyRelative("punchRotateRandom"), "Random Angle", () => step.punchRotateRandom);
			c.Add(randomRow);

			var freqField = new PropertyField(sp.FindPropertyRelative("punchRotateFrequency"), "Frequency");
			freqField.Bind(serializedObject);
			c.Add(freqField);

			var a1 = new PropertyField(sp.FindPropertyRelative("punchRotateAngle1"), "Angle 1");
			a1.Bind(serializedObject);
			c.Add(a1);

			var a2 = new PropertyField(sp.FindPropertyRelative("punchRotateAngle2"), "Angle 2");
			a2.Bind(serializedObject);
			c.Add(a2);

			var a = new PropertyField(sp.FindPropertyRelative("punchRotateAngle"), "Angle");
			a.Bind(serializedObject);
			c.Add(a);

			void Refresh() {
				bool isRandom = step.punchRotateRandom;

				if (isRandom) {
					a1.style.display = DisplayStyle.Flex;
					a2.style.display = DisplayStyle.Flex;
					a.style.display = DisplayStyle.None;
				} else {
					a1.style.display = DisplayStyle.None;
					a2.style.display = DisplayStyle.None;
					a.style.display = DisplayStyle.Flex;
				}
			}

			Refresh();
			randomToggle.onValueChanged += Refresh;
		}

		void BuildTextCounterFields(VisualElement c, SerializedProperty sp, AnimStep step, int seqIndex, int stepIndex) {
			var tmpField = new PropertyField(sp.FindPropertyRelative("tmpTarget"), "TMP Target");
			tmpField.Bind(serializedObject);
			c.Add(tmpField);

			var (fromCurrentRow, fromCurrentToggle) = MakeToggleField(sp.FindPropertyRelative("animateFromCurrent"), "From Current", () => step.animateFromCurrent);
			c.Add(fromCurrentRow);

			var fromField = new PropertyField(GetStepProp(seqIndex, stepIndex).FindPropertyRelative("textCounterFrom"), "From");
			fromField.name = "fromField";
			fromField.Bind(serializedObject);
			c.Add(fromField);

			void Refresh() {
				if (step.animateFromCurrent) {
					fromField.style.display = DisplayStyle.None;
				} else {
					fromField.style.display = DisplayStyle.Flex;
				}
			}

			Refresh();
			fromCurrentToggle.onValueChanged += Refresh;

			var toField = new PropertyField(sp.FindPropertyRelative("textCounterTo"), "To");
			toField.Bind(serializedObject);
			c.Add(toField);

			var fmtField = new PropertyField(sp.FindPropertyRelative("textCounterFormat"), "Format");
			fmtField.Bind(serializedObject);
			c.Add(fmtField);

			var (roundRow, _) = MakeToggleField(sp.FindPropertyRelative("textCounterRoundToInt"), "Round To Int", () => step.textCounterRoundToInt);
			c.Add(roundRow);
		}

		void BuildSetTransformFields(VisualElement c, SerializedProperty sp, AnimStep step, Label infoLabel, VisualElement body) {
			c.Add(MakeRelativeToggle(sp, step, infoLabel, body));

			var subField = new PropertyField(sp.FindPropertyRelative("transformSubType"), "Sub Type");
			subField.Bind(serializedObject);
			c.Add(subField);

			var valContainer = new VisualElement();
			c.Add(valContainer);

			void Rebuild() {
				valContainer.Clear();
				string lbl = "Value";

				if (step.relativeOffset) {
					lbl = "Offset";
				} else {
					switch (step.transformSubType) {
						case TransformSubType.LocalPosition:
							lbl = "Position";
							break;
						case TransformSubType.LocalRotation:
							lbl = "Rotation";
							break;
						case TransformSubType.LocalScale:
							lbl = "Scale";
							break;
					}
				}

				var vf = new PropertyField(sp.FindPropertyRelative("setTransformValue"), lbl);
				vf.Bind(serializedObject);
				valContainer.Add(vf);
			}

			Rebuild();

			subField.RegisterValueChangeCallback(evt => {
				step.transformSubType = (TransformSubType)evt.changedProperty.enumValueIndex;
				Rebuild();
				infoLabel.text = BuildStepTypeInfo(step);
			});
		}

		PropertyField MakeTargetField(SerializedProperty prop, string baseLabel) {
			string initLabel = baseLabel;

			if (prop.objectReferenceValue == null) {
				initLabel = $"{baseLabel} [Self]";
			}

			var f = new PropertyField(prop, initLabel);
			f.Bind(serializedObject);

			f.RegisterValueChangeCallback(evt => {
				if (evt.changedProperty.objectReferenceValue == null) {
					f.label = $"{baseLabel} [Self]";
				} else {
					f.label = baseLabel;
				}
			});

			return f;
		}

		static bool IsInstantType(AnimType t) {
			return t == AnimType.SetTransform ||
				   t == AnimType.SetText ||
				   t == AnimType.SetColor ||
				   t == AnimType.SetActive ||
				   t == AnimType.Trigger ||
				   t == AnimType.Event ||
				   t == AnimType.SetSprite ||
				   t == AnimType.SetImage ||
				   t == AnimType.SetFade;
		}

		static bool IsLogicType(AnimType t) {
			return t == AnimType.Anchor ||
				   t == AnimType.Repeat ||
				   t == AnimType.WaitUntil;
		}

		static bool IsModeHidden(AnimType t) {
			return t == AnimType.Anchor;
		}

		static bool IsDelayHidden(AnimType t) {
			return t == AnimType.Anchor;
		}

		static bool IsDurationHidden(AnimType t) {
			return IsInstantType(t) || t == AnimType.TypeWriter || IsLogicType(t) || t == AnimType.Wait;
		}

		static bool IsEaseHidden(AnimType t) {
			return IsInstantType(t) ||
				   t == AnimType.Wait ||
				   t == AnimType.Bounce ||
				   t == AnimType.PunchRotate ||
				   t == AnimType.PunchScale ||
				   IsLogicType(t);
		}

		static bool IsFromCurrentHidden(AnimStep step) {
			AnimType t = step.type;

			return t == AnimType.Wait ||
				   t == AnimType.Bounce ||
				   t == AnimType.PunchRotate ||
				   t == AnimType.PunchScale ||
				   t == AnimType.TypeWriter ||
				   t == AnimType.TextCounter ||
				   IsInstantType(t) ||
				   IsLogicType(t);
		}

		static bool IsTargetHidden(AnimType t) {
			return t == AnimType.Wait ||
				   t == AnimType.TypeWriter ||
				   t == AnimType.TextCounter ||
				   t == AnimType.SetText ||
				   t == AnimType.Trigger ||
				   t == AnimType.Event ||
				   t == AnimType.SetSprite ||
				   t == AnimType.FadeSpriteColor ||
				   t == AnimType.SetImage ||
				   IsLogicType(t);
		}

		static bool IsInteractableTrigger(TriggerType t) {
			return t == TriggerType.OnBecameInteractable || t == TriggerType.OnBecameNonInteractable;
		}

		static string Dur(float f) {
			return f.ToString("0.##", CultureInfo.InvariantCulture) + "s";
		}

		string BuildStepTypeInfo(AnimStep step) {
			string delay = "";

			if (step.delay > 0f) {
				delay = $"  +{Dur(step.delay)}";
			}

			string rel = "";

			if (step.relativeOffset && (step.type == AnimType.Slide || step.type == AnimType.Scale || step.type == AnimType.Rotate || step.type == AnimType.SetTransform)) {
				rel = " Relative";
			}

			if (step.type == AnimType.Trigger) {
				var targetSeq = _sequencer;

				if (step.triggerSequencer != null) {
					targetSeq = step.triggerSequencer;
				}

				bool exists = string.IsNullOrEmpty(step.triggerSequenceLabel) || (targetSeq != null && targetSeq.sequences.Exists(s => s.label == step.triggerSequenceLabel));
				string lbl = "None";

				if (!string.IsNullOrEmpty(step.triggerSequenceLabel)) {
					lbl = step.triggerSequenceLabel;
				}

				if (!exists) {
					lbl = $"<color=#ff5555>{lbl}</color>";
				}

				string targetName = "Self";

				if (step.triggerSequencer != null) {
					targetName = step.triggerSequencer.name;
				}

				return $"<b>Trigger</b> ({targetName} → {lbl}){delay}";
			}

			switch (step.type) {
				case AnimType.SetTransform:
					return $"<b>{step.type}{rel}</b> ({step.transformSubType}){delay}";
				case AnimType.SetText:
					return $"<b>SetText</b>{delay}";
				case AnimType.SetColor:
					return $"<b>SetColor</b>{delay}";
				case AnimType.SetSprite:
					return $"<b>SetSprite</b>{delay}";
				case AnimType.SetImage:
					return $"<b>SetImage</b>{delay}";
				case AnimType.SetFade:
					return $"<b>SetFade</b>{delay}";
				case AnimType.SetActive:
					string state = "Off";
					if (step.setActiveValue) {
						state = "On";
					}
					return $"<b>SetActive</b> ({state}){delay}";
				case AnimType.TypeWriter:
					return $"<b>TypeWriter</b>{delay}";
				case AnimType.Wait:
					if (step.waitMethod == WaitMethod.Frames) {
						return $"<b>Wait</b>  {step.waitFrames} Frames{delay}";
					} else {
						return $"<b>Wait</b>  {Dur(step.duration)}{delay}";
					}
				case AnimType.Event:
					return $"<b>Event</b>{delay}";
				case AnimType.Anchor:
					return $"<b><color=#888888>#</color><color=#ffffff>{step.anchorLabel}</color></b>";
				case AnimType.Repeat:
					return $"<b>Repeat</b> (→ <color=#888888>#</color><color=#ffffff>{step.repeatAnchorLabel}</color>){delay}";
				case AnimType.WaitUntil:
					return $"<b>WaitUntil</b>{delay}";
				default:
					return $"<b>{step.type}{rel}</b>  {Dur(step.duration)}{delay}";
			}
		}

		Color GetAnimTypeColor(AnimType type) {
			switch (type) {
				case AnimType.Fade: return ColorFade;
				case AnimType.Scale: return ColorScale;
				case AnimType.Slide: return ColorSlide;
				case AnimType.Rotate: return ColorRotate;
				case AnimType.Bounce: return ColorBounce;
				case AnimType.PunchRotate: return ColorPunchRotate;
				case AnimType.PunchScale: return ColorPunchScale;
				case AnimType.ColorTint: return ColorColorTint;
				case AnimType.SetColor: return ColorSetColor;
				case AnimType.TypeWriter: return ColorTypeWriter;
				case AnimType.TextCounter: return ColorTextCounter;
				case AnimType.SetTransform: return ColorSetTransform;
				case AnimType.Wait: return ColorWait;
				case AnimType.SetText: return ColorSetText;
				case AnimType.SetActive: return ColorSetActive;
				case AnimType.Trigger: return ColorTrigger;
				case AnimType.Event: return ColorEvent;
				case AnimType.SetSprite: return ColorSprite;
				case AnimType.FadeSpriteColor: return ColorSprite;
				case AnimType.SetImage: return ColorSetColor;
				case AnimType.SetFade: return ColorSetFade;
				case AnimType.Anchor: return ColorAnchor;
				case AnimType.Repeat: return ColorRepeat;
				case AnimType.WaitUntil: return ColorWaitUntil;
				default: return Color.gray;
			}
		}

		PropertyField MakeBoundField(SerializedProperty prop, string label) {
			var f = new PropertyField(prop, label);
			f.Bind(serializedObject);
			return f;
		}

		(VisualElement row, PillToggle toggle) MakeToggleField(SerializedProperty prop, string label, System.Func<bool> getValue) {
			var row = new VisualElement();
			row.AddToClassList("unity-base-field");
			row.AddToClassList("unity-base-field__aligned");
			row.style.minHeight = 22;

			var lbl = new Label(label);
			lbl.style.width = Length.Percent(45);
			lbl.style.minWidth = 127;
			lbl.style.paddingLeft = 3;
			lbl.style.color = new StyleColor(new Color(0.78f, 0.78f, 0.78f));

			var inputContainer = new VisualElement();
			inputContainer.style.flexGrow = 1;
			inputContainer.style.flexDirection = FlexDirection.Row;
			inputContainer.style.alignItems = Align.Center;

			var pill = new PillToggle(getValue(), ToggleOnBg, ToggleOffBg);
			pill.style.marginLeft = -20;

			pill.onClicked += () => {
				bool newVal = !getValue();
				Undo.RecordObject(_sequencer, $"Toggle {label}");
				prop.boolValue = newVal;
				prop.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(_sequencer);
				pill.SetValue(newVal);

				if (pill.onValueChanged != null) {
					pill.onValueChanged.Invoke();
				}
			};

			inputContainer.Add(pill);
			row.Add(lbl);
			row.Add(inputContainer);

			return (row, pill);
		}

		Button MakeSmallButton(string text, int width, System.Action onClick) {
			var btn = new Button(onClick);
			btn.text = text;
			btn.style.width = width;
			btn.style.height = 20;
			ApplyNeonButtonStyle(btn);
			return btn;
		}

		static void SetVisible(VisualElement parent, string name, bool visible) {
			var el = parent.Q<VisualElement>(name);

			if (el != null) {
				if (visible) {
					el.style.display = DisplayStyle.Flex;
				} else {
					el.style.display = DisplayStyle.None;
				}
			}
		}

		VisualElement CreateBox(int radius, Color borderColor) {
			var box = new VisualElement();
			box.style.borderTopWidth = 1;
			box.style.borderBottomWidth = 1;
			box.style.borderLeftWidth = 1;
			box.style.borderRightWidth = 1;
			box.style.borderTopColor = new StyleColor(borderColor);
			box.style.borderBottomColor = new StyleColor(borderColor);
			box.style.borderLeftColor = new StyleColor(borderColor);
			box.style.borderRightColor = new StyleColor(borderColor);
			box.style.borderTopLeftRadius = radius;
			box.style.borderTopRightRadius = radius;
			box.style.borderBottomLeftRadius = radius;
			box.style.borderBottomRightRadius = radius;
			box.style.overflow = Overflow.Hidden;
			return box;
		}

		VisualElement Spacer(int height) {
			var s = new VisualElement();
			s.style.height = height;
			return s;
		}

		void ApplyNeonButtonStyle(VisualElement btn, bool isAccent = false) {
			btn.style.backgroundColor = new StyleColor(ButtonBg);
			btn.style.color = new StyleColor(Color.white);
			btn.style.borderTopColor = new StyleColor(ButtonBorder);
			btn.style.borderBottomColor = new StyleColor(ButtonBorder);
			btn.style.borderLeftColor = new StyleColor(ButtonBorder);
			btn.style.borderRightColor = new StyleColor(ButtonBorder);
			btn.style.borderTopWidth = 1;
			btn.style.borderBottomWidth = 1;
			btn.style.borderLeftWidth = 1;
			btn.style.borderRightWidth = 1;
			btn.style.borderTopLeftRadius = 3;
			btn.style.borderTopRightRadius = 3;
			btn.style.borderBottomLeftRadius = 3;
			btn.style.borderBottomRightRadius = 3;

			btn.RegisterCallback<MouseOverEvent>(e => {
				btn.style.backgroundColor = new StyleColor(ButtonHoverBg);

				if (isAccent) {
					btn.style.borderTopColor = new StyleColor(ButtonAccent);
					btn.style.borderBottomColor = new StyleColor(ButtonAccent);
					btn.style.borderLeftColor = new StyleColor(ButtonAccent);
					btn.style.borderRightColor = new StyleColor(ButtonAccent);
				}
			});

			btn.RegisterCallback<MouseOutEvent>(e => {
				btn.style.backgroundColor = new StyleColor(ButtonBg);
				btn.style.borderTopColor = new StyleColor(ButtonBorder);
				btn.style.borderBottomColor = new StyleColor(ButtonBorder);
				btn.style.borderLeftColor = new StyleColor(ButtonBorder);
				btn.style.borderRightColor = new StyleColor(ButtonBorder);
			});
		}

		class PillToggle : VisualElement {
			public System.Action onClicked;
			public System.Action onValueChanged;

			readonly VisualElement _pill;
			readonly VisualElement _knob;
			readonly Color _onBg;
			readonly Color _offBg;

			public PillToggle(bool value, Color onBg, Color offBg) {
				_onBg = onBg;
				_offBg = offBg;

				_pill = new VisualElement();
				_pill.style.width = 30;
				_pill.style.height = 14;
				_pill.style.borderTopLeftRadius = 7;
				_pill.style.borderTopRightRadius = 7;
				_pill.style.borderBottomLeftRadius = 7;
				_pill.style.borderBottomRightRadius = 7;
				_pill.style.flexShrink = 0;
				_pill.style.position = Position.Relative;

				_knob = new VisualElement();
				_knob.style.width = 10;
				_knob.style.height = 10;
				_knob.style.borderTopLeftRadius = 5;
				_knob.style.borderTopRightRadius = 5;
				_knob.style.borderBottomLeftRadius = 5;
				_knob.style.borderBottomRightRadius = 5;
				_knob.style.backgroundColor = new StyleColor(Color.white);
				_knob.style.position = Position.Absolute;
				_knob.style.top = 2;

				_pill.Add(_knob);
				Add(_pill);
				SetValue(value);

				RegisterCallback<ClickEvent>(evt => {
					evt.StopPropagation();

					if (onClicked != null) {
						onClicked.Invoke();
					}
				});
			}

			public void SetValue(bool value) {
				if (value) {
					_pill.style.backgroundColor = new StyleColor(_onBg);
					_knob.style.left = 18;
				} else {
					_pill.style.backgroundColor = new StyleColor(_offBg);
					_knob.style.left = 2;
				}
			}
		}
	}
}
#endif