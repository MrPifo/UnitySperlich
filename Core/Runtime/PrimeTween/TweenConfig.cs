using UnityEngine;
using PrimeTween;

namespace Sperlich.Extensions.PrimeTween {
	[System.Serializable]
	public struct TweenConf {

		[Min(0.0001f)]
		public float duration;
		public Ease ease;

		public TweenConf(float duration, Ease ease = Ease.InOutSine) {
			this.duration = duration;
			this.ease = ease;
		}
	}
	[System.Serializable]
	public struct ShakeTeenConf {

		[Min(0.0001f)]
		public float duration;
		public int frequency;
		public Ease ease;

		public ShakeTeenConf(float duration, int frequency = 10, Ease ease = Ease.InOutSine) {
			this.duration = duration;
			this.ease = ease;
			this.frequency = frequency;
		}
	}
}