using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Tmds.MDns;

namespace AmbiBeam
{

    public interface ICommunication
    {
        void Write(List<Color> colors, byte brightness = 0x4F);
    }

    public class SerialCommunication : ICommunication
    {
        private readonly SerialPort _port;
        public SerialCommunication(string portname)
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

            var retval = _port.ReadByte();

        }

        ~SerialCommunication()
        {
            _port.Close();
        }
    }

    public class UdpCommunication : ICommunication
    {
        private Socket _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPEndPoint _endPoint;

        public UdpCommunication(string connection)
        {
            string[] parts = connection.Split(':');

            var port = 2222;
            if (parts.Length > 1)
            {
                port = int.Parse(parts[1]);
            }
            if (parts.Length >= 1)
            {
                var serverAddr = IPAddress.Parse(parts[0]);
                _endPoint = new IPEndPoint(serverAddr, port);
            }
        }

        public void Write(List<Color> colors, byte brightness = 0x4F)
        {
            byte[] buffer = new byte[3*colors.Count + 6];
            buffer[0] = 0x01;
            buffer[2] = (byte) (0xff & colors.Count);
            buffer[1] = (byte) ((colors.Count >> 8) & 0xff);
            buffer[3] = brightness;
            buffer[4] = 0x02; // STX
            int i = 5;
            foreach (Color color in colors)
            {
                buffer[i++] = color.R;
                buffer[i++] = color.G;
                buffer[i++] = color.B;
            }

            buffer[i++] = 0x03; //ETX
            
       
            _sock.SendTo(buffer, i, SocketFlags.None, _endPoint);
       
            
        }

        ~UdpCommunication()
        {
            _sock.Close();
        }
    }

}
