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

namespace RobotSystem
{
    /// <summary>
    /// Interaction logic for wdTrainModel.xaml
    /// </summary>
    public partial class wdTrainModel : Window
    {
        private Point startPoint;
        private bool isSelecting = false;
        private List<ComponentRegion> selectedRegions = new List<ComponentRegion>();

        public class ComponentRegion
        {
            public Rect Region { get; set; }
            public string ComponentType { get; set; }
        }

        public wdTrainModel(ImageSource sourceImage = null)
        {
            InitializeComponent();
            if (sourceImage != null)
            {
                imgTraining.Source = sourceImage;
            }
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
                    selectedRegions.Clear();
                    selectionRect.Visibility = Visibility.Collapsed;
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void imgTraining_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (imgTraining.Source == null) return;

            startPoint = e.GetPosition(imageContainer);
            isSelecting = true;

            selectionRect.Visibility = Visibility.Visible;
            selectionRect.Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0);
            selectionRect.Width = 0;
            selectionRect.Height = 0;

            // Bắt sự kiện MouseRightButtonUp cho toàn bộ cửa sổ
            this.PreviewMouseRightButtonUp += Window_PreviewMouseRightButtonUp;
        }

        private void Window_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isSelecting) return;

            isSelecting = false;
            Point endPoint = e.GetPosition(imageContainer);

            Rect selectedRegion = new Rect(
                Math.Min(startPoint.X, endPoint.X),
                Math.Min(startPoint.Y, endPoint.Y),
                Math.Abs(endPoint.X - startPoint.X),
                Math.Abs(endPoint.Y - startPoint.Y)
            );

            if (selectedRegion.Width > 10 && selectedRegion.Height > 10)
            {
                var selectedItem = cboComponentType.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    selectedRegions.Add(new ComponentRegion
                    {
                        Region = selectedRegion,
                        ComponentType = selectedItem.Content.ToString()
                    });
                    UpdateStatus();
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn loại linh kiện trước khi chọn vùng", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    selectionRect.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                selectionRect.Visibility = Visibility.Collapsed;
            }

            // Hủy đăng ký sự kiện
            this.PreviewMouseRightButtonUp -= Window_PreviewMouseRightButtonUp;
        }

        private void imgTraining_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;

            Point currentPoint = e.GetPosition(imageContainer);
            double width = Math.Abs(currentPoint.X - startPoint.X);
            double height = Math.Abs(currentPoint.Y - startPoint.Y);

            selectionRect.Margin = new Thickness(
                Math.Min(currentPoint.X, startPoint.X),
                Math.Min(currentPoint.Y, startPoint.Y),
                0, 0);

            selectionRect.Width = width;
            selectionRect.Height = height;
        }

        private void imgTraining_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Không cần xử lý ở đây nữa vì đã chuyển sang Window_PreviewMouseRightButtonUp
        }

        private void UpdateStatus()
        {
            txtStatus.Text = $"Đã chọn {selectedRegions.Count} vùng";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRegions.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một vùng linh kiện", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Lưu dữ liệu huấn luyện
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
