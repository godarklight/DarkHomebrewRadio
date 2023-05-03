using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using Gtk;


namespace DarkHomebrewRadio.Phase
{
    class PhaseSplitter
    {
        public const int SAMPLES_PER_FFT = 8192;
        bool running = true;
        public bool transmit = false;
        bool isTransmitting = false;
        ConcurrentQueue<double[]> inputBuffers = new ConcurrentQueue<double[]>();
        ConcurrentQueue<Complex[]> fftBuffers = new ConcurrentQueue<Complex[]>();
        ConcurrentQueue<double[]> outputBuffers = new ConcurrentQueue<double[]>();
        //Mono
        double[] source = new double[SAMPLES_PER_FFT];
        int sourceWritePos = 0;
        //Stereo
        double[] sink = new double[SAMPLES_PER_FFT * 2];
        int sinkReadPos = 0;
        Thread fftThread;
        AutoResetEvent fftARE = new AutoResetEvent(false);
        Thread ifftThread;
        AutoResetEvent ifftARE = new AutoResetEvent(false);
        Dictionary<int, AudioFilter> filters = new Dictionary<int, AudioFilter>();
        Action<Complex[]> audioFilter;
        public Action<double> alcEvent;
        public TransmitMode mode = TransmitMode.LSB;
        double micgain = 1;
        Options options;


        public PhaseSplitter(Options options)
        {
            this.options = options;
            filters[2000] = new AudioFilter(48000, SAMPLES_PER_FFT, 500, 550, 2450, 2500);
            filters[2400] = new AudioFilter(48000, SAMPLES_PER_FFT, 300, 350, 2650, 2700);
            filters[3000] = new AudioFilter(48000, SAMPLES_PER_FFT, 50, 100, 2950, 3000);
            filters[3500] = new AudioFilter(48000, SAMPLES_PER_FFT, 50, 100, 3450, 3500);
            filters[8000] = new AudioFilter(48000, SAMPLES_PER_FFT, 50, 100, 7950, 8000);
            audioFilter = filters[3000].Filter;
            fftThread = new Thread(new ThreadStart(FFTThread));
            fftThread.Start();
            ifftThread = new Thread(new ThreadStart(IFFTThread));
            ifftThread.Start();
        }

        public void SourceEvent(double[] data)
        {
            //Stereo to mono
            for (int i = 0; i < data.Length / 2; i++)
            {
                source[sourceWritePos + i] = data[i * 2];
            }
            sourceWritePos += data.Length / 2;

            //Buffer is full, copy and queue
            if (sourceWritePos == SAMPLES_PER_FFT)
            {
                double[] newSource = new double[source.Length];
                Array.Copy(source, 0, newSource, 0, SAMPLES_PER_FFT);
                sourceWritePos = 0;
                if (transmit)
                {
                    inputBuffers.Enqueue(newSource);
                    fftARE.Set();
                }
            }
        }

        public void SinkEvent(double[] data)
        {
            if (!transmit && isTransmitting)
            {
                isTransmitting = transmit;
                Array.Clear(data);
                inputBuffers.Clear();
                outputBuffers.Clear();
                fftBuffers.Clear();
            }
            if (transmit && !isTransmitting)
            {
                //Give us some headroom
                if (outputBuffers.Count > 2)
                {
                    isTransmitting = true;
                }

            }

            if (isTransmitting)
            {
                Array.Copy(sink, sinkReadPos, data, 0, data.Length);
                sinkReadPos += data.Length;
                if (sinkReadPos == sink.Length)
                {
                    if (outputBuffers.TryDequeue(out double[] copyData))
                    {
                        sink = copyData;
                    }
                    else
                    {
                        Array.Clear(sink);
                    }
                    sinkReadPos = 0;
                }
            }
        }

        public void Stop()
        {
            running = false;
            ifftARE.Set();
            fftARE.Set();
            fftThread.Join();
            ifftThread.Join();
        }

