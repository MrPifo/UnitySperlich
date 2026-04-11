using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Codescheduler {
	public class CodeScheduler {

		public enum Method { Update, LateUpdate, Fixed, LateFixed }

		readonly Method waitMethod;
		readonly float requiredTime;
		readonly List<TimestampEvent> timestampEvents;
		readonly List<Func<bool>> breakConditions;

		float time;
		float customTickSpeedMin = -1;
		float customTickSpeedMax = -1;
		ushort limitFPS;
		bool useTimescale = false;
		bool isPlaying = false;
		bool dontInvokeFinally;
		TickData lastTickData;
		Func<bool> pauseCondition;
		Action<TickData> finallyCode;
		Action<TickData> executeCode;

		public float NormTime => lastTickData.normTime;
		public float Time => time;
		public bool IsPlaying => isPlaying;
		protected System.Threading.CancellationTokenSource CancelToken { get; set; }

		public static Func<bool> DefaultPauseCondition { get; set; } = null;

		#region Configuration
		public CodeScheduler(Method waitMethod, float maxRuntime) {
			this.waitMethod = waitMethod;
			this.limitFPS = ushort.MaxValue;
			requiredTime = maxRuntime;
			pauseCondition = DefaultPauseCondition;
			timestampEvents = new();
			breakConditions = new();
			CancelToken = new();
		}
		public CodeScheduler(float customTickTime, float maxRuntime) {
			this.waitMethod = Method.Update;
			this.limitFPS = ushort.MaxValue;
			this.requiredTime = maxRuntime;
			this.customTickSpeedMin = customTickTime;
			pauseCondition = DefaultPauseCondition;
			timestampEvents = new();
			breakConditions = new();
			CancelToken = new();
		}
		public CodeScheduler(Vector2 customTickTimeRange, float maxRuntime) {
			this.waitMethod = Method.Update;
			this.limitFPS = ushort.MaxValue;
			this.requiredTime = maxRuntime;
			this.customTickSpeedMin = customTickTimeRange.x;
			this.customTickSpeedMax = customTickTimeRange.y;
			pauseCondition = DefaultPauseCondition;
			timestampEvents = new();
			breakConditions = new();
			CancelToken = new();
		}
		/// <summary>
		/// Set the Code to execute. No parameters
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public CodeScheduler SetCode(Action code) {
			this.executeCode = _ => code();

			return this;
		}
		public CodeScheduler SetCode(Action<TickData> code) {
			this.executeCode = code;
			return this;
		}
		public CodeScheduler SetFinally(Action endCode) {
			this.finallyCode = _ => endCode();

			return this;
		}
		public CodeScheduler SetFinally(Action<TickData> endCode) {
			finallyCode = endCode;
			return this;
		}
		public CodeScheduler SetPause(Func<bool> condition) {
			this.pauseCondition = condition;

			return this;
		}
		public CodeScheduler RegisterBreakCondition(Func<bool> condition) {
			this.breakConditions.Add(condition);

			return this;
		}
		public CodeScheduler RegisterTimeEvent(Action code, float timestamp, bool usePercentage = false) {
			timestampEvents.Add(new TimestampEvent(code, Mathf.Clamp(timestamp, 0, usePercentage ? 1 : requiredTime), usePercentage));
			
			return this;
		}
		public CodeScheduler LimitFPS(ushort targetFPS) {
			this.limitFPS = targetFPS;

			return this;
		}
		public CodeScheduler SetUseTimescale(bool useTimescale) {
			this.useTimescale = useTimescale;

			return this;
		}
		public CodeScheduler SetTickSpeed(float customTickSpeed) {
			this.customTickSpeedMin = customTickSpeed;

			return this;
		}
		public async UniTask WaitForCompletion() {
			while(isPlaying) {
				await UniTask.Yield();
			}
		}
		#endregion

		#region Execution-API
		public void Run() {
			RunAsync().Forget();
		}
		public async UniTask RunAsync() {
			CancelToken.Cancel();
			CancelToken.Dispose();
			CancelToken = new();

			await RunAsync_Internal();
		}
		public void Interrupt() {
			dontInvokeFinally = true;
			CancelToken.Cancel();
		}
		public void Stop() {
			CancelToken.Cancel();
		}
		#endregion

		#region Internal
		async UniTask RunAsync_Internal() {
			try {
				isPlaying = true;
				lastTickData = new TickData(time, UnityEngine.Time.deltaTime, requiredTime);

				while (Application.isPlaying) {
					if(time >= requiredTime) {
						break;
					}
					if(CancelToken.IsCancellationRequested) {
						break;
					}
					await CheckPause_Internal();
					if (CheckBreakCondition_Internal()) {
						break;
					}

					float deltaTime;
					PlayerLoopTiming timingMethod;
					int appFPS = Mathf.RoundToInt(1f / UnityEngine.Time.deltaTime);

					switch (waitMethod) {
						default:
						case Method.Update:
							deltaTime = UnityEngine.Time.deltaTime;
							timingMethod = PlayerLoopTiming.LastUpdate;
							break;
						case Method.LateUpdate:
							deltaTime = UnityEngine.Time.deltaTime;
							timingMethod = PlayerLoopTiming.PostLateUpdate;
							break;
						case Method.Fixed:
							deltaTime = UnityEngine.Time.fixedDeltaTime;
							timingMethod = PlayerLoopTiming.FixedUpdate;
							break;
						case Method.LateFixed:
							deltaTime = UnityEngine.Time.fixedDeltaTime;
							timingMethod = PlayerLoopTiming.LastFixedUpdate;
							break;
					}

					if(customTickSpeedMin > 0) {
						if(customTickSpeedMax > 0) {
							deltaTime = UnityEngine.Random.Range(customTickSpeedMin, customTickSpeedMax);
							timingMethod = PlayerLoopTiming.TimeUpdate;
						} else {
							deltaTime = customTickSpeedMin;
							timingMethod = PlayerLoopTiming.TimeUpdate;
						}
					} else if (limitFPS < ushort.MaxValue && appFPS > limitFPS) {
						deltaTime = 1f / limitFPS;
					}

					TimeSpan delaySpan = TimeSpan.FromSeconds(deltaTime);
					time += (float)delaySpan.TotalSeconds;
					lastTickData = new TickData(time, (float)delaySpan.TotalSeconds, requiredTime);
					executeCode?.Invoke(lastTickData);
					CheckTimestamps_Internal();
					await UniTask.Delay(delaySpan, useTimescale ? DelayType.DeltaTime : DelayType.UnscaledDeltaTime, timingMethod, CancelToken.Token);
				}
				if(Application.isPlaying == false) {
					return;
				}
			} catch (OperationCanceledException) {
			}
			catch (Exception e) {
				Debug.LogException(e);
			} finally {
				if (Application.isPlaying) {
					isPlaying = false;
					time = 0;
					
					if(dontInvokeFinally == false) {
						finallyCode?.Invoke(lastTickData);
					}

					dontInvokeFinally = false;
					timestampEvents.ForEach(e => e.hasBeenPlayed = false);
				}
			}
		}
		void CheckTimestamps_Internal() {
			if (timestampEvents.Count > 0) {
				foreach (TimestampEvent stamp in timestampEvents) {
					if (stamp.hasBeenPlayed) continue;

					if (stamp.useNormalized && NormTime >= stamp.timestamp) {
						stamp.hasBeenPlayed = true;
						stamp.action.Invoke();
					} else if (stamp.useNormalized == false && time >= stamp.timestamp) {
						stamp.hasBeenPlayed = true;
						stamp.action.Invoke();
					}
				}
			}
		}
		bool CheckBreakCondition_Internal() {
			if(breakConditions.Count > 0) {
				foreach(Func<bool> condition in breakConditions) {
					if(condition.Invoke()) {
						return true;
					}
				}
			}

			CancelToken.Token.ThrowIfCancellationRequested();
			return false;
		}
		async UniTask CheckPause_Internal() {
			if (pauseCondition != null) {
				while (pauseCondition.Invoke()) {
					await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
				}
			}
		}
		#endregion

		public class TimestampEvent {

			public readonly Action action;
			public readonly float timestamp;
			public readonly bool useNormalized;
			public bool hasBeenPlayed;

			public TimestampEvent(Action action, float timestamp, bool useNormalized) {
				this.action = action;
				this.timestamp = timestamp;
				this.useNormalized = useNormalized;
				this.hasBeenPlayed = false;
			}
		}
	}
}