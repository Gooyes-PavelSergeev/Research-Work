using System;
using System.Numerics;
using UnityEditor;

namespace Research.Main
{
    public static class FourierTransform
    {

        public static void DFT(Complex[] data)
        {
            int n = data.Length;
            double arg, cos, sin;
            Complex[] dst = new Complex[n];

            for (int i = 0; i < n; i++)
            {
                dst[i] = Complex.Zero;

                arg = -2.0 * Math.PI * (double)i / (double)n;

                for (int j = 0; j < n; j++)
                {
                    cos = Math.Cos(j * arg);
                    sin = Math.Sin(j * arg);
                    
                    double real = data[j].Real * cos - data[j].Imaginary * sin;
                    double imaginary = data[j].Real * sin + data[j].Imaginary * cos;
                    dst[i] = new Complex(real, imaginary);
                }
            }

            for (int i = 0; i < n; i++)
            {
                double real = data[i].Real / n;
                double imaginary = data[i].Imaginary / n;
                data[i] = new Complex(real, imaginary);
            }
        }
    }
}
