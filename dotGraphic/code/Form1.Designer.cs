namespace DotGraphic
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel1 = new System.Windows.Forms.Panel();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.fpSpread1 = new FarPoint.Win.Spread.FpSpread();
            this.fpSpread1_Sheet1 = new FarPoint.Win.Spread.SheetView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.fpSpread2 = new FarPoint.Win.Spread.FpSpread();
            this.fpSpread2_Sheet1 = new FarPoint.Win.Spread.SheetView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.radafterRehabilitation = new System.Windows.Forms.RadioButton();
            this.radNoRehabilitation = new System.Windows.Forms.RadioButton();
            this.radBeforeRehabilitation = new System.Windows.Forms.RadioButton();
            this.panel9 = new System.Windows.Forms.Panel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.radtdx = new System.Windows.Forms.RadioButton();
            this.rad163 = new System.Windows.Forms.RadioButton();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chkCandlestick = new System.Windows.Forms.CheckBox();
            this.rdoHighLow = new System.Windows.Forms.RadioButton();
            this.rdoClose = new System.Windows.Forms.RadioButton();
            this.panel7 = new System.Windows.Forms.Panel();
            this.chkOneDotRebuild = new System.Windows.Forms.CheckBox();
            this.rdoFive = new System.Windows.Forms.RadioButton();
            this.rdoOne = new System.Windows.Forms.RadioButton();
            this.rdoThree = new System.Windows.Forms.RadioButton();
            this.panel8 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.chkIsDateArea = new System.Windows.Forms.CheckBox();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.btnUpdateStockList = new System.Windows.Forms.Button();
            this.lblRecommendGridInterval = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtStockFilter = new System.Windows.Forms.TextBox();
            this.chkIsTest = new System.Windows.Forms.CheckBox();
            this.numLatticeValue = new System.Windows.Forms.NumericUpDown();
            this.numNumOfKLine = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbCycle = new System.Windows.Forms.ComboBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.TabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread1_Sheet1)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread2_Sheet1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel9.SuspendLayout();
            this.panel10.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLatticeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumOfKLine)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.TabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 81);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1656, 631);
            this.panel1.TabIndex = 0;
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.tabPage1);
            this.TabControl1.Controls.Add(this.tabPage2);
            this.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl1.Location = new System.Drawing.Point(0, 0);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(1656, 631);
            this.TabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1648, 602);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "点数图";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.Controls.Add(this.progressBar1);
            this.panel3.Controls.Add(this.fpSpread1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1642, 596);
            this.panel3.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(191, 271);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1039, 34);
            this.progressBar1.TabIndex = 3;
            this.progressBar1.Visible = false;
            // 
            // fpSpread1
            // 
            this.fpSpread1.AccessibleDescription = "fpSpread1, 点数图, Row 0, Column 0, ";
            this.fpSpread1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fpSpread1.Location = new System.Drawing.Point(0, 0);
            this.fpSpread1.Name = "fpSpread1";
            this.fpSpread1.Sheets.AddRange(new FarPoint.Win.Spread.SheetView[] {
            this.fpSpread1_Sheet1});
            this.fpSpread1.Size = new System.Drawing.Size(1642, 596);
            this.fpSpread1.TabIndex = 2;
            this.fpSpread1.TabStripPlacement = FarPoint.Win.Spread.TabStripPlacement.Top;
            this.fpSpread1.TextTipPolicy = FarPoint.Win.Spread.TextTipPolicy.Floating;
            this.fpSpread1.TextTipFetch += new FarPoint.Win.Spread.TextTipFetchEventHandler(this.fpSpread1_TextTipFetch);
            // 
            // fpSpread1_Sheet1
            // 
            this.fpSpread1_Sheet1.Reset();
            this.fpSpread1_Sheet1.SheetName = "点数图";
            // Formulas and custom names must be loaded with R1C1 reference style
            this.fpSpread1_Sheet1.ReferenceStyle = FarPoint.Win.Spread.Model.ReferenceStyle.R1C1;
            this.fpSpread1_Sheet1.RowHeader.Columns.Default.Resizable = false;
            this.fpSpread1_Sheet1.ReferenceStyle = FarPoint.Win.Spread.Model.ReferenceStyle.A1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chart1);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1648, 602);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "K线图";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(3, 3);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "ChartArea1";
            series2.Color = System.Drawing.Color.Red;
            series2.CustomProperties = "PixelPointWidth=3";
            series2.Legend = "Legend1";
            series2.MarkerSize = 1;
            series2.Name = "Series2";
            series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(1642, 596);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.GetToolTipText += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs>(this.chart1_GetToolTipText);
            // 
            // fpSpread2
            // 
            this.fpSpread2.AccessibleDescription = "";
            this.fpSpread2.HorizontalScrollBarPolicy = FarPoint.Win.Spread.ScrollBarPolicy.Never;
            this.fpSpread2.Location = new System.Drawing.Point(536, 85);
            this.fpSpread2.Name = "fpSpread2";
            this.fpSpread2.Sheets.AddRange(new FarPoint.Win.Spread.SheetView[] {
            this.fpSpread2_Sheet1});
            this.fpSpread2.Size = new System.Drawing.Size(287, 304);
            this.fpSpread2.TabIndex = 16;
            this.fpSpread2.VerticalScrollBarPolicy = FarPoint.Win.Spread.ScrollBarPolicy.Never;
            this.fpSpread2.Visible = false;
            this.fpSpread2.CellDoubleClick += new FarPoint.Win.Spread.CellClickEventHandler(this.fpSpread2_CellDoubleClick);
            this.fpSpread2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fpSpread2_KeyDown);
            // 
            // fpSpread2_Sheet1
            // 
            this.fpSpread2_Sheet1.Reset();
            this.fpSpread2_Sheet1.SheetName = "Sheet1";
            // Formulas and custom names must be loaded with R1C1 reference style
            this.fpSpread2_Sheet1.ReferenceStyle = FarPoint.Win.Spread.Model.ReferenceStyle.R1C1;
            this.fpSpread2_Sheet1.RowHeader.ColumnCount = 0;
            this.fpSpread2_Sheet1.RowHeader.Columns.Default.Resizable = false;
            this.fpSpread2_Sheet1.ReferenceStyle = FarPoint.Win.Spread.Model.ReferenceStyle.A1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.panel8);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1656, 81);
            this.panel2.TabIndex = 1;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.White;
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Controls.Add(this.panel9);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(1118, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(538, 81);
            this.panel5.TabIndex = 17;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.radafterRehabilitation);
            this.panel6.Controls.Add(this.radNoRehabilitation);
            this.panel6.Controls.Add(this.radBeforeRehabilitation);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(348, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(190, 81);
            this.panel6.TabIndex = 16;
            // 
            // radafterRehabilitation
            // 
            this.radafterRehabilitation.AutoSize = true;
            this.radafterRehabilitation.Location = new System.Drawing.Point(20, 53);
            this.radafterRehabilitation.Name = "radafterRehabilitation";
            this.radafterRehabilitation.Size = new System.Drawing.Size(73, 19);
            this.radafterRehabilitation.TabIndex = 23;
            this.radafterRehabilitation.Text = "后复权";
            this.radafterRehabilitation.UseVisualStyleBackColor = true;
            // 
            // radNoRehabilitation
            // 
            this.radNoRehabilitation.AutoSize = true;
            this.radNoRehabilitation.Location = new System.Drawing.Point(20, 3);
            this.radNoRehabilitation.Name = "radNoRehabilitation";
            this.radNoRehabilitation.Size = new System.Drawing.Size(73, 19);
            this.radNoRehabilitation.TabIndex = 21;
            this.radNoRehabilitation.Text = "不复权";
            this.radNoRehabilitation.UseVisualStyleBackColor = true;
            // 
            // radBeforeRehabilitation
            // 
            this.radBeforeRehabilitation.AutoSize = true;
            this.radBeforeRehabilitation.Checked = true;
            this.radBeforeRehabilitation.Location = new System.Drawing.Point(20, 28);
            this.radBeforeRehabilitation.Name = "radBeforeRehabilitation";
            this.radBeforeRehabilitation.Size = new System.Drawing.Size(73, 19);
            this.radBeforeRehabilitation.TabIndex = 22;
            this.radBeforeRehabilitation.TabStop = true;
            this.radBeforeRehabilitation.Text = "前复权";
            this.radBeforeRehabilitation.UseVisualStyleBackColor = true;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.panel10);
            this.panel9.Controls.Add(this.panel4);
            this.panel9.Controls.Add(this.panel7);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel9.Location = new System.Drawing.Point(0, 0);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(348, 81);
            this.panel9.TabIndex = 19;
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.radtdx);
            this.panel10.Controls.Add(this.rad163);
            this.panel10.Controls.Add(this.lblDataSource);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel10.Location = new System.Drawing.Point(193, 0);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(155, 51);
            this.panel10.TabIndex = 16;
            // 
            // radtdx
            // 
            this.radtdx.AutoSize = true;
            this.radtdx.Location = new System.Drawing.Point(68, 27);
            this.radtdx.Name = "radtdx";
            this.radtdx.Size = new System.Drawing.Size(73, 19);
            this.radtdx.TabIndex = 28;
            this.radtdx.Text = "通达信";
            this.radtdx.UseVisualStyleBackColor = true;
            this.radtdx.CheckedChanged += new System.EventHandler(this.radtdx_CheckedChanged);
            // 
            // rad163
            // 
            this.rad163.AutoSize = true;
            this.rad163.Checked = true;
            this.rad163.Location = new System.Drawing.Point(68, 5);
            this.rad163.Name = "rad163";
            this.rad163.Size = new System.Drawing.Size(52, 19);
            this.rad163.TabIndex = 27;
            this.rad163.TabStop = true;
            this.rad163.Text = "163";
            this.rad163.UseVisualStyleBackColor = true;
            this.rad163.CheckedChanged += new System.EventHandler(this.rad163_CheckedChanged);
            // 
            // lblDataSource
            // 
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDataSource.ForeColor = System.Drawing.Color.Red;
            this.lblDataSource.Location = new System.Drawing.Point(3, 10);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(55, 15);
            this.lblDataSource.TabIndex = 26;
            this.lblDataSource.Text = "数据源";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.chkCandlestick);
            this.panel4.Controls.Add(this.rdoHighLow);
            this.panel4.Controls.Add(this.rdoClose);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(193, 51);
            this.panel4.TabIndex = 16;
            // 
            // chkCandlestick
            // 
            this.chkCandlestick.AutoSize = true;
            this.chkCandlestick.Location = new System.Drawing.Point(26, 30);
            this.chkCandlestick.Name = "chkCandlestick";
            this.chkCandlestick.Size = new System.Drawing.Size(74, 19);
            this.chkCandlestick.TabIndex = 16;
            this.chkCandlestick.Text = "蜡烛图";
            this.chkCandlestick.UseVisualStyleBackColor = true;
            // 
            // rdoHighLow
            // 
            this.rdoHighLow.AutoSize = true;
            this.rdoHighLow.Location = new System.Drawing.Point(104, 8);
            this.rdoHighLow.Name = "rdoHighLow";
            this.rdoHighLow.Size = new System.Drawing.Size(73, 19);
            this.rdoHighLow.TabIndex = 15;
            this.rdoHighLow.Text = "高低价";
            this.rdoHighLow.UseVisualStyleBackColor = true;
            // 
            // rdoClose
            // 
            this.rdoClose.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoClose.AutoSize = true;
            this.rdoClose.Checked = true;
            this.rdoClose.Location = new System.Drawing.Point(26, 8);
            this.rdoClose.Name = "rdoClose";
            this.rdoClose.Size = new System.Drawing.Size(73, 19);
            this.rdoClose.TabIndex = 14;
            this.rdoClose.TabStop = true;
            this.rdoClose.Text = "收盘价";
            this.rdoClose.UseVisualStyleBackColor = true;
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.White;
            this.panel7.Controls.Add(this.chkOneDotRebuild);
            this.panel7.Controls.Add(this.rdoFive);
            this.panel7.Controls.Add(this.rdoOne);
            this.panel7.Controls.Add(this.rdoThree);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel7.Location = new System.Drawing.Point(0, 51);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(348, 30);
            this.panel7.TabIndex = 18;
            // 
            // chkOneDotRebuild
            // 
            this.chkOneDotRebuild.AutoSize = true;
            this.chkOneDotRebuild.Checked = true;
            this.chkOneDotRebuild.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOneDotRebuild.Location = new System.Drawing.Point(261, 5);
            this.chkOneDotRebuild.Name = "chkOneDotRebuild";
            this.chkOneDotRebuild.Size = new System.Drawing.Size(89, 19);
            this.chkOneDotRebuild.TabIndex = 5;
            this.chkOneDotRebuild.Text = "单点重建";
            this.chkOneDotRebuild.UseVisualStyleBackColor = true;
            // 
            // rdoFive
            // 
            this.rdoFive.AutoSize = true;
            this.rdoFive.Location = new System.Drawing.Point(183, 6);
            this.rdoFive.Name = "rdoFive";
            this.rdoFive.Size = new System.Drawing.Size(73, 19);
            this.rdoFive.TabIndex = 4;
            this.rdoFive.Text = "五点图";
            this.rdoFive.UseVisualStyleBackColor = true;
            // 
            // rdoOne
            // 
            this.rdoOne.AutoSize = true;
            this.rdoOne.Checked = true;
            this.rdoOne.Location = new System.Drawing.Point(26, 6);
            this.rdoOne.Name = "rdoOne";
            this.rdoOne.Size = new System.Drawing.Size(73, 19);
            this.rdoOne.TabIndex = 2;
            this.rdoOne.TabStop = true;
            this.rdoOne.Text = "单点图";
            this.rdoOne.UseVisualStyleBackColor = true;
            // 
            // rdoThree
            // 
            this.rdoThree.AutoSize = true;
            this.rdoThree.Location = new System.Drawing.Point(104, 6);
            this.rdoThree.Name = "rdoThree";
            this.rdoThree.Size = new System.Drawing.Size(73, 19);
            this.rdoThree.TabIndex = 3;
            this.rdoThree.Text = "三点图";
            this.rdoThree.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.White;
            this.panel8.Controls.Add(this.label4);
            this.panel8.Controls.Add(this.label1);
            this.panel8.Controls.Add(this.chkIsDateArea);
            this.panel8.Controls.Add(this.dateTimePicker2);
            this.panel8.Controls.Add(this.dateTimePicker1);
            this.panel8.Controls.Add(this.btnUpdateStockList);
            this.panel8.Controls.Add(this.lblRecommendGridInterval);
            this.panel8.Controls.Add(this.label5);
            this.panel8.Controls.Add(this.txtStockFilter);
            this.panel8.Controls.Add(this.chkIsTest);
            this.panel8.Controls.Add(this.numLatticeValue);
            this.panel8.Controls.Add(this.numNumOfKLine);
            this.panel8.Controls.Add(this.label7);
            this.panel8.Controls.Add(this.label2);
            this.panel8.Controls.Add(this.label8);
            this.panel8.Controls.Add(this.label3);
            this.panel8.Controls.Add(this.cmbCycle);
            this.panel8.Controls.Add(this.btnGenerate);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel8.Location = new System.Drawing.Point(0, 0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(1118, 81);
            this.panel8.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(900, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 15);
            this.label4.TabIndex = 25;
            this.label4.Text = "到";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(900, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 15);
            this.label1.TabIndex = 24;
            this.label1.Text = "从";
            // 
            // chkIsDateArea
            // 
            this.chkIsDateArea.AutoSize = true;
            this.chkIsDateArea.Location = new System.Drawing.Point(841, 13);
            this.chkIsDateArea.Name = "chkIsDateArea";
            this.chkIsDateArea.Size = new System.Drawing.Size(59, 19);
            this.chkIsDateArea.TabIndex = 23;
            this.chkIsDateArea.Text = "时间";
            this.chkIsDateArea.UseVisualStyleBackColor = true;
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Location = new System.Drawing.Point(944, 51);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(151, 25);
            this.dateTimePicker2.TabIndex = 22;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(944, 10);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(151, 25);
            this.dateTimePicker1.TabIndex = 21;
            // 
            // btnUpdateStockList
            // 
            this.btnUpdateStockList.Location = new System.Drawing.Point(11, 3);
            this.btnUpdateStockList.Name = "btnUpdateStockList";
            this.btnUpdateStockList.Size = new System.Drawing.Size(75, 73);
            this.btnUpdateStockList.TabIndex = 20;
            this.btnUpdateStockList.Text = "更新股票数据";
            this.btnUpdateStockList.UseVisualStyleBackColor = true;
            this.btnUpdateStockList.Click += new System.EventHandler(this.btnUpdateStockList_Click);
            // 
            // lblRecommendGridInterval
            // 
            this.lblRecommendGridInterval.AutoSize = true;
            this.lblRecommendGridInterval.ForeColor = System.Drawing.Color.Red;
            this.lblRecommendGridInterval.Location = new System.Drawing.Point(104, 17);
            this.lblRecommendGridInterval.Name = "lblRecommendGridInterval";
            this.lblRecommendGridInterval.Size = new System.Drawing.Size(67, 15);
            this.lblRecommendGridInterval.TabIndex = 19;
            this.lblRecommendGridInterval.Text = "推荐格值";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(606, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 15);
            this.label5.TabIndex = 18;
            this.label5.Text = "规则说明";
            // 
            // txtStockFilter
            // 
            this.txtStockFilter.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtStockFilter.Location = new System.Drawing.Point(536, 54);
            this.txtStockFilter.Name = "txtStockFilter";
            this.txtStockFilter.Size = new System.Drawing.Size(137, 25);
            this.txtStockFilter.TabIndex = 17;
            this.txtStockFilter.TextChanged += new System.EventHandler(this.txtStockFilter_TextChanged);
            this.txtStockFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtStockFilter_KeyDown);
            // 
            // chkIsTest
            // 
            this.chkIsTest.AutoSize = true;
            this.chkIsTest.Location = new System.Drawing.Point(452, 13);
            this.chkIsTest.Name = "chkIsTest";
            this.chkIsTest.Size = new System.Drawing.Size(89, 19);
            this.chkIsTest.TabIndex = 5;
            this.chkIsTest.Text = "测试数据";
            this.chkIsTest.UseVisualStyleBackColor = true;
            // 
            // numLatticeValue
            // 
            this.numLatticeValue.DecimalPlaces = 2;
            this.numLatticeValue.Location = new System.Drawing.Point(144, 53);
            this.numLatticeValue.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numLatticeValue.Name = "numLatticeValue";
            this.numLatticeValue.Size = new System.Drawing.Size(77, 25);
            this.numLatticeValue.TabIndex = 15;
            this.numLatticeValue.ValueChanged += new System.EventHandler(this.numLatticeValue_ValueChanged);
            this.numLatticeValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numLatticeValue_KeyDown);
            // 
            // numNumOfKLine
            // 
            this.numNumOfKLine.Location = new System.Drawing.Point(313, 54);
            this.numNumOfKLine.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numNumOfKLine.Name = "numNumOfKLine";
            this.numNumOfKLine.Size = new System.Drawing.Size(120, 25);
            this.numNumOfKLine.TabIndex = 14;
            this.numNumOfKLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numNumOfKLine_KeyDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(449, 58);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 15);
            this.label7.TabIndex = 11;
            this.label7.Text = "股票过滤";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(101, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "格值";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(259, 59);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 15);
            this.label8.TabIndex = 13;
            this.label8.Text = "K线数";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(259, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "周期";
            // 
            // cmbCycle
            // 
            this.cmbCycle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCycle.FormattingEnabled = true;
            this.cmbCycle.Location = new System.Drawing.Point(313, 11);
            this.cmbCycle.Name = "cmbCycle";
            this.cmbCycle.Size = new System.Drawing.Size(120, 23);
            this.cmbCycle.TabIndex = 10;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(700, 3);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(123, 74);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "生成点数图";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1656, 712);
            this.Controls.Add(this.fpSpread2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "点数图";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.TabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread1_Sheet1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fpSpread2_Sheet1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel9.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            this.panel10.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLatticeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumOfKLine)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rdoFive;
        private System.Windows.Forms.RadioButton rdoThree;
        private System.Windows.Forms.RadioButton rdoOne;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbCycle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RadioButton rdoHighLow;
        private System.Windows.Forms.RadioButton rdoClose;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.NumericUpDown numLatticeValue;
        private System.Windows.Forms.NumericUpDown numNumOfKLine;
        private FarPoint.Win.Spread.FpSpread fpSpread1;
        private FarPoint.Win.Spread.SheetView fpSpread1_Sheet1;
        private System.Windows.Forms.CheckBox chkIsTest;
        private FarPoint.Win.Spread.FpSpread fpSpread2;
        private FarPoint.Win.Spread.SheetView fpSpread2_Sheet1;
        private System.Windows.Forms.TextBox txtStockFilter;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnUpdateStockList;
        private System.Windows.Forms.Label lblRecommendGridInterval;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.TabControl TabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.RadioButton radafterRehabilitation;
        private System.Windows.Forms.RadioButton radNoRehabilitation;
        private System.Windows.Forms.RadioButton radBeforeRehabilitation;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkIsDateArea;
        private System.Windows.Forms.Label lblDataSource;
        private System.Windows.Forms.CheckBox chkOneDotRebuild;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.RadioButton radtdx;
        private System.Windows.Forms.RadioButton rad163;
        private System.Windows.Forms.CheckBox chkCandlestick;
    }
}

