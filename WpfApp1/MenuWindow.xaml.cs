using System;
using System.ComponentModel;
using System.Windows;
using dotenv.net;
using System.IO;


namespace Chatbar
{
    /// <summary>
    /// MenuWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MenuWindow : Window
    {
        private static MenuWindow? _instance;
        private MenuWindow()
        {
            InitializeComponent();
        }
        public static MenuWindow GetInstance()
        {
            _instance ??= new MenuWindow();
            return _instance;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DotEnv.Load();
            APIKEY.Text = Environment.GetEnvironmentVariable("API_KEY");
            APIBASE.Text = Environment.GetEnvironmentVariable("API_BASE");
            Prompt.Text = Environment.GetEnvironmentVariable("PROMPT");
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            _instance = null;
            string apiBase =APIBASE.Text;
            string apiKey = APIKEY.Text;
            string prompt = Prompt.Text;
            string envContents = $"API_KEY={apiKey}\nPROMPT={prompt}\nAPI_BASE={apiBase}";
            File.WriteAllText(".env", envContents);
            base.OnClosing(e);
        }
    }
}
