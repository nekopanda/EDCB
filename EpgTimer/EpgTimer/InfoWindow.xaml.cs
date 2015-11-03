using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EpgTimer
{
    /// <summary>
    /// InfoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow(InfoWindowViewModel dataContext)
        {
            InitializeComponent();

            DataContext = dataContext;
        }
    }
}
