using System;
using System.IO.Ports;


namespace DarkHomebrewRadio.Serial
{
    public class SerialDriver
    {
        public Action<double, double> SwrEvent;
        SerialPort sp;
        public SerialDriver()
        {
            string portName = null;
            foreach (string name in SerialPort.GetPortNames())
            {
                if (name.ToLower().Contains("usb"))
                {
                    portName = name;
                }
            }
            if (portName != null)
            {
                sp = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                if (!sp.IsOpen)
                {
                    sp.Open();
                    sp.DataReceived += SerialData;
                }
                else
                {
                    Console.WriteLine("Serial port in use");
                    sp = null;
                }
            }
            else
            {
                Console.WriteLine("Serial port not found");
            }
        }

        private void SerialData(object sender, SerialDataReceivedEventArgs e)
        {
            while (sp.BytesToRead > 0)
            {
                string currentLine = sp.ReadLine();
                string[] split = currentLine.Split(':');
                if (split.Length == 2)
                {
                    double vForward = 0.0;
                    double vReflected = 0.0;
                    if (!double.TryParse(split[0], out vForward))
                    {
                        continue;
                    }
                    if (!double.TryParse(split[1], out vReflected))
                    {
                        continue;
                    }
                    //We have to compensate for the diode drops.
                    if (vForward > 0.03)
                    {
                        vForward += 0.6;
                    }
                    else
                    {
                        vForward = 0.0;
                        vReflected = 0.0;
                    }
                    //We have to compensate for the diode drops.
                    if (vReflected > 0.03)
                    {
                        vReflected += 0.6;
                    }
                    else
                    {
                        vReflected = 0.0;
                    }
                    //Multiply for N=10 transformers
                    vForward *= 10.0;
                    vReflected *= 10.0;
                    if (SwrEvent != null)
                    {
                        SwrEvent(vForward, vReflected);
                    }
                }
            }
        }
    }
}