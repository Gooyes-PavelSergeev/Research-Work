using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Research
{
    public class DAC
    {
        private Dictionary<int, float> _changeTimeMap;
        private bool[] _currentInputValue;
        private float _currentOutputNonSmoothed;
        private float _previousOutputNonSmoothed;
        private float _currentOutputValue;

        private Func<int> _input;
        private Clock _clock;

        private int _hardOutputIndex;
        private List<int> _hardOutput = new List<int>
        {
            0b0010,
            0b0011,
            0b0011,
            0b0011,
            0b0011,
            0b0000,
            0b0000,
            0b0000,
            0b0000,
            0b0011,
            0b0111,
            0b1111,

            0b1111,
            0b0111,
            //0b0011,
            0b0011,
            0b0000,
            0b0000,
            0b0000,
            0b0000,
            0b0011,
            0b0011,
            0b0011,
            0b0011,
            0b0010,
            0b0010,
            0b0010,
        };

        public DAC(Clock clock, Func<int> input, Graph outputGraph)
        {
            _currentInputValue = new bool[4];
            _changeTimeMap = new Dictionary<int, float>
            {
                { 0, 0},
                { 1, 0},
                { 2, 0},
                { 3, 0}
            };

            //outputGraph.Setup(1 << 4);
            outputGraph.Setup(5);
            outputGraph.constantInput = () => { return _currentOutputValue; };

            _input = input;
            _clock = clock;

            clock.OnTick += () => OnTick().Forget();
        }

        public void FixedUpdate()
        {
            //float diff = Mathf.Abs(_currentOutputNonSmoothed - _previousOutputNonSmoothed) * 12;
            float diff =  13;
            _currentOutputValue = Mathf.MoveTowards(_currentOutputValue, _currentOutputNonSmoothed, diff * Time.fixedDeltaTime);

            //_currentOutputValue = _currentInputValue.ToInt();
        }

        private async UniTask OnTick()
        {
            bool[] inputValue = _input.Invoke().ToBinaryBool(4);
            //if (!_currentInputValue.HasChangedBits(inputValue)) return;

            ProcessInput(3, inputValue);
            ProcessInput(2, inputValue);

            int delay = _clock.peroidMs / 3 - 1;

            await UniTask.Delay(delay);

            ProcessInput(1, inputValue);

            await UniTask.Delay(delay);

            ProcessInput(0, inputValue);
        }

        private void ProcessInput(int bitNum, bool[] inputValue)
        {
            IEnumerable<KeyValuePair<int, float>> msBits = _changeTimeMap.Where((kvp) => kvp.Key < bitNum);
            bool notEnoughTime = msBits.Any(
                (kvp) =>
                Time.time - kvp.Value < _clock.peroidMs / 2000f &&
                Time.time - kvp.Value != 0);
            if (msBits.Count() == 0) notEnoughTime = false;
            if (notEnoughTime) return;
            bool oldValue = _currentInputValue[bitNum];
            _currentInputValue[bitNum] = inputValue[bitNum];
            _changeTimeMap[bitNum] = Time.time;

            _currentInputValue = _hardOutput[_hardOutputIndex].ToBinaryBool(4);
            _hardOutputIndex++;
            if (_hardOutputIndex >= _hardOutput.Count) _hardOutputIndex = 0;

            DigitalToAnalog();
            //Debug.Log($"Changed bit [{bitNum}]: from {oldValue.ToInt()} to {inputValue[bitNum].ToInt()}. Now value is {_currentInputValue.ToStringBool()}");
        }

        private void DigitalToAnalog()
        {
            int input = _currentInputValue.ToInt();
            float dacCoef = 5f / 15f;
            float analogOutput = dacCoef * input;
            _previousOutputNonSmoothed = _currentOutputNonSmoothed;
            _currentOutputNonSmoothed = analogOutput;
        }

        public Complex[] CreateSpectrum(float from, float to)
        {
            /*List<Signal> values = new List<Signal>();
            foreach (float registerTime in _history.Keys)
            {
                if (registerTime >= from && registerTime <= to)
                {
                    values.Add(_history[registerTime]);
                }
            }
            int n = (int)Math.Pow(2, (int)Math.Log(values.Count, 2));
            Complex[] complexes = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                double phi = values[i].registredTime * 2 * Math.PI;
                complexes[i] = new Complex(values[i].intValue * Math.Cos(phi), -values[i].intValue * Math.Sin(phi));
            }
            complexes = FourierTransform.FFT(complexes);
            complexes = FourierTransform.NFFT(complexes);
            if (complexes.Length == 0) Debug.LogWarning("No spectrum");
            return complexes;*/
            return null;
        }
    }
}
