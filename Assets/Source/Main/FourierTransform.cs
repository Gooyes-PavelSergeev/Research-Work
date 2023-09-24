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

        public static Complex[] FFT(Complex[] x)
        {
            Complex[] X;
            int N = x.Length;
            if (N == 2)
            {
                X = new Complex[2];
                X[0] = x[0] + x[1];
                X[1] = x[0] - x[1];
            }
            else
            {
                Complex[] x_even = new Complex[N / 2];
                Complex[] x_odd = new Complex[N / 2];
                for (int i = 0; i < N / 2; i++)
                {
                    x_even[i] = x[2 * i];
                    x_odd[i] = x[2 * i + 1];
                }
                Complex[] X_even = FFT(x_even);
                Complex[] X_odd = FFT(x_odd);
                X = new Complex[N];
                for (int i = 0; i < N / 2; i++)
                {
                    X[i] = X_even[i] + w(i, N) * X_odd[i];
                    X[i + N / 2] = X_even[i] - w(i, N) * X_odd[i];
                }
            }
            return X;
        }

        private static Complex w(int k, int N)
        {
            if (k % N == 0) return 1;
            double arg = -2 * Math.PI * k / N;
            return new Complex(Math.Cos(arg), Math.Sin(arg));
        }

        public static Complex[] NFFT(Complex[] X)
        {
            int N = X.Length;
            Complex[] X_n = new Complex[N];
            for (int i = 0; i < N / 2; i++)
            {
                X_n[i] = X[N / 2 + i];
                X_n[N / 2 + i] = X[i];
            }
            return X_n;
        }

        /*
        public static void FFT(Complex[] data)
        {
            int n = data.Length;
            int m = (int)Math.Log(n, 2);

            // reorder data first
            ReorderData(data);

            // compute FFT
            int tn = 1, tm;

            for (int k = 1; k <= m; k++)
            {
                Complex[] rotation = FourierTransform.GetComplexRotation(k);

                tm = tn;
                tn <<= 1;

                for (int i = 0; i < tm; i++)
                {
                    Complex t = rotation[i];

                    for (int even = i; even < n; even += tn)
                    {
                        int odd = even + tm;
                        Complex ce = data[even];
                        Complex co = data[odd];

                        double tr = co.Real * t.Real - co.Imaginary * t.Imaginary;
                        double ti = co.Real * t.Imaginary + co.Imaginary * t.Real;

                        data[even] = new Complex(data[even].Real + tr, data[even].Imaginary + ti);

                        data[odd] = new Complex(data[even].Real + ce.Real - tr, data[even].Imaginary + ce.Imaginary - ti);
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                data[i] = new Complex(data[i].Real / n, data[i].Imaginary / n);
            }
        }

        #region Private Region

        private const int minLength = 2;
        private const int maxLength = 16384;
        private const int minBits = 1;
        private const int maxBits = 14;
        private static int[][] reversedBits = new int[maxBits][];
        private static Complex[,][] complexRotation = new Complex[maxBits, 2][];

        // Get array, indicating which data members should be swapped before FFT
        private static int[] GetReversedBits(int numberOfBits)
        {
            UnityEngine.Debug.Log($"Here before, {numberOfBits} bits");
            if ((numberOfBits < minBits) || (numberOfBits > maxBits))
                throw new ArgumentOutOfRangeException();
            UnityEngine.Debug.Log("Here after");
            // check if the array is already calculated
            if (reversedBits[numberOfBits - 1] == null)
            {
                int n = (int)Math.Pow(2, numberOfBits);
                int[] rBits = new int[n];

                // calculate the array
                for (int i = 0; i < n; i++)
                {
                    int oldBits = i;
                    int newBits = 0;

                    for (int j = 0; j < numberOfBits; j++)
                    {
                        newBits = (newBits << 1) | (oldBits & 1);
                        oldBits = (oldBits >> 1);
                    }
                    rBits[i] = newBits;
                }
                reversedBits[numberOfBits - 1] = rBits;
            }
            return reversedBits[numberOfBits - 1];
        }

        // Get rotation of complex number
        private static Complex[] GetComplexRotation(int numberOfBits)
        {
            int directionIndex = 0;

            // check if the array is already calculated
            if (complexRotation[numberOfBits - 1, directionIndex] == null)
            {
                int n = 1 << (numberOfBits - 1);
                double uR = 1.0;
                double uI = 0.0;
                double angle = Math.PI / n;
                double wR = Math.Cos(angle);
                double wI = Math.Sin(angle);
                double t;
                Complex[] rotation = new Complex[n];

                for (int i = 0; i < n; i++)
                {
                    rotation[i] = new Complex(uR, uI);
                    t = uR * wI + uI * wR;
                    uR = uR * wR - uI * wI;
                    uI = t;
                }

                complexRotation[numberOfBits - 1, directionIndex] = rotation;
            }
            return complexRotation[numberOfBits - 1, directionIndex];
        }

        // Reorder data for FFT using
        private static void ReorderData(Complex[] data)
        {
            int len = data.Length;

            UnityEngine.Debug.Log("Here reorder");
            // check data length
            if ((len < minLength) || (len > maxLength) || (!IsPowerOfTwo(len)))
                throw new ArgumentException("Incorrect data length.");
            UnityEngine.Debug.Log("After reorder");
            int[] rBits = GetReversedBits((int)Math.Log(len, 2));

            for (int i = 0; i < len; i++)
            {
                int s = rBits[i];

                if (s > i)
                {
                    Complex t = data[i];
                    data[i] = data[s];
                    data[s] = t;
                }
            }
        }

        private static bool IsPowerOfTwo(int number)
        {
            double log = Math.Log(number, 2);
            double pow = Math.Pow(2, Math.Round(log));
            return pow == number;
        }

        #endregion
        */
    }
}
