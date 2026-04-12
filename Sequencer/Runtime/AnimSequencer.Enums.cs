namespace Sperlich.Sequencer {
	public enum TriggerType {
		OnEnable = 0,
		OnDisable = 1,
		OnClick = 2,
		OnPointerEnter = 3,
		OnPointerExit = 4,
		OnPointerDown = 5,
		OnPointerUp = 6,
		Manual = 7,
		OnBecameInteractable = 8,
		OnBecameNonInteractable = 9
	}

	public enum StepMode {
		Sequential = 0,
		Parallel = 1
	}

	public enum AnimType {
		Anchor = 20,
		Bounce = 5,
		ColorTint = 6,
		Event = 16,
		Fade = 0,
		FadeAudio = 30,
		FadeSpriteColor = 18,
		FillAmount = 26,
		MaterialColor = 35,
		MaterialFloat = 33,
		PlayAudio = 29,
		PunchRotate = 4,
		PunchScale = 7,
		Repeat = 21,
		Rotate = 3,
		Scale = 1,
		SetActive = 14,
		SetCanvasGroupState = 28,
		SetColor = 13,
		SetFade = 23,
		SetImage = 19,
		SetMaterialColor = 36,
		SetMaterialFloat = 34,
		SetSprite = 17,
		SetText = 12,
		SetTimeScale = 32,
		SetTransform = 10,
		ShakePosition = 24,
		ShakeRotation = 25,
		SizeDelta = 27,
		Slide = 2,
		TextCounter = 9,
		TimeScale = 31,
		Trigger = 15,
		TypeWriter = 8,
		Wait = 11,
		WaitUntil = 22
	}

	public enum WaitMethod {
		Seconds = 0,
		Frames = 1
	}

	public enum ColorTargetType {
		Image = 0,
		Text = 1
	}

	public enum TransformSubType {
		LocalPosition = 0,
		LocalRotation = 1,
		LocalScale = 2
	}

	public enum OptionalBool {
		Unchanged = 0,
		True = 1,
		False = 2
	}
}