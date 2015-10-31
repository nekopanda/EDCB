using System;
using System.Collections.Generic;
using System.Linq;
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
    /// EpgDataViewSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgDataViewSettingWindow : Window
    {
        public EpgDataViewSettingWindow()
        {
            InitializeComponent();

            if (Settings.Instance.NoStyle == 0)
            {
                ResourceDictionary rd = new ResourceDictionary();
                rd.MergedDictionaries.Add(
                    Application.LoadComponent(new Uri("/PresentationFramework.Aero, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35;component/themes/aero.normalcolor.xaml", UriKind.Relative)) as ResourceDictionary
                    //Application.LoadComponent(new Uri("/PresentationFramework.Classic, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/Classic.xaml", UriKind.Relative)) as ResourceDictionary
                    );
                this.Resources = rd;
            }
            else
            {
                button_OK.Style = null;
                button_cancel.Style = null;
            }
            checkBox_save_settings.IsChecked = false;
        }

        /// <summary>
        /// デフォルト表示の設定値
        /// </summary>
        /// <param name="setInfo"></param>
        public void SetDefSetting(CustomEpgTabInfo setInfo, bool show)
        {
            epgDataViewSetting.SetSetting(setInfo);

            if (show)
            {
                checkBox_save_settings.Visibility = Visibility.Visible;
                checkBox_save_settings.IsChecked = (Settings.Instance.AlwaysSaveEpgSetting == true);
            }
            else
            {
                checkBox_save_settings.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 設定値の取得
        /// </summary>
        /// <param name="setInfo"></param>
        public void GetSetting(ref CustomEpgTabInfo info)
        {
            epgDataViewSetting.GetSetting(ref info);

            if (checkBox_save_settings.Visibility == Visibility.Visible)
            {
                if (checkBox_save_settings.IsChecked == true)
                {
                    int index = Settings.Instance.CustomEpgTabList.IndexOf(info);
                    if (index >= 0)
                    {
                        Settings.Instance.CustomEpgTabList[index] = info;
                    }
                    Settings.Instance.AlwaysSaveEpgSetting = true;
                }
                else
                {
                    Settings.Instance.AlwaysSaveEpgSetting = false;
                }

            }
        }


        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
