using System;
using System.Device.Gpio;

namespace DarkHomebrewRadio.Vfo
{

    class AD9854Interface
    {
        private GpioBackend _backend;
        bool ud_is_input = true;

        public AD9854Interface(GpioBackend newbackend)
        {
            _backend = newbackend;
        }


        public void SetPhaseAjust1(int value)
        {
            _backend.SetRegister(AD9854Registers.PhaseAdjust1B0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.PhaseAdjust1B1, value & 0xFF);
        }

        public void SetPhaseAdjust2(int value)
        {
            _backend.SetRegister(AD9854Registers.PhaseAdjust2B0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.PhaseAdjust2B1, value & 0xFF);
        }

        public void SetFrequency1(long value)
        {
            _backend.SetRegister(AD9854Registers.Frequency1B0, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency1B1, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency1B2, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency1B3, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency1B4, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency1B5, (int)(value & 0xFF));
        }


        public void SetFrequency2(long value)
        {
            _backend.SetRegister(AD9854Registers.Frequency2B0, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency2B1, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency2B2, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency2B3, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency2B4, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Frequency2B5, (int)(value & 0xFF));
        }


        public void SetDelta(long value)
        {
            _backend.SetRegister(AD9854Registers.DeltaFrequency0, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.DeltaFrequency1, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.DeltaFrequency2, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.DeltaFrequency3, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.DeltaFrequency4, (int)(value & 0xFF));
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.DeltaFrequency5, (int)(value & 0xFF));
        }

        public void SetUpateClock(int value)
        {
            _backend.SetRegister(AD9854Registers.UpdateClock0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.UpdateClock1, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.UpdateClock2, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.UpdateClock3, value & 0xFF);
        }


        public void SetRampRateClock(int value)
        {
            _backend.SetRegister(AD9854Registers.RampRate0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.RampRate1, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.RampRate2, value & 0xFF);
        }

        public void SetControl(int value)
        {
            _backend.SetRegister(AD9854Registers.Control0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Control1, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Control2, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.Control3, value & 0xFF);
        }


        public void SetIMultiplier(int value)
        {
            _backend.SetRegister(AD9854Registers.IMultiply0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.IMultiply1, value & 0xFF);
        }


        public void SetQMultiplier(int value)
        {
            _backend.SetRegister(AD9854Registers.QMultiply0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.QMultiply1, value & 0xFF);
        }


        public void SetRampRate(int value)
        {
            _backend.SetRegister(AD9854Registers.OSKRate, value & 0xFF);
        }



        public void SetQDac(int value)
        {
            _backend.SetRegister(AD9854Registers.QDAC0, value & 0xFF);
            value = value >> 8;
            _backend.SetRegister(AD9854Registers.QDAC1, value & 0xFF);
        }


        public int GetPhaseAdjust1()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.PhaseAdjust1B1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.PhaseAdjust1B0);
            return value;
        }

        public int GetPhaseAdjust2()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.PhaseAdjust2B1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.PhaseAdjust2B0);
            return value;
        }


        public int GetFrequency1()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.Frequency1B5);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency1B4);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency1B3);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency1B2);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency1B1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency1B0);
            return value;
        }


        public int GetFrequency2()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.Frequency2B5);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency2B4);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency2B3);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency2B2);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency2B1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Frequency2B0);
            return value;
        }


        public int GetDelta()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.DeltaFrequency5);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.DeltaFrequency4);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.DeltaFrequency3);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.DeltaFrequency2);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.DeltaFrequency1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.DeltaFrequency0);
            return value;
        }


        public int GetUpdateClock()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.UpdateClock3);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.UpdateClock2);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.UpdateClock1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.UpdateClock0);
            return value;
        }

        public int GetRampRateClock()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.RampRate2);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.RampRate1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.RampRate0);
            return value;
        }

        public int GetIMultiplier()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.IMultiply1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.IMultiply0);
            return value;
        }

        public int GetQMultiplier()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.QMultiply1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.QMultiply0);
            return value;
        }

        public int GetRampRate()
        {
            return _backend.GetRegister(AD9854Registers.OSKRate);
        }


        public int GetControl()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.Control3);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Control2);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Control1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.Control0);
            return value;
        }


        public int GetQDac()
        {
            int value = 0;
            value = value | _backend.GetRegister(AD9854Registers.QDAC1);
            value = value << 8;
            value = value | _backend.GetRegister(AD9854Registers.QDAC0);
            return value;
        }


        public void SetUdMode(bool internalUpdate)
        {
            ud_is_input = internalUpdate;
            _backend.SetUdMode(internalUpdate);
        }


        public void TriggerUpdate()
        {
            if (ud_is_input)
            {
                SetUpateClock(0);
            }
            else
            {
                _backend.ToggleUd();
            }
        }
    }
}