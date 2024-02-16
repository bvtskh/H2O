using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UMC_Hydroelectricity.Models;
using ZedGraph;

namespace UMC_Hydroelectricity
{
    public partial class FormChart : Form
    {
        List<double> WaterLevel = new List<double>();
        List<double> WaterFlow = new List<double>();
        List<string> TimeUpdate = new List<string>();
        double deadWaterLevel;
        public static int number=15;
        string name;
        public FormChart(List<double> WaterL, double deadWater, List<double> waterFlow, List<string> time, string name)
        {
            WaterLevel = WaterL;
            WaterFlow = waterFlow;
            deadWaterLevel = deadWater;
            TimeUpdate = time;
            this.name = name;
            InitializeComponent();
        }

        private void FormChart_Load(object sender, EventArgs e)
        {
            
            //txtNumber.Text = "15";
            // Calculate the date range for the last days
            dateTimePickerFrom.Value = dateTimePickerTo.Value.AddDays(-14);
            if (WaterLevel.Count <= 0 && TimeUpdate.Count <= 0)
            {
                return;
            }
            CreateBarChart(WaterLevel, WaterFlow, TimeUpdate);
            cbHydroelectric.SelectedIndex = -1;
            cbArea.SelectedIndex = -1;
            cbArea.Text = FormMain.Area;
            cbHydroelectric.Text = name;                                          
        }
        private void CreateBarChart(List<double> waterLevels, List<double> waterFlows, List<string> timeUpdate)
        {
            try
            {
                string convertname = "";
                GraphPane graphPane = zedGraphControl1.GraphPane;
                if (name == "Sơn La") convertname = "SonLa";
                if (name == "Hòa Bình") convertname = "HoaBinh";
                if (name == "Thác Bà") convertname = "ThacBa";
                if (name == "Tuyên Quang") convertname = "TuyenQuang";
                if (name == "Lai Châu") convertname = "LaiChau";
                if (name == "Bản Chát") convertname = "BanChat";
                if (name == "Huội Quảng") convertname = "HuoiQuang";
                if (name == "Trung Sơn") convertname = "TrungSon";
                if (name == "Bản Vẽ") convertname = "BanVe";
                if (name == "Quảng Trị") convertname = "QuangTri";
                if (name == "Sông Ba Hạ") convertname = "SongBaHa";
                if (name == "A Vương") convertname = "AVuong";
                if (name == "Sông Tranh 2") convertname = "SongTranh2";
                if (name == "Sông Hinh") convertname = "SongHinh";
                if (name == "Sông Bung 2") convertname = "SongBung2";
                if (name == "Sông Bung 4") convertname = "SongBung4";
                if (name == "Buôn Kuốp") convertname = "BuonKuop";
                if (name == "Srêpốk 3") convertname = "Srepok3";
                if (name == "An Khê") convertname = "AnKhe";
                if (name == "Kanak") convertname = "Kanak";
                if (name == "Thượng Kon Tum") convertname = "KonTum";
                if (name == "Sê San 3A") convertname = "SeSan3A";
                if (name == "Sê San 3") convertname = "SeSan3";
                if (name == "Sê San 4") convertname = "SeSan4";
                if (name == "Đồng Nai 3") convertname = "DongNai3";
                if (name == "Đồng Nai 4") convertname = "DongNai4";
                if (name == "Ialy") convertname = "Ialy";
                if (name == "Pleikrông") convertname = "Pleikrong";
                if (name == "Thác Mơ") convertname = "ThacMo";
                if (name == "Trị An") convertname = "TriAn";
                if (name == "Vĩnh Sơn A") convertname = "VinhSonA";
                if (name == "Vĩnh Sơn B") convertname = "VinhSonB";
                if (name == "Vĩnh Sơn C") convertname = "VinhSonC";
                if (name == "Buôn Tua Srah") convertname = "BuonTuaSrah";
                if (name == "Đại Ninh") convertname = "DaiNinh";
                if (name == "Đơn Dương") convertname = "DonDuong";
                if (name == "Hàm Thuận") convertname = "HamThuan";
                if (name == "Đa Mi") convertname = "DaMi";


                graphPane.Title.Text = convertname + "ダム水位、流入量(Mức nước hồ, Lưu lượng)";
                graphPane.XAxis.Title.Text = "測定時間/Measurement Time";
                graphPane.YAxis.Title.Text = "ダム水位/Water Level(m)";
                graphPane.Y2Axis.IsVisible = true;
                graphPane.Y2Axis.Title.Text = "流入量/Water Flow(m3/s)";

                // Create a list to hold the data points
                PointPairList flowData = new PointPairList();

                for (int i = 0; i < waterFlows.Count; i++)
                {
                    //levelData.Add(i + 1, waterLevels[i]);
                    flowData.Add(i + 1, waterFlows[i]);
                }
                // Create the line graph
                LineItem lines = graphPane.AddCurve("流入量/Lưu lượng", flowData, Color.DarkOrange, SymbolType.None);
                lines.Line.Width = 2;
                lines.Symbol.Fill = new Fill(Color.White);
                lines.IsY2Axis = true;
                
                // Show the Y2 axis
                graphPane.Y2Axis.IsVisible = true;
                // Adjust the scale range for the Y and Y2 axes
                graphPane.YAxis.Scale.Max = waterLevels.Max() + 5;
                graphPane.Y2Axis.Scale.Max = waterFlows.Max() + 50;

                //Create a list of points to hold the bar chart data
                PointPairList dataPoints = new PointPairList();
                for (int i = 0; i < waterLevels.Count; i++)
                {
                    // Add a new point for each day's water level
                    dataPoints.Add(i + 1, waterLevels[i]);
                }
                string[] labels = new string[timeUpdate.Count];
                for (int point = 0; point < timeUpdate.Count; point++)
                {
                    labels[point] = timeUpdate[point];
                }
                graphPane.XAxis.Scale.MinorStep = 0;
                graphPane.XAxis.Scale.MajorStep = 2;
                graphPane.XAxis.Scale.TextLabels = labels;
                graphPane.XAxis.Type = ZedGraph.AxisType.Text;
                graphPane.YAxis.Scale.Min = WaterLevel.Min() - (WaterLevel.Min() * 0.2);
                var max = WaterLevel.Max();
                graphPane.YAxis.Scale.Max = max + (max * 0.1);
                graphPane.YAxis.Scale.MajorStep = max * 0.05;

                BarItem bar = graphPane.AddBar("ダム水位/Mực nước", dataPoints, Color.Blue);

                // Fill water level values on top of each point
                for (int i = 0; i < dataPoints.Count; i++)
                {
                    TextObj text = new TextObj(waterLevels[i].ToString(), i + 1, waterLevels[i]);
                    text.Location.AlignH = AlignH.Center;
                    text.Location.AlignV = AlignV.Bottom;
                    text.FontSpec.Size = 10;
                    text.FontSpec.FontColor = Color.Black;
                    text.FontSpec.Border.IsVisible = false;
                    graphPane.GraphObjList.Add(text);
                }

                //Create a bar chart with the data points
                LineObj line = new LineObj(Color.Red, 0, deadWaterLevel, WaterLevel.Count + 1, deadWaterLevel);
                line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
                line.Line.Width = 2;
                graphPane.GraphObjList.Add(line);
                //create line Deadwater
                LineItem lineDeadwaterlevel = graphPane.AddCurve("死水レベル/Mực nước chết = " + deadWaterLevel, null, Color.Red, SymbolType.None);
                lineDeadwaterlevel.Line.Width = 2;
                lineDeadwaterlevel.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

                // Refresh the chart
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(txtNumber.Text)|| txtNumber.Text == "0") return;
            //number = int.Parse(txtNumber.Text);
            //FormMain fmain = new FormMain();
            //fmain.Getdata(name);
            //WaterLevel = FormMain.waterLevel;
            //deadWaterLevel = FormMain.deadWaterL;
            //WaterFlow = FormMain.waterFlow;
            //TimeUpdate = FormMain.timeUpdate;
            //zedGraphControl1.GraphPane.CurveList.Clear();
            //zedGraphControl1.GraphPane.GraphObjList.Clear();
            //zedGraphControl1.Refresh();
            //CreateBarChart(WaterLevel, WaterFlow, TimeUpdate);
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void cbArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var index = cbArea.SelectedIndex;
                if (cbArea.SelectedIndex == index)
                {
                    using (var ctx = new DBContext())
                    {
                        var filteredData = ctx.Tbl_Hydroelectric
                         .Where(x => x.AreaNumber == index + 1)
                         .GroupBy(x => x.HydroelectricName)
                         .Where(g => g.Count() > 1)
                         .Select(g => g.Key)
                         .ToList();
                        cbHydroelectric.DroppedDown = false;
                        // Clear the ComboBox items
                        cbHydroelectric.Items.Clear();
                        // Add the retrieved elements to the ComboBox
                        cbHydroelectric.Items.AddRange(filteredData.ToArray());
                        //cbHydroelectric.DroppedDown = true;
                        Cursor.Current = Cursors.Default;
                    }
                }
            }          
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void cbHydroelectric_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeData();
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            ChangeData();          
        }

        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            ChangeData();
        }
        private void ChangeData()
        {
            
            WaterLevel.Clear();
            WaterFlow.Clear();
            TimeUpdate.Clear();

            try
            {
                var hydroelectricIndexName = cbHydroelectric.SelectedItem == null ? name : cbHydroelectric.SelectedItem.ToString();
                using (var ctx = new DBContext())
                {
                    DateTime fromDay = dateTimePickerFrom.Value.Date;
                    DateTime toDay = dateTimePickerTo.Value.Date.AddDays(1);
                    DateTime currentDate = ctx.Tbl_Hydroelectric.Max(m => m.UpdateTime);
                    List<Tbl_Hydroelectric> tblSelect = ctx.Tbl_Hydroelectric.Where(w => w.HydroelectricName == hydroelectricIndexName && w.UpdateTime >= fromDay && w.UpdateTime < toDay).ToList();
                    if (tblSelect.Count <= 0)
                    {
                       
                        MessageBox.Show("No data found for this period", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        zedGraphControl1.GraphPane.CurveList.Clear();
                        zedGraphControl1.GraphPane.GraphObjList.Clear();
                        name = hydroelectricIndexName;
                        zedGraphControl1.Refresh();
                        return;
                    }
                    deadWaterLevel = ctx.Tbl_Hydroelectric.Where(w => w.HydroelectricName == hydroelectricIndexName && w.UpdateTime.Day == currentDate.Day && w.UpdateTime.Month == currentDate.Month && w.UpdateTime.Year == currentDate.Year).Select(s => s.DeadWaterLevel).FirstOrDefault();
                    List<Tbl_Hydroelectric> SortHydroelectric = tblSelect.OrderBy(o => o.UpdateTime).ToList();
                    foreach (var item in SortHydroelectric)
                    {
                        var waterL = item.WaterLevel;
                        WaterLevel.Add(waterL);
                    }
                    foreach (var item in SortHydroelectric)
                    {
                        var wFlow = item.WaterFlow;
                        WaterFlow.Add(wFlow);
                    }
                    foreach (var item in SortHydroelectric)
                    {
                        var timeU = item.UpdateTime.ToShortDateString();
                        DateTime date = DateTime.Parse(timeU);

                        string formattedDate = date.ToString("dd-MMM", System.Globalization.CultureInfo.InvariantCulture);
                        TimeUpdate.Add(formattedDate);
                    }
                }
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                name = hydroelectricIndexName;
                zedGraphControl1.Refresh();
                CreateBarChart(WaterLevel, WaterFlow, TimeUpdate);
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }
}
