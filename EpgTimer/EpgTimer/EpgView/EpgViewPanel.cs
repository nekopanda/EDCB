using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EpgTimer.EpgView
{
    class EpgViewPanel : EpgTimer.UserCtrlView.PanelBase
    {
        private Dictionary<ProgramViewItem, List<TextDrawItem>> textDrawDict = new Dictionary<ProgramViewItem, List<TextDrawItem>>();

        private List<ProgramViewItem> items;
        public List<ProgramViewItem> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
                CreateDrawTextList();
            }
        }

        public override void ClearInfo()
        {
            items = new List<ProgramViewItem>();
        }

        protected void CreateDrawTextList()
        {
            textDrawDict = new Dictionary<ProgramViewItem, List<TextDrawItem>>();
            Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;

            this.VisualTextRenderingMode = TextRenderingMode.ClearType;
            this.VisualTextHintingMode = TextHintingMode.Fixed;
            this.UseLayoutRounding = true;
            if (Items == null)
            {
                return;
            }

            try
            {
                double sizeMin = Settings.Instance.FontSizeTitle;
                double sizeTitle = Settings.Instance.FontSizeTitle;
                double sizeNormal = Settings.Instance.FontSize;
                double indentTitle = Math.Floor(sizeMin * 1.7);
                double indentNormal = Math.Floor(Settings.Instance.EpgTitleIndent ? indentTitle : 2);
                SolidColorBrush colorTitle = CommonManager.Instance.CustTitle1Color;
                SolidColorBrush colorNormal = CommonManager.Instance.CustTitle2Color;

                double height1stLine = Math.Max(Settings.Instance.FontHeightTitle, Settings.Instance.FontHeightTitle);

                // あらかじめベースラインをそろえるために計算しておく。
                // 今は sizeMin と sizeTitle 同じだけどね…
                double baselineMin = Math.Max(sizeMin * vutil.GlyphTypefaceTitle.Baseline, sizeTitle * vutil.GlyphTypefaceTitle.Baseline);
                double baselineNormal = sizeNormal * vutil.GlyphTypefaceNormal.Baseline;

                foreach (ProgramViewItem info in Items)
                {
                    List<TextDrawItem> textDrawList = new List<TextDrawItem>();
                    textDrawDict[info] = textDrawList;
                    if (info.Height > 2)
                    {
                        if (info.Height < height1stLine) // ModifierMinimumHeight してるので足りなくならないハズ。
                        {
                            //高さ足りない
                            info.TitleDrawErr = true;
                        }

                        double x = info.LeftPos;
                        double y = info.TopPos;
                        double height = info.Height;
                        double width = info.Width;
                        double totalHeight = 0;

                        // offset
                        x += 2;
                        width -= 2;
                        y += 1;
                        height -= 1;

                        // margin 設定
                        width -= 4;

                        //分
                        string min = (info.EventInfo.StartTimeFlag != 1 ? "未定 " : info.EventInfo.start_time.Minute.ToString("d02"));
                        double useHeight = 0;
                        if (RenderText(min, ref textDrawList, vutil.GlyphTypefaceTitle, sizeMin, width, height, x, y + baselineMin, ref useHeight, colorTitle, m) == false)
                        {
                            info.TitleDrawErr = true;
                            continue;
                        }

                        //番組情報
                        if (info.EventInfo.ShortInfo != null)
                        {
                            //タイトル
                            if (info.EventInfo.ShortInfo.event_name.Length > 0)
                            {
                                if (RenderText(info.EventInfo.ShortInfo.event_name, ref textDrawList, vutil.GlyphTypefaceTitle, sizeTitle, width - indentTitle, height - totalHeight, x + indentTitle, y + totalHeight + baselineMin, ref useHeight, colorTitle, m) == false)
                                {
                                    info.TitleDrawErr = true;
                                    continue;
                                }
                                totalHeight += Math.Floor(useHeight + 4); // 説明との間隔は 4px にする
                            }


                            //説明
                            if (info.EventInfo.ShortInfo.text_char.Length > 0)
                            {
                                if (RenderText(info.EventInfo.ShortInfo.text_char, ref textDrawList, vutil.GlyphTypefaceNormal, sizeNormal, width - indentNormal, height - totalHeight, x + indentNormal, y + totalHeight + baselineNormal, ref useHeight, colorNormal, m) == false)
                                {
                                    continue;
                                }
                                //totalHeight += useHeight + 4; // 詳細との間隔は 4px にする
                            }

                            //詳細
//                            if (info.EventInfo.ExtInfo != null)
//                            {
//                                if (info.EventInfo.ExtInfo.text_char.Length > 0)
//                                {
//                                    if (RenderText(info.EventInfo.ExtInfo.text_char, ref textDrawList, vutil.GlyphTypefaceNormal, sizeNormal, width - widthOffset, height - totalHeight, x + widthOffset, y + totalHeight, ref useHeight, colorNormal, m) == false)
//                                    {
//                                        continue;
//                                    }
//                                    totalHeight += useHeight;
//                                }
//                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        protected bool RenderText(String text, ref List<TextDrawItem> textDrawList, GlyphTypeface glyphType, double fontSize, double maxWidth, double maxHeight, double x, double baseline, ref double useHeight, SolidColorBrush fontColor, Matrix m)
        {
            double totalHeight = 0;
            double fontHeight = fontSize * glyphType.Height;

            string[] lineText = text.Replace("\r", "").Split('\n');
            foreach (string line in lineText)
            {
                List<ushort> glyphIndexes = new List<ushort>();
                List<double> advanceWidths = new List<double>();

                //ベースラインの位置
                double dpix = x * m.M11;
                double dpiy = (totalHeight + baseline) * m.M22;
                Point origin = new Point(dpix / m.M11, dpiy / m.M22);

                //メイリオみたいに行間のあるフォントと MS P ゴシックみたいな行間のないフォントがあるので
                //なんとなく行間を作ってみる。
                totalHeight += Math.Max(fontHeight, fontSize + 2);
                double totalWidth = 0;

                for (int n = 0; n < line.Length; n++)
                {
                    // 行頭の空白を無視する
                    if (glyphIndexes.Count == 0 && (line[n] == ' ' || line[n] == '\x3000'))
                        continue;

                    ushort glyphIndex = glyphType.CharacterToGlyphMap[line[n]];
                    double width = glyphType.AdvanceWidths[glyphIndex] * fontSize;
                    if (totalWidth + width > maxWidth)
                    {
                        if (glyphIndexes.Count > 0)
                        {
                            TextDrawItem item = new TextDrawItem();
                            item.FontColor = fontColor;
                            item.Text = new GlyphRun(glyphType, 0, false, fontSize,
                                glyphIndexes, origin, advanceWidths, null, null, null, null,
                                null, null);
                            textDrawList.Add(item);
                        }
                        if (totalHeight > maxHeight)
                        {
                            //次の行無理
                            useHeight = totalHeight;
                            return false;
                        }
                        else
                        {
                            //次の行いける
                            dpiy = (totalHeight + baseline) * m.M22;
                            origin = new Point(dpix / m.M11, dpiy / m.M22);
                            totalHeight += Math.Max(fontHeight, fontSize + 2);
                            totalWidth = 0;

                            glyphIndexes = new List<ushort>();
                            advanceWidths = new List<double>();

                            // 行頭の空白を無視する
                            if (line[n] == ' ' || line[n] == '\x3000')
                                continue;
                        }
                    }
                    glyphIndexes.Add(glyphIndex);
                    advanceWidths.Add(width);
                    totalWidth += width;
                }
                if (glyphIndexes.Count > 0)
                {
                    TextDrawItem item = new TextDrawItem();
                    item.FontColor = fontColor;
                    item.Text = new GlyphRun(glyphType, 0, false, fontSize,
                        glyphIndexes, origin, advanceWidths, null, null, null, null,
                        null, null);
                    textDrawList.Add(item);
                }
            }
            useHeight = Math.Floor(totalHeight);
            return true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            Brush bgBrush = Background;
            dc.DrawRectangle(bgBrush, null, new Rect(RenderSize));
            this.VisualTextRenderingMode = TextRenderingMode.ClearType;
            this.VisualTextHintingMode = TextHintingMode.Fixed;
            this.UseLayoutRounding = true;

            if (Items == null)
            {
                return;
            }
            
            try
            {
                double sizeNormal = Settings.Instance.FontSize;
                double sizeTitle = Settings.Instance.FontSizeTitle;
                foreach (ProgramViewItem info in Items)
                {
                    dc.DrawRectangle(bgBrush, null, new Rect(info.LeftPos, info.TopPos, info.Width, 1));
                    dc.DrawRectangle(bgBrush, null, new Rect(info.LeftPos, info.TopPos + info.Height, info.Width, 1));
                    if (info.Height > 1)
                    {
                        dc.DrawRectangle(info.ContentColor, null, new Rect(info.LeftPos + 0, info.TopPos + 0.5, info.Width - 1, info.Height - 0.5));
                        if (textDrawDict.ContainsKey(info))
                        {
                            dc.PushClip(new RectangleGeometry(new Rect(info.LeftPos + 0, info.TopPos + 0.5, info.Width - 1, info.Height - 0.5)));
                            foreach (TextDrawItem txtinfo in textDrawDict[info])
                            {
                                dc.DrawGlyphRun(txtinfo.FontColor, txtinfo.Text);
                            }
                            dc.Pop();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }

    class TextDrawItem
    {
        public SolidColorBrush FontColor;
        public GlyphRun Text;
    }
}
