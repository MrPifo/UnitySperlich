#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Globalization;
using System.Collections.Generic;
using static Sperlich.Sequencer.AnimSequencer;
using TMPro;

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
		static readonly Color ColorSizeDelta = new Color(0.25f, 0.85f, 0.75f);
		static readonly Color ColorFill = new Color(0.40f, 0.70f, 0.90f);

		static readonly Color ColorBounce = new Color(0.95f, 0.90f, 0.30f);
		static readonly Color ColorPunchRotate = new Color(0.95f, 0.45f, 0.20f);
		static readonly Color ColorPunchScale = new Color(0.95f, 0.30f, 0.60f);
		static readonly Color ColorShake = new Color(0.95f, 0.60f, 0.20f);

		static readonly Color ColorFade = new Color(0.50f, 0.60f, 0.95f);
		static readonly Color ColorColorTint = new Color(0.90f, 0.20f, 0.50f);
		static readonly Color ColorSprite = new Color(0.40f, 0.85f, 0.60f);
		static readonly Color ColorAudio = new Color(0.85f, 0.40f, 0.95f);
		static readonly Color ColorMaterial = new Color(0.20f, 0.95f, 0.60f);
		static readonly Color ColorTimeScale = new Color(0.90f, 0.90f, 0.90f);

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

		void OnEnable() { Undo.undoRedoPerformed += OnUndoRedoPerformed; }
		void OnDisable() { Undo.undoRedoPerformed -= OnUndoRedoPerformed; }

		void OnUndoRedoPerformed() {
			if (_root == null || _sequencer == null) return;
			serializedObject.Update();
			BuildUI(_root);
		}

		bool IsSequencerUI() { return _sequencer != null && _sequencer.GetComponent<RectTransform>() != null; }
		bool IsStepUI(AnimStep step) { return step.target != null ? step.target is RectTransform : IsSequencerUI(); }

		public override VisualElement CreateInspectorGUI() {
			_sequencer = (AnimSequencer)target;
			_root = new VisualElement(); _root.style.paddingBottom = 4;
			BuildUI(_root);
			return _root;
		}

		void BuildUI(VisualElement root) {
			root.Clear();
			root.Add(MakeHeader());
			root.Add(MakeCopyPasteRow(root));
			for (int i = 0; i < _sequencer.sequences.Count; i++) root.Add(BuildSequenceElement(i, root));
			root.Add(MakeAddSequenceButton(root));
		}

		VisualElement MakeHeader() {
			var container = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, backgroundColor = new StyleColor(BgDark), paddingTop = 6, paddingBottom = 6, paddingLeft = 8, paddingRight = 8, marginBottom = 4 } };
			var label = new Label("Anim Sequencer") { style = { fontSize = 13, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white } };
			bool isUI = IsSequencerUI();
			var badge = new Label(isUI ? "UI [RectTransform]" : "World [Transform]") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, backgroundColor = new StyleColor(isUI ? new Color(0.15f, 0.45f, 0.85f) : new Color(0.85f, 0.2f, 0.2f)), paddingTop = 2, paddingBottom = 2, paddingLeft = 6, paddingRight = 6, borderTopLeftRadius = 3, borderTopRightRadius = 3, borderBottomLeftRadius = 3, borderBottomRightRadius = 3 } };
			container.Add(label); container.Add(badge); return container;
		}

		VisualElement MakeCopyPasteRow(VisualElement root) {
			var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4 } };
			var copy = new Button { text = "Copy All", style = { flexGrow = 1, height = 22 } }; ApplyNeonButtonStyle(copy);
			copy.clicked += () => { EditorGUIUtility.systemCopyBuffer = _sequencer.CopyToJson(); copy.text = "✓ Copied!"; copy.schedule.Execute(() => copy.text = "Copy All").StartingIn(1200); };
			var paste = new Button { text = (EditorApplication.timeSinceStartup - _pasteSuccessTime) < 1.2 ? "✓ Pasted!" : "Paste All", style = { flexGrow = 1, height = 22 } }; ApplyNeonButtonStyle(paste);
			paste.clicked += () => {
				string json = EditorGUIUtility.systemCopyBuffer;
				if (string.IsNullOrEmpty(json)) { paste.text = "⚠ Clipboard Empty"; paste.schedule.Execute(() => paste.text = "Paste All").StartingIn(1200); return; }
				try { Undo.RecordObject(_sequencer, "Paste Sequences"); _sequencer.PasteFromJson(json); EditorUtility.SetDirty(_sequencer); _pasteSuccessTime = EditorApplication.timeSinceStartup; BuildUI(root); } catch { paste.text = "⚠ Invalid Data"; paste.schedule.Execute(() => paste.text = "Paste All").StartingIn(1200); }
			};
			row.Add(copy); row.Add(paste); return row;
		}

		Button MakeAddSequenceButton(VisualElement root) {
			var btn = new Button(() => { Undo.RecordObject(_sequencer, "Add Sequence"); _sequencer.sequences.Add(new AnimSequence()); EditorUtility.SetDirty(_sequencer); BuildUI(root); }) { text = "+ Add Sequence", style = { height = 30, marginTop = 6 } };
			ApplyNeonButtonStyle(btn, true); return btn;
		}

		SerializedProperty GetSeqProp(int seqIndex) { serializedObject.Update(); return serializedObject.FindProperty("sequences").GetArrayElementAtIndex(seqIndex); }
		SerializedProperty GetStepProp(int seqIndex, int stepIndex) { return GetSeqProp(seqIndex).FindPropertyRelative("steps").GetArrayElementAtIndex(stepIndex); }

		VisualElement BuildSequenceElement(int seqIndex, VisualElement root) {
			var seq = _sequencer.sequences[seqIndex];
			var box = CreateBox(4, new Color(0.3f, 0.3f, 0.3f)); box.style.marginBottom = 6;
			var (headerRow, arrowLabel, titleLabel) = MakeSequenceHeader(seq, seqIndex, root); box.Add(headerRow);
			var body = new VisualElement { style = { paddingLeft = 8, paddingRight = 8, paddingTop = 6, paddingBottom = 6, display = seq.isExpanded ? DisplayStyle.Flex : DisplayStyle.None } };
			headerRow.RegisterCallback<ClickEvent>(evt => {
				if (evt.button != 0 || evt.target is Button || (evt.target as VisualElement)?.parent is Button) return;
				seq.isExpanded = !seq.isExpanded; body.style.display = seq.isExpanded ? DisplayStyle.Flex : DisplayStyle.None; arrowLabel.text = seq.isExpanded ? "▼" : "▶"; EditorUtility.SetDirty(_sequencer);
			});
			BuildSequenceBody(body, seq, seqIndex, root, titleLabel); box.Add(body); return box;
		}

		(VisualElement row, Label arrow, Label title) MakeSequenceHeader(AnimSequence seq, int seqIndex, VisualElement root) {
			var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, backgroundColor = new StyleColor(BgDark), paddingLeft = 4, paddingRight = 4, paddingTop = 3, paddingBottom = 3 } };
			row.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Sequence", a => EditorGUIUtility.systemCopyBuffer = "ANIMSEQ_SEQ:" + JsonUtility.ToJson(seq, true));
				evt.menu.AppendAction("Paste Sequence", a => {
					string clip = EditorGUIUtility.systemCopyBuffer;
					if (clip != null && clip.StartsWith("ANIMSEQ_SEQ:")) { Undo.RecordObject(_sequencer, "Paste Sequence"); _sequencer.sequences[seqIndex] = JsonUtility.FromJson<AnimSequence>(clip.Substring(12)); EditorUtility.SetDirty(_sequencer); BuildUI(root); }
				}, a => EditorGUIUtility.systemCopyBuffer != null && EditorGUIUtility.systemCopyBuffer.StartsWith("ANIMSEQ_SEQ:") ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));
			var arrow = new Label(seq.isExpanded ? "▼" : "▶") { style = { marginLeft = 4, marginRight = 4, fontSize = 9, width = 12, color = new StyleColor(new Color(0.7f, 0.7f, 0.7f)), unityTextAlign = TextAnchor.MiddleLeft } };
			var title = new Label(string.IsNullOrEmpty(seq.label) ? $"Sequence {seqIndex}" : seq.label) { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
			var playBtn = MakeSmallButton("▶", 22, () => { if (Application.isPlaying) _sequencer.PlayByLabel(seq.label); });
			var upBtn = MakeSmallButton("↑", 22, () => { if (seqIndex <= 0) return; Undo.RecordObject(_sequencer, "Move Sequence Up"); var temp = _sequencer.sequences[seqIndex - 1]; _sequencer.sequences[seqIndex - 1] = _sequencer.sequences[seqIndex]; _sequencer.sequences[seqIndex] = temp; EditorUtility.SetDirty(_sequencer); BuildUI(root); }); upBtn.SetEnabled(seqIndex > 0);
			var downBtn = MakeSmallButton("↓", 22, () => { if (seqIndex >= _sequencer.sequences.Count - 1) return; Undo.RecordObject(_sequencer, "Move Sequence Down"); var temp = _sequencer.sequences[seqIndex + 1]; _sequencer.sequences[seqIndex + 1] = _sequencer.sequences[seqIndex]; _sequencer.sequences[seqIndex] = temp; EditorUtility.SetDirty(_sequencer); BuildUI(root); }); downBtn.SetEnabled(seqIndex < _sequencer.sequences.Count - 1);
			var removeBtn = MakeSmallButton("✕", 22, () => { Undo.RecordObject(_sequencer, "Remove Sequence"); _sequencer.sequences.RemoveAt(seqIndex); EditorUtility.SetDirty(_sequencer); BuildUI(root); });
			row.Add(arrow); row.Add(title); row.Add(playBtn); row.Add(upBtn); row.Add(downBtn); row.Add(removeBtn); return (row, arrow, title);
		}

		void BuildSequenceBody(VisualElement body, AnimSequence seq, int seqIndex, VisualElement root, Label titleLabel) {
			var labelField = new PropertyField(GetSeqProp(seqIndex).FindPropertyRelative("label"), "Label"); labelField.Bind(serializedObject);
			labelField.RegisterValueChangeCallback(_ => titleLabel.text = string.IsNullOrEmpty(seq.label) ? $"Sequence {seqIndex}" : seq.label); body.Add(labelField);
			var (deactivateField, _) = MakeToggleField(GetSeqProp(seqIndex).FindPropertyRelative("deactivateAfter"), "Deactivate After", () => seq.deactivateAfter); deactivateField.style.display = seq.trigger == TriggerType.OnDisable ? DisplayStyle.Flex : DisplayStyle.None; body.Add(deactivateField);
			var selectableField = MakeTargetField(GetSeqProp(seqIndex).FindPropertyRelative("selectableTarget"), "Selectable"); selectableField.style.display = IsInteractableTrigger(seq.trigger) ? DisplayStyle.Flex : DisplayStyle.None;
			var triggerField = new PropertyField(GetSeqProp(seqIndex).FindPropertyRelative("trigger"), "Trigger"); triggerField.Bind(serializedObject);

			var triggerWarning = new HelpBox("", HelpBoxMessageType.Warning) { style = { display = DisplayStyle.None, marginTop = 4 } };
			void ValidateTrigger(TriggerType t) {
				if (!IsSequencerUI() && (t == TriggerType.OnBecameInteractable || t == TriggerType.OnBecameNonInteractable)) { triggerWarning.text = "Interactable Triggers require a UI Selectable component."; triggerWarning.style.display = DisplayStyle.Flex; } else if (!IsSequencerUI() && (t == TriggerType.OnClick || t == TriggerType.OnPointerEnter || t == TriggerType.OnPointerExit || t == TriggerType.OnPointerDown || t == TriggerType.OnPointerUp)) { triggerWarning.text = "Pointer Events on World objects require a Collider and PhysicsRaycaster."; triggerWarning.style.display = DisplayStyle.Flex; } else triggerWarning.style.display = DisplayStyle.None;
			}
			ValidateTrigger(seq.trigger);
			triggerField.RegisterValueChangeCallback(evt => {
				var newVal = (TriggerType)evt.changedProperty.enumValueIndex;
				deactivateField.style.display = newVal == TriggerType.OnDisable ? DisplayStyle.Flex : DisplayStyle.None;
				selectableField.style.display = IsInteractableTrigger(newVal) ? DisplayStyle.Flex : DisplayStyle.None;
				ValidateTrigger(newVal); AutoLabel(seq, seqIndex, titleLabel);
			});
			body.Add(triggerField); body.Add(triggerWarning); body.Add(deactivateField); body.Add(selectableField); body.Add(Spacer(6));

			var stepsContainer = new VisualElement(); body.Add(stepsContainer);
			void RebuildSteps() { serializedObject.Update(); stepsContainer.Clear(); for (int i = 0; i < seq.steps.Count; i++) stepsContainer.Add(BuildStepElement(seqIndex, i, RebuildSteps)); }
			RebuildSteps();

			var addStepBtn = new Button(() => { Undo.RecordObject(_sequencer, "Add Step"); seq.steps.Add(new AnimStep()); EditorUtility.SetDirty(_sequencer); RebuildSteps(); }) { text = "+ Add Step", style = { height = 22, marginTop = 4 } }; ApplyNeonButtonStyle(addStepBtn);
			body.Add(addStepBtn); body.Add(Spacer(6)); body.Add(MakeEventsSection(seq, seqIndex));
		}

		VisualElement MakeEventsSection(AnimSequence seq, int seqIndex) {
			var container = new VisualElement();
			var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f)), paddingLeft = 4, paddingTop = 3, paddingBottom = 3, borderTopLeftRadius = 3, borderTopRightRadius = 3 } };
			var arrow = new Label(seq.eventsExpanded ? "▼" : "▶") { style = { fontSize = 9, width = 12, marginRight = 4, color = new StyleColor(new Color(0.7f, 0.7f, 0.7f)) } };
			var title = new Label("Events") { style = { color = new StyleColor(new Color(0.75f, 0.75f, 0.75f)), fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } };
			headerRow.Add(arrow); headerRow.Add(title);
			var eventsBody = new VisualElement { style = { paddingTop = 4, paddingBottom = 4, display = seq.eventsExpanded ? DisplayStyle.Flex : DisplayStyle.None } };
			eventsBody.Add(MakeBoundField(GetSeqProp(seqIndex).FindPropertyRelative("onStart"), "On Start")); eventsBody.Add(MakeBoundField(GetSeqProp(seqIndex).FindPropertyRelative("onEnd"), "On End"));
			headerRow.RegisterCallback<ClickEvent>(_ => { seq.eventsExpanded = !seq.eventsExpanded; eventsBody.style.display = seq.eventsExpanded ? DisplayStyle.Flex : DisplayStyle.None; arrow.text = seq.eventsExpanded ? "▼" : "▶"; EditorUtility.SetDirty(_sequencer); });
			container.Add(headerRow); container.Add(eventsBody); return container;
		}

		void AutoLabel(AnimSequence seq, int seqIndex, Label titleLabel) {
			bool matchesTrigger = false;
			foreach (TriggerType t in System.Enum.GetValues(typeof(TriggerType))) if (seq.label == t.ToString()) { matchesTrigger = true; break; }
			if (!string.IsNullOrEmpty(seq.label) && !matchesTrigger) return;
			Undo.RecordObject(_sequencer, "Auto Label"); seq.label = seq.trigger.ToString();
			serializedObject.FindProperty("sequences").GetArrayElementAtIndex(seqIndex).FindPropertyRelative("label").stringValue = seq.label; serializedObject.ApplyModifiedProperties();
			titleLabel.text = seq.label;
		}

		VisualElement BuildStepElement(int seqIndex, int stepIndex, System.Action rebuild) {
			var seq = _sequencer.sequences[seqIndex]; var step = seq.steps[stepIndex]; Color typeColor = GetAnimTypeColor(step.type);
			var stepBox = CreateBox(3, new Color(0.28f, 0.28f, 0.28f)); stepBox.style.marginBottom = 4;
			var (stepHeader, colorBar, arrowLabel, infoLabel, tagLabel, modeEl, iconLabel, warningIcon) = MakeStepHeader(step, seqIndex, stepIndex, typeColor, seq, rebuild);
			stepBox.Add(stepHeader); stepBox.Add(MakeProgressBar(seqIndex, stepIndex, typeColor));
			var stepBody = new VisualElement { style = { paddingLeft = 8, paddingRight = 8, paddingTop = 6, paddingBottom = 6, backgroundColor = new StyleColor(BgStepBody), display = step.isExpanded ? DisplayStyle.Flex : DisplayStyle.None } };
			stepHeader.RegisterCallback<ClickEvent>(evt => {
				if (evt.button != 0 || evt.target is Button || (evt.target as VisualElement)?.parent is Button || evt.target is Toggle || (evt.target as VisualElement)?.parent is Toggle) return;
				step.isExpanded = !step.isExpanded; stepBody.style.display = step.isExpanded ? DisplayStyle.Flex : DisplayStyle.None; arrowLabel.text = step.isExpanded ? "▼" : "▶"; EditorUtility.SetDirty(_sequencer);
			});
			BuildStepBody(stepBody, seqIndex, stepIndex, colorBar, infoLabel, tagLabel, modeEl, iconLabel, warningIcon, step); stepBox.Add(stepBody); return stepBox;
		}

		(VisualElement header, VisualElement colorBar, Label arrow, Label info, Label tagLabel, Label modeEl, Label iconLabel, Image warningIcon)
		MakeStepHeader(AnimStep step, int seqIndex, int stepIndex, Color typeColor, AnimSequence seq, System.Action rebuild) {
			var header = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, minHeight = 24, backgroundColor = step.type == AnimType.Anchor ? new StyleColor(new Color(ColorAnchor.r, ColorAnchor.g, ColorAnchor.b, 0.4f)) : new StyleColor(BgStep) } };
			header.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Step", a => EditorGUIUtility.systemCopyBuffer = "ANIMSEQ_STEP:" + JsonUtility.ToJson(step, true));
				evt.menu.AppendAction("Paste Step", a => {
					string clip = EditorGUIUtility.systemCopyBuffer;
					if (clip != null && clip.StartsWith("ANIMSEQ_STEP:")) { Undo.RecordObject(_sequencer, "Paste Step"); seq.steps[stepIndex] = JsonUtility.FromJson<AnimStep>(clip.Substring(13)); EditorUtility.SetDirty(_sequencer); rebuild(); }
				}, a => EditorGUIUtility.systemCopyBuffer != null && EditorGUIUtility.systemCopyBuffer.StartsWith("ANIMSEQ_STEP:") ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));
			var colorBar = new VisualElement { style = { width = 5, alignSelf = Align.Stretch, backgroundColor = step.enabled ? new StyleColor(typeColor) : new StyleColor(new Color(0.4f, 0.4f, 0.4f)) } };
			var arrow = new Label(step.isExpanded ? "▼" : "▶") { style = { marginLeft = 6, marginRight = 4, fontSize = 9, width = 12, color = new StyleColor(new Color(0.7f, 0.7f, 0.7f)) } };
			var enableToggle = new Toggle { value = step.enabled, style = { marginRight = 6 } }; enableToggle.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
			void ApplyCheckmarkStyle(bool isChecked) { var c = enableToggle.Q<VisualElement>(className: "unity-toggle__checkmark"); if (c != null) { c.style.backgroundColor = new StyleColor(new Color(0.12f, 0.13f, 0.16f)); c.style.borderTopColor = new StyleColor(ButtonBorder); c.style.borderBottomColor = new StyleColor(ButtonBorder); c.style.borderLeftColor = new StyleColor(ButtonBorder); c.style.borderRightColor = new StyleColor(ButtonBorder); if (isChecked) c.style.unityBackgroundImageTintColor = new StyleColor(Color.white); } }
			enableToggle.schedule.Execute(() => ApplyCheckmarkStyle(enableToggle.value));
			enableToggle.RegisterValueChangedCallback(evt => { Undo.RecordObject(_sequencer, "Toggle Step Enabled"); step.enabled = evt.newValue; colorBar.style.backgroundColor = new StyleColor(step.enabled ? GetAnimTypeColor(step.type) : new Color(0.4f, 0.4f, 0.4f)); ApplyCheckmarkStyle(evt.newValue); EditorUtility.SetDirty(_sequencer); });
			var modeEl = new Label(step.mode == StepMode.Sequential ? "SEQ" : "PAR") { style = { color = new StyleColor(step.mode == StepMode.Sequential ? ColorSeq : ColorPar), unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, width = 28, display = IsModeHidden(step.type) ? DisplayStyle.None : DisplayStyle.Flex } };
			var info = new Label(BuildStepTypeInfo(step)) { enableRichText = true, style = { fontSize = 11, flexGrow = 1, color = new StyleColor(step.enabled ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f)) } };
			var tagLabel = new Label(string.IsNullOrEmpty(step.tag) ? "" : $"[{step.tag}]") { style = { fontSize = 11, color = new StyleColor(new Color(0.75f, 0.75f, 0.75f)), marginRight = 4, display = string.IsNullOrEmpty(step.tag) ? DisplayStyle.None : DisplayStyle.Flex } };
			var iconLabel = new Label("") { style = { fontSize = 14, color = new StyleColor(new Color(0.7f, 0.7f, 0.7f)), marginLeft = 4, marginRight = 8, display = DisplayStyle.None } };
			var warningIcon = new Image { image = EditorGUIUtility.IconContent("console.warnicon.sml").image, style = { width = 14, height = 14, marginRight = 4, display = DisplayStyle.None } };
			var upBtn = MakeSmallButton("↑", 18, () => { if (stepIndex <= 0) return; Undo.RecordObject(_sequencer, "Move Step Up"); var temp = seq.steps[stepIndex - 1]; seq.steps[stepIndex - 1] = seq.steps[stepIndex]; seq.steps[stepIndex] = temp; EditorUtility.SetDirty(_sequencer); rebuild(); }); upBtn.SetEnabled(stepIndex > 0);
			var downBtn = MakeSmallButton("↓", 18, () => { if (stepIndex >= seq.steps.Count - 1) return; Undo.RecordObject(_sequencer, "Move Step Down"); var temp = seq.steps[stepIndex + 1]; seq.steps[stepIndex + 1] = seq.steps[stepIndex]; seq.steps[stepIndex] = temp; EditorUtility.SetDirty(_sequencer); rebuild(); }); downBtn.SetEnabled(stepIndex < seq.steps.Count - 1);
			var removeBtn = MakeSmallButton("✕", 22, () => { Undo.RecordObject(_sequencer, "Remove Step"); seq.steps.RemoveAt(stepIndex); EditorUtility.SetDirty(_sequencer); rebuild(); }); removeBtn.style.marginRight = 4;
			header.Add(colorBar); header.Add(arrow); header.Add(enableToggle); header.Add(modeEl); header.Add(info); header.Add(tagLabel); header.Add(iconLabel); header.Add(warningIcon); header.Add(upBtn); header.Add(downBtn); header.Add(removeBtn);
			return (header, colorBar, arrow, info, tagLabel, modeEl, iconLabel, warningIcon);
		}

		VisualElement MakeProgressBar(int seqIndex, int stepIndex, Color typeColor) {
			var bg = new VisualElement { style = { height = 3, backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f)) } };
			var fill = new VisualElement { style = { height = 3, width = Length.Percent(0), backgroundColor = new StyleColor(new Color(typeColor.r, typeColor.g, typeColor.b, 0f)) } }; bg.Add(fill);
			int cs = seqIndex; int ci = stepIndex;
			bg.schedule.Execute(() => {
				if (_sequencer == null) return;
				bool active = _sequencer.editorPlayingSeqIndex == cs && _sequencer.editorStepProgress != null && ci < _sequencer.editorStepProgress.Length;
				fill.style.backgroundColor = new StyleColor(new Color(typeColor.r, typeColor.g, typeColor.b, active ? 1f : 0f));
				fill.style.width = Length.Percent(active ? _sequencer.editorStepProgress[ci] * 100f : 0f);
			}).Every(16); return bg;
		}

		List<string> GetValidAnimTypes(bool isUI) {
			var valid = new List<string>();
			foreach (AnimType t in System.Enum.GetValues(typeof(AnimType))) {
				bool uiOnly = t == AnimType.Fade || t == AnimType.SetFade || t == AnimType.ColorTint || t == AnimType.SetColor || t == AnimType.TypeWriter || t == AnimType.TextCounter || t == AnimType.SetText || t == AnimType.SetImage || t == AnimType.FillAmount || t == AnimType.SizeDelta || t == AnimType.SetCanvasGroupState;
				bool worldOnly = t == AnimType.SetSprite || t == AnimType.FadeSpriteColor;
				if (isUI && !worldOnly) valid.Add(t.ToString());
				else if (!isUI && !uiOnly) valid.Add(t.ToString());
			}
			valid.Sort(); return valid;
		}

		void BuildStepBody(VisualElement body, int seqIndex, int stepIndex, VisualElement colorBar, Label infoLabel, Label tagLabel, Label modeEl, Label iconLabel, Image warningIcon, AnimStep step) {
			var stepProp = GetStepProp(seqIndex, stepIndex);
			bool GetCurrentIsUI() { var t = stepProp.FindPropertyRelative("target").objectReferenceValue as Transform; return t != null ? t is RectTransform : IsSequencerUI(); }

			var tagField = new PropertyField(stepProp.FindPropertyRelative("tag"), "Tag"); tagField.Bind(serializedObject);
			tagField.RegisterValueChangeCallback(_ => { tagLabel.text = string.IsNullOrEmpty(step.tag) ? "" : $"[{step.tag}]"; tagLabel.style.display = string.IsNullOrEmpty(step.tag) ? DisplayStyle.None : DisplayStyle.Flex; }); body.Add(tagField);

			var modeField = new PropertyField(stepProp.FindPropertyRelative("mode"), "Mode") { name = "modeField", style = { display = IsModeHidden(step.type) ? DisplayStyle.None : DisplayStyle.Flex } }; modeField.Bind(serializedObject); body.Add(modeField);

			var typeFieldsContainer = new VisualElement();
			var contextWarning = new HelpBox("", HelpBoxMessageType.Error) { style = { display = DisplayStyle.None } };

			void UpdateContextWarning() {
				if (step == null) return;
				bool isUI = GetCurrentIsUI(); bool isCompatible = true; string msg = ""; HelpBoxMessageType msgType = HelpBoxMessageType.Error;

				if (!isUI && (step.type == AnimType.Fade || step.type == AnimType.SetFade || step.type == AnimType.ColorTint || step.type == AnimType.SetColor || step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || step.type == AnimType.SetText || step.type == AnimType.SetImage || step.type == AnimType.FillAmount || step.type == AnimType.SizeDelta || step.type == AnimType.SetCanvasGroupState)) { isCompatible = false; msg = $"Type '{step.type}' is strictly for UI elements."; } else if (isUI && (step.type == AnimType.SetSprite || step.type == AnimType.FadeSpriteColor)) { isCompatible = false; msg = $"Type '{step.type}' is for World 2D Sprites only."; }

				if (isCompatible && step.type == AnimType.Repeat) {
					if (!_sequencer.sequences[seqIndex].steps.Exists(s => s.type == AnimType.Anchor && s.anchorLabel == step.repeatAnchorLabel)) { isCompatible = false; msg = $"Target Anchor '#{step.repeatAnchorLabel}' does not exist."; }
				}

				if (isCompatible) {
					Transform effTarget = (stepProp.FindPropertyRelative("target").objectReferenceValue as Transform) ?? _sequencer.transform;
					bool missing = false; string comp = "";
					if (effTarget != null) {
						if ((step.type == AnimType.Fade || step.type == AnimType.SetFade || step.type == AnimType.SetCanvasGroupState) && isUI && effTarget.GetComponent<CanvasGroup>() == null) { missing = true; comp = "CanvasGroup"; } else if ((step.type == AnimType.TypeWriter || step.type == AnimType.TextCounter || step.type == AnimType.SetText) && stepProp.FindPropertyRelative("tmpTarget").objectReferenceValue == null && effTarget.GetComponent<TMP_Text>() == null) { missing = true; comp = "TMP_Text"; } else if ((step.type == AnimType.SetSprite || step.type == AnimType.FadeSpriteColor) && stepProp.FindPropertyRelative("spriteTarget").objectReferenceValue == null && effTarget.GetComponent<SpriteRenderer>() == null) { missing = true; comp = "SpriteRenderer"; } else if ((step.type == AnimType.SetImage || step.type == AnimType.FillAmount) && stepProp.FindPropertyRelative("imageTarget").objectReferenceValue == null && effTarget.GetComponent<Image>() == null) { missing = true; comp = "Image"; } else if ((step.type == AnimType.ColorTint || step.type == AnimType.SetColor) && effTarget.GetComponent<UnityEngine.UI.Graphic>() == null) { missing = true; comp = "Graphic (Image or Text)"; } else if ((step.type == AnimType.PlayAudio || step.type == AnimType.FadeAudio) && stepProp.FindPropertyRelative("audioTarget").objectReferenceValue == null && effTarget.GetComponent<AudioSource>() == null) { missing = true; comp = "AudioSource"; } else if ((step.type == AnimType.MaterialFloat || step.type == AnimType.MaterialColor || step.type == AnimType.SetMaterialFloat || step.type == AnimType.SetMaterialColor) && stepProp.FindPropertyRelative("rendererTarget").objectReferenceValue == null && effTarget.GetComponent<Renderer>() == null) { missing = true; comp = "Renderer"; }
					}
					if (missing) { isCompatible = false; msg = $"Missing Component: Needs a {comp} component!"; msgType = HelpBoxMessageType.Warning; }
				}

				if (!isCompatible) {
					contextWarning.text = msg; contextWarning.messageType = msgType;
					warningIcon.image = EditorGUIUtility.IconContent(msgType == HelpBoxMessageType.Error ? "console.erroricon.sml" : "console.warnicon.sml").image;
					contextWarning.style.display = DisplayStyle.Flex; warningIcon.style.display = DisplayStyle.Flex;
				} else { contextWarning.style.display = DisplayStyle.None; warningIcon.style.display = DisplayStyle.None; }
			}

			bool currentIsUI = GetCurrentIsUI(); var validChoices = GetValidAnimTypes(currentIsUI);
			string currentTypeStr = step.type.ToString(); if (!validChoices.Contains(currentTypeStr)) validChoices.Add(currentTypeStr);

			var typeDropdown = new DropdownField("Type", validChoices, currentTypeStr); typeDropdown.AddToClassList("unity-base-field"); typeDropdown.AddToClassList("unity-base-field__aligned");
			typeDropdown.RegisterValueChangedCallback(evt => {
				if (System.Enum.TryParse<AnimType>(evt.newValue, out var newType)) {
					Undo.RecordObject(_sequencer, "Change Step Type"); step.type = newType; stepProp.FindPropertyRelative("type").enumValueIndex = (int)newType; serializedObject.ApplyModifiedProperties();
					colorBar.style.backgroundColor = new StyleColor(step.enabled ? GetAnimTypeColor(step.type) : new Color(0.4f, 0.4f, 0.4f));
					infoLabel.text = BuildStepTypeInfo(step); colorBar.parent.style.backgroundColor = new StyleColor(step.type == AnimType.Anchor ? new Color(ColorAnchor.r, ColorAnchor.g, ColorAnchor.b, 0.4f) : BgStep);
					modeEl.style.display = IsModeHidden(step.type) ? DisplayStyle.None : DisplayStyle.Flex;
					RefreshStepBodyVisibility(body, step); BuildTypeFields(typeFieldsContainer, seqIndex, stepIndex, infoLabel, body); UpdateContextWarning();
				}
			}); body.Add(typeDropdown);

			var durationField = MakeBoundField(stepProp.FindPropertyRelative("duration"), "Duration"); durationField.name = "durationField"; durationField.RegisterValueChangeCallback(evt => { step.duration = evt.changedProperty.floatValue; infoLabel.text = BuildStepTypeInfo(step); });
			var delayField = MakeBoundField(stepProp.FindPropertyRelative("delay"), "Delay"); delayField.name = "delayField"; delayField.RegisterValueChangeCallback(evt => { step.delay = evt.changedProperty.floatValue; infoLabel.text = BuildStepTypeInfo(step); });
			var easeField = MakeBoundField(stepProp.FindPropertyRelative("ease"), "Ease"); easeField.name = "easeField"; easeField.RegisterValueChangeCallback(evt => { step.ease = (PrimeTween.Ease)evt.changedProperty.intValue; RefreshStepBodyVisibility(body, step); });
			var customCurveField = MakeBoundField(stepProp.FindPropertyRelative("customCurve"), "Custom Curve"); customCurveField.name = "customCurveField";
			var (fromCurrentField, fromCurrentPill) = MakeToggleField(stepProp.FindPropertyRelative("animateFromCurrent"), "From Current", () => step.animateFromCurrent); fromCurrentField.name = "fromCurrentField"; fromCurrentPill.onValueChanged += () => UpdateFromCurrentVisibility(body, step);
			var targetField = MakeTargetField(stepProp.FindPropertyRelative("target"), "Target Transform"); targetField.name = "targetField"; targetField.RegisterValueChangeCallback(_ => {
				bool newIsUI = GetCurrentIsUI(); var newChoices = GetValidAnimTypes(newIsUI); string typeStr = step.type.ToString();
				if (!newChoices.Contains(typeStr)) newChoices.Add(typeStr); typeDropdown.choices = newChoices; typeDropdown.value = typeStr;
				UpdateContextWarning(); BuildTypeFields(typeFieldsContainer, seqIndex, stepIndex, infoLabel, body);
			});

			body.Add(durationField); body.Add(delayField); body.Add(easeField); body.Add(customCurveField); body.Add(fromCurrentField); body.Add(targetField); body.Add(Spacer(4));
			BuildTypeFields(typeFieldsContainer, seqIndex, stepIndex, infoLabel, body); body.Add(typeFieldsContainer);
			UpdateContextWarning(); body.Add(Spacer(4)); body.Add(contextWarning); RefreshStepBodyVisibility(body, step);

			body.schedule.Execute(() => {
				if (step == null) return;
				string newInfo = BuildStepTypeInfo(step); if (infoLabel.text != newInfo) infoLabel.text = newInfo;
				string newMode = step.mode == StepMode.Sequential ? "SEQ" : "PAR";
				if (modeEl.text != newMode) { modeEl.text = newMode; modeEl.style.color = new StyleColor(step.mode == StepMode.Sequential ? ColorSeq : ColorPar); }
				UpdateContextWarning();
			}).Every(100);
		}

		void UpdateFromCurrentVisibility(VisualElement body, AnimStep step) { var f = body.Q<PropertyField>("fromField"); if (f != null) f.style.display = step.animateFromCurrent ? DisplayStyle.None : DisplayStyle.Flex; }
		void UpdateToLabelVisibility(VisualElement body, AnimStep step) { body.Query<PropertyField>("toField").ForEach(f => f.label = step.relativeOffset ? "To Offset" : "To"); body.Query<PropertyField>("fromField").ForEach(f => f.label = step.relativeOffset ? "From Offset" : "From"); }

		void RefreshStepBodyVisibility(VisualElement body, AnimStep step) {
			SetVisible(body, "durationField", !IsDurationHidden(step.type)); SetVisible(body, "delayField", !IsDelayHidden(step.type)); SetVisible(body, "easeField", !IsEaseHidden(step.type)); SetVisible(body, "customCurveField", !IsEaseHidden(step.type) && step.ease == PrimeTween.Ease.Custom); SetVisible(body, "fromCurrentField", !IsFromCurrentHidden(step)); SetVisible(body, "targetField", !IsTargetHidden(step.type)); SetVisible(body, "modeField", !IsModeHidden(step.type));
			UpdateFromCurrentVisibility(body, step); UpdateToLabelVisibility(body, step);
		}

		VisualElement MakeRelativeToggle(SerializedProperty sp, AnimStep step, Label infoLabel, VisualElement body) {
			var (relRow, relPill) = MakeToggleField(sp.FindPropertyRelative("relativeOffset"), "Relative Offset", () => step.relativeOffset);
			relPill.onValueChanged += () => { RefreshStepBodyVisibility(body, step); infoLabel.text = BuildStepTypeInfo(step); }; return relRow;
		}

		void BuildTypeFields(VisualElement c, int seqIndex, int stepIndex, Label infoLabel, VisualElement body) {
			c.Clear(); var step = _sequencer.sequences[seqIndex].steps[stepIndex]; var sp = GetStepProp(seqIndex, stepIndex); bool fc = step.animateFromCurrent; bool isUI = IsStepUI(step);

			void Add(SerializedProperty prop, string label, bool isFromField = false, bool isToField = false) {
				bool isRef = prop.propertyType == SerializedPropertyType.ObjectReference; string initLabel = isRef && prop.objectReferenceValue == null ? $"{label} [Self]" : label;
				var f = new PropertyField(prop, initLabel); f.Bind(serializedObject);
				if (isRef) f.RegisterValueChangeCallback(evt => f.label = evt.changedProperty.objectReferenceValue == null ? $"{label} [Self]" : label);
				if (isFromField) { f.name = "fromField"; f.style.display = fc ? DisplayStyle.None : DisplayStyle.Flex; }
				if (isToField) f.name = "toField";
				c.Add(f);
			}

			switch (step.type) {
				case AnimType.Fade: Add(sp.FindPropertyRelative("fadeFrom"), "From", true); Add(sp.FindPropertyRelative("fadeTo"), "To"); break;
				case AnimType.Scale: c.Add(MakeRelativeToggle(sp, step, infoLabel, body)); if (isUI) { Add(sp.FindPropertyRelative("scaleFrom"), "From", true); Add(sp.FindPropertyRelative("scaleTo"), "To", false, true); } else { Add(sp.FindPropertyRelative("scaleFrom3D"), "From", true); Add(sp.FindPropertyRelative("scaleTo3D"), "To", false, true); } break;
				case AnimType.Slide: c.Add(MakeRelativeToggle(sp, step, infoLabel, body)); Add(sp.FindPropertyRelative("slideFrom"), "From", true); Add(sp.FindPropertyRelative("slideTo"), "To", false, true); break;
				case AnimType.Rotate: c.Add(MakeRelativeToggle(sp, step, infoLabel, body)); Add(sp.FindPropertyRelative("rotateFrom"), "From", true); Add(sp.FindPropertyRelative("rotateTo"), "To", false, true); break;
				case AnimType.SizeDelta: c.Add(MakeRelativeToggle(sp, step, infoLabel, body)); Add(sp.FindPropertyRelative("sizeDeltaFrom"), "From", true); Add(sp.FindPropertyRelative("sizeDeltaTo"), "To", false, true); break;
				case AnimType.FillAmount: Add(sp.FindPropertyRelative("imageTarget"), "Image Target"); Add(sp.FindPropertyRelative("fillAmountFrom"), "From", true); Add(sp.FindPropertyRelative("fillAmountTo"), "To"); break;
				case AnimType.Bounce: if (isUI) Add(sp.FindPropertyRelative("bounceIntensity"), "Intensity"); else Add(sp.FindPropertyRelative("bounce3D"), "Bounce Vector"); Add(sp.FindPropertyRelative("bounceCount"), "Count"); break;
				case AnimType.PunchRotate: if (isUI) BuildPunchRotateFields(c, sp, step); else Add(sp.FindPropertyRelative("punchRotate3D"), "Punch Vector"); break;
				case AnimType.PunchScale: if (isUI) Add(sp.FindPropertyRelative("punchScaleIntensity"), "Intensity"); else Add(sp.FindPropertyRelative("punchScale3D"), "Punch Vector"); Add(sp.FindPropertyRelative("punchScaleFrequency"), "Frequency"); break;
				case AnimType.ShakePosition: Add(sp.FindPropertyRelative("shakeStrength"), "Strength"); Add(sp.FindPropertyRelative("shakeFrequency"), "Frequency"); var (srPill, _) = MakeToggleField(sp.FindPropertyRelative("shakeFalloff"), "Falloff", () => step.shakeFalloff); c.Add(srPill); break;
				case AnimType.ShakeRotation: Add(sp.FindPropertyRelative("shakeStrength"), "Strength"); Add(sp.FindPropertyRelative("shakeFrequency"), "Frequency"); var (srotPill, _) = MakeToggleField(sp.FindPropertyRelative("shakeFalloff"), "Falloff", () => step.shakeFalloff); c.Add(srotPill); break;
				case AnimType.ColorTint: Add(sp.FindPropertyRelative("colorTarget"), "Color Target"); Add(sp.FindPropertyRelative("colorFrom"), "From", true); Add(sp.FindPropertyRelative("colorTo"), "To"); break;
				case AnimType.FadeSpriteColor: Add(sp.FindPropertyRelative("spriteTarget"), "Sprite Target"); Add(sp.FindPropertyRelative("colorFrom"), "From", true); Add(sp.FindPropertyRelative("colorTo"), "To"); break;
				case AnimType.SetColor: Add(sp.FindPropertyRelative("colorTarget"), "Color Target"); Add(sp.FindPropertyRelative("colorTo"), "Color"); break;
				case AnimType.SetFade: Add(sp.FindPropertyRelative("setFadeValue"), "Alpha"); break;
				case AnimType.SetSprite: Add(sp.FindPropertyRelative("spriteTarget"), "Sprite Target"); Add(sp.FindPropertyRelative("setSpriteValue"), "New Sprite"); break;
				case AnimType.SetImage: Add(sp.FindPropertyRelative("imageTarget"), "Image Target"); Add(sp.FindPropertyRelative("setSpriteValue"), "New Sprite"); break;
				case AnimType.TypeWriter: Add(sp.FindPropertyRelative("tmpTarget"), "TMP Target"); Add(sp.FindPropertyRelative("setTextValue"), "Text String"); Add(sp.FindPropertyRelative("typeWriterCharsPerSecond"), "Chars Per Second"); break;
				case AnimType.TextCounter: BuildTextCounterFields(c, sp, step, seqIndex, stepIndex); break;
				case AnimType.SetTransform: BuildSetTransformFields(c, sp, step, infoLabel, body); break;
				case AnimType.SetText: Add(sp.FindPropertyRelative("tmpTarget"), "TMP Target"); Add(sp.FindPropertyRelative("setTextValue"), "Text Value"); break;
				case AnimType.SetActive: var (actRow, actPill) = MakeToggleField(sp.FindPropertyRelative("setActiveValue"), "Set Active", () => step.setActiveValue); actPill.onValueChanged += () => infoLabel.text = BuildStepTypeInfo(step); c.Add(actRow); break;
				case AnimType.SetCanvasGroupState: Add(sp.FindPropertyRelative("cgInteractable"), "Interactable"); Add(sp.FindPropertyRelative("cgBlocksRaycasts"), "Blocks Raycasts"); Add(sp.FindPropertyRelative("cgIgnoreParentGroups"), "Ignore Parent Groups"); break;
				case AnimType.PlayAudio: Add(sp.FindPropertyRelative("audioTarget"), "Audio Target"); Add(sp.FindPropertyRelative("audioClip"), "Audio Clip"); Add(sp.FindPropertyRelative("audioVolume"), "Volume (Min/Max)"); Add(sp.FindPropertyRelative("audioPitch"), "Pitch (Min/Max)"); Add(sp.FindPropertyRelative("audioSpatialBlend"), "Spatial Blend"); break;
				case AnimType.FadeAudio: Add(sp.FindPropertyRelative("audioTarget"), "Audio Target"); Add(sp.FindPropertyRelative("fadeAudioFrom"), "From", true); Add(sp.FindPropertyRelative("fadeAudioTo"), "To"); break;
				case AnimType.TimeScale: Add(sp.FindPropertyRelative("timeScaleFrom"), "From", true); Add(sp.FindPropertyRelative("timeScaleTo"), "To"); break;
				case AnimType.SetTimeScale: Add(sp.FindPropertyRelative("timeScaleTo"), "TimeScale Value"); break;
				case AnimType.MaterialFloat: Add(sp.FindPropertyRelative("rendererTarget"), "Renderer Target"); Add(sp.FindPropertyRelative("materialPropertyName"), "Property Name"); Add(sp.FindPropertyRelative("materialFloatFrom"), "From", true); Add(sp.FindPropertyRelative("materialFloatTo"), "To"); break;
				case AnimType.SetMaterialFloat: Add(sp.FindPropertyRelative("rendererTarget"), "Renderer Target"); Add(sp.FindPropertyRelative("materialPropertyName"), "Property Name"); Add(sp.FindPropertyRelative("materialFloatTo"), "Value"); break;
				case AnimType.MaterialColor: Add(sp.FindPropertyRelative("rendererTarget"), "Renderer Target"); Add(sp.FindPropertyRelative("materialPropertyName"), "Property Name"); Add(sp.FindPropertyRelative("materialColorFrom"), "From", true); Add(sp.FindPropertyRelative("materialColorTo"), "To"); break;
				case AnimType.SetMaterialColor: Add(sp.FindPropertyRelative("rendererTarget"), "Renderer Target"); Add(sp.FindPropertyRelative("materialPropertyName"), "Property Name"); Add(sp.FindPropertyRelative("materialColorTo"), "Color"); break;
				case AnimType.Trigger:
					var seqField = MakeTargetField(sp.FindPropertyRelative("triggerSequencer"), "Sequencer"); seqField.RegisterValueChangeCallback(evt => { step.triggerSequencer = evt.changedProperty.objectReferenceValue as AnimSequencer; infoLabel.text = BuildStepTypeInfo(step); }); c.Add(seqField);
					var lblField = new PropertyField(sp.FindPropertyRelative("triggerSequenceLabel"), "Sequence Label"); lblField.Bind(serializedObject); lblField.RegisterValueChangeCallback(evt => { step.triggerSequenceLabel = evt.changedProperty.stringValue; infoLabel.text = BuildStepTypeInfo(step); }); c.Add(lblField); break;
				case AnimType.Event: var eventField = new PropertyField(sp.FindPropertyRelative("onEvent"), "On Event"); eventField.Bind(serializedObject); c.Add(eventField); break;
				case AnimType.Anchor: var anchorField = new PropertyField(sp.FindPropertyRelative("anchorLabel"), "Anchor Name"); anchorField.Bind(serializedObject); anchorField.RegisterValueChangeCallback(evt => { step.anchorLabel = evt.changedProperty.stringValue; infoLabel.text = BuildStepTypeInfo(step); }); c.Add(anchorField); break;
				case AnimType.Repeat: var repField = new PropertyField(sp.FindPropertyRelative("repeatAnchorLabel"), "To Anchor"); repField.Bind(serializedObject); repField.RegisterValueChangeCallback(evt => { step.repeatAnchorLabel = evt.changedProperty.stringValue; infoLabel.text = BuildStepTypeInfo(step); }); c.Add(repField); break;
				case AnimType.WaitUntil:
					var (waitRow, waitPill) = MakeToggleField(sp.FindPropertyRelative("waitUntilValue"), "Condition Met", () => step.waitUntilValue); waitPill.onValueChanged += () => infoLabel.text = BuildStepTypeInfo(step); c.Add(waitRow);
					bool currentPillVisual = step.waitUntilValue; waitRow.schedule.Execute(() => { if (!Application.isPlaying || step == null) return; bool isMet = step.waitUntilValue; if (step.waitConditionLambda != null) isMet = isMet || step.waitConditionLambda.Invoke(); if (currentPillVisual != isMet) { currentPillVisual = isMet; waitPill.SetValue(isMet); } }).Every(50); break;
				case AnimType.Wait:
					var waitMethodField = new PropertyField(sp.FindPropertyRelative("waitMethod"), "Wait Method"); waitMethodField.Bind(serializedObject); c.Add(waitMethodField);
					var durField = new PropertyField(sp.FindPropertyRelative("duration"), "Duration (Seconds)"); durField.Bind(serializedObject); c.Add(durField);
					var framesField = new PropertyField(sp.FindPropertyRelative("waitFrames"), "Frames"); framesField.Bind(serializedObject); c.Add(framesField);
					void RefreshWait() { bool isFrames = step.waitMethod == WaitMethod.Frames; durField.style.display = isFrames ? DisplayStyle.None : DisplayStyle.Flex; framesField.style.display = isFrames ? DisplayStyle.Flex : DisplayStyle.None; }
					RefreshWait(); waitMethodField.RegisterValueChangeCallback(evt => { step.waitMethod = (WaitMethod)evt.changedProperty.enumValueIndex; RefreshWait(); infoLabel.text = BuildStepTypeInfo(step); }); break;
			}
			UpdateFromCurrentVisibility(body, step); UpdateToLabelVisibility(body, step);
		}

		void BuildPunchRotateFields(VisualElement c, SerializedProperty sp, AnimStep step) {
			var (randomRow, randomToggle) = MakeToggleField(sp.FindPropertyRelative("punchRotateRandom"), "Random Angle", () => step.punchRotateRandom); c.Add(randomRow);
			var freqField = new PropertyField(sp.FindPropertyRelative("punchRotateFrequency"), "Frequency"); freqField.Bind(serializedObject); c.Add(freqField);
			var a1 = new PropertyField(sp.FindPropertyRelative("punchRotateAngle1"), "Angle 1"); a1.Bind(serializedObject); c.Add(a1);
			var a2 = new PropertyField(sp.FindPropertyRelative("punchRotateAngle2"), "Angle 2"); a2.Bind(serializedObject); c.Add(a2);
			var a = new PropertyField(sp.FindPropertyRelative("punchRotateAngle"), "Angle"); a.Bind(serializedObject); c.Add(a);
			void Refresh() { bool r = step.punchRotateRandom; a1.style.display = r ? DisplayStyle.Flex : DisplayStyle.None; a2.style.display = r ? DisplayStyle.Flex : DisplayStyle.None; a.style.display = r ? DisplayStyle.None : DisplayStyle.Flex; }
			Refresh(); randomToggle.onValueChanged += Refresh;
		}

		void BuildTextCounterFields(VisualElement c, SerializedProperty sp, AnimStep step, int seqIndex, int stepIndex) {
			var tmpField = new PropertyField(sp.FindPropertyRelative("tmpTarget"), "TMP Target"); tmpField.Bind(serializedObject); c.Add(tmpField);
			var (fromCurrentRow, fromCurrentToggle) = MakeToggleField(sp.FindPropertyRelative("animateFromCurrent"), "From Current", () => step.animateFromCurrent); c.Add(fromCurrentRow);
			var fromField = new PropertyField(GetStepProp(seqIndex, stepIndex).FindPropertyRelative("textCounterFrom"), "From"); fromField.name = "fromField"; fromField.Bind(serializedObject); c.Add(fromField);
			void Refresh() { fromField.style.display = step.animateFromCurrent ? DisplayStyle.None : DisplayStyle.Flex; }
			Refresh(); fromCurrentToggle.onValueChanged += Refresh;
			var toField = new PropertyField(sp.FindPropertyRelative("textCounterTo"), "To"); toField.Bind(serializedObject); c.Add(toField);
			var fmtField = new PropertyField(sp.FindPropertyRelative("textCounterFormat"), "Format"); fmtField.Bind(serializedObject); c.Add(fmtField);
			var (roundRow, _) = MakeToggleField(sp.FindPropertyRelative("textCounterRoundToInt"), "Round To Int", () => step.textCounterRoundToInt); c.Add(roundRow);
		}

		void BuildSetTransformFields(VisualElement c, SerializedProperty sp, AnimStep step, Label infoLabel, VisualElement body) {
			c.Add(MakeRelativeToggle(sp, step, infoLabel, body));
			var subField = new PropertyField(sp.FindPropertyRelative("transformSubType"), "Sub Type"); subField.Bind(serializedObject); c.Add(subField);
			var valContainer = new VisualElement(); c.Add(valContainer);
			void Rebuild() { valContainer.Clear(); string lbl = step.relativeOffset ? "Offset" : (step.transformSubType == TransformSubType.LocalPosition ? "Position" : (step.transformSubType == TransformSubType.LocalRotation ? "Rotation" : "Scale")); var vf = new PropertyField(sp.FindPropertyRelative("setTransformValue"), lbl); vf.Bind(serializedObject); valContainer.Add(vf); }
			Rebuild(); subField.RegisterValueChangeCallback(evt => { step.transformSubType = (TransformSubType)evt.changedProperty.enumValueIndex; Rebuild(); infoLabel.text = BuildStepTypeInfo(step); });
		}

		PropertyField MakeTargetField(SerializedProperty prop, string baseLabel) {
			string initLabel = prop.objectReferenceValue == null ? $"{baseLabel} [Self]" : baseLabel; var f = new PropertyField(prop, initLabel); f.Bind(serializedObject);
			f.RegisterValueChangeCallback(evt => f.label = evt.changedProperty.objectReferenceValue == null ? $"{baseLabel} [Self]" : baseLabel); return f;
		}

		static bool IsInstantType(AnimType t) { return t == AnimType.SetTransform || t == AnimType.SetText || t == AnimType.SetColor || t == AnimType.SetActive || t == AnimType.Trigger || t == AnimType.Event || t == AnimType.SetSprite || t == AnimType.SetImage || t == AnimType.SetFade || t == AnimType.SetCanvasGroupState || t == AnimType.PlayAudio || t == AnimType.SetTimeScale || t == AnimType.SetMaterialFloat || t == AnimType.SetMaterialColor; }
		static bool IsLogicType(AnimType t) { return t == AnimType.Anchor || t == AnimType.Repeat || t == AnimType.WaitUntil; }
		static bool IsModeHidden(AnimType t) { return t == AnimType.Anchor; }
		static bool IsDelayHidden(AnimType t) { return t == AnimType.Anchor; }
		static bool IsDurationHidden(AnimType t) { return IsInstantType(t) || t == AnimType.TypeWriter || IsLogicType(t) || t == AnimType.Wait; }
		static bool IsEaseHidden(AnimType t) { return IsInstantType(t) || t == AnimType.Wait || t == AnimType.Bounce || t == AnimType.PunchRotate || t == AnimType.PunchScale || t == AnimType.ShakePosition || t == AnimType.ShakeRotation || IsLogicType(t); }
		static bool IsFromCurrentHidden(AnimStep step) { AnimType t = step.type; return t == AnimType.Wait || t == AnimType.Bounce || t == AnimType.PunchRotate || t == AnimType.PunchScale || t == AnimType.ShakePosition || t == AnimType.ShakeRotation || t == AnimType.TypeWriter || t == AnimType.TextCounter || IsInstantType(t) || IsLogicType(t); }
		static bool IsTargetHidden(AnimType t) { return t == AnimType.Wait || t == AnimType.TypeWriter || t == AnimType.TextCounter || t == AnimType.SetText || t == AnimType.Trigger || t == AnimType.Event || t == AnimType.SetSprite || t == AnimType.FadeSpriteColor || t == AnimType.SetImage || t == AnimType.TimeScale || t == AnimType.SetTimeScale || IsLogicType(t); }
		static bool IsInteractableTrigger(TriggerType t) { return t == TriggerType.OnBecameInteractable || t == TriggerType.OnBecameNonInteractable; }
		static string Dur(float f) { return f.ToString("0.##", CultureInfo.InvariantCulture) + "s"; }

		string BuildStepTypeInfo(AnimStep step) {
			string delay = step.delay > 0f ? $"  +{Dur(step.delay)}" : "";
			string rel = step.relativeOffset && (step.type == AnimType.Slide || step.type == AnimType.Scale || step.type == AnimType.Rotate || step.type == AnimType.SetTransform || step.type == AnimType.SizeDelta) ? " Relative" : "";
			if (step.type == AnimType.Trigger) {
				var targetSeq = step.triggerSequencer != null ? step.triggerSequencer : _sequencer;
				bool exists = string.IsNullOrEmpty(step.triggerSequenceLabel) || (targetSeq != null && targetSeq.sequences.Exists(s => s.label == step.triggerSequenceLabel));
				string lbl = string.IsNullOrEmpty(step.triggerSequenceLabel) ? "None" : (!exists ? $"<color=#ff5555>{step.triggerSequenceLabel}</color>" : step.triggerSequenceLabel);
				return $"<b>Trigger</b> ({(step.triggerSequencer != null ? step.triggerSequencer.name : "Self")} → {lbl}){delay}";
			}
			switch (step.type) {
				case AnimType.SetTransform: return $"<b>{step.type}{rel}</b> ({step.transformSubType}){delay}";
				case AnimType.SetText: return $"<b>SetText</b>{delay}";
				case AnimType.SetColor: return $"<b>SetColor</b>{delay}";
				case AnimType.SetSprite: return $"<b>SetSprite</b>{delay}";
				case AnimType.SetImage: return $"<b>SetImage</b>{delay}";
				case AnimType.SetFade: return $"<b>SetFade</b>{delay}";
				case AnimType.SetCanvasGroupState: return $"<b>SetCanvasGroupState</b>{delay}";
				case AnimType.PlayAudio: return $"<b>PlayAudio</b>{delay}";
				case AnimType.SetTimeScale: return $"<b>SetTimeScale</b>{delay}";
				case AnimType.SetMaterialFloat: return $"<b>SetMaterialFloat</b>{delay}";
				case AnimType.SetMaterialColor: return $"<b>SetMaterialColor</b>{delay}";
				case AnimType.SetActive: return $"<b>SetActive</b> ({(step.setActiveValue ? "On" : "Off")}){delay}";
				case AnimType.TypeWriter: return $"<b>TypeWriter</b>{delay}";
				case AnimType.Wait: return $"<b>Wait</b>  {(step.waitMethod == WaitMethod.Frames ? step.waitFrames + " Frames" : Dur(step.duration))}{delay}";
				case AnimType.Event: return $"<b>Event</b>{delay}";
				case AnimType.Anchor: return $"<b><color=#888888>#</color><color=#ffffff>{step.anchorLabel}</color></b>";
				case AnimType.Repeat: return $"<b>Repeat</b> (→ <color=#888888>#</color><color=#ffffff>{step.repeatAnchorLabel}</color>){delay}";
				case AnimType.WaitUntil: return $"<b>WaitUntil</b>{delay}";
				default: return $"<b>{step.type}{rel}</b>  {Dur(step.duration)}{delay}";
			}
		}

		Color GetAnimTypeColor(AnimType type) {
			switch (type) {
				case AnimType.Fade: return ColorFade;
				case AnimType.Scale: return ColorScale;
				case AnimType.Slide: return ColorSlide;
				case AnimType.Rotate: return ColorRotate;
				case AnimType.SizeDelta: return ColorSizeDelta;
				case AnimType.FillAmount: return ColorFill;
				case AnimType.Bounce: return ColorBounce;
				case AnimType.PunchRotate: return ColorPunchRotate;
				case AnimType.PunchScale: return ColorPunchScale;
				case AnimType.ShakePosition: case AnimType.ShakeRotation: return ColorShake;
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
				case AnimType.SetSprite: case AnimType.FadeSpriteColor: return ColorSprite;
				case AnimType.SetImage: return ColorSetColor;
				case AnimType.SetFade: return ColorSetFade;
				case AnimType.SetCanvasGroupState: return ColorSetActive;
				case AnimType.PlayAudio: case AnimType.FadeAudio: return ColorAudio;
				case AnimType.TimeScale: case AnimType.SetTimeScale: return ColorTimeScale;
				case AnimType.MaterialFloat: case AnimType.SetMaterialFloat: case AnimType.MaterialColor: case AnimType.SetMaterialColor: return ColorMaterial;
				case AnimType.Anchor: return ColorAnchor;
				case AnimType.Repeat: return ColorRepeat;
				case AnimType.WaitUntil: return ColorWaitUntil;
				default: return Color.gray;
			}
		}

		PropertyField MakeBoundField(SerializedProperty prop, string label) { var f = new PropertyField(prop, label); f.Bind(serializedObject); return f; }

		(VisualElement row, PillToggle toggle) MakeToggleField(SerializedProperty prop, string label, System.Func<bool> getValue) {
			var row = new VisualElement { style = { minHeight = 22 } }; row.AddToClassList("unity-base-field"); row.AddToClassList("unity-base-field__aligned");
			var lbl = new Label(label) { style = { width = Length.Percent(45), minWidth = 127, paddingLeft = 3, color = new StyleColor(new Color(0.78f, 0.78f, 0.78f)) } };
			var inputContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, alignItems = Align.Center } };
			var pill = new PillToggle(getValue(), ToggleOnBg, ToggleOffBg) { style = { marginLeft = -20 } };
			pill.onClicked += () => { bool newVal = !getValue(); Undo.RecordObject(_sequencer, $"Toggle {label}"); prop.boolValue = newVal; prop.serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(_sequencer); pill.SetValue(newVal); if (pill.onValueChanged != null) pill.onValueChanged.Invoke(); };
			inputContainer.Add(pill); row.Add(lbl); row.Add(inputContainer); return (row, pill);
		}

		Button MakeSmallButton(string text, int width, System.Action onClick) { var btn = new Button(onClick) { text = text, style = { width = width, height = 20 } }; ApplyNeonButtonStyle(btn); return btn; }
		static void SetVisible(VisualElement parent, string name, bool visible) { var el = parent.Q<VisualElement>(name); if (el != null) el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None; }

		VisualElement CreateBox(int radius, Color borderColor) { var box = new VisualElement { style = { borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = new StyleColor(borderColor), borderBottomColor = new StyleColor(borderColor), borderLeftColor = new StyleColor(borderColor), borderRightColor = new StyleColor(borderColor), borderTopLeftRadius = radius, borderTopRightRadius = radius, borderBottomLeftRadius = radius, borderBottomRightRadius = radius, overflow = Overflow.Hidden } }; return box; }
		VisualElement Spacer(int height) { return new VisualElement { style = { height = height } }; }

		void ApplyNeonButtonStyle(VisualElement btn, bool isAccent = false) {
			btn.style.backgroundColor = new StyleColor(ButtonBg); btn.style.color = new StyleColor(Color.white); btn.style.borderTopColor = new StyleColor(ButtonBorder); btn.style.borderBottomColor = new StyleColor(ButtonBorder); btn.style.borderLeftColor = new StyleColor(ButtonBorder); btn.style.borderRightColor = new StyleColor(ButtonBorder); btn.style.borderTopWidth = 1; btn.style.borderBottomWidth = 1; btn.style.borderLeftWidth = 1; btn.style.borderRightWidth = 1; btn.style.borderTopLeftRadius = 3; btn.style.borderTopRightRadius = 3; btn.style.borderBottomLeftRadius = 3; btn.style.borderBottomRightRadius = 3;
			btn.RegisterCallback<MouseOverEvent>(e => { btn.style.backgroundColor = new StyleColor(ButtonHoverBg); if (isAccent) { btn.style.borderTopColor = new StyleColor(ButtonAccent); btn.style.borderBottomColor = new StyleColor(ButtonAccent); btn.style.borderLeftColor = new StyleColor(ButtonAccent); btn.style.borderRightColor = new StyleColor(ButtonAccent); } });
			btn.RegisterCallback<MouseOutEvent>(e => { btn.style.backgroundColor = new StyleColor(ButtonBg); btn.style.borderTopColor = new StyleColor(ButtonBorder); btn.style.borderBottomColor = new StyleColor(ButtonBorder); btn.style.borderLeftColor = new StyleColor(ButtonBorder); btn.style.borderRightColor = new StyleColor(ButtonBorder); });
		}

		class PillToggle : VisualElement {
			public System.Action onClicked; public System.Action onValueChanged;
			readonly VisualElement _pill; readonly VisualElement _knob; readonly Color _onBg; readonly Color _offBg;
			public PillToggle(bool value, Color onBg, Color offBg) {
				_onBg = onBg; _offBg = offBg;
				_pill = new VisualElement { style = { width = 30, height = 14, borderTopLeftRadius = 7, borderTopRightRadius = 7, borderBottomLeftRadius = 7, borderBottomRightRadius = 7, flexShrink = 0, position = Position.Relative } };
				_knob = new VisualElement { style = { width = 10, height = 10, borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderBottomRightRadius = 5, backgroundColor = new StyleColor(Color.white), position = Position.Absolute, top = 2 } };
				_pill.Add(_knob); Add(_pill); SetValue(value);
				RegisterCallback<ClickEvent>(evt => { evt.StopPropagation(); if (onClicked != null) onClicked.Invoke(); });
			}
			public void SetValue(bool value) { if (value) { _pill.style.backgroundColor = new StyleColor(_onBg); _knob.style.left = 18; } else { _pill.style.backgroundColor = new StyleColor(_offBg); _knob.style.left = 2; } }
		}
	}
}
#endif