        public void UpdateTransmit(bool transmit)
        {
            this.transmit = transmit;
        }

        public void UpdateMode(TransmitMode mode, int bandwidth)
        {
            this.mode = mode;
            audioFilter = filters[bandwidth].Filter;
        }

        public void MicEvent(double value)
        {
            micgain = value;
        }

        private void FFTThread()
        {
            Complex[] inDataComplex = new Complex[SAMPLES_PER_FFT * 2];
            while (running)
            {
                fftARE.WaitOne(100);
                if (inputBuffers.TryDequeue(out double[] inputData))
                {
                    for (int i = 0; i < inputData.Length; i++)
                    {
                        //Cast
                        inDataComplex[i] = inDataComplex[i + SAMPLES_PER_FFT];
                        inDataComplex[i + SAMPLES_PER_FFT] = inputData[i];
                    }

                    Complex[] fftCalc = FFT.CalcFFT(inDataComplex);

                    //Hilbert transform
                    for (int i = 0; i < fftCalc.Length; i++)
                    {
                        //Leave DC and nyquist alone
                        if (i == 0 || i == fftCalc.Length / 2)
                        {
                            continue;
                        }

                        //Double positive frequencies
                        if (i < fftCalc.Length / 2)
                        {
                            fftCalc[i] = fftCalc[i] * 2.0;
                        }

                        //Zero negative frequencies
                        if (i > fftCalc.Length / 2)
                        {
                            fftCalc[i] = 0;
                        }
                    }

                    //Filtering here
                    if (audioFilter != null)
                    {
                        audioFilter(fftCalc);
                    }

                    //Signal IFFT
                    fftBuffers.Enqueue(fftCalc);
                    ifftARE.Set();
                }
            }
        }

        private void IFFTThread()
        {
            Complex[] lastIFFT = new Complex[SAMPLES_PER_FFT * 2];
            //Optional phase shift
            double phaseRadians = (options.phaseAdjust / 360.0) * Math.Tau;
            Complex phaseAdjust = new Complex(Math.Cos(phaseRadians), Math.Sin(phaseRadians));
            while (running)
            {
                ifftARE.WaitOne(100);
                if (fftBuffers.TryDequeue(out Complex[] fftData))
                {
                    Complex[] ifft = FFT.CalcIFFT(fftData);
                    //Stereo
                    double[] outputData = new double[SAMPLES_PER_FFT * 2];
                    double newAlc = 0;
                    for (int i = 0; i < SAMPLES_PER_FFT; i++)
                    {
                        //Overlap and save
                        double scale = i / (double)SAMPLES_PER_FFT;
                        Complex fromPoint = lastIFFT[i + SAMPLES_PER_FFT] * (1.0 - scale);
                        Complex toPoint = ifft[i] * scale;
                        Complex combined = fromPoint + toPoint;
                        Complex combinedShifted = combined * phaseAdjust;
                        double magnitude = combined.Magnitude * micgain;
                        if (magnitude > newAlc)
                        {
                            newAlc = magnitude;
                        }
                        if (mode == TransmitMode.USB)
                        {
                            outputData[i * 2] = combined.Real * micgain * options.leftAmplitude;
                            outputData[i * 2 + 1] = combinedShifted.Imaginary * micgain;
                        }
                        if (mode == TransmitMode.LSB)
                        {
                            outputData[i * 2] = combinedShifted.Imaginary * micgain * options.leftAmplitude;
                            outputData[i * 2 + 1] = combined.Real * micgain;
                        }
                        if (mode == TransmitMode.DSB)
                        {
                            outputData[i * 2] = combined.Real * micgain * 1.5;
                            outputData[i * 2 + 1] = 0.0;
                        }
                    }
                    if (alcEvent != null)
                    {
                        alcEvent(newAlc);
                    }
                    outputBuffers.Enqueue(outputData);
                    lastIFFT = ifft;
                }
            }
        }
    }
    public enum TransmitMode
    {
        LSB,
        USB,
        DSB,
    }
}