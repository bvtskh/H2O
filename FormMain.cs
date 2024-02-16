using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UMC_Hydroelectricity.Models;
using ZedGraph;
using System.Resources;
using System.Diagnostics;
using UMC_Hydroelectricity.Control;
using System.Threading;

namespace UMC_Hydroelectricity
{
    public partial class FormMain : Form
    {
        public static List<double> waterLevel = new List<double>();
        public static List<double> waterFlow = new List<double>();
        public static List<string> timeUpdate = new List<string>();
        public static double deadWaterL;
        int numberArea = 0;
        public static string Area;
        public FormMain()
        {
            InitializeComponent();
            picMap.MouseMove += picMap_MouseMove;
            lbNote.MouseMove += lbNote_MouseMove;
            lbNote.MouseUp += lbNote_MouseUp;

        }

        private void lbNote_MouseUp(object sender, MouseEventArgs e)
        {
            if (name != null)
            {
                Getdata(name);
                FormChart fchart = new FormChart(waterLevel, deadWaterL, waterFlow, timeUpdate, name);
                fchart.Show();
            }
        }

        private void lbNote_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            cbAreas.SelectedIndex = 0;
            int stX = panel6.Location.X;
            int endX = stX + panel6.Width;
            int stY = panel6.Location.Y;

            int subX = (stX + endX) / 2;

