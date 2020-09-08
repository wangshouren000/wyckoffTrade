using System.Collections.Generic;
using System.Data;

namespace DotGraphic
{
    public class StockHelp
    {
        /// <summary>
        /// 股票数据接口网站
        /// </summary>
        Dictionary<string, string> urlList = new System.Collections.Generic.Dictionary<string, string>();
        public DataTable UpdateStockList()
        {
            DataTable dataTable = new DataTable();
            return dataTable;
        }
    }
}
