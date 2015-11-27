using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EpgTimer.TunerReserveViewCtrl
{
    class TunerReservePanel : EpgTimer.UserCtrlView.PanelBase
    {
        public List<ReserveViewItem> Items { get; set; }

        public override void ClearInfo() 
        {
            Items = new List<ReserveViewItem>();
        }

        protected bool RenderText(String text, DrawingContext dc, GlyphTypeface glyphType, SolidColorBrush brush, double fontSize, double maxWidth, double maxHeight, double x, double baseline, ref double useHeight, bool nowrap = false)
        {
            if (x <= 0 || maxWidth <= 0)
            {
                useHeight = 0;
                return false;
            }

            double totalHeight = 0;
            double fontHeight = fontSize * glyphType.Height;

            string[] lineText = text.Replace("\r", "").Split('\n');
            foreach (string line in lineText)
            {
                //高さ確認
                if (totalHeight + fontHeight > maxHeight)
                {
                    //これ以上は無理
                    useHeight = totalHeight;
                    return false;
                }

                // ベースラインの位置の計算
                // ビットマップフォントがかすれる問題 とりあえず整数にしておく
                Point origin = new Point(Math.Round(x), Math.Round(totalHeight + baseline));

                //メイリオみたいに行間のあるフォントと MS P ゴシックみたいな行間のないフォントがあるので
                //なんとなく行間を作ってみる。
                totalHeight += Math.Max(fontHeight, fontSize + 2);
                double totalWidth = 0;

                List<ushort> glyphIndexes = new List<ushort>();
                List<double> advanceWidths = new List<double>();
                for (int n = 0; n < line.Length; n++)
                {
                    ushort glyphIndex = glyphType.CharacterToGlyphMap[line[n]];
                    double width = glyphType.AdvanceWidths[glyphIndex] * fontSize;

                    if (totalWidth + width > maxWidth)
                    {
                        if (nowrap == true) break;//改行しない場合ここで終り
                        if (totalWidth == 0) return false;//一文字も置けなかった(glyphIndexesなどのCount=0のまま)

                        if (totalHeight + fontHeight > maxHeight)
                        {
                            //次の行無理
                            glyphIndex = glyphType.CharacterToGlyphMap['…'];
                            double widthEllipsis = glyphType.AdvanceWidths[glyphIndex] * fontSize;
                            while (totalWidth - advanceWidths.Last() + widthEllipsis > maxWidth)
                            {
                                glyphIndexes.RemoveAt(glyphIndexes.Count-1);
                                advanceWidths.RemoveAt(advanceWidths.Count - 1);
                            }
                            glyphIndexes[glyphIndexes.Count - 1] = glyphIndex;
                            advanceWidths[advanceWidths.Count - 1] = widthEllipsis;

                            GlyphRun glyphRun = new GlyphRun(glyphType, 0, false, fontSize,
                                glyphIndexes, origin, advanceWidths, null, null, null, null,
                                null, null);

                            dc.DrawGlyphRun(brush, glyphRun);

                            useHeight = totalHeight;
                            return false;
                        }
                        else
                        {
                            //次の行いけるので今までの分出力
                            GlyphRun glyphRun = new GlyphRun(glyphType, 0, false, fontSize,
                                glyphIndexes, origin, advanceWidths, null, null, null, null,
                                null, null);

                            dc.DrawGlyphRun(brush, glyphRun);

                            origin = new Point(Math.Round(x), Math.Round(totalHeight + baseline));
                            totalHeight += Math.Max(fontHeight, fontSize + 2);
                            totalWidth = 0;
                            glyphIndexes = new List<ushort>();
                            advanceWidths = new List<double>();
                        }
                    }
                    glyphIndexes.Add(glyphIndex);
                    advanceWidths.Add(width);
                    totalWidth += width;
                }
                if (glyphIndexes.Count > 0)
                {
                    GlyphRun glyphRun = new GlyphRun(glyphType, 0, false, fontSize,
                        glyphIndexes, origin, advanceWidths, null, null, null, null,
                        null, null);

                    dc.DrawGlyphRun(brush, glyphRun);
                }
            }
            useHeight = totalHeight;
            return true;
        }
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Background, null, new Rect(RenderSize));
            this.VisualTextRenderingMode = TextRenderingMode.ClearType;
            this.VisualTextHintingMode = TextHintingMode.Fixed;

            if (Items == null) return;

            try
            {
                double sizeMin = Settings.Instance.TunerFontSize;
                double sizeTitle = Settings.Instance.TunerFontSizeService;
                double sizeNormal = Settings.Instance.TunerFontSize;
                double indentTitle = sizeMin * 1.7;
                double indentNormal = Settings.Instance.TunerTitleIndent ? indentTitle : 2;
                SolidColorBrush colorTitle = CommonManager.Instance.CustTunerServiceColor;
                SolidColorBrush colorNormal = CommonManager.Instance.CustTunerTextColor;

                double heightMin = Settings.Instance.TunerFontHeight;
                double heightTitle = Settings.Instance.TunerFontHeightService;
                double heightNormal = Settings.Instance.TunerFontHeight;
                double height1stLine = Math.Max(heightMin, heightTitle);

                // 起点はベースラインになるので、それぞれのベースラインを計算しておく。
                // 分とサービス名はフォントが異なることができるのでセンタリングして合うように調整しておく。
                double baselineMin = sizeMin * vutil.GlyphTypefaceTunerNormal.Baseline + (height1stLine - heightMin) / 2;
                double baselineTitle = sizeTitle * vutil.GlyphTypefaceTunerService.Baseline + (height1stLine - heightTitle) / 2;
                double baselineNormal = sizeNormal * vutil.GlyphTypefaceTunerNormal.Baseline;

                foreach (ReserveViewItem info in Items)
                {
                    colorTitle = Settings.Instance.TunerColorModeUse == true ? info.ForeColorPri : colorTitle;

                    double x = info.LeftPos;
                    double y = info.TopPos;
                    double height = Math.Max(info.Height, 0);
                    double width = info.Width;

                    dc.DrawRectangle(Brushes.LightGray, null, new Rect(x, y, width, height));
                    if (height > 2)
                    {
                        // 枠の内側を計算
                        x += 1;
                        y += 1;
                        width -= 2;
                        height -= 2;
                        dc.DrawRectangle(info.BackColor, null, new Rect(x, y, width, height));

                        if (height < height1stLine) // ModifierMinimumHeight してるので足りなくならないハズ。
                        {
                            //高さ足りない
                            info.TitleDrawErr = true;
                            continue;
                        }

                        // margin 設定
                        x += 2;
                        width -= 2;

                        double useHeight = 0;

                        //分
                        string min = info.ReserveInfo.StartTime.Minute.ToString("d02");
                        if (RenderText(min, dc, vutil.GlyphTypefaceTunerNormal, colorTitle, sizeMin, width, height, x, y + baselineMin, ref useHeight) == false)
                        {
                            info.TitleDrawErr = true;
                            continue;
                        }

                        //サービス名
                        if (info.ReserveInfo.StationName.Length > 0)
                        {
                            string serviceName = info.ReserveInfo.StationName
                                + "(" + CommonManager.ConvertNetworkNameText(info.ReserveInfo.OriginalNetworkID) + ")";
                            if (RenderText(serviceName, dc, vutil.GlyphTypefaceTunerService, colorTitle, sizeTitle, width - indentTitle, height, x + indentTitle, y + baselineTitle, ref useHeight, Settings.Instance.TunerServiceNoWrap) == false)
                            {
                                info.TitleDrawErr = true;
                                continue;
                            }
                            useHeight += 2; // margin: 番組名との間隔は 2px にする
                        }

                        //番組名
                        if (info.ReserveInfo.Title.Length > 0)
                        {
                            if (RenderText(info.ReserveInfo.Title, dc, vutil.GlyphTypefaceTunerNormal, colorNormal, sizeNormal, width - indentNormal, height - useHeight, x + indentNormal, y + useHeight + baselineNormal, ref useHeight) == false)
                            {
                                info.TitleDrawErr = true;
                                continue;
                            }
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
}
