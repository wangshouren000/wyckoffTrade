using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using static DotGraphic.Modes;

namespace DotGraphic
{
    /// <summary>
    /// 函数类
    /// </summary>
    public static class Function
    {
        static StockHelp stockHelp = new StockHelp();

        public static double CalLatticeValue(double basePrice)
        {
            if (basePrice <= 0.25)
            {
                return 0.0625;
            }
            else if (basePrice > 0.25 && basePrice <= 1)
            {
                return 0.125;
            }
            else if (basePrice > 1 && basePrice <= 5)
            {
                return 0.25;
            }
            else if (basePrice > 5 && basePrice <= 20)
            {
                return 0.5;
            }
            else if (basePrice > 20 && basePrice <= 100)
            {
                return 1;
            }
            else if (basePrice > 100 && basePrice <= 200)
            {
                return 2;
            }
            else if (basePrice > 200 && basePrice <= 500)
            {
                return 4;
            }
            else if (basePrice > 500 && basePrice <= 1000)
            {
                return 5;
            }
            else
            {
                return (int)(basePrice / 50);
            }
        }
        public static string dataSourceType = "163";
        public static List<CycleData> GetStockData(OperateStockInfo operateStockInfo)
        {
            List<CycleData> cycleDataList = new List<CycleData>();
            string fileName = "";
            //string type = (GetStockType(code) == "sh" ? "0" : "1");
            //从文件读取股票原始周期数据

            //日线读取网易163的数据
            if (operateStockInfo.cycleType == CycleType.日 && operateStockInfo.dataSource == "163")
            {

                fileName = System.AppDomain.CurrentDomain.BaseDirectory + "dataDay\\" + operateStockInfo.code + ".csv";
                if (!File.Exists(fileName))
                {
                    return cycleDataList;
                }
                using (StreamReader streamReader = new StreamReader(fileName, Encoding.Default))
                {
                    //日期,股票代码,名称,收盘价,最高价,最低价,开盘价,前收盘,成交量,成交金额
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (operateStockInfo.cycleType == CycleType.日)
                        {
                            string[] result = line.Split(',');

                            //while (result > 0 && result != 65536)
                            //{
                            CycleData cycleData = new CycleData();
                            cycleData.date = result[0];
                            cycleData.open = Convert.ToDouble(result[6]);
                            if (cycleData.open == 0)//剔除停牌的数据
                            {
                                continue;
                            }
                            cycleData.high = Convert.ToDouble(result[4]);
                            cycleData.low = Convert.ToDouble(result[5]);
                            cycleData.close = Convert.ToDouble(result[3]);
                            cycleData.amount = (long)ConvertScientificCounting(result[9]);
                            cycleData.vol = Convert.ToInt64(result[8]);
                            cycleData.lastClose = Convert.ToDouble(result[7]);

                            cycleDataList.Insert(0, cycleData);
                        }

                    }
                }
            }
            else if (operateStockInfo.dataSource == "通达信")
            {
                #region 通达信数据
                string type = GetStockType(operateStockInfo.code);

                if (operateStockInfo.cycleType == CycleType.日)
                {
                    fileName = ConfigurationManager.AppSettings["stockDataPath"] + type + "\\lday\\" + type + operateStockInfo.code + ".day";

                }
                else if (operateStockInfo.cycleType == CycleType.五分钟)
                {
                    fileName = ConfigurationManager.AppSettings["stockDataPath"] + type + "\\fzline\\" + type + operateStockInfo.code + ".lc5";

                }
                //fileName = file;
                using (BinaryReader br = new BinaryReader(new FileStream(fileName,
                   FileMode.Open)))
                {
                    #region MyRegion
                    /*
                                    一、通达信日线*.day文件
                                       文件名即股票代码
                                       每32个字节为一天数据
                                       每4个字节为一个字段，每个字段内低字节在前
                                       00 ~ 03 字节：年月日, 整型
                                       04 ~ 07 字节：开盘价*100， 整型
                                       08 ~ 11 字节：最高价*100,  整型
                                       12 ~ 15 字节：最低价*100,  整型
                                       16 ~ 19 字节：收盘价*100,  整型
                                       20 ~ 23 字节：成交额（元），float型
                                       24 ~ 27 字节：成交量（股），整型
                                       28 ~ 31 字节：上日收盘*100, 整型


                                   二、通达信5分钟线*.lc5文件和*.lc1文件
                                       文件名即股票代码
                                       每32个字节为一个5分钟数据，每字段内低字节在前
                                       00 ~ 01 字节：日期，整型，设其值为num，则日期计算方法为：
                                                     year=floor(num/2048)+2004;
                                                     month=floor(mod(num,2048)/100);
                                                     day=mod(mod(num,2048),100);
                                       02 ~ 03 字节： 从0点开始至目前的分钟数，整型
                                       04 ~ 07 字节：开盘价，float型
                                       08 ~ 11 字节：最高价，float型
                                       12 ~ 15 字节：最低价，float型
                                       16 ~ 19 字节：收盘价，float型
                                       20 ~ 23 字节：成交额，float型
                                       24 ~ 27 字节：成交量（股），整型
                                       28 ~ 31 字节：（保留）
                                    */
                    #endregion

                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        #region MyRegion
                        if (operateStockInfo.cycleType == CycleType.日)
                        {
                            int result = br.ReadInt32();

                            while (result > 0 && result != 65536)
                            {
                                CycleData cycleData = new CycleData();
                                cycleData.date = result.ToString("");
                                cycleData.open = (double)br.ReadInt32() / 100;
                                cycleData.high = (double)br.ReadInt32() / 100;
                                cycleData.low = (double)br.ReadInt32() / 100;
                                cycleData.close = (double)br.ReadInt32() / 100;
                                cycleData.amount = (long)Decimal.Parse(br.ReadSingle().ToString(), System.Globalization.NumberStyles.Float);
                                cycleData.vol = br.ReadInt32();
                                result = br.ReadInt32();

                                cycleDataList.Add(cycleData);
                            }
                        }
                        #endregion

                        if (operateStockInfo.cycleType == CycleType.五分钟)
                        {
                            //fuck 这个地方是无符号的才对
                            int result = br.ReadUInt16();

                            while (result > 0 && result != 65536)
                            {
                                CycleData cycleData = new CycleData();

                                int year = result / 2048 + 2004;
                                int month = result % 2048 / 100;
                                int day = result % 2048 % 100;

                                int min = br.ReadInt16();

                                int HH = min / 60;
                                int mm = min % 60;
                                cycleData.date = year.ToString() + month.ToString("00") + day.ToString("00") + " " + HH.ToString("00") + ":" + mm.ToString();
                                cycleData.open = Math.Round((double)br.ReadSingle());
                                cycleData.high = Math.Round((double)br.ReadSingle());
                                cycleData.low = Math.Round((double)br.ReadSingle());
                                cycleData.close = Math.Round((double)br.ReadSingle());
                                cycleData.amount = (long)br.ReadSingle();
                                cycleData.vol = br.ReadInt32();
                                cycleData.lastClose = (double)br.ReadInt32();
                                result = br.ReadUInt16();
                                //5f数据貌似前面的是很久前的数据，所以插入
                                cycleDataList.Add(cycleData);

                            }
                        }
                    }
                }
            }
            #endregion
            return cycleDataList;
        }
        public static double ConvertScientificCounting(string str)
        {
            double result = 0;
            if (str.ToUpper().Contains("E"))
            {
                double b = double.Parse(str.ToUpper().Split('E')[0].ToString());//整数部分
                double c = double.Parse(str.ToUpper().Split('E')[1].ToString());//指数部分
                result = b * Math.Pow(10, c);
            }
            else
            {
                result = double.Parse(str == "" ? "0" : str);
            }
            return result;
        }

        /// <summary>
        /// 获取股票列表
        /// </summary>
        /// <param name="stockListFile"></param>
        /// <returns></returns>
        public static DataTable GetStockList(string stockListFile)
        {
            DataTable dataTableSttockList = new DataTable();
            StreamReader sr = new StreamReader(stockListFile, Encoding.Default);
            String line;
            int index = 0;
            while ((line = sr.ReadLine()) != null)
            {
                string[] arr = line.ToString().Split('\t');
                if (index == 0)
                {
                    dataTableSttockList.Columns.Add(arr[0]);
                    dataTableSttockList.Columns.Add(arr[1]);
                    dataTableSttockList.Columns.Add("类型");
                    dataTableSttockList.Columns.Add("拼音码");
                }
                else
                {
                    DataRow dr = dataTableSttockList.NewRow();
                    dr[0] = arr[0];
                    dr[1] = arr[1];
                    dr[2] = GetStockType(arr[0]);
                    dr[3] = GetSpellCode(arr[1]);
                    dataTableSttockList.Rows.Add(dr);
                }
                index++;

            }
            return dataTableSttockList;

        }

        public static List<CycleData> GetTestStockData()
        {
            List<CycleData> cycleDataList = new List<CycleData>();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "test.csv";
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            int i = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (i > 0)
                {
                    string[] arr = line.ToString().Split(',');
                    CycleData cycleData = new CycleData();
                    cycleData.date = arr[0];
                    cycleData.open = Convert.ToDouble(arr[1]);
                    cycleData.high = Convert.ToDouble(arr[2]);
                    cycleData.low = Convert.ToDouble(arr[3]);
                    cycleData.close = Convert.ToDouble(arr[4]);
                    cycleDataList.Add(cycleData);
                }
                else
                {
                    i++;
                }
            }
            return cycleDataList;
        }
        public static string GetStockType(string code)
        {
            string type = "";
            if (code.StartsWith("60") || code.StartsWith("999999"))
            {
                type = "sh";
            }
            else if (code.StartsWith("00") || code.StartsWith("3") || code.StartsWith("399"))
            {
                type = "sz";
            }
            return type;
        }
        /// <summary>
        /// 获取拼音码
        /// </summary>
        public static string GetSpellCode(string CnStr)
        {

            string strTemp = "";

            int iLen = CnStr.Length;

            int i = 0;

            for (i = 0; i <= iLen - 1; i++)
            {

                strTemp += GetCharSpellCode(CnStr.Substring(i, 1));
            }

            return strTemp;

        }
        /// <summary>
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母
        /// </summary>
        public static string GetCharSpellCode(string CnChar)
        {

            long iCnChar;

            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回首字母

            if (ZW.Length == 1)
            {

                return CnChar;

            }
            else
            {

                // get the array of byte from the single char

                int i1 = (short)(ZW[0]);

                int i2 = (short)(ZW[1]);

                iCnChar = i1 * 256 + i2;

            }

            // iCnChar match the constant

            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {

                return "A";

            }

            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {

                return "B";

            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {

                return "C";

            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {

                return "D";

            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {

                return "E";

            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {

                return "F";

            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {

                return "G";

            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {

                return "H";

            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {

                return "J";

            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {

                return "K";

            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {

                return "L";

            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {

                return "M";

            }
            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {

                return "N";

            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {

                return "O";

            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {

                return "P";

            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {

                return "Q";

            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {

                return "R";

            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {

                return "S";

            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {

                return "T";

            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {

                return "W";

            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {

                return "X";

            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {

                return "Y";

            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {

                return "Z";

            }
            else

                return ("?");

        }
        /// <summary>
        /// 单点图计算
        /// </summary>
        /// <param name="operateStockInfo"></param>
        /// <param name="cycleDataList"></param>
        /// <returns></returns>
        public static List<DotValue> CalculateOneDotGraphicOld(OperateStockInfo operateStockInfo, List<CycleData> cycleDataList)
        {
            List<DotValue> dotValueList = new List<DotValue>();
            //第一个点确定了吗?这要由比它格值相差>=1的第一个点确定
            //第一点是否确定
            bool isFirstDotHasStatus = false;
            for (int i = 0; i < cycleDataList.Count; i++)
            {
                CycleData cycleData = cycleDataList[i];
                int curGridIndex = 0;
                //收盘价计算格值
                //if (operateStockInfo.isClose)
                //{
                //第一个点不确定
                if (!isFirstDotHasStatus)
                {
                    if (dotValueList.Count == 0)//真的第一个点
                    {
                        Point point = new Point();
                        DotValue dotValue = new DotValue();
                        point.X = 0;
                        point.Y = Function.getGridIndexCurrent(operateStockInfo, cycleData.close, true);//当前不确定点的格值要按向下取整算
                        dotValue.position = point;
                        List<CycleData> cycleDatas = new List<CycleData>();
                        cycleDatas.Insert(0, cycleData);
                        dotValue.data = cycleDatas;
                        dotValue.isFill = false;
                        dotValue.isUp = true;//先默认涨吧,毕竟其格值计算方法和上涨时格值计算方法是一致的
                        dotValue.space = 1;//第一点不确定,当然只能是一格
                        //dotValue.isTurn = false;
                        dotValueList.Add(dotValue);
                    }
                    else//目前为止的数据格值里都还在第一个点的格值里
                    {
                        curGridIndex = Function.getGridIndexCurrent(operateStockInfo, cycleData.close, true);
                        int preGridIndex = dotValueList[0].position.Y;
                        if (curGridIndex == preGridIndex)//没突破
                        {
                            dotValueList[0].data.Insert(0, cycleData);
                        }
                        else if (curGridIndex > preGridIndex)//向上突破
                        {
                            DotValue preDot = dotValueList[0];
                            //改前面的状态
                            dotValueList[0].isUp = true;
                            dotValueList[0].isTurn = true;//第一格子点确定了
                                                          //填充空点
                            for (int j = 1; j < curGridIndex - preDot.position.Y; j++)
                            {
                                Point point1 = new Point();
                                DotValue dot = new DotValue();
                                point1.X = preDot.position.X;
                                point1.Y = preDot.position.Y + j;
                                dot.position = point1;
                                List<CycleData> cycleData1 = new List<CycleData>();
                                cycleData1.Insert(0, cycleData);
                                dot.data = cycleData1;
                                dot.isFill = true;
                                dot.isUp = true;
                                dot.space = preDot.space + j;
                                //dot.isTurn = false;
                                dotValueList.Insert(0, dot);
                            }

                            //添加当前点

                            //添加第二个格子点
                            Point point = new Point();
                            DotValue dotValue = new DotValue();
                            point.X = 0;
                            point.Y = curGridIndex;
                            dotValue.position = point;
                            List<CycleData> cycleDatas = new List<CycleData>();
                            cycleDatas.Insert(0, cycleData);
                            dotValue.data = cycleDatas;
                            dotValue.isFill = false;
                            dotValue.isUp = true;
                            dotValue.space = curGridIndex - preGridIndex + 1;
                            //dotValue.isTurn = false;
                            dotValueList.Insert(0, dotValue);

                            //终于突破第一格子点了
                            isFirstDotHasStatus = true;
                        }
                        else if (curGridIndex < preGridIndex)//向下突破
                        {
                            //改前面的状态为下跌 是下跌确定的转折点
                            dotValueList[0].isUp = false;
                            dotValueList[0].isTurn = true;

                            DotValue preDot = dotValueList[0];

                            //填充空点
                            for (int j = 1; j < preDot.position.Y - curGridIndex; j++)
                            {
                                Point point1 = new Point();
                                DotValue dot = new DotValue();
                                point1.X = preDot.position.X;
                                point1.Y = preDot.position.Y - j;
                                dot.position = point1;
                                List<CycleData> cycleData1 = new List<CycleData>();
                                cycleData1.Insert(0, cycleData);
                                dot.data = cycleData1;
                                dot.isFill = true;
                                dot.isUp = false;
                                dot.space = preDot.space + j;
                                //dot.isTurn = false;
                                dotValueList.Insert(0, dot);
                            }

                            //添加当前点

                            //添加第二个格子点 
                            //虽然是跌 但是这个格值计算方式是按照上涨算的 相差为1
                            //所以可能下跌点还在第一点呢,当然下跌的状态是确定的
                            if (preGridIndex - curGridIndex == 1)
                            {
                                dotValueList[0].data.Insert(0, cycleData);
                            }
                            //前后格值按照上涨算>1哪无论如何一定会往下记录格子点(不会在原点打转)
                            else
                            {
                                Point point = new Point();
                                DotValue dotValue = new DotValue();
                                point.X = 0;
                                point.Y = curGridIndex;
                                dotValue.position = point;
                                List<CycleData> cycleDatas = new List<CycleData>();
                                cycleDatas.Insert(0, cycleData);
                                dotValue.data = cycleDatas;
                                dotValue.isFill = false;
                                dotValue.isUp = false;
                                dotValue.space = preGridIndex - curGridIndex + 1;
                                //dotValue.isTurn = false;
                                dotValueList.Insert(0, dotValue);

                                //此时就不填充了吧,反正如果只有第一列转折空着，还好
                            }
                            //终于突破第一格子点了
                            isFirstDotHasStatus = true;
                        }
                    }
                }
                //第一个点状态已确定
                else
                {
                    DotValue preDot = dotValueList[0];
                    bool preIsUp = preDot.isUp;
                    //按照前面的状态计算格值
                    curGridIndex = Function.getGridIndexCurrent(operateStockInfo, cycleData.close, preIsUp);
                    //前面状态是涨
                    if (preIsUp)
                    {
                        //当前也是涨 
                        //添加新的格子点
                        if (curGridIndex > preDot.position.Y)
                        {
                            //填充空点
                            for (int j = 1; j < curGridIndex - preDot.position.Y; j++)
                            {
                                Point point1 = new Point();
                                DotValue dot = new DotValue();
                                point1.X = preDot.position.X;
                                point1.Y = preDot.position.Y + j;
                                dot.position = point1;
                                List<CycleData> cycleData1 = new List<CycleData>();
                                cycleData1.Insert(0, cycleData);
                                dot.data = cycleData1;
                                dot.isFill = true;
                                dot.isUp = true;
                                dot.space = preDot.space + j;
                                //dot.isTurn = false;
                                dotValueList.Insert(0, dot);
                            }
                            //添加当前点
                            Point point = new Point();
                            DotValue dotValue = new DotValue();
                            point.X = preDot.position.X;
                            point.Y = curGridIndex;
                            dotValue.position = point;
                            List<CycleData> cycleDatas = new List<CycleData>();
                            cycleDatas.Insert(0, cycleData);
                            dotValue.data = cycleDatas;
                            dotValue.isFill = false;
                            dotValue.isUp = true;
                            dotValue.space = curGridIndex - preDot.position.Y + preDot.space;
                            //dotValue.isTurn = false;
                            dotValueList.Insert(0, dotValue);
                        }
                        /*注意容易忽略的点:
                             如前期是:40.5,43.1 格值分别为40，43 ，显然是涨(假设间隔值为1)
                             现在是42.5,则按照前期是涨的计算方法算,格值为42
                             而42比43显然是下跌的,那此时能算是转折为下跌吗?
                             显然不能。因为如果此时是下跌，则要重新计算格值,按照下跌的计算方法算格值
                             新的格值是“43”.这与前面点持平。所以既不能算涨也不能算跌。
                             算平
                         */
                        //前涨 当前居然是下跌
                        else if (curGridIndex < preDot.position.Y)
                        {
                            //假下跌 其实和上一个点在同一位置 算原地踏步
                            //新的格值按照下跌的计算方法算 与前面的格值等
                            if (Function.getGridIndexCurrent(operateStockInfo, cycleData.close, !preIsUp) == preDot.position.Y)
                            {
                                dotValueList[0].data.Insert(0, cycleData);
                            }
                            //由涨转跌  转折
                            else
                            {
                                //重新算格值
                                curGridIndex = Function.getGridIndexCurrent(operateStockInfo, cycleData.close, !preIsUp);
                                Point point = new Point();
                                DotValue dotValue = new DotValue();

                                point.X = preDot.position.X + 1;//换列
                                point.Y = curGridIndex;


                                //不换列问题
                                /*
                                    从某一点由下跌转涨,再下跌再涨，且那个点的格值与当前点的格值相同,则不换列
                                    例如:48,47,48,47
                                        显然从48跌到47，然后涨到48，然后又跌到47，则最后的47不再换列
                                 */

                                //不换列
                                bool isNotTurn = operateStockInfo.isOneDotRebuild && (preDot.position.X >= 1) &&
                                             (dotValueList[0].position.Y <= dotValueList[2].position.Y) &&
                                             (point.Y <= dotValueList[1].position.Y) && preDot.position.X - 1 == dotValueList[1].position.X;
                                if (isNotTurn)
                                {
                                    point.X = preDot.position.X;

                                }

                                //填充空点
                                for (int j = 1; j < preDot.position.Y - curGridIndex; j++)
                                {
                                    Point point1 = new Point();
                                    DotValue dot = new DotValue();
                                    point1.X = point.X;
                                    point1.Y = preDot.position.Y - j;
                                    dot.position = point1;
                                    List<CycleData> cycleData1 = new List<CycleData>();
                                    cycleData1.Insert(0, cycleData);
                                    dot.data = cycleData1;
                                    dot.isFill = true;
                                    dot.isUp = false;
                                    dot.space = j;
                                    //dot.isTurn = false;
                                    dotValueList.Insert(0, dot);
                                }
                                //添加当前点

                                dotValue.position = point;
                                List<CycleData> cycleDatas = new List<CycleData>();
                                cycleDatas.Insert(0, cycleData);
                                dotValue.data = cycleDatas;
                                dotValue.isFill = false;
                                dotValue.isUp = false;//跌
                                dotValue.space = preDot.position.Y - curGridIndex;
                                //dotValue.isTurn = false;
                                dotValueList.Insert(0, dotValue);

                                //前面的点是转折点
                                preDot.isTurn = true;
                            }
                        }
                        //没有争议的原地踏步
                        else
                        {
                            dotValueList[0].data.Insert(0, cycleData);
                        }
                    }
                    //前面状态是跌
                    else
                    {
                        //重新计算格值  其实暂时不需要  此处方便以后看
                        curGridIndex = Function.getGridIndexCurrent(operateStockInfo, cycleData.close, preIsUp);

                        //当前也是跌 
                        //添加新的格子点
                        if (curGridIndex < preDot.position.Y)
                        {
                            //填充空点
                            for (int j = 1; j < preDot.position.Y - curGridIndex; j++)
                            {
                                Point point1 = new Point();
                                DotValue dot = new DotValue();
                                point1.X = preDot.position.X;
                                point1.Y = preDot.position.Y - j;
                                dot.position = point1;
                                List<CycleData> cycleData1 = new List<CycleData>();
                                cycleData1.Insert(0, cycleData);
                                dot.data = cycleData1;
                                dot.isFill = true;
                                dot.isUp = false;
                                dot.space = preDot.space + j;
                                //dot.isTurn = false;
                                dotValueList.Insert(0, dot);
                            }
                            //添加当前点

                            Point point = new Point();
                            DotValue dotValue = new DotValue();
                            point.X = preDot.position.X;
                            point.Y = curGridIndex;
                            dotValue.position = point;
                            List<CycleData> cycleDatas = new List<CycleData>();
                            cycleDatas.Insert(0, cycleData);
                            dotValue.data = cycleDatas;
                            dotValue.isFill = false;
                            dotValue.isUp = false;
                            dotValue.space = preDot.position.Y - curGridIndex + preDot.space;
                            dotValue.isTurn = false;
                            dotValueList.Insert(0, dotValue);
                        }
                        /*注意容易忽略的点:
                             如前期是:43.1,40.5, 格值分别为44，41 (按照跌的计算方法算)，显然是跌(假设间隔值为1)
                             现在是41.1,则按照前期是跌的计算方法算,格值为42
                             而42比41显然是涨,那此时能算是转折为上涨吗?
                             显然不能。因为如果此时是涨，则要重新计算格值,按照涨的计算方法算格值
                             新的格值是“41”.这与前面点持平。所以既不能算涨也不能算跌。
                             算平
                         */
                        //前跌 当前居然是涨
                        else if (curGridIndex > preDot.position.Y)
                        {
                            //假上涨 其实和上一个点在同一位置 算原地踏步
                            //新的格值按照涨的计算方法算 与前面的格值等
                            if (Function.getGridIndexCurrent(operateStockInfo, cycleData.close, !preIsUp) == preDot.position.Y)
                            {
                                dotValueList[0].data.Insert(0, cycleData);
                            }
                            //由跌转涨
                            else
                            {
                                //重新算格值
                                curGridIndex = Function.getGridIndexCurrent(operateStockInfo, cycleData.close, !preIsUp);
                                Point point = new Point();
                                DotValue dotValue = new DotValue();
                                point.X = preDot.position.X + 1;//换列
                                point.Y = curGridIndex;

                                //不换列问题
                                /*
                                    从某一点由下跌专转涨,儿下跌是涨，且那个点的格值与当前点的格值相同,则不换列
                                    例如:47,48,47,48
                                        显然从48跌到47，然后涨到48，然后又跌到47，则最后的47不再换列
                                 */

                                //不换列
                                bool isNotTurn = operateStockInfo.isOneDotRebuild && (preDot.position.X >= 1) &&
                                    (preDot.position.Y >= dotValueList[2].position.Y) &&
                                    (point.Y >= dotValueList[1].position.Y) && preDot.position.X - 1 == dotValueList[1].position.X;
                                if (isNotTurn)
                                {
                                    point.X = preDot.position.X;
                                }
                                //填充空点
                                for (int j = 1; j < curGridIndex - preDot.position.Y; j++)
                                {
                                    Point point1 = new Point();
                                    DotValue dot = new DotValue();
                                    point1.X = point.X;
                                    point1.Y = preDot.position.Y + j;
                                    dot.position = point1;
                                    List<CycleData> cycleData1 = new List<CycleData>();
                                    cycleData1.Insert(0, cycleData);
                                    dot.data = cycleData1;
                                    dot.isFill = true;
                                    dot.isUp = true;
                                    dot.space = j;
                                    dot.isTurn = false;
                                    dotValueList.Insert(0, dot);
                                }
                                dotValue.position = point;
                                List<CycleData> cycleDatas = new List<CycleData>();
                                cycleDatas.Insert(0, cycleData);
                                dotValue.data = cycleDatas;
                                dotValue.isFill = false;
                                dotValue.isUp = true;//涨
                                dotValue.space = curGridIndex - preDot.position.Y;
                                dotValue.isTurn = false;
                                dotValueList.Insert(0, dotValue);

                                //前面的点是转折点
                                preDot.isTurn = true;
                            }
                        }
                        //没有争议的原地踏步
                        else
                        {
                            dotValueList[0].data.Insert(0, cycleData);
                        }
                    }
                }
            }
            return dotValueList;
        }


        /// <summary>
        /// 单点图计算  高低点
        /// </summary>
        /// <param name="operateStockInfo"></param>
        /// <param name="cycleDataList"></param>
        /// <returns></returns>
        public static List<DotValue> CalculateOneDotGraphic(OperateStockInfo operateStockInfo, List<CycleData> cycleDataList)
        {
            List<DotValue> dotValueList = new List<DotValue>();
            //第一个点确定了吗?这要由比它格值相差>=1的第一个点确定
            //第一点是否确定
            bool isFirstDotHasStatus = false;
            for (int i = 1; i < cycleDataList.Count; i++)
            {
                CycleData curData = cycleDataList[i];
                CycleData preData = cycleDataList[i - 1];
                double increaseRate = 1 - curData.high / preData.low;//前低到后高涨幅
                double decreaseRate = 1 - curData.low / preData.high;//前高到后低跌幅
                int startIndex = 0;//开始填充点(格值)
                int endIndex = 0;//结束填充点
                                 //收盘价计算格值
                int curGridIndex = 0;
                //收盘价计算格值
                if (operateStockInfo.isClose)
                {
                    //第一个点不确定
                    if (!isFirstDotHasStatus)
                    {
                        if (dotValueList.Count == 0)//真的第一个点
                        {
                            Point point = new Point();
                            DotValue dotValue = new DotValue();
                            point.X = 0;
                            point.Y = Function.getGridIndexCurrent(operateStockInfo, curData.close, true);//当前不确定点的格值要按向下取整算
                            dotValue.position = point;
                            List<CycleData> cycleDatas = new List<CycleData>();
                            cycleDatas.Insert(0, curData);
                            dotValue.data = cycleDatas;
                            dotValue.isFill = false;
                            dotValue.isUp = true;//先默认涨吧,毕竟其格值计算方法和上涨时格值计算方法是一致的
                            dotValue.space = 1;//第一点不确定,当然只能是一格
                                               //dotValue.isTurn = false;
                            dotValueList.Add(dotValue);
                        }
                        else//目前为止的数据格值里都还在第一个点的格值里
                        {
                            curGridIndex = Function.getGridIndexCurrent(operateStockInfo, curData.close, true);
                            int preGridIndex = dotValueList[0].position.Y;
                            if (curGridIndex == preGridIndex)//没突破
                            {
                                dotValueList[0].data.Insert(0, curData);
                            }
                            else if (curGridIndex > preGridIndex)//向上突破
                            {
                                DotValue preDot = dotValueList[0];
                                //改前面的状态
                                dotValueList[0].isUp = true;
                                dotValueList[0].isTurn = true;//第一格子点确定了
                                                              //填充空点
                                for (int j = 1; j < curGridIndex - preDot.position.Y; j++)
                                {
                                    Point point1 = new Point();
                                    DotValue dot = new DotValue();
                                    point1.X = preDot.position.X;
                                    point1.Y = preDot.position.Y + j;
                                    dot.position = point1;
                                    List<CycleData> cycleData1 = new List<CycleData>();
                                    cycleData1.Insert(0, curData);
                                    dot.data = cycleData1;
                                    dot.isFill = true;
                                    dot.isUp = true;
                                    dot.space = preDot.space + j;
                                    //dot.isTurn = false;
                                    dotValueList.Insert(0, dot);
                                }

                                //添加当前点

                                //添加第二个格子点
                                Point point = new Point();
                                DotValue dotValue = new DotValue();
                                point.X = 0;
                                point.Y = curGridIndex;
                                dotValue.position = point;
                                List<CycleData> cycleDatas = new List<CycleData>();
                                cycleDatas.Insert(0, curData);
                                dotValue.data = cycleDatas;
                                dotValue.isFill = false;
                                dotValue.isUp = true;
                                dotValue.space = curGridIndex - preGridIndex + 1;
                                //dotValue.isTurn = false;
                                dotValueList.Insert(0, dotValue);

                                //终于突破第一格子点了
                                isFirstDotHasStatus = true;
                            }
                            else if (curGridIndex < preGridIndex)//向下突破
                            {
                                //改前面的状态为下跌 是下跌确定的转折点
                                dotValueList[0].isUp = false;
                                dotValueList[0].isTurn = true;

                                DotValue preDot = dotValueList[0];

                                //填充空点
                                for (int j = 1; j < preDot.position.Y - curGridIndex; j++)
                                {
                                    Point point1 = new Point();
                                    DotValue dot = new DotValue();
                                    point1.X = preDot.position.X;
                                    point1.Y = preDot.position.Y - j;
                                    dot.position = point1;
                                    List<CycleData> cycleData1 = new List<CycleData>();
                                    cycleData1.Insert(0, curData);
                                    dot.data = cycleData1;
                                    dot.isFill = true;
                                    dot.isUp = false;
                                    dot.space = preDot.space + j;
                                    //dot.isTurn = false;
                                    dotValueList.Insert(0, dot);
                                }

                                //添加当前点

                                //添加第二个格子点 
                                //虽然是跌 但是这个格值计算方式是按照上涨算的 相差为1
                                //所以可能下跌点还在第一点呢,当然下跌的状态是确定的
                                if (preGridIndex - curGridIndex == 1)
                                {
                                    dotValueList[0].data.Insert(0, curData);
                                }
                                //前后格值按照上涨算>1哪无论如何一定会往下记录格子点(不会在原点打转)
                                else
                                {
                                    Point point = new Point();
                                    DotValue dotValue = new DotValue();
                                    point.X = 0;
                                    point.Y = curGridIndex;
                                    dotValue.position = point;
                                    List<CycleData> cycleDatas = new List<CycleData>();
                                    cycleDatas.Insert(0, curData);
                                    dotValue.data = cycleDatas;
                                    dotValue.isFill = false;
                                    dotValue.isUp = false;
                                    dotValue.space = preGridIndex - curGridIndex + 1;
                                    //dotValue.isTurn = false;
                                    dotValueList.Insert(0, dotValue);

                                    //此时就不填充了吧,反正如果只有第一列转折空着，还好
                                }
                                //终于突破第一格子点了
                                isFirstDotHasStatus = true;
                            }
                        }
                    }
                    //第一个点状态已确定
                    else
                    {
                        DotValue preDot = dotValueList[0];
                        bool preIsUp = preDot.isUp;
                        //按照前面的状态计算格值
                        curGridIndex = Function.getGridIndexCurrent(operateStockInfo, curData.close, preIsUp);
                        //前面状态是涨
                        if (preIsUp)
                        {
                            //当前也是涨 
                            //添加新的格子点
                            if (curGridIndex > preDot.position.Y)
                            {
                                //填充空点
                                for (int j = 1; j < curGridIndex - preDot.position.Y; j++)
                                {
                                    Point point1 = new Point();
                                    DotValue dot = new DotValue();
                                    point1.X = preDot.position.X;
                                    point1.Y = preDot.position.Y + j;
                                    dot.position = point1;
                                    List<CycleData> cycleData1 = new List<CycleData>();
                                    cycleData1.Insert(0, curData);
                                    dot.data = cycleData1;
                                    dot.isFill = true;
                                    dot.isUp = true;
                                    dot.space = preDot.space + j;
                                    //dot.isTurn = false;
                                    dotValueList.Insert(0, dot);
                                }
                                //添加当前点
                                Point point = new Point();
                                DotValue dotValue = new DotValue();
                                point.X = preDot.position.X;
                                point.Y = curGridIndex;
                                dotValue.position = point;
                                List<CycleData> cycleDatas = new List<CycleData>();
                                cycleDatas.Insert(0, curData);
                                dotValue.data = cycleDatas;
                                dotValue.isFill = false;
                                dotValue.isUp = true;
                                dotValue.space = curGridIndex - preDot.position.Y + preDot.space;
                                //dotValue.isTurn = false;
                                dotValueList.Insert(0, dotValue);
                            }
                            /*注意容易忽略的点:
                                 如前期是:40.5,43.1 格值分别为40，43 ，显然是涨(假设间隔值为1)
                                 现在是42.5,则按照前期是涨的计算方法算,格值为42
                                 而42比43显然是下跌的,那此时能算是转折为下跌吗?
                                 显然不能。因为如果此时是下跌，则要重新计算格值,按照下跌的计算方法算格值
                                 新的格值是“43”.这与前面点持平。所以既不能算涨也不能算跌。
                                 算平
                             */
                            //前涨 当前居然是下跌
                            else if (curGridIndex < preDot.position.Y)
                            {
                                //假下跌 其实和上一个点在同一位置 算原地踏步
                                //新的格值按照下跌的计算方法算 与前面的格值等
                                if (Function.getGridIndexCurrent(operateStockInfo, curData.close, !preIsUp) == preDot.position.Y)
                                {
                                    dotValueList[0].data.Insert(0, curData);
                                }
                                //由涨转跌  转折
                                else
                                {
                                    //重新算格值
                                    curGridIndex = Function.getGridIndexCurrent(operateStockInfo, curData.close, !preIsUp);
                                    Point point = new Point();
                                    DotValue dotValue = new DotValue();

                                    point.X = preDot.position.X + 1;//换列
                                    point.Y = curGridIndex;


                                    //不换列问题
                                    /*
                                        从某一点由下跌转涨,再下跌再涨，且那个点的格值与当前点的格值相同,则不换列
                                        例如:48,47,48,47
                                            显然从48跌到47，然后涨到48，然后又跌到47，则最后的47不再换列
                                     */

                                    //不换列
                                    bool isNotTurn = operateStockInfo.isOneDotRebuild && operateStockInfo.dotInterval == 1 && (preDot.position.X >= 1) &&
                                                 (dotValueList[0].position.Y <= dotValueList[2].position.Y) &&
                                                 (point.Y <= dotValueList[1].position.Y) && preDot.position.X - 1 == dotValueList[1].position.X;
                                    if (isNotTurn)
                                    {
                                        point.X = preDot.position.X;

                                    }

                                    //填充空点
                                    for (int j = 1; j < preDot.position.Y - curGridIndex; j++)
                                    {
                                        Point point1 = new Point();
                                        DotValue dot = new DotValue();
                                        point1.X = point.X;
                                        point1.Y = preDot.position.Y - j;
                                        dot.position = point1;
                                        List<CycleData> cycleData1 = new List<CycleData>();
                                        cycleData1.Insert(0, curData);
                                        dot.data = cycleData1;
                                        dot.isFill = true;
                                        dot.isUp = false;
                                        dot.space = j;
                                        //dot.isTurn = false;
                                        dotValueList.Insert(0, dot);
                                    }
                                    //添加当前点

                                    dotValue.position = point;
                                    List<CycleData> cycleDatas = new List<CycleData>();
                                    cycleDatas.Insert(0, curData);
                                    dotValue.data = cycleDatas;
                                    dotValue.isFill = false;
                                    dotValue.isUp = false;//跌
                                    dotValue.space = preDot.position.Y - curGridIndex;
                                    //dotValue.isTurn = false;
                                    dotValueList.Insert(0, dotValue);

                                    //前面的点是转折点
                                    preDot.isTurn = true;
                                }
                            }
                            //没有争议的原地踏步
                            else
                            {
                                dotValueList[0].data.Insert(0, curData);
                            }
                        }
                        //前面状态是跌
                        else
                        {
                            //重新计算格值  其实暂时不需要  此处方便以后看
                            curGridIndex = Function.getGridIndexCurrent(operateStockInfo, curData.close, preIsUp);

                            //当前也是跌 
                            //添加新的格子点
                            if (curGridIndex < preDot.position.Y)
                            {
                                //填充空点
                                for (int j = 1; j < preDot.position.Y - curGridIndex; j++)
                                {
                                    Point point1 = new Point();
                                    DotValue dot = new DotValue();
                                    point1.X = preDot.position.X;
                                    point1.Y = preDot.position.Y - j;
                                    dot.position = point1;
                                    List<CycleData> cycleData1 = new List<CycleData>();
                                    cycleData1.Insert(0, curData);
                                    dot.data = cycleData1;
                                    dot.isFill = true;
                                    dot.isUp = false;
                                    dot.space = preDot.space + j;
                                    //dot.isTurn = false;
                                    dotValueList.Insert(0, dot);
                                }
                                //添加当前点

                                Point point = new Point();
                                DotValue dotValue = new DotValue();
                                point.X = preDot.position.X;
                                point.Y = curGridIndex;
                                dotValue.position = point;
                                List<CycleData> cycleDatas = new List<CycleData>();
                                cycleDatas.Insert(0, curData);
                                dotValue.data = cycleDatas;
                                dotValue.isFill = false;
                                dotValue.isUp = false;
                                dotValue.space = preDot.position.Y - curGridIndex + preDot.space;
                                dotValue.isTurn = false;
                                dotValueList.Insert(0, dotValue);
                            }
                            /*注意容易忽略的点:
                                 如前期是:43.1,40.5, 格值分别为44，41 (按照跌的计算方法算)，显然是跌(假设间隔值为1)
                                 现在是41.1,则按照前期是跌的计算方法算,格值为42
                                 而42比41显然是涨,那此时能算是转折为上涨吗?
                                 显然不能。因为如果此时是涨，则要重新计算格值,按照涨的计算方法算格值
                                 新的格值是“41”.这与前面点持平。所以既不能算涨也不能算跌。
                                 算平
                             */
                            //前跌 当前居然是涨
                            else if (curGridIndex > preDot.position.Y)
                            {
                                //假上涨 其实和上一个点在同一位置 算原地踏步
                                //新的格值按照涨的计算方法算 与前面的格值等
                                if (Function.getGridIndexCurrent(operateStockInfo, curData.close, !preIsUp) == preDot.position.Y)
                                {
                                    dotValueList[0].data.Insert(0, curData);
                                }
                                //由跌转涨
                                else
                                {
                                    //重新算格值
                                    curGridIndex = Function.getGridIndexCurrent(operateStockInfo, curData.close, !preIsUp);
                                    Point point = new Point();
                                    DotValue dotValue = new DotValue();
                                    point.X = preDot.position.X + 1;//换列
                                    point.Y = curGridIndex;

                                    //不换列问题
                                    /*
                                        从某一点由下跌专转涨,儿下跌是涨，且那个点的格值与当前点的格值相同,则不换列
                                        例如:47,48,47,48
                                            显然从48跌到47，然后涨到48，然后又跌到47，则最后的47不再换列
                                     */

                                    //不换列
                                    bool isNotTurn = operateStockInfo.isOneDotRebuild && operateStockInfo.dotInterval == 1 && (preDot.position.X >= 1) &&
                                        (preDot.position.Y >= dotValueList[2].position.Y) &&
                                        (point.Y >= dotValueList[1].position.Y) && preDot.position.X - 1 == dotValueList[1].position.X;
                                    if (isNotTurn)
                                    {
                                        point.X = preDot.position.X;
                                    }
                                    //填充空点
                                    for (int j = 1; j < curGridIndex - preDot.position.Y; j++)
                                    {
                                        Point point1 = new Point();
                                        DotValue dot = new DotValue();
                                        point1.X = point.X;
                                        point1.Y = preDot.position.Y + j;
                                        dot.position = point1;
                                        List<CycleData> cycleData1 = new List<CycleData>();
                                        cycleData1.Insert(0, curData);
                                        dot.data = cycleData1;
                                        dot.isFill = true;
                                        dot.isUp = true;
                                        dot.space = j;
                                        dot.isTurn = false;
                                        dotValueList.Insert(0, dot);
                                    }
                                    dotValue.position = point;
                                    List<CycleData> cycleDatas = new List<CycleData>();
                                    cycleDatas.Insert(0, curData);
                                    dotValue.data = cycleDatas;
                                    dotValue.isFill = false;
                                    dotValue.isUp = true;//涨
                                    dotValue.space = curGridIndex - preDot.position.Y;
                                    dotValue.isTurn = false;
                                    dotValueList.Insert(0, dotValue);

                                    //前面的点是转折点
                                    preDot.isTurn = true;
                                }
                            }
                            //没有争议的原地踏步
                            else
                            {
                                dotValueList[0].data.Insert(0, curData);
                            }
                        }
                    }
                }
                else//高低价
                {
                    //第一个点不确定时先算第一个点
                    if (!isFirstDotHasStatus)
                    {
                        /*
                        第一个点的确认：
                           1、获取第一天最低价到第二天最高价的涨幅，与第一天最高价到第二天最低价的跌幅
                              如果涨幅超过跌幅，则从第一天最低价到第二天最高价画X,反之第一天最高价到第二天最低价画O
                        */
                        //确定第一点为涨
                        if (increaseRate > decreaseRate)
                        {
                            //第一天最低价到第二天最高价的涨幅
                            startIndex = Function.getGridIndexCurrent(operateStockInfo, preData.low, false);
                            endIndex = Function.getGridIndexCurrent(operateStockInfo, curData.high, true);
                            for (int j = startIndex; j <= endIndex; j++)
                            {
                                Point point = new Point();
                                DotValue dot = new DotValue();
                                point.X = 0;
                                point.Y = j;
                                dot.position = point;
                                List<CycleData> cycleData = new List<CycleData>();
                                cycleData.Insert(0, curData);
                                dot.data = cycleData;
                                dot.isFill = true;
                                dot.isUp = true;
                                dot.space = j + 1 - startIndex;
                                //dot.isTurn = false;
                                dotValueList.Insert(0, dot);
                            }
                            //区分填充点
                            if (startIndex < endIndex)
                            {
                                dotValueList[0].isFill = false;
                                dotValueList[dotValueList.Count - 1].isFill = false;
                            }
                            isFirstDotHasStatus = true;

                        }
                        else if (increaseRate <= decreaseRate)
                        {
                            //第一天最高价到第二天最低价的跌幅
                            startIndex = Function.getGridIndexCurrent(operateStockInfo, preData.high, true);
                            endIndex = Function.getGridIndexCurrent(operateStockInfo, curData.low, true);
                            for (int j = startIndex; j >= endIndex; j--)
                            {
                                Point point = new Point();
                                DotValue dot = new DotValue();
                                point.X = 0;
                                point.Y = j;
                                dot.position = point;
                                List<CycleData> cycleData = new List<CycleData>();
                                cycleData.Insert(0, curData);
                                dot.data = cycleData;
                                dot.isFill = true;
                                dot.isUp = false;
                                dot.space = startIndex - j + 1;
                                //dot.isTurn = false;
                                dotValueList.Insert(0, dot);
                            }
                            //区分填充点
                            if (startIndex > endIndex)
                            {
                                dotValueList[0].isFill = false;
                                dotValueList[dotValueList.Count - 1].isFill = false;
                            }
                            isFirstDotHasStatus = true;
                        }

                    }
                    //第一个点状态已确定
                    else
                    {
                        DotValue preDot = dotValueList[0];//点是插入的，所以0点永远是前面的点
                        bool preIsUp = preDot.isUp;

                        if (preIsUp)//前面是涨
                        {

                            startIndex = preDot.position.Y;
                            endIndex = getGridIndexCurrent(operateStockInfo, curData.high, true);
                            //当前创新高 且升高大于等于一格
                            if (curData.high > preData.high && endIndex > startIndex)
                            {
                                startIndex = preDot.position.Y;
                                endIndex = getGridIndexCurrent(operateStockInfo, curData.high, true);
                                for (int j = startIndex + 1; j <= endIndex; j++)
                                {
                                    Point point = new Point();
                                    DotValue dot = new DotValue();
                                    point.X = preDot.position.X;
                                    point.Y = j;
                                    dot.position = point;
                                    List<CycleData> cycleData = new List<CycleData>();
                                    cycleData.Insert(0, curData);
                                    dot.data = cycleData;
                                    dot.isFill = true;
                                    dot.isUp = true;
                                    dot.space = j - startIndex + preDot.space;
                                    dotValueList.Insert(0, dot);
                                }
                                //区分填充点
                                if (startIndex < endIndex)
                                {
                                    dotValueList[0].isFill = false;
                                }

                            }
                            //不创新高
                            else
                            {
                                startIndex = preDot.position.Y;
                                endIndex = getGridIndexCurrent(operateStockInfo, curData.low, false);
                                //前涨 后创新低 且降低大于等于转折格数
                                if (startIndex-endIndex>=operateStockInfo.dotInterval)
                                {
                                    //列
                                    int indexX = preDot.position.X+1;

                                    //满足换列
                                    //if (startIndex - endIndex >= operateStockInfo.dotInterval)
                                    //{
                                    //    indexX = preDot.position.X + 1;
                                    //}

                                    //单格转折不换列
                                    bool isNotTurn = operateStockInfo.isOneDotRebuild && operateStockInfo.dotInterval==1 && (preDot.position.X >= 1) &&
                                                 (dotValueList[0].position.Y <= dotValueList[2].position.Y) &&
                                                 (endIndex <= dotValueList[1].position.Y) && preDot.position.X - 1 == dotValueList[1].position.X;
                                    if (isNotTurn)
                                    {
                                        indexX = preDot.position.X;

                                    }

                                    //填充点
                                    for (int j = startIndex - 1; j >= endIndex; j--)
                                    {
                                        Point point = new Point();
                                        DotValue dot = new DotValue();
                                        point.X = indexX;
                                        point.Y = j;
                                        dot.position = point;
                                        List<CycleData> cycleData = new List<CycleData>();
                                        cycleData.Insert(0, curData);
                                        dot.data = cycleData;
                                        dot.isFill = true;
                                        dot.isUp = false;
                                        dot.space = j - startIndex + 1;
                                        dotValueList.Insert(0, dot);
                                    }
                                    //区分填充点
                                    if (startIndex > endIndex)
                                    {
                                        dotValueList[0].isFill = false;
                                        dotValueList[dotValueList.Count - 1].isFill = false;
                                    }

                                }
                                //无新高 新低 原地踏步
                                else
                                {
                                    dotValueList[0].data.Insert(0, curData);
                                }
                            }
                        }
                        else//前面是跌
                        {
                            startIndex = preDot.position.Y;
                            endIndex = getGridIndexCurrent(operateStockInfo, curData.low, false);
                            //当前创新低 且降低大于等于一个格
                            if (curData.low < preData.low && startIndex > endIndex)
                            {

                                for (int j = startIndex; j >= endIndex; j--)
                                {
                                    Point point = new Point();
                                    DotValue dot = new DotValue();
                                    point.X = preDot.position.X;
                                    point.Y = j;
                                    dot.position = point;
                                    List<CycleData> cycleData = new List<CycleData>();
                                    cycleData.Insert(0, curData);
                                    dot.data = cycleData;
                                    dot.isFill = true;
                                    dot.isUp = false;
                                    dot.space = startIndex - j + preDot.space;
                                    dotValueList.Insert(0, dot);
                                }
                                //区分填充点
                                if (startIndex > endIndex)
                                {
                                    dotValueList[0].isFill = false;
                                }

                            }
                            //不创新低
                            else
                            {
                                startIndex = preDot.position.Y;
                                endIndex = getGridIndexCurrent(operateStockInfo, curData.high, true);
                                //创新高 且身高大于等于一格
                                if (endIndex - startIndex>=operateStockInfo.dotInterval)
                                {
                                    //列
                                    int indexX = preDot.position.X+1;
                                    //满足换列
                                    //if (endIndex - startIndex >= operateStockInfo.dotInterval)
                                    //{
                                    //    indexX = preDot.position.X + 1;
                                    //}

                                    //单格转折不换列
                                    bool isNotTurn = operateStockInfo.isOneDotRebuild && operateStockInfo.dotInterval == 1 && (preDot.position.X >= 1) &&
                                        (preDot.position.Y >= dotValueList[2].position.Y) &&
                                        (endIndex >= dotValueList[1].position.Y) && preDot.position.X - 1 == dotValueList[1].position.X;
                                    if (isNotTurn)
                                    {
                                        indexX = preDot.position.X;
                                    }
                                    //填充点
                                    if (endIndex - startIndex >= operateStockInfo.dotInterval)
                                    {
                                        for (int j = startIndex + 1; j <= endIndex; j++)
                                        {
                                            Point point = new Point();
                                            DotValue dot = new DotValue();
                                            point.X = indexX;
                                            point.Y = j;
                                            dot.position = point;
                                            List<CycleData> cycleData = new List<CycleData>();
                                            cycleData.Insert(0, curData);
                                            dot.data = cycleData;
                                            dot.isFill = true;
                                            dot.isUp = true;
                                            dot.space = j - startIndex;
                                            dotValueList.Insert(0, dot);
                                        }
                                        //区分填充点
                                        if (startIndex > endIndex)
                                        {
                                            dotValueList[0].isFill = false;
                                        }
                                    }
                                }
                                //不转折 无新高无新低
                                else
                                {
                                    dotValueList[0].data.Insert(0, curData);
                                }
                            }
                        }
                    }
                }
            }
            return dotValueList;
        }

        /// <summary>
        /// 计算单多点图  
        /// 注意:此函数依赖单点图的填充,所以单点图函数一定不能把填充点去掉
        /// 未解决问题:isOneDotRebuild的处理.即当单个反转点不换列时,
        /// 导致的多点图中转折点的开始位置后移，这将导致少量的涨或跌列少掉前面的点
        /// </summary>
        /// <param name="cycleDataList"></param>
        /// <param name="dotSpace"></param>
        /// <returns></returns>
        public static List<DotValue> CalculateMultipleDotGraphic(List<CycleData> cycleDataList, OperateStockInfo operateStockInfo)
        {
            List<DotValue> dotValueListOne = Function.CalculateOneDotGraphic(operateStockInfo, cycleDataList);
            List<DotValue> dotValueListMultiple = new List<DotValue>();
            List<DotTurn> dotTurnList = new List<DotTurn>();//转折点集合

            //当前列涨跌方向
            bool multiIsUp = false;
            //第一列的涨跌方向是否确定了
            bool isHasFirstColumn = false;

            //顺序反转一下
            dotValueListOne.Reverse();

            //找出反转点和反转状态
            //符合转向的点确定其涨跌状态 其下一个符合反向转向的点确定其边界


            int curUpSpace = 0;//当前为涨的格子累计值 计算规则:上涨段space每次减去下降段space必须>=0，下同
            int curDownSpace = 0;//当前为跌的格子累计值
            DotTurn dotTurn = new DotTurn();//转折点

            for (int i = 0; i < dotValueListOne.Count - 1; i++)
            {
                DotValue curDot = dotValueListOne[i];
                DotValue nextDot = dotValueListOne[i + 1];
                //多点点图合并 涨跌方向不一致
                //条件:当前点转折格数>=dotSpace 且下一个点的转折格数=1 则为符合转折格数的转折点

                /*问题:当前不是一次性的格数>=dotSpace，而是合理的累计>=dotSpace怎么算???这是难点
                   方法:从第一个点开始算,上涨则curUpIndex+1，curDownIndex-1,下跌反过来
                        谁先>=dotSpace,则奠定第一列涨跌状态另一个清零
                 */
                if (!isHasFirstColumn)
                {
                    curUpSpace += curDot.isUp ? 1 : -1;
                    curDownSpace += curDot.isUp ? -1 : 1;
                    if (curUpSpace >= operateStockInfo.dotInterval)
                    {
                        multiIsUp = true;
                        curDownSpace = 0;
                        isHasFirstColumn = true;
                        dotTurn.index = i;
                        dotTurn.isUp = multiIsUp;
                    }
                    if (curDownSpace >= operateStockInfo.dotInterval)
                    {
                        multiIsUp = false;
                        curUpSpace = 0;
                        isHasFirstColumn = true;
                        dotTurn.index = i;
                        dotTurn.isUp = multiIsUp;
                    }
                }
                else
                {
                    if (curDot.isUp == multiIsUp)//当前点顺势 大势+当点势
                    {
                        if (multiIsUp)//涨涨
                        {
                            curUpSpace += 1;
                            if (curDownSpace > 0)
                            {
                                curDownSpace += -1;
                            }

                        }
                        //跌跌
                        else
                        {
                            curDownSpace += 1;
                            if (curUpSpace > 0)
                            {
                                curUpSpace += -1;
                            }
                        }
                        dotTurn.index = i;
                    }
                    //当前点逆势
                    else
                    {
                        if (multiIsUp)//涨跌
                        {
                            curDownSpace += 1;
                            if (curDownSpace >= operateStockInfo.dotInterval)
                            {
                                multiIsUp = !multiIsUp;
                                dotTurnList.Add(dotTurn);
                                dotTurn = new DotTurn();
                                dotTurn.index = i;
                                dotTurn.isUp = multiIsUp;
                                curUpSpace = 0;//转折 上涨space归0
                            }
                            //if (isOneDotRebuild)
                            //{
                            //    if (i > 0 && curDot.position.X == dotValueListOne[i - 1].position.X && curDot.isUp != dotValueListOne[i - 1].isUp)
                            //    {
                            //        dotTurn.index = i - 1;
                            //    }
                            //}
                        }
                        else//跌涨
                        {
                            curUpSpace += 1;
                            if (curUpSpace >= operateStockInfo.dotInterval)
                            {
                                multiIsUp = !multiIsUp;
                                dotTurnList.Add(dotTurn);
                                dotTurn = new DotTurn();
                                dotTurn.index = i;
                                dotTurn.isUp = multiIsUp;
                                curDownSpace = 0;//转折 下跌space归0
                            }
                            //if(isOneDotRebuild)
                            //{
                            //    if(i>0 && curDot.position.X== dotValueListOne[i-1].position.X && curDot.isUp!= dotValueListOne[i - 1].isUp)
                            //    {
                            //        dotTurn.index = i-1;
                            //    }
                            //}
                        }
                    }
                }
            }
            //签一个转折点遇到下一个转折点才归位 所以最后一个别忘了归位
            dotTurnList.Add(dotTurn);
            int start = 0;
            for (int i = 0; i < dotTurnList.Count; i++)
            {
                int end = dotTurnList[i].index;
                for (int j = start; j <= end; j++)
                {
                    DotValue dotValue = new DotValue();
                    dotValue.position.X = i;
                    dotValue.position.Y = dotValueListOne[j].position.Y;
                    dotValue.isUp = dotTurnList[i].isUp;
                    dotValue.data = dotValueListOne[j].data;
                    dotValue.isFill = dotValueListOne[j].isFill;
                    dotValue.space = dotValueListOne[j].position.Y - dotValueListOne[start].position.Y;
                    dotValueListMultiple.Add(dotValue);
                }
                start = end + 1;
            }

            //剩下的部分直接挪过来
            if (dotTurnList.Count == 0 || dotTurnList[dotTurnList.Count - 1].index < (dotValueListOne.Count - 1))
            {
                for (int i = start; i < dotValueListOne.Count; i++)
                {
                    DotValue dotValue = new DotValue();
                    dotValue.position.X = dotValueListOne[i].position.X - (dotValueListOne[dotValueListOne.Count - 1].position.X - dotTurnList.Count);
                    dotValue.position.Y = dotValueListOne[i].position.Y;
                    dotValue.isUp = dotValueListOne[i].isUp;
                    dotValue.data = dotValueListOne[i].data;
                    dotValue.isFill = dotValueListOne[i].isFill;
                    dotValue.space = dotValueListOne[i].space;
                    dotValueListMultiple.Add(dotValue);
                }
            }

            return dotValueListMultiple;
        }

        /// <summary>
        /// 计算高低价格值
        /// </summary>
        /// <param name="preDot"></param>
        /// <param name="curDot"></param>
        /// <returns></returns>
        public static int getGridIndexHL(OperateStockInfo operateStockInfo, CycleData curDot, int preGridIndexHL)
        {
            int gridIndexHigh = ((int)Math.Ceiling((curDot.high - operateStockInfo.startValue) / (operateStockInfo.latticeValue)));
            int gridIndexLow = ((int)Math.Ceiling((curDot.low - operateStockInfo.startValue) / (operateStockInfo.latticeValue)));

            if (gridIndexHigh > preGridIndexHL)//最高价形成新的格子
            {
                return gridIndexHigh;
            }
            //这里有个问题:如果最高 最低价都形成新的格子,最高价在前面 ，则是否就最高价算?????
            else if (gridIndexLow < preGridIndexHL)//最低价形成新的格子
            {
                return gridIndexLow;
            }
            //都不形成新格子,是否二者格值一致
            else if (gridIndexHigh == gridIndexLow)
            {
                //return ((int)Math.Ceiling((curDot.close - operateStockInfo.startValue) / (operateStockInfo.latticeValue)));
                return gridIndexHigh;
            }
            else
            {
                //MessageBox.Show("计算格值有问题,日期：" + curDot.date);
                return 0;
            }
        }
        /// <summary>
        /// 根据当前点和前面点的状态计算当前点的格值
        /// 1、在上涨状态确定时向下取整(如价格=44.01~44.99,起示位置0,一格子单位长1,则格值为44)
        /// 2、在下跌状态确定时向上取整(上面的例子格值为45)
        /// </summary>
        /// <param name="operateStockInfo"></param>
        /// <param name="curDot"></param>
        /// <param name="preIsUp">前面点的涨跌状态 ,这决定了当前点的取整方式</param>
        /// <returns></returns>
        // public static int getGridIndex(OperateStockInfo operateStockInfo, CycleData curDot, bool preIsUp)
        //{
        //    int gridIndex = 0;

        //    if (operateStockInfo.isClose)
        //    {
        //        //向上取整
        //        int cellingIndex = ((int)Math.Ceiling((curDot.close) / (operateStockInfo.latticeValue)));
        //        //向下取整
        //        int floorIndex = ((int)Math.Floor((curDot.close) / (operateStockInfo.latticeValue)));
        //        if (preIsUp)
        //        {
        //            gridIndex = floorIndex;
        //        }
        //        else
        //        {
        //            gridIndex = cellingIndex;
        //        }
        //    }
        //    else
        //    {

        //    }
        //    return gridIndex;
        //}
        /// <summary>
        /// 获取当前实际的格值 按照涨算向下取整 跌则向上取整
        /// </summary>
        /// <param name="operateStockInfo"></param>
        /// <param name="curDot"></param>
        /// <returns></returns>
        public static int getGridIndexCurrent(OperateStockInfo operateStockInfo, double curPrice, bool preIsUp)
        {
            if (preIsUp)
            {
                return (int)Math.Floor((curPrice / operateStockInfo.latticeValue));
            }
            else
            {
                return ((int)Math.Ceiling((curPrice) / (operateStockInfo.latticeValue)));
            }
        }
        /// <summary>
        /// 根据当前点和前面点的状态计算当前点的格值
        /// 1、在上涨状态确定时向下取整(如价格=44.01~44.99,起示位置0,一格子单位长1,则格值为44)
        /// 2、在下跌状态确定时向上取整(上面的例子格值为45)
        /// </summary>
        /// <param name="operateStockInfo"></param>
        /// <param name="curDot"></param>
        /// <param name="preIsUp">前面点的涨跌状态 ,这决定了当前点的取整方式</param>
        /// <returns></returns>
        public static int getGridIndexNew(OperateStockInfo operateStockInfo, CycleData curData, CycleData preData, bool preIsUp)
        {
            int gridIndex = 0;

            if (operateStockInfo.isClose)
            {
                //向上取整
                int cellingIndex = ((int)Math.Ceiling((curData.close) / (operateStockInfo.latticeValue)));
                //向下取整
                int floorIndex = ((int)Math.Floor((curData.close) / (operateStockInfo.latticeValue)));
                if (preIsUp)
                {
                    gridIndex = floorIndex;
                }
                else
                {
                    gridIndex = cellingIndex;
                }
            }
            else
            {

            }
            return gridIndex;
        }

        /// <summary>
        /// 字体单位pt转像素单位
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static int ptTopx(double pt)
        {
            //pt = 1/72(英寸), px = 1/dpi(英寸)

            int dpiX;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = (int)graphics.DpiX;
            }
            int px = (int)(pt / 72 * dpiX);
            return px;
        }
        public static void UpdateStockList()
        {
            // http://www.sse.com.cn/assortment/stock/list/share/
            stockHelp.UpdateStockList();
        }
        /// <summary>
        /// 日线数据下载 网易 不复权数据
        /// </summary>
        /// <param name="code"></param>
        public static void UpdataStockDataDay(string code)
        {
            //http://quotes.money.163.com/service/chddata.html?code=0600708&start=20200606&end=20200629&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;VOTURNOVER;VATURNOVER;


            string type = GetStockType(code);
            string filename = System.AppDomain.CurrentDomain.BaseDirectory + "\\dataDay\\" + code + ".csv";
            if (code == "999999")
            {
                code = "000001";
                type = "sh";
            }
            DateTime beginDatetime = new DateTime(1991, 1, 1);
            DateTime endDateTime = DateTime.Now.Date;
            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename, Encoding.Default))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] date = line.Split(',')[0].Split('-');
                        beginDatetime = new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2])).AddDays(1);
                        break;
                    }
                }
            }
            string url = string.Format(@"http://quotes.money.163.com/service/chddata.html?code={0}&start={1}&end={2}&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;VOTURNOVER;VATURNOVER;"
                        , (type == "sh" ? 0 : 1) + code, beginDatetime.ToString("yyyyMMdd"), endDateTime.ToString("yyyyMMdd"));
            //string url = @"http://quotes.money.163.com/service/chddata.html?code=0600708&start=20200506&end=20200601&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;VOTURNOVER;VATURNOVER;";


            TimeSpan ts = endDateTime - beginDatetime;
            if (ts.Days <= 1 && endDateTime.Hour < 15)
            {
                return;
            }
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            WebResponse wResponse = request.GetResponse();
            Stream stream = wResponse.GetResponseStream();
            StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default);
            string resStr = reader.ReadToEnd();
            if (string.IsNullOrEmpty(resStr))
            {
                return;
            }
            resStr = resStr.Replace("'", "");   //url返回的值
            resStr = resStr.Replace("日期,股票代码,名称,收盘价,最高价,最低价,开盘价,前收盘,成交量,成交金额,\r\n", "");
            resStr = resStr.Replace("None", "");

            //FileStream 貌似不能实现多行插入第一行（只能追加到末尾）
            //所以更新模式是：先取出文件的内容，然后把新获取的数据从第一行开始覆盖，接下来把前面取出的内容接着覆盖
            //因为从网上获取的数据是最新的在第一行，所以暂时只能这样处理
            string curStr = "";
            if (File.Exists(filename))
            {
                StreamReader streamReader = new StreamReader(filename, Encoding.Default);
                curStr = streamReader.ReadToEnd();
                streamReader.Close();
            }
            using (FileStream fsWrite = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = Encoding.Default.GetBytes(resStr);
                fsWrite.Seek(0, SeekOrigin.Begin);
                fsWrite.Write(buffer, 0, buffer.Length);
                if(!string.IsNullOrEmpty(curStr))
                {
                    byte[] bufferCur = Encoding.Default.GetBytes(curStr);
                    fsWrite.Write(bufferCur, 0, bufferCur.Length);
                }
            }
            reader.Close();
            wResponse.Close();

        }
        /// <summary>
        /// 5分钟数据下载 新浪 前复权数据
        /// </summary>
        /// <param name="code"></param>
        public static void UpdataStockData5Min(string code)
        {
            //http://blog.sina.com.cn/s/blog_afae4ee50102wu8a.html
            //方法3：http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=[市场][股票代码]&scale=[周期]&ma=no&datalen=[长度]
            //返回结果：获取5、10、30、60分钟JSON数据；day日期、open开盘价、high最高价、low最低价、close收盘价、volume成交量；向前复权的数据。
            //  注意，最多只能获取最近的1023个数据节点。
            //http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=sz000002&scale=5&ma=no&datalen=1023


            string type = GetStockType(code);
            string filename = System.AppDomain.CurrentDomain.BaseDirectory + "\\data5Min\\" + type + code + ".json";
            if (code == "999999")
            {
                code = "000001";
                type = "sh";
            }
            DateTime beginDatetime = new DateTime(1991, 1, 1);
            DateTime endDateTime = DateTime.Now.Date;
            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename, Encoding.Default))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //string[] date = line.Replace("[{\"day\":\"", "").Replace(" ", "").Split('-');
                        //beginDatetime = new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]));
                        break;
                    }
                }
            }

            string symbol = type + code;
            TimeSpan ts = endDateTime - beginDatetime;
            int datalen = 0;
            if (ts.Minutes < 5)
            {
                return;
            }
            else
            {
                datalen = ts.Minutes / 5;
            }
            string url = string.Format(@"http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol={0}&scale=5&ma=no&datalen={1}", symbol, datalen);

            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            WebResponse wResponse = request.GetResponse();
            Stream stream = wResponse.GetResponseStream();
            StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default);
            string resStr = reader.ReadToEnd();
            //resStr = resStr.Replace("'", "");   //url返回的值
            //resStr = resStr.Replace("日期,股票代码,名称,收盘价,最高价,最低价,开盘价,前收盘,成交量,成交金额,\r\n", "");
            //resStr = resStr.Replace("None", "");
            using (FileStream fsWrite = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = Encoding.Default.GetBytes(resStr);
                fsWrite.Position = fsWrite.Length;
                fsWrite.Write(buffer, 0, buffer.Length);
            }
            reader.Close();
            wResponse.Close();
        }
    }
}
