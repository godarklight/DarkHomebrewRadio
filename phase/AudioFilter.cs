
using System;
using System.Numerics;

namespace DarkHomebrewRadio.Phase
{
    public class AudioFilter
    {
        double[] kernel;
        public AudioFilter(double sampleRate, int fftSize, double lowStart, double lowEnd, double highStart, double highEnd)
        {
            kernel = new double[fftSize * 2];
            double hzPerBin = sampleRate / (double)(fftSize * 2.0);
            int binStartLow = (int)(lowStart / hzPerBin);
            int binEndLow = (int)(lowEnd / hzPerBin);
            int binStartHigh = (int)(highStart / hzPerBin);
            int binEndHigh = (int)(highEnd / hzPerBin);
            int lowSpread = binEndLow - binStartLow;
            int highSpread = binEndHigh - binStartHigh;
            for (int i = 0; i < fftSize * 2; i++)
            {
                if (i < binStartLow)
                {
                    continue;
                }
                if (i > binEndHigh)
                {
                    continue;
                }
                if (i > binEndLow && i < binStartHigh)
                {
                    kernel[i] = 1.0;
                }
                if (i > binStartLow && i < binEndLow)
                {
                    int iShift = i - binStartLow;
                    double scale = iShift / (double)lowSpread;
                    kernel[i] = Math.Sin((Math.PI / 2.0) * scale);
                }
                if (i > binStartHigh && i < binEndHigh)
                {
                    int iShift = i - binStartHigh;
                    double scale = iShift / (double)highSpread;
                    kernel[i] = 1.0 - Math.Sin((Math.PI / 2.0) * scale);
                }
            }
            kernel[0] = 1.0;
            kernel[fftSize] = 1.0;
        }

        public void Filter(Complex[] inputData)
        {
            for (int i = 0; i < inputData.Length; i++)
            {
                inputData[i] = inputData[i] * kernel[i];
            }
        }
    }
}