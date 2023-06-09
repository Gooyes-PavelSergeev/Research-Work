namespace Research.Main
{
    public class ClockTrigger
    {
        public ClockSignal _clock;
        public ClockTrigger(ClockSignal target)
        {
            _clock = target;
        }

        public void Trigger(int bit, Signal signal)
        {
            _clock.ManualUpdate(bit, signal);
        }
    }
}