using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Main
{
    public class TEMP_TEST : MonoBehaviour
    {
        [SerializeField] private int _delay = 200;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Action a = async () => await Calc();
                Action a = async () => await Timer();
                a.Invoke();
            }
        }

        private async Task Calc()
        {
            float startTime = Time.time;
            Debug.Log($"Start time: {startTime}");
            float value = 0;
            await Task.Delay(1);
            for (int i = 0; i < 2000000; i++)
            {
                value += Mathf.Pow(Mathf.Cos(Mathf.PI / 3f), 5) / Mathf.Pow(Mathf.Sin(Mathf.PI / 3f), 5);
            }
            float stopTime = Time.time;
            Debug.Log($"Stop time: {stopTime}. Value = {value}");
            float diff = stopTime - startTime;
            Debug.Log($"Diff: {diff}");
        }

        private async Task Timer()
        {
            List<float> diffs = new List<float>();
            float elapsedCorrected = 0;
            float startTime = Time.time;
            float delay = _delay / 1000f;
            int delayMillisconds = _delay;
            int i = 0;
            while (delay * i <= 15)
            {
                float elapsed = Time.time - startTime;
                float elapsedExpected = delay * i;
                float diff = Mathf.Abs(elapsedExpected - elapsed);

                float currentDiff = Mathf.Abs(elapsedCorrected - elapsed);

                Debug.Log($"Time elapsed: {elapsed}. Expected: {elapsedExpected}. Diff: {diff}. Cur diff {currentDiff}");
                diffs.Add(currentDiff);

                await Task.Delay(delayMillisconds);

                elapsedCorrected = elapsed + delay;
                i++;
            }
            float o = 0;
            for (int j = 0; j < diffs.Count; j++)
            {
                o += diffs[j];
            }
            float avg = o / diffs.Count;
            o = 0;
            for (int j = 0; j < diffs.Count; j++)
            {
                o += Mathf.Pow(avg - diffs[j], 2);
            }
            o /= diffs.Count;
            o = Mathf.Sqrt(o);
            Debug.Log($"Avg deviation: {avg}");
        }
    }
}