            int lbX = lbName.Width / 2;
            int resultX = subX - lbX;
            lbName.Location = new Point(resultX, 20);
            lbNote.Visible = false;
            try
            {
                using (var ctx = new DBContext())
                {
                    var currentDate = ctx.Tbl_Hydroelectric.Max(m => m.UpdateTime);
                    var DataList = ctx.Tbl_Hydroelectric.Where(w => w.UpdateTime.Day == currentDate.Day && w.UpdateTime.Month == currentDate.Month && w.UpdateTime.Year == currentDate.Year && w.AreaNumber == 1).ToList();
                    if (DataList != null)
                    {
                        List<Tbl_Hydroelectric> sortedList = DataList.OrderBy(obj => obj.WaterLevel - (obj.DeadWaterLevel + (obj.DeadWaterLevel * 0.1))).ToList();
                        var NewList = new List<object>();
                        foreach (var item in sortedList)
                        {
                            object obj = new
                            {
                                HydroelectricName = item.HydroelectricName,
                                WaterLevel = item.WaterLevel,
                                DeadWaterLevel = item.DeadWaterLevel,
                                WaterFlow = item.WaterFlow
                            };
                            NewList.Add(obj);
                        }
                        dgvListHydroelectric.DataSource = NewList;
                        dgvListHydroelectric.AutoGenerateColumns = false;
                    }
                    else
                    {
                        dgvListHydroelectric.DataSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ImportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    Common.StartFormLoading();
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        XSSFWorkbook workbook = new XSSFWorkbook(fileStream);
                        ISheet sheet = workbook.GetSheetAt(0);
                        List<IRow> ListDataArea1 = new List<IRow>();
                        List<IRow> ListDataArea2 = new List<IRow>();
                        List<IRow> ListDataArea3 = new List<IRow>();
                        List<IRow> ListDataArea4 = new List<IRow>();
                        List<IRow> ListDataArea5 = new List<IRow>();

                        int currentGroup = -1; // Track the current group
                        for (int row = 0; row <= sheet.LastRowNum; row++)
                        {
                            IRow currentRow = sheet.GetRow(row);
                            var name = currentRow.GetCell(0).ToString();

                            // Determine the group based on the name
                            if (name.StartsWith("Đông Bắc Bộ") || name.StartsWith("Tây Bắc Bộ"))
                            {
                                currentGroup = 1;
                            }
                            else if (name.StartsWith("Bắc Trung Bộ"))
                            {
                                currentGroup = 2;
                            }
                            else if (name.StartsWith("Nam Trung Bộ"))
                            {
                                currentGroup = 3;
                            }
                            else if (name.StartsWith("Tây Nguyên"))
                            {
                                currentGroup = 4;
                            }
                            else if (name.StartsWith("Đông Nam Bộ"))
                            {
                                currentGroup = 5;
                            }
                            
                            // Add the row to the appropriate list based on the group
                            var dataRow = sheet.GetRow(row);
                            switch (currentGroup)
                            {
                                case 1:
                                    ListDataArea1.Add(dataRow);
                                    break;
                                case 2:
                                    ListDataArea2.Add(dataRow);
                                    break;
                                case 3:
                                    ListDataArea3.Add(dataRow);
                                    break;
                                case 4:
                                    ListDataArea4.Add(dataRow);
                                    break;
                                case 5:
                                    ListDataArea5.Add(dataRow);
                                    break;
                            }
                        }
                        SaveData(ListDataArea1, 1);
                        SaveData(ListDataArea2, 2);
                        SaveData(ListDataArea3, 3);
                        SaveData(ListDataArea4, 4);
                        SaveData(ListDataArea5, 5);
                    }
                    Thread.Sleep(200);
                    Common.CloseFormLoading();
                    MessageBox.Show("Completed!", "Notification", MessageBoxButtons.OK,MessageBoxIcon.Information);

                    return;
                }
            }
            catch (Exception ex)
            {
                Common.CloseFormLoading();
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void picMap_MouseUp(object sender, MouseEventArgs e)
        {
            //float percentageX = (float)e.X / picMap.Width * 100;
            //float percentageY = (float)e.Y / picMap.Height * 100;
            //var position = $"Position: X = {percentageX}%, Y = {percentageY}%";
            //MessageBox.Show(position, "Mouse Click Position", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (name != null)
            {
                Getdata(name);
                Area = cbAreas.SelectedItem.ToString();
                FormChart fchart = new FormChart(waterLevel, deadWaterL, waterFlow, timeUpdate, name);
                fchart.Show();
            }
        }
        private void picMap_MouseMove(object sender, MouseEventArgs e)
        {
            float percentageX = (float)e.X / picMap.Width * 100;
            float percentageY = (float)e.Y / picMap.Height * 100;
            int absoluteX = (int)percentageX + e.X - (int)(e.X * 0.08);
            int absoluteY = (int)percentageX + e.Y - (int)(e.Y * 0.1);
            if (name != null)
            {
                lbNote.Visible = true;
                lbNote.Text = name;
                lbNote.Location = new Point((int)absoluteX, (int)absoluteY);
            }
            else lbNote.Visible = false;
            // Call method to display information table
            DisplayCoordinatesInfo(percentageX, percentageY);
        }
        string name;
        private void DisplayCoordinatesInfo(float percentageX, float percentageY)
        {
            if (numberArea == 0)
            {
                if (name != null)
                {
                    Cursor.Current = Cursors.Hand;

                }

                if (percentageX >= 35.5 && percentageX <= 38 && percentageY >= 45.5 && percentageY <= 50.5)
                {
                    name = "Sơn La";
                }
                else if (percentageX >= 59.5 && percentageX <= 62 && percentageY >= 69.5 && percentageY <= 74.5)
                {
                    name = "Hòa Bình";
                }
                else if (percentageX >= 55 && percentageX <= 57.5 && percentageY >= 37 && percentageY <= 42)
                {
                    name = "Thác Bà";
                }
                else if (percentageX >= 61 && percentageX <= 63.5 && percentageY >= 16 && percentageY <= 21)
                {
                    name = "Tuyên Quang";
                }
                else if (percentageX >= 18 && percentageX <= 20.5 && percentageY >= 25.5 && percentageY <= 30.5)
                {
                    name = "Lai Châu";
                }
                else if (percentageX >= 32.5 && percentageX <= 35 && percentageY >= 31.5 && percentageY <= 36.5)
                {
                    name = "Bản Chát";
                }
                else if (percentageX >= 32 && percentageX <= 34.5 && percentageY >= 24 && percentageY <= 29)
                {
                    name = "Huội Quảng";
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    waterLevel.Clear();
                    waterFlow.Clear();
                    timeUpdate.Clear();
                    name = null;
                    return;
                }
            }
            else if (numberArea == 1)
            {
                if (name != null)
                {
                    Cursor.Current = Cursors.Hand;
                }

                if (percentageX >= 42.5 && percentageX <= 45 && percentageY >= 7.5 && percentageY <= 12.5)
                {
                    name = "Trung Sơn";
                }
                else if (percentageX >= 39 && percentageX <= 41.5 && percentageY >= 32 && percentageY <= 37)
                {
                    name = "Bản Vẽ";
                }
                else if (percentageX >= 66.5 && percentageX <= 69 && percentageY >= 81.5 && percentageY <= 86.5)
                {
                    name = "Quảng Trị";
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    waterLevel.Clear();
                    waterFlow.Clear();
                    timeUpdate.Clear();
                    name = null;
                    return;
                }
            }
            else if (numberArea == 2)
            {
                if (name != null)
                {
                    Cursor.Current = Cursors.Hand;
                }

                if (percentageX >= 64.5 && percentageX <= 67 && percentageY >= 77 && percentageY <= 82)
                {
                    name = "Sông Ba Hạ";
                }
                else if (percentageX >= 67.5 && percentageX <= 70 && percentageY >= 81 && percentageY <= 86)
                {
                    name = "Sông Hinh";
                }
                else if (percentageX >= 53.5 && percentageX <= 56 && percentageY >= 22 && percentageY <= 27)
                {
                    name = "Sông Tranh 2";
                }
                else if (percentageX >= 45 && percentageX <= 47.5 && percentageY >= 8.5 && percentageY <= 13.5)
                {
                    name = "A Vương";
                }
                else if (percentageX >= 43 && percentageX <= 45.5 && percentageY >= 14.5 && percentageY <= 19.5)
                {
                    name = "Sông Bung 2";
                }
                else if (percentageX >= 47 && percentageX <= 49.5 && percentageY >= 12 && percentageY <= 17)
                {
                    name = "Sông Bung 4";
                }
                else if (percentageX >= 59.5 && percentageX <= 62 && percentageY >= 50 && percentageY <= 55)
                {
                    name = "Vĩnh Sơn A";
                }
                else if (percentageX >= 60.5 && percentageX <= 63 && percentageY >= 44 && percentageY <= 49)
                {
                    name = "Vĩnh Sơn B";
                }
                else if (percentageX >= 57 && percentageX <= 59.5 && percentageY >= 44 && percentageY <= 49)
                {
                    name = "Vĩnh Sơn C";
                }

                else
                {
                    Cursor.Current = Cursors.Default;
                    waterLevel.Clear();
                    waterFlow.Clear();
                    timeUpdate.Clear();
                    name = null;
                    return;
                }
            }
            else if (numberArea == 3)
            {
                if (name != null)
                {
                    Cursor.Current = Cursors.Hand;
                }

                if (percentageX >= 66.5 && percentageX <= 69 && percentageY >= 81 && percentageY <= 86)
                {
                    name = "Đại Ninh";
                }
                else if (percentageX >= 62 && percentageX <= 64.5 && percentageY >= 89 && percentageY <= 94)
                {
                    name = "Hàm Thuận";
                }
                else if (percentageX >= 58.5 && percentageX <= 61 && percentageY >= 87.5 && percentageY <= 92.5)
                {
                    name = "Đa Mi";
                }
                else if (percentageX >= 61 && percentageX <= 63.5 && percentageY >= 78 && percentageY <= 83)
                {
                    name = "Đồng Nai 3";
                }
                else if (percentageX >= 57.5 && percentageX <= 60 && percentageY >= 75.5 && percentageY <= 80.5)
                {
                    name = "Đồng Nai 4";
                }
                else if (percentageX >= 70.5 && percentageX <= 73 && percentageY >= 76 && percentageY <= 81)
                {
                    name = "Đơn Dương";
                }
                else if (percentageX >= 63.5 && percentageX <= 66 && percentageY >= 68 && percentageY <= 73)
                {
                    name = "Buôn Tua Srah";
                }
                else if (percentageX >= 56 && percentageX <= 58.5 && percentageY >= 28.5 && percentageY <= 33.5)
                {
                    name = "Sê San 4";
                }
                else if (percentageX >= 62.5 && percentageX <= 65 && percentageY >= 61 && percentageY <= 66)
                {
                    name = "Buôn Kuốp";
                }
                else if (percentageX >= 60 && percentageX <= 62.5 && percentageY >= 59 && percentageY <= 64)
                {
                    name = "Srêpốk 3";
                }
                else if (percentageX >= 72.5 && percentageX <= 75 && percentageY >= 28 && percentageY <= 33)
                {
                    name = "An Khê";
                }
                else if (percentageX >= 69.5 && percentageX <= 72 && percentageY >= 24 && percentageY <= 29)
                {
                    name = "Kanak";
                }
                else if (percentageX >= 67.5 && percentageX <= 70 && percentageY >= 8.5 && percentageY <= 13.5)
                {
                    name = "Thượng Kon Tum";
                }
                else if (percentageX >= 57.5 && percentageX <= 60 && percentageY >= 26 && percentageY <= 31)
                {
                    name = "Sê San 3A";
                }
                else if (percentageX >= 60 && percentageX <= 62.5 && percentageY >= 23.5 && percentageY <= 28.5)
                {
                    name = "Sê San 3";
                }
                else if (percentageX >= 62 && percentageX <= 64.5 && percentageY >= 23.5 && percentageY <= 28.5)
                {
                    name = "Ialy";
                }
                else if (percentageX >= 58 && percentageX <= 60.5 && percentageY >= 20 && percentageY <= 25)
                {
                    name = "Pleikrông";
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    waterLevel.Clear();
                    waterFlow.Clear();
                    timeUpdate.Clear();
                    name = null;
                    return;
                }
            }
            else if (numberArea == 4)
            {
                if (name != null)
                {
                    Cursor.Current = Cursors.Hand;
                }

                if (percentageX >= 52 && percentageX <= 54.5 && percentageY >= 26 && percentageY <= 31)
                {
                    name = "Thác Mơ";
                }
                else if (percentageX >= 54 && percentageX <= 56.5 && percentageY >= 42 && percentageY <= 47)
                {
                    name = "Trị An";
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    waterLevel.Clear();
                    waterFlow.Clear();
                    timeUpdate.Clear();
                    name = null;
                    return;
                }
            }
            else
            {
                return;
            }
        }
        public void Getdata(string name)
        {
            try
            {
                waterLevel.Clear();
                waterFlow.Clear();
                timeUpdate.Clear();
                using (var ctx = new DBContext())
                {
                    // Calculate the date range for the last 30 days
                    DateTime currentDate = ctx.Tbl_Hydroelectric.Max(m => m.UpdateTime);
                    //Form
                    DateTime thirtyDaysAgo = currentDate.AddDays(-(FormChart.number-1));
                    List<Tbl_Hydroelectric> tblSelect = ctx.Tbl_Hydroelectric.Where(w => w.HydroelectricName == name && w.UpdateTime > thirtyDaysAgo && w.UpdateTime <= currentDate).ToList();

                    deadWaterL = ctx.Tbl_Hydroelectric.Where(w => w.HydroelectricName == name && w.UpdateTime.Day == currentDate.Day && w.UpdateTime.Month == currentDate.Month && w.UpdateTime.Year == currentDate.Year).Select(s => s.DeadWaterLevel).FirstOrDefault();

                    List<Tbl_Hydroelectric> SortHydroelectric = tblSelect.OrderBy(o => o.UpdateTime).ToList();

                    foreach (var item in SortHydroelectric)
                    {
                        var waterL = item.WaterLevel;
                        waterLevel.Add(waterL);
                    }
                    foreach (var item in SortHydroelectric)
                    {
                        var wFlow = item.WaterFlow;
                        waterFlow.Add(wFlow);
                    }
                    //List<string> sortedDates = new List<string>();
                    foreach (var item in SortHydroelectric)
                    {
                        var timeU = item.UpdateTime.ToShortDateString();
                        DateTime date = DateTime.Parse(timeU);

                        string formattedDate = date.ToString("dd-MMM", System.Globalization.CultureInfo.InvariantCulture);
                        timeUpdate.Add(formattedDate);
                    }
                    //List<DateTime> dates = sortedDates
                    //.Select(dateStr => DateTime.ParseExact(dateStr, "dd-MMM", null))
                    //.OrderBy(date => date)
                    //.ToList();
                    //foreach (var item in dates)
                    //{
                    //    var date = item.ToString("dd-MMM");
                    //    timeUpdate.Add(date);
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            Showdata();
        }

        private void dgvListHydroelectric_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 || e.ColumnIndex == 1 || e.ColumnIndex == 2)
            {

                var dataGridView = (DataGridView)sender;
                var row = dataGridView.Rows[e.RowIndex];
                var waterLV = (double)row.Cells[1].Value;
                var waterDead = (double)row.Cells[2].Value;

                var threshold = waterDead + (waterDead * 0.1); // Calculate 10% of value2

                if (waterLV > threshold)
                {
                    e.CellStyle.BackColor = Color.LimeGreen;
                    e.CellStyle.ForeColor = Color.White;
                }
                else if (waterLV < threshold && waterLV > waterDead)
                {
                    e.CellStyle.BackColor = Color.Yellow;
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (waterLV < waterDead)
                {
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.ForeColor = Color.White;
                }
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == 3)
            {
                var dataGridView = (DataGridView)sender;
                var row = dataGridView.Rows[e.RowIndex];
                e.CellStyle.BackColor = Color.WhiteSmoke;
                e.CellStyle.ForeColor = Color.Black;
            }
        }

        private void cbAreas_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                name = null;
                if (cbAreas.SelectedIndex == 0)
                {
                    picMap.BackgroundImage = Properties.Resources.m1;
                    numberArea = 0;
                }
                else if (cbAreas.SelectedIndex == 1)
                {
                    picMap.BackgroundImage = Properties.Resources.m2;
                    numberArea = 1;
                }
                else if (cbAreas.SelectedIndex == 2)
                {
                    picMap.BackgroundImage = Properties.Resources.m3;
                    numberArea = 2;
                }
                else if (cbAreas.SelectedIndex == 3)
                {
                    picMap.BackgroundImage = Properties.Resources.m4;
                    numberArea = 3;
                }
                else if (cbAreas.SelectedIndex == 4)
                {
                    picMap.BackgroundImage = Properties.Resources.m5;
                    numberArea = 4;
                }
                lbName.Text = cbAreas.SelectedItem.ToString() + " Viet Nam";
                int stX = panel6.Location.X;
                int endX = stX + panel6.Width;
                int stY = panel6.Location.Y;

                int subX = (stX + endX) / 2;

                int lbX = lbName.Width / 2;
                int resultX = subX - lbX;
                lbName.Location = new Point(resultX, 20);

                Showdata();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SaveData(List<IRow> listData, int AreaNumber)
        {
            try
            {
                foreach (var row in listData)
                {
                    string dateString;
                    var name = row.GetCell(0).ToString();
                    if (row.Cells[0].ToString() == "Đông Bắc Bộ" || row.Cells[0].ToString() == "Tây Bắc Bộ" || row.Cells[0].ToString() == "Bắc Trung Bộ" || row.Cells[0].ToString() == "Nam Trung Bộ" ||
                        row.Cells[0].ToString() == "Tây Nguyên" || row.Cells[0].ToString() == "Đông Nam Bộ")
                    {
                        continue;
                    }
                    if(row.GetCell(1) == null || row.GetCell(1).ToString() == "")
                    {
                        MessageBox.Show("Omitted a data because time cannot be empty", "",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                        continue;
                    }
                    var valuecell = row.GetCell(1).ToString();
                    string formats = "dd/MM HH:mm";
                    DateTime Celltime;

                    if (DateTime.TryParseExact(valuecell, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out Celltime))
                    {
                        dateString = Celltime.ToString("MM/dd/yyyy h:mm tt");
                    }
                    else
                    {
                        var date = row.GetCell(1).DateCellValue.ToString();
                        //DateTime dateTime = DateTime.Parse(date);
                        if (!DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out Celltime))
                        {
                            MessageBox.Show("Invalid time format");
                            return;
                        }
                        dateString = Celltime.ToString("dd/MM/yyyy h:mm tt");
                    }
                    DateTime convertDate = DateTime.Parse(dateString);

                    

                    var waterLevel = string.IsNullOrEmpty(row.GetCell(2).ToString()) ? "0" : row.GetCell(2).ToString();
                    var deadWaterLevel = string.IsNullOrEmpty(row.GetCell(4).ToString()) ? "0" : row.GetCell(4).ToString();
                    var waterFlow = string.IsNullOrEmpty(row.GetCell(5).ToString()) ? "0" : row.GetCell(5).ToString();
                    if (!IsDouble(waterLevel) || !IsDouble(deadWaterLevel) || !IsDouble(waterFlow))
                    {
                        Console.WriteLine("Omitted a data because Input value is incorrect, value must be numeric!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    using (var ctx = new DBContext())
                    {
                        Tbl_Hydroelectric tblHydroExist = ctx.Tbl_Hydroelectric.Where(w => w.HydroelectricName == name && w.UpdateTime.Day == convertDate.Day && w.UpdateTime.Month == convertDate.Month && w.UpdateTime.Year == convertDate.Year).FirstOrDefault();
                        if (tblHydroExist == null)
                        {
                            Tbl_Hydroelectric tblHydro = new Tbl_Hydroelectric();
                            tblHydro.HydroelectricName = name;
                            tblHydro.UpdateTime = convertDate;
                            tblHydro.WaterLevel = double.Parse(waterLevel);
                            tblHydro.DeadWaterLevel = double.Parse(deadWaterLevel);
                            tblHydro.WaterFlow = double.Parse(waterFlow);
                            tblHydro.AreaNumber = AreaNumber;

                            ctx.Tbl_Hydroelectric.Add(tblHydro);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            tblHydroExist.WaterLevel = double.Parse(waterLevel);
                            tblHydroExist.DeadWaterLevel = double.Parse(deadWaterLevel);
                            tblHydroExist.WaterFlow = double.Parse(waterFlow);
                            tblHydroExist.UpdateTime = convertDate;
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.CloseFormLoading();
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        private void Showdata()
        {
            try
            {
                using (var ctx = new DBContext())
                {

                    var searchDate = dateTimePicker1.Value.Date;
                    var DataList = ctx.Tbl_Hydroelectric.Where(w => w.UpdateTime.Day == searchDate.Day && w.UpdateTime.Month == searchDate.Month && w.UpdateTime.Year == searchDate.Year && w.AreaNumber == numberArea + 1).ToList();
                    
                    if (DataList != null)
                    {
                        List<Tbl_Hydroelectric> NGdata = new List<Tbl_Hydroelectric>();
                        foreach (var item in DataList)
                        {
                            if (item.WaterLevel < item.DeadWaterLevel)
                            {                              
                                NGdata.Add(item);
                            }
                        }

                        List<Tbl_Hydroelectric> sortedListNG = NGdata.OrderBy(obj => obj.WaterLevel < obj.DeadWaterLevel).ToList();                      
                        List<Tbl_Hydroelectric> sortedList = DataList.OrderBy(obj => obj.WaterLevel - (obj.DeadWaterLevel + (obj.DeadWaterLevel * 0.1))).ToList();
                        List<Tbl_Hydroelectric> ResultsSort = new List<Tbl_Hydroelectric>();
                        // Add elements from List1 to rearrangedList2
                        foreach (var element in sortedList)
                        {
                            if (sortedListNG.Contains(element))
                            {
                                ResultsSort.Add(element);
                            }
                        }
                        foreach(var element in sortedList)
                        {
                            if (ResultsSort.Contains(element)) continue;
                            ResultsSort.Add(element);
                        }

                        var NewList = new List<object>();
                        foreach (var item in ResultsSort)
                        {
                            object obj = new
                            {
                                HydroelectricName = item.HydroelectricName,
                                WaterLevel = item.WaterLevel,
                                DeadWaterLevel = item.DeadWaterLevel,
                                WaterFlow = item.WaterFlow
                            };
                            NewList.Add(obj);
                        }
                        dgvListHydroelectric.DataSource = NewList;
                        dgvListHydroelectric.AutoGenerateColumns = false;
                    }
                    else
                    {
                        dgvListHydroelectric.DataSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void picDownload_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void picDownload_Click(object sender, EventArgs e)
        {
            try
            {
                Form1 f1 = new Form1();
                f1.Show();
                //string url = "https://www.evn.com.vn/c3/thong-tin-ho-thuy-dien/Muc-nuoc-cac-ho-thuy-dien-117-123.aspx"; // Replace with your desired URL

                //try
                //{
                //    Process.Start(url);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("An error occurred while trying to open the web page: " + ex.Message);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static bool IsDouble(string input)
        {
            double result;
            return double.TryParse(input, out result);
        }
    }
}
