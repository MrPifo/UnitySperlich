using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Sperlich.Performance {
    public class UnityTimer {

        private static Stopwatch Watch { get; set; } = new Stopwatch();
        private static string Title { get; set; }

        public static void Record(string title = "") {
            Title = title;
            Watch = Stopwatch.StartNew();
            Watch.Start();
        }

        public static void Stop() {
            Watch.Stop();
            UnityEngine.Debug.Log(Title + " " + Watch.Elapsed.TotalMilliseconds + "ms");
        }

    }
}