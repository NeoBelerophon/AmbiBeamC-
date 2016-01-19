using System;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            
            cbSerialPort.SelectedText = Config.Portname;

            numMarginBottom.Value = Config.MarginBottom;
            numMarginLeft.Value = Config.MarginLeft;
            numMarginRight.Value = Config.MarginRight;
            numMarginTop.Value = Config.MarginTop;
            numOffsetBottom.Value = Config.OffsetBottom;
            numOffsetLeft.Value = Config.OffsetLeft;
            numMarginRight.Value = Config.OffsetRight;
            numMarginTop.Value = Config.OffsetTop;
        }

        public Configure()
        {
            InitializeComponent();

            cbSerialPort.DataSource = SerialPort.GetPortNames();
            cbSerialPort.SelectedIndexChanged += cbSerialPort_SelectedIndexChanged;
            
            foreach (Screen screen in Screen.AllScreens)
            {
                cbScreen.Items.Add(screen.DeviceName);
            }
            cbScreen.SelectedIndexChanged += CbScreenOnSelectedIndexChanged;

            numOffsetTop.TextChanged += NumOffsetTopOnTextChanged;
            numOffsetBottom.TextChanged += NumOffsetBottomOnTextChanged;
            numOffsetLeft.TextChanged += NumOffsetLeftOnTextChanged;
            numOffsetRight.TextChanged += NumOffsetRightOnTextChanged;
            numMarginTop.TextChanged += NumMarginTopOnTextChanged;
            numMarginBottom.TextChanged += NumMarginBottomOnTextChanged;
            numMarginLeft.TextChanged += NumMarginLeftOnTextChanged;
            numMarginRight.TextChanged += NumMarginRightOnTextChanged;

        }

        private void NumMarginRightOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginRight = Convert.ToInt32(numMarginRight.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumMarginLeftOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginLeft = Convert.ToInt32(numMarginLeft.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumMarginBottomOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginBottom= Convert.ToInt32(numMarginBottom.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumMarginTopOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.MarginTop = Convert.ToInt32(numMarginTop.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumOffsetRightOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetRight = Convert.ToInt32(numOffsetRight.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumOffsetLeftOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetLeft = Convert.ToInt32(numOffsetLeft.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumOffsetBottomOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetBottom = Convert.ToInt32(numOffsetBottom.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        private void NumOffsetTopOnTextChanged(object sender, EventArgs eventArgs)
        {
            Config.OffsetTop = Convert.ToInt32(numOffsetTop.Value);
            TbOnTextChanged(sender, eventArgs);
        }

        void cbSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.Portname = cbSerialPort.SelectedItem.ToString();
        }

        private void TbOnTextChanged(object sender, EventArgs eventArgs)
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
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

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
        }



    }
}
