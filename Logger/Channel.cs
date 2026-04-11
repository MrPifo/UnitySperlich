namespace Sperlich.Logging {
	public enum Category : uint {
		/// <summary>
		/// Logs to do with Exceptions
		/// </summary>
		Error,
		/// <summary>
		/// Logs to do with AI
		/// </summary>
		Default,
		/// <summary>
		/// Logs to do with graphics/rendering
		/// </summary>
		Rendering,
		/// <summary>
		/// Logs to do with UI system
		/// </summary>
		UI,
		/// <summary>
		/// Logs to do with sound
		/// </summary>
		Audio,
		/// <summary>
		/// Logs to do with loading
		/// </summary>
		Loading,
		/// <summary>
		/// Logs to do with platform services
		/// </summary>
		Platform,
		/// <summary>
		/// Logs asserts
		/// </summary>
		Assert,
		/// <summary>
		/// Logs to do with systems/generation
		/// </summary>
		System,
		/// <summary>
		/// Logs to do with progress/game saving
		/// </summary>
		SaveGame,
		/// <summary>
		/// Logs to do with GraphicSettings.
		/// </summary>
		Graphics,
		/// <summary>
		/// Logs to do with Gameplay.
		/// </summary>
		Gameplay,
		/// <summary>
		/// Logs to do with Progression
		/// </summary>
		Progression,
		/// <summary>
		/// Logs to do with PlayerStats
		/// </summary>
		Statistik,
		/// <summary>
		/// Logs to do with Terminal-Commands
		/// </summary>
		Terminal,
		/// <summary>
		/// Logs to do with AI-Logic
		/// </summary>
		AI,
		Editor,
		MLAgents,
		Input,
		Info
	}
}