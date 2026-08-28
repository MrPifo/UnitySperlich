using Rewired.Glyphs;
using Rewired.Integration.UnityUI;
using Sperlich.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class UISystem : MonoBehaviour {

	[SerializeField]
	[HideInInspector]
	private SInputModule inputModule;

	[SerializeField]
	[HideInInspector]
	private RewiredEventSystem eventSystem;

	[SerializeField]
	[HideInInspector]
	private GlyphProvider glyphProvider;

	[Header("Navigator")]
	[SerializeField]
	private bool isInactive;

	private bool cursorDisabled;
	private float navCooldown;
	private bool isDisabled;

	public static bool HasSelection => EventSystem != null && EventSystem.currentSelectedGameObject != null;
	public static bool IsPointerOverUI => EventSystem != null && EventSystem.IsPointerOverGameObject();
	public static bool PreventBackgroundDeselect {
		get => Instance != null && Instance.inputModule != null && !Instance.inputModule.deselectIfBackgroundClicked;
		set { if (Instance != null && Instance.inputModule != null) Instance.inputModule.deselectIfBackgroundClicked = !value; }
	}
	public static Selectable Selected => HasSelection ? EventSystem.currentSelectedGameObject.GetComponent<Selectable>() : null;
	public static GameObject SelectedGameObject => HasSelection ? EventSystem.currentSelectedGameObject : null;

	#region Information
	public static bool IsInactive {
		get => Instance.isInactive; private set {
			Instance.isInactive = value;
		}
	}
	public static bool CursorDisabled { get => Instance.cursorDisabled; private set => Instance.cursorDisabled = value; }
	public static bool CooldownActive => Instance.navCooldown > 0f;
	public static bool IsEnabled => Instance.isDisabled == false && IsInactive == false;
	private static UISystem _instance;
	public static UISystem Instance {
		get {
			if (_instance == null) {
				_instance = FindFirstObjectByType<UISystem>(FindObjectsInactive.Include);
			}

			return _instance;
		}
	}
	public static SInputModule InputModule => Instance.inputModule;
	public static EventSystem EventSystem => Instance != null ? Instance.eventSystem : null;
	public static GlyphProvider GlyphProvider => Instance.glyphProvider;
	#endregion

	void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		_instance = this;
		FetchComponents();
	}
	void Update() {
		if (CooldownActive) {
			// Unscaled, damit ein UI-Cooldown nicht einfriert wenn Time.timeScale == 0 (z.B. Pause/Popup).
			navCooldown = Mathf.Max(navCooldown - Time.unscaledDeltaTime, 0f);

			if (navCooldown == 0) {
				Instance.inputModule.enabled = true;
				Instance.eventSystem.enabled = true;
				Instance.isDisabled = false;
			} else {
				return;
			}
		}
	}
	void FetchComponents() {
		if (inputModule == null) {
			inputModule = GetComponentInChildren<SInputModule>();
		}
		if (eventSystem == null) {
			eventSystem = GetComponentInChildren<RewiredEventSystem>();
		}
		if (glyphProvider == null) {
			glyphProvider = GetComponentInChildren<GlyphProvider>();
		}
	}

	public static void SetInactive(bool state) {
		IsInactive = state;
	}
	public static void TriggerCooldown(float cooldown) {
		Instance.navCooldown = cooldown;

		// W�hrend des Cooldowns komplette UI-Interaktion einfrieren
		Instance.inputModule.enabled = false;
		Instance.eventSystem.enabled = false;
		Instance.isDisabled = true;
	}
	public static void Select(Component comp) => Select(comp.gameObject);
	public static void Select(GameObject obj) {
		if (IsInactive || Instance == null) return;

		Instance.eventSystem.SetSelectedGameObject(obj);
	}

	public static void SetFirstSelectedObject(GameObject obj) {
		Instance.eventSystem.firstSelectedGameObject = obj;
	}
	public static void ClearSelection(bool clearAll = false) {
		if (Instance == null) return;

		Instance.eventSystem.SetSelectedGameObject(null);

		if (clearAll) {
			Instance.eventSystem.firstSelectedGameObject = null;
		}
	}
	public static void ShowCursor() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.Confined;

		CursorDisabled = false;
	}
	public static void HideCursor() {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;

		CursorDisabled = true;
	}
	public static T GetSelection<T>() where T : Selectable {
		var selected = SelectedGameObject;
		if (selected == null || selected.TryGetComponent(out T element) == false) {
			return null;
		}

		return element;
	}
	public static bool TryGetSelection<T>(out T result) where T : Selectable {
		var selected = SelectedGameObject;
		if (selected == null || selected.TryGetComponent(out result) == false) {
			result = null;
			return false;
		}

		return true;
	}
	public static bool IsSelection(Component comp) => IsSelection(comp.gameObject);
	public static bool IsSelection(GameObject obj) {
		if(SelectedGameObject != null && SelectedGameObject.Equals(obj)) {
			return true;
		}

		return false;
	}

#if UNITY_EDITOR
	void OnValidate() {
		FetchComponents();
	}
#endif
}