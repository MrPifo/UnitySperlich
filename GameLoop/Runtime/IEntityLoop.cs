namespace Sperlich.GameLoop {
	public interface IEntityLoop {

		/// <summary>
		/// Gets executed every frame if not paused.
		/// </summary>
		public void OnUpdate(float delta) { }
		/// <summary>
		/// Gets executed 60 times per second if not paused.
		/// </summary>
		public void OnFixed(float delta) { }
		/// <summary>
		/// Ticks every 0.1s
		/// </summary>
		/// <param name="delta"></param>
		public void OnTick(float delta) { }

#if UNITY_EDITOR
		public void OnEditorUpdate() { }
#endif
	}
}
