using DarkHomebrewRadio.Audio;
using DarkHomebrewRadio.Phase;
using DarkHomebrewRadio.Serial;
using DarkHomebrewRadio.Vfo;
using System;
using Gtk;

namespace DarkHomebrewRadio
{
    class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            Options options = new Options(args);
            PortAudioSharp.PortAudio.Initialize();
            AudioDriver defaultAudio = new AudioDriver("default", 1024);
            //AudioDriver radioAudio = new AudioDriver("Radio_virtual", 256);

            PhaseSplitter splitter = new PhaseSplitter(options);
            defaultAudio.sourceEvent = splitter.SourceEvent;
            defaultAudio.sinkEvent = splitter.SinkEvent;
            SerialDriver swr = null;
            PiAD9854 vfo = null;

            Application.Init();

            var app = new Application("org.DarkHomebrewRadio.DarkHomebrewRadio", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            win.pttEvent += splitter.UpdateTransmit;
            splitter.alcEvent = win.ALCEvent;
            if (options.swrEnabled)
            {
                swr = new SerialDriver(win.UpdateSWROK, win.SWREvent);
            }
            if (options.vfoEnabled)
            {
                vfo = new PiAD9854(win.UpdateVFOOK, options);
                win.vfoEvent += vfo.SetFrequency;
                win.vfoResetEvent += vfo.Reset;
                win.pttEvent += vfo.PTTEvent;
            }
            win.modeEvent += splitter.UpdateMode;
            win.micChangedEvent += splitter.MicEvent;
            win.DeleteEvent += (object sender, DeleteEventArgs e) =>
            {
                if (options.vfoEnabled)
                {
                    vfo.Stop();
                }
                if (options.swrEnabled)
                {
                    swr.Stop();
                }
                splitter.Stop();
                defaultAudio.Stop();
            };
            Application.Run();
        }
    }
}
