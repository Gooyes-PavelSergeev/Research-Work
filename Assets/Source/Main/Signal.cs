using System;
using UnityEngine;

namespace Research.Main
{
    public class Signal
    {
        public int intValue;
        public int[] binaryValueInt;
        public bool[] binaryValueBool;
        public int bitDepth;
        public int valuesAvailable;
        public float magnitude;
        public float registredTime;

        public Signal(int binaryValue, float magnitude, int bitDepth)
        {
            this.intValue = binaryValue;
            this.bitDepth = bitDepth;
            this.valuesAvailable = 1 << bitDepth;
            this.magnitude = magnitude;
            binaryValueInt = intValue.ToBinaryInt(bitDepth);
            binaryValueBool = intValue.ToBinaryBool(bitDepth);
            registredTime = Time.time;
        }

        public Signal(Signal signal, int newValue)
        {
            this.intValue = newValue;
            this.bitDepth = signal.bitDepth;
            this.valuesAvailable = 1 << bitDepth;
            this.magnitude = signal.magnitude;
            binaryValueInt = intValue.ToBinaryInt(bitDepth);
            binaryValueBool = intValue.ToBinaryBool(bitDepth);
            registredTime = Time.time;
        }
    }
}
