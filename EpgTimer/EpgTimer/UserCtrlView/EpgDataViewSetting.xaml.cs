using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EpgTimer
{
    /// <summary>
    /// EpgDataViewSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgDataViewSetting : UserControl
    {
        private BoxExchangeEditor bx_service = new BoxExchangeEditor();
        private BoxExchangeEditor bx_genre = new BoxExchangeEditor();
        private EpgSearchKeyInfo searchKey = new EpgSearchKeyInfo();
        private int tabInfoID = -1;

        public EpgDataViewSetting()
        {
            InitializeComponent();

            try
            {
                comboBox_timeH_week.ItemsSource = CommonManager.Instance.HourDictionary.Values;
                comboBox_timeH_week.SelectedIndex = 4;

                foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
                {
                    if (info.IsDttv == true)
                    {
                        listBox_serviceDttv.Items.Add(info);
                    }
                    else if (info.IsBS == true)
                    {
                        listBox_serviceBS.Items.Add(info);
                    }
                    else if (info.IsCS == true)
                    {
                        listBox_serviceCS.Items.Add(info);
                    }
                    else
                    {
                        listBox_serviceOther.Items.Add(info);
                    }
                    listBox_serviceAll.Items.Add(info);
                }
                listBox_jyanru.DataContext = CommonManager.Instance.ContentKindList;

                radioButton_rate.IsChecked = true;
                radioButton_week.IsChecked = false;
                radioButton_list.IsChecked = false;

                bx_service.SourceBox = listBox_serviceDttv;
                bx_service.TargetBox = listBox_serviceView;
                button_service_addAll.Click += new RoutedEventHandler(bx_service.button_addAll_Click);
                button_service_add.Click += new RoutedEventHandler(bx_service.button_add_Click);
                button_service_del.Click += new RoutedEventHandler(bx_service.button_del_Click);
                button_service_delAll.Click += new RoutedEventHandler(bx_service.button_delAll_Click);
                button_service_up.Click += new RoutedEventHandler(bx_service.button_up_Click);
                button_service_down.Click += new RoutedEventHandler(bx_service.button_down_Click);
                button_service_top.Click += new RoutedEventHandler(bx_service.button_top_Click);
                button_service_bottom.Click += new RoutedEventHandler(bx_service.button_bottom_Click);
                tabControl2.SelectionChanged += (sender, e) => 
                {
                    try { bx_service.SourceBox = ((sender as TabControl).SelectedItem as TabItem).Content as ListBox; }
                    catch { }
                };
                listBox_serviceDttv.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_service.mouse_DoubleClick);
                listBox_serviceBS.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_service.mouse_DoubleClick);
                listBox_serviceCS.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_service.mouse_DoubleClick);
                listBox_serviceOther.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_service.mouse_DoubleClick);
                listBox_serviceAll.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_service.mouse_DoubleClick);
                listBox_serviceView.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_service.mouse_DoubleClick);

                bx_genre.SourceBox = listBox_jyanru;
                bx_genre.TargetBox = listBox_jyanruView;
                button_jyanru_addAll.Click += new RoutedEventHandler(bx_genre.button_addAll_Click);
                button_jyanru_add.Click += new RoutedEventHandler(bx_genre.button_add_Click);
                button_jyanru_del.Click += new RoutedEventHandler(bx_genre.button_del_Click);
                button_jyanru_delAll.Click += new RoutedEventHandler(bx_genre.button_delAll_Click);
                listBox_jyanru.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_genre.mouse_DoubleClick);
                listBox_jyanruView.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(bx_genre.mouse_DoubleClick);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }

        }

        /// <summary>
        /// デフォルト表示の設定値
        /// </summary>
        /// <param name="setInfo"></param>
        public void SetSetting(CustomEpgTabInfo setInfo)
        {
            tabInfoID = setInfo.ID;
            searchKey = setInfo.SearchKey.Clone();

            textBox_tabName.Text = setInfo.TabName;
            radioButton_rate.IsChecked = false;
            radioButton_week.IsChecked = false;
            radioButton_list.IsChecked = false;
            switch (setInfo.ViewMode)
            {
                case 1:
                    radioButton_week.IsChecked = true;
                    break;
                case 2:
                    radioButton_list.IsChecked = true;
                    break;
                default:
                    radioButton_rate.IsChecked = true;
                    break;
            }

            checkBox_noTimeView_rate.IsChecked = setInfo.NeedTimeOnlyBasic;
            checkBox_noTimeView_week.IsChecked = setInfo.NeedTimeOnlyWeek;
            comboBox_timeH_week.SelectedIndex = setInfo.StartTimeWeek;
            checkBox_searchMode.IsChecked = setInfo.SearchMode;
            checkBox_filterEnded.IsChecked = (setInfo.FilterEnded == true);

            foreach (UInt64 id in setInfo.ViewServiceList)
            {
                if (ChSet5.Instance.ChList.ContainsKey(id) == true)
                {
                    listBox_serviceView.Items.Add(ChSet5.Instance.ChList[id]);
                }
            }
            foreach (UInt16 id in setInfo.ViewContentKindList)
            {
                if (CommonManager.Instance.ContentKindDictionary.ContainsKey(id) == true)
                {
                    listBox_jyanruView.Items.Add(CommonManager.Instance.ContentKindDictionary[id]);
                }
            }
        }

        /// <summary>
        /// 設定値の取得
        /// </summary>
        /// <param name="setInfo"></param>
        public void GetSetting(ref CustomEpgTabInfo info)
        {
            info.TabName = textBox_tabName.Text;
            info.ViewMode = 0;
            if (radioButton_week.IsChecked == true)
            {
                info.ViewMode = 1;
            }
            else if (radioButton_list.IsChecked == true)
            {
                info.ViewMode = 2;
            }

            info.NeedTimeOnlyBasic = (checkBox_noTimeView_rate.IsChecked == true);
            info.NeedTimeOnlyWeek = (checkBox_noTimeView_week.IsChecked == true);
            info.StartTimeWeek = comboBox_timeH_week.SelectedIndex;
            info.SearchMode = (checkBox_searchMode.IsChecked == true);
            info.FilterEnded = (checkBox_filterEnded.IsChecked == true);
            info.SearchKey = searchKey.Clone();
            info.ID = tabInfoID;
 
            info.ViewServiceList.Clear();
            foreach (ChSet5Item item in listBox_serviceView.Items)
            {
                info.ViewServiceList.Add(item.Key);
            }

            info.ViewContentKindList.Clear();
            foreach (ContentKindInfo item in listBox_jyanruView.Items)
            {
                info.ViewContentKindList.Add(item.ID);
            }
        }

        /// <summary>映像のみ全追加</summary>
        private void button_service_addVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListBox listBox = bx_service.SourceBox;
                if (listBox == null) return;

                listBox.UnselectAll();
                foreach (ChSet5Item info in listBox.Items)
                {
                    if (info.IsVideo == true)
                    {
                        listBox.SelectedItems.Add(info);
                    }
                }
                bx_service.addItems(listBox, listBox_serviceView);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void listBox_serviceView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Display_ServiceView(listBox_serviceView, textBox_serviceView1);
        }

        private void listBox_service_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = bx_service.SourceBox;
            if (listBox == null) return;

            Display_ServiceView(listBox, textBox_serviceView2);
        }

        private void Display_ServiceView(ListBox srclistBox, TextBox targetBox)
        {
            try
            {
                targetBox.Text = "";
                if (srclistBox.SelectedItem == null) return;

                ChSet5Item info = srclistBox.SelectedItems[srclistBox.SelectedItems.Count - 1] as ChSet5Item;
                targetBox.Text = 
                    info.ServiceName + "\r\n" +
                    info.NetworkName + "\r\n" +
                    "OriginalNetworkID : " + info.ONID.ToString() + " (0x" + info.ONID.ToString("X4") + ")\r\n" +
                    "TransportStreamID : " + info.TSID.ToString() + " (0x" + info.TSID.ToString("X4") + ")\r\n" +
                    "ServiceID : " + info.SID.ToString() + " (0x" + info.SID.ToString("X4") + ")\r\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_searchKey_Click(object sender, RoutedEventArgs e)
        {
            SetDefSearchSettingWindow dlg = new SetDefSearchSettingWindow();
            PresentationSource topWindow = PresentationSource.FromVisual(this);
            if (topWindow != null)
            {
                dlg.Owner = (Window)topWindow.RootVisual;
            }
            dlg.SetDefSetting(searchKey);
            if (dlg.ShowDialog() == true)
            {
                dlg.GetSetting(ref searchKey);
            }
        }
    }
}
