namespace DarkHomebrewRadio.Vfo
{
    //Specifically for the Banana Pi M5
    public enum AD9854Registers
    {
        PhaseAdjust1B1 = 0x0,
        PhaseAdjust1B0 = 0x1,
        PhaseAdjust2B1 = 0x2,
        PhaseAdjust2B0 = 0x3,
        Frequency1B5 = 0x4,
        Frequency1B4 = 0x5,
        Frequency1B3 = 0x6,
        Frequency1B2 = 0x7,
        Frequency1B1 = 0x8,
        Frequency1B0 = 0x9,
        Frequency2B5 = 0x0A,
        Frequency2B4 = 0x0B,
        Frequency2B3 = 0x0C,
        Frequency2B2 = 0x0D,
        Frequency2B1 = 0x0E,
        Frequency2B0 = 0x0F,
        DeltaFrequency5 = 0x10,
        DeltaFrequency4 = 0x11,
        DeltaFrequency3 = 0x12,
        DeltaFrequency2 = 0x13,
        DeltaFrequency1 = 0x14,
        DeltaFrequency0 = 0x15,
        UpdateClock3 = 0x16,
        UpdateClock2 = 0x17,
        UpdateClock1 = 0x18,
        UpdateClock0 = 0x19,
        RampRate2 = 0x1A,
        RampRate1 = 0x1B,
        RampRate0 = 0x1C,
        Control3 = 0x1D,
        Control2 = 0x1E,
        Control1 = 0x1F,
        Control0 = 0x20,
        IMultiply1 = 0x21,
        IMultiply0 = 0x22,
        QMultiply1 = 0x23,
        QMultiply0 = 0x24,
        OSKRate = 0x25,
        QDAC1 = 0x26,
        QDAC0 = 0x27,
    }
}


