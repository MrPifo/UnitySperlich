using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sperlich.Extensions.PrimeTween {
	public static class PrimeTweenExt {
		#region Shortcuts
		/*public static Tween Shake(this UIBehaviour bvh, float strength, float duration) {

		}*/
		public static Tween ShakeRotate(this Transform bvh, float angle, float duration, int frequency = 10, Ease ease = Ease.InOutSine) {
			return Tween.ShakeLocalRotation(bvh.transform, new Vector3(0, 0, angle), duration, frequency: frequency, easeBetweenShakes: ease);
		}
		public static Tween Fade(this CanvasGroup canvas, float target, float duration, Ease ease = Ease.InOutSine) {
			return Tween.Alpha(canvas, target, duration, ease: ease);
		}
		public static Tween FadeIn(this CanvasGroup canvas, float duration, Ease ease = Ease.InOutSine) {
			return Tween.Alpha(canvas, startValue: 0f, endValue: 1f, duration, ease: ease);
		}
		public static Tween FadeOut(this CanvasGroup canvas, float duration, Ease ease = Ease.InOutSine) {
			return Tween.Alpha(canvas, startValue: 1f, endValue: 0f, duration, ease: ease);
		}
		public static Tween DoScale(this Transform t, float targetValue, float duration, Ease ease = Ease.InOutSine, float? fromScale = null) {
			if (fromScale != null) {
				t.localScale = Vector3.one * fromScale.Value;
			}

			return Tween.Scale(t, targetValue, duration, ease);
		}
		public static Tween DoLocalMove(this Transform t, Vector3 targetPos, float duration, Ease ease = Ease.InOutSine, Vector3? startPos = null) {
			if (startPos != null) {
				t.localPosition = startPos.Value;
			}

			return Tween.LocalPosition(t, targetPos, duration, ease: ease);
		}
		public static Tween DoLocalRotation(this Transform t, Quaternion targetRot, float duration, Ease ease = Ease.InOutSine, Quaternion? startRot = null) {
			if (startRot != null) {
				t.localRotation = startRot.Value;
			}

			return Tween.LocalRotation(t, targetRot, duration, ease);
		}
		#endregion
	}
}