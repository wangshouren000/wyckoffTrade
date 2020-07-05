using System.Collections.Generic;
using System.Drawing;

namespace DotGraphic
{
    /// <summary>
    /// 实体类
    /// </summary>
    public static class Modes
    {
        /// <summary>
        /// 单周期数据
        /// </summary>
        public class CycleData
        {
            public string date;
            public double open;
            public double high;
            public double low;
            public double close;
            public long amount;
            public long vol;
            public double lastClose;
        }
        /// <summary>
        /// 点的基本信息
        /// </summary>
        public class DotValue
        {
            /// <summary>
            /// 当前状态
            /// </summary>
            public bool isUp;
            /// <summary>
            /// 当前点包含的价格集合
            /// </summary>
            public List<CycleData> data;
            /// <summary>
            /// 位置 position.Y 就是其格值 position.X就是其列
            /// </summary>
            public Point position;
            /// <summary>
            /// 每一个转折的价格变化区间 当前列的顶底格值做减法
            /// </summary>
            public int space;
            /// <summary>
            /// 是否是填充点
            /// </summary>
            public bool isFill;

            //是否是转折点
            public bool isTurn;
        }
        /// <summary>
        /// 股票基本信息
        /// </summary>
        public class OperateStockInfo
        {
            public string code;
            public string name;
            public string type;
            /// <summary>
            /// 单格格子间隔值
            /// </summary>
            public double latticeValue;
            /// <summary>
            /// 按照什么价格计算
            /// true 收盘价 false 高低价
            /// </summary>
            public bool isClose = true;
            /// <summary>
            /// 坐标的最低点(通常比当前范围内的最低价略小,且价格数目较“整”)
            /// </summary>
            public double startValue;
            public double endValue;
            /// <summary>
            /// 单格反转是否重建(不换列)
            /// </summary>
            public bool isOneDotRebuild = true;
            /// <summary>
            /// 转折点数值 一般默认 1 常用 3，5
            /// </summary>
            public int dotInterval = 1;
            /// <summary>
            /// 复权状态 0 不复权 1 前复权 2 后复权
            /// </summary>
            public int RehabilitationStatus = 1;
            /// <summary>
            /// 当前周期
            /// </summary>
            public CycleType cycleType = CycleType.日;
            /// <summary>
            /// 数据来源： 163 ，通达信
            /// </summary>
            public string dataSource = "163";
        }
        /// <summary>
        /// 周期类型
        /// </summary>
        public enum CycleType
        {
            /// <summary>
            /// 1f
            /// </summary>
            一分钟,
            /// <summary>
            /// 5f
            /// </summary>
            五分钟,
            /// <summary>
            /// 30f
            /// </summary>
            三十分钟,
            /// <summary>
            /// day
            /// </summary>
            日,
            /// <summary>
            /// week
            /// </summary>
            周,
            /// <summary>
            /// month
            /// </summary>
            月,
            /// <summary>
            /// year
            /// </summary>
            年
        };

        /// <summary>
        /// 转折点
        /// </summary>
        public class DotTurn
        {
            /// <summary>
            /// 多点点数图的转折在单点点数图列表里的位置
            /// </summary>
            public int index { set; get; }
            /// <summary>
            /// 这个转折前的涨跌状态
            /// </summary>
            public bool isUp;
        }
    }
}