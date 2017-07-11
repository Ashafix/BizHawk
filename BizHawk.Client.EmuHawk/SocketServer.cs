using System.Net.Sockets;
using System.Net;
using BizHawk.Emulation.Common;
using BizHawk.Bizware.BizwareGL;
using System.Drawing;
using System.Windows.Forms;
using BizHawk.Emulation.Common.IEmulatorExtensions;
using BizHawk.Client.Common;

namespace BizHawk.Client.EmuHawk
{
    public class SocketServer
    {

        public string ip = "192.168.178.21";
        public int port = 9999;
        public System.Text.Decoder decoder = System.Text.Encoding.UTF8.GetDecoder();
        public Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public System.Net.IPAddress ipAdd;
        public System.Net.IPEndPoint remoteEP;
        public IVideoProvider currentVideoProvider = null;
        public bool connected = false;
        public bool initialized = false;

        public void initialize(IVideoProvider _currentVideoProvider)
        {
            currentVideoProvider = _currentVideoProvider;
            initialized = true;
        }
        public void connect()
        {
            if (ipAdd == null || remoteEP == null)
            {
                set_ip(ip, port);
            }
            soc.Connect(remoteEP);
            connected = true;
        }
        public void set_ip(string ip_, int port_)
        {
            ip = ip_;
            port = port_;
            ipAdd = System.Net.IPAddress.Parse(ip);
            remoteEP = new IPEndPoint(ipAdd, port);
        }
        public void SocketConnected()
        {
            bool part1 = soc.Poll(1000, SelectMode.SelectRead);
            bool part2 = (soc.Available == 0);
            if (part1 && part2)
                connected = false;
            else
                connected = true;
        }
        public void reconnect()
        {
            SocketConnected();
            if (!connected)
            {
                connect();
            }

            initialize(Global.Emulator.AsVideoProviderOrDefault());
            
        }
        public void send_screenshot()
        {
            reconnect();
            ScreenShot screenShot = new ScreenShot();
            using (var bb = screenShot.MakeScreenShotImage(currentVideoProvider))
            {
                using (var img = bb.ToSysdrawingBitmap())
                {
                    byte[] bmpBytes = screenShot.ImageToByte(img);
                    Clipboard.SetImage(img);
                    soc.Send(bmpBytes);                   
                }
            }
        }
        public string receive_message()
        {
            reconnect();
            byte[] receivedBytes = new byte[1024];
            int receivedLength = soc.Receive(receivedBytes);
            char[] chars = new char[receivedLength];
            int charLen = decoder.GetChars(receivedBytes, 0, receivedLength, chars, 0);
            return chars.ToString();
        }
    }
    class ScreenShot
    {
        public BitmapBuffer MakeScreenShotImage(IVideoProvider currentVideoProvider)
        {
            return GlobalWin.DisplayManager.RenderVideoProvider(currentVideoProvider);
        }
        public byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
