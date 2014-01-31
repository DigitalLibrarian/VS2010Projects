using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Forever.Gaze;
using System.IO;

namespace GazeTrackingDemo
{
    public partial class Form1 : Form
    {

        Bitmap Subject { get; set; }

        Pen detectedPen;
        Pen nadaPen;

        private int SmallestPupilSize { get; set; }
        private float Threshold { get; set; }

        List<string> ImageFiles { get; set;}
        int CurrentIndex { get; set; }

        public Form1()
        {
            InitializeComponent();
            
            detectedPen = new Pen(Color.Red);
            nadaPen = new Pen(Color.Red);
            var files = from file in Directory.GetFiles(@"C:\Users\Valued Customer\Pictures\eye-pics\")
                        where file.EndsWith(".jpg")
                        select file;

            SmallestPupilSize = 10;
            Threshold = 10f;

            ImageFiles = files.ToList<string>();
            CurrentIndex = 0;
            LoadNextSubject();

            SyncFormFields();
        }

        private void LoadSubject(Bitmap image)
        {
            if (Subject != null) Subject.Dispose();
            Subject = image;
            pictureBox1.BackgroundImage = Subject;
            pictureBox1.Invalidate();
        }

        private void ReloadSubject()
        {
            LoadSubject((Bitmap)Image.FromFile(ImageFiles[CurrentIndex]));
        }

        private void LoadNextSubject()
        {
            CurrentIndex++;
            CurrentIndex %= ImageFiles.Count();
            ReloadSubject();
        }

        private void LoadPreviousSubject()
        {
            CurrentIndex--;
            if (CurrentIndex < 0) CurrentIndex = ImageFiles.Count() - 1;

            ReloadSubject();
        }


        private void RetinaScan(Bitmap image)
        {
            var smallestPupilSize = SmallestPupilSize;
            var threshold = Threshold;
            PupilFinder pupilFinder = new PupilFinder(smallestPupilSize, threshold);
            
            var rect = pupilFinder.FindPupil(image);

            using (Graphics g = Graphics.FromImage(image))
            {
                if (rect != null)
                {
                    g.DrawRectangle(detectedPen, (Rectangle)rect);
                }
                else
                {
                    g.DrawLine(nadaPen, new Point(0, 0), new Point(image.Width, image.Height));
                    g.DrawLine(nadaPen, new Point(image.Width, 0), new Point(0, image.Height));
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape || keyData == Keys.Q)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SyncFormFields()
        {
            richTextBox1.Text = SmallestPupilSize.ToString();
            richTextBox2.Text = Threshold.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            RetinaScan(Subject);
            //debugging info drawn on image
            pictureBox1.Invalidate();  
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (VisitNumberBox(richTextBox1))
            {
                SmallestPupilSize = int.Parse(richTextBox1.Text);
            }
            else
            {
                richTextBox1.Text = "" + SmallestPupilSize;
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

            if (VisitNumberBox(richTextBox2))
            {
                Threshold = float.Parse(richTextBox2.Text);
            }
            else
            {
                richTextBox2.Text = "" + Threshold;
            }
        }


        private bool VisitNumberBox(RichTextBox richTextBox1)
        {
            var text = richTextBox1.Text;
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, @"\D") && text != "")
            {
                richTextBox1.BackColor = Color.AntiqueWhite;
                return true;
            }
            else
            {
                richTextBox1.BackColor = Color.Red;
                richTextBox1.Text = "";
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadPreviousSubject();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadNextSubject();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ReloadSubject();
        }

       
    }
}
