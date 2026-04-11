using static Sperlich.GameSettings.Settings;

namespace Sperlich.GameSettings {
	public interface IGraphicsReceiver {

		public object AppliedValue { get; set; }
		public void OnGameSettingsApplied(GameSetting setting, object value);

	}
}