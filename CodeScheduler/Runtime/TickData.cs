using System.Collections;
using System.Collections.Generic;
using Sperlich.Core;
using UnityEngine;

namespace Sperlich.Codescheduler {
    public readonly struct TickData {

        public readonly float time;
        public readonly float normTime;
        public readonly float maxTime;
        public readonly float deltaTime;

        public TickData(float time, float deltaTime, float maxTime) {
            this.time = time;
            this.deltaTime = deltaTime;
            this.maxTime = maxTime;
            this.normTime = time.Remap(0f, maxTime, 0f, 1f);
        }
    }
}