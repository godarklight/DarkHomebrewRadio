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
    }
}
