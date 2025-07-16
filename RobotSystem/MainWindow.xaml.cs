using ActUtlTypeLib;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.IO;
using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using System.Configuration;

namespace RobotSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FilterInfoCollection filterInfo;
        private VideoCaptureDevice captureDevice;
        private System.Timers.Timer timer;
        //private ActUtlType PLC = new ActUtlType();
        private string IP_PLC = "192.168.1.250";
        private string _oldQRCode;
        private int psWait;
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY_MS = 1000;
        private const string TRAINING_HISTORY_FILE = "training_history.json";
        private string templateFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateData");

        public class ComponentRegion
        {
            public Rect Region { get; set; }
            public string ComponentType { get; set; }
        }

        public class TrainingHistory
        {
            public Dictionary<string, List<Rect>> ComponentRegions { get; set; } = new Dictionary<string, List<Rect>>();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ConnectPLC();
            //ConnectCamera();
        }

        private void ConnectPLC()
        {

        }

        private void ConnectCamera()
        {
            try
            {
                filterInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (filterInfo.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy thiết bị camera nào!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Thử kết nối với camera
                int retryCount = 0;
                bool connected = false;

                while (!connected && retryCount < MAX_RETRY_ATTEMPTS)
                {
                    try
                    {
                        captureDevice = new VideoCaptureDevice(filterInfo[0].MonikerString);
                        captureDevice.NewFrame += CaptureDevice_NewFrame;
                        captureDevice.Start();
                        connected = true;
                        MessageBox.Show("Kết nối camera thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        if (retryCount < MAX_RETRY_ATTEMPTS)
                        {
                            System.Threading.Thread.Sleep(RETRY_DELAY_MS);
                            continue;
                        }
                        throw new Exception($"Không thể kết nối với camera sau {MAX_RETRY_ATTEMPTS} lần thử: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối camera: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectCamera()
        {
            try
            {
                if (captureDevice != null && captureDevice.IsRunning)
                {
                    captureDevice.SignalToStop();
                    captureDevice.WaitForStop();
                    captureDevice.NewFrame += CaptureDevice_NewFrame;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi ngắt kết nối camera: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        private void CaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    using (Bitmap img = (Bitmap)eventArgs.Frame.Clone())
                    {
                        IntPtr hBitmap = img.GetHbitmap();
                        try
                        {
                            System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                hBitmap,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions()
                            );
                            img_Camera.Source = bitmapSource;
                        }
                        finally
                        {
                            DeleteObject(hBitmap);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý frame camera: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCrCommand_Click(object sender, RoutedEventArgs e)
        {

        }


        private void btnCommandHistory_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //PLC.Close();
                //DisconnectCamera();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đóng ứng dụng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dtgCrCommand_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void btnAddCommand_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteCommand_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnManualControl_Click(object sender, RoutedEventArgs e)
        {

        }



        private void btnReport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dtgCommandHistory_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void dtgVision_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void btn_OpenImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                Title = "Chọn ảnh"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();
                    img_InputImage.Source = bitmap;

                    // Thực hiện nhận dạng
                    DetectComponents(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btn_TakeImage_Click(object sender, RoutedEventArgs e)
        {
            //if (captureDevice != null && captureDevice.IsRunning)
            //{
            //    try
            //    {
            //        // Lấy frame hiện tại từ camera
            //        using (Bitmap currentFrame = (Bitmap)captureDevice.GetCurrentVideoFrame())
            //        {
            //            if (currentFrame != null)
            //            {
            //                // Chuyển đổi sang BitmapImage để hiển thị
            //                BitmapImage bitmap = new BitmapImage();
            //                using (MemoryStream ms = new MemoryStream())
            //                {
            //                    currentFrame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            //                    ms.Position = 0;
            //                    bitmap.BeginInit();
            //                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
            //                    bitmap.StreamSource = ms;
            //                    bitmap.EndInit();
            //                }
            //                img_InputImage.Source = bitmap;

            //                // Lưu ảnh tạm thời để xử lý
            //                string tempImagePath = "temp_capture.jpg";
            //                currentFrame.Save(tempImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);

            //                // Thực hiện nhận dạng
            //                DetectComponents(tempImagePath);

            //                // Xóa file tạm
            //                if (File.Exists(tempImagePath))
            //                {
            //                    File.Delete(tempImagePath);
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Lỗi khi chụp ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Camera chưa được kết nối!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        private void DetectComponents(string imagePath)
        {
            try
            {
                string modelPath = ConfigurationManager.AppSettings["ModelPath"] ?? AppDomain.CurrentDomain.BaseDirectory;
                string onnxModelPath = System.IO.Path.Combine(modelPath, "best.onnx");
                if (!File.Exists(onnxModelPath))
                {
                    MessageBox.Show("Không tìm thấy file best.onnx!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Load class names from classes.txt
                string classesPath = System.IO.Path.Combine(modelPath, "classes.txt");
                string[] classNames = null;

                if (File.Exists(classesPath))
                {
                    classNames = File.ReadAllLines(classesPath)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .ToArray();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy file classes.txt!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                using (var img = new Bitmap(imagePath))
                {
                    int inputWidth = 640;
                    int inputHeight = 640;
                    using (var resized = new Bitmap(img, new System.Drawing.Size(inputWidth, inputHeight)))
                    {
                        var input = new DenseTensor<float>(new[] { 1, 3, inputHeight, inputWidth });
                        for (int y = 0; y < inputHeight; y++)
                        {
                            for (int x = 0; x < inputWidth; x++)
                            {
                                var pixel = resized.GetPixel(x, y);
                                input[0, 0, y, x] = pixel.R / 255.0f;
                                input[0, 1, y, x] = pixel.G / 255.0f;
                                input[0, 2, y, x] = pixel.B / 255.0f;
                            }
                        }

                        using (var session = new InferenceSession(onnxModelPath))
                        {
                            var inputs = new List<NamedOnnxValue>
                            {
                                NamedOnnxValue.CreateFromTensor("images", input)
                            };

                            using (var results = session.Run(inputs))
                            {
                                // Get output tensor và hiển thị shape để debug
                                var outputTensor = results.First().AsTensor<float>();
                                var dims = outputTensor.Dimensions.ToArray();

                                // Debug: In ra shape để xác nhận
                                string shapeStr = string.Join(", ", dims);
                                MessageBox.Show($"Output shape: [{shapeStr}]", "Debug Shape", MessageBoxButton.OK, MessageBoxImage.Information);

                                var detected = new List<object>();
                                using (var g = Graphics.FromImage(img))
                                {
                                    // Kiểm tra format output
                                    if (dims.Length == 3)
                                    {
                                        // Format [1, N, attrs]
                                        int numDetections = dims[1];
                                        int numAttrs = dims[2];

                                        for (int i = 0; i < numDetections; i++)
                                        {
                                            // YOLOv11L thường có format [x_center, y_center, width, height, ...classes]
                                            // Nhưng có thể normalized về [0,1] hoặc pixel coordinates

                                            float x_center = outputTensor[0, i, 0];
                                            float y_center = outputTensor[0, i, 1];
                                            float width = outputTensor[0, i, 2];
                                            float height = outputTensor[0, i, 3];

                                            // Tìm class có confidence cao nhất
                                            int classId = -1;
                                            float maxClassConf = 0;
                                            for (int c = 4; c < numAttrs; c++)
                                            {
                                                float conf = outputTensor[0, i, c];
                                                if (conf > maxClassConf)
                                                {
                                                    maxClassConf = conf;
                                                    classId = c - 4;
                                                }
                                            }

                                            // Kiểm tra threshold
                                            if (maxClassConf < 0.5f) continue;

                                            // Chuyển đổi tọa độ - kiểm tra nếu đã normalized
                                            float x_real, y_real, w_real, h_real;
                                            if (x_center <= 1.0f && y_center <= 1.0f && width <= 1.0f && height <= 1.0f)
                                            {
                                                // Coordinates are normalized [0,1]
                                                x_real = x_center * img.Width;
                                                y_real = y_center * img.Height;
                                                w_real = width * img.Width;
                                                h_real = height * img.Height;
                                            }
                                            else
                                            {
                                                // Coordinates are in input resolution
                                                x_real = x_center * img.Width / inputWidth;
                                                y_real = y_center * img.Height / inputHeight;
                                                w_real = width * img.Width / inputWidth;
                                                h_real = height * img.Height / inputHeight;
                                            }

                                            // Tính tọa độ góc trên trái
                                            float x1 = x_real - w_real / 2;
                                            float y1 = y_real - h_real / 2;

                                            // Đảm bảo tọa độ hợp lệ
                                            x1 = Math.Max(0, x1);
                                            y1 = Math.Max(0, y1);
                                            w_real = Math.Min(w_real, img.Width - x1);
                                            h_real = Math.Min(h_real, img.Height - y1);

                                            if (w_real <= 0 || h_real <= 0) continue;

                                            var rect = new System.Drawing.Rectangle((int)x1, (int)y1, (int)w_real, (int)h_real);
                                            g.DrawRectangle(Pens.Lime, rect);

                                            // Get class name from classes.txt or use fallback
                                            string className = "Unknown";
                                            if (classNames != null && classId >= 0 && classId < classNames.Length)
                                            {
                                                className = classNames[classId];
                                            }
                                            else if (classNames == null)
                                            {
                                                className = $"Class {classId}";
                                            }

                                            // Vẽ label
                                            string label = $"{className} {maxClassConf:0.00}";
                                            var font = new Font("Arial", 12, System.Drawing.FontStyle.Bold);
                                            var textSize = g.MeasureString(label, font);
                                            var textRect = new System.Drawing.RectangleF(x1, y1 - textSize.Height, textSize.Width, textSize.Height);
                                            g.FillRectangle(System.Drawing.Brushes.Black, textRect);
                                            g.DrawString(label, font, System.Drawing.Brushes.Lime, x1, y1 - textSize.Height);

                                            detected.Add(new
                                            {
                                                ComponentType = className,
                                                Confidence = maxClassConf,
                                                Location = new System.Windows.Rect(rect.X, rect.Y, rect.Width, rect.Height)
                                            });
                                        }
                                    }
                                    else if (dims.Length == 2)
                                    {
                                        // Format [N, attrs] - xử lý tương tự nhưng không có batch dimension
                                        int numDetections = dims[0];
                                        int numAttrs = dims[1];

                                        for (int i = 0; i < numDetections; i++)
                                        {
                                            float x_center = outputTensor[i, 0];
                                            float y_center = outputTensor[i, 1];
                                            float width = outputTensor[i, 2];
                                            float height = outputTensor[i, 3];

                                            // Tìm class có confidence cao nhất
                                            int classId = -1;
                                            float maxClassConf = 0;
                                            for (int c = 4; c < numAttrs; c++)
                                            {
                                                float conf = outputTensor[i, c];
                                                if (conf > maxClassConf)
                                                {
                                                    maxClassConf = conf;
                                                    classId = c - 4;
                                                }
                                            }

                                            if (maxClassConf < 0.5f) continue;

                                            // Chuyển đổi tọa độ
                                            float x_real, y_real, w_real, h_real;
                                            if (x_center <= 1.0f && y_center <= 1.0f && width <= 1.0f && height <= 1.0f)
                                            {
                                                x_real = x_center * img.Width;
                                                y_real = y_center * img.Height;
                                                w_real = width * img.Width;
                                                h_real = height * img.Height;
                                            }
                                            else
                                            {
                                                x_real = x_center * img.Width / inputWidth;
                                                y_real = y_center * img.Height / inputHeight;
                                                w_real = width * img.Width / inputWidth;
                                                h_real = height * img.Height / inputHeight;
                                            }

                                            float x1 = x_real - w_real / 2;
                                            float y1 = y_real - h_real / 2;

                                            x1 = Math.Max(0, x1);
                                            y1 = Math.Max(0, y1);
                                            w_real = Math.Min(w_real, img.Width - x1);
                                            h_real = Math.Min(h_real, img.Height - y1);

                                            if (w_real <= 0 || h_real <= 0) continue;

                                            var rect = new System.Drawing.Rectangle((int)x1, (int)y1, (int)w_real, (int)h_real);
                                            g.DrawRectangle(Pens.Lime, rect);

                                            // Get class name from classes.txt or use fallback
                                            string className = "Unknown";
                                            if (classNames != null && classId >= 0 && classId < classNames.Length)
                                            {
                                                className = classNames[classId];
                                            }
                                            else if (classNames == null)
                                            {
                                                className = $"Class {classId}";
                                            }

                                            string label = $"{className} {maxClassConf:0.00}";
                                            var font = new Font("Arial", 12, System.Drawing.FontStyle.Bold);
                                            var textSize = g.MeasureString(label, font);
                                            var textRect = new System.Drawing.RectangleF(x1, y1 - textSize.Height, textSize.Width, textSize.Height);
                                            g.FillRectangle(System.Drawing.Brushes.Black, textRect);
                                            g.DrawString(label, font, System.Drawing.Brushes.Lime, x1, y1 - textSize.Height);

                                            detected.Add(new
                                            {
                                                ComponentType = className,
                                                Confidence = maxClassConf,
                                                Location = new System.Windows.Rect(rect.X, rect.Y, rect.Width, rect.Height)
                                            });
                                        }
                                    }
                                }

                                img_InputImage.Source = ConvertToBitmapSource(img);
                                dtg_Result.ItemsSource = detected;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhận dạng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    handle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        private void btn_Train_Click(object sender, RoutedEventArgs e)
        {
            ImageSource sourceImage = null;
            if (img_InputImage.Source != null)
            {
                sourceImage = img_InputImage.Source;
            }

            var trainWindow = new wdTrainModel(sourceImage);
            if (trainWindow.ShowDialog() == true)
            {
                // TODO: Xử lý kết quả huấn luyện
                MessageBox.Show("Huấn luyện thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btn_Pass_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Fail_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAGV_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
