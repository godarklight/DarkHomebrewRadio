using System;
namespace DarkHomebrewRadio.Vfo
{
    class AD9854Control
    {
        AD9854Interface _interface;
        AD9854ControlFlags _controlFlags;
        private int pll = 1;
        private double ppm = 0;

        public AD9854Control(AD9854Interface newinterface)
        {
            _interface = newinterface;
            _controlFlags = new AD9854ControlFlags();
        }


        public void SetPPM(double value)
        {
            ppm = value;
        }

        public void SetPLL(int value)
        {
            pll = value;
            if (value == 1)
            {
                _controlFlags.bypass_pll = 1;
                _controlFlags.pll_multiplier = 4;
            }
            else
            {
                _controlFlags.bypass_pll = 0;
                _controlFlags.pll_multiplier = value;
            }
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.TriggerUpdate();
        }

        public void SetOSK(int value)
        {
            _controlFlags.osk_en = value;
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.TriggerUpdate();
        }

        public void SetBypassInvSinc(int value)
        {
            _controlFlags.bypass_inv_sinc = value;
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.TriggerUpdate();
        }

        public void SetInternalUpdate(bool value)
        {
            _controlFlags.internal_update = value ? 1 : 0;
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.SetUdMode(value);
            _interface.TriggerUpdate();
        }

        //Input 0-1 0-360 deg
        public void SetPhase1(double value)
        {
            _interface.SetPhaseAjust1((int)(value * 8192));
            _interface.TriggerUpdate();
        }

        public void SetPhase2(double value)
        {
            _interface.SetPhaseAdjust2((int)(value * 8192));
            _interface.TriggerUpdate();
        }

        public void SetFrequency1(double value)
        {
            double adjust_freq = value * (1.0 + ppm / 1000000.0);
            double ftw = (adjust_freq * (Math.Pow(2, 48))) / (20000000.0 * pll);
            Console.WriteLine("freq 1 is " + ftw);
            _interface.SetFrequency1((long)ftw);
            _interface.TriggerUpdate();
        }

        public void SetFrequency2(double value)
        {
            double adjust_freq = value * (1.0 + ppm / 1000000.0);
            double ftw = (adjust_freq * (Math.Pow(2, 48))) / (20000000.0 * pll);
            Console.WriteLine("freq 2 is " + ftw);
            _interface.SetFrequency2((long)ftw);
            _interface.TriggerUpdate();
        }

        public void SetMode(int value)
        {
            _controlFlags.mode = value;
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.TriggerUpdate();
        }

        public void SetAmplitudeI(double value)
        {
            _interface.SetIMultiplier((int)(value * Math.Pow(2, 11)));
            _interface.TriggerUpdate();
        }

        public void SetAmplitudeQ(double value)
        {
            _interface.SetQMultiplier((int)(value * Math.Pow(2, 11)));
            _interface.TriggerUpdate();

        }

        public void Powerup()
        {
            _controlFlags.dac_powerdown = 0;
            _controlFlags.qdac_powerdown = 0;
            _controlFlags.digital_powerdown = 0;
            _controlFlags.comparator_powerdown = 0;
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.TriggerUpdate();
        }

        public void Powerdown()
        {
            _controlFlags.dac_powerdown = 1;
            _controlFlags.qdac_powerdown = 1;
            _controlFlags.digital_powerdown = 1;
            _controlFlags.comparator_powerdown = 1;
            int controlValue = _controlFlags.GetRegisterInt();
            _interface.SetControl(controlValue);
            _interface.TriggerUpdate();
        }
    }
}