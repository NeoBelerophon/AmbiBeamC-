using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Threading;

namespace AmbiBeam
{
    public class Communication
    {
        private readonly SerialPort _port;
        public Communication(string portname)
        {
            _port = new SerialPort(portname, 115200);
            _port.Open();
            // reset Arduino
            _port.DtrEnable = true;
            _port.DtrEnable = false;
            Thread.Sleep(100);
        }

        public void Write(List<Color> colors, byte brightness = 0x4F)
        {


            byte[] buffer = new byte[3* colors.Count];
            buffer[0] = 1; // SOH
            buffer[1] = (byte)(0xff & colors.Count);
            buffer[2] = (byte)((colors.Count >> 8) & 0xff);
            buffer[3] = brightness;
            buffer[4] = 2; // STX
            _port.Write(buffer, 0, 5);
            
            int i = 0;
            foreach (Color color in colors)
            {
                buffer[i++] = color.R;
                buffer[i++] = color.G;
                buffer[i++] = color.B;
            }
            _port.Write(buffer, 0, 3 * colors.Count);
            buffer[0] = 3; // ETX
            _port.Write(buffer, 0, 1);
        }

        ~Communication()
        {
            _port.Close();
        }
    }


}
