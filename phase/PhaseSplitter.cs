using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using Gtk;


namespace DarkHomebrewRadio.Phase
{
    class PhaseSplitter
    {
        private const int SAMPLES_PER_FFT = 8192;
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

        public PhaseSplitter()
        {
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
                inputBuffers.Enqueue(newSource);
                fftARE.Set();
            }
        }

        public void SinkEvent(double[] data)
        {
            if (!transmit && isTransmitting)
            {
                isTransmitting = transmit;
                Array.Clear(data);
            }
            if (transmit && !isTransmitting)
            {
                //Give us some headroom
                if (outputBuffers.Count > 3)
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

        public void UpdateTransmit(object sender, EventArgs args)
        {
            ToggleButton toggle = sender as ToggleButton;
            transmit = toggle.Active;
        }

        private void FFTThread()
        {
            while (running)
            {
                fftARE.WaitOne(100);
                if (inputBuffers.TryDequeue(out double[] inputData))
                {
                    Complex[] inDataComplex = new Complex[inputData.Length * 2];
                    for (int i = 0; i < inputData.Length; i++)
                    {
                        //Cast
                        inDataComplex[i] = inputData[i];
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

                    //Signal IFFT
                    fftBuffers.Enqueue(fftCalc);
                    ifftARE.Set();
                }
            }
        }

        private void IFFTThread()
        {
            Complex[] lastIFFT = null;
            while (running)
            {
                ifftARE.WaitOne(100);
                if (fftBuffers.TryDequeue(out Complex[] fftData))
                {
                    Complex[] ifft = FFT.CalcIFFT(fftData);
                    if (lastIFFT != null)
                    {
                        //Stereo
                        double[] outputData = new double[SAMPLES_PER_FFT * 2];
                        for (int i = 0; i < SAMPLES_PER_FFT; i++)
                        {
                            //Overlap and save
                            outputData[i * 2] = lastIFFT[i + (SAMPLES_PER_FFT)].Real + ifft[i].Real;
                            outputData[i * 2 + 1] = lastIFFT[i + (SAMPLES_PER_FFT)].Imaginary + ifft[i].Imaginary;
                        }
                        outputBuffers.Enqueue(outputData);
                    }
                    lastIFFT = ifft;
                }
            }
        }
    }
}