using System;
using UnityEngine;

namespace Research.Graph
{
    internal class GraphPoint : MonoBehaviour
    {
        private Graph _graph;
        private Vector2 _borders;
        private float _length;
        private bool _xAxis;
        public bool IsFlasePoint { get { return !_xAxis; } }
        public void Initialize(Vector2 borders, Graph graph, bool xAxis = true)
        {
            _borders = borders;
            _length = borders.y - _borders.x;
            _graph = graph;
            _xAxis = xAxis;
        }

        public void Move(Vector2 translation)
        {
            transform.Translate(translation);
            if (transform.position.x < (_borders.x + 0.01f))
            {
                if (!_xAxis)
                {
                    _graph.RemovePoint(this);
                    Destroy(gameObject);
                    return;
                }
                Vector2 position = transform.position;
                position.x += _length;
                position.y = _graph.CurrentValue + _graph.transform.position.y;
                transform.position = position;
            }
        }
    }
}
