﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EpgTimer.Setting
{
    //BonDriver一覧の表示・設定用クラス
    public class TunerInfo
    {
        public TunerInfo(string bon) { BonDriver = bon; }
        public String BonDriver { get; set; }
        public String TunerNum { get; set; }
        public String EPGNum { get; set; }
        public bool IsEpgCap { get; set; }
        public override string ToString() { return BonDriver; }
    }

    /// <summary>
    /// SetBasicView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetBasicView : UserControl
    {
        private ObservableCollection<EpgCaptime> timeList;
        private List<ServiceViewItem> serviceList;

        public SetBasicView()
        {
            InitializeComponent();

            listBox_Button_Set();

            try
            {
                // tabItem1 - 保存フォルダ
                // 保存できない項目は IsEnabled = false にする
                if (CommonManager.Instance.NWMode == true)
                {
                    label1.IsEnabled = false; // 設定関係保存フォルダ
                    textBox_setPath.IsEnabled = false;
                    button_setPath.IsEnabled = false; // 開く
                    label2.IsEnabled = false; // 録画用アプリのexe
                    textBox_exe.IsEnabled = false;
                    button_exe.IsEnabled = false; // 開く
                    label3.IsEnabled = false; //録画保存フォルダ
                    label4.IsEnabled = false; //※ 録画中やEPG取得中に設定を変更すると正常動作しなくなる可能性があります。
                    label5.IsEnabled = false; //スタートアップにショートカットを作成する
                    button_shortCut.IsEnabled = false;
                    button_shortCutSrv.IsEnabled = false;
                }
                listBox_recFolder.IsEnabled = IniFileHandler.CanUpdateInifile;
                button_rec_up.IsEnabled = IniFileHandler.CanUpdateInifile; // ↑
                button_rec_down.IsEnabled = IniFileHandler.CanUpdateInifile; // ↓
                button_rec_del.IsEnabled = IniFileHandler.CanUpdateInifile; // 削除
                textBox_recFolder.IsEnabled = IniFileHandler.CanUpdateInifile;
                button_rec_open.IsEnabled = IniFileHandler.CanUpdateInifile; // 開く
                button_rec_add.IsEnabled = IniFileHandler.CanUpdateInifile; // 追加

                // 読める設定のみ項目に反映させる
                if (IniFileHandler.CanReadInifile)
                {
                    textBox_setPath.Text = SettingPath.SettingFolderPath;
                    textBox_exe.Text = SettingPath.EdcbExePath;

                    textBox_recInfoFolder.Text = IniFileHandler.GetPrivateProfileString("SET", "RecInfoFolder", "", SettingPath.CommonIniPath);

                    Settings.Instance.DefRecFolders.ForEach(folder => listBox_recFolder.Items.Add(new UserCtrlView.BGBarListBoxItem(folder)));
                }

                // tabItem2 - チューナー
                // 保存できない項目は IsEnabled = false にする
                if (IniFileHandler.CanUpdateInifile == false)
                {
                    CommonManager.Instance.VUtil.DisableControlChildren(tabItem2);
                    grid_tuner.IsEnabled = true;
                    CommonManager.Instance.VUtil.ChangeChildren(grid_tuner, false);
                }
                listBox_bon.IsEnabled = IniFileHandler.CanUpdateInifile;

                // 読める設定のみ項目に反映させる
                if (IniFileHandler.CanReadInifile)
                {
                    SortedList<Int32, TunerInfo> tunerInfo = new SortedList<Int32, TunerInfo>();
                    foreach (string fileName in CommonManager.Instance.GetBonFileList())
                    {
                        try
                        {
                            TunerInfo item = new TunerInfo(fileName);
                            item.TunerNum = IniFileHandler.GetPrivateProfileInt(item.BonDriver, "Count", 0, SettingPath.TimerSrvIniPath).ToString();
                            item.IsEpgCap = (IniFileHandler.GetPrivateProfileInt(item.BonDriver, "GetEpg", 1, SettingPath.TimerSrvIniPath) != 0);
                            item.EPGNum = IniFileHandler.GetPrivateProfileInt(item.BonDriver, "EPGCount", 0, SettingPath.TimerSrvIniPath).ToString();
                            int priority = IniFileHandler.GetPrivateProfileInt(item.BonDriver, "Priority", 0xFFFF, SettingPath.TimerSrvIniPath);
                            while (true)
                            {
                                if (tunerInfo.ContainsKey(priority) == true)
                                {
                                    priority++;
                                }
                                else
                                {
                                    tunerInfo.Add(priority, item);
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }
                    foreach (TunerInfo info in tunerInfo.Values)
                    {
                        listBox_bon.Items.Add(info);
                    }
                    if (listBox_bon.Items.Count > 0)
                    {
                        listBox_bon.SelectedIndex = 0;
                    }
                }
                button_shortCut.Content = (string)button_shortCut.Content + (File.Exists(
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), SettingPath.ModuleName + ".lnk")) ? "を解除" : "");
                button_shortCutSrv.Content = (string)button_shortCutSrv.Content + (File.Exists(
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "EpgTimerSrv.lnk")) ? "を解除" : "");

                // tabItem3 - EPG取得
                // 保存できない項目は IsEnabled = false にする
                if (IniFileHandler.CanUpdateInifile == false)
                {
                    CommonManager.Instance.VUtil.DisableControlChildren(tabItem3);
                }
                listView_service.IsEnabled = IniFileHandler.CanUpdateInifile;

                // 読める設定のみ項目に反映させる
                serviceList = new List<ServiceViewItem>();
                if (IniFileHandler.CanReadInifile)
                {
                    comboBox_HH.DataContext = CommonManager.Instance.HourDictionary.Values;
                    comboBox_HH.SelectedIndex = 0;
                    comboBox_MM.DataContext = CommonManager.Instance.MinDictionary.Values;
                    comboBox_MM.SelectedIndex = 0;

                    try
                    {
                        foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
                        {
                            ServiceViewItem item = new ServiceViewItem(info);
                            if (info.EpgCapFlag == 1)
                            {
                                item.IsSelected = true;
                            }
                            else
                            {
                                item.IsSelected = false;
                            }
                            serviceList.Add(item);
                        }
                    }
                    catch
                    {
                    }
                    listView_service.DataContext = serviceList;

                    if (IniFileHandler.GetPrivateProfileInt("SET", "BSBasicOnly", 1, SettingPath.CommonIniPath) == 1)
                    {
                        checkBox_bs.IsChecked = true;
                    }
                    else
                    {
                        checkBox_bs.IsChecked = false;
                    }
                    if (IniFileHandler.GetPrivateProfileInt("SET", "CS1BasicOnly", 1, SettingPath.CommonIniPath) == 1)
                    {
                        checkBox_cs1.IsChecked = true;
                    }
                    else
                    {
                        checkBox_cs1.IsChecked = false;
                    }
                    if (IniFileHandler.GetPrivateProfileInt("SET", "CS2BasicOnly", 1, SettingPath.CommonIniPath) == 1)
                    {
                        checkBox_cs2.IsChecked = true;
                    }
                    else
                    {
                        checkBox_cs2.IsChecked = false;
                    }

                    timeList = new ObservableCollection<EpgCaptime>();
                    int capCount = IniFileHandler.GetPrivateProfileInt("EPG_CAP", "Count", 0, SettingPath.TimerSrvIniPath);
                    if (capCount == 0)
                    {
                        EpgCaptime item = new EpgCaptime();
                        item.IsSelected = true;
                        item.Time = "23:00";
                        item.BSBasicOnly = checkBox_bs.IsChecked == true;
                        item.CS1BasicOnly = checkBox_cs1.IsChecked == true;
                        item.CS2BasicOnly = checkBox_cs2.IsChecked == true;
                        timeList.Add(item);
                    }
                    else
                    {
                        for (int i = 0; i < capCount; i++)
                        {
                            EpgCaptime item = new EpgCaptime();
                            item.Time = IniFileHandler.GetPrivateProfileString("EPG_CAP", i.ToString(), "", SettingPath.TimerSrvIniPath);
                            if (IniFileHandler.GetPrivateProfileInt("EPG_CAP", i.ToString() + "Select", 0, SettingPath.TimerSrvIniPath) == 1)
                            {
                                item.IsSelected = true;
                            }
                            else
                            {
                                item.IsSelected = false;
                            }
                            // 取得種別(bit0(LSB)=BS,bit1=CS1,bit2=CS2)。負値のときは共通設定に従う
                            int flags = IniFileHandler.GetPrivateProfileInt("EPG_CAP", i.ToString() + "BasicOnlyFlags", -1, SettingPath.TimerSrvIniPath);
                            if (flags >= 0)
                            {
                                item.BSBasicOnly = (flags & 1) != 0;
                                item.CS1BasicOnly = (flags & 2) != 0;
                                item.CS2BasicOnly = (flags & 4) != 0;
                            }
                            else
                            {
                                item.BSBasicOnly = checkBox_bs.IsChecked == true;
                                item.CS1BasicOnly = checkBox_cs1.IsChecked == true;
                                item.CS2BasicOnly = checkBox_cs2.IsChecked == true;
                            }
                            timeList.Add(item);
                        }
                    }
                    ListView_time.DataContext = timeList;

                    textBox_ngCapMin.Text = IniFileHandler.GetPrivateProfileInt("SET", "NGEpgCapTime", 20, SettingPath.TimerSrvIniPath).ToString();
                    textBox_ngTunerMin.Text = IniFileHandler.GetPrivateProfileInt("SET", "NGEpgCapTunerTime", 20, SettingPath.TimerSrvIniPath).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void SaveSetting()
        {
            try
            {
                // tabItem1
                if (textBox_setPath.IsEnabled)
                {
                    System.IO.Directory.CreateDirectory(textBox_setPath.Text);

                    IniFileHandler.WritePrivateProfileString("SET", "DataSavePath",textBox_setPath.Text, SettingPath.CommonIniPath);
                }
                if (textBox_recInfoFolder.IsEnabled)
                {
                    IniFileHandler.WritePrivateProfileString("SET", "RecInfoFolder",
                                                             textBox_recInfoFolder.Text.Trim() == "" ? null : textBox_recInfoFolder.Text, SettingPath.CommonIniPath);
                }
                if (textBox_exe.IsEnabled)
                {
                    IniFileHandler.WritePrivateProfileString("SET", "RecExePath", textBox_exe.Text, SettingPath.CommonIniPath);
                }
                if (listBox_recFolder.IsEnabled)
                {
                    int recFolderCount = listBox_recFolder.Items.Count;
                    IniFileHandler.WritePrivateProfileString("SET", "RecFolderNum", recFolderCount.ToString(), SettingPath.CommonIniPath);
                    for (int i = 0; i < recFolderCount; i++)
                    {
                        string key = "RecFolderPath" + i.ToString();
                        string val = listBox_recFolder.Items[i].ToString();
                        IniFileHandler.WritePrivateProfileString("SET", key, val, SettingPath.CommonIniPath);
                    }
                }

                // tabItem2
                if (listBox_bon.IsEnabled)
                {
                    for (int i = 0; i < listBox_bon.Items.Count; i++)
                    {
                        TunerInfo info = listBox_bon.Items[i] as TunerInfo;

                        IniFileHandler.WritePrivateProfileString(info.BonDriver, "Count", info.TunerNum, SettingPath.TimerSrvIniPath);
                        if (info.IsEpgCap == true)
                        {
                            IniFileHandler.WritePrivateProfileString(info.BonDriver, "GetEpg", "1", SettingPath.TimerSrvIniPath);
                        }
                        else
                        {
                            IniFileHandler.WritePrivateProfileString(info.BonDriver, "GetEpg", "0", SettingPath.TimerSrvIniPath);
                        }
                        IniFileHandler.WritePrivateProfileString(info.BonDriver, "EPGCount", info.EPGNum, SettingPath.TimerSrvIniPath);
                        IniFileHandler.WritePrivateProfileString(info.BonDriver, "Priority", i.ToString(), SettingPath.TimerSrvIniPath);
                    }
                }

                // tabItem3
                if (checkBox_bs.IsEnabled)
                {
                    if (checkBox_bs.IsChecked == true)
                    {
                        IniFileHandler.WritePrivateProfileString("SET", "BSBasicOnly", "1", SettingPath.CommonIniPath);
                    }
                    else
                    {
                        IniFileHandler.WritePrivateProfileString("SET", "BSBasicOnly", "0", SettingPath.CommonIniPath);
                    }
                }
                if (checkBox_cs1.IsEnabled)
                {
                    if (checkBox_cs1.IsChecked == true)
                    {
                        IniFileHandler.WritePrivateProfileString("SET", "CS1BasicOnly", "1", SettingPath.CommonIniPath);
                    }
                    else
                    {
                        IniFileHandler.WritePrivateProfileString("SET", "CS1BasicOnly", "0", SettingPath.CommonIniPath);
                    }
                }
                if (checkBox_cs2.IsEnabled)
                {
                    if (checkBox_cs2.IsChecked == true)
                    {
                        IniFileHandler.WritePrivateProfileString("SET", "CS2BasicOnly", "1", SettingPath.CommonIniPath);
                    }
                    else
                    {
                        IniFileHandler.WritePrivateProfileString("SET", "CS2BasicOnly", "0", SettingPath.CommonIniPath);
                    }
                }

                foreach (ServiceViewItem info in serviceList)
                {
                    UInt64 key = info.ServiceInfo.Key;
                    try
                    {
                        if (info.IsSelected == true)
                        {
                            ChSet5.Instance.ChList[key].EpgCapFlag = 1;
                        }
                        else
                        {
                            ChSet5.Instance.ChList[key].EpgCapFlag = 0;
                        }
                    }
                    catch
                    {
                    }
                }

                if (ListView_time.IsEnabled)
                {
                    IniFileHandler.WritePrivateProfileString("EPG_CAP", "Count", timeList.Count.ToString(), SettingPath.TimerSrvIniPath);
                    for (int i = 0; i < timeList.Count; i++)
                    {
                        EpgCaptime item = timeList[i] as EpgCaptime;
                        IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString(), item.Time, SettingPath.TimerSrvIniPath);
                        if (item.IsSelected == true)
                        {
                            IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString() + "Select", "1", SettingPath.TimerSrvIniPath);
                        }
                        else
                        {
                            IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString() + "Select", "0", SettingPath.TimerSrvIniPath);
                        }
                        int flags = (item.BSBasicOnly ? 1 : 0) | (item.CS1BasicOnly ? 2 : 0) | (item.CS2BasicOnly ? 4 : 0);
                        IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString() + "BasicOnlyFlags", flags.ToString(), SettingPath.TimerSrvIniPath);
                    }
                }

                if (textBox_ngCapMin.IsEnabled)
                {
                    IniFileHandler.WritePrivateProfileString("SET", "NGEpgCapTime", textBox_ngCapMin.Text, SettingPath.TimerSrvIniPath);
                }
                if (textBox_ngTunerMin.IsEnabled)
                {
                    IniFileHandler.WritePrivateProfileString("SET", "NGEpgCapTunerTime", textBox_ngTunerMin.Text, SettingPath.TimerSrvIniPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_setPath_Click(object sender, RoutedEventArgs e)
        {
            string path = CommonManager.Instance.GetFolderNameByDialog(textBox_setPath.Text, "設定関係保存フォルダの選択");
            if (path != null)
            {
                textBox_setPath.Text = path;
            }
        }

        private void button_exe_Click(object sender, RoutedEventArgs e)
        {
            string path = CommonManager.Instance.GetFileNameByDialog(textBox_exe.Text, "", ".exe");
            if (path != null)
            {
                textBox_exe.Text = path;
            }
        }

        private void button_recInfoFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = CommonManager.Instance.GetFolderNameByDialog(textBox_recInfoFolder.Text, "録画情報保存フォルダの選択");
            if (path != null)
            {
                textBox_recInfoFolder.Text = path;
            }
        }


        //ボタン表示画面の上下ボタンのみ他と同じものを使用する。
        private BoxExchangeEditor bxr = new BoxExchangeEditor();
        private BoxExchangeEditor bxb = new BoxExchangeEditor();
        private void listBox_Button_Set()
        {
            //録画設定関係
            bxr.TargetBox = this.listBox_recFolder;
            button_rec_up.Click += new RoutedEventHandler(bxr.button_up_Click);
            button_rec_down.Click += new RoutedEventHandler(bxr.button_down_Click);

            //チューナ関係関係
            bxb.TargetBox = this.listBox_bon;
            button_bon_up.Click += new RoutedEventHandler(bxb.button_up_Click);
            button_bon_down.Click += new RoutedEventHandler(bxb.button_down_Click);
        }

        private void button_rec_del_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 誤って削除ボタンを押したときにすぐに追加できるようにするため
                // 該当アイテムを追加するパスの候補に戻しておく。
                if (listBox_recFolder.SelectedItem != null)
                {
                    textBox_recFolder.Text = listBox_recFolder.SelectedItem.ToString();
                    listBox_recFolder.Items.RemoveAt(listBox_recFolder.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_rec_open_Click(object sender, RoutedEventArgs e)
        {
            string path = CommonManager.Instance.GetFolderNameByDialog(textBox_recFolder.Text, "録画フォルダの選択");
            if (path != null)
            {
                textBox_recFolder.Text = path;
            }
        }

        private void button_rec_add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(textBox_recFolder.Text) == false)
                {
                    foreach (var info in listBox_recFolder.Items)
                    {
                        if (String.Compare(textBox_recFolder.Text, info.ToString(), true) == 0)
                        {
                            MessageBox.Show("すでに追加されています");
                            return;
                        }
                    }

                    // 追加対象のフォルダーの空き容量をサーバーに問い合わせてみる
                    // SendEnumRecFolders にフォルダー名を指定した場合、そのフォルダーの空き容量を返してくる
                    var folders = new List<RecFolderInfo>();
                    if (CommonManager.Instance.CtrlCmd.SendEnumRecFolders(textBox_recFolder.Text, ref folders) != ErrCode.CMD_SUCCESS)
                    {
                        if (CommonManager.Instance.NW.IsConnected == false)
                        {
                            if (System.IO.Directory.Exists(textBox_recFolder.Text))
                            {
                                //サーバーが問い合わせに対応していないようなので、フォルダー名だけ登録する
                                folders.Add(new RecFolderInfo(textBox_recFolder.Text));
                            }
                            else
                            {
                                MessageBox.Show("フォルダーが存在するか確認してください。");
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("EpgTimerNW ではフォルダーの追加は出来ません。");
                            return;
                        }
                    }
                    if (folders.Count == 1)
                    {
                        listBox_recFolder.Items.Add(new UserCtrlView.BGBarListBoxItem(folders[0]));
                    }
                    else
                    {
                        // SendEnumRecFolders でフォルダーの空き容量が取得できなかった場合
                        MessageBox.Show("サーバーからアクセスできるフォルダーか確認してください。");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_shortCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), SettingPath.ModuleName + ".lnk");
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
                else
                {
                    CreateShortCut(shortcutPath, Assembly.GetEntryAssembly().Location, "");
                }
                button_shortCut.Content = ((string)button_shortCut.Content).Replace("を解除", "") + (File.Exists(shortcutPath) ? "を解除" : "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_shortCutSrv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "EpgTimerSrv.lnk");
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
                else
                {
                    CreateShortCut(shortcutPath, System.IO.Path.Combine(SettingPath.ModulePath, "EpgTimerSrv.exe"), "");
                }
                button_shortCutSrv.Content = ((string)button_shortCutSrv.Content).Replace("を解除", "") + (File.Exists(shortcutPath) ? "を解除" : "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// ショートカットの作成
        /// </summary>
        /// <remarks>WSHを使用して、ショートカット(lnkファイル)を作成します。(遅延バインディング)</remarks>
        /// <param name="path">出力先のファイル名(*.lnk)</param>
        /// <param name="targetPath">対象のアセンブリ(*.exe)</param>
        /// <param name="description">説明</param>
        private void CreateShortCut(String path, String targetPath, String description)
        {
            //using System.Reflection;

            // WSHオブジェクトを作成し、CreateShortcutメソッドを実行する
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(shellType);
            object shortCut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { path });

            Type shortcutType = shell.GetType();
            // TargetPathプロパティをセットする
            shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortCut, new object[] { targetPath });
            shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortCut, new object[] { System.IO.Path.GetDirectoryName(targetPath) });
            // Descriptionプロパティをセットする
            shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortCut, new object[] { description });
            // Saveメソッドを実行する
            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortCut, null);
        }

        private void listBox_bon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (listBox_bon.SelectedItem != null)
                {
                    TunerInfo info = listBox_bon.SelectedItem as TunerInfo;
                    textBox_bon_num.DataContext = info;
                    checkBox_bon_epg.DataContext = info;
                    textBox_bon_epgnum.DataContext = info;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_allChk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ServiceViewItem info in this.serviceList)
                {
                    info.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_videoChk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ServiceViewItem info in this.serviceList)
                {
                    info.IsSelected = (ChSet5.IsVideo(info.ServiceInfo.ServiceType) == true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_allClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ServiceViewItem info in this.serviceList)
                {
                    info.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_addTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBox_HH.SelectedItem != null && comboBox_MM.SelectedItem != null)
                {
                    UInt16 hh = (UInt16)comboBox_HH.SelectedItem;
                    UInt16 mm = (UInt16)comboBox_MM.SelectedItem;
                    String time = hh.ToString("D2") + ":" + mm.ToString("D2");
                    int wday = comboBox_wday.SelectedIndex;
                    if (1 <= wday && wday <= 7)
                    {
                        // 曜日指定接尾辞(w1=Mon,...,w7=Sun)
                        time += "w" + ((wday + 5) % 7 + 1);
                    }

                    foreach (EpgCaptime info in timeList)
                    {
                        if (String.Compare(info.Time, time, true) == 0)
                        {
                            MessageBox.Show("すでに登録済みです");
                            return;
                        }
                    }
                    EpgCaptime item = new EpgCaptime();
                    item.IsSelected = true;
                    item.Time = time;
                    item.BSBasicOnly = checkBox_bs.IsChecked == true;
                    item.CS1BasicOnly = checkBox_cs1.IsChecked == true;
                    item.CS2BasicOnly = checkBox_cs2.IsChecked == true;
                    timeList.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_delTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListView_time.SelectedItem != null)
                {
                    EpgCaptime item = ListView_time.SelectedItem as EpgCaptime;
                    timeList.Remove(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

    }
}
