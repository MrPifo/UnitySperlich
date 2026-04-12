#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Sperlich.Sequencer;
using PrimeTween;
using UnityEditor.Events;
using UnityEngine.Events;
using static Sperlich.Sequencer.AnimSequencer;

namespace Sperlich.Sequencer.Editor {
	public static class AnimSequencerShowcaseGenerator {

		// Inspector Theme Colors
		static readonly Color BgDark = new Color(0.12f, 0.13f, 0.16f);
		static readonly Color BgStep = new Color(0.17f, 0.18f, 0.22f);
		static readonly Color BgStepBody = new Color(0.14f, 0.15f, 0.18f);
		static readonly Color BtnDark = new Color(0.22f, 0.23f, 0.27f);

		static readonly Color NeonCyan = new Color(0.20f, 0.75f, 0.95f);
		static readonly Color NeonGreen = new Color(0.30f, 0.90f, 0.50f);
		static readonly Color NeonRed = new Color(0.95f, 0.25f, 0.25f);
		static readonly Color NeonPurple = new Color(0.70f, 0.50f, 0.95f);
		static readonly Color NeonOrange = new Color(0.95f, 0.60f, 0.20f);
		static readonly Color NeonYellow = new Color(0.95f, 0.90f, 0.30f);

		[MenuItem("Tools/AnimSequencer/Generate 60 Pro Cases")]
		public static void GenerateShowcase() {
			var canvasGO = new GameObject("AnimSequencer_60_ProShowcase");
			Undo.RegisterCreatedObjectUndo(canvasGO, "Create Showcase");

			var canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			var scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			canvasGO.AddComponent<GraphicRaycaster>();

			if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
				new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
			}

			// Main Background
			var bg = CreateUIObj("Background", canvasGO.transform, typeof(Image));
			bg.GetComponent<Image>().color = BgDark;
			Stretch(bg.GetComponent<RectTransform>());

			// Header
			var header = CreateUIObj("Header", bg.transform, typeof(Image));
			header.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.11f);
			var hRect = header.GetComponent<RectTransform>();
			hRect.anchorMin = new Vector2(0, 1); hRect.anchorMax = new Vector2(1, 1);
			hRect.pivot = new Vector2(0.5f, 1); hRect.sizeDelta = new Vector2(0, 90);

			var title = CreateUIObj("Title", header.transform, typeof(TextMeshProUGUI));
			Stretch(title.GetComponent<RectTransform>());
			var tTxt = title.GetComponent<TextMeshProUGUI>();
			tTxt.text = "ANIM SEQUENCER <color=#33e680>PRO</color> SHOWCASE";
			tTxt.alignment = TextAlignmentOptions.Center; tTxt.fontSize = 38; tTxt.fontStyle = FontStyles.Bold;

			// Scroll View
			var scrollView = CreateUIObj("ScrollView", bg.transform, typeof(ScrollRect));
			var svRect = scrollView.GetComponent<RectTransform>();
			svRect.anchorMin = Vector2.zero; svRect.anchorMax = Vector2.one;
			svRect.offsetMin = new Vector2(50, 20); svRect.offsetMax = new Vector2(-50, -100);

			var viewport = CreateUIObj("Viewport", scrollView.transform, typeof(Image), typeof(Mask));
			Stretch(viewport.GetComponent<RectTransform>());
			viewport.GetComponent<Image>().color = Color.white;
			viewport.GetComponent<Mask>().showMaskGraphic = false;

			var content = CreateUIObj("Content", viewport.transform, typeof(GridLayoutGroup), typeof(ContentSizeFitter));
			var contentRect = content.GetComponent<RectTransform>();
			contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
			contentRect.pivot = new Vector2(0.5f, 1);

			// 5 Elements per Row: (1920 - 100 margin) = 1820. 5 * 340 + 4 * 25 = 1700 + 100 = 1800
			var grid = content.GetComponent<GridLayoutGroup>();
			grid.cellSize = new Vector2(340, 260);
			grid.spacing = new Vector2(25, 25);
			grid.padding = new RectOffset(10, 10, 20, 40);
			grid.childAlignment = TextAnchor.UpperCenter;
			content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;

			var sr = scrollView.GetComponent<ScrollRect>();
			sr.content = contentRect; sr.viewport = viewport.GetComponent<RectTransform>();
			sr.horizontal = false; sr.scrollSensitivity = 50f;

