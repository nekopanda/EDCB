using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace EpgTimer
{
    public class InfoWindowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged実装

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }

        #endregion

        #region ReserveMode 変更通知プロパティ

        private IEnumerable<ReserveItem> _ReserveList = null;

        public IEnumerable<ReserveItem> ReserveList
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

        public void UpdateInfo()
        {
            ReserveList = CommonManager.Instance.DB.ReserveList.Values.
                Where(x => x.RecSetting.RecMode != 5). // 無効予約を除外
                OrderBy(x => x.StartTime). // 開始時刻でソート
                Select(x => new ReserveItem(x));
        }
    }
}
