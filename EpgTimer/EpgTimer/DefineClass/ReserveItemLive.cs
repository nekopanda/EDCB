using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EpgTimer
{
    public class ReserveItemLive : ReserveItem, INotifyPropertyChanged
    {
        public enum ReserveItemBgColorIndex
        {
            Level1BgColor = 0,
            Level2BgColor = 1,
            Level3BgColor = 2,
            DisabledBgColor = 3,
            ErrorBgColor = 4,
        }

        private static readonly Brush DefaultBgColor = new SolidColorBrush(Colors.Transparent);

        private static Brush GetItemBgColor(ReserveItemBgColorIndex index)
        {
            return CommonManager.Instance.InfoWindowItemBgColors[(int)index];
        }

        #region プロパティ用のフィールド

        private Brush _Background;
        private bool _IsOnAirOrRec;
        private int _OnAirOrRecProgress;
        private Visibility _OnAirOrRecProgressVisibility;

        #endregion

        private int secondsToStart;

        public ReserveItemLive() : this(null) { }

        public ReserveItemLive(ReserveData item) : base(item)
        {
            Update(DateTime.Now);
        }

        #region INotifyPropertyChangedの実装

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            field = value;
            RaisePropertyChanged(propertyName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 放送または録画の開始時刻を返します。
        /// </summary>
        /// <returns>放送または録画の開始時刻</returns>
        /// <remarks>Settings.Instance.InfoWindowBasedOnBroadcastがtrueなら放送、falseなら録画の開始時刻を返します。</remarks>
        private DateTime GetOnAirOrRecStartTime()
        {
            DateTime startTime = ReserveInfo.StartTime;

            if (!Settings.Instance.InfoWindowBasedOnBroadcast)
            {
                // 録画の開始終了を基準
                int startMargin = ReserveInfo.RecSetting.GetTrueMargin(true);

                startTime = startTime.AddSeconds(-startMargin);
            }

            return startTime;
        }

        /// <summary>
        /// 放送または録画の長さを秒数で返します。
        /// </summary>
        /// <returns>放送または録画の長さ</returns>
        /// <remarks>Settings.Instance.InfoWindowBasedOnBroadcastがtrueなら放送、falseなら録画の開始時刻を返します。</remarks>
        private uint GetOnAirOrRecDurationSecond()
        {
            uint durationSecond = ReserveInfo.DurationSecond;

            if (!Settings.Instance.InfoWindowBasedOnBroadcast)
            {
                // 録画の開始終了を基準
                int startMargin = ReserveInfo.RecSetting.GetTrueMargin(true);
                int endMargin = ReserveInfo.RecSetting.GetTrueMargin(false);

                durationSecond += (uint)(startMargin + endMargin);
            }

            return durationSecond;
        }

        /// <summary>
        /// 指定した時刻から放送または録画が開始されるまでの秒数をsecondsToStartに代入します。
        /// 放送中かどうか等の判定はすべてsecondsToStartを参照して決定しています。
        /// </summary>
        /// <param name="now"></param>
        private void UpdateSecondsToStart(DateTime now)
        {
            secondsToStart = (ReserveInfo != null) ? (int)GetOnAirOrRecStartTime().Subtract(now).TotalSeconds : int.MaxValue;
        }

        /// <summary>
        /// 放送または録画時間内にあるかどうかを返します。
        /// </summary>
        /// <returns>放送または録画時間内ならtrueを、そうでなければfalseを返します。</returns>
        private bool CalcIsOnAirOrRec()
        {
            bool result = false;

            if (ReserveInfo == null) return result;
            if (!IsEnabled) return result;

            uint durationSecond = GetOnAirOrRecDurationSecond();
            if (durationSecond <= 0) return result;

            int elapsedTimeSecond = -secondsToStart;

            result = (elapsedTimeSecond >= 0 && elapsedTimeSecond <= durationSecond);

            return result;
        }

        /// <summary>
        /// 放送または録画時間内でどれだけ経過しているかの割合を0から100の範囲で返します。
        /// </summary>
        /// <returns>放送または録画時間内でどれだけ経過しているかの割合を0から100の範囲で返します。</returns>
        private int CalcOnAirOrRecProgress()
        {
            int result = 0;

            if (ReserveInfo == null) return result;
            if (!IsEnabled) return result;

            uint durationSecond = GetOnAirOrRecDurationSecond();
            if (durationSecond <= 0) return result;

            int elapsedTimeSecond = -secondsToStart;

            if (elapsedTimeSecond <= 0)
            {
                result = 0;
            }
            else if (elapsedTimeSecond >= durationSecond)
            {
                result = 100;
            }
            else
            {
                result = (int)(elapsedTimeSecond * 100.0 / durationSecond + 0.5);
            }

            return result;
        }

        /// <summary>
        /// ReserveItemの状態と放送または録画までの時刻に応じてBrushを決定して返します。
        /// 決定できなかった場合はDefaultBgColorを返します。
        /// </summary>
        /// <returns>背景用のBrush</returns>
        private Brush CalcBackground()
        {
            Brush result = DefaultBgColor;

            if (ReserveInfo == null) return result;

            if (IsEnabled)
            {
                if (ReserveInfo.OverlapMode == 1 || ReserveInfo.OverlapMode == 2 || ReserveInfo.IsAutoAddInvalid)
                {
                    result = GetItemBgColor(ReserveItemBgColorIndex.ErrorBgColor);
                }
                else if (secondsToStart <= Settings.Instance.InfoWindowItemLevel1Seconds)
                {
                    result = GetItemBgColor(ReserveItemBgColorIndex.Level1BgColor);
                }
                else if (secondsToStart <= Settings.Instance.InfoWindowItemLevel2Seconds)
                {
                    result = GetItemBgColor(ReserveItemBgColorIndex.Level2BgColor);
                }
                else if (secondsToStart <= Settings.Instance.InfoWindowItemLevel3Seconds)
                {
                    result = GetItemBgColor(ReserveItemBgColorIndex.Level3BgColor);
                }
            }
            else
            {
                result = GetItemBgColor(ReserveItemBgColorIndex.DisabledBgColor);
            }

            return result;
        }

        /// <summary>
        /// 基準となる時刻nowの値に応じてプロパティを更新します。
        /// </summary>
        public void Update(DateTime now)
        {
            UpdateSecondsToStart(now);

            Background = CalcBackground();
            IsOnAirOrRec = CalcIsOnAirOrRec();
            OnAirOrRecProgress = CalcOnAirOrRecProgress();
            OnAirOrRecProgressVisibility = (IsOnAirOrRec ? Visibility.Visible : Visibility.Collapsed);
        }

        #endregion

        #region Properties

        /// <summary>
        /// ReserveItemの状態と放送または録画までの時刻に応じてBrushを決定します。
        /// 決定できなかった場合はDefaultBgColor。
        /// </summary>
        public Brush Background
        {
            get { return _Background; }
            private set { SetProperty(ref _Background, value); }
        }

        /// <summary>
        /// 放送または録画中ならtrue、そうでなければfalse。
        /// </summary>
        public bool IsOnAirOrRec
        {
            get { return _IsOnAirOrRec; }
            private set { SetProperty(ref _IsOnAirOrRec, value); }
        }

        /// <summary>
        /// 放送または録画の進行度を示す0から100の値。
        /// </summary>
        public int OnAirOrRecProgress
        {
            get { return _OnAirOrRecProgress; }
            private set { SetProperty(ref _OnAirOrRecProgress, value); }
        }

        /// <summary>
        /// 放送または録画中ならVisible、そうでなければCollapsed。
        /// </summary>
        public Visibility OnAirOrRecProgressVisibility
        {
            get { return _OnAirOrRecProgressVisibility; }
            private set { SetProperty(ref _OnAirOrRecProgressVisibility, value); }
        }

        #endregion
    }
}
