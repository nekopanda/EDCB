﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using EpgTimer.UserCtrlView;

namespace EpgTimer
{
    /// <summary>
    /// ReserveView.xaml の相互作用ロジック
    /// </summary>
    public partial class ReserveView : DataViewBase
    {
        private ListViewController<ReserveItem> lstCtrl;
        private CmdExeReserve mc; //予約系コマンド集

        public ReserveView()
        {
            InitializeComponent();

            try
            {
                //リストビュー関連の設定
                var list_columns = Resources["ReserveItemViewColumns"] as GridViewColumnList;
                list_columns.AddRange(Resources["RecSettingViewColumns"] as GridViewColumnList);

                lstCtrl = new ListViewController<ReserveItem>(this);
                lstCtrl.SetSavePath(CommonUtil.GetMemberName(() => Settings.Instance.ReserveListColumn)
                    , CommonUtil.GetMemberName(() => Settings.Instance.ResColumnHead)
                    , CommonUtil.GetMemberName(() => Settings.Instance.ResSortDirection));
                lstCtrl.SetViewSetting(listView_reserve, gridView_reserve, true, true, list_columns);
                lstCtrl.SetSelectedItemDoubleClick(EpgCmds.ShowDialog);

                //最初にコマンド集の初期化
                mc = new CmdExeReserve(this);
                mc.SetFuncGetDataList(isAll => (isAll == true ? lstCtrl.dataList : lstCtrl.GetSelectedItemsList()).GetReserveList());
                mc.SetFuncSelectSingleData((noChange) =>
                {
                    var item = lstCtrl.SelectSingleItem(noChange);
                    return item == null ? null : item.ReserveInfo;
                });
                mc.SetFuncReleaseSelectedData(() => listView_reserve.UnselectAll());

                //コマンド集に無いもの
                mc.AddReplaceCommand(EpgCmds.ChgOnOffCheck, (sender, e) => lstCtrl.ChgOnOffFromCheckbox(e.Parameter, EpgCmds.ChgOnOff));

                //コマンド集からコマンドを登録。多少冗長だが、持っているコマンドは全部登録してしまう。
                //フォーカスによってコンテキストメニューからウィンドウにコマンドが繋がらない場合があるので、
                //コンテキストメニューにもコマンドを登録する。
                mc.ResetCommandBindings(this, listView_reserve.ContextMenu);

                //ボタンの設定。XML側でコマンド指定しておけば、ループでまとめ処理できるけど、
                //インテリセンス効かないし(一応エラーチェックは入る)、コード側に一覧として書き出す。
                mBinds.View = CtxmCode.ReserveView;
                mBinds.SetCommandToButton(button_on_off, EpgCmds.ChgOnOff);
                mBinds.SetCommandToButton(button_change, EpgCmds.ShowDialog);
                mBinds.SetCommandToButton(button_del, EpgCmds.Delete);
                mBinds.SetCommandToButton(button_add_manual, EpgCmds.ShowAddDialog);
                mBinds.SetCommandToButton(button_timeShiftPlay, EpgCmds.Play);

                //メニューの作成、ショートカットの登録
                RefreshMenu();

                //コンテキストメニューを開く時の設定
                listView_reserve.ContextMenu.Opened += new RoutedEventHandler(mc.SupportContextMenuLoading);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        public void RefreshMenu()
        {
            mm.CtxmGenerateContextMenu(listView_reserve.ContextMenu, CtxmCode.ReserveView, true);
            mBinds.ResetInputBindings(this, listView_reserve);
            //mBinds.SetCommandBindings(mc.CommandBindings(), this, listView_reserve.ContextMenu);//やめ。あるだけ全部最初に登録することにする。
        }
        protected override bool ReloadInfoData()
        {
            return lstCtrl.ReloadInfoData(dataList =>
            {
                if (vutil.ReloadReserveData(this) == false) return false;

                foreach (ReserveData info in CommonManager.Instance.DB.ReserveList.Values)
                {
                    dataList.Add(new ReserveItem(info));
                }
                return true;
            });
        }
        public void SaveViewData()
        {
            lstCtrl.SaveViewDataToSettings();
        }

        protected override void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            base.UserControl_IsVisibleChanged(sender, e);

            if (this.IsVisible == false) return;

            if (BlackoutWindow.HasReserveData == true)
            {
                MoveToReserveItem(BlackoutWindow.SelectedItem.ReserveInfo, BlackoutWindow.NowJumpTable);
            }

            BlackoutWindow.Clear();
        }

        protected void MoveToReserveItem(ReserveData target, bool IsMarking)
        {
            uint ID = target.ReserveID;
            ReserveItem item = lstCtrl.dataList.Find(data => data.ReserveInfo.ReserveID == ID);
            vutil.ScrollToFindItem(item, listView_reserve, IsMarking);
        }

    }
}
