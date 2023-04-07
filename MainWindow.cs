using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace DarkHomebrewRadio
{
    class MainWindow : Window
    {
        [UI] private SpinButton spinVfoA = null;
        [UI] private SpinButton spinVfoB = null;
        [UI] private RadioButton radioVfoA = null;
        [UI] private RadioButton radioVfoB = null;
        [UI] private Button btnAB = null;
        [UI] private ToggleButton toggleSplit = null;
        [UI] private Button btnBandwidth = null;
        [UI] private Button btnMode = null;
        [UI] private ProgressBar progressPower = null;
        [UI] private Label lblPower = null;
        [UI] private ProgressBar progressSWR = null;
        [UI] private Label lblSWR = null;
        [UI] private ProgressBar progressALC = null;
        [UI] private Label lblALC = null;
        [UI] private Label lblMIC = null;
        [UI] private Scrollbar scrollMic = null;
        [UI] private Label lblPBTI = null;
        [UI] private Scrollbar scrollPBTI = null;
        [UI] private Label lblPBTO = null;
        [UI] private Scrollbar scrollPBTO = null;
        [UI] private ToggleButton toggleNotch = null;
        [UI] private Scrollbar scrollNotch = null;
        [UI] private ToggleButton togglePTT = null;
        public EventHandler pttEvent = null;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            togglePTT.Toggled += togglePTT_Toggled;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void togglePTT_Toggled(object sender, EventArgs a)
        {
            if (pttEvent != null)
            {
                pttEvent(sender, a);
            }
        }

        public void ALCEvent(double newAlc)
        {
            Application.Invoke((object o, EventArgs e) =>
                {
                    lblALC.Text = (newAlc * 100).ToString("N0") + "%";
                    progressALC.Fraction = newAlc;
                }
            );
        }

        public void SWREvent(double vForward, double vReflected)
        {
            Application.Invoke((object o, EventArgs e) =>
                {
                    double power = VoltageToPower(vForward);
                    lblPower.Text = $"{power.ToString("N1")}W";
                    progressPower.Fraction = power / 5.0;
                    double refCoeff = vReflected / vForward;
                    double swr = 1.0 + (refCoeff) / (1.0 - refCoeff);
                    if (vForward == 0)
                    {
                        swr = 1.0;
                    }
                    if (double.IsNaN(swr))
                    {
                        swr = 10.0;
                    }
                    if (swr > 10)
                    {
                        swr = 10.0;
                    }
                    lblSWR.Text = $"{swr.ToString("N1")}:1";
                    progressSWR.Fraction = SwrToFraction(swr);


                    //lblALC.Text = (newAlc * 100).ToString("N0") + "%";
                    //progressALC.Fraction = newAlc;
                }
            );
        }

        public double SwrToFraction(double swr)
        {
            //0% SWR 1
            //50% SWR 2
            //75% SWR 3
            //100% SWR 10
            if (swr < 2.0)
            {
                return (swr - 1.0) / 2.0;
            }
            if (swr < 3.0)
            {
                return 0.5 + (swr - 2.0) / 4.0;
            }
            return 0.75 + (swr - 3) / 28.0;
        }

        public double VoltageToPower(double voltage)
        {
            //Convert vPeak to vRMS and return power
            return Math.Pow((voltage * 0.707), 2.0) / 50.0;
        }
    }
}
