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
        private ActUtlType PLC = new ActUtlType();
        private string IP_PLC = "192.168.1.250";
        private string _oldQRCode;
        private int psWait;
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY_MS = 1000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectPLC();
            ConnectCamera();
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
                    captureDevice.NewFrame -= CaptureDevice_NewFrame;
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
                PLC.Close();
                DisconnectCamera();
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
    }
}
