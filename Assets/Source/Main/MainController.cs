using Gooyes.Tools;
using UnityEngine;

namespace Research
{
    public class MainController : Singleton<MainController>
    {
        [SerializeField] private float _clockFrequency = 4;
        [SerializeField] private float _signalFrequency = 1;

        public Clock clock;
        public DAC dac;
        public Generator generator;

        public Graph inputGraph;
        public Graph outputGraph;

        public bool IsPaused
        {
            get { return Time.timeScale == 0; }
            set { Time.timeScale = value ? 0 : 1; }
        }

        private void Start()
        {
            clock = new Clock(_clockFrequency);
            generator = new Generator(clock, inputGraph, _signalFrequency);
            dac = new DAC(clock, () => generator.Output, outputGraph);
        }

        private void FixedUpdate()
        {
            dac.FixedUpdate();
        }
    }
}
