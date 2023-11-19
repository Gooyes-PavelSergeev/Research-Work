using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Research
{
    public class Clock
    {
        private float _frequency;
        public int peroidMs;

        private CancellationTokenSource _cancelToken;

        public Action OnTick;

        public Clock(float frequency)
        {
            _cancelToken = new CancellationTokenSource();
            _frequency = frequency;
            peroidMs = Mathf.RoundToInt(1000 / _frequency);
            ClockRoutine(_frequency).Forget();
        }

        private async UniTask ClockRoutine(float frequency)
        {
            int period = Mathf.RoundToInt(1000 / frequency);
            await UniTask.Delay(1000);
            while (true)
            {
                await UniTask.Delay(
                    period - 1,
                    delayTiming: PlayerLoopTiming.FixedUpdate,
                    cancellationToken: _cancelToken.Token);
                OnTick?.Invoke();
            }
        }

        public void Dispose()
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }
    }
}
