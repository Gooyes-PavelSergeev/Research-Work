using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Research
{
    public class Graph : MonoBehaviour
    {
        public bool _isInput;
        [SerializeField] private GraphPoint _pointPrefab;

        [SerializeField] private int _resolution = 10;
        [SerializeField] private int _spectrumResolutionMultiplier = 5;
        [SerializeField] private float _spectrumMaxValue = 4;
        [SerializeField] private bool _interpolateByFactor;
        [SerializeField] private float _interpolateFactor = 5;

        private Vector2 _borders;

        private List<GraphPoint> _points;
        private List<GraphPoint> _instantRange;

        [SerializeField] private int _graphLength = 10;

        [SerializeField] private float _yAxisSize = 5;
        private float _maxInputValue;

        [SerializeField] private float _speed = 3;
        internal Dictionary<GraphPoint, Signal> history;
        public bool Active { get; set; }

        public float CurrentValue { get; private set; }
        private bool _showingRange;
        private float _mostRight;

        public Func<float> constantInput;

        private void Awake()
        {
            Active = true;
            _showingRange = false;
            _borders = new Vector2(-_graphLength / 2, _graphLength / 2);
            _points = new List<GraphPoint>();
            _instantRange = new List<GraphPoint>();
            history = new Dictionary<GraphPoint, Signal>();
            float step = (float)_graphLength / (float)_resolution;
            Vector3 position = Vector3.zero;
            Vector3 scale = Vector3.one * step;
            for (int i = 0; i < _resolution; i++)
            {
                GraphPoint point = Instantiate(_pointPrefab);
                point.Initialize(_borders, this);
                _points.Add(point);
                position.x = (i + 0.5f) * step - _graphLength / 2;
                point.gameObject.name = $"Point {i}";
                point.transform.position = position;
                point.transform.localScale = scale;
                point.transform.SetParent(transform, false);
                point.Move(Vector2.zero);
                history[point] = null;
                //point.signal = null;
            }
            _mostRight = _points[_points.Count - 1].transform.position.x;
            //StartCoroutine(UpdateHistory());
        }

        private void FixedUpdate()
        {
            if (!Active || _showingRange) return;
            for (int i = 0; i < _points.Count; i++)
            {
                GraphPoint point = _points[i];
                point.Move(_speed * Time.deltaTime * Vector2.left);
            }
            if (constantInput != null)
            {
                PushValue(constantInput.Invoke());
            }
        }

        private IEnumerator UpdateHistory()
        {
            yield return new WaitForSeconds(1);
            while (true)
            {
                if (Active)
                {
                    GraphPoint targetPoint = null;
                    float minDist = 100;
                    foreach (GraphPoint point in history.Keys)
                    {
                        float dist = Mathf.Abs(point.transform.position.x - _mostRight);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            targetPoint = point;
                        }
                    }
                    //history[targetPoint] = _isInput ? DAC.Instance._inputHistory.Last().Value : DAC.Instance._history.Last().Value;
                    //if (!_isInput) Debug.Log($"Point {targetPoint.gameObject.name} now has value {currentValueInt}");
                }
                yield return null;
            }
        }

        public void PushValue(float value)
        {
            if (!Active) return;
            if (_showingRange) HideRange();
            float relativeValue = value / _maxInputValue;
            relativeValue *= _yAxisSize;
            GraphPoint targetPoint = history.Last().Key;
            float max = 0;
            foreach (GraphPoint point in history.Keys)
            {
                float time = point.registerTime;
                if (time > max)
                {
                    max = time;
                    targetPoint = point;
                }
            }
            //history[targetPoint] = value;
            if (CurrentValue != relativeValue)
            {
                float step = (float)_graphLength / (float)_resolution * (relativeValue > CurrentValue ? -1 : 1);
                Vector3 position = Vector3.zero;
                Vector3 scale = Vector3.one * step;
                float factor = Mathf.Abs(relativeValue - CurrentValue) / _graphLength;
                int pointAmount = (int)(_resolution * factor) + 1;
                for (int i = 0; i < pointAmount; i++)
                {
                    GraphPoint point = Instantiate(_pointPrefab);
                    point.gameObject.name = "False point";
                    point.Initialize(_borders, this, false);
                    _points.Add(point);
                    position.y = (i + 0.5f) * step + relativeValue;
                    position.x = _borders.y;
                    point.transform.position = position;
                    point.transform.localScale = scale;
                    point.transform.SetParent(transform, false);
                }
            }
            CurrentValue = relativeValue;
        }

        public void Setup(float maxInputValue)
        {
            _maxInputValue = maxInputValue;
        }

        internal void RemovePoint(GraphPoint point)
        {
            if (_points.Contains(point))
            {
                _points.Remove(point);
            }
        }

        public Signal[] GetSignalsInRange(Vector3 from, Vector3 to)
        {
            float fromPos = 0;
            float toPos = 0;
            float minDistFrom = 1000f;
            float minDistTo = 1000f;
            foreach (GraphPoint point in history.Keys)
            {
                float pointPos = point.transform.position.x;
                float distFrom = Mathf.Abs(pointPos - from.x);
                float distTo = Mathf.Abs(pointPos - to.x);
                if (distFrom < minDistFrom)
                {
                    minDistFrom = distFrom;
                    fromPos = pointPos;
                }
                if (distTo < minDistTo)
                {
                    minDistTo = distTo;
                    toPos = pointPos;
                }
            }
            List<Signal> values = new List<Signal>();
            if (fromPos >= toPos) throw new System.Exception("Wrong points found");
            foreach (GraphPoint point in history.Keys)
            {
                float pointPos = point.transform.position.x;
                if (pointPos > fromPos && pointPos < toPos)
                {
                    values.Add(history[point]);
                }
            }
            return values.ToArray();
        }

        public void DisplayRange(System.Numerics.Complex[] complexValues)
        {
            float[] values = new float[complexValues.Length];
            for (int i = 0; i < complexValues.Length; i++) values[i] = (float)System.Numerics.Complex.Abs(complexValues[i]);
            int originalResolution = _resolution;
            _resolution *= _spectrumResolutionMultiplier;
            _showingRange = true;
            foreach (GraphPoint point in _points) point.gameObject.SetActive(false);
            foreach (GraphPoint point in _instantRange) Destroy(point.gameObject);
            _instantRange.Clear();

            float step = _graphLength / (float)_resolution;
            Vector3 position = Vector3.zero;
            Vector3 scale = Vector3.one * (_graphLength / (float)originalResolution);

            float maxValue = Mathf.Max(values);
            for (int i = 0; i < values.Length; i++) values[i] = values[i] / maxValue * _spectrumMaxValue;
            //for (int i = 0; i < values.Length; i++) values[i] = values[i] * _spectrumMaxValue;

            //float[] correctedRange = Interpolate(values, _resolution);
            float[] correctedRange;
            if (_interpolateByFactor) correctedRange = Interpolate(values, (int)(values.Length * _interpolateFactor));
            else correctedRange = Interpolate(values, _resolution);

            //for (int i = 0; i < _resolution; i++)
            for (int i = 0; i < correctedRange.Length; i++)
            {
                GraphPoint point = Instantiate(_pointPrefab);
                point.Initialize(_borders, this);
                _instantRange.Add(point);
                position.x = (i + 0.5f) * step - _graphLength / 2;
                position.y = correctedRange[i];
                point.gameObject.name = "Instant range point";
                point.transform.position = position;
                point.transform.localScale = scale;
                point.transform.SetParent(transform, false);
            }
            _resolution = originalResolution;
        }

        private float[] Interpolate(float[] values, int targetLength)
        {
            int m = values.Length;
            float[] destination = new float[targetLength];
            destination[0] = values[0];
            destination[targetLength - 1] = values[m - 1];

            for (int i = 1; i < targetLength - 1; i++)
            {
                float jd = (i * (float)(m - 1) / targetLength - 1);
                int j = (int)jd;
                destination[i] = values[j] + (values[j + 1] - values[j]) * (jd - j);
            }
            return destination;
        }

        public void HideRange()
        {
            _showingRange = false;
            foreach (GraphPoint point in _points) point.gameObject.SetActive(true);
            foreach (GraphPoint point in _instantRange) Destroy(point.gameObject);
            _instantRange.Clear();
        }
    }
}
