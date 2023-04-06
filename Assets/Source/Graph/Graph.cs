using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Research.Graph
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private GraphPoint _pointPrefab;

        [SerializeField] private int _resolution = 10;

        private Vector2 _borders;

        private List<GraphPoint> _points;

        [SerializeField] private int _graphLength = 10;

        [SerializeField] private float _yAxisSize = 5;
        private float _maxInputValue;

        [SerializeField] private float _speed = 3;

        public float CurrentValue { get; private set; }

        private void Awake()
        {
            _borders = new Vector2(-_graphLength / 2, _graphLength / 2);
            _points = new List<GraphPoint>();
            float step = (float)_graphLength / (float)_resolution;
            Vector3 position = Vector3.zero;
            Vector3 scale = Vector3.one * step;
            for (int i = 0; i < _resolution; i++)
            {
                GraphPoint point = Instantiate(_pointPrefab);
                point.Initialize(_borders, this);
                _points.Add(point);
                position.x = (i + 0.5f) * step - _graphLength / 2;
                point.transform.position = position;
                point.transform.localScale = scale;
                point.transform.SetParent(transform, false);
                point.Move(Vector2.zero);
            }
        }

        private void Update()
        {
            for (int i = 0; i < _points.Count; i++)
            {
                GraphPoint point = _points[i];
                point.Move(_speed * Time.deltaTime * Vector2.left);
            }
        }

        public void PushValue(float value)
        {
            value /= _maxInputValue;
            value *= _yAxisSize;
            if (CurrentValue != value)
            {
                float step = (float)_graphLength / (float)_resolution * (value > CurrentValue ? -1 : 1);
                Vector3 position = Vector3.zero;
                Vector3 scale = Vector3.one * step;
                float factor = Mathf.Abs(value - CurrentValue) / _graphLength;
                int pointAmount = (int)(_resolution * factor) + 1;
                for (int i = 0; i < pointAmount; i++)
                {
                    GraphPoint point = Instantiate(_pointPrefab);
                    point.gameObject.name = "False point";
                    point.Initialize(_borders, this, false);
                    _points.Add(point);
                    position.y = (i + 0.5f) * step + value;
                    position.x = _borders.y;
                    point.transform.position = position;
                    point.transform.localScale = scale;
                    point.transform.SetParent(transform, false);
                }
            }
            CurrentValue = value;
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
    }
}
