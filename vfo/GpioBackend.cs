using System;
using System.IO;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;

namespace DarkHomebrewRadio.Vfo
{
    //Specifically for the Banana Pi M5
    public class GpioBackend
    {
        public bool init
        {
            private set;
            get;
        }
        GpioDriver gpiod0;
        GpioDriver gpiod1;
        GpioController gpioc0;
        GpioController gpioc1;
        int[] addrMap = new int[6];
        int[] dataMap = new int[8];
        int resetPin;
        int wdPin;
        int rdPin;
        int uclkPin;


        public GpioBackend()
        {
            if (!File.Exists("/proc/device-tree/model"))
            {
                return;
            }
            string deviceModel = File.ReadAllText("/proc/device-tree/model");
            if (deviceModel.Contains("Raspberry"))
            {
                Console.WriteLine("Raspberry Pi GPIO found");
                SetupRaspberryPi();
            }
            if (deviceModel.Contains("BPI-M5"))
            {
                Console.WriteLine("Banana Pi M5 GPIO found");
                SetupBananaPiM5();
            }
            if (!init)
            {
                Console.WriteLine("GPIO not found");
            }
        }

        private void SetupRaspberryPi()
        {
            gpiod0 = new LibGpiodDriver(0);
            gpioc0 = new GpioController(PinNumberingScheme.Logical, gpiod0);
            ConfigurePins();
        }

        private void SetupBananaPiM5()
        {
            gpiod0 = new LibGpiodDriver(0);
            gpiod1 = new LibGpiodDriver(1);
            gpioc0 = new GpioController(PinNumberingScheme.Logical, gpiod0);
            gpioc1 = new GpioController(PinNumberingScheme.Logical, gpiod1);
            addrMap[0] = 82;
            addrMap[1] = 83;
            addrMap[2] = 70;
            addrMap[3] = 68;
            addrMap[4] = 69;
            addrMap[5] = 72;
            dataMap[0] = 73;
            dataMap[1] = 74;
            dataMap[2] = 76;
            dataMap[3] = 63;
            dataMap[4] = 79;
            dataMap[5] = 80;
            dataMap[6] = 71;
            dataMap[7] = 107;
            resetPin = 109;
            wdPin = 110;
            rdPin = 104;
            uclkPin = 21;
            ConfigurePins();
        }

        private void ConfigurePins()
        {
            init = true;
            //Address pins
            for (int i = 0; i < addrMap.Length; i++)
            {
                OpenPin(addrMap[i], PinMode.Output);
                SetPinValue(addrMap[i], PinValue.Low);
            }
            //Data pins
            for (int i = 0; i < dataMap.Length; i++)
            {
                OpenPin(dataMap[i], PinMode.Input);
            }
            //Control pins
            OpenPin(resetPin, PinMode.Output);
            OpenPin(wdPin, PinMode.Output);
            OpenPin(rdPin, PinMode.Output);
            OpenPin(uclkPin, PinMode.Input);
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.High);
            SetPinValue(rdPin, PinValue.High);
            Thread.Sleep(1);
            SetPinValue(resetPin, PinValue.High);
            Thread.Sleep(1);
            SetPinValue(resetPin, PinValue.Low);
        }

        private void OpenPin(int value, PinMode mode)
        {
            if (!init)
            {
                return;
            }
            if (value < 100)
            {
                gpioc0.OpenPin(value, mode);
            }
            else
            {
                gpioc1.OpenPin(value - 100, mode);
            }
        }

        public void PrintAllRegisters()
        {
            foreach (AD9854Registers value in Enum.GetValues<AD9854Registers>())
            {
                Console.WriteLine($"{value} = 0x{GetRegister(value).ToString("X2")}");
            }
        }

