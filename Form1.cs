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
using WebCamLib;

namespace EnarioImageProcessing
{
    public partial class Form1 : Form
    {
        /*
        The camera function does continuous subtraction of the background image to the live feed when the camera is on
.       and will continue until the camera is turned off

        hitting any other function for image processing will cause an error if the camera is on (specifically the filters)

        my apologies sir
        */

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
            ResizeImageCToMatchImageA();
           pictureBox3.Image = imageC;
        }

        private void buttonLoadImage_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private Device webcamDevice = null;
        private bool isCameraOn = false;
        private Timer cameraTimer;

        private void buttonCamera_Click(object sender, EventArgs e) {
            if (!isCameraOn) {
                Device[] devices = DeviceManager.GetAllDevices();
                if (devices.Length == 0) return;
                webcamDevice = devices[0];
                webcamDevice.ShowWindow(pictureBox1);
                isCameraOn = true;
                buttonCamera.Text = "Stop Camera";

                if (cameraTimer == null) {
                    cameraTimer = new Timer();
                    cameraTimer.Interval = 10; 
                    cameraTimer.Tick += CameraTimer_Tick;
                }
                cameraTimer.Start();
            }
            else {
                if (webcamDevice != null) {
                    webcamDevice.Stop();
                    webcamDevice = null;
                }
                if (cameraTimer != null)
                    cameraTimer.Stop();
                pictureBox1.Image = null;
                isCameraOn = false;
                buttonCamera.Text = "Start Camera";
            }
        }

        private void CameraTimer_Tick(object sender, EventArgs e)
        {
            if (webcamDevice != null && imageC != null)
            {
                webcamDevice.Sendmessage();
                IDataObject data = Clipboard.GetDataObject();
                if (data != null && data.GetDataPresent("System.Drawing.Bitmap"))
                {
                    Image capturedImage = (Image)data.GetData("System.Drawing.Bitmap", true);
                    Bitmap frame = new Bitmap(capturedImage);

                    // Ensure imageC matches the frame size
                    if (imageC.Width != frame.Width || imageC.Height != frame.Height)
                    {
                        ResizeImageCToMatchFrame(frame);
                    }

                    imageA = frame;
                    pictureBox1.Image = imageA;

                    int threshold = trackBar1.Value;
                    PerformSubtraction(imageA, imageC, threshold);
                }
            }
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

        private void ResizeImageCToMatchImageA() {
            if (imageC == null || imageA == null) return;

            if (imageC.Width != imageA.Width || imageC.Height != imageA.Height) {
                Bitmap resized = new Bitmap(imageA.Width, imageA.Height);
                using (Graphics g = Graphics.FromImage(resized)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imageC, 0, 0, imageA.Width, imageA.Height);
                }
                imageC.Dispose();
                imageC = resized;
                pictureBox3.Image = imageC;
            }
        }

        private void ResizeImageCToMatchFrame(Bitmap frame) {
            if (imageC == null || frame == null) return;

            if (imageC.Width != frame.Width || imageC.Height != frame.Height) {
                Bitmap resized = new Bitmap(frame.Width, frame.Height);
                using (Graphics g = Graphics.FromImage(resized)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imageC, 0, 0, frame.Width, frame.Height);
                }
                imageC.Dispose();
                imageC = resized;
                pictureBox3.Image = imageC;
            }
        }

        private void PerformSubtraction(Bitmap foreground, Bitmap background, int threshold)
        {
            if (foreground == null || background == null) return;
            if (foreground.Width != background.Width || foreground.Height != background.Height) return;

            Bitmap resultImage = new Bitmap(foreground.Width, foreground.Height);

            for (int x = 0; x < foreground.Width; x++)
            {
                for (int y = 0; y < foreground.Height; y++)
                {
                    Color pixel = foreground.GetPixel(x, y);
                    Color bgPixel = background.GetPixel(x, y);

                    if (isColorCloseToGreen(pixel, threshold))
                        resultImage.SetPixel(x, y, bgPixel);
                    else
                        resultImage.SetPixel(x, y, pixel);
                }
            }
            pictureBox2.Image = resultImage;
        }

        private void buttonSubtract_Click(object sender, EventArgs e)
        {
            int threshold = trackBar1.Value;
            PerformSubtraction(imageA, imageC, threshold);
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
