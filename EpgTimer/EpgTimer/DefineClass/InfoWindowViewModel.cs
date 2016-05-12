using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace EpgTimer
{
    public class InfoWindowViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _RefreshTimer;
        private DispatcherTimer RefreshTimer
        {
            get
            {
                if (_RefreshTimer == null)
                {
                    _RefreshTimer = new DispatcherTimer(DispatcherPriority.Background);
                }

                return _RefreshTimer;
            }
        }

        #region INotifyPropertyChanged実装

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }

        #endregion

        #region ReserveList 変更通知プロパティ

        private List<ReserveItemLive> _ReserveList = null;

        public List<ReserveItemLive> ReserveList
        {
            get { return this._ReserveList; }
            set
            {
                if (this._ReserveList != value)
                {
                    this._ReserveList = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region IsTopMost 変更通知プロパティ

        private bool _IsTopMost = Settings.Instance.InfoWindowTopMost;

        public bool IsTopMost
        {
            get { return this._IsTopMost; }
            set
            {
                if (this._IsTopMost != value)
                {
                    this._IsTopMost = value;
                    if (this._IsTopMost)
                    {
                        this.IsBottomMost = false;
                    }
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region IsBottomMost 変更通知プロパティ

        private bool _IsBottomMost = Settings.Instance.InfoWindowBottomMost;

        public bool IsBottomMost
        {
            get { return this._IsBottomMost; }
            set
            {
                if (this._IsBottomMost != value)
                {
                    this._IsBottomMost = value;
                    if (this._IsBottomMost)
                    {
                        this.IsTopMost = false;
                    }
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region IsDisabledReserveItemVisible プロパティ

        private bool _IsDisabledReserveItemVisible = Settings.Instance.InfoWindowDisabledReserveItemVisible;

        public bool IsDisabledReserveItemVisible
        {
            get { return this._IsDisabledReserveItemVisible; }
            set
            {
                if (this._IsDisabledReserveItemVisible != value)
                {
                    this._IsDisabledReserveItemVisible = value;
                    this.UpdateInfo();
                }
            }
        }

        #endregion

        #region 予約一覧自動更新プロパティ

        private bool _IsAutoRefreshEnabled = false;
        public bool IsAutoRefreshEnabled
        {
            get { return _IsAutoRefreshEnabled; }
            set
            {
                if (_IsAutoRefreshEnabled == value) return;

                _IsAutoRefreshEnabled = value;

                if (value)
                {
                    RefreshTimer.Interval = GetNextTimerInterval();
                    RefreshTimer.Tick += TimerHandler;
                    RefreshTimer.IsEnabled = true;
                }
                else
                {
                    RefreshTimer.IsEnabled = false;
                    RefreshTimer.Tick -= TimerHandler;
                }
            }
        }

        #endregion

        private TimeSpan GetNextTimerInterval()
        {
            int interval = Settings.Instance.InfoWindowRefreshInterval;
            int secondsToNextUpdate = 0;
            if (Settings.Instance.InfoWindowItemProgressBarType == 0)
            {
                // ProgressBar を動かす必要がないので、 次にステートが変わるまでの最小時間を探す
                long nowTicks = DateTime.Now.Ticks;
                long level1Ticks = nowTicks + Settings.Instance.InfoWindowItemLevel1Seconds * 10000000L;
                long level2Ticks = Settings.Instance.InfoWindowItemFilterLevel > 1 ? nowTicks + Settings.Instance.InfoWindowItemLevel2Seconds * 10000000L : long.MaxValue;
                long level3Ticks = Settings.Instance.InfoWindowItemFilterLevel > 2 ? nowTicks + Settings.Instance.InfoWindowItemLevel3Seconds * 10000000L : long.MaxValue;
                long nextTicks = ReserveList.Select(x => x.OnAirOrRecStart < level1Ticks ? -1L :
                                                         x.OnAirOrRecStart < level2Ticks ? x.OnAirOrRecStart - level1Ticks :
                                                         x.OnAirOrRecStart < level3Ticks ? x.OnAirOrRecStart - level2Ticks : x.OnAirOrRecStart - level3Ticks)
                                            .Where(x => x > 0L)
                                            .OrderBy(x => x)
                                            .FirstOrDefault();

                // Timer の誤差を考慮して、1更新間隔分前に一度更新されるようにしておく
                nextTicks -= interval * 10000000L;
                secondsToNextUpdate = nextTicks < 10000000L ? 0 : (int)Math.Ceiling(nextTicks / 10000000D);
            }
            if (secondsToNextUpdate == 0)
            {
                secondsToNextUpdate = interval - DateTime.Now.Second % interval;
            }

            System.Diagnostics.Debug.WriteLine(DateTime.Now + " Sleep: " + secondsToNextUpdate + " [sec]");

            return new TimeSpan(0, 0, secondsToNextUpdate);
        }

        private void TimerHandler(object sender, EventArgs e)
        {
            UpdateReserveList();

            RefreshTimer.Interval = GetNextTimerInterval();
        }

        private void UpdateReserveList()
        {
            if (ReserveList == null) return;

            long nowTicks = DateTime.Now.Ticks;
            
            ReserveList.ForEach(item => item.Update(nowTicks));
        }

        public void UpdateInfo()
        {
            var items = CommonManager.Instance.DB.ReserveList.Values.
                // 無効予約を除外
                Where(x => IsDisabledReserveItemVisible || (x.RecSetting.RecMode != 5)).
                // 開始時刻でソート
                OrderBy(x => Settings.Instance.InfoWindowBasedOnBroadcast ? x.StartTime : x.StartTimeWithMargin(0)). 
                Select(x => new ReserveItemLive(x));

            if (Settings.Instance.InfoWindowItemFilterLevel == int.MaxValue)
            {
                //項目数制限
                items = items.Take(Settings.Instance.InfoWindowItemTopN);
            }
            // 表示レベルでの制限は時刻とともに表示レベルが変化するので、ここでは除外しない

            // すべてのitemを定期的に更新するので毎回評価されないようにListに変換
            ReserveList = items.ToList();

            RefreshTimer.Interval = GetNextTimerInterval();
        }
    }
}
