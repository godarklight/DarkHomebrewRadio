using DarkHomebrewRadio.Audio;
using DarkHomebrewRadio.Phase;
using DarkHomebrewRadio.Serial;
using System;
using Gtk;

namespace DarkHomebrewRadio
{
    class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            PortAudioSharp.PortAudio.Initialize();
            AudioDriver defaultAudio = new AudioDriver("default", 256);
            AudioFilter audioFilter = new AudioFilter(48000, PhaseSplitter.SAMPLES_PER_FFT, 50, 250, 2800, 3000);
            //AudioDriver radioAudio = new AudioDriver("Radio_virtual", 256);
            PhaseSplitter splitter = new PhaseSplitter();
            splitter.audioFilter = audioFilter.Filter;
            defaultAudio.sourceEvent = splitter.SourceEvent;
            defaultAudio.sinkEvent = splitter.SinkEvent;
            SerialDriver swr = new SerialDriver();

            Application.Init();

            var app = new Application("org.DarkHomebrewRadio.DarkHomebrewRadio", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            win.pttEvent = splitter.UpdateTransmit;
            splitter.alcEvent = win.ALCEvent;
            swr.SwrEvent = win.SWREvent;
            Application.Run();
        }
    }
}
