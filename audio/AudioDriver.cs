using System;
using System.Collections.Concurrent;
using PortAudioSharp;
using System.Runtime.InteropServices;
using System.Threading;

namespace DarkHomebrewRadio.Audio
{
    class AudioDriver
    {
        Stream audioStream;
        int chunk_size;
        int sampleBalance = 0;
        /// <summary>
        /// Output to soundcard
        /// </summary>
        public Action<double[]> sinkEvent = null;
        /// <summary>
        /// Input from soundcard
        /// </summary>
        public Action<double[]> sourceEvent = null;
        double[] outputBuffer;
        int outputBufferReadPos = 0;
        double[] inputBuffer;
        int inputBufferWritePos = 0;

        public AudioDriver(string input, int chunk_size)
        {
            this.chunk_size = chunk_size;
            //Stereo
            this.inputBuffer = new double[2 * chunk_size];
            this.outputBuffer = new double[2 * chunk_size];
            int inputDevice = -1;
            int outputDevice = -1;
            for (int i = 0; i < PortAudio.DeviceCount; i++)
            {
                DeviceInfo di = PortAudio.GetDeviceInfo(i);
                if (di.name == input)
                {
                    Console.WriteLine($"Sinking to {di.name}");
                    outputDevice = i;
                }
                if (di.name == input)
                {
                    Console.WriteLine($"Sourcing from {di.name}");
                    inputDevice = i;
                }
            }
            if (inputDevice == -1)
            {
                Console.WriteLine($"Unable to find input source, using default");
            }
            if (outputDevice == -1)
            {
                Console.WriteLine($"Unable to find output sink, using default");
            }
            StreamParameters inParam = new StreamParameters();
            inParam.channelCount = 2;
            inParam.device = inputDevice;
            inParam.sampleFormat = SampleFormat.Float32;
            inParam.suggestedLatency = 0.01;
            StreamParameters outParam = new StreamParameters();
            outParam.channelCount = 2;
            outParam.device = outputDevice;
            outParam.sampleFormat = SampleFormat.Float32;
            outParam.suggestedLatency = 0.01;
            audioStream = new Stream(inParam, outParam, 48000, 0, StreamFlags.NoFlag, AudioCallback, null);
            audioStream.Start();
        }

        private StreamCallbackResult AudioCallback(IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userDataPtr)
        {
            unsafe
            {
                float* inputPtr = (float*)input.ToPointer();
                float* outputPtr = (float*)output.ToPointer();
                for (int i = 0; i < 2 * frameCount; i++)
                {
                    inputBuffer[inputBufferWritePos] = *inputPtr;
                    *outputPtr = (float)outputBuffer[outputBufferReadPos];
                    inputPtr++;
                    outputPtr++;
                    inputBufferWritePos++;
                    outputBufferReadPos++;
                    if (inputBufferWritePos == inputBuffer.Length)
                    {
                        if (sourceEvent != null)
                        {
                            sourceEvent(inputBuffer);
                        }
                        inputBufferWritePos = 0;
                    }
                    if (outputBufferReadPos == outputBuffer.Length)
                    {
                        if (sinkEvent != null)
                        {
                            sinkEvent(outputBuffer);
                        }
                        outputBufferReadPos = 0;
                    }
                }
            }
            return StreamCallbackResult.Continue;
        }

        public void Stop()
        {
            audioStream.Stop();
            PortAudio.Terminate();
        }
    }
}