using DarkHomebrewRadio.Audio;
using DarkHomebrewRadio.Phase;
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
            //AudioDriver radioAudio = new AudioDriver("Radio_virtual", 256);
            PhaseSplitter splitter = new PhaseSplitter();
            defaultAudio.sourceEvent = splitter.SourceEvent;
            defaultAudio.sinkEvent = splitter.SinkEvent;

            Application.Init();

            var app = new Application("org.DarkHomebrewRadio.DarkHomebrewRadio", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            win.pttEvent = splitter.UpdateTransmit;
            Application.Run();
        }
    }
}
