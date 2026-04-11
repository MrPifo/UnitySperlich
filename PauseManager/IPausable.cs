using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Sperlich.PauseManager {
	/// <summary>
	/// Interface for GameObjects that should be pausable
	/// </summary>
	public interface IPausable {

        public bool IsPaused { get; set; }
        public UnityEvent OnPauseEvent { get; set; }
        public UnityEvent OnResumeEvent { get; set; }

        public abstract void OnPause();
        public abstract void OnResume();
    }
}