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
        bool compression = false;
        bool isTransmitting = false;
        ConcurrentQueue<double[]> inputBuffers = new ConcurrentQueue<double[]>();
        ConcurrentQueue<Complex[]> fftBuffers = new ConcurrentQueue<Complex[]>();
        ConcurrentQueue<double[]> outputBuffers = new ConcurrentQueue<double[]>();
        //Mono
        double[] source = new double[SAMPLES_PER_FFT];
        int sourceWritePos = 0;
        bool sourceClearEvent = false;
        //Stereo
        double[] sink = new double[SAMPLES_PER_FFT * 2];
        int sinkReadPos = 0;
        Thread fftThread;
        AutoResetEvent fftARE = new AutoResetEvent(false);
        Thread ifftThread;
        AutoResetEvent ifftARE = new AutoResetEvent(false);
        Dictionary<int, AudioFilter> filters = new Dictionary<int, AudioFilter>();
        Action<Complex[]> audioFilter;
        public Action<double, double> alcEvent;
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
            if (sourceClearEvent)
            {
                sourceClearEvent = false;
                Array.Clear(source);
                inputBuffers.Clear();
                sourceWritePos = 0;
                return;
            }

            if (!transmit)
            {
                return;
            }

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
                inputBuffers.Enqueue(newSource);
                fftARE.Set();
            }
        }

        public void SinkEvent(double[] data)
        {
            //Start transmitting once we have a few audio chunks ready
            if (transmit && !isTransmitting)
            {
                if (outputBuffers.Count > 1)
                {
                    Console.WriteLine("TX ON");
                    isTransmitting = true;
                    //Mark the data as read so we instantly queue new data
                    sinkReadPos = sink.Length;
                }
            }

            if (isTransmitting)
            {
                //Read new data if needed
                if (sinkReadPos == sink.Length)
                {
                    if (outputBuffers.TryDequeue(out double[] copyData))
                    {
                        sink = copyData;
                        sinkReadPos = 0;
                    }
                    else
                    {
                        //We have run out of samples, either underflow or we stopped transmitting.
                        if (transmit)
                        {
                            Console.WriteLine("Audio Buffer Underflow");
                            Array.Clear(data);
                            Array.Clear(inDataComplex);
                            Array.Clear(lastIFFT);
                            return;
                        }
                        else
                        {
                            Console.WriteLine("TX OFF");
                            isTransmitting = false;
                            sourceClearEvent = true;
                            Array.Clear(data);
                            Array.Clear(inDataComplex);
                            Array.Clear(lastIFFT);
                            alcEvent(0.0, 0.0);
                            return;
                        }
                    }
                }

                //Queue new samples
                Array.Copy(sink, sinkReadPos, data, 0, data.Length);
                sinkReadPos += data.Length;
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

        public void UpdateCompress(bool compression)
        {
            this.compression = compression;
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

        Complex[] inDataComplex = new Complex[SAMPLES_PER_FFT * 2];
        private void FFTThread()
        {
            while (running)
            {
                fftARE.WaitOne(100);
                while (inputBuffers.TryDequeue(out double[] inputData))
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
                        //if (i == 0 || i == fftCalc.Length / 2)

                        //Nope, just DC. Nyquist has to go.
                        if (i == 0)
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

        Complex[] lastIFFT = new Complex[SAMPLES_PER_FFT * 2];
        double alcSlow = 1.0;
        double compSlow = 1.0;

        private void IFFTThread()
        {
            //Optional phase shift
            double phaseRadians = (options.phaseAdjust / 360.0) * Math.Tau;
            Complex phaseAdjust = new Complex(Math.Cos(phaseRadians), Math.Sin(phaseRadians));
            while (running)
            {
                ifftARE.WaitOne(100);
                while (fftBuffers.TryDequeue(out Complex[] fftData))
                {
                    Complex[] ifft = FFT.CalcIFFT(fftData);
                    //Stereo
                    double[] outputData = new double[SAMPLES_PER_FFT * 2];
                    double newAlc = 1.0;
                    double newMic = 0;
                    double alcGain = 1.0;
                    for (int i = 0; i < SAMPLES_PER_FFT; i++)
                    {
                        //Overlap and save
                        double scale = i / (double)SAMPLES_PER_FFT;
                        Complex fromPoint = lastIFFT[i + SAMPLES_PER_FFT] * (1.0 - scale);
                        Complex toPoint = ifft[i] * scale;
                        Complex combined = fromPoint + toPoint;
                        combined *= micgain;
                        Complex combinedShifted = combined * phaseAdjust;
                        double magnitude = combined.Magnitude;
                        double compressionGain = 1.0;

                        if (magnitude > newMic)
                        {
                            newMic = magnitude;
                        }
                        if (compression)
                        {
                            //Compression settings
                            const double compressionRatio = 3.0;
                            const double compressionThreshold = -12;
                            const double thresholdPoint = compressionThreshold / compressionRatio;

                            double currentDB = 20 * Math.Log10(magnitude);
                            double targetDB = 0;
                            //Trigger at -20db
                            if (currentDB > compressionThreshold)
                            {
                                targetDB = currentDB / compressionRatio;
                            }
                            else
                            {
                                targetDB = (currentDB - compressionThreshold) + thresholdPoint;
                            }
                            //Apply zero gain
                            targetDB += thresholdPoint;
                            double targetMagnitude = Math.Pow(10.0, (targetDB / 20.0));
                            compressionGain = targetMagnitude / magnitude;
                            if (compressionGain < compSlow)
                            {
                                compSlow = compressionGain;
                            }
                            else
                            {
                                compSlow = (0.999 * compSlow) + (0.001 * compressionGain);
                            }
                            magnitude *= compSlow;
                        }

                        if (magnitude > 0.90)
                        {
                            alcGain = 0.90 / magnitude;
                            if (alcSlow > alcGain)
                            {
                                alcSlow = alcGain;
                            }
                        }
                        else
                        {
                            alcSlow = (0.99999 * alcSlow) + 0.00001;
                        }
                        if (alcSlow < newAlc)
                        {
                            newAlc = alcSlow;
                        }


                        double finalGain = compSlow * alcSlow;

                        if (mode == TransmitMode.USB)
                        {
                            outputData[i * 2] = combined.Real * finalGain * options.leftAmplitude;
                            outputData[i * 2 + 1] = combinedShifted.Imaginary * finalGain;
                        }
                        if (mode == TransmitMode.LSB)
                        {
                            outputData[i * 2] = combinedShifted.Imaginary * finalGain * options.leftAmplitude;
                            outputData[i * 2 + 1] = combined.Real * finalGain;
                        }
                        if (mode == TransmitMode.DSB)
                        {
                            outputData[i * 2] = combined.Real * finalGain * 1.414;
                            outputData[i * 2 + 1] = 0.0;
                        }
                    }
                    if (alcEvent != null)
                    {
                        alcEvent(newMic, 1.0 - alcSlow);
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