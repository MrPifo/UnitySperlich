using Newtonsoft.Json;

namespace Sperlich.Serialization {
	public static class JsonAPI {

		public static JsonSerializerSettings DefaultSettings { get; set; } = new JsonSerializerSettings {
			TypeNameHandling = TypeNameHandling.None,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Populate,
			ObjectCreationHandling = ObjectCreationHandling.Auto,
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			Error = (sender, args) => {
				// Fehler markieren, damit Newtonsoft weitermacht
				args.ErrorContext.Handled = true;
			}
		};

		public static T Deserialize<T>(string raw, JsonSerializerSettings settings = null) =>
			JsonConvert.DeserializeObject<T>(raw, settings ?? DefaultSettings);

		public static string Serialize(object data, bool indent = false, JsonSerializerSettings settings = null) {
			settings ??= CloneSettings(DefaultSettings);
			settings.Formatting = indent ? Formatting.Indented : Formatting.None;
			return JsonConvert.SerializeObject(data, settings);
		}

		public static JsonSerializerSettings CloneSettings(JsonSerializerSettings from) {
			return new JsonSerializerSettings(from);
		}
	}
}
