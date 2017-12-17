using System;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ComputerVision
{
    public partial class Form1 : Form
    {
        private Mat inputImage;
        private Mat grayscaleImage;
        private Mat gradientXImage;
        private Mat gradientYImage;
        private Mat gradientImage;
        private Mat cannyImage;
        private Mat distance;
        private Mat integrateImage;
        private Mat filterImage;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.PNG;*.JPG)|*.PNG;*.JPG|All Files(*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                inputImage = new Mat(openFileDialog.FileName);
                pictureBox1.Image = BitmapConverter.ToBitmap(inputImage);
            }
            
            Cv2.CvtColor(inputImage, grayscaleImage, ColorConversionCodes.BGRA2GRAY);
            pictureBox2.Image = BitmapConverter.ToBitmap(grayscaleImage);

            Cv2.Sobel(grayscaleImage, gradientXImage, MatType.CV_16S, 1, 0);
            Cv2.Sobel(grayscaleImage, gradientYImage, MatType.CV_16S, 0, 1);

            Cv2.ConvertScaleAbs(gradientXImage, gradientXImage);
            Cv2.ConvertScaleAbs(gradientYImage, gradientYImage);

            Cv2.AddWeighted(gradientXImage, 1.0, gradientYImage, 1.0, 0, gradientImage);

            Cv2.Canny(grayscaleImage, cannyImage, 50, 200);
            pictureBox3.Image = BitmapConverter.ToBitmap(cannyImage);

            Cv2.DistanceTransform(1 - cannyImage, distance, DistanceTypes.L2, DistanceMaskSize.Mask3);

            Cv2.Normalize(distance, distance, 0, 1.0, NormTypes.MinMax);
            pictureBox4.Image = BitmapConverter.ToBitmap(distance);

            Cv2.Integral(inputImage, integrateImage, MatType.CV_32F);

            for (int i = 0; i < filterImage.Width; i++)
            {
                for (int j = 0; j < filterImage.Height; j++)
                {
                    float tets = distance.Get<float>(i, j);
                    int size = (int)(10 * distance.Get<float>(i, j));
                    if (size >= 1)
                    {
                        int pixelsCount = ((Clamp(i + size, 0, integrateImage.Width - 1) - Clamp(i - size, 0, integrateImage.Width - 1)) *
                    (Clamp(j + size, 0, integrateImage.Height - 1) - Clamp(j - size, 0, integrateImage.Height - 1)));

                        var p0 = new OpenCvSharp.Point(Clamp(i - size, 0, integrateImage.Width - 1), Clamp(j - size, 0, integrateImage.Height - 1));
                        var p1 = new OpenCvSharp.Point(Clamp(i + size, 0, integrateImage.Width - 1), Clamp(j + size, 0, integrateImage.Height - 1));
                        var p2 = new OpenCvSharp.Point(Clamp(i - size, 0, integrateImage.Width - 1), Clamp(j + size, 0, integrateImage.Height - 1));
                        var p3 = new OpenCvSharp.Point(Clamp(i + size, 0, integrateImage.Width - 1), Clamp(j - size, 0, integrateImage.Height - 1));

                        filterImage.Set<Vec3b>(i, j, new Vec3b((byte)((integrateImage.Get<Vec3f>(p0.X, p0.Y).Item0
                        + integrateImage.Get<Vec3f>(p1.X, p1.Y).Item0 - integrateImage.Get<Vec3f>(p2.X, p2.Y).Item0
                        - integrateImage.Get<Vec3f>(p3.X, p3.Y).Item0) / pixelsCount),
                        (byte)((integrateImage.Get<Vec3f>(p0.X, p0.Y).Item1 + integrateImage.Get<Vec3f>(p1.X, p1.Y).Item1
                        - integrateImage.Get<Vec3f>(p2.X, p2.Y).Item1 - integrateImage.Get<Vec3f>(p3.X, p3.Y).Item1)
                        / pixelsCount),
                        (byte)((integrateImage.Get<Vec3f>(p0.X, p0.Y).Item2 + integrateImage.Get<Vec3f>(p1.X, p1.Y).Item2
                        - integrateImage.Get<Vec3f>(p2.X, p2.Y).Item2 - integrateImage.Get<Vec3f>(p3.X, p3.Y).Item2)
                        / pixelsCount)));
                    }
                    else
                        filterImage.Set<Vec3b>(i, j, inputImage.Get<Vec3b>(i, j));
                }
            }
            pictureBox5.Image = BitmapConverter.ToBitmap(filterImage);
        }

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
