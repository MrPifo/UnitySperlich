using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace Sperlich.Logging {
	public class SLog {

		public static string FileLogPath => Application.persistentDataPath + "/BattleTanks.log";

		public static void Error(string message, Category channel = Category.Error) {
#if UNITY_EDITOR
			string errorMessage = $"[<color={channelToColour[channel]}>" + channel + "</color>]: " + message;
			Debug.LogError(errorMessage);
#else
		Console.WriteLine($"[ERROR][{channel}] {message}");
#endif
		}
		public static void Error(Exception e, string message = null) => Error(e, Category.Error, message);
		public static void Error(Exception e, Category channel, string message = null) => Error(e, null, channel, message); 
		public static void Error(Exception e, UnityEngine.Object attachObject, Category channel, string message = null) {
#if UNITY_EDITOR
			string errorMessage = e.Message + " \n " + e.StackTrace;
			if (message != null) {
				errorMessage = $"[<color={channelToColour[channel]}>" + channel + "</color>]: " + message + "\n" + e.Message + " \n " + e.StackTrace;
			}
			Debug.LogError(errorMessage, attachObject);
#else
		string errorMessage = e.Message + " \n " + e.StackTrace;
		Console.WriteLine(FormatStringForFile(errorMessage));
#endif
		}
		public static void Log(object message, Category channel = Category.Default) => Log(message, null, channel);
		public static void Log(object message, UnityEngine.Object attachObject, Category channel = Category.Default) {
			string consoleString = string.Empty;

#if UNITY_EDITOR
			if(channel == Category.Default) {
				consoleString = message.ToString();
			} else {
				consoleString = $"[<color={channelToColour[channel]}>" + channel.ToString() + "</color>] " + message;
			}
			
			Debug.Log(consoleString, attachObject);
#else
			Console.WriteLine(FormatStringForFile(message.ToString()));
#endif
		}

		public static void Initialize() {
			ClearLogFile();
		}
		public static void ClearLogFile() {
			if (File.Exists(FileLogPath)) {
				File.WriteAllText(FileLogPath, "");
			}
		}
		private static void WriteToLogfile(string message) {
			try {
				if (File.Exists(FileLogPath) == false) {
					using (var fs = File.Create(FileLogPath)) { }
				}
				using (StreamWriter sw = new StreamWriter(FileLogPath, true, Encoding.UTF8)) {
					sw.WriteLine(message);
				}
			} catch {

			}
		}
		private static string FormatStringForFile(string message) {
			return $"[{DateTime.Now.ToShortTimeString()}] {message}";
		}

		/// <summary>
		/// Map a channel to a colour, using Unity's rich text system
		/// </summary>
		private static readonly Dictionary<Category, string> channelToColour = new Dictionary<Category, string> {
		{ Category.System,       "#24d7ff" },	// blue
		{ Category.Rendering,    "#57bf32" },	// olive-green
		{ Category.Default,      "#ffffff" },	// white
		{ Category.SaveGame,     "#ff26af" },	// magenta
		{ Category.UI,           "#daff61" },	// blue-grey
		{ Category.Audio,        "#1e2be6" },	// blue-purple
		{ Category.Loading,      "#97f531" },	// olive-yellow
		{ Category.Platform,     "#ff3870" },	// dark-grey
		{ Category.Assert,       "#f22" },
		{ Category.Graphics,     "#a473ff" },	// purple-violet
		{ Category.Gameplay,     "#2af79e" },
		{ Category.Progression,      "pink" },
		{ Category.Statistik,  "red"},
		{ Category.Terminal,     "grey" },
		{ Category.AI,           "#f7ff0f" },		// yellow
		{ Category.Editor,       "#ffe30f"},	// yellow
		{ Category.MLAgents,     "green"},
		{ Category.Input,        "#7c35f0"},
		{ Category.Error,    "#ff3908"},    // red-orange
		{ Category.Info,    "#ff6200" }
	};

	}
}