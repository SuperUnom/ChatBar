using System;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace Chatbar
{
    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartWindow : Window
    {
        private bool isClose = false;
        public StartWindow()
        {
            InitializeComponent();

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
        private void Text_Change(object sender, EventArgs e)
        {
            if (MyText.Text.Length > 0)
            {
                PlaceHolder.Visibility = Visibility.Hidden;
            }
            else
            {
                PlaceHolder.Visibility = Visibility.Visible;
            }
        }
        //窗口失去聚焦事件
        private void Window_Deactivated(object sender, EventArgs e)
        {
            //此时不是窗口关闭事件
            if (this.isClose == false)
            {
                this.Close();
            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MyText.Text.Replace(Environment.NewLine, "")))
            {
                e.Handled = true;
                MainWindow window = MainWindow.GetInstance();
                isClose = true;
                this.Close();
                window.Show();
                window.MyText.Text = MyText.Text;
                window.TextBox_KeyDown(sender, e);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = 180; // 将窗口的顶部位置设置为屏幕顶部
            this.Left = (SystemParameters.PrimaryScreenWidth - this.ActualWidth) / 2; // 将窗口的左侧位置设置为屏幕中央
            Activate();
            MyText.Focus();
        }
    }
}
