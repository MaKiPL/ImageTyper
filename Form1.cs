using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageTyper
{
    public partial class Form1 : Form
    {
        private static List<String> _lastCommand;
        private byte _commandSelect = 0;
        private static string _command;
        private static Bitmap _bmp;
        private static IntPtr _scan;
        private static BitmapData _bmpdata;
        private bool bCustom = false;
        private Random _rand;
        
        internal struct Kolor
        {
            public static byte R = 0xFF;
            public static byte G = 0xFF;
            public static byte B = 0xFF;
        }

        public Form1()
        {
            InitializeComponent();
            _lastCommand = new List<string>();
            _bmp = new Bitmap(512, 512);
            pictureBox1.Image = _bmp;
            _rand = new Random();
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
            if (_lastCommand.Count != 0)
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (!string.IsNullOrWhiteSpace(_lastCommand[0]))
                    {
                        /*if(_commandSelect == _lastCommand.Count - 1)
                        {
                            textBox1.Text = _lastCommand[_commandSelect];
                            _commandSelect--;
                        }*/
                        textBox1.Text = _lastCommand[_commandSelect];
                        _commandSelect = (byte)MathExtended.ClampUnsigned(--_commandSelect, _lastCommand.Count - 1);
                        
                    }
                }
                if (e.KeyCode == Keys.Down)
                {
                    if (!string.IsNullOrWhiteSpace(_lastCommand[0]))
                    {
                        _commandSelect = (byte)MathExtended.ClampUnsigned(++_commandSelect, _lastCommand.Count - 1);
                        textBox1.Text = _lastCommand[_commandSelect];
                    }
                }
            }
        }

        private void TransferCommand(string command)
        {
            richTextBox1.AppendText(">" + command + "\n");
            _command = command;
            _lastCommand.Add(_command);
            _commandSelect = (byte)Math.Abs(_lastCommand.Count-1);
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
            bCustom = false;
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
            if (com[0] == "color" || com[0] == "colour" && parameters.Length == 3)
            {
                SetColor(parameters[0], parameters[1], parameters[2]);
                return;
            }
            if (com[0] == "load" || com[0] == "loadfile")
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    if(ofd.ShowDialog() == DialogResult.OK)
                    {
                        bCustom = true;
                        Image img = Image.FromFile(ofd.FileName);
                        pictureBox1.Image = img;
                    }
                }
                    return;
            }
            if (com[0] == "generatenoise" || com[0] == "generate_noise" && parameters.Length == 2)
            {
                GenerateNoise(b,parameters[0], parameters[1]);
                return;
            }
            if (com[0] == "e" || com[0] == "ex" && parameters.Length == 1)
            {
                TestEffect(b, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
                return;
            }
        }

        private void TestEffect(byte[] b, string v1, string v2, string v3, string v4, string v5, string v6)
        {
            int pass = int.Parse(v6);
            if (pass > 3)
                pass = 3;
            if (pass < 0)
                pass = 1;
            int x = int.Parse(v1) & 511;
            int y = int.Parse(v2) & 511;
            int h = Math.Abs(MathExtended.Clamp(int.Parse(v3), 511 - y));
            int w = Math.Abs(MathExtended.Clamp(int.Parse(v4), 511 - x));
            int start = GetPixelLocation(x, y);
            int end = GetPixelLocation(x + w, y + h);
            int locY = 0, locX = 0;
            int tolerance = int.Parse(v5);
            byte buffer = MathExtended.MostRepetativeByte(b, pass);
            for (int i = start; i < end; i += 3)
            {
                if (locX >= w)
                {
                    locY++;
                    i = GetPixelLocation(x, y + locY);
                    locX = 0;
                }
                if(pass == 3)
                    b[i] = (byte)MathExtended.ClampUnsigned(_rand.Next(buffer - tolerance, buffer + tolerance),255);
                if(pass == 2 || pass == 1)
                    b[i + 1] = (byte)MathExtended.ClampUnsigned(_rand.Next(buffer - tolerance, buffer + tolerance), 255);
                if(pass == 1)
                    b[i + 2] = (byte)MathExtended.ClampUnsigned(_rand.Next(buffer - tolerance, buffer + tolerance), 255);
                locX++;
            }
        }

        internal void GenerateNoise(byte[] b, string v1, string v2)
        {
            int a1 = int.Parse(v1); //size
            int a2 = int.Parse(v2); //color
            for(int i = 0; i<b.Length-4; i+=3 *a1)
            {
                byte red = (byte)_rand.Next(0, 255);
                byte green = (byte)_rand.Next(0, 255);
                byte blue = (byte)_rand.Next(0, 255);
                if(a2 > 0)
                {
                    for (int ii = i; ii < a1 * 3 + i -3; ii += 3)
                    {
                        try
                        {
                            b[ii + 2] = red;
                            b[ii + 1] = green;
                            b[ii] = blue;
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                else
                {
                    b[i] = red;
                    b[i + 1] = red;
                    b[i + 2] = red;
                }
            }
        }

        internal void SetColor(string v1, string v2, string v3)
        {
            Kolor.B = (byte)(MathExtended.Clamp(int.Parse(v1),255));
            Kolor.G = (byte)(MathExtended.Clamp(int.Parse(v2), 255));
            Kolor.R = (byte)(MathExtended.Clamp(int.Parse(v3), 255));
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
                b[i] = Kolor.R;
                b[i + 1] = Kolor.G;
                b[i + 2] = Kolor.B;
                locX++;
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
            if (!bCustom)
            {
                Marshal.Copy(b, 0, _scan, b.Length);
                _bmp.UnlockBits(_bmpdata);
                pictureBox1.Image = _bmp;
            }
            else return;
        }

        //internal Func<int, int> GetPixelLocation = (int x, int y) => return (y * _bmpdata.Stride + x*3);

        internal int GetPixelLocation(int x, int y)
        {
            return y * _bmpdata.Stride + x * 3;
        }

        #endregion
    }
}
