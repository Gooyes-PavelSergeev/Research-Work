using System.Collections.Generic;
using UnityEngine;

namespace Research
{
    public class Generator
    {
        private const int BIT_DEPTH = 4;
        private const int VALUES_AVAILABLE = 1 << BIT_DEPTH;

        private float _magnitude = 1f;
        private float _frequency;

        private List<int> _hardInput = new List<int>
        {
            0b0010,
            0b0001,
            0b0000,
            0b1111,
            0b0000,
            0b0000,
            0b0011,
        };

        private Graph _graph;

        public int Output { get; private set; }

        public Generator(Clock clock, Graph inputGraph, float frequency)
        {
            inputGraph.Setup(1 << BIT_DEPTH);
            _graph = inputGraph;
            _frequency = frequency;
            clock.OnTick += OnTick;
        }

        private void OnTick()
        {
            int signal = GetSignal(SignalType.Hard);
            _graph.PushValue(signal);
            Output = signal;
        }

        public int GetSignal(SignalType type)
        {
            float value = 0;
            switch (type)
            {
                case SignalType.Sin:
                    value = GenerateSinValue();
                    break;
                case SignalType.Sinc:
                    value = GenerateSincValue();
                    break;
                case SignalType.Hard:
                    value = GenerateHardValue();
                    break;
            }
            int valueInt = Mathf.RoundToInt(value);
            if (valueInt > 15) valueInt = 15;
            return valueInt;
        }

        private float GenerateSinValue()
        {
            float valueAnalog = 0.5f * Mathf.Sin(2f * 3.14f * _frequency * Time.time) + 0.5f;
            float valueNormalized = valueAnalog / _magnitude;
            float value = valueNormalized * (VALUES_AVAILABLE - 1);
            return value;
        }

        private float GenerateHardValue()
        {
            float relativeValue = Time.time * _frequency;
            int index = Mathf.RoundToInt(relativeValue) % _hardInput.Count;
            return _hardInput[index];
        }

        private float GenerateSincValue()
        {
            float time = Time.time - 4;
            float x = 2f * 3.14f * _frequency * time;
            float valueAnalog = Sinc(x);
            float valueNormalized = valueAnalog / _magnitude;
            float value = valueNormalized * (VALUES_AVAILABLE - 1);
            return value;
        }

        private float Sinc(float x)
        {
            if (x == 0) return 1.25f;
            float value = Mathf.Sin(x) / x + 0.25f;
            return value;
        }

        public enum SignalType
        {
            Sin,
            Sinc,
            Hard
        }
    }
}
