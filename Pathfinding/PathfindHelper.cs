using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR

[ExecuteAlways]
public class PathfindHelper : MonoBehaviour {
	public static Vector3 mousePos;
	public static Vector3 camPos;
	public static Ray mouseRay;
	public static bool mouseDown;
	public static bool controlKeyDown;
	public static bool controlKeyUp;
	public static bool altKeyPressed;

	public static void Refresh() {
		Event guiEvent = Event.current;
		if (guiEvent != null) {
			if (guiEvent.type == EventType.MouseDown && Event.current.button == 0) {
				mouseDown = true;
			}
			if (guiEvent.type == EventType.MouseUp && Event.current.button == 0) {
				mouseDown = false;
			}

			bool control = guiEvent.control;
			controlKeyUp = false;
			if (controlKeyDown == false && control) {
				controlKeyDown = true;
			} else if (controlKeyDown && control == false) {
				controlKeyUp = true;
				controlKeyDown = false;
			}
			altKeyPressed = guiEvent.alt;
			mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
			float dstToDrawPlane = (0 - mouseRay.origin.y) / mouseRay.direction.y;
			mousePos = mouseRay.GetPoint(dstToDrawPlane);

			if (guiEvent.type == EventType.Layout) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			}
		}

		if (Camera.current != null && Application.isPlaying) {
			camPos = Camera.current.transform.position;
		} else {
			if (SceneView.currentDrawingSceneView != null) {
				camPos = SceneView.currentDrawingSceneView.camera.transform.position;
			}
		}
	}
}
#endif
