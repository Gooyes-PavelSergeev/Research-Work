using Gooyes.Tools;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;

namespace Research.Main
{
    public class Processor : MonoBehaviour
    {
        private InputCode _input;
        private ClockSignal _clock;
        private ClockTrigger _manualTrigger;
        private Signal _lastRegistredSignal;
        [SerializeField] private Graph.Graph _graphInput;
        [SerializeField] private Graph.Graph _graphOutput;
        private Dictionary<int, float> _bitChangeMap;
        private int _lastChangedBit = 0;
        public bool Active { get; private set; }

        private void Start()
        {
            int bitDepth = 4;
            _bitChangeMap = new Dictionary<int, float>();
            for (int i = 0; i < bitDepth; i++) _bitChangeMap[i] = Time.time;
            _lastRegistredSignal = new Signal(0, 1f, bitDepth);
            _graphInput.Setup(1 << bitDepth);
            _graphOutput.Setup(1 << bitDepth);
            _input = new InputCode(1, 1.5f, this, _graphInput);
            _clock = new ClockSignal(1.5f, this);
            _manualTrigger = new ClockTrigger(_clock);
            _input.ManualTrigger = _manualTrigger;
            Active = true;
        }

        public void Stop()
        {
            Active = false;
            _graphInput.Active = false;
            _graphOutput.Active = false;
            _clock.SetActive(false);
            Time.timeScale = 0f;
        }
        public void Continue()
        {
            Active = true;
            _graphInput.Active = true;
            _graphOutput.Active = true;
            _clock.SetActive(true);
            Time.timeScale = 1f;
        }

        public async Task OnClockTickInput(bool clockValue)
        {
            await Task.Delay(0);
            if (!clockValue || !Active) return;
            Signal signal = _input.GetSignal();
            _graphInput.PushValue(signal.intValue);
        }

        public void OnClockTick(bool clockValue, int bit, Signal signal = null)
        {
            if (!clockValue || !Active) return;
            if (signal == null) signal = _input.GetSignal();

            int[] changedBits = GetChangedBits(signal, _lastRegistredSignal);
            int bitsChanged = changedBits.Length;

            if (bitsChanged == 0) return;
            int bitToProcess = signal.bitDepth - 1 - bit;
            int buffer = bitToProcess;
            if (signal.intValue < _lastRegistredSignal.intValue)
            {
                bitToProcess = changedBits[^1];
                //if (CheckContainsHigherBits(changedBits, bitToProcess))
                //return;
            }
            else
            {
                bitToProcess = changedBits[0];
                //if (CheckContainsLowerBits(changedBits, bitToProcess))
                //return;
            }
            if (Mathf.Abs(bitToProcess - buffer) > 1) return;

            _lastChangedBit = bitToProcess;

            _bitChangeMap[bitToProcess] = signal.registredTime;

            bool[] binaryValueLast = _lastRegistredSignal.binaryValueBool;
            bool[] binaryValueCurrent = signal.binaryValueBool;

            binaryValueLast[signal.bitDepth - 1 - bitToProcess] = binaryValueCurrent[signal.bitDepth - 1 - bitToProcess];

            Signal changedSignal = new Signal(signal, binaryValueLast.ToInt());

            _lastRegistredSignal = changedSignal;
            _graphOutput.PushValue(changedSignal.intValue);
        }

        private int[] GetChangedBits(Signal current, Signal previous)
        {
            int depth = current.bitDepth;
            if (depth != previous.bitDepth)
                throw new Exception("Bit depth of your signals must match");

            List<int> changedBits = new List<int>();
            bool[] currentValue = current.binaryValueBool;
            bool[] previousValue = previous.binaryValueBool;

            for (int i = depth - 1; i >= 0; i--)
            {
                if (currentValue[i] != previousValue[i])
                {
                    changedBits.Add(depth - 1 - i);
                }
            }
            int[] changedBitsArray = changedBits.ToArray();
            if (changedBitsArray.Length == 0) return new int[0];
            return changedBitsArray;
        }

        private void OnApplicationQuit()
        {
            _clock.SetActive(false);
        }

        private bool CheckContainsHigherBits(int[] bits, int targetBit)
        {
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] > targetBit) return true;
            }
            return false;
        }

        private bool CheckContainsLowerBits(int[] bits, int targetBit)
        {
            for (int i = bits.Length - 1; i >= 0; i--)
            {
                if (bits[i] < targetBit) return true;
            }
            return false;
        }

        public float[] CreateSpectrum(int[] input)
        {
            Complex[] complexes = new Complex[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                complexes[i] = new Complex(input[i], 0);
            }
            FourierTransform.DFT(complexes);
            float[] result = new float[complexes.Length];
            for (int i = 0; i < complexes.Length; i++)
            {
                result[i] = (float)complexes[i].Magnitude;
            }
            return result;
        }
    }
}
