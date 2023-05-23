using System;
using System.IO;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
namespace DarkHomebrewRadio.Vfo
{
    //Specifically for the Banana Pi M5
    public class PiAD9854
    {
        GpioBackend backend;
        AD9854Interface inter;
        AD9854Control controller;
        Action<string> UpdateVFOOK;
        double setFreq = 7132000;
        bool transmit = false;
        Options options;
        public PiAD9854(Action<string> UpdateVFOOK, Options options)
        {            
            this.UpdateVFOOK = UpdateVFOOK;
            this.options = options;
            backend = new GpioBackend();
            if (!backend.init)
            {
                return;
            }
            inter = new AD9854Interface(backend);
            controller = new AD9854Control(inter);
            backend.Stop();
            controller.Powerdown();
        }

        public void SetFrequency(double freqHz)
        {
            Console.WriteLine($"Frequency set to: {freqHz}");
            if (!backend.init)
            {
                return;
            }
            setFreq = freqHz;
            if (transmit)
            {
                controller.SetFrequency1(freqHz);
            }
        }

        public void PTTEvent(bool transmit)
        {
            if (transmit)
            {
                controller.Powerup();
                Reset();
                controller.SetFrequency1(setFreq);
            }
            else
            {
                backend.Stop();
                controller.Powerdown();
                UpdateVFOOK("VFO OFF");
            }
            backend.SetTXPin(transmit);
        }

        public void Stop()
        {
            backend.Stop();
        }

        public void Reset()
        {
            UpdateVFOOK("VFO RESET");
            backend.Stop();
            controller.SetInternalUpdate(false);
            controller.SetPPM(options.ppmAdjust);
            controller.SetPLL(10);
            controller.SetBypassInvSinc(1);
            controller.SetOSK(0);
            UpdateVFOOK("VFO OK");
        }
    }
}
