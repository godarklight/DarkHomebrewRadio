namespace DarkHomebrewRadio.Vfo
{
    //Specifically for the Banana Pi M5
    public class AD9854ControlFlags
    {
        public int comparator_powerdown = 1;
        public int qdac_powerdown = 0;
        public int dac_powerdown = 0;
        public int digital_powerdown = 0;
        public int pll_range = 1;
        public int bypass_pll = 1;
        public int pll_multiplier = 4;
        public int clear_accumulator1 = 0;
        public int clear_accumulator = 0;
        public int triangle = 0;
        public int source_qdac = 0;
        public int mode = 0;
        public int internal_update = 1;
        public int bypass_inv_sinc = 0;
        public int osk_en = 0;
        public int osk_int = 0;

        public int GetRegisterInt()
        {
            int value = 0;
            value |= (comparator_powerdown << 28);
            value |= (qdac_powerdown << 26);
            value |= (dac_powerdown << 25);
            value |= (digital_powerdown << 24);
            value |= (pll_range << 22);
            value |= (bypass_pll << 21);
            value |= (pll_multiplier << 16);
            value |= (clear_accumulator1 << 15);
            value |= (clear_accumulator << 14);
            value |= (triangle << 13);
            value |= (source_qdac << 12);
            value |= (mode << 9);
            value |= (internal_update << 8);
            value |= (bypass_inv_sinc << 6);
            value |= (osk_en << 5);
            value |= (osk_int << 4);
            clear_accumulator1 = 0;
            clear_accumulator = 0;
            return value;
        }
    }
}