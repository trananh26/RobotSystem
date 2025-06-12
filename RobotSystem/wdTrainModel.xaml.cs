using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Text.Json;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;

namespace RobotSystem
{
    /// <summary>
    /// Interaction logic for wdTrainModel.xaml
    /// </summary>
    public partial class wdTrainModel : Window
    {
        private System.Windows.Point startPoint;
        private System.Windows.Point startPointUI;
        private bool isSelecting = false;
        private List<SampleInfo> sampleList = new List<SampleInfo>();
        private string templateFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateData");
        private int templateIndex = 1;

        public class SampleInfo
        {
            public int STT { get; set; }
            public string ComponentType { get; set; }
            public int Count { get; set; }
        }

        public wdTrainModel(ImageSource sourceImage = null)
        {
            InitializeComponent();
            if (!Directory.Exists(templateFolder)) Directory.CreateDirectory(templateFolder);
            if (sourceImage != null)
            {
                imgTraining.Source = sourceImage;
            }
            UpdateTemplateList();
        }

        private System.Drawing.Bitmap ConvertBitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[height * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);
            return new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                              System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0));
        }

        private BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
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

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        private void UpdateTemplateList()
        {
            var files = Directory.GetFiles(templateFolder, "*.png");
            var labelCount = files.Select(f => System.IO.Path.GetFileName(f).Split('_')[0])
                                  .GroupBy(l => l)
                                  .Select(g => new SampleInfo { ComponentType = g.Key, Count = g.Count() })
                                  .ToList();
            for (int i = 0; i < labelCount.Count; i++) labelCount[i].STT = i + 1;
            dgSamples.ItemsSource = labelCount;
        }

        private void btnOpenImage_Click(object sender, RoutedEventArgs e)
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
                    imgTraining.Source = bitmap;
                    selectionRect.Visibility = Visibility.Collapsed;
                    UpdateStatus();
                    UpdateTemplateList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private System.Windows.Point GetImageCoordinates(System.Windows.Point mousePosition)
        {
            if (imgTraining.Source == null) return mousePosition;
            double imgW = imgTraining.Source.Width;
            double imgH = imgTraining.Source.Height;
            double ctrlW = imgTraining.ActualWidth;
            double ctrlH = imgTraining.ActualHeight;
            double ratio = Math.Min(ctrlW / imgW, ctrlH / imgH);
            double displayW = imgW * ratio;
            double displayH = imgH * ratio;
            double offsetX = (ctrlW - displayW) / 2;
            double offsetY = (ctrlH - displayH) / 2;
            double x = (mousePosition.X - offsetX) / ratio;
            double y = (mousePosition.Y - offsetY) / ratio;
            return new System.Windows.Point(Math.Max(0, Math.Min(x, imgW)), Math.Max(0, Math.Min(y, imgH)));
        }

        private void imgTraining_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (imgTraining.Source == null) return;
            startPointUI = e.GetPosition(imageContainer);
            startPoint = GetImageCoordinates(startPointUI); 
            isSelecting = true;
            selectionRect.Visibility = Visibility.Visible;
            selectionRect.Margin = new Thickness(startPointUI.X, startPointUI.Y, 0, 0);
            selectionRect.Width = 0;
            selectionRect.Height = 0;
            this.PreviewMouseRightButtonUp += Window_PreviewMouseRightButtonUp;
        }

        private void imgTraining_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;
            System.Windows.Point currentUI = e.GetPosition(imageContainer);
            selectionRect.Margin = new Thickness(
                Math.Min(currentUI.X, startPointUI.X),
                Math.Min(currentUI.Y, startPointUI.Y),
                0, 0);
            selectionRect.Width = Math.Abs(currentUI.X - startPointUI.X);
            selectionRect.Height = Math.Abs(currentUI.Y - startPointUI.Y);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhả chuột phải sau khi chọn vùng trên ảnh huấn luyện.
        /// Hàm này kết thúc quá trình chọn vùng, kiểm tra tính hợp lệ của vùng chọn
        /// và thực hiện lưu template nếu vùng chọn hợp lệ.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 1. Kiểm tra trạng thái chọn: Nếu không đang trong quá trình chọn, thoát hàm.
            if (!isSelecting) return;

            // 2. Đặt lại trạng thái chọn: Kết thúc quá trình chọn.
            isSelecting = false;

            // 3. Lấy tọa độ điểm cuối của vùng chọn trên ảnh gốc:
            // Sử dụng GetImageCoordinates để chuyển đổi tọa độ UI thành tọa độ ảnh gốc.
            System.Windows.Point endPoint = GetImageCoordinates(e.GetPosition(imageContainer));

            // 4. Tạo đối tượng Rect đại diện cho vùng được chọn trên ảnh gốc:
            // Tính toán X, Y, Width, Height của vùng chọn, đảm bảo chúng là giá trị dương
            // bằng cách sử dụng Math.Min và Math.Abs.
            Rect selectedRegion = new Rect(
                Math.Min(startPoint.X, endPoint.X),
                Math.Min(startPoint.Y, endPoint.Y),
                Math.Abs(endPoint.X - startPoint.X),
                Math.Abs(endPoint.Y - startPoint.Y)
            );

            // 5. Kiểm tra tính hợp lệ của vùng chọn:
            // Vùng chọn được coi là hợp lệ nếu cả chiều rộng và chiều cao đều lớn hơn 10 pixel
            // (để tránh các vùng chọn rất nhỏ hoặc không mong muốn).
            if (selectedRegion.Width > 10 && selectedRegion.Height > 10)
            {
                // 6. Kiểm tra xem người dùng đã chọn loại linh kiện chưa:
                var selectedItem = cboComponentType.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    // 7. Lấy nhãn (loại linh kiện) và tạo tên file:
                    string label = selectedItem.Content.ToString();
                    // Tên file được tạo theo định dạng "Label_YYYYMMDDHHmmssfff.png"
                    string fileName = $"{label}_{DateTime.Now:yyyyMMddHHmmssfff}.png";
                    // Tạo đường dẫn đầy đủ để lưu file.
                    string savePath = System.IO.Path.Combine(templateFolder, fileName);

                    // 8. Lưu ảnh template:
                    // Gọi hàm SaveTemplateImage để cắt và lưu vùng ảnh đã chọn.
                    SaveTemplateImage(selectedRegion, savePath);

                    // 9. Thông báo và cập nhật danh sách:
                    MessageBox.Show($"Đã lưu template: {fileName}", "Thông báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    UpdateTemplateList(); // Cập nhật lại danh sách các mẫu đã huấn luyện trên DataGrid.
                }
                else
                {
                    // Nếu chưa chọn loại linh kiện, hiển thị cảnh báo và ẩn khung chọn.
                    MessageBox.Show("Vui lòng chọn loại linh kiện trước khi chọn vùng", "Thông báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    selectionRect.Visibility = Visibility.Collapsed; // Ẩn khung chọn
                }
            }
            else
            {
                // Nếu vùng chọn không hợp lệ (quá nhỏ), ẩn khung chọn.
                selectionRect.Visibility = Visibility.Collapsed;
            }

            // 10. Gỡ bỏ sự kiện: Quan trọng để tránh việc xử lý sự kiện trùng lặp
            // khi người dùng nhả chuột phải trong các lần tương tác sau.
            this.PreviewMouseRightButtonUp -= Window_PreviewMouseRightButtonUp;
        }

        /// <summary>
        /// Cắt một vùng ảnh được chọn và lưu nó dưới dạng template. Đồng thời hiển thị ảnh đã cắt lên imgSelectedSample.
        /// </summary>
        /// <param name="region">Vùng ảnh được chọn, sử dụng tọa độ ảnh gốc.</param>
        /// <param name="savePath">Đường dẫn đầy đủ để lưu file template PNG.</param>
        private void SaveTemplateImage(Rect region, string savePath)
        {
            try // Khối try-catch để bắt các ngoại lệ có thể xảy ra trong quá trình xử lý ảnh.
            {
                // 1. Kiểm tra nguồn ảnh:
                // Đảm bảo rằng imgTraining.Source (ảnh đang hiển thị trên giao diện) thực sự là một BitmapSource.
                if (imgTraining.Source is BitmapSource bmpSrc)
                {
                    // 2. Chuyển đổi BitmapSource sang System.Drawing.Bitmap:
                    // WPF sử dụng BitmapSource, trong khi thư viện System.Drawing (để cắt ảnh) sử dụng System.Drawing.Bitmap.
                    // Hàm ConvertBitmapSourceToBitmap sẽ chuyển đổi định dạng này.
                    using (System.Drawing.Bitmap originalBitmap = ConvertBitmapSourceToBitmap(bmpSrc))
                    {
                       
                        // 3. Tính toán vùng cắt (cropRect) trong tọa độ pixel của ảnh gốc:
                        // `region` là một System.Windows.Rect chứa tọa độ và kích thước của vùng chọn trên ảnh (đã được chuyển đổi sang tọa độ ảnh gốc).
                        // Math.Floor: Làm tròn xuống để đảm bảo lấy trọn vẹn pixel bắt đầu.
                        // Math.Ceiling: Làm tròn lên để đảm bảo vùng cắt bao phủ toàn bộ pixel kết thúc.
                        int cropX = (int)Math.Floor(region.X);
                        int cropY = (int)Math.Floor(region.Y);
                        int cropWidth = (int)Math.Ceiling(region.Width);
                        int cropHeight = (int)Math.Ceiling(region.Height);

                        // 4. Điều chỉnh vùng cắt để nằm hoàn toàn trong ảnh gốc:
                        // Đây là phần quan trọng để tránh lỗi khi vùng chọn vượt ra ngoài biên của ảnh.

                        // Điều chỉnh cropX và cropWidth theo chiều ngang:
                        if (cropX < 0) // Nếu điểm bắt đầu X âm (vượt biên trái)
                        {
                            cropWidth += cropX; // Giảm chiều rộng đi phần bị tràn ra ngoài
                            cropX = 0; // Đặt lại X về 0
                        }
                        if (cropX + cropWidth > originalBitmap.Width) // Nếu điểm kết thúc X vượt biên phải
                        {
                            cropWidth = originalBitmap.Width - cropX; // Giảm chiều rộng để vừa với ảnh
                        }
                        cropWidth = Math.Max(0, cropWidth); // Đảm bảo chiều rộng không bao giờ âm

                        // Điều chỉnh cropY và cropHeight theo chiều dọc:
                        if (cropY < 0) // Nếu điểm bắt đầu Y âm (vượt biên trên)
                        {
                            cropHeight += cropY; // Giảm chiều cao đi phần bị tràn ra ngoài
                            cropY = 0; // Đặt lại Y về 0
                        }
                        if (cropY + cropHeight > originalBitmap.Height) // Nếu điểm kết thúc Y vượt biên dưới
                        {
                            cropHeight = originalBitmap.Height - cropY; // Giảm chiều cao để vừa với ảnh
                        }
                        cropHeight = Math.Max(0, cropHeight); // Đảm bảo chiều cao không bao giờ âm

                        // 5. Tạo đối tượng System.Drawing.Rectangle cho vùng cắt:
                        System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight);

                        // 6. Kiểm tra kích thước vùng cắt hợp lệ:
                        if (cropRect.Width <= 0 || cropRect.Height <= 0)
                        {
                            MessageBox.Show("Vùng chọn quá nhỏ hoặc không hợp lệ để cắt ảnh. Vui lòng chọn lại.", "Cảnh báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            return; // Thoát nếu vùng cắt không hợp lệ
                        }

                        // Bỏ comment dòng dưới để hiển thị thông tin debug về cropRect nếu cần
                        // MessageBox.Show($"Cropping image with Rect: X={cropRect.X}, Y={cropRect.Y}, Width={cropRect.Width}, Height={cropRect.Height}", "Debug Info - Crop Rectangle", MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                        // 7. Cắt ảnh và lưu:
                        // Sử dụng originalBitmap.Clone() với cropRect để tạo ra một Bitmap mới chỉ chứa vùng đã chọn.
                        using (System.Drawing.Bitmap croppedBitmap = originalBitmap.Clone(cropRect, originalBitmap.PixelFormat))
                        {
                            croppedBitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Png); // Lưu ảnh đã cắt dưới dạng PNG
                            imgSelectedSample.Source = ConvertToBitmapSource(croppedBitmap); // Hiển thị ảnh đã cắt lên imgSelectedSample
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 8. Xử lý lỗi:
                // Nếu có bất kỳ lỗi nào xảy ra trong quá trình, hiển thị thông báo lỗi.
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateStatus()
        {
            txtStatus.Text = "Sẵn sàng"; // Cập nhật trạng thái chung của cửa sổ huấn luyện.
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Các mẫu đã được lưu vào thư mục TemplateData", "Thông báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            DialogResult = true; // Đặt DialogResult = true để cho biết cửa sổ đóng thành công.
            Close(); // Đóng cửa sổ huấn luyện.
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Đặt DialogResult = false để cho biết cửa sổ đóng không thành công hoặc bị hủy.
            Close(); // Đóng cửa sổ huấn luyện.
        }

        private void btnDeleteSample_Click(object sender, RoutedEventArgs e)
        {
            // Xử lý sự kiện khi nhấn nút "Xóa mẫu"
            if (dgSamples.SelectedItem is SampleInfo selected)
            {
                string label = selected.ComponentType; // Lấy loại linh kiện được chọn từ DataGrid.
                // Tìm tất cả các file template có cùng label trong thư mục TemplateData.
                var files = System.IO.Directory.GetFiles(templateFolder, $"{label}_*.png");

                if (files.Length == 0)
                {
                    MessageBox.Show($"Không tìm thấy mẫu nào để xóa cho '{label}'", "Thông báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                // Hỏi người dùng xác nhận trước khi xóa.
                if (MessageBox.Show($"Bạn có chắc muốn xóa tất cả mẫu của '{label}'?", "Xác nhận xóa", MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    foreach (var file in files)
                    {
                        try { System.IO.File.Delete(file); } catch { } // Xóa từng file template.
                    }
                    MessageBox.Show($"Đã xóa tất cả mẫu của '{label}'", "Thông báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    UpdateTemplateList(); // Cập nhật lại danh sách mẫu trên DataGrid.
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn loại mẫu muốn xóa trong bảng!", "Thông báo", MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }
    }
}
