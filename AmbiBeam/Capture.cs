using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;


namespace AmbiBeam
{
    public class Capture
    {

        

        public List<Color> TopColors { get; private set; }
        
        public List<Color> BottomColors { get; private set; }
        
        public List<Color> LeftColors { get; private set; }

        public List<Color> RightColors { get; private set; }


        private readonly Device _d;
        private readonly Config _config;

        private List<List<long>> TopLongs { get; set; }

        private List<List<long>> BottomLongs { get; set; }

        private List<List<long>> RightLongs { get; set; }

        private List<List<long>> LeftLongs { get; set; }

        public Capture(Config config)
        {
            // init DX
            PresentParameters presentParameters = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard
            };
            _config = config;
            int idx = Array.IndexOf(Screen.AllScreens, config.GetScreen());

            _d = new Device(new Direct3D(), idx, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing,
                presentParameters);

            // init capture positions
            int hx = config.GetCaptureRectangle().Width / config.LEDsWidth;
            int hy = config.GetCaptureRectangle().Height / config.LEDsHeight;

            TopLongs = new List<List<long>>();
            BottomLongs = new List<List<long>>();
            LeftLongs = new List<List<long>> ();
            RightLongs = new List<List<long>> ();

            int part = 0;
            Rectangle captureSize = config.GetCaptureRectangle();
            int add = (captureSize.Width - (hx*config.LEDsWidth))/2;
            int nextBlock = hx + add ;
            TopLongs.Add(new List<long>());
            BottomLongs.Add(new List<long>());
            for (int i = 0; i < config.GetScreen().Bounds.Width; ++i)
            {
                if (i == nextBlock)
                {
                    part++;
                    
                    nextBlock += hx;
                    if (nextBlock == hx*config.LEDsWidth + add)
                        nextBlock += add;

                    TopLongs.Add(new List<long>());
                    BottomLongs.Add(new List<long>());
                }

                for (int j = 0; j < config.MarginTop; ++j)
                    TopLongs[part].Add(PointToLong(i, j + config.OffsetTop));
                
                for (int j = 0; j < config.MarginBottom; ++j)
                    BottomLongs[part].Add(PointToLong(i, config.GetScreen().Bounds.Height - 1 - j - config.OffsetBottom));
    
            }

            part = 0;
            add = (captureSize.Height - (hy * config.LEDsHeight)) / 2;
            nextBlock = hy + add;
            LeftLongs.Add(new List<long>());
            RightLongs.Add(new List<long>());
            for (int i = 0; i < config.GetScreen().Bounds.Height; ++i)
            {
                if (i == nextBlock)
                {
                    part++;

                    nextBlock += hy;
                    if (nextBlock == hy * config.LEDsHeight + add)
                        nextBlock += add;

                    LeftLongs.Add(new List<long>());
                    RightLongs.Add(new List<long>());
                }

                for (int j = 0; j < config.MarginLeft; ++j)
                    LeftLongs[part].Add(PointToLong(j + config.OffsetLeft, i));
                for (int j = 0; j < config.MarginRight; ++j)
                    RightLongs[part].Add(PointToLong(config.GetScreen().Bounds.Width - 1 - config.OffsetRight - j, i));
            }

            // init color positions
            TopColors = new List<Color>();
            BottomColors = new List<Color>();
            LeftColors = new List<Color>();
            RightColors = new List<Color>();

        }

        public void Update()
        {
            Surface s = CaptureScreen();
            DataRectangle dr = s.LockRectangle(LockFlags.None);
            DataStream gs = dr.Data;

            TopColors.Clear();
            foreach (var positions in TopLongs)
            {
                TopColors.Add(avcs(gs, positions));
            }

            BottomColors.Clear();
            foreach (var positions in BottomLongs)
            {
                BottomColors.Add(avcs(gs, positions));
            }

            LeftColors.Clear();
            foreach (var positions in LeftLongs)
            {
                LeftColors.Add(avcs(gs, positions));
            }

            RightColors.Clear();
            foreach (var positions in RightLongs)
            {
                RightColors.Add(avcs(gs, positions));
            }

            gs.Close();
            gs.Dispose();
            s.UnlockRectangle();
            s.Dispose();
        }

        private Color avcs(DataStream gs, List<long> positions)
        {
            byte[] bu = new byte[4];
            int r = 0;
            int g = 0;
            int b = 0;
            int i = 0;

            foreach (long pos in positions)
            {
                gs.Position = pos;
                gs.Read(bu, 0, 4);
                r += bu[2];
                g += bu[1];
                b += bu[0];
                i++;
            }

            return Color.FromArgb(r / i, g / i, b / i);
        }

        private long PointToLong(int x, int y)
        {
            return (y * _config.GetScreen().Bounds.Width + x) * (_config.GetScreen().BitsPerPixel / 8);
        }

        public Surface CaptureScreen()
        {
            var width = _config.GetScreen().Bounds.Width;
            var height = _config.GetScreen().Bounds.Height;

            Surface s = Surface.CreateOffscreenPlain(_d, width, height, Format.A8R8G8B8, Pool.Scratch);
            try
            {
                _d.GetFrontBufferData(0, s);
            }
            catch (Direct3D9Exception)
            {
                Debug.WriteLine("missed capture");
            }
            return s;
        }
    }

}