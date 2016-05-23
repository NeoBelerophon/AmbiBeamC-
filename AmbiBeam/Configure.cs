using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SlimDX.X3DAudio;
using Tmds.MDns;

namespace AmbiBeam
{
    public partial class Configure : Form
    {
        private IntPtr hdc = IntPtr.Zero;
        private Config _config = new Config();

        public Config Config
        {
            get { return _config; }
            set { _config = value;
                UpdateFields();
            }
        }

        private void UpdateFields()
        {
            if(Config.Screen != null && cbScreen.Items.Contains(Config.Screen))
                cbScreen.SelectedItem = Config.Screen;
            
            cbNode.SelectedItem = Config.Node;

            numMarginBottom.Value = Config.MarginBottom;
            numMarginLeft.Value = Config.MarginLeft;
            numMarginRight.Value = Config.MarginRight;
            numMarginTop.Value = Config.MarginTop;
            numOffsetBottom.Value = Config.OffsetBottom;
            numOffsetLeft.Value = Config.OffsetLeft;
            numOffsetRight.Value = Config.OffsetRight;
            numOffsetTop.Value = Config.OffsetTop;
            numLEDHeight.Value = Config.LEDsHeight;
            numLEDWidth.Value = Config.LEDsWidth;
            tbBrightness.Value = Config.Brightness;
            UpdateBrightnessPrecent(tbBrightness.Value);
        }

        public ServiceBrowser ServiceBrowser { get; set; }

        public Configure(ServiceBrowser serviceBrowser)
        {
            InitializeComponent();
            Icon = Properties.Resources.Color;

            cbNode.Items.AddRange(SerialPort.GetPortNames());

            ServiceBrowser = serviceBrowser;
           
            if (ServiceBrowser != null)
            {
                foreach (var item in ServiceBrowser.Services)
                {
                    cbNode.Items.Add(item.Hostname);
                }
                // in case we get more
                ServiceBrowser.ServiceAdded += BrowserOnServiceAdded;
            }
            



            cbNode.TextChanged += cbSerialPort_SelectedIndexChanged;
            
            foreach (Screen screen in Screen.AllScreens)
            {
                cbScreen.Items.Add(screen.DeviceName);
            }
            cbScreen.SelectedIndexChanged += CbScreenOnSelectedIndexChanged;

            numOffsetTop.ValueChanged += NumOffsetTopOnValueChanged;
            numOffsetBottom.ValueChanged += NumOffsetBottomOnValueChanged;
            numOffsetLeft.ValueChanged += NumOffsetLeftOnValueChanged;
            numOffsetRight.ValueChanged += NumOffsetRightOnValueChanged;
            numMarginTop.ValueChanged += NumMarginTopOnValueChanged;
            numMarginBottom.ValueChanged += NumMarginBottomOnValueChanged;
            numMarginLeft.ValueChanged += NumMarginLeftOnValueChanged;
            numMarginRight.ValueChanged += NumMarginRightOnValueChanged;
            numLEDWidth.ValueChanged += NumLedWidthOnValueChanged;
            numLEDHeight.ValueChanged += NumLedHeightOnValueChanged;
            tbBrightness.ValueChanged += TbBrightnessOnValueChanged;

        }

        private void BrowserOnServiceAdded(object sender, ServiceAnnouncementEventArgs serviceAnnouncementEventArgs)
        {
            cbNode.Items.Add(serviceAnnouncementEventArgs.Announcement.Hostname);
        }

        private void UpdateBrightnessPrecent(int value)
        {
            lbBrightness.Text = ((int)((float)value / 255 * 100)).ToString() + "%";
        }

        private void TbBrightnessOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.Brightness = tbBrightness.Value;
            UpdateBrightnessPrecent(tbBrightness.Value);
        }

        private void NumLedHeightOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.LEDsHeight = Convert.ToInt32(numLEDHeight.Value);
        }

        private void NumLedWidthOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.LEDsWidth = Convert.ToInt32(numLEDWidth.Value);
        }

        private void NumMarginRightOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginRight = Convert.ToInt32(numMarginRight.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumMarginLeftOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginLeft = Convert.ToInt32(numMarginLeft.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumMarginBottomOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginBottom= Convert.ToInt32(numMarginBottom.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumMarginTopOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginTop = Convert.ToInt32(numMarginTop.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumOffsetRightOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetRight = Convert.ToInt32(numOffsetRight.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumOffsetLeftOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetLeft = Convert.ToInt32(numOffsetLeft.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumOffsetBottomOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetBottom = Convert.ToInt32(numOffsetBottom.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        private void NumOffsetTopOnValueChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetTop = Convert.ToInt32(numOffsetTop.Value);
            TbOnValueChanged(sender, eventArgs);
        }

        void cbSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.Node = cbNode.SelectedItem.ToString();
        }

        private void TbOnValueChanged(object sender, EventArgs eventArgs)
        {
            UpdateHelpers();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
          
        }

        private void CbScreenOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            Config.Screen = cbScreen.SelectedItem.ToString();
            /*
            if (hdc != IntPtr.Zero)
                DeleteDC(hdc);

            Config.Screen = Screen.AllScreens[cbScreen.SelectedIndex];
            hdc = CreateDC(Config.Screen.DeviceName, "", "", IntPtr.Zero);

            UpdateHelpers();
             */

        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        private void UpdateHelpers()
        {

            if (hdc != IntPtr.Zero)
            {
                Graphics graphics = Graphics.FromHdc(hdc);
                Pen pen = new Pen(Color.Red);
                graphics.DrawRectangle(pen, Config.GetTopRectangle());
                graphics.DrawRectangle(pen, Config.GetBottomRectangle());
                graphics.DrawRectangle(pen, Config.GetLeftRectangle());
                graphics.DrawRectangle(pen, Config.GetRightRectangle());
            }            
        }

        ~Configure()
        {
            DeleteDC(hdc);
            ServiceBrowser.ServiceAdded -= BrowserOnServiceAdded;
        }

        private void tbBrightness_ValueChanged(object sender, EventArgs e)
        {

        }



    }
}
