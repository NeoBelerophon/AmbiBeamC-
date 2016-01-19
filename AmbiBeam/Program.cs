using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AmbiBeam
{


    public class AmbiBeam : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new AmbiBeam());
        }

        private readonly NotifyIcon _trayIcon;
        private Config _config;
        private Capture _capture;
        private Communication _comm;
        private System.Timers.Timer _timer;

        public AmbiBeam()
        {
            _config = Properties.Settings.Default["Setting"] as Config;
            if (_config == null)
                _config = new Config();
            
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Start/Stop", OnStartStop);
            trayMenu.MenuItems.Add("Configure", OnConfigure);
            trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon = new NotifyIcon
            {
                Text = "AmbiBeam",
                Icon = new Icon(SystemIcons.Application, 40, 40),
                ContextMenu = trayMenu,
                Visible = true
            };

            _timer = new System.Timers.Timer();

        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnStartStop(object sender, EventArgs e)
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                List<Color> black = new List<Color>(_config.LEDsHeight*2 + _config.LEDsWidth*2);
                black.ForEach(x => x = Color.Black);
                _comm.Write(black, 0);

            }
            else
            {

                _capture = new Capture(_config);
                _comm = new Communication(_config.Portname);

                _timer.AutoReset = true;
                _timer.Elapsed += timer_Tick;
                _timer.Interval = 300;
                _timer.Start();
            }

        }

        void timer_Tick(object sender, EventArgs e)
        {
            _capture.Update();
            _capture.TopColors.Reverse();
            _capture.LeftColors.Reverse();
            List<Color> data = _capture.BottomColors.Concat(_capture.LeftColors).Concat(_capture.TopColors).Concat(_capture.RightColors).ToList();
            _comm.Write(data, Convert.ToByte(_config.Brightness));
             
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            Configure configure = new Configure {Config = _config};
            var result = configure.ShowDialog();
            if (result == DialogResult.OK)
            {
                _config = configure.Config;
                Properties.Settings.Default["Setting"] = _config;
                Properties.Settings.Default.Save();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                _trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}