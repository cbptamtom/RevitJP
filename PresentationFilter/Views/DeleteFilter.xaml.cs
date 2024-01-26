using PresentationFilter.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PresentationFilter.Views
{
    /// <summary>
    /// Interaction logic for DeleteFilter.xaml
    /// </summary>
    public partial class DeleteFilter : UserControl
    {
        public DeleteFilter()
        {
            InitializeComponent();
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedCheckBox = sender as CheckBox;

            if (clickedCheckBox != null)
            {
                bool isChecked = clickedCheckBox.IsChecked ?? false;

                if (isShiftKeyPressed)
                {
                    // Nếu đang nhấn phím Shift, chọn nhiều mục
                    int startIndex = lbViews3D.Items.IndexOf(lbViews3D.SelectedItems[0]);
                    int endIndex = lbViews3D.Items.IndexOf(clickedCheckBox.DataContext);

                    for (int i = Math.Min(startIndex, endIndex); i <= Math.Max(startIndex, endIndex); i++)
                    {
                        var item = lbViews3D.Items[i] as FilterterDel;
                        if (item != null)
                        {
                            item.Selected = isChecked;
                        }
                    }
                }
                else
                {
                    // Ngược lại, chỉ chọn/deselect mục đang được nhấp vào
                    var clickedItem = clickedCheckBox.DataContext as FilterterDel;
                    if (clickedItem != null)
                    {
                        clickedItem.Selected = isChecked;
                    }
                }
            }
        }

        //private void CheckBox_Click(object sender, RoutedEventArgs e)
        //{
        //    if (isShiftKeyPressed)
        //    {
        //        foreach (var item in lbViews3D.SelectedItems)
        //        {
        //            // Đánh dấu tất cả các mục đang được chọn
        //            // Nếu đã chọn, bỏ chọn và ngược lại
        //            var dataItem = item; // Thay thế YourItemType bằng kiểu dữ liệu thực của mục trong danh sách
        //            dataItem.Selected = !dataItem.Selected;
        //        }
        //    }
        //}

        private bool isShiftKeyPressed = false;

        private void lbViews3D_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                isShiftKeyPressed = true;
            }
        }

        private void lbViews3D_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                isShiftKeyPressed = false;
            }
        }
    }
}
