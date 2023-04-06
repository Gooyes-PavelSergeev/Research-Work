using Gooyes.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Research.Main
{
    public class ClockSignal
    {
        public const int BIT_SENT = 4;

        private float _frequency;
        public float period;
        private int _delayMs;

        private float _bitDelay;

        private Processor _processor;

        private bool _active;

        public ClockSignal(float frequency, Processor processor)
        {
            _active = true;
            _frequency = frequency;
            period = 1 / _frequency;
            _delayMs = Mathf.RoundToInt(period * 1000) * 3 / 10;
            _processor = processor;
            _bitDelay = period / BIT_SENT * 2;
            Coroutines.StartCoroutine(Start());
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            //Action action = async () => await UpdateAll(false);
            //action.Invoke();
            //for (int i = 0; i < BIT_SENT; i++)
            Action updateInput = async () => await UpdateInput(true);
            updateInput.Invoke();
            for (int i = 0; i < BIT_SENT; i++)
            {
                yield return new WaitForSeconds(_bitDelay);
                Coroutines.StartCoroutine(Update(false, i));
            }
        }

        private IEnumerator Update(bool initValue, int bitNum)
        {
            while (_active)
            {
                initValue = !initValue;
                _processor.OnClockTick(initValue , bitNum);
                yield return new WaitForSeconds((float)_delayMs / 1000);
            }
        }

        private async Task UpdateInput(bool initValue)
        {
            while (_active)
            {
                _ = _processor.OnClockTickInput(initValue);
                await Task.Delay(5);
            }
        }

        public void ManualUpdate(int bitNum)
        {
            _processor.OnClockTick(true, bitNum);
        }

        public void Deactivate()
        {
            _active = false;
        }
    }
}
