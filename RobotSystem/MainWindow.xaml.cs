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

                string classesPath = System.IO.Path.Combine(modelPath, "classes.txt");
                string[] classNames = null;
                if (File.Exists(classesPath))
                {
                    classNames = File.ReadAllLines(classesPath)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .ToArray();
                }

                using (var session = new InferenceSession(onnxModelPath))
                {
                    // Đọc thông tin output từ model
                    var outputName = session.OutputNames.First();
                    session.OutputMetadata.TryGetValue(outputName, out var outputMetadata);

                    var elementType = outputMetadata.ElementType;
                    var dimensions = outputMetadata.Dimensions;
                    string dimString = string.Join(", ", dimensions.Select(d => d.ToString()));

                    using (var originalBmp = new Bitmap(imagePath))
                    {
                        var inputTensor = PreprocessImage(originalBmp);
                        var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor("images", inputTensor)
                    };

                        using (var results = session.Run(inputs))
                        {
                            var outputTensor = results.First().AsTensor<float>();
                            var foobar = JsonSerializer.Serialize(outputTensor);
                            var detected = new List<object>();
                            int batchSize = outputTensor.Dimensions[0];
                            int numDetections = outputTensor.Dimensions[1];
                            int anchors = outputTensor.Dimensions[2];

                            for (int b = 0; b < batchSize; b++)
                            {
                                for (int i = 0; i < numDetections; i++)
                                {
                                    if (anchors >= 6)
                                    {
                                        float x1 = outputTensor[b, i, 0];
                                        float y1 = outputTensor[b, i, 1];
                                        float x2 = outputTensor[b, i, 2];
                                        float y2 = outputTensor[b, i, 3];
                                        float conf = outputTensor[b, i, 4];
                                        int classId = (int)outputTensor[b, i, 5];

                                        if (conf > 0.5f)
                                        {
                                            string className = "Unknown";
                                            if (classNames != null && classId >= 0 && classId < classNames.Length)
                                                className = classNames[classId];

                                            detected.Add(new
                                            {
                                                STT = detected.Count + 1,
                                                ComponentType = className,
                                                Confidence = $"{conf}"
                                            });
                                        }
                                    }
                                }
                            }

                            img_InputImage.Source = ConvertToBitmapSource(originalBmp);
                            dtg_Result.ItemsSource = detected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhận dạng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DenseTensor<float> PreprocessImage(Bitmap bmp)
        {
            int inputWidth = 640;
            int inputHeight = 640;

            using (var resized = new Bitmap(bmp, new System.Drawing.Size(inputWidth, inputHeight)))
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

                return input;
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
