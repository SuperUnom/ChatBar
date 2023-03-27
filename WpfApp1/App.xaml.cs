using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace Chatbar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem exitMenuItem;
        private ToolStripMenuItem showMainWindowItem;
        private ToolStripMenuItem showMenuWindowItem;
        private IContainer components;
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private const int HOTKEY_ID = 9000;
        private const int MOD_ALT = 0x0001;
        private const int VK_Q = 0x51;
        IntPtr Globalhandle;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeNotifyIcon();
            var window = new System.Windows.Window();
            IntPtr handle = new WindowInteropHelper(window).Handle;
            Globalhandle = handle;
            RegisterHotKey(handle, HOTKEY_ID, MOD_ALT, VK_Q);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }
        private void InitializeNotifyIcon()
        {
            components = new Container();
            contextMenu = new ContextMenuStrip();
            exitMenuItem = new ToolStripMenuItem();
            showMainWindowItem = new ToolStripMenuItem();
            showMenuWindowItem = new ToolStripMenuItem();

            // Initialize context menu
            contextMenu.SuspendLayout();

            // Show window menu item
            showMainWindowItem.Text = "显示窗口";
            showMainWindowItem.Click += (sender, e) => ShowMainWindow();
            contextMenu.Items.Add(showMainWindowItem);

            showMenuWindowItem.Text = "显示菜单";
            showMenuWindowItem.Click += (sender, e) => ShowMenuWindow();
            contextMenu.Items.Add(showMenuWindowItem);
            // Exit menu item
            exitMenuItem.Text = "退出";
            exitMenuItem.Click += (sender, e) => ExitApplication();
            contextMenu.Items.Add(exitMenuItem);



            contextMenu.ResumeLayout(false);

            // Initialize notify icon
            notifyIcon = new NotifyIcon(components)
            {
                Icon = new Icon("chatbar.ico"), // Replace "icon.ico" with your desired icon file
                ContextMenuStrip = contextMenu,
                Text = "WPF System Tray Demo",
                Visible = true
            };

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }
        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ShowMainWindow();
        }
        private static void ShowMenuWindow()
        {
            var window = MenuWindow.GetInstance();
            window.WindowState=WindowState.Normal;
            window.Show();
        }
        private static void ShowMainWindow()
        {
            _ = new StartWindow
            {
                WindowState = WindowState.Normal,
                Visibility = Visibility.Visible
            };
        }
        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            Shutdown();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            UnregisterHotKey(Globalhandle, HOTKEY_ID);
            base.OnExit(e);
            if (components != null)
            {
                components.Dispose();
            }
        }
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == 0x0312 && msg.wParam.ToInt32() == HOTKEY_ID)
            {
                ShowMainWindow();
            }
        }
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            UnregisterHotKey(Globalhandle, HOTKEY_ID);
            base.OnSessionEnding(e);
        }
    }
}
