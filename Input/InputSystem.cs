using Rewired;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.Input {
	public class InputSystem : MonoBehaviour {

		[SerializeField]
		private bool _isEnabled;

		public static bool IsEnabled => ReInput.isReady && Instance._isEnabled && Input != null;
		public static Vector2 MousePos => UnityEngine.Input.mousePosition;
		public static Vector2 MouseDelta => Input.GetAxis2D("MouseDeltaX", "MouseDeltaY");

		public static Controller ActiveController { get; private set; }

		private static InputSystem _instance;
		public static Player Input { get; private set; }
		public static Player SystemPlayer => ReInput.players.SystemPlayer;
		public static InputSystem Instance {
			get {
				if(_instance == null) {
					_instance = FindFirstObjectByType<InputSystem>(FindObjectsInactive.Include);
				}
				return _instance;
			}
		}

		protected void Awake() {
			_isEnabled = true;
			_instance = this;

			// Assign default Input if not set
			Input = Input == null ? ReInput.players.GetPlayer(0) : Input;
			ActiveController = SystemPlayer.controllers.Keyboard;
		}
		public static void SetInput(Player _input) {
			Input = _input;
		}
		public static void ToggleInput(bool state) {
			Instance._isEnabled = state;
		}
		public static bool Button(System.Enum en) {
			if (IsEnabled) {
				bool state = Input.GetButton(en.ToString());
				return state;
			} else {
				return false;
			}
		}
		public static bool Button(string key) {
			if (IsEnabled) {
				bool state = Input.GetButton(key);
				return state;
			} else {
				return false;
			}
		}
		public static bool ButtonDown(System.Enum en) {
			if (IsEnabled) {
				bool state = Input.GetButtonDown(en.ToString());
				return state;
			} else {
				return false;
			}
		}
		public static bool ButtonDown(string key) {
			if (IsEnabled) {
				bool state = Input.GetButtonDown(key);
				return state;
			} else {
				return false;
			}
		}
		public static bool ButtonUp(string key) {
			if (IsEnabled) {
				bool state = Input.GetButtonUp(key);
				return state;
			} else {
				return false;
			}
		}
		public static bool AnyButton() {
			return Input.GetAnyButton();
		}
		public static bool AnyButtonDown() {
			return Input.GetAnyButtonDown();
		}
		public static bool Key(UnityEngine.KeyCode key) {
			if(IsEnabled) {
				UnityEngine.Input.GetKey(key);
			}
			return false;
		}
		public static bool KeyDown(UnityEngine.KeyCode key) {
			if (IsEnabled) {
				UnityEngine.Input.GetKeyDown(key);
			}
			return false;
		}
		public static bool KeyUp(UnityEngine.KeyCode key) {
			if (IsEnabled) {
				UnityEngine.Input.GetKeyUp(key);
			}
			return false;
		}
		public static float Axis(System.Enum key) {
			if (IsEnabled) {
				return Input.GetAxis(key.ToString());
			}
			return 0;
		}
		public static float Axis(string key) {
			if(IsEnabled) {
				return Input.GetAxis(key);
			}
			return 0;
		}
		public static Vector2 Axis(string key1, string key2) {
			if(IsEnabled) {
				return Input.GetAxis2D(key1, key2);
			}
			return Vector2.zero;
		}
	}
}