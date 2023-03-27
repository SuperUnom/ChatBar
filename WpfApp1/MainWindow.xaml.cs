using Chatbar.ScrollView;
using dotenv.net;
using Newtonsoft.Json;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace Chatbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string apiKey;
        private string prompt;
        private string apiBase;
        private static MainWindow? _instance;
        private List<ChatMessage> Messages;
        private string currentTime;
        private MainWindow()
        {
            InitializeComponent();
            currentTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
        }
        private void ReadAllMessages()
        {
            int num = 0;
            string[] file_names = Directory.GetFiles("Messages");
            var sortedFileNames = file_names.OrderByDescending(file_name => DateTime.ParseExact(Path.GetFileNameWithoutExtension(file_name.Replace("Messages\\", "").Replace(".txt", "")), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).ToArray();
            foreach (string file_name in sortedFileNames)
            {
                num++;
                if (num <= 20)
                {
                    var save_border = new Border
                    {
                        Style = (Style)FindResource("Save_Border")
                    };
                    var date_text = new TextBlock
                    {
                        Style = (Style)FindResource("Save_DateText")
                    };
                    Save_StackPanel.Children.Add(save_border);
                    var name = file_name.Replace("Messages\\", "").Replace(".txt", "");
                    date_text.Text = name;
                    save_border.Tag = name;
                    save_border.Child = date_text;
                }
                else
                {
                    File.Delete(file_name);
                }
            }
        }
        public void Save(string time)
        {
            //如果列表Messages只有一个元素，就不之前下面操作
            if (Messages.Count == 1)
            {
                return;
            }
            string json = JsonConvert.SerializeObject(Messages);
            if (Directory.Exists("Messages/"))
            {
                System.IO.File.WriteAllText("Messages/" + time + ".txt", json);
            }
            else
            {
                Directory.CreateDirectory("Messages/");
                System.IO.File.WriteAllText("Messages/" + currentTime + ".txt", json);
            }
        }
        public void Read(string file_name)
        {
            currentTime = file_name;
            string json = System.IO.File.ReadAllText("Messages\\" + file_name + ".txt");
            Messages = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
            MyStackPanel.Children.Clear();
            foreach (var message in Messages)
            {
                if(message.Role== StaticValues.ChatMessageRoles.System)
                {
                    continue;
                }
                var border = new Border();
                var text = new TextBox();
                var docker = new DockPanel();
                if (message.Role == StaticValues.ChatMessageRoles.User)
                {
                    border.Style = (Style)FindResource("s2");
                    text.Style = (Style)FindResource("s3");
                }
                else if (message.Role == StaticValues.ChatMessageRoles.Assistant)
                {
                    border.Style = (Style)FindResource("s1");
                    text.Style = (Style)FindResource("s3");
                }
                border.Child = text;
                text.Text = message.Content;
                docker.Children.Add(border);
                MyStackPanel.Children.Add(docker);
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Save(currentTime);
            _instance = null;
            base.OnClosing(e);
        }
        public static MainWindow GetInstance()
        {
            _instance ??= new MainWindow();
            return _instance;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //获取窗口位置
            DotEnv.Load();
            apiBase = Environment.GetEnvironmentVariable("API_BASE") ?? "https://api.openai.com/";
            apiKey= Environment.GetEnvironmentVariable("API_KEY") ?? "NULL";
            prompt= Environment.GetEnvironmentVariable("PROMPT") ?? "你是一个人工助手";
            Messages = new(){ new(StaticValues.ChatMessageRoles.System, prompt)};
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowHeight = this.ActualHeight;
            double windowWidth = this.ActualWidth;
            this.Left = screenWidth - windowWidth;
            this.Top = 100;
            Activate();
            MyText.Focus();
        }
        void Move_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void History_click(object sender, RoutedEventArgs e)
        {
            if (Save_Border.Visibility == Visibility.Hidden)
            {
                ReadAllMessages();
                Save_Border.Visibility = Visibility.Visible;
            }
            else
            {
                Save_Border.Visibility = Visibility.Hidden;
                Save_StackPanel.Children.Clear();
            }
        }
        private void Refresh_click(object sender, RoutedEventArgs e)
        {
            Save(currentTime);
            currentTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
            Refresh();
        }
        private void Close_click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Refresh()
        {
            Messages = new List<ChatMessage>
    {
        new(StaticValues.ChatMessageRoles.System,prompt)
    };
            MyStackPanel.Children.Clear();
        }
        public async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MyText.Text.Replace(Environment.NewLine, "")))
            {
                string message = MyText.Text;
                var docker = new DockPanel();
                var border = new Border();
                var text = new TextBox();
                border.Style = (Style)FindResource("s2");
                text.Style = (Style)FindResource("s3");
                border.Child = text;
                text.Text = message;
                docker.Children.Add(border);
                MyStackPanel.Children.Add(docker);
                MyText.Text = string.Empty;
                Messages.Add(new(StaticValues.ChatMessageRoles.User, message));
                e.Handled = true;
                await Chat(Messages.Skip(Math.Max(0,Messages.Count-16)).ToList());
            }
        }
        async public Task Chat(List<ChatMessage> messages)
        {
            var border = new Border();
            var text = new TextBox();
            var docker = new DockPanel();
            border.Style = (Style)FindResource("s1");
            text.Style = (Style)FindResource("s3");

            border.Child = text;
            docker.Children.Add(border);
            MyStackPanel.Children.Add(docker);
            var sdk = new OpenAIService(new OpenAiOptions()
            {
                BaseDomain = apiBase,
                ApiKey =  apiKey,
            });
            try
            {
                var completionResult = sdk.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Model = Models.ChatGpt3_5Turbo
                });
                await Task.Run(async () =>
                {
                    var m = string.Empty;
                    await foreach (var completion in completionResult)
                    {
                        if (completion.Successful)
                        {
                            m += completion.Choices.First().Message.Content;
                            Dispatcher.Invoke(() => text.Text = m);
                        }
                        else
                        {
                            m = $"{completion.Error.Code}: {completion.Error.Message}";
                            Dispatcher.Invoke(() => text.Text = m);
                        }
                    }
                    Messages.Add(new(StaticValues.ChatMessageRoles.Assistant, m));
                });
            }
            catch (Exception ex)
            {
                // 执行失败时的代码
                text.Text = $"网络连接错误！{ex.Message}";
            }
            
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
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border clickedBorder = sender as Border;
            Read(clickedBorder.Tag.ToString());
        }
        private void ZScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ZScrollViewer zScrollViewer = sender as ZScrollViewer;

            if (Math.Abs(zScrollViewer.VerticalOffset - zScrollViewer.ScrollableHeight) < 0.01)
            {
                zScrollViewer.ScrollToEnd();
            }
        }
    }
}



