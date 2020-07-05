using FarPoint.Win.Spread;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static DotGraphic.Modes;
/// <summary>
/// 点数图思路:
///     1、读取通达信的数据(如,日线在 \zd_huatai\vipdoc\sh\sh000001.day)
///     2、依据选取的K线数,计算最大、最小值
///     3、以最小值得到格值,然后计算出行数
///     4、填充。
///         从第二个点开始处理(处理第二点时填充顺便第一个点),此后有五种情况:
///             a、前涨后涨,从上往下填,最后的点不填(坐标是反着的所以是从上往下,下同)
///             b、前跌后涨,从上往下填,最后的点与前跌水平
///             c、前涨后跌,从下往上填,最后的点与前涨水平
///             d、前跌后跌,从下往上填,最后的点点不填
///             e、不涨不跌,只覆盖当前点的悬停,O或X不填
///             
///     注意:控件的坐标是和常用的X-Y轴坐标反着的,所以要转换(函数getY),且填充时往下索引“+”,网上索引“-”
///     问题 1:格值转向中,三点图,是格子大小是1,3格转向;还是格子大小是3,一个转向???目前是后者.三点、五点图还有问题
///     （重点->转向规则:当前点所在列是涨,则其上下波动不超过dotSpace则一直不换列,所以比然有相邻转角点的价格差>=dotSpace）
///     问题 2:点数图是傅里叶变换吗?这就是把价格的变化频率和幅度突出,而不关注时间
///     问题 3:复权的问题怎么解决???
/// </summary>
namespace DotGraphic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string[] inParams = new string[10];

        /// <summary>
        /// 股票原始周期数据列表
        /// </summary>
        List<CycleData> CycleDataList = new List<CycleData>();
        /// <summary>
        /// 股票填充作图数据
        /// </summary>
        List<CycleData> CycleDataListNow = new List<CycleData>();
        List<OperateStockInfo> stockList = new List<OperateStockInfo>();
        /// <summary>
        /// 当前操作股票的基本信息
        /// </summary>
        OperateStockInfo currentOperateStockInfo = new OperateStockInfo();
        /// <summary>
        /// 当前周期
        /// </summary>
       // CycleType currentCycleType = CycleType.日;
        /// <summary>
        /// 使用K线数量
        /// </summary>
        int numOfKLine = 500;
        /// <summary>
        /// 收盘价最高
        /// </summary>
        double maxPriceClose = 0;
        double minPriceClose = 0;
        /// <summary>
        /// 最高高价
        /// </summary>
        double maxPriceHigh = 0;
        /// <summary>
        /// 最低低价
        /// </summary>
        double minPriceLow = 0;
        double maxPrice = 0;
        double minPrice = 0;

        /// <summary>
        /// 用收盘价 是 用高低价 否
        /// </summary>

        /// <summary>
        /// 格子总行数 
        /// </summary>
        int rowCount = 0;
        /// <summary>
        /// 格子高度
        /// </summary>
        int rowHeight = 20;
        /// <summary>
        /// 前一点是涨 1; 跌 2 ;转角 3
        /// 转角点的OX由其下一个点确认
        /// </summary>
        int preStatus = 3;

        /// <summary>
        /// 股票代码文件
        /// </summary>
        string stockListFile = "";
        /// <summary>
        /// 数据位置
        /// </summary>
        string stockDataPath = "";
        /// <summary>
        /// 股票代码列表
        /// </summary>
        DataTable dataTableSttockList = new DataTable();
        DataTable dataTableCycleList = new DataTable();
        /// <summary>
        /// 当前放大比例
        /// </summary>
        double zoom = 1;
        private string dataSource = "163";

        /// <summary>
        /// 数据处理
        /// </summary>
        void InitData()
        {
            zoom = 1;
            int length = CycleDataList.Count;
            if (length <= 0)
            {
                MessageBox.Show("找不到股票[" + currentOperateStockInfo.code + currentOperateStockInfo.name + "]的数据");
                return;
            }
            maxPriceClose = CycleDataList[CycleDataList.Count - 1].close;
            minPriceClose = maxPriceClose;
            maxPriceHigh = CycleDataList[CycleDataList.Count - 1].high;
            minPriceLow = CycleDataList[CycleDataList.Count - 1].low;

            if (currentOperateStockInfo.cycleType == CycleType.日 && currentOperateStockInfo.dataSource == "163")
            {
                // 涨跌幅算法
                // 复权因子 = 当日的前收盘价 / 昨天的收盘
                //向前复权价 = 实际价 * 每一次的复权因子
                //向后复权价 = 实际价 / 每一次的复权因子

                //向前复权
                double rehabilitationValue = 1;
                if (radBeforeRehabilitation.Checked == true)
                {
                    for (int i = CycleDataList.Count - 2; i >= 0; i--)
                    {
                        CycleData cycleData = CycleDataList[i];
                        rehabilitationValue = CycleDataList[i + 1].lastClose / CycleDataList[i].close;
                        cycleData.close = Math.Round(cycleData.close * rehabilitationValue, 2);
                        cycleData.high = Math.Round(cycleData.high * rehabilitationValue, 2);
                        cycleData.low = Math.Round(cycleData.low * rehabilitationValue, 2);
                        cycleData.open = Math.Round(cycleData.open * rehabilitationValue, 2);
                        cycleData.lastClose = Math.Round(cycleData.lastClose * rehabilitationValue, 2);
                    }
                }
                //复权因子 = 当日的前收盘价 / 昨天的收盘
                //向后复权 = 实际价 / 每一次的复权因子
                if (radafterRehabilitation.Checked == true)
                {
                    for (int i = 1; i < CycleDataList.Count; i++)
                    {
                        CycleData cycleData = CycleDataList[i];
                        //注意：这里把除法改成乘法 加速度?
                        rehabilitationValue = CycleDataList[i].lastClose / (CycleDataList[i - 1].close);
                        cycleData.close = Math.Round(cycleData.close / rehabilitationValue, 2);
                        cycleData.high = Math.Round(cycleData.high / rehabilitationValue, 2);
                        cycleData.low = Math.Round(cycleData.low / rehabilitationValue, 2);
                        cycleData.open = Math.Round(cycleData.open / rehabilitationValue, 2);
                        cycleData.lastClose = Math.Round(cycleData.lastClose / rehabilitationValue, 2);
                    }
                }
            }
            //选中最近的numOfKLine条K线
            CycleDataListNow.Clear();
            int startIndex = 0;
            int endIndex = length;
            if (this.chkIsDateArea.Checked)
            {
                DateTime dateTimeBegin = dateTimePicker1.Value.Date;
                DateTime dateTimeEnd = dateTimePicker2.Value.Date;
                for (int i = 0; i < CycleDataList.Count; i++)
                {
                    CycleData cycleData = CycleDataList[i];
                    DateTime dateTime = Convert.ToDateTime(cycleData.date);
                    if ((dateTimeBegin - dateTime).Days <= 0 && startIndex == 0)
                    {
                        startIndex = i;
                    }
                    if ((dateTimeEnd - dateTime).Days <= 0)
                    {
                        endIndex = i;
                        break;
                    }
                }
            }
            else
            {
                startIndex = (length < numOfKLine ? 0 : length - numOfKLine);
                //挑出需要的K线 
            }
            if (endIndex - startIndex < 2)
            {
                MessageBox.Show("没有选择K线数据,请检查是否有周期数据或时间段、K线是否选择正确");
                return;
            }
            //找高低价
            for (int i = startIndex; i < endIndex; i++)
            {
                CycleData cycleData = CycleDataList[i];
                CycleDataListNow.Add(cycleData);
                maxPriceClose = cycleData.close > maxPriceClose ? cycleData.close : maxPriceClose;
                minPriceClose = cycleData.close < minPriceClose ? cycleData.close : minPriceClose;
                maxPriceHigh = cycleData.high > maxPriceHigh ? cycleData.high : maxPriceHigh;
                minPriceLow = cycleData.low < minPriceLow ? cycleData.low : minPriceLow;
            }
            //收盘价还是高低价
            if (rdoClose.Checked)
            {
                maxPrice = maxPriceClose;
                minPrice = minPriceClose;
            }
            else
            {
                maxPrice = maxPriceHigh;
                minPrice = minPriceLow;
            }

            //计算格子间隔值

            currentOperateStockInfo.latticeValue = Function.CalLatticeValue(minPrice);
            this.lblRecommendGridInterval.Text = "推荐格值:" + currentOperateStockInfo.latticeValue;
            if (this.numLatticeValue.Value <= 0)
            {
                this.numLatticeValue.Value = Convert.ToDecimal(currentOperateStockInfo.latticeValue);

            }
            else
            {
                currentOperateStockInfo.latticeValue = Convert.ToDouble(this.numLatticeValue.Value);
            }
            currentOperateStockInfo.startValue = ((int)(minPrice / currentOperateStockInfo.latticeValue)) * currentOperateStockInfo.latticeValue;

            currentOperateStockInfo.endValue = ((int)(maxPrice / currentOperateStockInfo.latticeValue) + 1) * currentOperateStockInfo.latticeValue;

        }
        /// <summary>
        /// 画O和X
        /// </summary>
        private void DrawOX()
        {
            //获得基本信息

            InitData();

            InitGrid();
            FillGrid();
            SetCells();
            //重新设置格子的大小
            SetGridArea(1);

            //周围边边角角的赋值
            string infoToday = "【点数图】   最新日期: " + CycleDataList[CycleDataList.Count - 1].date.ToString() + "  开盘价: " + CycleDataList[CycleDataList.Count - 1].open.ToString() + " 收盘价: " + CycleDataList[CycleDataList.Count - 1].close.ToString() +
                "   最高价: " + CycleDataList[CycleDataList.Count - 1].high.ToString() + "   最低价: " + CycleDataList[CycleDataList.Count - 1].low.ToString() + "    价格区间: " + minPrice + "~" + maxPrice;
            string info1 = "    单格波幅:" + Math.Round(currentOperateStockInfo.latticeValue / maxPrice * 100, 2) + "%~" + Math.Round(currentOperateStockInfo.latticeValue / minPrice * 100, 2) + "%";
            this.Text = currentOperateStockInfo.code + "   " + currentOperateStockInfo.name + "   周期: " + this.cmbCycle.Text + infoToday + info1;
        }

        private void InitGrid()
        {
            //清空
            this.fpSpread1_Sheet1.Rows.Clear();
            this.fpSpread1_Sheet1.Columns.Clear();

            // 列头隐藏
            fpSpread1_Sheet1.ColumnHeaderVisible = false;
            // 行头隐藏
            fpSpread1_Sheet1.RowHeaderVisible = false;
            //添加第一列  Y坐标数值列
            this.fpSpread1_Sheet1.ColumnCount++;


            //添加行、Y坐标赋值
            double priceArea = maxPrice - currentOperateStockInfo.startValue;
            rowCount = (int)Math.Ceiling((priceArea) / (currentOperateStockInfo.latticeValue)) + 2;
            for (int i = 0; i < rowCount; i++)
            {
                //DataGridViewRow dr = new DataGridViewRow();
                //dr.Height = rowHeight;
                this.fpSpread1_Sheet1.Rows.Count++;
                //this.dataGridView1.Rows[i].Cells[0].Value = ((rowCount - 1 - i) * dotSpace * latticeValue + currentOperateStockInfo.startValue).ToString();
                this.fpSpread1_Sheet1.Cells[i, 0].Value = ((rowCount - 2 - i) * currentOperateStockInfo.latticeValue + currentOperateStockInfo.startValue).ToString("0.00");

            }


            //添加第二列
            this.fpSpread1_Sheet1.ColumnCount++;
        }
        private void FillGrid()
        {
            List<DotValue> dotValueList = Function.CalculateOneDotGraphic(currentOperateStockInfo, CycleDataListNow);
            string date = dotValueList[0].data[0].date;
            CycleData tempDot = new CycleData();
            tempDot.close = currentOperateStockInfo.startValue;
            this.fpSpread1_Sheet1.ColumnCount = dotValueList[0].position.X + 2;
            int col = 1;
            for (int i = dotValueList.Count - 1; i >= 0; i--)
            {
                DotValue dotValue = dotValueList[i];
                string cellText = dotValue.isUp ? "X" : "O";
                Color color = dotValue.isUp ? Color.Red : Color.Blue;

                if (dotValue.position.X > col)
                {
                    col = dotValue.position.X;
                    if (currentOperateStockInfo.cycleType == CycleType.日)
                    {
                        this.fpSpread1_Sheet1.Cells[rowCount - 1, col].Text = dotValue.data[0].date.Replace("-", "").Substring(0, 6);
                    }
                    else if (currentOperateStockInfo.cycleType == CycleType.五分钟)
                    {
                        this.radNoRehabilitation.Checked = true;
                        this.fpSpread1_Sheet1.Cells[rowCount - 1, col].Text = dotValue.data[0].date;

                    }
                }

                int rowIndex = rowCount - 1 - (dotValue.position.Y - Function.getGridIndexCurrent(currentOperateStockInfo, tempDot.close, true) + 1);
                Cell curCell = this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1];
                if ((curCell.Text.Contains("X") || curCell.Text.Contains("O")))
                {
                    string strDataList = "";
                    //如果当前点不是填充点,而此位置又被填充点赋值了高、开、低、收价(因为只有填充的这一个点)
                    //则把填充的高、开、低、收价替换为当前点
                    if (!dotValue.isFill && this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1].Note.Contains("填"))
                    {
                        string tip = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}", "时间:" + dotValue.data[0].date,
                    "收盘价:" + dotValue.data[0].close, "最高价:" + dotValue.data[0].high, "最低价:" + dotValue.data[0].low, "转折价差(格):" + dotValue.space);
                        strDataList = "点数列表:\r\n";
                        this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1].Note = tip + "\r\n" + strDataList;
                    }
                }
                else
                {
                    //if(i == dotValueList.Count - 1)//开始点涨跌不明确用五角星代替
                    //{
                    //    cellText = "★";
                    //}
                    this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1].Text = cellText;
                    this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1].ForeColor = color;
                    string tip = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}", "时间:" + dotValue.data[0].date,
                            "收盘价:" + dotValue.data[0].close, "最高价:" + dotValue.data[0].high, "最低价:" + dotValue.data[0].low, "转折价差(格):" + dotValue.space);
                    string strDataList = "点数列表:\r\n";
                    for (int j = 0; j < dotValue.data.Count; j++)
                    {
                        string str = string.Format("{0} -> {1}", dotValue.data[j].date, dotValue.data[j].close) + (dotValue.isFill ? "[填]\r\n" : "\r\n"); ;
                        strDataList += str;
                    }
                    this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1].Note = tip + "\r\n" + strDataList;
                    //同点同色
                    if (dotValue.data[0].date == date)
                    {
                        this.fpSpread1_Sheet1.Cells[rowIndex, dotValue.position.X + 1].BackColor = Color.WhiteSmoke;
                        int row = (rowCount - 1 - (dotValue.position.Y - Function.getGridIndexCurrent(currentOperateStockInfo, tempDot.close, true) + 1) + (dotValue.isUp ? 1 : -1));
                        this.fpSpread1_Sheet1.Cells[row, dotValue.position.X + 1].BackColor = Color.WhiteSmoke;
                    }
                    else
                    {
                        date = dotValue.data[0].date;
                    }
                }
            }


        }
        private void SetGridArea(double zoom)
        {
            int length = this.fpSpread1_Sheet1.RowCount > this.fpSpread1_Sheet1.ColumnCount ? this.fpSpread1_Sheet1.RowCount : this.fpSpread1_Sheet1.ColumnCount;
            rowHeight = (int)(((this.fpSpread1.Width) * zoom / length));

            if (rowHeight < 15)
            {
                rowHeight = 15;
                zoom = 1;
            }
            else if (rowHeight > 50)
            {
                rowHeight = 50;
                zoom = (double)50 / length;
            }
            //else
            //{
            //    zoom = zoom * 0.8;
            //}

            double fontZoom = (double)rowHeight / this.fpSpread1_Sheet1.Rows[0].Height;
            //设高度
            for (int i = 0; i < this.fpSpread1_Sheet1.RowCount; i++)
            {
                this.fpSpread1_Sheet1.Rows[i].Height = rowHeight;
                this.fpSpread1_Sheet1.Cells[i, 0].VerticalAlignment = CellVerticalAlignment.Bottom;
                this.fpSpread1_Sheet1.Cells[i, 0].HorizontalAlignment = CellHorizontalAlignment.Right;

                Font f = new Font(Font.FontFamily, (float)((Font.Size) * fontZoom), Font.Style);
                //this.fpSpread1_Sheet1.Cells[i, 0].Font = f;
                //Font f1 = new Font(Font.FontFamily, (float)(Font.Size * zoom), Font.Style);

                for (int j = 0; j < this.fpSpread1_Sheet1.ColumnCount; j++)
                {
                    this.fpSpread1_Sheet1.Cells[i, j].Font = f;
                }

                //最后一行高
                if (i == this.fpSpread1_Sheet1.RowCount - 1)
                {
                    this.fpSpread1_Sheet1.Rows[this.fpSpread1_Sheet1.RowCount - 1].Height = this.fpSpread1_Sheet1.Rows[this.fpSpread1_Sheet1.RowCount - 1].GetPreferredHeight();
                }

            }
            //最后一行高度
            //this.fpSpread1_Sheet1.Rows[fpSpread1_Sheet1.RowCount - 1].Height = rowHeight * 2;


            //设置宽度和x坐标轴单位值
            for (int i = 1; i < this.fpSpread1_Sheet1.ColumnCount - 1; i++)
            {
                this.fpSpread1_Sheet1.Columns[i].Width = rowHeight;
            }
            //第一列 最后列宽度
            this.fpSpread1_Sheet1.Columns[0].Width = this.fpSpread1_Sheet1.Columns[0].GetPreferredWidth();
            this.fpSpread1_Sheet1.Columns[fpSpread1_Sheet1.ColumnCount - 1].Width = this.fpSpread1_Sheet1.Columns[0].GetPreferredWidth();
            //冻结第一列
            this.fpSpread1_Sheet1.FrozenColumnCount = 1;

            //冻结最后列
            this.fpSpread1_Sheet1.FrozenTrailingColumnCount = 1;

            //水平滚动条到最右 竖直条到最下
            this.fpSpread1_Sheet1.SetActiveCell(fpSpread1_Sheet1.RowCount - 1, fpSpread1_Sheet1.ColumnCount - 2);
            this.fpSpread1.ShowActiveCell(FarPoint.Win.Spread.VerticalPosition.Bottom, FarPoint.Win.Spread.HorizontalPosition.Right);

        }
        /// <summary>
        /// 表格设置
        /// </summary>
        private void SetCells()
        {
            for (int i = 0; i < this.fpSpread1_Sheet1.RowCount; i++)
            {
                for (int j = 1; j < this.fpSpread1_Sheet1.ColumnCount; j++)
                {
                    this.fpSpread1_Sheet1.Cells[i, j].VerticalAlignment = CellVerticalAlignment.Center;
                    this.fpSpread1_Sheet1.Cells[i, j].HorizontalAlignment = CellHorizontalAlignment.Center;
                    this.fpSpread1_Sheet1.Cells[i, j].NoteIndicatorColor = Color.White;
                    this.fpSpread1_Sheet1.Cells[i, j].NoteStyle = NoteStyle.Hidden;

                    FarPoint.Win.Spread.CellType.TextCellType textCellType1 = new FarPoint.Win.Spread.CellType.TextCellType();
                    this.fpSpread1_Sheet1.Cells[i, j].CellType = textCellType1;
                    this.fpSpread1_Sheet1.Cells[i, j].Font = Font;
                    //最后一行 x轴值 旋转90度
                    if (i == (this.fpSpread1_Sheet1.RowCount - 1))
                    {
                        textCellType1.TextOrientation = FarPoint.Win.TextOrientation.TextVertical;
                    }
                }

            }
            this.fpSpread1_Sheet1.Rows[this.fpSpread1_Sheet1.RowCount - 1].Height = this.fpSpread1_Sheet1.Rows[this.fpSpread1_Sheet1.RowCount - 1].GetPreferredHeight();
            this.fpSpread1_Sheet1.Cells[this.fpSpread1_Sheet1.RowCount - 1, 0].Text = "";


            //添加最后一列Y轴
            this.fpSpread1_Sheet1.ColumnCount++;
            FarPoint.Win.Spread.CellType.TextCellType textCellType2 = new FarPoint.Win.Spread.CellType.TextCellType();

            for (int i = 0; i < this.fpSpread1_Sheet1.RowCount - 1; i++)
            {
                this.fpSpread1_Sheet1.Cells[i, this.fpSpread1_Sheet1.ColumnCount - 1].CellType = textCellType2;
                this.fpSpread1_Sheet1.Cells[i, this.fpSpread1_Sheet1.ColumnCount - 1].Text = this.fpSpread1_Sheet1.Cells[i, 0].Text;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            currentOperateStockInfo.dataSource = dataSource;
            if (rdoOne.Checked)
            {
                currentOperateStockInfo.dotInterval = 1;
            }
            else if (rdoThree.Checked)
            {
                currentOperateStockInfo.dotInterval = 3;
            }
            else if (rdoFive.Checked)
            {
                currentOperateStockInfo.dotInterval = 5;
            }
            //单点重建 即一列只有一个点 是否合并
            currentOperateStockInfo.isOneDotRebuild = this.chkOneDotRebuild.Checked;

            if (chkIsTest.Checked)
            {
                currentOperateStockInfo.code = "测试数据";
                currentOperateStockInfo.name = "测试数据";
                CycleDataList = Function.GetTestStockData();

            }
            else
            {
                if (rdoClose.Checked)
                {
                    currentOperateStockInfo.isClose = true;
                }
                else
                {
                    currentOperateStockInfo.isClose = false;
                }

                if (this.txtStockFilter.Tag != null)
                {
                    currentOperateStockInfo.code = this.txtStockFilter.Tag.ToString();
                }
                else
                {
                    if (this.txtStockFilter.Tag == null)
                    {
                        currentOperateStockInfo.code = "";
                        MessageBox.Show("请选择股票");
                        return;

                    }
                }
                currentOperateStockInfo.name = this.txtStockFilter.Text;
                numOfKLine = Convert.ToInt32(this.numNumOfKLine.Value);

                //string fileName = "";
                string type = Function.GetStockType(currentOperateStockInfo.code);
                if (this.cmbCycle.Text == CycleType.日.ToString())
                {
                    //fileName = stockDataPath + type + "\\lday\\" + type + currentOperateStockInfo.code + ".day";
                    currentOperateStockInfo.cycleType = CycleType.日;
                }
                else if (this.cmbCycle.Text == CycleType.五分钟.ToString())
                {
                    currentOperateStockInfo.cycleType = CycleType.五分钟;
                    //fileName = stockDataPath + type + "\\fzline\\" + type + currentOperateStockInfo.code + ".lc5";
                    //MessageBox.Show("未实现");
                    //return;
                }
                else if (this.cmbCycle.Text == CycleType.一分钟.ToString())
                {
                    currentOperateStockInfo.cycleType = CycleType.一分钟;
                    //fileName = stockDataPath + type + "\\minline\\" + type + currentOperateStockInfo.code + ".lc1";
                    MessageBox.Show("周期未实现");
                    return;
                }
                else
                {
                    MessageBox.Show("周期未实现");
                    return;
                }
                if (radNoRehabilitation.Checked)
                {
                    currentOperateStockInfo.RehabilitationStatus = 0;
                }
                else if (radBeforeRehabilitation.Checked)
                {
                    currentOperateStockInfo.RehabilitationStatus = 1;
                }
                else if (radafterRehabilitation.Checked)
                {
                    currentOperateStockInfo.RehabilitationStatus = 2;
                }
                CycleDataList = Function.GetStockData(currentOperateStockInfo);
            }
            DrawOX();
            DrawKLine();
        }


        private void txtNumOfKLine_TextChanged(object sender, EventArgs e)
        {

            numOfKLine = Convert.ToInt32(this.numNumOfKLine.Value);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.chart1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseWheel);
            this.fpSpread1.MouseWheel += new MouseEventHandler(fpSpread1_MouseWheel);
            stockListFile = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + ConfigurationManager.AppSettings["stockListFile"];
            stockDataPath = ConfigurationManager.AppSettings["stockDataPath"];
            if (string.IsNullOrEmpty(stockListFile) || string.IsNullOrEmpty(stockDataPath))
            {
                MessageBox.Show("读取配置文件失败");
            }
            this.numNumOfKLine.Value = numOfKLine;
            dataTableSttockList = Function.GetStockList(stockListFile);
            this.fpSpread2_Sheet1.RowCount = dataTableSttockList.Rows.Count;
            this.fpSpread2_Sheet1.ColumnCount = dataTableSttockList.Columns.Count;
            for (int i = 0; i < this.fpSpread2_Sheet1.ColumnCount; i++)
            {
                this.fpSpread2_Sheet1.Columns[i].Width = this.fpSpread2_Sheet1.Columns[i].GetPreferredWidth();
            }
            this.fpSpread2_Sheet1.DataSource = dataTableSttockList;

            dataTableCycleList.Columns.Add();
            dataTableCycleList.Columns.Add();

            this.cmbCycle.Items.AddRange(new string[] {
                CycleType.一分钟.ToString(),
                CycleType.五分钟.ToString(),
                CycleType.三十分钟.ToString(),
                CycleType.日.ToString(),
                CycleType.周.ToString(),
                CycleType.年.ToString(),

                    });
            this.cmbCycle.SelectedIndex = 3;
            this.toolTip1.SetToolTip(this.label5, "点数图规则:\r\n" +
                " a、按照收盘价计算:" +
                "   1、今日与昨日的收盘价格值比较，若前面状态是涨，今日高于昨日至少1格则填充X;\r\n比昨日至少低于转折格数(常用1、3、5)，则换列填充O\r\n" +
                "   2、若昨日是跌的状态，今日处理方法同上反之\r\n" +
                "   3、单个转折可以不换列。这个可以选择\r\n" +
                " b、按照高低价计算:" +
                "\r\n   1、第一点最低与第二点最高算涨幅，第一点最高与第二点最低算跌幅。涨幅大于跌幅则第一点为涨,画X;否则为跌,画O" +
                "\r\n   2、前一点为涨，则若创新高且格值比前一点高至少一格，则用最高价；\r\n否则看是否创新低且格值比前一点至少低于转折格数(常用1、3、5)，若是,则换列画O;\r\n否则就是无新高新低且无转折，是原地踏步" +
                "\r\n   3、前一点为跌，则与前面类似，反之" +
                "\r\n   4、格值的获取规则是：上涨的格值向下取整，下跌的格值向上取整\r\n" +
                "\r\n复权使用涨跌幅算法：\r\n复权因子 = 当日的前收盘价 / 昨天的收盘\r\n向前复权价 = 实际价 * 每一次的复权因子\r\n向后复权价 = 实际价 / 每一次的复权因子\r\n(注意：这里重点是获取的数据要有真实的今日昨收价，不是昨天的收盘价，是今天的昨日收盘价)" +
                "\r\n缩放规则:\r\n点击一下最后一列或第一列任意一格,滚动鼠标则上下滑动,点击一下其他列任意一格滚动鼠标缩放。默认在倒数第二列最后一行" +
                "通达信数据没有今日昨收价，所以只能不复权");
            toolTip1.BackColor = Color.Red;
            if (!string.IsNullOrEmpty(this.inParams[0]))
            {
                AutoLoadStockData(inParams[0]);
                dataSource = "通达信";
                currentOperateStockInfo.dataSource = "通达信";
                this.radNoRehabilitation.Checked = true;
                this.radafterRehabilitation.Enabled = false;
                this.radBeforeRehabilitation.Enabled = false;
                this.WindowState = FormWindowState.Normal;
            }
            this.dateTimePicker1.Value = DateTime.Now.Date.AddMonths(-3);
        }

        /// <summary>
        /// 计算单点图
        /// 
        /// 还差两个问题:回调或反弹1格如何不换行
        ///            填充空点
        /// </summary>
        /// <param name="cycleDataList"></param>
        /// <returns></returns>


        private void fpSpread1_TextTipFetch(object sender, TextTipFetchEventArgs e)
        {
            e.ShowTip = true;
            e.TipText = this.fpSpread1_Sheet1.Cells[e.Row, e.Column].Note;
        }

        private void txtStockFilter_TextChanged(object sender, EventArgs e)
        {
            this.fpSpread2.Visible = true;
            DataView dv = dataTableSttockList.DefaultView;
            dv.RowFilter = "代码 LIKE '%" + txtStockFilter.Text + "%' or 名称 LIKE '%" + txtStockFilter.Text + "%' or 拼音码 LIKE '%" + txtStockFilter.Text + "%'";
            for (int i = 0; i < this.fpSpread2_Sheet1.ColumnCount; i++)
            {
                this.fpSpread2_Sheet1.Columns[i].Width = 50;
            }
        }

        private void fpSpread2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.fpSpread2.Visible = false;
            }
            if (e.KeyCode == Keys.Enter)
            {
                StockChoose(this.fpSpread2_Sheet1.ActiveRowIndex);
            }
        }

        private void StockChoose(int row)
        {
            this.fpSpread2.Visible = false;

            string name = fpSpread2_Sheet1.Cells[row, 1].Text;
            string code = fpSpread2_Sheet1.Cells[row, 0].Text;
            this.txtStockFilter.TextChanged -= new System.EventHandler(this.txtStockFilter_TextChanged);
            this.txtStockFilter.KeyDown -= new System.Windows.Forms.KeyEventHandler(this.txtStockFilter_KeyDown);
            //给txtStockFilter赋值时先把它的键盘捕捉去掉  之后再加上
            this.txtStockFilter.Text = name;
            this.txtStockFilter.Tag = code;

            this.fpSpread2.Visible = false;
            this.numLatticeValue.Value = 0;
            this.btnGenerate.Focus();
            button1_Click(null, null);
            //this.txtStockFilter.Text = "";
            //this.txtStockFilter.Tag = null;
            this.txtStockFilter.Focus();
            this.txtStockFilter.TextChanged += new System.EventHandler(this.txtStockFilter_TextChanged);
            this.txtStockFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtStockFilter_KeyDown);


        }

        private void txtStockFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.fpSpread2.Visible = false;
            }
            if (e.KeyCode == Keys.Down)
            {
                this.fpSpread2.Focus();
                this.fpSpread2_Sheet1.SetActiveCell(0, 0);
            }
            if (e.KeyCode == Keys.Enter)
            {
                StockChoose(0);
            }
        }

        private void fpSpread2_CellDoubleClick(object sender, CellClickEventArgs e)
        {
            StockChoose(e.Row);
        }

        private void numLatticeValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.button1_Click(null, null);
            }
        }

        private void numNumOfKLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.button1_Click(null, null);
            }
        }
        private void AutoLoadStockData(string code)
        {
            txtStockFilter.Text = code;
            KeyEventArgs e = new KeyEventArgs(Keys.Enter);
            this.txtStockFilter_KeyDown(null, e);
        }

        private void btnUpdateStockList_Click(object sender, EventArgs e)
        {
            string code = "hs";
            if (code == "hs")
            {
                string stockListFile = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + ConfigurationManager.AppSettings["stockListFile"];
                DataTable dataTable = Function.GetStockList(stockListFile);
                //ProgressBar progress = new ProgressBar();
                progressBar1.Maximum = dataTable.Rows.Count;
                progressBar1.Visible = true;
                progressBar1.Show();

                string message = string.Empty;
                progressBar1.Value = 0;
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    progressBar1.Value++;
                    code = dataTable.Rows[i][0].ToString();

                    Function.UpdataStockDataDay(code);

                   
                    //Thread.Sleep(100);
                }
                progressBar1.Visible = false;
            }
        }

        private void DrawKLine()
        {

            chart1.Series[0].Points.Clear();

            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisY.Interval = currentOperateStockInfo.latticeValue * 2;//(int)(currentOperateStockInfo.endValue - currentOperateStockInfo.startValue) / 10;
            chart1.ChartAreas[0].AxisY.Minimum = currentOperateStockInfo.startValue / 2;
            chart1.ChartAreas[0].AxisY.Maximum = currentOperateStockInfo.endValue;
            chart1.ChartAreas[0].AxisX.LineColor = Color.Transparent;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisX.Title = "时间(日)";
            chart1.ChartAreas[0].AxisY.Title = "价格(元)";


            chart1.Series[0].Name = "price";
            chart1.Series[0].LegendText = currentOperateStockInfo.name;
            chart1.Series[0].XValueMember = "date";
            if (!chkCandlestick.Checked)
            {//竹线图
                chart1.Series[0].YValueMembers = "high,low,close,close";
            }
            else
            { 
                //蜡烛图
                chart1.Series[0].YValueMembers = "high,low,open,close";
            }
            chart1.BackColor = Color.White;
            //chart1.Series[0].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.String;
            chart1.Series[0].CustomProperties = "PriceDownColor=MediumPurple,PriceUpColor=OrangeRed";

            //将滚动内嵌到坐标轴中
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            // 设置滚动条的大小
            chart1.ChartAreas[0].AxisX.ScrollBar.Size = 10;


            chart1.ChartAreas[0].CursorX.AutoScroll = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.Enabled = true;

            //chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Size = 100;
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("date");
            dataTable.Columns.Add("high");
            dataTable.Columns.Add("low");
            dataTable.Columns.Add("open");
            dataTable.Columns.Add("close");
            dataTable.Columns.Add("amount");
            dataTable.Columns.Add("vol");
            dataTable.Columns.Add("lastClose");
            long maxVol = 0;
            for (int i = 0; i < CycleDataListNow.Count; i++)
            {
                CycleData cycleData = CycleDataListNow[i];
                dataTable.Rows.Add(dataTable.NewRow());
                dataTable.Rows[i]["date"] = cycleData.date;
                dataTable.Rows[i]["open"] = cycleData.open;
                dataTable.Rows[i]["high"] = cycleData.high;
                dataTable.Rows[i]["low"] = cycleData.low;
                dataTable.Rows[i]["close"] = cycleData.close;
                dataTable.Rows[i]["amount"] = cycleData.amount;
                dataTable.Rows[i]["vol"] = cycleData.vol;
                dataTable.Rows[i]["lastClose"] = cycleData.lastClose;
                if (cycleData.vol > maxVol)
                {
                    maxVol = cycleData.vol;
                }
            }

            /*两个Y轴公用一个X轴，Y轴数据分开两种刻度显示还未实现*/
            //次X轴 成交量
            //this.chart1.ChartAreas[0].AxisY2.MajorGrid.LineColor = System.Drawing.Color.Transparent;
            //chart1.ChartAreas[0].AxisX2.Interval = 1;
            //chart1.ChartAreas[0].AxisX2.Title = "时间(日)11";

            //次Y轴 成交量
            this.chart1.ChartAreas[0].AxisY2.MajorGrid.LineColor = System.Drawing.Color.Transparent;
            chart1.ChartAreas[0].AxisY2.Interval = maxVol*currentOperateStockInfo.endValue/currentOperateStockInfo.startValue;
            chart1.ChartAreas[0].AxisY2.Minimum = 0;
            chart1.ChartAreas[0].AxisY2.Maximum = maxVol* currentOperateStockInfo.endValue / currentOperateStockInfo.startValue;

            chart1.Series[1].Color = Color.OrangeRed;
            chart1.Series[1]["PixelPointWidth"] = "1";
            chart1.Series[1].Name = "vol";
            chart1.Series[1].LegendText = "成交量";
            chart1.Series[1].XValueMember = "date";
            chart1.Series[1].YValueMembers = "vol";

            //数据绑定
            chart1.DataSource = dataTable;
            chart1.DataBind();
            //位置靠右
            chart1.ChartAreas[0].AxisX.ScaleView.Position = chart1.Series[0].Points.Count - chart1.ChartAreas[0].AxisX.ScaleView.Size;
            //成交量上色
            for(int i=1;i< chart1.Series[1].Points.Count;i++)
            {
                DataPoint dpCur = chart1.Series[0].Points[i];
                DataPoint dpPre = chart1.Series[0].Points[i-1];
                chart1.Series[1].Points[i].Color = dpCur.YValues[3]> dpPre.YValues[3]?Color.OrangeRed:Color.MediumPurple;
            }
            //竹线图上色
            if (!chkCandlestick.Checked)
            {
                for (int i = 1; i < chart1.Series[0].Points.Count; i++)
                {
                    DataPoint dpCur = chart1.Series[0].Points[i];
                    DataPoint dpPre = chart1.Series[0].Points[i - 1];
                    chart1.Series[0].Points[i].Color = dpCur.YValues[3] > dpPre.YValues[3] ? Color.OrangeRed : Color.MediumPurple;
                }
            }
        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            chart1.ChartAreas[0].AxisX.ScaleView.Size += (e.Delta > 0 ? -20 : 20);
            chart1.ChartAreas[0].AxisX.ScaleView.Position = chart1.Series[0].Points.Count - chart1.ChartAreas[0].AxisX.ScaleView.Size;
        }
        private void fpSpread1_MouseWheel(object sender, MouseEventArgs e)
        {
            FarPoint.Win.Spread.SheetView sheetView = (sender as FarPoint.Win.Spread.FpSpread).ActiveSheet;
            if (sheetView.ColumnCount > 2 && (sheetView.ActiveColumnIndex < sheetView.ColumnCount - 1 && sheetView.ActiveColumnIndex > 0))
            {
                zoom += (e.Delta < 0 ? -0.2 : 0.2);
                SetGridArea(zoom);
            }
        }
        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint && e.HitTestResult.Series.Name == "price")
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                DataRow dr = (this.chart1.DataSource as DataTable).Rows[i];
                string amount = (Math.Round((Convert.ToDouble(dr["amount"].ToString()) / 10000), 2)).ToString() + "万元";
                string vol = (Math.Round(Convert.ToDouble(dr["vol"].ToString()) / 1000000, 2)).ToString() + "万手";
                double lastClose = Convert.ToDouble(dr["lastClose"]);
                if (i > 0)
                {
                    lastClose = Convert.ToDouble((this.chart1.DataSource as DataTable).Rows[i - 1]["close"]);
                }
                string wave = Math.Round(dp.YValues[3] / lastClose * 100 - 100, 2).ToString() + "%";
                //分别显示x轴和y轴的数值，其中{1:F3},表示显示的是float类型，精确到小数点后3位。  
                e.Text = string.Format("日期:{0}\r\n收盘价:{1}\r\n最高价:{2}\r\n开盘价:{3}\r\n最低价{4:F2}\r\n昨收价:{5}\r\n成交金额:{6}\r\n成交量:{7}\r\n涨幅:{8}", dp.AxisLabel, dp.YValues[3], dp.YValues[0], dp.YValues[2], dp.YValues[1], lastClose, amount, vol, wave);
            }
            else if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint && e.HitTestResult.Series.Name == "vol")
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                DataRow dr = (this.chart1.DataSource as DataTable).Rows[i];
                string vol = (Math.Round(Convert.ToDouble(dr["vol"].ToString()) / 1000000, 2)).ToString() + "万手";

                e.Text = string.Format("日期:{0}\r\n成交量:{1}", dp.AxisLabel, vol);
            }
        }

        private void numLatticeValue_ValueChanged(object sender, EventArgs e)
        {
            currentOperateStockInfo.latticeValue = Convert.ToDouble(this.numLatticeValue.Value);
        }

        private void rad163_CheckedChanged(object sender, EventArgs e)
        {
            this.radBeforeRehabilitation.Checked = true;
            this.radafterRehabilitation.Enabled = true;
            this.radBeforeRehabilitation.Enabled = true;
        }

        private void radtdx_CheckedChanged(object sender, EventArgs e)
        {
            this.radNoRehabilitation.Checked = true;
            this.radafterRehabilitation.Enabled = false;
            this.radBeforeRehabilitation.Enabled = false;
        }
    }
    public static class ExtensionMethods

    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)

        {

            Type dgvType = dgv.GetType();

            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",

            BindingFlags.Instance | BindingFlags.NonPublic);

            pi.SetValue(dgv, setting, null);

        }
    }
}