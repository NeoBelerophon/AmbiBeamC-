using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using Tmds.MDns;

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
        private ICommunication _comm;
        private System.Timers.Timer _timer;
        private LEDTest _test;
        private ServiceBrowser _mDnsBrowser;

        public AmbiBeam()
        {
            _config = Properties.Settings.Default["Setting"] as Config;
            if (_config == null)
                _config = new Config();
            
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Start/Stop", OnStartStop);
            trayMenu.MenuItems.Add("Configure", OnConfigure);
            trayMenu.MenuItems.Add("Test", OnTest);
            trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon = new NotifyIcon
            {
                Text = "AmbiBeam",
                Icon = Properties.Resources.Color,
                ContextMenu = trayMenu,
                Visible = true
            };

            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = 50
            };

            _mDnsBrowser = new ServiceBrowser();
            _mDnsBrowser.StartBrowse("_ambibeam._udp");
            

        }
        
        private void OnTest(object sender, EventArgs e)
        {
            _comm = GetCommunication(_config.Node);

            _test = new LEDTest(2*_config.LEDsHeight + 2*_config.LEDsWidth);
            _timer.Elapsed += TimerOnElapsedTest;
            _timer.Start();


        }

        private void TimerOnElapsedTest(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _comm.Write(_test.GetColors());
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
                _comm = GetCommunication(_config.Node);

                if (_comm == null)
                {
                    MessageBox.Show("Can't connect to AmbiBeam: " + _config.Node, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _timer.Elapsed += timer_Tick;;
                _timer.Start();
            }

        }

        private ICommunication GetCommunication(string node)
        {
            ICommunication comm = null;
            if (_config.Node.ToLower().Contains("ambibeam"))
            {
                foreach (var service in _mDnsBrowser.Services)
                {
                    if (service.Hostname == node && service.Addresses.Count > 0)
                    {
                        comm = new UdpCommunication(service.Addresses[0].ToString());
                        break;
                    }
                }
            }
            else
            {
                comm = new SerialCommunication(node);
            }
            return comm;
        }


        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _capture.Update();
                _capture.TopColors.Reverse();
                _capture.RightColors.Reverse();
                List<Color> data = _capture.BottomColors.Concat(_capture.RightColors).Concat(_capture.TopColors).Concat(_capture.LeftColors).ToList();
                _comm.Write(data, Convert.ToByte(_config.Brightness));
            }
            catch (Exception)
            { }
                             
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            Configure configure = new Configure(_mDnsBrowser) {Config = _config};
            
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AmbiBeam));
            this.SuspendLayout();
            // 
            // AmbiBeam
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AmbiBeam";
            this.ResumeLayout(false);

        }
    }
}