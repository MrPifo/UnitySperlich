using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PauseManager {
	public class PauseManager {

		private static HashSet<IPausable> pausables;
		private static PauseManager _instance;
		public static PauseManager Instance {
			get {
				if (_instance == null) {
					_instance = new PauseManager();
				}

				return _instance;
			}
		}
		public static bool IsPaused { get; private set; }

		public PauseManager() {
			_instance = this;
			pausables = new();
		}

		public void Add(IPausable pausable) {
			if (pausables.Contains(pausable) == false) {
				pausables.Add(pausable);
			}
		}
		public void Remove(IPausable pausable) {
			if (pausables.Contains(pausable)) {
				pausables.Remove(pausable);
			}
		}
		public void Pause() {
			IsPaused = true;

			foreach (IPausable p in pausables) {
				try {
					p.OnPause();
				} catch (System.Exception e) {
					Debug.LogException(e);
				}
			}
		}
		public void Resume() {
			IsPaused = false;

			foreach (IPausable p in pausables) {
				try {
					p.OnResume();
				} catch (System.Exception e) {
					Debug.LogException(e);
				}
			}
		}
	}

	public static class PauseManagerExt {

		public static void SubscribePause(this IPausable p) {
			PauseManager.Instance.Add(p);
		}
		public static void UnsubscribePause(this IPausable p) {
			PauseManager.Instance.Add(p);
		}
	}
}