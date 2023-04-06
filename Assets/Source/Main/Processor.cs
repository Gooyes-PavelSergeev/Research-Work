using Gooyes.Tools;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Research.Main
{
    public class Processor : MonoBehaviour
    {
        private InputCode _input;
        private ClockSignal _clock;
        private Signal _lastRegistredSignal;
        [SerializeField] private Graph.Graph _graphInput;
        [SerializeField] private Graph.Graph _graphOutput;
        private Dictionary<int, float> _bitChangeMap;
        private int _lastChangedBit = 0;

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
        }

        public async Task OnClockTickInput(bool clockValue)
        {
            await Task.Delay(0);
            if (!clockValue) return;
            Signal signal = _input.GetSignal();
            _graphInput.PushValue(signal.intValue);
        }

        public void OnClockTick(bool clockValue, int bit)
        {
            if (!clockValue) return;
            Signal signal = _input.GetSignal();

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
            _clock.Deactivate();
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
    }
}
