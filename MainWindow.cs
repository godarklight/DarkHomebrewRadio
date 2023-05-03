using System;
using Gtk;
using DarkHomebrewRadio.Phase;
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
        [UI] private Button btnResetVFO = null;
        [UI] private Label lblSWROK = null;
        [UI] private Label lblVFOOK = null;
        [UI] private ToggleButton togglePTT = null;
        public event Action<bool> pttEvent;
        public event Action<double> vfoEvent = null;
        public event System.Action vfoResetEvent = null;
        public event Action<TransmitMode, int> modeEvent = null;
        public event Action<double> micChangedEvent = null;
        double lastKhz = 7132.0;
        int modeInt = 0;
        int[] bandwidths = new int[] { 2000, 2400, 3000, 3500, 8000 };
        int bandwidthInt = 2;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            togglePTT.Toggled += togglePTT_Toggled;
            spinVfoA.Changed += vfoChanged;
            spinVfoB.Changed += vfoChanged;
            radioVfoA.Clicked += vfoChanged;
            radioVfoB.Clicked += vfoChanged;
            btnAB.Clicked += abClicked;
            btnMode.Clicked += modeClicked;
            btnBandwidth.Clicked += bandwidthClicked;
            btnResetVFO.Clicked += vfoResetClicked;
            scrollMic.ValueChanged += MicChanged;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void togglePTT_Toggled(object sender, EventArgs a)
        {
            vfoChanged(sender, a);
            pttEvent(togglePTT.Active);
        }

        private void modeClicked(object sender, EventArgs a)
        {
            modeInt++;
            if (modeInt == Enum.GetNames<TransmitMode>().Length)
            {
                modeInt = 0;
            }
            btnMode.Label = ((TransmitMode)modeInt).ToString();
            modeEvent((TransmitMode)modeInt, bandwidths[bandwidthInt]);
        }

        private void bandwidthClicked(object sender, EventArgs a)
        {
            bandwidthInt++;
            if (bandwidthInt == bandwidths.Length)
            {
                bandwidthInt = 0;
            }
            btnBandwidth.Label = bandwidths[bandwidthInt].ToString();
            if (modeEvent != null)
            {
                modeEvent((TransmitMode)modeInt, bandwidths[bandwidthInt]);
            }
        }

        private void abClicked(object sender, EventArgs a)
        {
            double valA = spinVfoA.Value;
            spinVfoA.Value = spinVfoB.Value;
            spinVfoB.Value = valA;
            vfoChanged(sender, a);
        }

        private void vfoChanged(object sender, EventArgs a)
        {
            double newKhz = 0;
            if (radioVfoA.Active)
            {
                newKhz = spinVfoA.Value;
                if (toggleSplit.Active && togglePTT.Active)
                {
                    newKhz = spinVfoB.Value;
                }
            }
            if (radioVfoB.Active)
            {
                newKhz = spinVfoB.Value;
                if (toggleSplit.Active && togglePTT.Active)
                {
                    newKhz = spinVfoA.Value;
                }
            }
            if (lastKhz != newKhz)
            {
                lastKhz = newKhz;

                vfoEvent(newKhz * 1000);
            }
        }

        private void MicChanged(object sender, EventArgs a)
        {
            double dbToGain = Math.Pow(10, scrollMic.Value / 10.0);
            micChangedEvent(dbToGain);
            lblMIC.Text = $"{scrollMic.Value.ToString("N2")}db";
        }


        private void vfoResetClicked(object sender, EventArgs a)
        {
            vfoResetEvent();
            lastKhz = 0;
            vfoChanged(sender, a);
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
                }
            );
        }

        public void UpdateSWROK(string text)
        {
            Application.Invoke((object o, EventArgs e) =>
            {
                lblSWROK.Text = text;
            });
        }

        public void UpdateVFOOK(string text)
        {
            Application.Invoke((object o, EventArgs e) =>
            {
                lblVFOOK.Text = text;
            });
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
