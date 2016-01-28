using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EpgTimer
{
    /// <summary>
    /// InfoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InfoWindow : RestorableWindow
    {
        private NotifyIcon notifyIcon = new NotifyIcon();
        private bool trueClosing = false;

        public InfoWindow(InfoWindowViewModel dataContext)
        {
            InitializeComponent();

            notifyIcon.Icon = Properties.Resources.TaskIconInfo;
            notifyIcon.Text = "予約簡易表示 (EpgTimer)";
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.Visible = true;

            var menu = new System.Windows.Forms.ContextMenuStrip();
            ToolStripMenuItem closeMenu = new ToolStripMenuItem();
            closeMenu.Text = "消す";
            closeMenu.Click += (s, e) => TrueClose();
            menu.Items.Add(closeMenu);
            notifyIcon.ContextMenuStrip = menu;

            DataContext = dataContext;
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if((e.Button & MouseButtons.Left) != 0)
            {
                WindowActivate();
            }
        }

        public void TrueClose()
        {
            trueClosing = true;
            Close();
        }

        public void WindowActivate()
        {
            if (Visibility != Visibility.Visible)
            {
                Visibility = Visibility.Visible;
            }
            Activate();
        }

#if false
        // 最小化できないようにする //
        private const int GWL_STYLE = -16,
                      WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MINIMIZEBOX));
        }
#endif

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!trueClosing)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            TrueClose();
        }
    }
}
