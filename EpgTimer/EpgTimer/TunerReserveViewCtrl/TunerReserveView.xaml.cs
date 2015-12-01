using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EpgTimer.TunerReserveViewCtrl
{
    /// <summary>
    /// TunerReserveView.xaml の相互作用ロジック
    /// </summary>
    public partial class TunerReserveView : EpgTimer.UserCtrlView.PanelViewBase
    {
        protected override bool IsSingleClickOpen { get { return Settings.Instance.TunerInfoSingleClick; } }
        protected override double DragScroll { get { return Settings.Instance.TunerDragScroll; } }
        protected override bool IsMouseScrollAuto { get { return Settings.Instance.TunerMouseScrollAuto; } }
        protected override double ScrollSize { get { return Settings.Instance.TunerScrollSize; } }
        protected override bool IsPopupEnabled { get { return Settings.Instance.TunerPopup; } }
        protected override FrameworkElement PopUp { get { return popupItem; } }

        public TunerReserveView()
        {
            InitializeComponent();

            base.scroll = scrollViewer;
            base.cnvs = canvas;
        }

        public void SetReserveList(List<ReserveViewItem> reserveList, double width, double height)
        {
            try
            {
                canvas.Height = Math.Ceiling(height + 1);//右端のチューナ列の線を描画するため+1。他の+1も同じ。
                canvas.Width = Math.Ceiling(width + 1);
                reserveViewPanel.ItemFontNormal = CommonManager.Instance.VUtil.ItemFontTunerNormal.PrepareCache();
                reserveViewPanel.ItemFontTitle = CommonManager.Instance.VUtil.ItemFontTunerService.PrepareCache();
                reserveViewPanel.Height = canvas.Height;
                reserveViewPanel.Width = canvas.Width;
                reserveViewPanel.Items = reserveList;
                reserveViewPanel.InvalidateVisual();

                PopUpWork(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        protected override object GetPopupItem(Point cursorPos)
        {
            if (reserveViewPanel.Items == null) return null;

            return reserveViewPanel.Items.Find(pg => pg.IsPicked(cursorPos));
        }

        protected override void SetPopup(object item)
        {
            var viewInfo = (ReserveViewItem)item;
            var resItem = new ReserveItem(viewInfo.ReserveInfo);
            
            popupItem.Background = viewInfo.BackColorTuner;

            Canvas.SetLeft(popupItem, Math.Floor(viewInfo.LeftPos));
            Canvas.SetTop(popupItem, Math.Floor(viewInfo.TopPos));
            popupItem.Width = Math.Ceiling(viewInfo.Width + 1);
            popupItem.MinHeight = Math.Ceiling(viewInfo.Height + 1);
            //popupItemTextArea.Margin = new Thickness(2, -1, 0, 0);
            popupItemTextArea.Margin = new Thickness(1, -1, 5, 1);

            double sizeTitle = Settings.Instance.TunerFontSizeService;
            double sizeNormal = Settings.Instance.TunerFontSize;
            double indentTitle = Math.Floor((Settings.Instance.TunerPopupRecinfo == false || Settings.Instance.TunerTitleIndent == true) ? sizeNormal * 1.7 : 2);
            double indentNormal = Math.Floor(Settings.Instance.TunerTitleIndent == true ? indentTitle : 2);
            var fontTitle = new FontFamily(Settings.Instance.TunerFontNameService);
            var fontNormal = new FontFamily(Settings.Instance.TunerFontName);
            FontWeight weightTitle = Settings.Instance.TunerFontBoldService == true ? FontWeights.Bold : FontWeights.Normal;
            SolidColorBrush colorTitle = Settings.Instance.TunerColorModeUse == true ? viewInfo.ForeColorPriTuner : CommonManager.Instance.CustTunerServiceColor;
            SolidColorBrush colorNormal = CommonManager.Instance.CustTunerTextColor;

            //録画中は枠をかえる
            popupItem.BorderBrush = viewInfo.BorderBrushTuner;

            //追加情報の表示
            if (Settings.Instance.TunerPopupRecinfo == true)
            {
                timeText.Visibility = Visibility.Visible;
                recInfoText.Visibility = Visibility.Visible;
                minText.Visibility = Visibility.Collapsed;

                //'録画中'を表示
                statusText.Visibility = Visibility.Collapsed;
                if (statusText.Text != "")
                {
                    statusText.Visibility = Visibility.Visible;
                    statusText.Text = viewInfo.StatusTuner;
                    statusText.FontFamily = fontNormal;
                    statusText.FontSize = sizeNormal;
                    //statusText.FontWeight = FontWeights.Normal;
                    statusText.Foreground = CommonManager.Instance.StatRecForeColor;
                    minText.Margin = new Thickness(1, 1, 0, 4);
                    statusText.LineHeight = Math.Max(Settings.Instance.TunerFontHeight, sizeNormal + 2);
                }

                timeText.Text = resItem.StartTimeShort;
                timeText.FontFamily = fontNormal;
                timeText.FontSize = sizeNormal;
                timeText.Foreground = colorTitle;
                timeText.Margin = new Thickness(1, 1, 0, 2);
                timeText.LineHeight = Math.Max(Settings.Instance.TunerFontHeight, sizeNormal + 2);

                recInfoText.Text = "優先度 : " + resItem.Priority + "\r\n" + "録画モード : " + resItem.RecMode;
                recInfoText.FontFamily = fontNormal;
                recInfoText.FontSize = sizeNormal;
                //recInfoText.FontWeight = FontWeights.Normal;
                recInfoText.Foreground = colorTitle;
                recInfoText.Margin = new Thickness(indentTitle, 0, 2, 4);
                recInfoText.LineHeight = Math.Max(Settings.Instance.TunerFontHeight, sizeNormal + 2);
            }
            else
            {
                statusText.Visibility = Visibility.Collapsed;
                timeText.Visibility = Visibility.Collapsed;
                recInfoText.Visibility = Visibility.Collapsed;
                minText.Visibility = Visibility.Visible;

                minText.Text = viewInfo.ReserveInfo.StartTime.Minute.ToString("d02");
                minText.FontFamily = fontNormal;
                minText.FontSize = sizeNormal;
                minText.VerticalAlignment = VerticalAlignment.Center;
                //minText.FontWeight = FontWeights.Normal;
                minText.Foreground = colorTitle;
                //minText.Margin = new Thickness(0, 0, 0, 0);
                minText.LineHeight = Settings.Instance.TunerFontHeight;
            }

            var titletext = resItem.ServiceName + "(" + resItem.NetworkName + ")";
            titleText.Text = Regex.Replace(titletext, ".", "$0\u200b");
            titleText.FontFamily = fontTitle;
            titleText.FontSize = sizeTitle;
            titleText.FontWeight = weightTitle;
            titleText.Foreground = colorTitle;
            titleText.VerticalAlignment = VerticalAlignment.Center;
            titleText.Margin = new Thickness(indentTitle, 0, 2, 3);
            titleText.LineHeight = Math.Max(Settings.Instance.TunerFontHeightService, sizeTitle + 2);

            //必ず文字単位で折り返すためにZWSPを挿入 (\\w を使うと記号の間にZWSPが入らない)
            infoText.Text = Regex.Replace(resItem.EventName, ".", "$0\u200b"); 
            //infoText.Text = resItem.EventName;
            infoText.FontFamily = fontNormal;
            infoText.FontSize = sizeNormal;
            //infoText.FontWeight = FontWeights.Normal;
            infoText.Foreground = colorNormal;
            infoText.Margin = new Thickness(indentNormal, 0, 2, 3);
            infoText.LineHeight = Math.Max(Settings.Instance.TunerFontHeight, sizeNormal + 2);
        }

    }

}
