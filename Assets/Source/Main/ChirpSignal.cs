using System.Collections;
using UnityEngine;

namespace Research
{
    public class ChirpSignal
    {
        public float duration;
        public float frequency;
        public float startPhase;
        public float magnitude;
        private float _createTime;

        public ChirpSignal(float duration, float frequency, float startPhase, float magnitude)
        {
            this.duration = duration;
            this.frequency = frequency;
            this.startPhase = startPhase;
            this.magnitude = magnitude;
            _createTime = Time.time;
        }

        public float GetValue(float? timeIn = null)
        {
            float time = timeIn == null ? (Time.time - _createTime) : timeIn.Value;
            if (time < -duration / 2 || time > duration / 2) return 0;
            float value = magnitude * Mathf.Cos(frequency * time + startPhase * time * time / 2);
            return value;
        }
    }
}
