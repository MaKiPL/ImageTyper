using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageTyper
{
    public partial class Form1 : Form
    {
        private static string _lastCommand;
        private static string _command;
        private static Bitmap _bmp;
        private static IntPtr _scan;
        private static BitmapData _bmpdata;

        public Form1()
        {
            InitializeComponent();
            _bmp = new Bitmap(512, 512);
            pictureBox1.Image = _bmp;
        }
        #region CommandWorker
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text.Length == 0)
                    return;
                TransferCommand(textBox1.Text);
                textBox1.Clear();
            }
            if(e.KeyCode == Keys.Up)
                if (!string.IsNullOrWhiteSpace(_lastCommand))
                    textBox1.Text = _lastCommand;
        }

        private void TransferCommand(string command)
        {
            richTextBox1.AppendText(">" + command + "\n");
            _command = command;
            _lastCommand = _command;
            PrepareCommands();
        }

        private void PrepareCommands()
        {
            byte[] b = MarshalImage();
            Process(b);
            UpdateImage(b);
        }
        #endregion

        internal void Process(byte[] b)
        {
            string[] com = _command.ToLower().Split('(');
            string[] parameters = null;
            if(com.Length != 1)
            {
                parameters = com[1].Split(',');
                if(parameters[parameters.Length-1].Trim()[parameters[parameters.Length-1].Length-1] == ')')
                    parameters[parameters.Length - 1] = parameters[parameters.Length - 1].Substring(0, parameters[parameters.Length - 1].Length - 1);
            }

            if(com[0] == "drawshape" || com[0] == "shape" && parameters.Length == 4)
            {
                DrawShape(b, parameters[0], parameters[1], parameters[2], parameters[3]);
                return;
            }

        }

        internal void DrawShape(byte[] b, string xx, string yy, string height, string width)
        {
            int x = int.Parse(xx) & 511;
            int y = int.Parse(yy) & 511;
            int h = Math.Abs(MathExtended.Clamp(int.Parse(height), 511-y));
            int w = Math.Abs(MathExtended.Clamp(int.Parse(width), 511 - x));
            int start = GetPixelLocation(x, y);
            int end = GetPixelLocation(x + w, y + h);
            int locY = 0, locX = 0;
            for(int i = start; i<end; i+= 3)
            {
                if(locX >= w)
                {
                    locY++;
                    i = GetPixelLocation(x, y + locY);
                    locX = 0;
                }
                b[i] = 0xFF;
                b[i + 1] = 0xFF;
                b[i + 2] = 0xFF;
                locX++;
            }
        }

        internal static class MathExtended
        {
            public static int Clamp(int value, int maxValue)
            {
                if (value > maxValue)
                    return maxValue;
                else return value;
            }
        }

        #region core
        internal byte[] MarshalImage()
        {
            _bmp = new Bitmap(pictureBox1.Image);
            _bmpdata = _bmp.LockBits(new Rectangle(0, 0, 512, 512), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            _scan = _bmpdata.Scan0;
            byte[] b = new byte[_bmpdata.Stride * _bmpdata.Height];
            Marshal.Copy(_scan, b, 0, b.Length);
            return b;
        }

        internal void UpdateImage(byte[] b)
        {
            Marshal.Copy(b, 0, _scan, b.Length);
            _bmp.UnlockBits(_bmpdata);
            pictureBox1.Image = _bmp;
        }

        //internal Func<int, int> GetPixelLocation = (int x, int y) => return (y * _bmpdata.Stride + x*3);

        internal int GetPixelLocation(int x, int y)
        {
            return y * _bmpdata.Stride + x * 3;
        }

        #endregion
    }
}
