using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Paint
{

    public partial class Form1 : Form
    {
        #region Çizim itemis için gerekli varsayılan veriler
        private bool Brush = true;                      //Uses either Brush or Eraser. Default is Brush
        private Shapes DrawingShapes = new Shapes();    //Stores all the drawing data
        private bool IsPainting = false;                //Is the mouse currently down (PAINTING)
        private bool IsEraseing = false;                 //Is the mouse currently down (ERASEING)
        private Point LastPos = new Point(0, 0);        //Last Position, used to cut down on repative data.
        private Color CurrentColour = Color.White;      //Deafult Colour
        private float CurrentWidth = 10;                //Deafult Pen width
        private int ShapeNum = 0;                       //record the shapes so they can be drawn sepratley.
        private Point MouseLoc = new Point(0, 0);       //Record the mouse position 
        #endregion

        //Genel olarak kullanabilmek için process global bir değer
        Process PythonProcess = new Process();

        public Form1()
        {
            InitializeComponent();
            //Set Double Buffering
            //çizim panelinin stabil olması için gerekli
            panel1.GetType().GetMethod("SetStyle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(panel1, new object[] { System.Windows.Forms.ControlStyles.UserPaint | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | System.Windows.Forms.ControlStyles.DoubleBuffer, true });
        }


        private void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //If we're painting...
            if (Brush)
            {
                //set it to mouse down, illatrate the shape being drawn and reset the last position
                IsPainting = true;
                ShapeNum++;
                LastPos = new Point(0, 0);
            }
            //but if we're eraseing...
            else
            {
                IsEraseing = true;
            }
        }

        protected void panel1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseLoc = e.Location;
            //PAINTING
            if (IsPainting)
            {
                //check its not at the same place it was last time, saves on recording more data.
                if (LastPos != e.Location)
                {
                    //set this position as the last positon
                    LastPos = e.Location;
                    //store the position, width, colour and shape relation data
                    DrawingShapes.NewShape(LastPos, CurrentWidth, CurrentColour, ShapeNum);
                }
            }
            if (IsEraseing)
            {
                //Remove any point within a certain distance of the mouse
                DrawingShapes.RemoveShape(e.Location, 10);
            }
            //refresh the panel so it will be forced to re-draw.
            panel1.Refresh();
        }

        private void panel1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (IsPainting)
            {
                //Finished Painting.
                IsPainting = false;
            }
            if (IsEraseing)
            {
                //Finished Earsing.
                IsEraseing = false;
            }
        }

        //Çizim Fonksiyonu
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //Apply a smoothing mode to smooth out the line.
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //DRAW THE LINES
            for (int i = 0; i < DrawingShapes.NumberOfShapes() - 1; i++)
            {
                Shape T = DrawingShapes.GetShape(i);
                Shape T1 = DrawingShapes.GetShape(i + 1);
                //make sure shape the two ajoining shape numbers are part of the same shape
                if (T.ShapeNumber == T1.ShapeNumber)
                {
                    //create a new pen with its width and colour
                    Pen p = new Pen(T.Colour, T.Width);
                    p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    //draw a line between the two ajoining points
                    e.Graphics.DrawLine(p, T.Location, T1.Location);
                    //get rid of the pen when finished
                    p.Dispose();
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //Fırça Silgi değişimi
            Brush = !Brush;
        }



        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //Fırça kalınlığını değiştir
            CurrentWidth = Convert.ToSingle(numericUpDown1.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Çizimi temizle ve paneli yenile
            DrawingShapes = new Shapes();
            panel1.Refresh();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Sürekli olarak çizimi tehmin.png içerisine kaydet
            while (true)
            {
                try
                {
                    int width = panel1.Size.Width;
                    int height = panel1.Size.Height;
                    Bitmap bm = new Bitmap(width, height);
                    panel1.DrawToBitmap(bm, new Rectangle(0, 0, panel1.Width, panel1.Height));
                    bm = ResizeBitmap(bm, int.Parse(textBox1.Text), int.Parse(textBox2.Text));
                    bm.Save(Application.StartupPath + "\\Model\\valid_images\\tahmin.png", ImageFormat.Bmp);
                    System.Threading.Thread.Sleep(500);

                }
                catch (Exception exc)
                {
                    //Bir hata olursa devam et
                }
            }
        }

        private static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            //Çizimi yapay zeka için yeniden boyutlandır (Varsayılan yapay zeka resim boyutu 28x28. eğer bu değerleri değiştirirseniz yapay zekayı da değiştirmeyi unutmayın)
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //Arkaplanda çizimi saniyede iki kere kaydet
            //ve sonucu saniyede 4 kere oku
            //thread'lerini çalıştır
            CheckForIllegalCrossThreadCalls = false;
            this.MaximizeBox = false;
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
            if (!backgroundWorker2.IsBusy)
                backgroundWorker2.RunWorkerAsync();
            //Yapay Zeka'nın bulunduğu python dosyasını çalıştır
            RunAI();

        }

        public void RunAI(bool visible=false)
        {
            try
            {
                //Çalışıyorsa yapay zekayı kapat
                PythonProcess.Kill();
            }
            catch (Exception)
            {}
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "python";
            info.Arguments = "\"" + Application.StartupPath + "\\Model\\model.py" + "\"";
            info.WorkingDirectory = Application.StartupPath + "\\Model";
            info.UseShellExecute = false;

            //Yapay zeka için açılan cmd penceresini gizlemek için true yap
            info.CreateNoWindow = visible;


            PythonProcess.StartInfo = info;
            PythonProcess.Start();


        }


        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    //text dosyasında tahmini oku ve labela bas
                    label3.Text = System.IO.File.ReadAllText(Application.StartupPath + "\\Model\\valid_images\\tahmin.txt");
                    System.Threading.Thread.Sleep(250);
                }
                catch (Exception)
                {}//bir hata durumunda devam et
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //Program kapanırken yapay zekayı da kapatalım
                PythonProcess.Kill();
            }
            catch (Exception)
            {}
            //Programın tamamen kapandığından emin olmak için
            Process p = Process.GetCurrentProcess();
            p.Kill();
        }

        #region Resim kaydederken oran seçeneği işretli ise onu ayarlamak için
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.Text = textBox1.Text;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.Text = textBox2.Text;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.Text = textBox1.Text;
            }
        } 
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            //checkbox2'nin durumuna göre yapay zekayı tekrar başlat
            RunAI(!checkBox2.Checked);
        }
    }

    #region Çizim için kullanılan class
    public class Shape
    {
        public Point Location;          //position of the point
        public float Width;             //width of the line
        public Color Colour;            //colour of the line
        public int ShapeNumber;         //part of which shape it belongs to

        //CONSTRUCTOR
        public Shape(Point L, float W, Color C, int S)
        {
            Location = L;               //Stores the Location
            Width = W;                  //Stores the width
            Colour = C;                 //Stores the colour
            ShapeNumber = S;            //Stores the shape number
        }
    }
    public class Shapes
    {
        private List<Shape> _Shapes;    //Stores all the shapes

        public Shapes()
        {
            _Shapes = new List<Shape>();
        }
        //Returns the number of shapes being stored.
        public int NumberOfShapes()
        {
            return _Shapes.Count;
        }
        //Add a shape to the database, recording its position, width, colour and shape relation information
        public void NewShape(Point L, float W, Color C, int S)
        {
            _Shapes.Add(new Shape(L, W, C, S));
        }
        //returns a shape of the requested data.
        public Shape GetShape(int Index)
        {
            return _Shapes[Index];
        }
        //Removes any point data within a certain threshold of a point.
        public void RemoveShape(Point L, float threshold)
        {
            for (int i = 0; i < _Shapes.Count; i++)
            {
                //Finds if a point is within a certain distance of the point to remove.
                if ((Math.Abs(L.X - _Shapes[i].Location.X) < threshold) && (Math.Abs(L.Y - _Shapes[i].Location.Y) < threshold))
                {
                    //removes all data for that number
                    _Shapes.RemoveAt(i);

                    //goes through the rest of the data and adds an extra 1 to defined them as a seprate shape and shuffles on the effect.
                    for (int n = i; n < _Shapes.Count; n++)
                    {
                        _Shapes[n].ShapeNumber += 1;
                    }
                    //Go back a step so we dont miss a point.
                    i -= 1;
                }
            }
        }
    }
    #endregion


}
