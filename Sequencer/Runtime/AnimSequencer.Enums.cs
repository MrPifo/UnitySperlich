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
		OnBecameNonInteractable = 9,
		OnSelect = 10,
		OnDeselect = 11
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
		MaterialProperty = 51,
		PlayAudio = 29,
		PunchRotate = 4,
		PunchScale = 7,
		Repeat = 21,
		Rotate = 3,
		Scale = 1,
		SetMaterialProperty = 52,
		SetProperty = 50,
		ShakePosition = 24,
		ShakeRotation = 25,
		SizeDelta = 27,
		Slide = 2,
		TextCounter = 9,
		TimeScale = 31,
		Trigger = 15,
		TypeWriter = 8,
		Wait = 11,
		WaitUntil = 22,
		ControlSequence = 53,
		Destroy = 54,
	}

	public enum SetPropertyType {
		Active = 0,
		Transform = 1,
		Color = 2,
		Fade = 3,
		Text = 4,
		Sprite = 5,
		Image = 6,
		CanvasGroupState = 7,
		TimeScale = 8,
		SizeDelta = 9,
		Pivot = 10
	}

	public enum MaterialPropertyType {
		Float = 0,
		Color = 1
	}

	public enum SequenceControlType {
		Stop = 0,
		Complete = 1,
		Pause = 2,
		Resume = 3
	}

	public enum SequenceControlTarget {
		Self = 0,
		Specific = 1,
		All = 2
	}

	public enum WaitMethod { Seconds = 0, Frames = 1 }
	public enum ColorTargetType { Image = 0, Text = 1 }
	public enum TransformSubType { LocalPosition = 0, LocalRotation = 1, LocalScale = 2 }
	public enum OptionalBool { Unchanged = 0, True = 1, False = 2 }
}