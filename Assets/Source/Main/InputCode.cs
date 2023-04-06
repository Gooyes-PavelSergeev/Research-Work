using System;
using Gooyes.Tools;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Research.Main
{
    public class InputCode
    {
        private const int BIT_DEPTH = 4;
        private const int VALUES_AVAILABLE = 1 << BIT_DEPTH;

        private float _magnitude = 1f;
        private float _frequency;
        private float _period;

        private Processor _processor;
        public ClockTrigger ManualTrigger { get; set; }
        private Graph.Graph _inputGraph;

        private List<int> _hardInput = new List<int>
        {
            0b0010,
            0b0001,
            0b0000,
            0b1111,
            0b0000,
            0b0011,
            0b0010,
        };

        /*private List<int> _hardInput = new List<int>
        {
            0b1101,
            0b1110,
            0b1111,
            0b0000,
            0b1111,
            0b1100,
            0b1101,
        };*/

        public InputCode(float magnitude, float frequency, Processor processor, Graph.Graph graph)
        {
            _magnitude = magnitude;
            _frequency = frequency;
            _period = 1 / frequency;
            _processor = processor;
            _inputGraph = graph;
            //Coroutines.StartCoroutine(Update());
        }

        private IEnumerator Update()
        {
            while (true)
            {
                float value = GenerateSinValue();
                _inputGraph.PushValue(value);
                yield return null;
            }
        }

        public Signal GetSignal(SignalType type = SignalType.Hard)
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
            Signal signal = new Signal(valueInt, _magnitude, BIT_DEPTH);
            //_inputGraph.PushValue(valueInt);
            return signal;
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
            float relativeValue = Time.time * _frequency / _hardInput.Count;
            Debug.Log(relativeValue);
            int index = Mathf.RoundToInt(relativeValue) % _hardInput.Count;
            //ProcessCurrentValue(relativeValue);
            //int index = Mathf.RoundToInt(Time.time * _frequency) % _hardInput.Count;
            return _hardInput[index];
        }

        private float GenerateSincValue(float startTime = 4.5f)
        {
            float time = Time.time - startTime;
            float x = 2f * 3.14f * _frequency * time;
            float valueAnalog = 1 * Mathf.Sin(x) / x + 0.25f;
            float valueNormalized = valueAnalog / _magnitude;
            float value = valueNormalized * (VALUES_AVAILABLE - 1);
            return value;
        }


        private float _triggerTime;
        private void ProcessCurrentValue(float value)
        {
            //if (Time.time - _triggerTime < ManualTrigger._clock.period / ManualTrigger._clock.BIT_SENT) return;
            if (Time.time - _triggerTime < 0.1f) return;
            float fractional = Mathf.Abs(value - Mathf.RoundToInt(value));
            float fromCenter = 0.5f - fractional;
            if (Mathf.Abs(fromCenter) < 0.3f) ManualTrigger.Trigger(2);
            else if (fromCenter > 0.3f) ManualTrigger.Trigger(3);
            else
            {
                ManualTrigger.Trigger(0);
                ManualTrigger.Trigger(1);
            }
            _triggerTime = Time.time;
        }
    }

    public enum SignalType
    {
        Sin,
        Sinc,
        Hard
    }
}