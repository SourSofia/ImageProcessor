using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using WebCamLib;

public enum FilterType
{
    None,
    Grayscale,
    Invert,
    Sepia,
    Histogram,
    Subtract,
    Smooth,
    GaussianBlur,
    Sharpen,
    MeanRemoval,
    HorzVertical,
    AllDirections,
    Emboss,
    Lossy,
    HorizontalOnly,
    VerticalOnly
}

namespace ImageProcessor
{
    public partial class Form1 : Form
    {
        // Fields
        private Bitmap originalImage;
        private Bitmap processedImage;
        private Bitmap imageB, imageA;
        private Bitmap currentBackground = null;
        private Device myDevice;
        private Bitmap liveFrame = null;
        // Webcam fields
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        private FilterType currentFilter = FilterType.None;

        // Constructor
        public Form1()
        {
            InitializeComponent();
        }

        // --- Menu Item Events ---
        private void fileToolStripMenuItem_Click(object sender, EventArgs e){}

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.png;*.jpeg"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(ofd.FileName);
                processedImage = (Bitmap)originalImage.Clone();
                pictureBox1.Image = processedImage;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.None;
            if (originalImage == null) return;


            Bitmap copyImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    copyImage.SetPixel(x, y, pixelColor);
                }
            }

            pictureBox2.Image = copyImage;
            MessageBox.Show("Image copied successfully!", "Copy Complete");
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Grayscale;
            if (originalImage == null) return;

            processedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color pixel = originalImage.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    processedImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            pictureBox2.Image = processedImage;

        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Invert;
            if (originalImage == null) return;

            processedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color pixel = originalImage.GetPixel(x, y);
                    Color inverted = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                    processedImage.SetPixel(x, y, inverted);
                }
            }

            pictureBox2.Image = processedImage;

        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Histogram;

            if (originalImage == null) return;

            int width = 256;
            int height = 100;
            int[] histogram = new int[256];

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color pixel = originalImage.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    histogram[gray]++;
                }
            }

            int max = histogram.Max();
            Bitmap histImage = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int barHeight = (int)((histogram[i] / (float)max) * height);
                    g.DrawLine(Pens.Black, i, height, i, height - barHeight);
                }
            }

            pictureBox2.Image = histImage;
            //pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage; 
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Sepia;
            if (originalImage == null) return;

            processedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color pixel = originalImage.GetPixel(x, y);

                    int tr = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                    int tg = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                    int tb = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);

                    tr = Math.Min(255, tr);
                    tg = Math.Min(255, tg);
                    tb = Math.Min(255, tb);

                    processedImage.SetPixel(x, y, Color.FromArgb(tr, tg, tb));
                }
            }

            pictureBox2.Image = processedImage;

        }

        // --- Buttons ---
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.png;*.jpeg"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(ofd.FileName);
                imageB = originalImage;
                processedImage = (Bitmap)originalImage.Clone();
                pictureBox1.Image = processedImage;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.bmp;*.jpg;*.png;*.jpeg" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                currentBackground = new Bitmap(ofd.FileName); // use this for live subtract
                pictureBox3.Image = (Bitmap)currentBackground.Clone();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Subtract;

            Bitmap sourceFrame = imageB != null ? (Bitmap)imageB.Clone() : liveFrame != null ? (Bitmap)liveFrame.Clone() : null;
            Bitmap background = imageA != null ? (Bitmap)imageA.Clone() : currentBackground != null ? (Bitmap)currentBackground.Clone() : null;

            if (sourceFrame == null || background == null)
            {
                MessageBox.Show("Please load background and source image or start webcam first!");
                return;
            }

            if (background.Width != sourceFrame.Width || background.Height != sourceFrame.Height)
            {
                Bitmap resizedBg = new Bitmap(background, sourceFrame.Size);
                background.Dispose();
                background = resizedBg;
            }


            pictureBox2.Image?.Dispose();
            pictureBox2.Image = ApplySubtractFast(sourceFrame, background);

            sourceFrame.Dispose();
            background.Dispose();
        }




        // --- Form & PictureBox Events ---
        private void Form1_Load(object sender, EventArgs e){ }


        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("No webcam detected!");
                    return;
                }

                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

                videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);

                videoSource.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting webcam: " + ex.Message);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (Bitmap frame = (Bitmap)eventArgs.Frame.Clone())
                {

                    liveFrame?.Dispose();
                    liveFrame = (Bitmap)frame.Clone();

                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = (Bitmap)frame.Clone();

                    pictureBox2.Image?.Dispose();

                    switch (currentFilter)
                    {
                        case FilterType.Histogram:
                            pictureBox2.Image = ApplyHistogram(frame);
                            break;
                        case FilterType.Subtract:
                            Bitmap bg = imageA ?? currentBackground;

                            if (bg != null)
                            {
                                if (bg.Width != frame.Width || bg.Height != frame.Height)
                                {
                                    Bitmap resizedBg = new Bitmap(bg, frame.Size);
                                    pictureBox2.Image = ApplySubtractFast(frame, resizedBg);
                                    resizedBg.Dispose();
                                }
                                else
                                {
                                    pictureBox2.Image = ApplySubtractFast(frame, bg);
                                }
                            }
                            else
                            {
                                pictureBox2.Image = (Bitmap)frame.Clone();
                            }
                            break;
                        default:
                            pictureBox2.Image = ApplyFilterFast(frame, currentFilter);
                            break;
                    }
                }
            }
            catch { }
        }






        private Bitmap ApplyHistogram(Bitmap input)
        {
            int width = 256;
            int height = 100;
            int[] histogram = new int[256];

            BitmapData data = input.LockBits(new Rectangle(0, 0, input.Width, input.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = data.Stride;
            int bytes = Math.Abs(stride) * input.Height;
            byte[] buffer = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, bytes);
            input.UnlockBits(data);

            for (int i = 0; i < buffer.Length; i += 3)
            {
                byte gray = (byte)((buffer[i] + buffer[i + 1] + buffer[i + 2]) / 3);
                histogram[gray]++;
            }

            int max = histogram.Max();

            Bitmap histImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int barHeight = (int)((histogram[i] / (float)max) * height);
                    g.DrawLine(Pens.Black, i, height, i, height - barHeight);
                }
            }

            return histImage;
        }




        private Bitmap ApplyFilterFast(Bitmap input, FilterType filter)
        {
            Bitmap output;

            switch (filter)
            {
                case FilterType.Smooth:
                    output = ApplyConvolution(input, new double[,] {
                {1, 1, 1},
                {1, 1, 1},
                {1, 1, 1}}, 1.0 / 9.0);
                    break;

                case FilterType.GaussianBlur:
                    output = ApplyConvolution(input, new double[,] {
                {1, 2, 1},
                {2, 4, 2},
                {1, 2, 1}}, 1.0 / 16.0);
                    break;

                case FilterType.Sharpen:
                    output = ApplyConvolution(input, new double[,] {
                { 0, -1,  0},
                {-1,  5, -1},
                { 0, -1,  0}
            });
                    break;

                case FilterType.MeanRemoval:
                    output = ApplyConvolution(input, new double[,] {
                {-1, -1, -1},
                {-1,  9, -1},
                {-1, -1, -1}
            });
                    break;

                case FilterType.HorzVertical:
                    output = ApplyConvolution(input, new double[,] {
                {-1,  0,  1},
                { 0,  0,  0},
                { 1,  0, -1}
            });
                    break;

                case FilterType.AllDirections:
                    output = ApplyConvolution(input, new double[,] {
                { 1,  1,  1},
                { 1, -8,  1},
                { 1,  1,  1}
            });
                    break;

                case FilterType.Emboss:
                    output = ApplyConvolution(input, new double[,] {
                {-2, -1,  0},
                {-1,  1,  1},
                { 0,  1,  2}
            });
                    break;

                case FilterType.Lossy:
                    output = ApplyConvolution(input, new double[,] {
                { 1,  1,  1},
                { 1, -7,  1},
                { 1,  1,  1}
            });
                    break;

                case FilterType.HorizontalOnly:
                    output = ApplyConvolution(input, new double[,] {
                {-1, -2, -1},
                { 0,  0,  0},
                { 1,  2,  1}
            });
                    break;

                case FilterType.VerticalOnly:
                    output = ApplyConvolution(input, new double[,] {
                {-1,  0,  1},
                {-2,  0,  2},
                {-1,  0,  1}
            });
                    break;

                default:
                    // Non-convolution filters handled here
                    output = new Bitmap(input.Width, input.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var rect = new Rectangle(0, 0, input.Width, input.Height);

                    var inputData = input.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var outputData = output.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, output.PixelFormat);

                    int bytes = Math.Abs(inputData.Stride) * input.Height;
                    byte[] pixelBuffer = new byte[bytes];
                    byte[] resultBuffer = new byte[bytes];

                    System.Runtime.InteropServices.Marshal.Copy(inputData.Scan0, pixelBuffer, 0, bytes);
                    input.UnlockBits(inputData);

                    for (int i = 0; i < pixelBuffer.Length; i += 3)
                    {
                        byte b = pixelBuffer[i];
                        byte g = pixelBuffer[i + 1];
                        byte r = pixelBuffer[i + 2];

                        byte nr = r, ng = g, nb = b;

                        switch (filter)
                        {
                            case FilterType.Grayscale:
                                byte gray = (byte)((r + g + b) / 3);
                                nr = ng = nb = gray;
                                break;

                            case FilterType.Invert:
                                nr = (byte)(255 - r);
                                ng = (byte)(255 - g);
                                nb = (byte)(255 - b);
                                break;

                            case FilterType.Sepia:
                                nr = (byte)Math.Min(255, (0.393 * r + 0.769 * g + 0.189 * b));
                                ng = (byte)Math.Min(255, (0.349 * r + 0.686 * g + 0.168 * b));
                                nb = (byte)Math.Min(255, (0.272 * r + 0.534 * g + 0.131 * b));
                                break;
                        }

                        resultBuffer[i] = nb;
                        resultBuffer[i + 1] = ng;
                        resultBuffer[i + 2] = nr;
                    }

                    System.Runtime.InteropServices.Marshal.Copy(resultBuffer, 0, outputData.Scan0, bytes);
                    output.UnlockBits(outputData);
                    break;
            }

            return output;
        }


        private Bitmap ApplyConvolution(Bitmap source, double[,] kernel, double factor = 1.0, int bias = 0)
        {
            Bitmap output = new Bitmap(source.Width, source.Height);

            int width = source.Width;
            int height = source.Height;
            int kernelSize = 3;

            //MessageBox.Show("Convolution called!");

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double r = 0.0, g = 0.0, b = 0.0;

                    for (int ky = 0; ky < kernelSize; ky++)
                    {
                        for (int kx = 0; kx < kernelSize; kx++)
                        {
                            int px = x + (kx - 1);
                            int py = y + (ky - 1);

                            Color pixel = source.GetPixel(px, py);

                            r += pixel.R * kernel[ky, kx];
                            g += pixel.G * kernel[ky, kx];
                            b += pixel.B * kernel[ky, kx];
                        }
                    }

                    int rr = Math.Min(Math.Max((int)(factor * r + bias), 0), 255);
                    int gg = Math.Min(Math.Max((int)(factor * g + bias), 0), 255);
                    int bb = Math.Min(Math.Max((int)(factor * b + bias), 0), 255);

                    output.SetPixel(x, y, Color.FromArgb(rr, gg, bb));
                }
            }

            //MessageBox.Show("Convolution results!");
            return output;
        }


        private void pictureBox2_Click(object sender, EventArgs e){}

        private Bitmap ApplySubtractFast(Bitmap foreground, Bitmap background)
        {
            Bitmap output = new Bitmap(foreground.Width, foreground.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, foreground.Width, foreground.Height);

            BitmapData fgData = foreground.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bgData = background.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData outData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(fgData.Stride) * foreground.Height;
            byte[] fgBuffer = new byte[bytes];
            byte[] bgBuffer = new byte[bytes];
            byte[] outBuffer = new byte[bytes];

            Marshal.Copy(fgData.Scan0, fgBuffer, 0, bytes);
            Marshal.Copy(bgData.Scan0, bgBuffer, 0, bytes);

            foreground.UnlockBits(fgData);
            background.UnlockBits(bgData);

            for (int i = 0; i < fgBuffer.Length; i += 3)
            {
                byte b = fgBuffer[i];
                byte g = fgBuffer[i + 1];
                byte r = fgBuffer[i + 2];


                if (g > 150 && g > r + 50 && g > b + 50) 
                {
                    outBuffer[i] = bgBuffer[i];
                    outBuffer[i + 1] = bgBuffer[i + 1];
                    outBuffer[i + 2] = bgBuffer[i + 2];
                }
                else
                {
                    outBuffer[i] = b;
                    outBuffer[i + 1] = g;
                    outBuffer[i + 2] = r;
                }
            }

            Marshal.Copy(outBuffer, 0, outData.Scan0, bytes);
            output.UnlockBits(outData);

            return output;
        }


        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                myDevice.Sendmessage(); 
                IDataObject data = Clipboard.GetDataObject();

                if (data != null && data.GetDataPresent(DataFormats.Bitmap))
                {
                    Bitmap bmp = (Bitmap)data.GetData(DataFormats.Bitmap);
                    if (bmp != null)
                    {
                        pictureBox1.Image = (Bitmap)bmp.Clone();

                        Bitmap filtered = ApplyFilter(bmp, currentFilter);
                        pictureBox2.Image = filtered;

                        bmp.Dispose(); 
                    }
                }
            }
            catch
            {

            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();  
                    videoSource.WaitForStop();     
                    videoSource.NewFrame -= VideoSource_NewFrame;

                    pictureBox1.Image?.Dispose();
                    pictureBox2.Image?.Dispose();
                    liveFrame?.Dispose();

                    pictureBox1.Image = null;
                    pictureBox2.Image = null;
                    liveFrame = null;

                    currentFilter = FilterType.None;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping webcam: " + ex.Message);
            }
        }

        private void webCamProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //delete
        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                pictureBox3.Image.Dispose(); 
                pictureBox3.Image = null;   
            }

            
            currentBackground?.Dispose();
            currentBackground = null;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No processed image to save!", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                sfd.Title = "Save Processed Image";

              
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                sfd.FileName = $"processed_image_{timestamp}";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (Bitmap bmp = new Bitmap(pictureBox2.Image))
                        {
                            System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                            if (sfd.FileName.ToLower().EndsWith(".jpg") || sfd.FileName.ToLower().EndsWith(".jpeg"))
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            else if (sfd.FileName.ToLower().EndsWith(".bmp"))
                                format = System.Drawing.Imaging.ImageFormat.Bmp;

                            bmp.Save(sfd.FileName, format);
                        }

                        MessageBox.Show("Image saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to save image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Smooth;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void gaussianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.GaussianBlur;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void sharpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Sharpen;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void meanRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.MeanRemoval;
            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void horzVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.HorzVertical;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void allDirectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.AllDirections;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void embosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Emboss;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void lossyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.Lossy;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.HorizontalOnly;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilter = FilterType.VerticalOnly;

            if (pictureBox1.Image != null)
            {
                Bitmap input = new Bitmap(pictureBox1.Image);
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = ApplyFilterFast(input, currentFilter);
            }
        }

        private Bitmap ApplyFilter(Bitmap source, FilterType filter)
        {
            if (filter == FilterType.Grayscale)
            {
                Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
                BitmapData data = source.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int stride = data.Stride;
                IntPtr scan0 = data.Scan0;
                int bytes = Math.Abs(stride) * source.Height;
                byte[] rgbValues = new byte[bytes];

                System.Runtime.InteropServices.Marshal.Copy(scan0, rgbValues, 0, bytes);

                for (int i = 0; i < rgbValues.Length; i += 3)
                {
                    byte b = rgbValues[i];
                    byte g = rgbValues[i + 1];
                    byte r = rgbValues[i + 2];

                    byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

                    rgbValues[i] = gray;     // B
                    rgbValues[i + 1] = gray; // G
                    rgbValues[i + 2] = gray; // R
                }

                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, scan0, bytes);

                source.UnlockBits(data);
            }

            return source;
        }




    }
}