        public int GetRegister(AD9854Registers addr)
        {
            int addrInt = (int)addr;
            if (!init)
            {
                return 0;
            }

            //Set ADDR pins
            for (int i = 0; i < addrMap.Length; i++)
            {
                SetPinValue(addrMap[i], ((addrInt & 1) == 1) ? PinValue.High : PinValue.Low);
                addrInt = addrInt >> 1;
            }

            //WD+RD are disable flags
            SetPinValue(rdPin, PinValue.Low);
            Thread.Sleep(1);

            int value = 0;
            for (int i = 0; i < dataMap.Length; i++)
            {
                value = value << 1;
                if (GetPinValue(dataMap[i]) == PinValue.High)
                {
                    value |= 1;
                }
            }

            SetPinValue(rdPin, PinValue.High);
            Thread.Sleep(1);

            //Set ADDR pins to 0
            for (int i = 0; i < addrMap.Length; i++)
            {
                SetPinValue(addrMap[i], PinValue.Low);
            }

            return value;
        }

        public void SetRegister(AD9854Registers addr, int value)
        {
            //Console.WriteLine($"UPDATE {addr} = {value.ToString("X2")}");
            int addrInt = (int)addr;
            if (!init)
            {
                return;
            }
            for (int i = 0; i < addrMap.Length; i++)
            {
                SetPinValue(addrMap[i], ((addrInt & 1) == 1) ? PinValue.High : PinValue.Low);
                addrInt = addrInt >> 1;
            }
            for (int i = 0; i < dataMap.Length; i++)
            {
                SetPinMode(dataMap[i], PinMode.Output);
            }
            Thread.Sleep(1);
            for (int i = 0; i < dataMap.Length; i++)
            {
                PinValue pv = (value & 1) == 1;
                SetPinValue(dataMap[i], ((value & 1) == 1) ? PinValue.High : PinValue.Low);
                value = value >> 1;
            }
            //WD+RD are disable flags
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.Low);
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.High);
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.Low);
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.High);
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.Low);
            Thread.Sleep(1);
            SetPinValue(wdPin, PinValue.High);
            Thread.Sleep(1);
            //Set DATA pins pins to read
            for (int i = 0; i < dataMap.Length; i++)
            {
                SetPinValue(dataMap[i], PinValue.Low);
                SetPinMode(dataMap[i], PinMode.Input);
            }
            //Set ADDR pins to 0
            for (int i = 0; i < addrMap.Length; i++)
            {
                SetPinValue(addrMap[i], PinValue.Low);
            }
        }

        private void SetPinMode(int pin, PinMode mode)
        {
            if (!init)
            {
                return;
            }
            if (pin < 100)
            {
                gpioc0.SetPinMode(pin, mode);
            }
            else
            {
                gpioc1.SetPinMode(pin - 100, mode);
            }
        }

        private void SetPinValue(int pin, PinValue value)
        {
            if (!init)
            {
                return;
            }
            if (pin < 100)
            {
                gpioc0.Write(pin, value);
            }
            else
            {
                gpioc1.Write(pin - 100, value);
            }
        }

        private PinValue GetPinValue(int pin)
        {
            if (!init)
            {
                return PinValue.Low;
            }
            if (pin < 100)
            {
                return gpioc0.Read(pin);
            }
            return gpioc1.Read(pin - 100);
        }

        public void SetUdMode(bool internalUpdate)
        {
            if (!init)
            {
                return;
            }
            SetPinMode(uclkPin, internalUpdate ? PinMode.Input : PinMode.Output);
            if (!internalUpdate)
            {
                SetPinValue(uclkPin, PinValue.Low);
            }
        }

        public void ToggleUd()
        {
            if (!init)
            {
                return;
            }
            SetPinValue(uclkPin, PinValue.High);
            Thread.Sleep(1);
            SetPinValue(uclkPin, PinValue.Low);
        }

        public void Stop()
        {
            if (!init)
            {
                return;
            }
            SetPinValue(resetPin, PinValue.High);
            Thread.Sleep(1);
            SetPinValue(resetPin, PinValue.Low);
        }
    }
}

