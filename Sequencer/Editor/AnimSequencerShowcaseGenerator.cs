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

		[MenuItem("Tools/AnimSequencer/Generate Neon-Dark Showcase")]
		public static void GenerateShowcase() {
			var canvasGO = new GameObject("AnimSequencer_NeonShowcase");
			Undo.RegisterCreatedObjectUndo(canvasGO, "Create Showcase");

			var canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			var scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			canvasGO.AddComponent<GraphicRaycaster>();

			if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
				var esGO = new GameObject("EventSystem");
				esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
				esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
			}

			// Main Background
			var bg = CreateUIObj("Background", canvasGO.transform, typeof(Image));
			bg.GetComponent<Image>().color = BgDark;
			Stretch(bg.GetComponent<RectTransform>());

			// Header
			var header = CreateUIObj("Header", bg.transform, typeof(TextMeshProUGUI));
			var hRect = header.GetComponent<RectTransform>();
			hRect.anchorMin = new Vector2(0, 1); hRect.anchorMax = new Vector2(1, 1);
			hRect.pivot = new Vector2(0.5f, 1); hRect.sizeDelta = new Vector2(0, 80);
			var hTxt = header.GetComponent<TextMeshProUGUI>();
			hTxt.text = "ANIM SEQUENCER <color=#33e680>PRO</color> SHOWCASE";
			hTxt.alignment = TextAlignmentOptions.Center; hTxt.fontSize = 32; hTxt.color = Color.white;
			hTxt.fontStyle = FontStyles.Bold;

			// Scroll View
			var scrollView = CreateUIObj("ScrollView", bg.transform, typeof(ScrollRect));
			var svRect = scrollView.GetComponent<RectTransform>();
			svRect.anchorMin = Vector2.zero; svRect.anchorMax = Vector2.one;
			svRect.offsetMin = new Vector2(40, 40); svRect.offsetMax = new Vector2(-40, -80);

			var viewport = CreateUIObj("Viewport", scrollView.transform, typeof(Image), typeof(Mask));
			Stretch(viewport.GetComponent<RectTransform>());
			viewport.GetComponent<Image>().color = Color.white;
			viewport.GetComponent<Mask>().showMaskGraphic = false;

			var content = CreateUIObj("Content", viewport.transform, typeof(GridLayoutGroup), typeof(ContentSizeFitter));
			var contentRect = content.GetComponent<RectTransform>();
			contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
			contentRect.pivot = new Vector2(0.5f, 1);

			// Exakt 5 Elemente pro Reihe: (1920 - 80 Margin) = 1840. 5 * 340 + 4 * 25 = 1700 + 100 = 1800.
			var grid = content.GetComponent<GridLayoutGroup>();
			grid.cellSize = new Vector2(340, 280);
			grid.spacing = new Vector2(25, 25);
			grid.padding = new RectOffset(20, 20, 20, 20);
			grid.childAlignment = TextAnchor.UpperCenter;

			content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;
			var sr = scrollView.GetComponent<ScrollRect>();
			sr.content = contentRect; sr.viewport = viewport.GetComponent<RectTransform>();
			sr.horizontal = false; sr.scrollSensitivity = 45f;

			Texture2D tex = new Texture2D(4, 4);
			for (int i = 0; i < 16; i++) tex.SetPixel(i % 4, i / 4, Color.white); tex.Apply();
			Sprite blank = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

			// =========================================================================================
			// GROUP 1: THE BASICS (1 - 10)
			// =========================================================================================

			var s1 = Card(content, "1. Fade InOut", false, out var t1, out _, out _, out _);
			s1.AppendStep(new FadeConfig { target = t1, from = 1f, to = 0f, duration = 0.3f });
			s1.AppendStep(new FadeConfig { target = t1, from = 0f, to = 1f, duration = 0.3f, delay = 0.2f });

			var s2 = Card(content, "2. Slide XY Combo", false, out var t2, out _, out _, out _);
			s2.AppendStep(new SlideConfig { target = t2, to = new Vector2(50, 50), relativeOffset = true, duration = 0.4f, ease = Ease.OutQuad });
			s2.AppendStep(new SlideConfig { target = t2, to = new Vector2(-50, -50), relativeOffset = true, duration = 0.4f, ease = Ease.InQuad });

			var s3 = Card(content, "3. Scale Pop", false, out var t3, out _, out _, out _);
			s3.AppendStep(new ScaleConfig { target = t3, from = Vector3.zero, to = Vector3.one, duration = 0.5f, ease = Ease.OutBack });

			var s4 = Card(content, "4. Rotate 90°", false, out var t4, out _, out _, out _);
			s4.AppendStep(new RotateConfig { target = t4, to = 90f, relativeOffset = true, duration = 0.5f, ease = Ease.InOutSine });

			var s5 = Card(content, "5. Color Tinting", false, out var t5, out var i5, out _, out _);
			s5.AppendStep(new ColorTintConfig { target = t5, from = i5.color, to = NeonPurple, duration = 0.4f });
			s5.AppendStep(new ColorTintConfig { target = t5, from = NeonPurple, to = i5.color, duration = 0.4f, delay = 0.2f });

			var s6 = Card(content, "6. Size Stretch", false, out var t6, out _, out _, out _);
			s6.AppendStep(new SizeDeltaConfig { target = t6, to = new Vector2(250, 40), duration = 0.4f, ease = Ease.OutBack });
			s6.AppendStep(new SizeDeltaConfig { target = t6, to = new Vector2(100, 100), duration = 0.4f, ease = Ease.OutQuad, delay = 0.2f });

			var s7 = Card(content, "7. Set Active Toggle", false, out var t7, out _, out _, out _);
			s7.AppendStep(new SetActiveConfig { target = t7, active = false });
			s7.AppendStep(new WaitConfig { duration = 0.8f });
			s7.AppendStep(new SetActiveConfig { target = t7, active = true });

			var s8 = Card(content, "8. Pivot Scale Y", false, out var t8, out _, out _, out _);
			t8.pivot = new Vector2(0.5f, 0f); t8.anchoredPosition = new Vector2(0, -40);
			s8.AppendStep(new ScaleConfig { target = t8, from = new Vector3(1, 0, 1), to = Vector3.one, duration = 0.5f, ease = Ease.OutBounce });

			var s9 = Card(content, "9. Pivot Scale X", false, out var t9, out _, out _, out _);
			t9.pivot = new Vector2(0f, 0.5f); t9.anchoredPosition = new Vector2(-40, 10);
			s9.AppendStep(new ScaleConfig { target = t9, from = new Vector3(0, 1, 1), to = Vector3.one, duration = 0.5f, ease = Ease.OutBounce });

			var s10 = Card(content, "10. Parallel Fade & Move", false, out var t10, out _, out _, out _);
			s10.AppendStep(new SlideConfig { target = t10, from = new Vector2(-100, 0), to = Vector2.zero, duration = 0.6f, ease = Ease.OutExpo });
			s10.AppendStep(new FadeConfig { target = t10, from = 0f, to = 1f, duration = 0.6f, mode = StepMode.Parallel });

			// =========================================================================================
			// GROUP 2: GAME FEEL & JUICE (11 - 20)
			// =========================================================================================

			var s11 = Card(content, "11. Punch Scale", false, out var t11, out _, out _, out _);
			s11.AppendStep(new PunchScaleConfig { target = t11, intensity = 0.4f, frequency = 12, duration = 0.6f });

			var s12 = Card(content, "12. Punch Rotate", false, out var t12, out _, out _, out _);
			s12.AppendStep(new PunchRotateConfig { target = t12, angle = 30f, frequency = 15, duration = 0.6f });

			var s13 = Card(content, "13. Heavy Hit", false, out var t13, out _, out _, out _);
			s13.AppendStep(new PunchScaleConfig { target = t13, intensity = 0.5f, frequency = 10, duration = 0.5f });
			s13.AppendStep(new PunchRotateConfig { target = t13, randomAngle = true, angle1 = 35f, angle2 = -35f, frequency = 15, duration = 0.5f, mode = StepMode.Parallel });

			var s14 = Card(content, "14. Shake Position", false, out var t14, out _, out _, out _);
			s14.AppendStep(new ShakePositionConfig { target = t14, strength = new Vector3(15, 15, 0), frequency = 30, duration = 0.4f });

			var s15 = Card(content, "15. Shake Rotation", false, out var t15, out _, out _, out _);
			s15.AppendStep(new ShakeRotationConfig { target = t15, strength = new Vector3(0, 0, 25f), frequency = 25, duration = 0.4f });

			var s16 = Card(content, "16. Bounce Drop", false, out var t16, out _, out _, out _);
			s16.AppendStep(new SlideConfig { target = t16, from = new Vector2(0, 150), to = Vector2.zero, duration = 1.0f, ease = Ease.OutBounce });

			var s17 = Card(content, "17. Elastic Spring", false, out var t17, out _, out _, out _);
			s17.AppendStep(new ScaleConfig { target = t17, from = Vector3.zero, to = Vector3.one, duration = 0.8f, ease = Ease.OutElastic });

			var s18 = Card(content, "18. Swing In", false, out var t18, out _, out _, out _);
			s18.AppendStep(new RotateConfig { target = t18, from = -90f, to = 0f, duration = 0.8f, ease = Ease.OutElastic });
			s18.AppendStep(new FadeConfig { target = t18, from = 0f, to = 1f, duration = 0.3f, mode = StepMode.Parallel });

			var s19 = Card(content, "19. Squeeze & Stretch", false, out var t19, out _, out _, out _);
			s19.AppendStep(new ScaleConfig { target = t19, to = new Vector3(1.4f, 0.6f, 1f), duration = 0.15f }); // Squish down
			s19.AppendStep(new ScaleConfig { target = t19, to = new Vector3(0.7f, 1.3f, 1f), duration = 0.15f }); // Stretch up
			s19.AppendStep(new ScaleConfig { target = t19, to = Vector3.one, duration = 0.4f, ease = Ease.OutElastic });

			var s20 = Card(content, "20. Error Jiggle", false, out var t20, out var i20, out _, out _);
			s20.AppendStep(new SlideConfig { target = t20, to = new Vector2(25, 0), relativeOffset = true, duration = 0.1f });
			s20.AppendStep(new SlideConfig { target = t20, to = new Vector2(-50, 0), relativeOffset = true, duration = 0.1f });
			s20.AppendStep(new SlideConfig { target = t20, to = new Vector2(25, 0), relativeOffset = true, duration = 0.1f });
			s20.AppendStep(new ColorTintConfig { target = t20, from = NeonRed, to = i20.color, duration = 0.4f, mode = StepMode.Parallel });

			// =========================================================================================
			// GROUP 3: UI ELEMENTS & DATA (21 - 30)
			// =========================================================================================

			var s21 = Card(content, "21. Fill Radial", false, out var t21, out var i21, out _, out _);
			i21.sprite = blank; i21.type = Image.Type.Filled; i21.fillMethod = Image.FillMethod.Radial360; i21.fillAmount = 0f;
			s21.AppendStep(new FillAmountConfig { imageTarget = i21, from = 0f, to = 1f, duration = 1.0f, ease = Ease.InOutSine });

			var s22 = Card(content, "22. Fill Linear", false, out var t22, out var i22, out _, out _);
			i22.sprite = blank; i22.type = Image.Type.Filled; i22.fillMethod = Image.FillMethod.Horizontal; i22.fillAmount = 0f; t22.sizeDelta = new Vector2(250, 40);
			s22.AppendStep(new FillAmountConfig { imageTarget = i22, from = 0f, to = 1f, duration = 0.8f, ease = Ease.OutCubic });

			var s23 = Card(content, "23. Text Counter", false, out var t23, out var i23, out _, out var tx23);
			i23.enabled = false; tx23.gameObject.SetActive(true); tx23.text = "0 / 100"; tx23.fontSize = 32;
			s23.AppendStep(new TextCounterConfig { tmpTarget = tx23, from = 0, to = 100, format = "{0} / 100", duration = 1.5f, ease = Ease.OutQuad });

			var s24 = Card(content, "24. Counter + Scale", false, out var t24, out var i24, out _, out var tx24);
			i24.enabled = false; tx24.gameObject.SetActive(true); tx24.text = "$0"; tx24.fontSize = 40; tx24.color = NeonOrange;
			s24.AppendStep(new ScaleConfig { target = t24, from = Vector3.one, to = Vector3.one * 1.4f, duration = 0.2f });
			s24.AppendStep(new TextCounterConfig { tmpTarget = tx24, from = 0, to = 5000, format = "${0}", duration = 1.5f, mode = StepMode.Parallel });
			s24.AppendStep(new ScaleConfig { target = t24, to = Vector3.one, duration = 0.3f, ease = Ease.OutQuad });

			var s25 = Card(content, "25. Typewriter", false, out var t25, out var i25, out _, out var tx25);
			i25.enabled = false; tx25.gameObject.SetActive(true); tx25.text = "Hello World!"; tx25.fontSize = 20;
			s25.AppendStep(new TypeWriterConfig { tmpTarget = tx25, text = "This is fully automatic\ntyping text functionality!", charsPerSecond = 25f });

			var s26 = Card(content, "26. Hover Button", false, out var t26, out var i26, out _, out var tx26);
			tx26.text = "HOVER ME"; tx26.gameObject.SetActive(true); t26.sizeDelta = new Vector2(200, 60); i26.color = BtnDark;
			s26.AppendStep(new ScaleConfig { target = t26, to = Vector3.one * 1.1f, duration = 0.2f });
			s26.AppendStep(new ColorTintConfig { target = t26, to = NeonCyan, duration = 0.2f, mode = StepMode.Parallel });
			s26.AppendStep(new WaitConfig { duration = 1.0f });
			s26.AppendStep(new ScaleConfig { target = t26, to = Vector3.one, duration = 0.2f });
			s26.AppendStep(new ColorTintConfig { target = t26, to = BtnDark, duration = 0.2f, mode = StepMode.Parallel });

			var s27 = Card(content, "27. Toggle Switch", false, out var t27, out var i27, out _, out var tx27);
			t27.sizeDelta = new Vector2(100, 50); i27.color = BtnDark;
			var knob = CreateUIObj("Knob", t27, typeof(Image));
			var kRect = knob.GetComponent<RectTransform>(); kRect.anchorMin = new Vector2(0, 0.5f); kRect.anchorMax = new Vector2(0, 0.5f); kRect.sizeDelta = new Vector2(40, 40); kRect.anchoredPosition = new Vector2(25, 0);
			s27.AppendStep(new SlideConfig { target = knob.transform, to = new Vector2(75, 0), duration = 0.2f, ease = Ease.OutBack });
			s27.AppendStep(new ColorTintConfig { target = t27, to = NeonGreen, duration = 0.2f, mode = StepMode.Parallel });
			s27.AppendStep(new WaitConfig { duration = 1.0f });
			s27.AppendStep(new SlideConfig { target = knob.transform, to = new Vector2(25, 0), duration = 0.2f, ease = Ease.InBack });
			s27.AppendStep(new ColorTintConfig { target = t27, to = BtnDark, duration = 0.2f, mode = StepMode.Parallel });

			var s28 = Card(content, "28. Set Image & Sprite", false, out var t28, out var i28, out _, out _);
			s28.AppendStep(new SetImageConfig { imageTarget = i28, sprite = blank });
			s28.AppendStep(new WaitConfig { duration = 0.8f });
			s28.AppendStep(new SetImageConfig { imageTarget = i28, sprite = null });

			var s29 = Card(content, "29. CG Block Raycast", false, out var t29, out _, out _, out _);
			s29.AppendStep(new SetCanvasGroupStateConfig { target = t29, blocksRaycasts = OptionalBool.False, interactable = OptionalBool.False });
			s29.AppendStep(new FadeConfig { target = t29, to = 0.2f, duration = 0.3f, mode = StepMode.Parallel });
			s29.AppendStep(new WaitConfig { duration = 1f });
			s29.AppendStep(new SetCanvasGroupStateConfig { target = t29, blocksRaycasts = OptionalBool.True, interactable = OptionalBool.True });
			s29.AppendStep(new FadeConfig { target = t29, to = 1f, duration = 0.3f, mode = StepMode.Parallel });

			var s30 = Card(content, "30. Stamp 'APPROVED'", false, out var t30, out var i30, out _, out var tx30);
			tx30.text = "APPROVED"; tx30.color = NeonGreen; tx30.fontSize = 28; tx30.gameObject.SetActive(true); i30.enabled = false;
			s30.AppendStep(new SetFadeConfig { target = t30, alpha = 0f });
			s30.AppendStep(new ScaleConfig { target = t30, from = Vector3.one * 5f, to = Vector3.one, duration = 0.3f, ease = Ease.InExpo });
			s30.AppendStep(new FadeConfig { target = t30, to = 1f, duration = 0.2f, mode = StepMode.Parallel });
			s30.AppendStep(new ShakePositionConfig { target = t30, strength = new Vector3(10, 10, 0), duration = 0.2f });

			// =========================================================================================
			// GROUP 4: GAME LOGIC & POPUPS (31 - 40)
			// =========================================================================================

			var s31 = Card(content, "31. Achievement Toast", false, out var t31, out var i31, out _, out var tx31);
			tx31.text = "ACHIEVEMENT UNLOCKED"; tx31.gameObject.SetActive(true); t31.sizeDelta = new Vector2(280, 60); i31.color = BtnDark;
			s31.AppendStep(new SlideConfig { target = t31, from = new Vector2(0, -100), to = Vector2.zero, duration = 0.5f, ease = Ease.OutBack });
			s31.AppendStep(new FadeConfig { target = t31, from = 0f, to = 1f, duration = 0.3f, mode = StepMode.Parallel });
			s31.AppendStep(new WaitConfig { duration = 1.5f });
			s31.AppendStep(new SlideConfig { target = t31, to = new Vector2(0, -100), duration = 0.4f, ease = Ease.InBack });
			s31.AppendStep(new FadeConfig { target = t31, to = 0f, duration = 0.3f, mode = StepMode.Parallel });

			var s32 = Card(content, "32. Quest Complete Ribbon", false, out var t32, out var i32, out _, out var tx32);
			tx32.text = "QUEST COMPLETE"; tx32.color = NeonOrange; tx32.gameObject.SetActive(true); i32.color = BgDark; t32.sizeDelta = new Vector2(280, 60);
			s32.AppendStep(new SetFadeConfig { target = t32, alpha = 0f });
			s32.AppendStep(new SizeDeltaConfig { target = t32, from = new Vector2(0, 60), to = new Vector2(280, 60), duration = 0.6f, ease = Ease.OutExpo });
			s32.AppendStep(new FadeConfig { target = t32, to = 1f, duration = 0.2f, mode = StepMode.Parallel });
			s32.AppendStep(new PunchScaleConfig { target = t32, intensity = 0.1f, duration = 0.4f, delay = 0.5f });
			s32.AppendStep(new WaitConfig { duration = 1f });
			s32.AppendStep(new FadeConfig { target = t32, to = 0f, duration = 0.3f });

			var s33 = Card(content, "33. Dialogue Reveal", false, out var t33, out var i33, out _, out var tx33);
			tx33.text = ""; tx33.alignment = TextAlignmentOptions.TopLeft; tx33.gameObject.SetActive(true); t33.sizeDelta = new Vector2(300, 80); i33.color = BtnDark;
			s33.AppendStep(new SetFadeConfig { target = t33, alpha = 0f });
			s33.AppendStep(new FadeConfig { target = t33, to = 1f, duration = 0.2f });
			s33.AppendStep(new TypeWriterConfig { tmpTarget = tx33, text = "NPC:\nHello traveler. Stay a while and listen.", charsPerSecond = 30f });
			s33.AppendStep(new WaitConfig { duration = 1.5f });
			s33.AppendStep(new FadeConfig { target = t33, to = 0f, duration = 0.3f });

			var s34 = Card(content, "34. Critical Damage", false, out var t34, out var i34, out _, out var tx34);
			tx34.text = "CRITICAL 999!"; tx34.fontSize = 26; tx34.color = NeonRed; tx34.gameObject.SetActive(true); i34.enabled = false;
			s34.AppendStep(new SetFadeConfig { target = t34, alpha = 1f });
			s34.AppendStep(new ScaleConfig { target = t34, from = Vector3.zero, to = Vector3.one * 1.5f, duration = 0.3f, ease = Ease.OutBack });
			s34.AppendStep(new ShakePositionConfig { target = t34, strength = new Vector3(15, 15, 0), duration = 0.3f, mode = StepMode.Parallel });
			s34.AppendStep(new WaitConfig { duration = 0.5f });
			s34.AppendStep(new FadeConfig { target = t34, to = 0f, duration = 0.4f });

			var s35 = Card(content, "35. Level Up Banner", false, out var t35, out var i35, out _, out var tx35);
			tx35.text = "LEVEL UP"; tx35.color = NeonGreen; tx35.fontSize = 28; tx35.gameObject.SetActive(true); t35.sizeDelta = new Vector2(300, 60); i35.color = BgDark;
			s35.AppendStep(new SizeDeltaConfig { target = t35, from = new Vector2(0, 5), to = new Vector2(300, 5), duration = 0.4f, ease = Ease.OutExpo });
			s35.AppendStep(new SizeDeltaConfig { target = t35, to = new Vector2(300, 60), duration = 0.4f, ease = Ease.OutBack, delay = 0.4f });
			s35.AppendStep(new PunchScaleConfig { target = t35, intensity = 0.1f, duration = 0.3f, delay = 0.8f });

			var s36 = Card(content, "36. Damage Float Up", false, out var t36, out var i36, out _, out var tx36);
			tx36.text = "-45"; tx36.fontSize = 28; tx36.color = Color.white; tx36.gameObject.SetActive(true); i36.enabled = false;
			s36.AppendStep(new SlideConfig { target = t36, from = Vector2.zero, to = new Vector2(30, 80), duration = 0.8f, ease = Ease.OutQuad });
			s36.AppendStep(new FadeConfig { target = t36, from = 1f, to = 0f, duration = 0.4f, delay = 0.4f, mode = StepMode.Parallel });

			var s37 = Card(content, "37. Match Found VS", false, out var t37, out var i37, out _, out var tx37);
			tx37.text = "VS"; tx37.fontSize = 46; tx37.fontStyle = FontStyles.Italic | FontStyles.Bold; tx37.gameObject.SetActive(true); i37.enabled = false;
			s37.AppendStep(new ScaleConfig { target = t37, from = Vector3.one * 5f, to = Vector3.one, duration = 0.3f, ease = Ease.InExpo });
			s37.AppendStep(new ShakePositionConfig { target = t37, strength = new Vector3(20, 20, 0), duration = 0.3f, delay = 0.3f });

			var s38 = Card(content, "38. Item Drop Splat", false, out var t38, out var i38, out _, out var tx38);
			tx38.text = "SWORD"; tx38.gameObject.SetActive(true); t38.sizeDelta = new Vector2(80, 80); i38.color = NeonCyan;
			s38.AppendStep(new SlideConfig { target = t38, from = new Vector2(0, 200), to = Vector2.zero, duration = 0.4f, ease = Ease.InQuad });
			s38.AppendStep(new ScaleConfig { target = t38, to = new Vector3(1.5f, 0.5f, 1f), duration = 0.1f, delay = 0.4f }); // Squish
			s38.AppendStep(new ScaleConfig { target = t38, to = Vector3.one, duration = 0.3f, ease = Ease.OutBack, delay = 0.5f }); // Recover

			var s39 = Card(content, "39. Purchase Confirmed", false, out var t39, out var i39, out _, out var tx39);
			tx39.text = "$500"; tx39.gameObject.SetActive(true); t39.sizeDelta = new Vector2(200, 50); i39.color = BtnDark;
			s39.AppendStep(new TextCounterConfig { tmpTarget = tx39, from = 500, to = 0, format = "${0}", duration = 1.0f, ease = Ease.OutExpo });
			s39.AppendStep(new ColorTintConfig { target = t39, to = NeonGreen, duration = 0.3f, delay = 1.0f });
			s39.AppendStep(new SetTextConfig { tmpTarget = tx39, text = "PURCHASED" });

			var s40 = Card(content, "40. Combo Multiplier", false, out var t40, out var i40, out _, out var tx40);
			tx40.text = "x1"; tx40.fontSize = 36; tx40.gameObject.SetActive(true); i40.enabled = false; tx40.color = NeonOrange;
			s40.AppendStep(new PunchScaleConfig { target = t40, intensity = 0.4f, duration = 0.2f });
			s40.AppendStep(new SetTextConfig { tmpTarget = tx40, text = "x2" });
			s40.AppendStep(new WaitConfig { duration = 0.3f });
			s40.AppendStep(new PunchScaleConfig { target = t40, intensity = 0.6f, duration = 0.2f });
			s40.AppendStep(new SetTextConfig { tmpTarget = tx40, text = "x3!" });

			// =========================================================================================
			// GROUP 5: INFINITE LOOPS (41 - 50) (WITH STOP BUTTONS)
			// =========================================================================================

			var s41 = Card(content, "41. Saving Spinner", true, out var t41, out var i41, out _, out var tx41);
			tx41.text = "C"; tx41.fontSize = 60; tx41.gameObject.SetActive(true); i41.enabled = false;
			s41.AppendStep(new AnchorConfig("L"));
			s41.AppendStep(new RotateConfig { target = t41, to = -360f, relativeOffset = true, duration = 1.0f, ease = Ease.Linear });
			s41.AppendStep(new RepeatConfig("L"));

			var s42 = Card(content, "42. Radar Ping", true, out var t42, out var i42, out _, out _);
			i42.sprite = blank; i42.type = Image.Type.Sliced; i42.color = NeonCyan;
			s42.AppendStep(new AnchorConfig("L"));
			s42.AppendStep(new ScaleConfig { target = t42, from = Vector3.zero, to = Vector3.one * 2f, duration = 1.0f, ease = Ease.OutQuad });
			s42.AppendStep(new FadeConfig { target = t42, from = 1f, to = 0f, duration = 1.0f, mode = StepMode.Parallel });
			s42.AppendStep(new WaitConfig { duration = 0.5f });
			s42.AppendStep(new RepeatConfig("L"));

			var s43 = Card(content, "43. Warning Pulse", true, out var t43, out var i43, out _, out var tx43);
			tx43.text = "HAZARD"; tx43.gameObject.SetActive(true); i43.color = NeonRed; t43.sizeDelta = new Vector2(200, 50);
			s43.AppendStep(new AnchorConfig("L"));
			s43.AppendStep(new FadeConfig { target = t43, to = 0.2f, duration = 0.5f, ease = Ease.InOutSine });
			s43.AppendStep(new FadeConfig { target = t43, to = 1f, duration = 0.5f, ease = Ease.InOutSine });
			s43.AppendStep(new RepeatConfig("L"));

			var s44 = Card(content, "44. Low HP Heartbeat", true, out var t44, out var i44, out _, out var tx44);
			tx44.text = "♥"; tx44.color = NeonRed; tx44.fontSize = 60; tx44.gameObject.SetActive(true); i44.enabled = false;
			s44.AppendStep(new AnchorConfig("L"));
			s44.AppendStep(new ScaleConfig { target = t44, to = Vector3.one * 1.3f, duration = 0.15f });
			s44.AppendStep(new ScaleConfig { target = t44, to = Vector3.one, duration = 0.15f });
			s44.AppendStep(new WaitConfig { duration = 0.1f });
			s44.AppendStep(new ScaleConfig { target = t44, to = Vector3.one * 1.3f, duration = 0.15f });
			s44.AppendStep(new ScaleConfig { target = t44, to = Vector3.one, duration = 0.5f });
			s44.AppendStep(new RepeatConfig("L"));

			var s45 = Card(content, "45. Typing ...", true, out var t45, out var i45, out _, out var tx45);
			tx45.text = "• • •"; tx45.fontSize = 50; tx45.gameObject.SetActive(true); i45.enabled = false;
			s45.AppendStep(new AnchorConfig("L"));
			s45.AppendStep(new SetTextConfig { tmpTarget = tx45, text = "•" }); s45.AppendStep(new WaitConfig { duration = 0.3f });
			s45.AppendStep(new SetTextConfig { tmpTarget = tx45, text = "• •" }); s45.AppendStep(new WaitConfig { duration = 0.3f });
			s45.AppendStep(new SetTextConfig { tmpTarget = tx45, text = "• • •" }); s45.AppendStep(new WaitConfig { duration = 0.5f });
			s45.AppendStep(new RepeatConfig("L"));

			var s46 = Card(content, "46. Hover Float", true, out var t46, out var i46, out _, out var tx46);
			tx46.text = "MAGIC"; tx46.gameObject.SetActive(true); i46.color = NeonPurple;
			s46.AppendStep(new AnchorConfig("L"));
			s46.AppendStep(new SlideConfig { target = t46, to = new Vector2(0, 15), relativeOffset = true, duration = 1.0f, ease = Ease.InOutSine });
			s46.AppendStep(new SlideConfig { target = t46, to = new Vector2(0, -15), relativeOffset = true, duration = 1.0f, ease = Ease.InOutSine });
			s46.AppendStep(new RepeatConfig("L"));

			var s47 = Card(content, "47. Sleep Zzz", true, out var t47, out var i47, out _, out var tx47);
			tx47.text = "Z"; tx47.fontSize = 40; tx47.gameObject.SetActive(true); i47.enabled = false;
			s47.AppendStep(new AnchorConfig("L"));
			s47.AppendStep(new SlideConfig { target = t47, from = Vector2.zero, to = new Vector2(20, 50), duration = 1.5f, ease = Ease.OutSine });
			s47.AppendStep(new FadeConfig { target = t47, from = 1f, to = 0f, duration = 1.5f, mode = StepMode.Parallel });
			s47.AppendStep(new ScaleConfig { target = t47, from = Vector3.one * 0.5f, to = Vector3.one * 1.5f, duration = 1.5f, mode = StepMode.Parallel });
			s47.AppendStep(new RepeatConfig("L"));

			var s48 = Card(content, "48. Scanline Pass", true, out var t48, out var i48, out _, out _);
			t48.sizeDelta = new Vector2(200, 5); i48.color = new Color(0, 1f, 0, 0.5f);
			s48.AppendStep(new AnchorConfig("L"));
			s48.AppendStep(new SlideConfig { target = t48, from = new Vector2(0, 80), to = new Vector2(0, -80), duration = 1.5f, ease = Ease.Linear });
			s48.AppendStep(new RepeatConfig("L"));

			var s49 = Card(content, "49. Incoming Call", true, out var t49, out var i49, out _, out var tx49);
			tx49.text = "CALL"; tx49.gameObject.SetActive(true); i49.color = NeonGreen;
			s49.AppendStep(new AnchorConfig("L"));
			s49.AppendStep(new RotateConfig { target = t49, to = 15f, duration = 0.05f });
			s49.AppendStep(new RotateConfig { target = t49, to = -15f, duration = 0.05f });
			s49.AppendStep(new RotateConfig { target = t49, to = 15f, duration = 0.05f });
			s49.AppendStep(new RotateConfig { target = t49, to = 0f, duration = 0.05f });
			s49.AppendStep(new WaitConfig { duration = 1.0f });
			s49.AppendStep(new RepeatConfig("L"));

			var s50 = Card(content, "50. RGB Gamer Loop", true, out var t50, out var i50, out _, out var tx50);
			tx50.text = "RGB"; tx50.gameObject.SetActive(true); t50.sizeDelta = new Vector2(150, 50); i50.color = NeonRed;
			s50.AppendStep(new AnchorConfig("L"));
			s50.AppendStep(new ColorTintConfig { target = t50, to = NeonGreen, duration = 0.5f });
			s50.AppendStep(new ColorTintConfig { target = t50, to = NeonCyan, duration = 0.5f });
			s50.AppendStep(new ColorTintConfig { target = t50, to = NeonRed, duration = 0.5f });
			s50.AppendStep(new RepeatConfig("L"));

			// Cleanup & Refresh
			var allSequencers = canvasGO.GetComponentsInChildren<AnimSequencer>(true);
			foreach (var seq in allSequencers) { EditorUtility.SetDirty(seq); }
			Selection.activeGameObject = canvasGO;
		}

		static AnimSequence Card(GameObject parent, string titleText, bool isLoop, out RectTransform visualTarget, out Image img, out CanvasGroup cg, out TextMeshProUGUI txt) {
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

			// Visual Target Area
			var targetArea = CreateUIObj("TargetArea", card.transform, typeof(Image));
			targetArea.GetComponent<Image>().color = BgStepBody;
			var taRect = targetArea.GetComponent<RectTransform>();
			taRect.anchorMin = Vector2.zero; taRect.anchorMax = Vector2.one;
			taRect.offsetMin = new Vector2(10, 55); taRect.offsetMax = new Vector2(-10, -45); // Margin for button and title

			var target = CreateUIObj("VisualTarget", targetArea.transform, typeof(CanvasGroup), typeof(Image));
			visualTarget = target.GetComponent<RectTransform>();
			visualTarget.anchoredPosition = Vector2.zero;
			visualTarget.sizeDelta = new Vector2(100, 100);
			cg = target.GetComponent<CanvasGroup>();

			img = target.GetComponent<Image>();
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

			var playBtnGo = CreateBtn(btnContainer.transform, "PLAY", NeonGreen);
			Stretch(playBtnGo.GetComponent<RectTransform>());

			UnityEventTools.AddStringPersistentListener(playBtnGo.GetComponent<Button>().onClick, sequencer.PlayByLabel, "Play");

			if (isLoop) {
				var stopBtnGo = CreateBtn(btnContainer.transform, "STOP", NeonRed);
				Stretch(stopBtnGo.GetComponent<RectTransform>());
				stopBtnGo.SetActive(false);

				UnityAction<bool> setPlayActive = playBtnGo.SetActive;
				UnityAction<bool> setStopActive = stopBtnGo.SetActive;

				UnityEventTools.AddBoolPersistentListener(playBtnGo.GetComponent<Button>().onClick, setPlayActive, false);
				UnityEventTools.AddBoolPersistentListener(playBtnGo.GetComponent<Button>().onClick, setStopActive, true);

				UnityEventTools.AddStringPersistentListener(stopBtnGo.GetComponent<Button>().onClick, sequencer.StopByLabel, "Play");
				UnityEventTools.AddBoolPersistentListener(stopBtnGo.GetComponent<Button>().onClick, setStopActive, false);
				UnityEventTools.AddBoolPersistentListener(stopBtnGo.GetComponent<Button>().onClick, setPlayActive, true);
			}

			return sequence;
		}

		static GameObject CreateUIObj(string name, Transform parent, params System.Type[] components) {
			var go = new GameObject(name, typeof(RectTransform));
			go.transform.SetParent(parent, false);
			foreach (var c in components) go.AddComponent(c);
			return go;
		}

		static GameObject CreateBtn(Transform parent, string text, Color neonOutline) {
			var btnGo = CreateUIObj($"Btn_{text}", parent, typeof(Image), typeof(Button), typeof(Outline));
			btnGo.GetComponent<Image>().color = BtnDark;
			var outline = btnGo.GetComponent<Outline>(); outline.effectColor = neonOutline; outline.effectDistance = new Vector2(1, -1);
			var btnTxt = CreateUIObj("Text", btnGo.transform, typeof(TextMeshProUGUI));
			Stretch(btnTxt.GetComponent<RectTransform>());
			var bTmp = btnTxt.GetComponent<TextMeshProUGUI>();
			bTmp.text = text; bTmp.alignment = TextAlignmentOptions.Center; bTmp.fontSize = 14; bTmp.color = Color.white; bTmp.fontStyle = FontStyles.Bold;
			return btnGo;
		}

		static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
	}
}
#endif