using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Sperlich.Logging {
	public class SLog {

		// ───────────────────────────── Konfiguration ─────────────────────────────
		/// <summary>Ob Millisekunden im Zeitstempel mitgeschrieben werden.</summary>
		public const bool IncludeMilliseconds = true;
		/// <summary>Maximale Anzahl an Backup-Dateien vergangener Sessions.</summary>
		public const int MaxBackupFiles = 10;

		const string LogFileName = "GroveTop.log";
		const string BackupFolderName = "Logs";

		// ───────────────────────────── Interner State ────────────────────────────
		static StreamWriter writer;
		static readonly object fileLock = new object();

		// Entfernt Unity Rich-Text-Tags (color/b/i/size/material/quad) aus dem Dateitext.
		static readonly Regex richTextRegex =
			new Regex(@"<\/?(color|b|i|size|material|quad)(=[^>]*)?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static string FileLogPath => Path.Combine(Application.persistentDataPath, LogFileName);
		static string BackupDirectory => Path.Combine(Application.persistentDataPath, BackupFolderName);

		// ───────────────────────────── Public API ────────────────────────────────
		public static void Error(string message, Category channel = Category.Error) {
#if UNITY_EDITOR
			Debug.LogError($"[<color={ColourOf(channel)}>{channel}</color>]: {message}");
#endif
			WriteToLogfile(channel, message, true);
		}
		public static void Error(Exception e, string message = null) => Error(e, Category.Error, message);
		public static void Error(Exception e, Category channel, string message = null) => Error(e, null, channel, message);
		public static void Error(Exception e, UnityEngine.Object attachObject, Category channel, string message = null) {
			string body = e.Message + "\n" + e.StackTrace;
			if (message != null) {
				body = message + "\n" + body;
			}
#if UNITY_EDITOR
			Debug.LogError($"[<color={ColourOf(channel)}>{channel}</color>]: {body}", attachObject);
#endif
			WriteToLogfile(channel, body, true);
		}

		public static void Log(object message, Category channel = Category.Default) => Log(message, null, channel);
		public static void Log(object message, UnityEngine.Object attachObject, Category channel = Category.Default) {
#if UNITY_EDITOR
			string consoleString = channel == Category.Default
				? message?.ToString()
				: $"[<color={ColourOf(channel)}>{channel}</color>] {message}";
			Debug.Log(consoleString, attachObject);
#endif
			WriteToLogfile(channel, message?.ToString(), false);
		}

		// ───────────────────────────── Lifecycle ─────────────────────────────────
		/// <summary>
		/// Startet eine neue Log-Session: rotiert die vorherige Datei in den Backup-Ordner
		/// und öffnet eine frische Logdatei. Pro Session entsteht so ein Backup
		/// (max. <see cref="MaxBackupFiles"/>).
		/// </summary>
		public static void Initialize() {
			lock (fileLock) {
				CloseWriter();
				RotateBackups();
				OpenWriter(truncate: true);
			}
			Application.quitting -= CloseWriter;
			Application.quitting += CloseWriter;
		}

		/// <summary>Leert die aktuelle Logdatei (ohne Backup).</summary>
		public static void ClearLogFile() {
			lock (fileLock) {
				CloseWriter();
				OpenWriter(truncate: true);
			}
		}

		// ───────────────────────────── Datei-Schreiben ───────────────────────────
		static void WriteToLogfile(Category channel, string message, bool isError) {
			string clean = StripRichText(message ?? "null");
			string line = isError
				? $"{Timestamp()} [{channel}] [ERROR] {clean}"
				: $"{Timestamp()} [{channel}] {clean}";

			lock (fileLock) {
				try {
					// Falls Initialize() (noch) nicht lief – z.B. nach Editor-Recompile –
					// hängen wir an die bestehende Datei an statt sie zu verlieren.
					if (writer == null) {
						OpenWriter(truncate: false);
					}
					writer?.WriteLine(line);
				} catch {
					// Logging darf niemals das Spiel crashen.
				}
			}
		}

		static void OpenWriter(bool truncate) {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(FileLogPath));
				var fs = new FileStream(
					FileLogPath,
					truncate ? FileMode.Create : FileMode.Append,
					FileAccess.Write,
					FileShare.ReadWrite); // erlaubt Live-Lesen während Unity läuft
				writer = new StreamWriter(fs, new UTF8Encoding(false)) { AutoFlush = true };
			} catch {
				writer = null;
			}
		}

		static void CloseWriter() {
			try {
				writer?.Flush();
				writer?.Dispose();
			} catch { }
			writer = null;
		}

		// ───────────────────────────── Backup-Rotation ───────────────────────────
		static void RotateBackups() {
			try {
				if (!File.Exists(FileLogPath)) {
					return;
				}
				Directory.CreateDirectory(BackupDirectory);

				string stamp = File.GetLastWriteTime(FileLogPath).ToString("yyyy-MM-dd_HH-mm-ss");
				string dest = Path.Combine(BackupDirectory, $"GroveTop_{stamp}.log");
				int i = 1;
				while (File.Exists(dest)) {
					dest = Path.Combine(BackupDirectory, $"GroveTop_{stamp}_{i++}.log");
				}

				File.Move(FileLogPath, dest);
				PruneBackups();
			} catch {
				// Rotation darf den Start nicht blockieren.
			}
		}

		static void PruneBackups() {
			try {
				var dir = new DirectoryInfo(BackupDirectory);
				var files = dir.GetFiles("GroveTop_*.log");
				if (files.Length <= MaxBackupFiles) {
					return;
				}
				Array.Sort(files, (a, b) => a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc));
				for (int i = 0; i < files.Length - MaxBackupFiles; i++) {
					try { files[i].Delete(); } catch { }
				}
			} catch { }
		}

		// ───────────────────────────── Helpers ───────────────────────────────────
		static string Timestamp() => DateTime.Now.ToString(
			IncludeMilliseconds ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss");

		static string StripRichText(string message) =>
			string.IsNullOrEmpty(message) ? message : richTextRegex.Replace(message, string.Empty);

		static string ColourOf(Category channel) =>
			channelToColour.TryGetValue(channel, out var c) ? c : "#ffffff";

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
			{ Category.Progression,  "pink" },
			{ Category.Statistik,    "red"},
			{ Category.Terminal,     "grey" },
			{ Category.AI,           "#f7ff0f" },	// yellow
			{ Category.Editor,       "#ffe30f"},	// yellow
			{ Category.MLAgents,     "green"},
			{ Category.Input,        "#7c35f0"},
			{ Category.Error,        "#ff3908"},	// red-orange
			{ Category.Info,         "#ff6200" }
		};

	}
}
