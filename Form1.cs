using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace EnarioImageProcessing
{
    public partial class Form1 : Form
    {

        Bitmap imageA, imageB, imageC;


        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void uploadNewImageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e){
           imageA = new Bitmap(openFileDialog1.FileName);
           imageB = new Bitmap(imageA.Width, imageA.Height);
           pictureBox1.Image = imageA;
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e) {
           imageC = new Bitmap(openFileDialog2.FileName);
           pictureBox3.Image = imageC;
        }

        private void buttonLoadImage_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private void buttonLoadBackground_Click(object sender, EventArgs e) {
            openFileDialog2.ShowDialog();
        }

        private void buttonSaveImage_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PNG Image|*.png|JPEG Image|*.jpg";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.FileName = "output";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                ImageFormat format = null;

                switch (saveFileDialog1.FilterIndex) {
                    case 1:
                        format = ImageFormat.Png;
                        break;
                    case 2: 
                        format = ImageFormat.Jpeg;
                        break;
                }

                pictureBox2.Image.Save(fs, format);
                fs.Close();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e){
            openFileDialog1.ShowDialog();
        }

        private void buttonCopy_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                for (int i = 0; i < imageA.Width; i++) {
                    for (int j = 0; j < imageA.Height; j++) {
                        Color p = imageA.GetPixel(i, j);
                        imageB.SetPixel(i, j, Color.FromArgb(p.R, p.G, p.B));
                    }
                }
                pictureBox2.Image = imageB;
                generateHistogram();
            }
        }

        private void buttonGrayScale_Click(object sender, EventArgs e) {
            Color p;
            int avg;
            
            if (pictureBox1.Image != null) {
                for (int i = 0; i < imageA.Width; i++) {
                    for (int j = 0; j < imageA.Height; j++) {
                        p = imageA.GetPixel(i, j);
                        avg = (int)(p.R + p.G + p.B)/3;
                        imageB.SetPixel(i, j, Color.FromArgb(avg, avg, avg));
                    }
                }
                pictureBox2.Image = imageB;
                generateHistogram();
            }
        }

        private void buttonInvert_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                for (int i = 0; i < imageA.Width; i++) {
                    for (int j = 0; j < imageA.Height; j++) {
                        Color p = imageA.GetPixel(i, j);
                        imageB.SetPixel(i, j, Color.FromArgb(255-p.R, 255-p.G, 255-p.B));
                    }
                }
                pictureBox2.Image = imageB;
                generateHistogram();
            }
        }

        private void buttonSepia_Click(object sender, EventArgs e) {
            Color p;
            int nR, nG, nB;

            if (pictureBox1.Image != null) {
                for (int i = 0; i < imageA.Width; i++) {
                    for (int j = 0; j < imageA.Height; j++) {
                        p = imageA.GetPixel(i, j);
                        nR = (int)Math.Min(255, (0.393 * p.R + 0.769 * p.G + 0.189 * p.B));
                        nG = (int)Math.Min(255, (0.349 * p.R + 0.686 * p.G + 0.168 * p.B));
                        nB = (int)Math.Min(255, (0.272 * p.R + 0.543 * p.G + 0.131 * p.B));
                        imageB.SetPixel(i, j, Color.FromArgb(p.A, nR, nG, nB));
                    }
                }
                pictureBox2.Image = imageB;
                generateHistogram();
            }
        }

        private bool isColorCloseToGreen(Color pixel, int threshold) {
            Color green = Color.FromArgb(0, 255, 0);
            int greygreen = (green.R + green.G + green.B) / 3;
            int grey = (pixel.R + pixel.G + pixel.B) / 3;
            int distance = Math.Abs(greygreen - grey);

            return distance < threshold;
        }

        private void buttonSubtract_Click(object sender, EventArgs e) {
            int threshold = trackBar1.Value;
            Bitmap resultImage = new Bitmap(imageA.Width, imageA.Height);

            for (int x = 0; x < imageA.Width; x++) {
                for (int y = 0; y < imageA.Height; y++) {
                    Color pixel = imageA.GetPixel(x, y);
                    Color newbg_pixel = imageC.GetPixel(x, y);

                    if (isColorCloseToGreen(pixel, threshold))
                        resultImage.SetPixel(x, y, newbg_pixel);
                    else 
                        resultImage.SetPixel(x, y, pixel);
                }
            }
            pictureBox2.Image = resultImage;
        }

        private void generateHistogram(){
            Color p;
            int avg;
            
            int[] histogram  = new int[256];
            for (int i = 0; i < imageB.Width; i++) {
                for (int j = 0; j < imageB.Height; j++) {
                    p = imageB.GetPixel(i, j);
                    avg = (int)(p.R + p.G + p.B) / 3;
                    
                    histogram[avg]++;
                }
            }

            chartBnC.Series.Clear();
            chartBnC.ChartAreas[0].AxisX.Minimum = 0;
            chartBnC.ChartAreas[0].AxisX.Maximum = 255;
            chartBnC.ChartAreas[0].AxisY.Title = "Pixels";

            Series series = new Series {
                Name = "Brightness",
                ChartType = SeriesChartType.Column,
                Color = Color.Black
            };

            chartBnC.Series.Add(series);

            for (int i = 0; i < 256; i++) {
                series.Points.AddXY(i, histogram[i]);
            }
        }
    }
}
