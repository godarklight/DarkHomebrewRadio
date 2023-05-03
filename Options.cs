using System;

namespace DarkHomebrewRadio
{
    public class Options
    {
        public bool vfoEnabled = true;
        public bool swrEnabled = true;
        public double phaseAdjust = 0.0;
        public double ppmAdjust = 0.0;
        public double leftAmplitude = 1.0;
        public Options(string[] args)
        {
            bool nextIsPPM = false;
            bool nextIsPhase = false;
            bool nextIsLeftAmplitude = false;
            foreach (string s in args)
            {
                if (nextIsPPM)
                {
                    nextIsPPM = false;
                    if (!double.TryParse(s, out ppmAdjust))
                    {
                        Console.WriteLine($"PPM parse error: {s} is not a number");
                    }
                    else
                    {
                        Console.WriteLine($"PPM adjust set to {ppmAdjust}");
                    }
                    continue;
                }
                if (nextIsPhase)
                {
                    nextIsPhase = false;
                    if (!double.TryParse(s, out phaseAdjust))
                    {
                        Console.WriteLine($"Phase parse error: {s} is not a number");
                    }
                    else
                    {
                        Console.WriteLine($"Phase adjust set to {phaseAdjust} degrees");
                    }
                    continue;
                }
                if (nextIsLeftAmplitude)
                {
                    nextIsLeftAmplitude = false;
                    if (!double.TryParse(s, out leftAmplitude))
                    {
                        Console.WriteLine($"Left amplitude Parse error: {s} is not a number");
                    }
                    else
                    {
                        Console.WriteLine($"Left amplitude set to {leftAmplitude}");
                    }
                    continue;
                }
                if (s == "--no-vfo")
                {
                    Console.WriteLine("VFO disabled");
                    vfoEnabled = false;
                }
                if (s == "--no-swr")
                {
                    Console.WriteLine("SWR disabled");
                    swrEnabled = false;
                }
                if (s == "--ppm")
                {
                    nextIsPPM = true;
                }
                if (s == "--phase")
                {
                    nextIsPhase = true;
                }
                if (s == "--leftAmplitude")
                {
                    nextIsLeftAmplitude = true;
                }
            }
        }
    }
}