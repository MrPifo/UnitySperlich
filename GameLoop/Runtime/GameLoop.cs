using Cysharp.Threading.Tasks;
using Sperlich.PauseManager;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.GameLoop {
	public class GameLoop : IPausable {

		public bool IsPaused { get; set; } = new();
		public UnityEvent OnPauseEvent { get; set; } = new();
		public UnityEvent OnResumeEvent { get; set; } = new();
		public CancellationTokenSource CancelToken { get; private set; } = new();
		public Dictionary<GameCycle, List<IEntityLoop>> ActiveLoops { get; private set; } = new();

		public float TickSpeed { get; set; }

		private static GameLoop _instance;
		public static GameLoop Instance {
			get {
				if (_instance == null) {
					_instance = new GameLoop();
				}

				return _instance;
			}
		}

		public GameLoop() {
			foreach (GameCycle l in Enum.GetValues(typeof(GameCycle))) {
				ActiveLoops.Add(l, new());
			}

			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Update];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null) {
								foundNull = true;
								continue;
							}

							try {
								current.OnUpdate(Time.deltaTime);
							} catch (Exception ex) {
								foundNull = true;
								Debug.LogError($"Error in OnUpdate: {ex.Message} {ex.StackTrace}");
							}
						}
					}

					await UniTask.Yield(PlayerLoopTiming.Update);
				}
			}).AttachExternalCancellation(CancelToken.Token);
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Fixed];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null || current.Equals(null)) {
								foundNull = true;
								continue;
							}

							try {
								current.OnFixed(Time.fixedDeltaTime);
							} catch (Exception ex) {
								foundNull = true;
								Debug.LogError($"Error in OnUpdate: {ex.Message} \n {ex.StackTrace}");
							}
						}
					}

					await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Tick];
				bool foundNull = false;
				const float tickSpeed = 0.1f;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null || current.Equals(null)) {
								foundNull = true;
								continue;
							}

							try {
								current.OnTick(tickSpeed);
							} catch (Exception ex) {
								foundNull = true;
								Debug.LogError($"Error in Tick: {ex.Message} \n {ex.StackTrace}");
							}
						}
					}

					await UniTask.WaitForSeconds(tickSpeed, false, PlayerLoopTiming.FixedUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
		}

		public void OnPause() {
			IsPaused = true;
		}
		public void OnResume() {
			IsPaused = false;
		}
		public LoopAction AddListener(Action<float> action, GameCycle cycle, bool autoAddToCycle = true) {
			var loopAction = new LoopAction(cycle, action, autoAddToCycle);
			AddToCycle(loopAction);

			return loopAction;
		}
		public void RemoveListener(LoopAction action) {
			action.RemoveFromCycle();
		}
		public void AddToCycle(IEntityLoop entity) {
			foreach (GameCycle l in System.Enum.GetValues(typeof(GameCycle))) {
				AddToCycle(l, entity);
			}
		}
		public void AddToCycle(GameCycle loop, IEntityLoop entity) {
			if (ActiveLoops[loop].Contains(entity) == false) {
				ActiveLoops[loop].Add(entity);
			}
		}
		public void RemoveFromCycle(IEntityLoop entity) {
			foreach (GameCycle l in System.Enum.GetValues(typeof(GameCycle))) {
				RemoveFromCycle(l, entity);
			}
		}
		public void RemoveFromCycle(GameCycle loop, IEntityLoop entity) {
			if (ActiveLoops[loop].Contains(entity)) {
				ActiveLoops[loop].Remove(entity);
			}
		}
		public void Reset() {
			CancelToken.Cancel();
			OnPauseEvent = new();
			OnResumeEvent = new();
			CancelToken = new CancellationTokenSource();

			foreach (var l in ActiveLoops) {
				l.Value.Clear();
			}
		}
	}

	public class LoopAction : IEntityLoop {

		private Action<float> action;
		private Action onRemoveAction;
		internal readonly GameCycle cycle;

		public LoopAction(GameCycle cycle) {
			this.cycle = cycle;
		}
		public LoopAction(GameCycle cycle, Action<float> action, bool autoAddToCycle = true) {
			this.cycle = cycle;
			this.action = action;

			if (autoAddToCycle) {
				this.AddToCycle(cycle);
			}
		}

		internal void Enable() {
			this.AddToCycle(cycle);
		}
		internal void Disable() {
			this.RemoveFromCycle(cycle);
		}
		public void OnFixed(float delta) {
			action.Invoke(delta);
		}
		public void OnUpdate(float delta) {
			action.Invoke(delta);
		}
		public void OnLateUpdate(float delta) {
			action.Invoke(delta);
		}
		public void OnLateFixedUpdate(float delta) {
			action.Invoke(delta);
		}
		public void OnTick(float delta) {
			action.Invoke(delta);
		}
		public void OnAITick(float delta) {
			action.Invoke(delta);
		}

		public void RemoveFromCycle() {
			onRemoveAction?.Invoke();

			GameLoop.Instance.RemoveFromCycle(this);
		}
		public void RemoveFromCycle(GameCycle cycle) {
			onRemoveAction?.Invoke();

			GameLoop.Instance.RemoveFromCycle(cycle, this);
		}
		public void AddToCycle() {

		}
		public void SetAction(Action<float> action) {
			this.action = action;
		}
		public void OnRemove(Action onRemoveCallback) {
			onRemoveAction = onRemoveCallback;
		}
	}
	public static class GameLoopExt {

		public static void AddToCycle(this IEntityLoop entity) => GameLoop.Instance.AddToCycle(entity);
		public static void AddToCycle(this IEntityLoop entity, GameCycle loop) => GameLoop.Instance.AddToCycle(loop, entity);
		public static void RemoveFromCycle(this IEntityLoop entity, GameCycle loop) => GameLoop.Instance.RemoveFromCycle(loop, entity);
		public static void RemoveFromCycle(this IEntityLoop entity) => GameLoop.Instance.RemoveFromCycle(entity);

	}

	[Flags]
	public enum GameCycle {
		Update = 0,
		Fixed = 1 << 0,
		Tick = 1 << 5,
	}
}