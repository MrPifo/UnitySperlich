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
		Fade = 0,
		Scale = 1,
		Slide = 2,
		Rotate = 3,
		PunchRotate = 4,
		Bounce = 5,
		ColorTint = 6,
		PunchScale = 7,
		TypeWriter = 8,
		TextCounter = 9,
		SetTransform = 10,
		Wait = 11,
		SetText = 12,
		SetColor = 13,
		SetActive = 14,
		Trigger = 15,
		Event = 16,
		SetSprite = 17,
		FadeSpriteColor = 18,
		SetImage = 19,
		Anchor = 20,
		Repeat = 21,
		WaitUntil = 22,
		SetFade = 23
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
}