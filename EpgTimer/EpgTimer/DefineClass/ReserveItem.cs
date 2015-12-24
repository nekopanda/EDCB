﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace EpgTimer
{
    public class ReserveItem : SearchItem
    {
        //EventInfo、ReserveInfo、JyanruKey、ForeColor、BackColor、BorderBrush -> SearchItem.cs

        public ReserveItem() { }
        public ReserveItem(ReserveData item)
        {
            base.ReserveInfo = item;
        }

        public static new string GetValuePropertyName(string key)
        {
            switch (key)
            {
                case "MarginStart": return "MarginStartValue";
                case "MarginEnd": return "MarginEndValue";
                default: return SearchItem.GetValuePropertyName(key);
            }
        }

        private EpgEventInfo eventInfo = null;
        public override EpgEventInfo EventInfo
        {
            get
            {
                if (eventInfo == null)
                {
                    if (ReserveInfo != null)
                    {
                        eventInfo = CommonManager.Instance.GetEpgEventInfoFromReserveData(ReserveInfo, false);
                    }
                }
                return eventInfo;
            }

            set { eventInfo = value; }
        }

        public override String EventName
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return ReserveInfo.Title;
            }
        }
        public override String ServiceName
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return ReserveInfo.StationName;
            }
        }
        public override String NetworkName
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.ConvertNetworkNameText(ReserveInfo.OriginalNetworkID);
            }
        }
        public override String StartTime
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.ConvertTimeText(ReserveInfo.StartTime, ReserveInfo.DurationSecond, Settings.Instance.ResInfoNoYear, Settings.Instance.ResInfoNoSecond);
            }
        }
        public override DateTime StartTimeValue
        {
            get
            {
                if (ReserveInfo == null) return new DateTime();
                //
                return ReserveInfo.StartTime;
            }
        }
        public String StartTimeShort
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.ConvertTimeText(ReserveInfo.StartTime, ReserveInfo.DurationSecond, true, true);
            }
        }
        public override String ProgramDuration
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.ConvertDurationText(ReserveInfo.DurationSecond, Settings.Instance.ResInfoNoDurSecond);
            }
        }
        public override UInt32 ProgramDurationValue
        {
            get
            {
                if (ReserveInfo == null) return UInt32.MinValue;
                //
                return ReserveInfo.DurationSecond;
            }
        }
        public String MarginStart
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return mutil.MarginText(ReserveInfo.RecSetting,true);
            }
        }
        public Double MarginStartValue
        {
            get
            {
                if (ReserveInfo == null) return Double.MinValue;
                //
                return mutil.GetMarginForSort(ReserveInfo.RecSetting, true);
            }
        }
        public String MarginEnd
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return mutil.MarginText(ReserveInfo.RecSetting, false);
            }
        }
        public Double MarginEndValue
        {
            get
            {
                if (ReserveInfo == null) return Double.MinValue;
                //
                return mutil.GetMarginForSort(ReserveInfo.RecSetting, false);
            }
        }
        //public String ProgramContent -> SearchItem.cs
        //public String JyanruKey -> SearchItem.cs
        public String Preset
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return ReserveInfo.RecSetting.LookUpPreset(ReserveInfo.EventID == 0xFFFF).DisplayName;
            }
        }
        public bool IsEnabled
        {
            set
            {
                //選択されている場合、複数選択時に1回の通信で処理するため、ウインドウ側に処理を渡す。
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.reserveView.ChgOnOffFromCheckbox(this);
            }
            get
            {
                if (ReserveInfo == null) return false;
                //
                return ReserveInfo.RecSetting.RecMode != 5;
            }
        }
        public String RecMode
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.Instance.ConvertRecModeText(ReserveInfo.RecSetting.RecMode);
            }
        }
        public String Priority
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return ReserveInfo.RecSetting.Priority.ToString();
            }
        }
        public String Tuijyu
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.Instance.YesNoDictionary[ReserveInfo.RecSetting.TuijyuuFlag].DisplayName;
            }
        }
        public String Pittari
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.Instance.YesNoDictionary[ReserveInfo.RecSetting.PittariFlag].DisplayName;
            }
        }
        public String Tuner
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                return CommonManager.Instance.ConvertTunerText(ReserveInfo.RecSetting.TunerID);
            }
        }
        public String Comment
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                String view = (ReserveInfo.IsAutoAddMissing() == true ? "不明な" : "") + ReserveInfo.Comment.ToString();
                if (view == "")
                {
                    view = "個別予約(" + (ReserveInfo.EventID == 0xFFFF ? "プログラム" : "EPG") + ")";
                }
                return view;
            }
        }
        public String AutoAddInfo
        {
            get
            {
                if (ReserveInfo == null) return "";
                //
                String info = "";
                var reserveList = ReserveInfo.AutoAddInfo;
                foreach (var data in reserveList)
                {
                    if (info.Length > 0) info += ",";
                    info += data.andKey + "(" + data.dataID + ")";
                }
                return info;
            }
        }
        public List<String> RecFolder
        {
            get
            {
                if (ReserveInfo == null) new List<string>();
                //
                return mutil.GetRecFolderViewList(ReserveInfo.RecSetting);
            }
        }
        public List<String> RecFileName
        {
            get
            {
                if (ReserveInfo == null) return new List<string>();
                //
                return ReserveInfo.RecFileNameList;
            }
        }
        public override TextBlock ToolTipView
        {
            get
            {
                if (Settings.Instance.NoToolTip == true) return null;
                if (ReserveInfo == null) return mutil.GetTooltipBlockStandard("");
                //
                return mutil.GetTooltipBlockStandard(CommonManager.Instance.ConvertReserveText(ReserveInfo));
            }
        }
        public override String Status
        {
            get
            {
                String[] wiewString = { "", "無", "予+", "無+", "録*", "無*" };
                int index = 0;
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.IsOnAir() == true)
                    {
                        index = 2;
                    }
                    if (ReserveInfo.IsOnRec() == true)//マージンがあるので、IsOnAir==trueとは限らない
                    {
                        index = 4;
                    }
                    if (ReserveInfo.RecSetting.RecMode == 5) //無効の判定
                    {
                        index += 1;
                    }
                }
                return wiewString[index];
            }
        }
        public override SolidColorBrush StatusColor
        {
            get
            {
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.IsOnRec() == true)
                    {
                        return CommonManager.Instance.StatRecForeColor;
                    }
                    if (ReserveInfo.IsOnAir() == true)
                    {
                        return CommonManager.Instance.StatOnAirForeColor;
                    }
                }
                return CommonManager.Instance.StatResForeColor;
            }
        }

    }

    public static class ReserveItemEx
    {
        public static List<ReserveData> ReserveInfoList(this ICollection<ReserveItem> itemlist)
        {
            return itemlist.Where(item => item != null).Select(item => item.ReserveInfo).ToList();
        }
    }
}
