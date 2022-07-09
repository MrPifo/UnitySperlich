using System;
using System.Collections.Generic;

namespace Sperlich.FSM {
	public class StackFSM<T> where T : struct, Enum {

		private Stack<T> stack;
		/// <summary>
		/// The current active State
		/// </summary>
		public T Active => stack.Count > 0 ? stack.Peek() : default(T);

		public StackFSM() {
			stack = new Stack<T>();
		}

		public void Push(T state) {
			stack.Push(state);
		}

		public void Pop() {
			stack.Pop();
		}
	}
}