			// Blank Sprite for Fill/Images
			Texture2D tex = new Texture2D(8, 8);
			for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++) tex.SetPixel(x, y, Color.white);
			tex.Apply();
			Sprite blank = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));

			// =========================================================================================
			// GROUP 1: THE BASICS (1 - 10)
			// =========================================================================================

			var c1 = Card(content, "1. Fade In / Out", false, blank, out var t1, out _, out _, out _);
			c1.AppendStep(new FadeConfig { target = t1, from = 1f, to = 0f, duration = 0.3f });
			c1.AppendStep(new FadeConfig { target = t1, from = 0f, to = 1f, duration = 0.3f, delay = 0.2f });

			var c2 = Card(content, "2. Scale Pop", false, blank, out var t2, out _, out _, out _);
			c2.AppendStep(new ScaleConfig { target = t2, from = Vector3.zero, to = Vector3.one, duration = 0.5f, ease = Ease.OutBack });

			var c3 = Card(content, "3. Slide Y Sequence", false, blank, out var t3, out _, out _, out _);
			c3.AppendStep(new SlideConfig { target = t3, from = new Vector2(0, -100), to = Vector2.zero, duration = 0.4f, ease = Ease.OutQuad });
			c3.AppendStep(new WaitConfig { duration = 0.5f });
			c3.AppendStep(new SlideConfig { target = t3, to = new Vector2(0, 100), duration = 0.4f, ease = Ease.InQuad });

			var c4 = Card(content, "4. Rotate 90", false, blank, out var t4, out _, out _, out _);
			c4.AppendStep(new RotateConfig { target = t4, from = 0f, to = 90f, duration = 0.4f, ease = Ease.OutBack });
			c4.AppendStep(new WaitConfig { duration = 0.4f });
			c4.AppendStep(new RotateConfig { target = t4, to = 0f, duration = 0.4f, ease = Ease.InOutSine });

			var c5 = Card(content, "5. SizeDelta Stretch", false, blank, out var t5, out _, out _, out _);
			c5.AppendStep(new SizeDeltaConfig { target = t5, from = new Vector2(100, 100), to = new Vector2(250, 40), duration = 0.4f, ease = Ease.OutBack });
			c5.AppendStep(new WaitConfig { duration = 0.5f });
			c5.AppendStep(new SizeDeltaConfig { target = t5, to = new Vector2(100, 100), duration = 0.3f, ease = Ease.OutQuad });

			var c6 = Card(content, "6. Color Tinting", false, blank, out var t6, out var i6, out _, out _);
			c6.AppendStep(new ColorTintConfig { target = t6, from = i6.color, to = NeonPurple, duration = 0.4f });
			c6.AppendStep(new WaitConfig { duration = 0.2f });
			c6.AppendStep(new ColorTintConfig { target = t6, to = i6.color, duration = 0.4f });

			var c7 = Card(content, "7. Parallel Move & Fade", false, blank, out var t7, out _, out _, out _);
			c7.AppendStep(new SlideConfig { target = t7, from = new Vector2(-100, 0), to = Vector2.zero, duration = 0.6f, ease = Ease.OutExpo });
			c7.AppendStep(new FadeConfig { target = t7, from = 0f, to = 1f, duration = 0.6f, mode = StepMode.Parallel });

			var c8 = Card(content, "8. SetActive Toggle", false, blank, out var t8, out _, out _, out _);
			c8.AppendStep(new SetActiveConfig { target = t8, active = false });
			c8.AppendStep(new WaitConfig { duration = 0.8f });
			c8.AppendStep(new SetActiveConfig { target = t8, active = true });

			var c9 = Card(content, "9. Pivot Scale X", false, blank, out var t9, out _, out _, out _);
			t9.pivot = new Vector2(0f, 0.5f); t9.anchoredPosition = new Vector2(-40, 10);
			c9.AppendStep(new ScaleConfig { target = t9, from = new Vector3(0, 1, 1), to = Vector3.one, duration = 0.5f, ease = Ease.OutBounce });

			var c10 = Card(content, "10. Pivot Scale Y", false, blank, out var t10, out _, out _, out _);
			t10.pivot = new Vector2(0.5f, 0f); t10.anchoredPosition = new Vector2(0, -40);
			c10.AppendStep(new ScaleConfig { target = t10, from = new Vector3(1, 0, 1), to = Vector3.one, duration = 0.5f, ease = Ease.OutBounce });

			// =========================================================================================
			// GROUP 2: GAME FEEL & JUICE (11 - 20)
			// =========================================================================================

			var c11 = Card(content, "11. Punch Scale", false, blank, out var t11, out _, out _, out _);
			c11.AppendStep(new PunchScaleConfig { target = t11, intensity = 0.4f, frequency = 12, duration = 0.6f });

			var c12 = Card(content, "12. Punch Rotate", false, blank, out var t12, out _, out _, out _);
			c12.AppendStep(new PunchRotateConfig { target = t12, angle = 30f, frequency = 15, duration = 0.6f });

			var c13 = Card(content, "13. Heavy Hit (Combo)", false, blank, out var t13, out _, out _, out _);
			c13.AppendStep(new PunchScaleConfig { target = t13, intensity = 0.5f, frequency = 10, duration = 0.5f });
			c13.AppendStep(new PunchRotateConfig { target = t13, randomAngle = true, angle1 = 35f, angle2 = -35f, frequency = 15, duration = 0.5f, mode = StepMode.Parallel });

			var c14 = Card(content, "14. Shake Position", false, blank, out var t14, out _, out _, out _);
			c14.AppendStep(new ShakePositionConfig { target = t14, strength = new Vector3(15, 15, 0), frequency = 30, duration = 0.4f });

			var c15 = Card(content, "15. Shake Rotation", false, blank, out var t15, out _, out _, out _);
			c15.AppendStep(new ShakeRotationConfig { target = t15, strength = new Vector3(0, 0, 25f), frequency = 25, duration = 0.4f });

			var c16 = Card(content, "16. Bounce Drop", false, blank, out var t16, out _, out _, out _);
			c16.AppendStep(new SlideConfig { target = t16, from = new Vector2(0, 150), to = Vector2.zero, duration = 1.0f, ease = Ease.OutBounce });

			var c17 = Card(content, "17. Elastic Spring", false, blank, out var t17, out _, out _, out _);
			c17.AppendStep(new ScaleConfig { target = t17, from = Vector3.zero, to = Vector3.one, duration = 0.8f, ease = Ease.OutElastic });

			var c18 = Card(content, "18. Swing In", false, blank, out var t18, out _, out _, out _);
			c18.AppendStep(new RotateConfig { target = t18, from = -90f, to = 0f, duration = 0.8f, ease = Ease.OutElastic });
			c18.AppendStep(new FadeConfig { target = t18, from = 0f, to = 1f, duration = 0.3f, mode = StepMode.Parallel });

			var c19 = Card(content, "19. Squeeze & Stretch", false, blank, out var t19, out _, out _, out _);
			c19.AppendStep(new ScaleConfig { target = t19, to = new Vector3(1.4f, 0.6f, 1f), duration = 0.15f });
			c19.AppendStep(new ScaleConfig { target = t19, to = new Vector3(0.7f, 1.3f, 1f), duration = 0.15f });
			c19.AppendStep(new ScaleConfig { target = t19, to = Vector3.one, duration = 0.4f, ease = Ease.OutElastic });

			var c20 = Card(content, "20. Error Jiggle", false, blank, out var t20, out var i20, out _, out _);
			c20.AppendStep(new SlideConfig { target = t20, from = Vector2.zero, to = new Vector2(25, 0), duration = 0.1f });
			c20.AppendStep(new SlideConfig { target = t20, to = new Vector2(-25, 0), duration = 0.1f });
			c20.AppendStep(new SlideConfig { target = t20, to = Vector2.zero, duration = 0.1f });
			c20.AppendStep(new ColorTintConfig { target = t20, from = NeonRed, to = i20.color, duration = 0.4f, mode = StepMode.Parallel });

			// =========================================================================================
			// GROUP 3: DATA, BARS & TEXT (21 - 30)
			// =========================================================================================

			var c21 = Card(content, "21. Fill Radial", false, blank, out var t21, out var i21, out _, out _);
			i21.type = Image.Type.Filled; i21.fillMethod = Image.FillMethod.Radial360; i21.fillAmount = 0f;
			c21.AppendStep(new FillAmountConfig { imageTarget = i21, from = 0f, to = 1f, duration = 1.0f, ease = Ease.InOutSine });

			var c22 = Card(content, "22. Fill Linear", false, blank, out var t22, out var i22, out _, out _);
			i22.type = Image.Type.Filled; i22.fillMethod = Image.FillMethod.Horizontal; i22.fillAmount = 0f; t22.sizeDelta = new Vector2(250, 30);
			c22.AppendStep(new FillAmountConfig { imageTarget = i22, from = 0f, to = 1f, duration = 0.8f, ease = Ease.OutCubic });

			var c23 = Card(content, "23. Text Counter", false, blank, out var t23, out var i23, out _, out var tx23);
			i23.enabled = false; tx23.gameObject.SetActive(true); tx23.text = "0 / 100"; tx23.fontSize = 32;
			c23.AppendStep(new TextCounterConfig { tmpTarget = tx23, from = 0, to = 100, format = "{0} / 100", duration = 1.5f, ease = Ease.OutQuad });

			var c24 = Card(content, "24. Counter + Scale", false, blank, out var t24, out var i24, out _, out var tx24);
			i24.enabled = false; tx24.gameObject.SetActive(true); tx24.text = "$0"; tx24.fontSize = 40; tx24.color = NeonOrange;
			c24.AppendStep(new ScaleConfig { target = t24, from = Vector3.one, to = Vector3.one * 1.4f, duration = 0.2f });
			c24.AppendStep(new TextCounterConfig { tmpTarget = tx24, from = 0, to = 5000, format = "${0}", duration = 1.5f, mode = StepMode.Parallel });
			c24.AppendStep(new ScaleConfig { target = t24, to = Vector3.one, duration = 0.3f, ease = Ease.OutQuad });

			var c25 = Card(content, "25. Typewriter", false, blank, out var t25, out var i25, out _, out var tx25);
			i25.enabled = false; tx25.gameObject.SetActive(true); tx25.text = "Initializing..."; tx25.fontSize = 20;
			c25.AppendStep(new TypeWriterConfig { tmpTarget = tx25, text = "This is fully automatic\ntyping text functionality!", charsPerSecond = 25f });

			var c26 = Card(content, "26. Set Image Swap", false, blank, out var t26, out var i26, out _, out _);
			c26.AppendStep(new SetImageConfig { imageTarget = i26, sprite = null });
			c26.AppendStep(new WaitConfig { duration = 0.5f });
			c26.AppendStep(new SetImageConfig { imageTarget = i26, sprite = blank });

			var c27 = Card(content, "27. Instant Color Swap", false, blank, out var t27, out var i27, out _, out _);
			c27.AppendStep(new SetColorConfig { target = t27, color = NeonRed });
			c27.AppendStep(new WaitConfig { duration = 0.5f });
			c27.AppendStep(new SetColorConfig { target = t27, color = i27.color });

			var c28 = Card(content, "28. Instant Text Swap", false, blank, out var t28, out var i28, out _, out var tx28);
			i28.enabled = false; tx28.gameObject.SetActive(true); tx28.text = "READY"; tx28.fontSize = 30;
			c28.AppendStep(new SetTextConfig { tmpTarget = tx28, text = "SET" }); c28.AppendStep(new WaitConfig { duration = 0.5f });
			c28.AppendStep(new SetTextConfig { tmpTarget = tx28, text = "GO!" }); c28.AppendStep(new WaitConfig { duration = 0.5f });
			c28.AppendStep(new SetTextConfig { tmpTarget = tx28, text = "READY" });

			var c29 = Card(content, "29. CG Block Raycast", false, blank, out var t29, out _, out _, out _);
			c29.AppendStep(new SetCanvasGroupStateConfig { target = t29, blocksRaycasts = OptionalBool.False, interactable = OptionalBool.False });
			c29.AppendStep(new FadeConfig { target = t29, to = 0.2f, duration = 0.3f, mode = StepMode.Parallel });
			c29.AppendStep(new WaitConfig { duration = 1f });
			c29.AppendStep(new SetCanvasGroupStateConfig { target = t29, blocksRaycasts = OptionalBool.True, interactable = OptionalBool.True });
			c29.AppendStep(new FadeConfig { target = t29, to = 1f, duration = 0.3f, mode = StepMode.Parallel });

			var c30 = Card(content, "30. Alpha Visibility", false, blank, out var t30, out _, out _, out _);
			c30.AppendStep(new SetFadeConfig { target = t30, alpha = 0f });
			c30.AppendStep(new WaitConfig { duration = 0.5f });
			c30.AppendStep(new SetFadeConfig { target = t30, alpha = 1f });

			// =========================================================================================
			// GROUP 4: INTERACTION & LOGIC (31 - 40)
			// =========================================================================================

			var c31 = Card(content, "31. Pointer Hover", false, blank, out var t31, out var i31, out _, out var tx31);
			tx31.text = "Hover Me"; tx31.gameObject.SetActive(true); t31.sizeDelta = new Vector2(200, 50); i31.color = BtnDark;
			var s31_h = t31.gameObject.AddComponent<AnimSequencer>();
			var seq31_enter = s31_h.CreateSequence("Enter", TriggerType.OnPointerEnter);
			seq31_enter.AppendStep(new ScaleConfig { target = t31, to = Vector3.one * 1.1f, duration = 0.2f });
			seq31_enter.AppendStep(new ColorTintConfig { target = t31, to = NeonCyan, duration = 0.2f, mode = StepMode.Parallel });
			var seq31_exit = s31_h.CreateSequence("Exit", TriggerType.OnPointerExit);
			seq31_exit.AppendStep(new ScaleConfig { target = t31, to = Vector3.one, duration = 0.2f });
			seq31_exit.AppendStep(new ColorTintConfig { target = t31, to = BtnDark, duration = 0.2f, mode = StepMode.Parallel });
			c31.AppendStep(new PunchScaleConfig { target = t31, intensity = 0.2f, duration = 0.3f }); // Button trigger

			var c32 = Card(content, "32. Pointer Click", false, blank, out var t32, out var i32, out _, out var tx32);
			tx32.text = "Click Card"; tx32.gameObject.SetActive(true); t32.sizeDelta = new Vector2(200, 50); i32.color = BtnDark;
			var s32_c = t32.gameObject.AddComponent<AnimSequencer>();
			var seq32_click = s32_c.CreateSequence("Click", TriggerType.OnClick);
			seq32_click.AppendStep(new ScaleConfig { target = t32, from = Vector3.one * 0.9f, to = Vector3.one, duration = 0.3f, ease = Ease.OutBack });
			seq32_click.AppendStep(new ColorTintConfig { target = t32, from = NeonGreen, to = BtnDark, duration = 0.4f, mode = StepMode.Parallel });
			c32.AppendStep(new SlideConfig { target = t32, to = new Vector2(0, 20), relativeOffset = true, duration = 0.2f }); c32.AppendStep(new SlideConfig { target = t32, to = new Vector2(0, -20), relativeOffset = true, duration = 0.2f });

			var c33 = Card(content, "33. Frame Delay", false, blank, out var t33, out _, out _, out _);
			c33.AppendStep(new WaitConfig { waitMethod = WaitMethod.Frames, frameCount = 30 });
			c33.AppendStep(new ScaleConfig { target = t33, from = Vector3.zero, to = Vector3.one, duration = 0.4f, ease = Ease.OutBack });

			var c34 = Card(content, "34. Trigger Sequence", false, blank, out var t34, out _, out _, out _);
			c34.AppendStep(new TriggerConfig { targetSequenceLabel = "Play", targetSequencer = c1.owner }); // Triggers Card 1
			c34.AppendStep(new PunchScaleConfig { target = t34, intensity = 0.3f, duration = 0.4f });

			var c35 = Card(content, "35. Unity Event", false, blank, out var t35, out var i35, out _, out var tx35);
			tx35.text = "Fires Event"; tx35.gameObject.SetActive(true); t35.sizeDelta = new Vector2(200, 50); i35.color = BtnDark;
			var eventStep = new EventConfig(); UnityEventTools.AddStringPersistentListener(eventStep.onEvent, tx35.SetText, "EVENT FIRED!");
			c35.AppendStep(eventStep);
			c35.AppendStep(new WaitConfig { duration = 1.0f });
			c35.AppendStep(new SetTextConfig { tmpTarget = tx35, text = "Fires Event" });

			var c36 = Card(content, "36. Toggle Switch", false, blank, out var t36, out var i36, out _, out _);
			t36.sizeDelta = new Vector2(100, 50); i36.color = BtnDark;
			var knob = CreateUIObj("Knob", t36, typeof(Image)); knob.GetComponent<Image>().sprite = blank;
			var kRect = knob.GetComponent<RectTransform>(); kRect.anchorMin = new Vector2(0, 0.5f); kRect.anchorMax = new Vector2(0, 0.5f); kRect.sizeDelta = new Vector2(40, 40); kRect.anchoredPosition = new Vector2(25, 0);
			c36.AppendStep(new SlideConfig { target = knob.transform, to = new Vector2(75, 0), duration = 0.2f, ease = Ease.OutBack });
			c36.AppendStep(new ColorTintConfig { target = t36, to = NeonGreen, duration = 0.2f, mode = StepMode.Parallel });
			c36.AppendStep(new WaitConfig { duration = 1.0f });
			c36.AppendStep(new SlideConfig { target = knob.transform, to = new Vector2(25, 0), duration = 0.2f, ease = Ease.InBack });
			c36.AppendStep(new ColorTintConfig { target = t36, to = BtnDark, duration = 0.2f, mode = StepMode.Parallel });

			var c37 = Card(content, "37. Dropdown Open", false, blank, out var t37, out var i37, out _, out var tx37);
			tx37.text = "Select..."; tx37.gameObject.SetActive(true); t37.sizeDelta = new Vector2(200, 40); i37.color = BtnDark;
			var dropBody = CreateUIObj("Body", t37, typeof(Image)); dropBody.GetComponent<Image>().color = BgDark;
			var dbRect = dropBody.GetComponent<RectTransform>(); dbRect.anchorMin = new Vector2(0, 1); dbRect.anchorMax = new Vector2(1, 1); dbRect.pivot = new Vector2(0.5f, 1); dbRect.anchoredPosition = new Vector2(0, -40); dbRect.sizeDelta = new Vector2(0, 0);
			c37.AppendStep(new SizeDeltaConfig { target = dropBody.transform, from = new Vector2(0, 0), to = new Vector2(0, 120), duration = 0.3f, ease = Ease.OutExpo });
			c37.AppendStep(new WaitConfig { duration = 1.0f });
			c37.AppendStep(new SizeDeltaConfig { target = dropBody.transform, to = new Vector2(0, 0), duration = 0.2f, ease = Ease.InExpo });

			var c38 = Card(content, "38. TimeScale Hitstop", false, blank, out var t38, out _, out _, out var tx38);
			tx38.text = "SLOW MO"; tx38.gameObject.SetActive(true); t38.sizeDelta = new Vector2(200, 50);
			c38.AppendStep(new SetTimeScaleConfig { timeScale = 0.1f });
			c38.AppendStep(new RotateConfig { target = t38, to = 180f, relativeOffset = true, duration = 0.5f }); // This will run in unscaled time natively
			c38.AppendStep(new SetTimeScaleConfig { timeScale = 1f });

			var c39 = Card(content, "39. Checkbox Tick", false, blank, out var t39, out var i39, out _, out var tx39);
			tx39.text = "✓"; tx39.gameObject.SetActive(true); tx39.color = Color.white; tx39.fontSize = 40; t39.sizeDelta = new Vector2(60, 60); i39.color = BtnDark;
			c39.AppendStep(new SetFadeConfig { target = tx39.transform, alpha = 0f });
			c39.AppendStep(new ScaleConfig { target = tx39.transform, from = Vector3.one * 3f, to = Vector3.one, duration = 0.3f, ease = Ease.OutBack });
			c39.AppendStep(new FadeConfig { target = tx39.transform, to = 1f, duration = 0.2f, mode = StepMode.Parallel });
			c39.AppendStep(new WaitConfig { duration = 1.0f });
			c39.AppendStep(new FadeConfig { target = tx39.transform, to = 0f, duration = 0.2f });

			var c40 = Card(content, "40. Sequential Array", false, blank, out var t40, out _, out _, out _);
			var gridGo = CreateUIObj("Grid", t40, typeof(GridLayoutGroup));
			Stretch(gridGo.GetComponent<RectTransform>()); var g = gridGo.GetComponent<GridLayoutGroup>(); g.cellSize = new Vector2(20, 20); g.spacing = new Vector2(5, 5);
			Transform[] dots = new Transform[9];
			for (int i = 0; i < 9; i++) { var d = CreateUIObj($"d{i}", gridGo.transform, typeof(Image)); d.GetComponent<Image>().sprite = blank; dots[i] = d.transform; c40.AppendStep(new SetFadeConfig { target = dots[i], alpha = 0f }); }
			for (int i = 0; i < 9; i++) c40.AppendStep(new FadeConfig { target = dots[i], to = 1f, duration = 0.1f });
			for (int i = 0; i < 9; i++) c40.AppendStep(new FadeConfig { target = dots[i], to = 0f, duration = 0.1f });

			// =========================================================================================
			// GROUP 5: COMPLEX POPUPS (41 - 50)
			// =========================================================================================

			var c41 = Card(content, "41. Achievement Toast", false, blank, out var t41, out var i41, out _, out var tx41);
			tx41.text = "ACHIEVEMENT UNLOCKED"; tx41.gameObject.SetActive(true); t41.sizeDelta = new Vector2(280, 60); i41.color = BgDark;
			c41.AppendStep(new SlideConfig { target = t41, from = new Vector2(0, -100), to = Vector2.zero, duration = 0.5f, ease = Ease.OutBack });
			c41.AppendStep(new FadeConfig { target = t41, from = 0f, to = 1f, duration = 0.3f, mode = StepMode.Parallel });
			c41.AppendStep(new WaitConfig { duration = 1.5f });
			c41.AppendStep(new SlideConfig { target = t41, to = new Vector2(0, -100), duration = 0.4f, ease = Ease.InBack });
			c41.AppendStep(new FadeConfig { target = t41, to = 0f, duration = 0.3f, mode = StepMode.Parallel });

			var c42 = Card(content, "42. Quest Complete", false, blank, out var t42, out var i42, out _, out var tx42);
			tx42.text = "QUEST COMPLETE"; tx42.color = NeonYellow; tx42.gameObject.SetActive(true); i42.color = BgDark; t42.sizeDelta = new Vector2(280, 60);
			c42.AppendStep(new SetFadeConfig { target = t42, alpha = 0f });
			c42.AppendStep(new SizeDeltaConfig { target = t42, from = new Vector2(0, 60), to = new Vector2(280, 60), duration = 0.6f, ease = Ease.OutExpo });
			c42.AppendStep(new FadeConfig { target = t42, to = 1f, duration = 0.2f, mode = StepMode.Parallel });
			c42.AppendStep(new PunchScaleConfig { target = t42, intensity = 0.1f, duration = 0.4f, delay = 0.5f });
			c42.AppendStep(new WaitConfig { duration = 1f });
			c42.AppendStep(new FadeConfig { target = t42, to = 0f, duration = 0.3f });

			var c43 = Card(content, "43. Dialogue Reveal", false, blank, out var t43, out var i43, out _, out var tx43);
			tx43.text = ""; tx43.alignment = TextAlignmentOptions.TopLeft; tx43.gameObject.SetActive(true); t43.sizeDelta = new Vector2(300, 80); i43.color = BtnDark;
			c43.AppendStep(new SetFadeConfig { target = t43, alpha = 0f });
			c43.AppendStep(new FadeConfig { target = t43, to = 1f, duration = 0.2f });
			c43.AppendStep(new TypeWriterConfig { tmpTarget = tx43, text = "NPC:\nHello traveler. Stay a while and listen.", charsPerSecond = 30f });
			c43.AppendStep(new WaitConfig { duration = 1.5f });
			c43.AppendStep(new FadeConfig { target = t43, to = 0f, duration = 0.3f });

			var c44 = Card(content, "44. Critical Damage", false, blank, out var t44, out var i44, out _, out var tx44);
			tx44.text = "CRITICAL 999!"; tx44.fontSize = 26; tx44.color = NeonRed; tx44.gameObject.SetActive(true); i44.enabled = false;
			c44.AppendStep(new SetFadeConfig { target = t44, alpha = 1f });
			c44.AppendStep(new ScaleConfig { target = t44, from = Vector3.zero, to = Vector3.one * 1.5f, duration = 0.3f, ease = Ease.OutBack });
			c44.AppendStep(new ShakePositionConfig { target = t44, strength = new Vector3(15, 15, 0), duration = 0.3f, mode = StepMode.Parallel });
			c44.AppendStep(new WaitConfig { duration = 0.5f });
			c44.AppendStep(new FadeConfig { target = t44, to = 0f, duration = 0.4f });

			var c45 = Card(content, "45. Level Up Banner", false, blank, out var t45, out var i45, out _, out var tx45);
			tx45.text = "LEVEL UP"; tx45.color = NeonGreen; tx45.fontSize = 28; tx45.gameObject.SetActive(true); t45.sizeDelta = new Vector2(300, 60); i45.color = BgDark;
			c45.AppendStep(new SizeDeltaConfig { target = t45, from = new Vector2(0, 5), to = new Vector2(300, 5), duration = 0.4f, ease = Ease.OutExpo });
			c45.AppendStep(new SizeDeltaConfig { target = t45, to = new Vector2(300, 60), duration = 0.4f, ease = Ease.OutBack, delay = 0.4f });
			c45.AppendStep(new PunchScaleConfig { target = t45, intensity = 0.1f, duration = 0.3f, delay = 0.8f });

			var c46 = Card(content, "46. Match Found VS", false, blank, out var t46, out var i46, out _, out var tx46);
			tx46.text = "VS"; tx46.fontSize = 46; tx46.fontStyle = FontStyles.Italic | FontStyles.Bold; tx46.gameObject.SetActive(true); i46.enabled = false;
			c46.AppendStep(new ScaleConfig { target = t46, from = Vector3.one * 5f, to = Vector3.one, duration = 0.3f, ease = Ease.InExpo });
			c46.AppendStep(new ShakePositionConfig { target = t46, strength = new Vector3(20, 20, 0), duration = 0.3f, delay = 0.3f });

			var c47 = Card(content, "47. Item Drop Splat", false, blank, out var t47, out var i47, out _, out var tx47);
			tx47.text = "SWORD"; tx47.gameObject.SetActive(true); t47.sizeDelta = new Vector2(80, 80); i47.color = NeonCyan;
			c47.AppendStep(new SlideConfig { target = t47, from = new Vector2(0, 200), to = Vector2.zero, duration = 0.4f, ease = Ease.InQuad });
			c47.AppendStep(new ScaleConfig { target = t47, to = new Vector3(1.5f, 0.5f, 1f), duration = 0.1f, delay = 0.4f }); // Squish
			c47.AppendStep(new ScaleConfig { target = t47, to = Vector3.one, duration = 0.3f, ease = Ease.OutBack, delay = 0.5f }); // Recover

			var c48 = Card(content, "48. Buy Confirmed", false, blank, out var t48, out var i48, out _, out var tx48);
			tx48.text = "$500"; tx48.gameObject.SetActive(true); t48.sizeDelta = new Vector2(200, 50); i48.color = BtnDark;
			c48.AppendStep(new TextCounterConfig { tmpTarget = tx48, from = 500, to = 0, format = "${0}", duration = 1.0f, ease = Ease.OutExpo });
			c48.AppendStep(new ColorTintConfig { target = t48, to = NeonGreen, duration = 0.3f, delay = 1.0f });
			c48.AppendStep(new SetTextConfig { tmpTarget = tx48, text = "PURCHASED" });

			var c49 = Card(content, "49. Combo Multiplier", false, blank, out var t49, out var i49, out _, out var tx49);
			tx49.text = "x1"; tx49.fontSize = 36; tx49.gameObject.SetActive(true); i49.enabled = false; tx49.color = NeonOrange;
			c49.AppendStep(new PunchScaleConfig { target = t49, intensity = 0.4f, duration = 0.2f });
			c49.AppendStep(new SetTextConfig { tmpTarget = tx49, text = "x2" });
			c49.AppendStep(new WaitConfig { duration = 0.3f });
			c49.AppendStep(new PunchScaleConfig { target = t49, intensity = 0.6f, duration = 0.2f });
			c49.AppendStep(new SetTextConfig { tmpTarget = tx49, text = "x3!" });

			var c50 = Card(content, "50. System Glitch", false, blank, out var t50, out var i50, out _, out var tx50);
			tx50.text = "REBOOTING"; tx50.gameObject.SetActive(true); i50.color = Color.black; t50.sizeDelta = new Vector2(250, 80);
			var origC = i50.color;
			c50.AppendStep(new SetColorConfig { target = t50, color = NeonCyan });
			c50.AppendStep(new SetTransformConfig { target = t50, subType = TransformSubType.LocalPosition, value = new Vector3(20, 0, 0), relativeOffset = true }); c50.AppendStep(new WaitConfig { duration = 0.05f });
			c50.AppendStep(new SetTransformConfig { target = t50, subType = TransformSubType.LocalPosition, value = new Vector3(-40, 10, 0), relativeOffset = true }); c50.AppendStep(new WaitConfig { duration = 0.05f });
			c50.AppendStep(new SetTransformConfig { target = t50, subType = TransformSubType.LocalPosition, value = new Vector3(20, -10, 0), relativeOffset = true });
			c50.AppendStep(new SetColorConfig { target = t50, color = NeonRed }); c50.AppendStep(new WaitConfig { duration = 0.1f });
			c50.AppendStep(new SetColorConfig { target = t50, color = origC }); c50.AppendStep(new SlideConfig { target = t50, to = Vector2.zero, duration = 0.05f });

			// =========================================================================================
			// GROUP 6: INFINITE LOOPS (51 - 60) (WITH STOP BUTTONS)
			// =========================================================================================

			var c51 = Card(content, "51. Saving Spinner", true, blank, out var t51, out var i51, out _, out var tx51);
			tx51.text = "C"; tx51.fontSize = 60; tx51.gameObject.SetActive(true); i51.enabled = false;
			c51.AppendStep(new AnchorConfig("L"));
			c51.AppendStep(new RotateConfig { target = t51, to = -360f, relativeOffset = true, duration = 1.0f, ease = Ease.Linear });
			c51.AppendStep(new RepeatConfig("L"));

			var c52 = Card(content, "52. Radar Ping", true, blank, out var t52, out var i52, out _, out _);
			i52.type = Image.Type.Sliced; i52.color = NeonCyan;
			c52.AppendStep(new AnchorConfig("L"));
			c52.AppendStep(new ScaleConfig { target = t52, from = Vector3.zero, to = Vector3.one * 2f, duration = 1.0f, ease = Ease.OutQuad });
			c52.AppendStep(new FadeConfig { target = t52, from = 1f, to = 0f, duration = 1.0f, mode = StepMode.Parallel });
			c52.AppendStep(new WaitConfig { duration = 0.5f });
			c52.AppendStep(new RepeatConfig("L"));

			var c53 = Card(content, "53. Hazard Pulse", true, blank, out var t53, out var i53, out _, out var tx53);
			tx53.text = "HAZARD"; tx53.gameObject.SetActive(true); i53.color = NeonRed; t53.sizeDelta = new Vector2(200, 50);
			c53.AppendStep(new AnchorConfig("L"));
			c53.AppendStep(new FadeConfig { target = t53, to = 0.2f, duration = 0.5f, ease = Ease.InOutSine });
			c53.AppendStep(new FadeConfig { target = t53, to = 1f, duration = 0.5f, ease = Ease.InOutSine });
			c53.AppendStep(new RepeatConfig("L"));

			var c54 = Card(content, "54. HP Heartbeat", true, blank, out var t54, out var i54, out _, out var tx54);
			tx54.text = "♥"; tx54.color = NeonRed; tx54.fontSize = 60; tx54.gameObject.SetActive(true); i54.enabled = false;
			c54.AppendStep(new AnchorConfig("L"));
			c54.AppendStep(new ScaleConfig { target = t54, to = Vector3.one * 1.3f, duration = 0.15f });
			c54.AppendStep(new ScaleConfig { target = t54, to = Vector3.one, duration = 0.15f });
			c54.AppendStep(new WaitConfig { duration = 0.1f });
			c54.AppendStep(new ScaleConfig { target = t54, to = Vector3.one * 1.3f, duration = 0.15f });
			c54.AppendStep(new ScaleConfig { target = t54, to = Vector3.one, duration = 0.5f });
			c54.AppendStep(new RepeatConfig("L"));

			var c55 = Card(content, "55. Typing ...", true, blank, out var t55, out var i55, out _, out var tx55);
			tx55.text = "• • •"; tx55.fontSize = 50; tx55.gameObject.SetActive(true); i55.enabled = false;
			c55.AppendStep(new AnchorConfig("L"));
			c55.AppendStep(new SetTextConfig { tmpTarget = tx55, text = "•" }); c55.AppendStep(new WaitConfig { duration = 0.3f });
			c55.AppendStep(new SetTextConfig { tmpTarget = tx55, text = "• •" }); c55.AppendStep(new WaitConfig { duration = 0.3f });
			c55.AppendStep(new SetTextConfig { tmpTarget = tx55, text = "• • •" }); c55.AppendStep(new WaitConfig { duration = 0.5f });
			c55.AppendStep(new RepeatConfig("L"));

			var c56 = Card(content, "56. Hover Float", true, blank, out var t56, out var i56, out _, out var tx56);
			tx56.text = "MAGIC"; tx56.gameObject.SetActive(true); i56.color = NeonPurple;
			c56.AppendStep(new AnchorConfig("L"));
			c56.AppendStep(new SlideConfig { target = t56, from = Vector2.zero, to = new Vector2(0, 15), duration = 1.0f, ease = Ease.InOutSine });
			c56.AppendStep(new SlideConfig { target = t56, to = new Vector2(0, -15), duration = 1.0f, ease = Ease.InOutSine });
			c56.AppendStep(new RepeatConfig("L"));

			var c57 = Card(content, "57. Sleep Zzz", true, blank, out var t57, out var i57, out _, out var tx57);
			tx57.text = "Z"; tx57.fontSize = 40; tx57.gameObject.SetActive(true); i57.enabled = false;
			c57.AppendStep(new AnchorConfig("L"));
			c57.AppendStep(new SlideConfig { target = t57, from = Vector2.zero, to = new Vector2(20, 50), duration = 1.5f, ease = Ease.OutSine });
			c57.AppendStep(new FadeConfig { target = t57, from = 1f, to = 0f, duration = 1.5f, mode = StepMode.Parallel });
			c57.AppendStep(new ScaleConfig { target = t57, from = Vector3.one * 0.5f, to = Vector3.one * 1.5f, duration = 1.5f, mode = StepMode.Parallel });
			c57.AppendStep(new RepeatConfig("L"));

			var c58 = Card(content, "58. Scanline", true, blank, out var t58, out var i58, out _, out _);
			t58.sizeDelta = new Vector2(200, 5); i58.color = new Color(0, 1f, 0, 0.5f);
			c58.AppendStep(new AnchorConfig("L"));
			c58.AppendStep(new SlideConfig { target = t58, from = new Vector2(0, 80), to = new Vector2(0, -80), duration = 1.5f, ease = Ease.Linear });
			c58.AppendStep(new RepeatConfig("L"));

			var c59 = Card(content, "59. Call Ringing", true, blank, out var t59, out var i59, out _, out var tx59);
			tx59.text = "CALL"; tx59.gameObject.SetActive(true); i59.color = NeonGreen;
			c59.AppendStep(new AnchorConfig("L"));
			c59.AppendStep(new RotateConfig { target = t59, to = 15f, duration = 0.05f });
			c59.AppendStep(new RotateConfig { target = t59, to = -15f, duration = 0.05f });
			c59.AppendStep(new RotateConfig { target = t59, to = 15f, duration = 0.05f });
			c59.AppendStep(new RotateConfig { target = t59, to = 0f, duration = 0.05f });
			c59.AppendStep(new WaitConfig { duration = 1.0f });
			c59.AppendStep(new RepeatConfig("L"));

			var c60 = Card(content, "60. RGB Glow Loop", true, blank, out var t60, out var i60, out _, out var tx60);
			tx60.text = "RGB"; tx60.gameObject.SetActive(true); t60.sizeDelta = new Vector2(150, 50); i60.color = NeonRed;
			c60.AppendStep(new AnchorConfig("L"));
			c60.AppendStep(new ColorTintConfig { target = t60, to = NeonGreen, duration = 0.5f });
			c60.AppendStep(new ColorTintConfig { target = t60, to = NeonCyan, duration = 0.5f });
			c60.AppendStep(new ColorTintConfig { target = t60, to = NeonRed, duration = 0.5f });
			c60.AppendStep(new RepeatConfig("L"));

			// Cleanup & Refresh
			var allSequencers = canvasGO.GetComponentsInChildren<AnimSequencer>(true);
			foreach (var seq in allSequencers) { EditorUtility.SetDirty(seq); }
			Selection.activeGameObject = canvasGO;
			Debug.Log("[AnimSequencer] Ultimate V3 Pro Showcase created! 60 Hand-Crafted Cases.");
		}

		static AnimSequence Card(GameObject parent, string titleText, bool isLoop, Sprite defSprite, out RectTransform visualTarget, out Image img, out CanvasGroup cg, out TextMeshProUGUI txt) {
			var card = CreateUIObj($"Card_{titleText}", parent.transform, typeof(Image), typeof(Outline), typeof(AnimSequencer));
			card.GetComponent<Image>().color = BgStep;
			var outline = card.GetComponent<Outline>(); outline.effectColor = BgDark; outline.effectDistance = new Vector2(2, -2);

			var sequencer = card.GetComponent<AnimSequencer>();
			var sequence = sequencer.CreateSequence("Play", TriggerType.Manual);

			// Title Bar
			var titleBar = CreateUIObj("TitleBar", card.transform, typeof(Image));
			titleBar.GetComponent<Image>().color = BgDark;
			var tbRect = titleBar.GetComponent<RectTransform>();
			tbRect.anchorMin = new Vector2(0, 1); tbRect.anchorMax = new Vector2(1, 1);
			tbRect.pivot = new Vector2(0.5f, 1); tbRect.sizeDelta = new Vector2(0, 35);

			var title = CreateUIObj("TitleTxt", titleBar.transform, typeof(TextMeshProUGUI));
			Stretch(title.GetComponent<RectTransform>());
			var titleTmp = title.GetComponent<TextMeshProUGUI>();
			titleTmp.text = titleText; titleTmp.alignment = TextAlignmentOptions.Center;
			titleTmp.color = new Color(0.7f, 0.75f, 0.8f); titleTmp.fontSize = 15; titleTmp.fontStyle = FontStyles.Bold;

			// Visual Area
			var targetArea = CreateUIObj("Area", card.transform, typeof(Image));
			targetArea.GetComponent<Image>().color = BgStepBody;
			var taRect = targetArea.GetComponent<RectTransform>();
			taRect.anchorMin = Vector2.zero; taRect.anchorMax = Vector2.one;
			taRect.offsetMin = new Vector2(10, 55); taRect.offsetMax = new Vector2(-10, -45);

			var target = CreateUIObj("VisualTarget", targetArea.transform, typeof(CanvasGroup), typeof(Image));
			visualTarget = target.GetComponent<RectTransform>();
			visualTarget.sizeDelta = new Vector2(80, 80);
			cg = target.GetComponent<CanvasGroup>();
			img = target.GetComponent<Image>();
			img.sprite = defSprite; // Fixes FillAmount out of the box!
			img.color = NeonCyan;

			var txtGo = CreateUIObj("TextInfo", target.transform, typeof(TextMeshProUGUI));
			Stretch(txtGo.GetComponent<RectTransform>());
			txt = txtGo.GetComponent<TextMeshProUGUI>();
			txt.alignment = TextAlignmentOptions.Center; txt.color = Color.white; txt.fontSize = 18;
			txtGo.SetActive(false);

			// Button Container
			var btnContainer = CreateUIObj("BtnContainer", card.transform);
			var bcRect = btnContainer.GetComponent<RectTransform>();
			bcRect.anchorMin = new Vector2(0, 0); bcRect.anchorMax = new Vector2(1, 0);
			bcRect.pivot = new Vector2(0.5f, 0); bcRect.anchoredPosition = new Vector2(0, 10);
			bcRect.sizeDelta = new Vector2(-20, 35);

			// Overlaying Buttons instead of LayoutGroup
			var playBtnGo = CreateBtn(btnContainer.transform, "PLAY", NeonGreen);
			Stretch(playBtnGo.GetComponent<RectTransform>());
			UnityEventTools.AddStringPersistentListener(playBtnGo.GetComponent<Button>().onClick, sequencer.PlayByLabel, "Play");

			if (isLoop) {
				var stopBtnGo = CreateBtn(btnContainer.transform, "STOP", NeonRed);
				Stretch(stopBtnGo.GetComponent<RectTransform>());
				stopBtnGo.SetActive(false);

				// When Play is clicked -> Hide Play, Show Stop
				UnityEventTools.AddBoolPersistentListener(playBtnGo.GetComponent<Button>().onClick, playBtnGo.SetActive, false);
				UnityEventTools.AddBoolPersistentListener(playBtnGo.GetComponent<Button>().onClick, stopBtnGo.SetActive, true);

				// When Stop is clicked -> Stop Sequence, Hide Stop, Show Play
				UnityEventTools.AddStringPersistentListener(stopBtnGo.GetComponent<Button>().onClick, sequencer.StopByLabel, "Play");
				UnityEventTools.AddBoolPersistentListener(stopBtnGo.GetComponent<Button>().onClick, stopBtnGo.SetActive, false);
				UnityEventTools.AddBoolPersistentListener(stopBtnGo.GetComponent<Button>().onClick, playBtnGo.SetActive, true);
			}

			return sequence;
		}

		static GameObject CreateUIObj(string name, Transform parent, params System.Type[] components) {
			var go = new GameObject(name, typeof(RectTransform));
			go.transform.SetParent(parent, false);
			foreach (var c in components) go.AddComponent(c);
			return go;
		}

		static GameObject CreateBtn(Transform parent, string text, Color accent) {
			var btnGo = CreateUIObj($"Btn_{text}", parent, typeof(Image), typeof(Button), typeof(Outline));
			btnGo.GetComponent<Image>().color = BtnDark;
			var outline = btnGo.GetComponent<Outline>(); outline.effectColor = accent; outline.effectDistance = new Vector2(1.5f, -1.5f);
			var btnTxt = CreateUIObj("Text", btnGo.transform, typeof(TextMeshProUGUI));
			Stretch(btnTxt.GetComponent<RectTransform>());
			var bTmp = btnTxt.GetComponent<TextMeshProUGUI>();
			bTmp.text = text; bTmp.alignment = TextAlignmentOptions.Center; bTmp.fontSize = 13; bTmp.color = Color.white; bTmp.fontStyle = FontStyles.Bold;
			return btnGo;
		}

		static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
	}
}
#endif