using Research.Graph;
using Research.Main;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Research.UI {
    public class UIManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _stopButton;
        private TextMeshProUGUI _stopButtonText;

        [SerializeField] private Button _chooseAreaButton;
        private TextMeshProUGUI _chooseAreaButtonText;
        [SerializeField] private Button _confirmAreaButton;

        [Header("Graphs")]
        [SerializeField] private Graph.Graph _inputGraph;
        [SerializeField] private Graph.Graph _outGraph;

        [Header("Areas")]
        [SerializeField] private Image _inputAreaImage;
        [SerializeField] private Image _outputAreaImage;

        private Processor _processor;

        private bool _listeningToAreaClicks;
        private Area _inputArea;
        private Area _outputArea;

        private bool _isRunning
        {
            get => _processor.Active;

            set
            {
                _chooseAreaButton.gameObject.SetActive(!value);
                if (value)
                {
                    _processor.Continue();
                }
                else
                {
                    _processor.Stop();
                }
            }
        }

        private void Start()
        {
            _processor = FindObjectOfType<Processor>();

            _stopButtonText = _stopButton.GetComponentInChildren<TextMeshProUGUI>();
            _chooseAreaButtonText = _chooseAreaButton.GetComponentInChildren<TextMeshProUGUI>();

            _stopButton.onClick.AddListener(OnStopButtonClick);
            _chooseAreaButton.onClick.AddListener(OnChooseAreaButtonClick);
            _confirmAreaButton.onClick.AddListener(OnConfirmClick);

            _stopButton.gameObject.SetActive(true);
            _chooseAreaButton.gameObject.SetActive(false);
            _confirmAreaButton.gameObject.SetActive(false);

            _inputArea = new Area(_inputGraph);
            _outputArea = new Area(_outGraph);
            _listeningToAreaClicks = false;
            UpdateArea(false);
            UpdateArea(true);
        }

        private void OnStopButtonClick()
        {
            _isRunning = !_isRunning;
            _stopButtonText.text = _isRunning ? "Stop" : "Continue";
        }

        private void OnChooseAreaButtonClick()
        {
            _listeningToAreaClicks = !_listeningToAreaClicks;
            _stopButton.gameObject.SetActive(!_listeningToAreaClicks);
            _confirmAreaButton.gameObject.SetActive(_listeningToAreaClicks);
            _chooseAreaButtonText.text = _listeningToAreaClicks ? "Reset" : "Choose area";
            _inputArea = new Area(_inputGraph);
            _outputArea = new Area(_outGraph);
            UpdateArea(false);
            UpdateArea(true);
        }

        private void OnConfirmClick()
        {
            if (_inputArea.Validate()) CreateSpectrum(_inputArea);
            if (_outputArea.Validate()) CreateSpectrum(_outputArea);
        }

        private void Update()
        {
            if (_listeningToAreaClicks && Input.GetMouseButtonDown(0))
            {
                Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float distanceToInput = Vector3.Distance(position, _inputGraph.transform.position);
                float distanceToOutput = Vector3.Distance(position, _outGraph.transform.position);
                Area activeArea = distanceToInput < distanceToOutput ? _inputArea : _outputArea;
                if (activeArea.firstIsSet && !activeArea.secondIsSet)
                {
                    activeArea.second = position;
                    activeArea.secondPxPosition = Input.mousePosition;
                    activeArea.secondIsSet = true;
                }
                if (!activeArea.firstIsSet)
                {
                    activeArea.first = position;
                    activeArea.firstPxPosition = Input.mousePosition;
                    activeArea.firstIsSet = true;
                }
                UpdateArea(false);
                UpdateArea(true);
            }
        }

        public void UpdateArea(bool input)
        {
            Area area = input ? _inputArea : _outputArea;
            Image areaImage = input ? _inputAreaImage : _outputAreaImage;
            bool inputValidate = area.Validate();
            if (!inputValidate)
            {
                Vector2 size = areaImage.rectTransform.sizeDelta;
                size.x = 0;
                areaImage.rectTransform.sizeDelta = size;
            }
            else
            {
                float width = Mathf.Abs(area.firstPxPosition.x - area.secondPxPosition.x);
                float position = (area.firstPxPosition.x + area.secondPxPosition.x) / 2;
                Vector3 imagePos = areaImage.rectTransform.position;
                imagePos.x = position;
                Vector2 size = areaImage.rectTransform.sizeDelta;
                size.x = width;
                areaImage.rectTransform.position = imagePos;
                areaImage.rectTransform.sizeDelta = size;
            }
        }

        private void CreateSpectrum(Area area)
        {
            Signal[] signals = area.graph.GetSignalsInRange(area.first, area.second);
            float from = 100;
            float to = 0;
            for (int i = 0; i < signals.Length; i++)
            {
                float time = signals[i].registredTime;
                if (time < from) from = time;
                else if (time > to) to = time;
            }
            System.Numerics.Complex[] spectrum = _processor.CreateSpectrum(from, to, !area.graph._isInput);
            area.graph.DisplayRange(spectrum);
            area.firstIsSet = false;
            area.secondIsSet = false;
            UpdateArea(false);
            UpdateArea(true);
        }

        private class Area
        {
            public Vector3 first;
            public Vector3 firstPxPosition;
            public bool firstIsSet;
            public Vector3 second;
            public Vector3 secondPxPosition;
            public bool secondIsSet;

            public Graph.Graph graph;

            public Area(Graph.Graph graph)
            {
                this.graph = graph;
                first = Vector3.zero;
                firstPxPosition = Vector3.zero;
                firstIsSet = false;
                second = Vector3.zero;
                secondPxPosition = Vector3.zero;
                secondIsSet = false;
            }

            public bool Validate()
            {
                bool validation = firstIsSet && secondIsSet;
                return validation;
            }
        }
    }
}
