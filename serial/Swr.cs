using System;
using System.IO.Ports;
using System.Threading;


namespace DarkHomebrewRadio.Serial
{
    public class SerialDriver
    {
        Action<string> SwrText;
        Action<double, double> SwrEvent;
        SerialPort sp;
        bool setOk = false;
        public SerialDriver(Action<string> SwrText, Action<double, double> SwrEvent)
        {
            this.SwrText = SwrText;
            this.SwrEvent = SwrEvent;
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
                    int BytesToRead = sp.BytesToRead;
                    byte[] discardBuffer = new byte[BytesToRead];
                    sp.Read(discardBuffer, 0, BytesToRead);
                    sp.DataReceived += SerialData;
                    SwrText("SWR INIT");
                    Console.WriteLine($"Serial port INIT, discarded {BytesToRead} bytes.");
                }
                else
                {
                    SwrText("SWR IN USE");
                    Console.WriteLine("Serial port in use");
                    sp = null;
                }
            }
            else
            {
                SwrText("SWR NOT FOUND");
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
                        vForward += 0.4;
                    }
                    else
                    {
                        vForward = 0.0;
                        vReflected = 0.0;
                    }
                    //We have to compensate for the diode drops.
                    if (vReflected > 0.03)
                    {
                        vReflected += 0.4;
                    }
                    else
                    {
                        vReflected = 0.0;
                    }
                    //Multiply for N=10 transformers
                    vForward *= 10.0;
                    vReflected *= 10.0;
                    if (!setOk)
                    {
                        setOk = true;
                        SwrText("SWR OK");
                    }
                    SwrEvent(vForward, vReflected);
                }
                Thread.Sleep(1);
            }
        }


        public void Stop()
        {
            try
            {
                if (sp.IsOpen)
                {
                    sp.DataReceived -= SerialData;
                    sp.Close();
                    sp.Dispose();
                }
            }
            catch
            {
                //Don't care
            }
        }
    }
}