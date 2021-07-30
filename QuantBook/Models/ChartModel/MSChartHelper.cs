using Caliburn.Micro;
using QuantBook.Models.Yahoo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuantBook.Models.ChartModel
{
    public static class MSChartHelper
    {
        #region MyChart Factory Methods

        public static void MyChart(Chart chart1, BindableCollection<Series> chartSeries, string chartTitle, string xLabel, string yLabel, ChartBackgroundColor backgroundColor, params string[] y2Label)
        {
            if (chart1.ChartAreas.Count < 1)
            {
                ChartArea area = new ChartArea();
                ChartStyle(chart1, area, backgroundColor);
            }

            if (chartTitle != "")
                chart1.Titles.Add(chartTitle);
            chart1.ChartAreas[0].AxisX.Title = xLabel;
            chart1.ChartAreas[0].AxisY.Title = yLabel;
            if (y2Label.Length > 0)
                chart1.ChartAreas[0].AxisY2.Title = y2Label[0];

            foreach (var ds in chartSeries)
                chart1.Series.Add(ds);

            if (chartSeries.Count > 1)
            {
                Legend legend = new Legend();
                legend.Font = new System.Drawing.Font("Trebuchet MS", 7.0F, FontStyle.Regular);
                legend.BackColor = Color.Transparent;
                legend.AutoFitMinFontSize = 5;
                legend.LegendStyle = LegendStyle.Column;

                legend.IsDockedInsideChartArea = true;
                legend.Docking = Docking.Left;
                legend.InsideChartArea = chart1.ChartAreas[0].Name;
                chart1.Legends.Add(legend);
            }
        }
        public static void ChartStyle(Chart chart1, ChartArea area, ChartBackgroundColor backgroundColor)
        {
            int r1 = 211;
            int g1 = 223;
            int b1 = 240;
            int r2 = 26;
            int g2 = 59;
            int b2 = 105;
            int r3 = 165;
            int g3 = 191;
            int b3 = 228;

            switch (backgroundColor)
            {
                case ChartBackgroundColor.Blue:
                    chart1.BackColor = Color.FromArgb(r1, g1, b1);
                    chart1.BorderlineColor = Color.FromArgb(r2, g2, b2);
                    area.BackColor = Color.FromArgb(64, r3, g3, b3);
                    break;
                case ChartBackgroundColor.Green:
                    chart1.BackColor = Color.FromArgb(g1, b1, r1);
                    chart1.BorderlineColor = Color.FromArgb(g2, b2, r2);
                    area.BackColor = Color.FromArgb(64, g3, b3, r3);
                    break;
                case ChartBackgroundColor.Red:
                    chart1.BackColor = Color.FromArgb(b1, r1, g1);
                    chart1.BorderlineColor = Color.FromArgb(b2, r2, g2);
                    area.BackColor = Color.FromArgb(64, b3, r3, g3);
                    break;
                case ChartBackgroundColor.White:
                    chart1.BackColor = Color.White;
                    chart1.BorderlineColor = Color.White;
                    area.BackColor = Color.White;
                    break;
            }

            if (backgroundColor != ChartBackgroundColor.White)
            {
                chart1.BackSecondaryColor = Color.White;
                chart1.BackGradientStyle = GradientStyle.TopBottom;
                chart1.BorderlineDashStyle = ChartDashStyle.Solid;
                chart1.BorderlineWidth = 2;
                chart1.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

                area.Area3DStyle.IsClustered = true;
                area.Area3DStyle.Perspective = 10;
                area.Area3DStyle.IsRightAngleAxes = false;
                area.Area3DStyle.WallWidth = 0;
                area.Area3DStyle.Inclination = 15;
                area.Area3DStyle.Rotation = 10;
            }

            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, FontStyle.Regular);
            //area.AxisX.LabelStyle.IsEndLabelVisible = false;
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisX.IsStartedFromZero = false;
            area.AxisX.RoundAxisValues();

            area.AxisY.IsLabelAutoFit = false;
            area.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY.IsStartedFromZero = false;

            area.AxisY2.IsLabelAutoFit = false;
            area.AxisY2.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area.AxisY2.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY2.MajorGrid.LineColor = Color.FromArgb(15, 15, 15, 15);
            area.AxisY2.IsStartedFromZero = false;


            area.BackSecondaryColor = System.Drawing.Color.White;
            area.BackGradientStyle = GradientStyle.TopBottom;
            area.BorderColor = Color.FromArgb(64, 64, 64, 64);
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.Position.Auto = false;
            area.Position.Height = 82F;
            area.Position.Width = 88F;
            area.Position.X = 3F;
            area.Position.Y = 10F;
            area.ShadowColor = Color.Transparent;

            chart1.ChartAreas.Add(area);
            //chart1.Dock = DockStyle.Fill;
            chart1.Invalidate();
        }
        public static void MyChart2(Chart chart1, BindableCollection<Series> chartSeries, string chartTitle, string xLabel, string yLabel, ChartBackgroundColor backgroundColor, params string[] y2Label)
        {
            if (chart1.ChartAreas.Count < 1)
            {
                ChartArea area1 = new ChartArea();
                ChartArea area2 = new ChartArea();
                ChartStyle2(chart1, area1, area2, backgroundColor);
            }

            if (chartTitle != "")
                chart1.Titles.Add(chartTitle);
            chart1.ChartAreas[1].AxisX.Title = xLabel;
            chart1.ChartAreas[0].AxisY.Title = yLabel;

            if (y2Label.Length > 0)
                chart1.ChartAreas[1].AxisY.Title = y2Label[0];

            List<int> numSeries = new List<int>();
            if (chartSeries.Count > 1)
            {
                for (int i = 0; i < chartSeries.Count; i++)
                {
                    int a1 = (int)chartSeries[i].Tag - 1;
                    numSeries.Add(a1);
                    chartSeries[i].ChartArea = chart1.ChartAreas[a1].Name;
                    chart1.Series.Add(chartSeries[i]);
                }
            }


            int n1 = (numSeries.Where(x => x == 0)).Count();
            int n2 = (numSeries.Where(x => x == 1)).Count();

            if (n1 > 1)
            {
                Legend legend = new Legend();
                legend.Font = new System.Drawing.Font("Trebuchet MS", 7.0F, FontStyle.Regular);
                legend.BackColor = Color.Transparent;
                legend.AutoFitMinFontSize = 5;
                legend.LegendStyle = LegendStyle.Column;

                legend.IsDockedInsideChartArea = true;
                legend.Docking = Docking.Left;
                legend.InsideChartArea = chart1.ChartAreas[0].Name;
                chart1.Legends.Add(legend);
            }

            if (n2 > 1)
            {
                Legend legend = new Legend();
                legend.Font = new System.Drawing.Font("Trebuchet MS", 7.0F, FontStyle.Regular);
                legend.BackColor = Color.Transparent;
                legend.AutoFitMinFontSize = 5;
                legend.LegendStyle = LegendStyle.Column;

                legend.IsDockedInsideChartArea = true;
                legend.Docking = Docking.Left;
                legend.InsideChartArea = chart1.ChartAreas[1].Name;
                chart1.Legends.Add(legend);
            }
        }
        public static void ChartStyle2(Chart chart1, ChartArea area, ChartArea area2, ChartBackgroundColor backgroundColor)
        {
            int r1 = 211;
            int g1 = 223;
            int b1 = 240;
            int r2 = 26;
            int g2 = 59;
            int b2 = 105;
            int r3 = 165;
            int g3 = 191;
            int b3 = 228;

            switch (backgroundColor)
            {
                case ChartBackgroundColor.Blue:
                    chart1.BackColor = Color.FromArgb(r1, g1, b1);
                    chart1.BorderlineColor = Color.FromArgb(r2, g2, b2);
                    area.BackColor = Color.FromArgb(64, r3, g3, b3);
                    area2.BackColor = Color.FromArgb(64, r3, g3, b3);
                    break;
                case ChartBackgroundColor.Green:
                    chart1.BackColor = Color.FromArgb(g1, b1, r1);
                    chart1.BorderlineColor = Color.FromArgb(g2, b2, r2);
                    area.BackColor = Color.FromArgb(64, g3, b3, r3);
                    area2.BackColor = Color.FromArgb(64, g3, b3, r3);
                    break;
                case ChartBackgroundColor.Red:
                    chart1.BackColor = Color.FromArgb(b1, r1, g1);
                    chart1.BorderlineColor = Color.FromArgb(b2, r2, g2);
                    area.BackColor = Color.FromArgb(64, b3, r3, g3);
                    area2.BackColor = Color.FromArgb(64, b3, r3, g3);
                    break;
                case ChartBackgroundColor.White:
                    chart1.BackColor = Color.White;
                    chart1.BorderlineColor = Color.White;
                    area.BackColor = Color.White;
                    area2.BackColor = Color.White;
                    break;
            }


            if (backgroundColor != ChartBackgroundColor.White)
            {
                chart1.BackSecondaryColor = Color.White;
                chart1.BackGradientStyle = GradientStyle.TopBottom;
                chart1.BorderlineDashStyle = ChartDashStyle.Solid;
                chart1.BorderlineWidth = 2;
                chart1.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

                area.Area3DStyle.IsClustered = true;
                area.Area3DStyle.Perspective = 10;
                area.Area3DStyle.IsRightAngleAxes = false;
                area.Area3DStyle.WallWidth = 0;
                area.Area3DStyle.Inclination = 15;
                area.Area3DStyle.Rotation = 10;
            }

            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, FontStyle.Regular);
            //area.AxisX.LabelStyle.IsEndLabelVisible = false;
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisX.IsStartedFromZero = false;
            area.AxisX.RoundAxisValues();

            area.AxisY.IsLabelAutoFit = false;
            area.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY.IsStartedFromZero = false;

            /*area.AxisY2.IsLabelAutoFit = false;
            area.AxisY2.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area.AxisY2.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY2.MajorGrid.LineColor = Color.FromArgb(15, 15, 15, 15);
            area.AxisY2.IsStartedFromZero = false;*/

            area.BackSecondaryColor = System.Drawing.Color.White;
            area.BackGradientStyle = GradientStyle.TopBottom;
            area.BorderColor = Color.FromArgb(64, 64, 64, 64);
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.Position.Auto = false;
            area.Position.Height = 50F;
            area.Position.Width = 88F;
            area.Position.X = 3F;
            area.Position.Y = 10F;
            area.ShadowColor = Color.Transparent;
            area.Name = "Area1";
            chart1.ChartAreas.Add(area);




            area2.AlignWithChartArea = "Area1";
            if (backgroundColor != ChartBackgroundColor.White)
            {
                area2.Area3DStyle.IsClustered = true;
                area2.Area3DStyle.Perspective = 10;
                area2.Area3DStyle.IsRightAngleAxes = false;
                area2.Area3DStyle.WallWidth = 0;
                area2.Area3DStyle.Inclination = 15;
                area2.Area3DStyle.Rotation = 10;
            }

            area2.AxisX.IsLabelAutoFit = false;
            area2.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, FontStyle.Regular);
            //area2.AxisX.LabelStyle.IsEndLabelVisible = false;
            area2.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area2.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisX.IsStartedFromZero = false;
            area2.AxisX.RoundAxisValues();

            area2.AxisY.IsLabelAutoFit = false;
            area2.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area2.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisY.IsStartedFromZero = false;

            area2.AxisY2.IsLabelAutoFit = false;
            area2.AxisY2.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area2.AxisY2.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisY2.MajorGrid.LineColor = Color.FromArgb(15, 15, 15, 15);
            area2.AxisY2.IsStartedFromZero = false;

            area2.BackSecondaryColor = System.Drawing.Color.White;
            area2.BackGradientStyle = GradientStyle.TopBottom;
            area2.BorderColor = Color.FromArgb(64, 64, 64, 64);
            area2.BorderDashStyle = ChartDashStyle.Solid;
            area2.Position.Auto = false;
            area2.Position.Height = 35F;
            area2.Position.Width = 88F;
            area2.Position.X = 3F;
            area2.Position.Y = 60F;
            area2.ShadowColor = Color.Transparent;
            area2.Name = "Area2";
            chart1.ChartAreas.Add(area2);

            //chart1.Dock = DockStyle.Fill;
            chart1.Invalidate();
        }
        public static void MyChart3(Chart chart1, BindableCollection<Series> chartSeries, string chartTitle, string xLabel, string yLabel, ChartBackgroundColor backgroundColor, params string[] y2Label)
        {
            if (chart1.ChartAreas.Count < 1)
            {
                ChartArea area1 = new ChartArea();
                ChartArea area2 = new ChartArea();
                ChartArea area3 = new ChartArea();
                ChartStyle3(chart1, area1, area2, area3, backgroundColor);
            }

            if (chartTitle != "")
                chart1.Titles.Add(chartTitle);
            chart1.ChartAreas[2].AxisX.Title = xLabel;
            chart1.ChartAreas[0].AxisY.Title = yLabel;

            if (y2Label.Length > 0)
                chart1.ChartAreas[1].AxisY.Title = y2Label[0];
            if (y2Label.Length > 1)
                chart1.ChartAreas[2].AxisY.Title = y2Label[1];

            List<int> numSeries = new List<int>();
            if (chartSeries.Count > 1)
            {
                for (int i = 0; i < chartSeries.Count; i++)
                {
                    int a1 = (int)chartSeries[i].Tag - 1;
                    numSeries.Add(a1);
                    chartSeries[i].ChartArea = chart1.ChartAreas[a1].Name;
                    chart1.Series.Add(chartSeries[i]);
                }
            }


            int n1 = (numSeries.Where(x => x == 0)).Count();
            int n2 = (numSeries.Where(x => x == 1)).Count();
            int n3 = (numSeries.Where(x => x == 2)).Count();

            if (n1 > 1)
            {
                Legend legend = new Legend();
                legend.Font = new System.Drawing.Font("Trebuchet MS", 7.0F, FontStyle.Regular);
                legend.BackColor = Color.Transparent;
                legend.AutoFitMinFontSize = 5;
                legend.LegendStyle = LegendStyle.Column;

                legend.IsDockedInsideChartArea = true;
                legend.Docking = Docking.Left;
                legend.InsideChartArea = chart1.ChartAreas[0].Name;
                chart1.Legends.Add(legend);
            }

            if (n2 > 1)
            {
                Legend legend = new Legend();
                legend.Font = new System.Drawing.Font("Trebuchet MS", 7.0F, FontStyle.Regular);
                legend.BackColor = Color.Transparent;
                legend.AutoFitMinFontSize = 5;
                legend.LegendStyle = LegendStyle.Column;

                legend.IsDockedInsideChartArea = true;
                legend.Docking = Docking.Left;
                legend.InsideChartArea = chart1.ChartAreas[1].Name;
                chart1.Legends.Add(legend);
            }

            if (n3 > 1)
            {
                Legend legend = new Legend();
                legend.Font = new System.Drawing.Font("Trebuchet MS", 7.0F, FontStyle.Regular);
                legend.BackColor = Color.Transparent;
                legend.AutoFitMinFontSize = 5;
                legend.LegendStyle = LegendStyle.Column;

                legend.IsDockedInsideChartArea = true;
                legend.Docking = Docking.Left;
                legend.InsideChartArea = chart1.ChartAreas[2].Name;
                chart1.Legends.Add(legend);
            }
        }
        public static void ChartStyle3(Chart chart1, ChartArea area, ChartArea area2, ChartArea area3, ChartBackgroundColor backgroundColor)
        {
            int r1 = 211;
            int g1 = 223;
            int b1 = 240;
            int r2 = 26;
            int g2 = 59;
            int b2 = 105;
            int r3 = 165;
            int g3 = 191;
            int b3 = 228;

            switch (backgroundColor)
            {
                case ChartBackgroundColor.Blue:
                    chart1.BackColor = Color.FromArgb(r1, g1, b1);
                    chart1.BorderlineColor = Color.FromArgb(r2, g2, b2);
                    area.BackColor = Color.FromArgb(64, r3, g3, b3);
                    area2.BackColor = Color.FromArgb(64, r3, g3, b3);
                    area3.BackColor = Color.FromArgb(64, r3, g3, b3);
                    break;
                case ChartBackgroundColor.Green:
                    chart1.BackColor = Color.FromArgb(g1, b1, r1);
                    chart1.BorderlineColor = Color.FromArgb(g2, b2, r2);
                    area.BackColor = Color.FromArgb(64, g3, b3, r3);
                    area2.BackColor = Color.FromArgb(64, g3, b3, r3);
                    area3.BackColor = Color.FromArgb(64, g3, b3, r3);
                    break;
                case ChartBackgroundColor.Red:
                    chart1.BackColor = Color.FromArgb(b1, r1, g1);
                    chart1.BorderlineColor = Color.FromArgb(b2, r2, g2);
                    area.BackColor = Color.FromArgb(64, b3, r3, g3);
                    area2.BackColor = Color.FromArgb(64, b3, r3, g3);
                    area3.BackColor = Color.FromArgb(64, b3, r3, g3);
                    break;
                case ChartBackgroundColor.White:
                    chart1.BackColor = Color.White;
                    chart1.BorderlineColor = Color.White;
                    area.BackColor = Color.White;
                    area2.BackColor = Color.White;
                    area3.BackColor = Color.White;
                    break;
            }

            if (backgroundColor != ChartBackgroundColor.White)
            {
                chart1.BackSecondaryColor = Color.White;
                chart1.BackGradientStyle = GradientStyle.TopBottom;
                chart1.BorderlineDashStyle = ChartDashStyle.Solid;
                chart1.BorderlineWidth = 2;
                chart1.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

                area.Area3DStyle.IsClustered = true;
                area.Area3DStyle.Perspective = 10;
                area.Area3DStyle.IsRightAngleAxes = false;
                area.Area3DStyle.WallWidth = 0;
                area.Area3DStyle.Inclination = 15;
                area.Area3DStyle.Rotation = 10;
            }

            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, FontStyle.Regular);
            //area.AxisX.LabelStyle.IsEndLabelVisible = false;
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisX.IsStartedFromZero = false;
            area.AxisX.RoundAxisValues();

            area.AxisY.IsLabelAutoFit = false;
            area.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area.AxisY.IsStartedFromZero = false;

            area.BackSecondaryColor = System.Drawing.Color.White;
            area.BackGradientStyle = GradientStyle.TopBottom;
            area.BorderColor = Color.FromArgb(64, 64, 64, 64);
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.Position.Auto = false;
            area.Position.Height = 40F;
            area.Position.Width = 88F;
            area.Position.X = 3F;
            area.Position.Y = 6F;
            area.ShadowColor = Color.Transparent;
            area.Name = "Area1";
            chart1.ChartAreas.Add(area);





            area2.AlignWithChartArea = "Area1";
            area2.Area3DStyle.IsClustered = true;
            area2.Area3DStyle.Perspective = 10;
            area2.Area3DStyle.IsRightAngleAxes = false;
            area2.Area3DStyle.WallWidth = 0;
            area2.Area3DStyle.Inclination = 15;
            area2.Area3DStyle.Rotation = 10;

            area2.AxisX.IsLabelAutoFit = false;
            area2.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, FontStyle.Regular);
            //area2.AxisX.LabelStyle.IsEndLabelVisible = false;
            area2.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area2.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisX.IsStartedFromZero = false;
            area2.AxisX.RoundAxisValues();

            area2.AxisY.IsLabelAutoFit = false;
            area2.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area2.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area2.AxisY.IsStartedFromZero = false;

            area2.BackSecondaryColor = System.Drawing.Color.White;
            area2.BackGradientStyle = GradientStyle.TopBottom;
            area2.BorderColor = Color.FromArgb(64, 64, 64, 64);
            area2.BorderDashStyle = ChartDashStyle.Solid;
            area2.Position.Auto = false;
            area2.Position.Height = 26F;
            area2.Position.Width = 88F;
            area2.Position.X = 3F;
            area2.Position.Y = 46F;
            area2.ShadowColor = Color.Transparent;
            area2.Name = "Area2";
            chart1.ChartAreas.Add(area2);







            area3.AlignWithChartArea = "Area1";
            area3.Area3DStyle.IsClustered = true;
            area3.Area3DStyle.Perspective = 10;
            area3.Area3DStyle.IsRightAngleAxes = false;
            area3.Area3DStyle.WallWidth = 0;
            area3.Area3DStyle.Inclination = 15;
            area3.Area3DStyle.Rotation = 10;

            area3.AxisX.IsLabelAutoFit = false;
            area3.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, FontStyle.Regular);
            //area2.AxisX.LabelStyle.IsEndLabelVisible = false;
            area3.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area3.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            area3.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area3.AxisX.IsStartedFromZero = false;
            area3.AxisX.RoundAxisValues();

            area3.AxisY.IsLabelAutoFit = false;
            area3.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 7.25F, System.Drawing.FontStyle.Regular);
            area3.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            area3.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            area3.AxisY.IsStartedFromZero = false;

            area3.BackSecondaryColor = System.Drawing.Color.White;
            area3.BackGradientStyle = GradientStyle.TopBottom;
            area3.BorderColor = Color.FromArgb(64, 64, 64, 64);
            area3.BorderDashStyle = ChartDashStyle.Solid;
            area3.Position.Auto = false;
            area3.Position.Height = 26F;
            area3.Position.Width = 88F;
            area3.Position.X = 3F;
            area3.Position.Y = 72F;
            area3.ShadowColor = Color.Transparent;
            area3.Name = "Area3";
            chart1.ChartAreas.Add(area3);

            //chart1.Dock = DockStyle.Fill;
            chart1.Invalidate();
        }

        #endregion

        public static DataTable IndicatorChart(Chart chart1, BindableCollection<StockPrice> hlocvData, FinancialFormula formula, BindableCollection<int> periods, ChartBackgroundColor backgroundColor)
        {
            if (formula == FinancialFormula.ExponentialMovingAverage || formula == FinancialFormula.MovingAverage ||
                formula == FinancialFormula.TriangularMovingAverage || formula == FinancialFormula.WeightedMovingAverage ||
                formula == FinancialFormula.Envelopes ||
                formula == FinancialFormula.BollingerBands || formula == FinancialFormula.MedianPrice ||
                formula == FinancialFormula.WeightedClose || formula == FinancialFormula.TypicalPrice)
                return MovingAverage(chart1, hlocvData, formula, periods, backgroundColor);
            else
                return Indicators(chart1, hlocvData, formula, periods, backgroundColor);
        }
        private static DataTable Indicators(Chart chart1, BindableCollection<StockPrice> hlocvData, FinancialFormula formula, BindableCollection<int> periods, ChartBackgroundColor backgroundColor)
        {
            DataTable res = new DataTable();
            ChartArea area = new ChartArea();
            ChartArea area2 = new ChartArea();
            ChartArea area3 = new ChartArea();
            ChartStyle3(chart1, area, area2, area3, backgroundColor);
            chart1.Titles.Add(hlocvData[0].Ticker + ": " + formula.ToString());
            //chart1.Titles.Add(formula.ToString());
            chart1.Titles[0].Docking = Docking.Top;
            chart1.ChartAreas[2].AxisX.Title = "Date";
            chart1.ChartAreas[0].AxisY.Title = "Price";
            chart1.ChartAreas[1].AxisY.Title = "Signal";
            chart1.ChartAreas[2].AxisY.Title = "Volume";

            chart1.DataSource = hlocvData;
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Candlestick;
            ds.XValueType = ChartValueType.Date;
            ds["OpenCloseStyle"] = "Triangle";
            ds["ShowOpenClose"] = "Both";
            ds["PointWidth"] = "0.6";
            ds["PriceUpColor"] = "Green";
            ds["PriceDownColor"] = "Red";
            ds.XValueMember = "Date";
            ds.YValueMembers = "PriceHigh, PriceLow, PriceOpen, PriceClose";
            ds.Name = "Series1";
            ds.IsXValueIndexed = true;
            chart1.Series.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Column;
            ds["PointWidth"] = "0.6";
            ds.XValueType = ChartValueType.Date;
            ds.XValueMember = "Date";
            ds.YValueMembers = "Volume";
            ds.Name = "Series3";
            ds.ChartArea = area3.Name;
            ds.Color = System.Drawing.Color.DarkGreen;
            chart1.Series.Add(ds);
            ds.IsXValueIndexed = true;
            chart1.DataBind();

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueType = ChartValueType.Date;
            ds.XValueMember = "Date";
            ds.Name = "Series2";
            ds.Color = System.Drawing.Color.DarkBlue;
            ds.IsXValueIndexed = true;
            ds.ChartArea = area2.Name;

            foreach (var p in hlocvData)
                ds.Points.AddXY(p.Date, 0);
            chart1.Series.Add(ds);

            string s1 = string.Empty;
            string s2 = string.Empty;
            string s3 = string.Empty;

            switch (formula)
            {
                case FinancialFormula.WilliamsR:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.VolumeOscillator:
                    s1 = string.Format("{0},{1},true", periods[0], periods[1]);
                    s2 = "Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.VolatilityChaikins:
                    s1 = string.Format("{0},{1},true", periods[0], periods[1]);
                    s2 = "Series1:Y,Series1:Y2";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.StochasticIndicator:
                    ds = new Series();
                    ds.ChartType = SeriesChartType.Line;
                    ds.XValueType = ChartValueType.Date;
                    ds.XValueMember = "Date";
                    ds.Name = "Series4";
                    ds.Color = System.Drawing.Color.DarkRed;
                    ds.IsXValueIndexed = true;
                    ds.ChartArea = area2.Name;
                    foreach (var p in hlocvData)
                    {
                        ds.Points.AddXY(p.Date, 0);
                    }
                    chart1.Series.Add(ds);
                    s1 = string.Format("{0},{1}", periods[0], periods[1]);
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4";
                    s3 = "Series2:Y,Series4:Y";
                    break;
                case FinancialFormula.StandardDeviation:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.RelativeStrengthIndex:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.RateOfChange:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.PriceVolumeTrend:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.OnBalanceVolume:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.PositiveVolumeIndex:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.NegativeVolumeIndex:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.MovingAverageConvergenceDivergence:
                    s1 = string.Format("{0},{1}", periods[0], periods[1]);
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.MoneyFlow:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.MassIndex:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.EaseOfMovement:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.DetrendedPriceOscillator:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.CommodityChannelIndex:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.ChaikinOscillator:
                    s1 = string.Format("{0},{1}", periods[0], periods[1]);
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.AverageTrueRange:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.AccumulationDistribution:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4,Series3:Y";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.TripleExponentialMovingAverage:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.Performance:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
            }

            chart1.DataManipulator.IsStartFromFirst = true;
            chart1.DataManipulator.FinancialFormula(formula, s1, s2, s3);

            int n0 = chart1.Series["Series1"].Points.Count;
            int n1 = chart1.Series["Series2"].Points.Count;
            for (int i = 0; i < n0 - n1; i++)
            {
                chart1.Series["Series1"].Points.RemoveAt(0);
                chart1.Series["Series3"].Points.RemoveAt(0);
            }

            chart1.Series["Series1"].IsXValueIndexed = true;
            chart1.Series["Series2"].IsXValueIndexed = true;
            chart1.Series["Series3"].IsXValueIndexed = true;


            DataSet dataSet1 = chart1.DataManipulator.ExportSeriesValues("Series2");
            res = dataSet1.Tables[0];
            if (formula == FinancialFormula.StochasticIndicator)
            {
                chart1.Series["Series4"].IsXValueIndexed = true;
                DataSet dataSet2 = chart1.DataManipulator.ExportSeriesValues("Series4");
                res = new DataTable();
                res.Columns.Add("Date", typeof(DateTime));
                res.Columns.Add("PercentK", typeof(double));
                res.Columns.Add("PercentD", typeof(double));

                for (int i = 0; i < dataSet1.Tables[0].Rows.Count; i++)
                {
                    res.Rows.Add(dataSet1.Tables[0].Rows[i][0], dataSet1.Tables[0].Rows[i][1], dataSet2.Tables[0].Rows[i][1]);
                }
            }

            return res;
        }
        private static DataTable MovingAverage(Chart chart1, BindableCollection<StockPrice> hlocvData, FinancialFormula formula, BindableCollection<int> periods, ChartBackgroundColor backgroundColor)
        {
            DataTable res = new DataTable();
            ChartArea area = new ChartArea();
            ChartArea area2 = new ChartArea();
            ChartStyle2(chart1, area, area2, backgroundColor);

            chart1.Titles.Add(hlocvData[0].Ticker + ": " + formula.ToString());
            chart1.Titles[0].Docking = Docking.Top;
            chart1.ChartAreas[1].AxisX.Title = "Date";
            chart1.ChartAreas[0].AxisY.Title = "Price";
            chart1.ChartAreas[1].AxisY.Title = "Volume";

            chart1.DataSource = hlocvData;
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Candlestick;
            ds.XValueType = ChartValueType.Date;
            ds["OpenCloseStyle"] = "Triangle";
            ds["ShowOpenClose"] = "Both";
            ds["PointWidth"] = "0.6";
            ds["PriceUpColor"] = "Green";
            ds["PriceDownColor"] = "Red";
            ds.XValueMember = "Date";
            ds.YValueMembers = "PriceHigh, PriceLow, PriceOpen, PriceClose";
            ds.Name = "Series1";
            ds.IsXValueIndexed = true;
            chart1.Series.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Column;
            ds["PointWidth"] = "0.6";
            ds.XValueType = ChartValueType.Date;
            ds.XValueMember = "Date";
            ds.YValueMembers = "Volume";
            ds.Name = "Series3";
            ds.ChartArea = area2.Name;
            ds.Color = System.Drawing.Color.DarkGreen;
            chart1.Series.Add(ds);
            ds.IsXValueIndexed = true;
            chart1.DataBind();

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueType = ChartValueType.Date;
            ds.XValueMember = "Date";
            ds.Name = "Series2";
            ds.Color = System.Drawing.Color.DarkBlue;
            ds.ChartArea = area.Name;
            ds.IsXValueIndexed = true;
            foreach (var p in hlocvData)
                ds.Points.AddXY(p.Date, 0);
            chart1.Series.Add(ds);


            string s1 = string.Empty;
            string s2 = string.Empty;
            string s3 = string.Empty;

            switch (formula)
            {
                case FinancialFormula.ExponentialMovingAverage:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.MovingAverage:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.TriangularMovingAverage:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.WeightedMovingAverage:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                /*case FinancialFormula.TripleExponentialMovingAverage:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y";
                    break;*/
                case FinancialFormula.Envelopes:
                    ds = new Series();
                    ds.ChartType = SeriesChartType.Line;
                    ds.XValueType = ChartValueType.Date;
                    ds.XValueMember = "Date";
                    ds.Name = "Series4";
                    ds.Color = System.Drawing.Color.DarkBlue;
                    ds.ChartArea = area.Name;
                    ds.IsXValueIndexed = true;
                    foreach (var p in hlocvData)
                        ds.Points.AddXY(p.Date, 0);
                    chart1.Series.Add(ds);

                    s1 = string.Format("{0},{1}", periods[0], periods[1]);
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y,Series4:Y";
                    break;
                case FinancialFormula.BollingerBands:
                    ds = new Series();
                    ds.ChartType = SeriesChartType.Line;
                    ds.XValueType = ChartValueType.Date;
                    ds.XValueMember = "Date";
                    ds.Name = "Series4";
                    ds.Color = System.Drawing.Color.DarkBlue;
                    ds.ChartArea = area.Name;
                    ds.IsXValueIndexed = true;
                    foreach (var p in hlocvData)
                        ds.Points.AddXY(p.Date, 0);
                    chart1.Series.Add(ds);

                    s1 = string.Format("{0},{1}", periods[0], periods[1]);
                    s2 = "Series1:Y4";
                    s3 = "Series2:Y,Series4:Y";
                    break;
                case FinancialFormula.MedianPrice:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.TypicalPrice:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4";
                    s3 = "Series2:Y";
                    break;
                case FinancialFormula.WeightedClose:
                    s1 = periods[0].ToString();
                    s2 = "Series1:Y,Series1:Y2,Series1:Y4";
                    s3 = "Series2:Y";
                    break;
            }

            chart1.DataManipulator.IsStartFromFirst = true;
            chart1.DataManipulator.FinancialFormula(formula, s1, s2, s3);

            int n0 = chart1.Series["Series1"].Points.Count;
            int n1 = chart1.Series["Series2"].Points.Count;
            for (int i = 0; i < n0 - n1; i++)
            {
                chart1.Series["Series1"].Points.RemoveAt(0);
                chart1.Series["Series3"].Points.RemoveAt(0);
            }

            chart1.Series["Series1"].IsXValueIndexed = true;
            chart1.Series["Series2"].IsXValueIndexed = true;
            chart1.Series["Series3"].IsXValueIndexed = true;


            DataSet dataSet1 = chart1.DataManipulator.ExportSeriesValues("Series2");
            res = dataSet1.Tables[0];

            if (formula == FinancialFormula.Envelopes || formula == FinancialFormula.BollingerBands)
            {
                chart1.Series["Series4"].IsXValueIndexed = true;
                DataSet dataSet2 = chart1.DataManipulator.ExportSeriesValues("Series4");
                res = new DataTable();
                res.Columns.Add("Date", typeof(DateTime));
                res.Columns.Add("Top", typeof(double));
                res.Columns.Add("Bottom", typeof(double));

                for (int i = 0; i < dataSet1.Tables[0].Rows.Count; i++)
                {
                    res.Rows.Add(dataSet1.Tables[0].Rows[i][0], dataSet1.Tables[0].Rows[i][1], dataSet2.Tables[0].Rows[i][1]);
                }
            }
            return res;
        }

    }
}