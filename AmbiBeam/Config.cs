using System;
using System.Drawing;
using System.Windows.Forms;

namespace AmbiBeam
{
    [Serializable]
    public class Config
    {

        public Config() 
        { }

        /// <summary>
        /// The screen object used for calculations
        /// </summary>
        public String Screen { get; set; }
        
        /// <summary>
        /// Number of LEDs used in width
        /// </summary>
        public int LEDsWidth { get; set; }
        /// <summary>
        /// Number of LEDs used in height
        /// </summary>
        public int LEDsHeight { get; set; }


        public int MarginTop { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }

        public int OffsetTop { get; set; }
        public int OffsetBottom { get; set; }
        public int OffsetLeft { get; set; }
        public int OffsetRight { get; set; }

        public int Brightness { get; set; }

        /// <summary>
        /// Name of the serial port used for communication with arduino
        /// </summary>
        public string Portname { get; set; }

        public Rectangle GetTopRectangle()
        {
            return new Rectangle( OffsetLeft, OffsetTop, GetScreen().Bounds.Width - OffsetLeft - OffsetRight, MarginTop);
        }

        public Rectangle GetLeftRectangle()
        {
            return new Rectangle(OffsetLeft, OffsetTop, MarginLeft,  GetScreen().Bounds.Height - OffsetTop - OffsetBottom);
        }

        public Rectangle GetBottomRectangle()
        {
            return new Rectangle(OffsetLeft, GetScreen().Bounds.Height - OffsetBottom - MarginBottom, GetScreen().Bounds.Width - OffsetLeft - OffsetRight, MarginBottom);
        }

        public Rectangle GetRightRectangle()
        {
            return new Rectangle(GetScreen().Bounds.Width- OffsetRight - MarginRight, OffsetTop, MarginRight, GetScreen().Bounds.Height - OffsetTop - OffsetBottom);
        }

        public Rectangle GetCaptureRectangle()
        {
            return new Rectangle(OffsetLeft, OffsetTop, GetScreen().Bounds.Width - OffsetLeft - OffsetRight, GetScreen().Bounds.Height - OffsetTop - OffsetBottom);
        }

        public Screen GetScreen()
        {
            foreach (Screen  screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.DeviceName == Screen)
                    return screen;
            }
            return null;
        }
    }
}
