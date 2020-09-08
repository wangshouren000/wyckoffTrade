/*数据结构：
    日期,股票代码,名称,收盘价,最高价,最低价,开盘价,前收盘,成交量,成交金额
  在线数据来源1:
        新浪："http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=sh000001&scale=5&ma=no&datalen=1023"
        (day,open,high,low,close,volume)
    可以获取5、10、30、60、240分钟数据，最多可获取1023个周期
  离线数据来源：
        网易：http://quotes.money.163.com/service/chddata.html?code=0000001&start=20000101&end=20200101&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP
        获取日线数据,数据可能会滞后一天
在线数据来源2:
    东方财富:
    http://push2his.eastmoney.com/api/qt/stock/kline/get?cb=jQuery&secid=1.603917&fields1=f1%2Cf2%2Cf3%2Cf4%2Cf5&fields2=f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf58&klt=101&fqt=0&beg=20200710&end=20200716

    secid=1.603917
          sh 1 sz 0 +code
    fields1=
    "code","market","name","decimal","dktotal","klines"

    fields2= date,open,close,high,low,,volume,amount
    klt=
        5 5f
        15 15f
        101 day
    fqt=
        0 不复权 1 前复权 2 后复权
*/
//公有变量
{
    //在线获取股票列表
    var isOnlineStockList = true;
    var stockArray = {};
    var name;
    var code;
    var type;
    var isOnline = false;
    var keypairFileList = {};
    var canvasD;
    var contextD;
    var canvasK;
    var contextK;
    var table;
    var tableK;
    var isClose;
    var dotInterval;
    var beginDate;
    var endDate;
    var curList;
    var isKline;
    var lineWidth = 1;
    var cycle;
    // 设置一格间隔 像素
    var minSpace = 20; //最小
    var maxSpace = 25;
    var space = minSpace;
    var spaceD = space;
    var spaceK = space;
    //单点反转换行
    var isOneDotRebuild = false;
    //复权 0 不 1 前 2 后
    var rehabilitation = 0;
    var dataSource = "东方财富";
    //按照比例计算格值吗
    var isPercentLattice = false;
    //每格子占当前价格比例
    var percentLatticeValue = 2;

    //存放点数图的信息
    var dotValueList;
    var scale = 1;
    var stockInfo;
    var offsetKY;
    var offsetDY;

}
//点数图点的位置表
function SetTable(x, y)
{
    table = new Array(x + 1); //表格有x行
    for (var i = 0; i < table.length; i++)
    {
        table[i] = new Array(y + 1); //每行有y列
        for (var j = 0; j < table[i].length; j++)
        {
            table[i][j] = 0;
        }
    }
    return table;
}

//画点数图
function DrawPointAndFigure(returnValue)
{
    var offsetX = 3;
    var offsetY = 2;
    offsetDY = offsetY;
    //索引后面偏移量

    var offsetXL = 5;
    var offsetYL = 2;
    var dateIndex = 3;
    //成交量累计
    var volSumIndex = 3;
    //初始化
    {
        //清理
        table = [];

        dotValueList = returnValue.dotValueList;
        var latticeValue = returnValue.stockInfo.latticeValue;
        // 定义当前坐标

        //minSpace = 20; //最小
        //maxSpace = 25;
        space = minSpace * scale;
        var yIndex = returnValue.stockInfo.endIndex - returnValue.stockInfo.startIndex + offsetY;
        console.log(returnValue.stockInfo.endIndex +"|"+ returnValue.stockInfo.startIndex+"|"+stockInfo.latticeValue)
        var xIndex = dotValueList[0].position.x + offsetX;

        canvasD.width = space * (xIndex + offsetXL);
        canvasD.height = space * (yIndex + offsetYL + dateIndex + volSumIndex);
        table = SetTable(xIndex, yIndex);

        if (canvasD.height > window.innerHeight * 0.92)
        {
            space = parseInt(window.innerHeight * 0.92 / (yIndex + offsetYL + dateIndex + volSumIndex)) * scale;
            if (space < (minSpace / 2))
            {
                space = minSpace / 2;
            }
            canvasD.height = space * (yIndex + offsetYL + dateIndex + volSumIndex);
            canvasD.width = space * (xIndex + offsetXL);
        }

        spaceD = space;
        var offsetXSpace = offsetX * space;
        var offsetYSpace = offsetY * space;

        //抗锯齿 抗模糊
        var width = canvasD.width,
            height = canvasD.height;

        if (window.devicePixelRatio)
        {
            canvasD.style.width = width + "px";
            canvasD.style.height = height + "px";
            canvasD.height = height * devicePixelRatio * 2;
            canvasD.width = width * devicePixelRatio * 2;
            contextD.scale(devicePixelRatio * 2, devicePixelRatio * 2);
        }
        //0点
        contextD.beginPath();
        contextD.arc(offsetXSpace, offsetYSpace, 2, 0, 2 * Math.PI);
        contextD.fillStyle = "black";
        contextD.fill();
        contextD.stroke();

        //字体
        var font8 = 'bold ' + 8 * scale + 'px 宋体';
        var font10 = 'bold ' + 10 * scale + 'px 宋体';
        var font12 = 'bold ' + 12 * scale + 'px 宋体';

    }

    //绘制水平方向的网格线
    //线条比格子多1,且加入上面一格刻度，下面两格刻度
    for (var y = 0; y <=(yIndex - offsetY); y++)
    {
        //开启路径
        contextD.beginPath()
        contextD.lineWidth = 0.2
        //水平线要多画一格(最高点是向下取整的,所以多一格,且最上方要封顶)
        contextD.moveTo(offsetXSpace, space * y + offsetYSpace)
        contextD.lineTo(space * xIndex + space, space * y + offsetYSpace)
        contextD.stroke();

        contextD.save();
        contextD.fillStyle = 'OrangeRed'
        contextD.font = font8
        contextD.textBaseline = 'right';
        //设置文本的垂直对齐方式
        contextD.textAlign = 'right';

        //标垂直刻度
        //注意：网格是从前偏移量的位置开始画的,所以最后列也是同等位移
        //offsetXSpace offsetYSpace
        //刻度偏移 便于看
        //var maxPriceNew = ((parseInt(returnValue.stockInfo.maxPrice / 0.05) + 1) * 0.05).toFixed(2);
        var str = (returnValue.stockInfo.maxPrice - y * latticeValue).toFixed(2);
        contextD.fillText(str, offsetXSpace - space / 2, y * space + offsetYSpace);

        contextD.fillText(str, space * xIndex + space * 3, y * space + offsetYSpace);
    }
    //加两行 放日期
    // contextD.beginPath()
    // contextD.moveTo(offsetXSpace, space * yIndex + space)
    // contextD.lineTo(space * xIndex + space, space * yIndex + space)
    //contextD.stroke();

    contextD.beginPath()
    contextD.moveTo(offsetXSpace, space * yIndex + space * dateIndex)
    contextD.lineTo(space * xIndex + space, space * yIndex + space * dateIndex)
    contextD.stroke();

    //绘制垂直方向的网格线
    //增加上下三格刻度
    for (var x = 0; x <= xIndex - offsetX + 1; x++)
    {
        //开启路径
        contextD.beginPath()
        contextD.lineWidth = 0.2
        contextD.moveTo(offsetXSpace + x * space, offsetYSpace)
        contextD.lineTo(offsetXSpace + x * space, space * yIndex + space * dateIndex + space * volSumIndex)
        contextD.stroke()

        //标顶部水平刻度
        contextD.save();
        contextD.fillStyle = 'green'
        contextD.font = font8
        contextD.textBaseline = 'bottom';
        //设置文本的垂直对齐方式
        contextD.textAlign = 'center';
        var str = x;
        if ((x % 5) == 0 || x == (xIndex - offsetX + 1))
        {
            contextD.fillText(str, offsetXSpace + space * x - space / 2, offsetYSpace - space / 2);
            contextD.restore();
        }
    }


    //填充OX
    contextD.save();
    contextD.font = font10
    contextD.textBaseline = 'middle';
    contextD.textAlign = 'center';

    //记录点数图的成交量和
    var volList = [];
    //列数
    var col = dotValueList[0].position.x;
    var curVolSum = 0;
    var maxVolSum = 0;
    var minVolSum = 1000000000000000;
    var k = 0;
    var lastDate = dotValueList[0].datas[0].date;
    for (var i = 0; i < dotValueList.length; i++)
    {
        var dotValue = dotValueList[i];
        var preDotValue = dotValueList[i];
        if (i > 0)
        {
            preDotValue = dotValueList[i - 1];
        }

        var curCol = dotValue.position.x;
        var x = (dotValue.position.x + offsetX);
        var y = yIndex - (dotValue.position.y - returnValue.stockInfo.startIndex) - 1;
        var px = x * space + space / 2;
        var py = y * space + space / 2;

        //记录某位置的值 备用
        table[x][y] = dotValue;

        if (dotValue.isUp)
        {
            //画X
            contextD.font = font12
            if (dotValue.isFill)
            {
                contextD.fillStyle = 'lightgray';
                contextD.fillRect(px - space / 2, py - space / 2, space, space);
            }
            contextD.fillStyle = 'OrangeRed';
            contextD.fillText("×", px, py);
        }
        else
        {
            //画O
            contextD.font = font12
            if (dotValue.isFill)
            {
                contextD.fillStyle = 'lightgray';
                contextD.fillRect(px - space / 2, py - space / 2, space, space);
            }
            contextD.fillStyle = 'SlateBlue';
            contextD.fillText("○", px, py);
        }

        if (col != curCol)
        {
            //换列
            col = curCol;
            //累计成交量
            var item = {
                volSum: curVolSum,
                date: preDotValue.datas[0].date
            };
            volList.push(item);
            if (maxVolSum < curVolSum)
            {
                maxVolSum = curVolSum;
            }
            if (minVolSum > curVolSum)
            {
                minVolSum = curVolSum;
            }
            curVolSum = 0;
            contextD.save();
            //标记日期
            var date = dotValue.datas[0].date;

            if (cycle.indexOf("分钟") == -1)
            {
                date = date.substring(0, 6)
            }
            else
            {
                date = date.substring(4, 8) + " " + date.substring(8, 10) + ":" + date.substring(10, 12)
            }

            contextD.font = font8
            //contextD.fillText(month, space * x + space / 2, space * yIndex + space * 3 / 2);
            contextD.fillStyle = 'green';

            if (date.substring(0, 4) != lastDate.substring(0, 4) && cycle.indexOf("分钟") == -1)
            {
                contextD.font = font10
                contextD.fillStyle = 'red';
                lastDate = date
                date = date.substring(0, 4)
            }

            contextD.translate(space * x + space / 2, space * yIndex + space * 1.5);
            contextD.rotate(90 * Math.PI / 180);
            //contextD.clearRect(0, 0, spaceD*0.9, spaceD*3*0.9);
            contextD.fillText(date, 0, 0);
            contextD.restore();

        }

        if (!dotValue.isFill)
        {
            for (var j = 0; j < dotValue.datas.length; j++)
            {
                curVolSum = parseFloat(curVolSum) + parseFloat(dotValue.datas[j].volume);
            }
        }
    }
    //console.log(volList);
    //console.log(maxVolSum);
    //console.log(minVolSum);
    //绘制累计成交量图
    var startIsUp = dotValueList[0].isUp;
    var volSumStartPy = space * (yIndex + dateIndex);
    for (var i = 0; i < volList.length; i++)
    {
        contextD.save();
        contextD.lineWidth = 0.2;
        var startpx = offsetXSpace + (volList.length - i + 0.1) * space;
        //var percent = (volList[i].volSum - minVolSum) / (maxVolSum - minVolSum);
        var percent = (volList[i].volSum) / (maxVolSum);

        //console.log(percent);
        var startpy = volSumStartPy + space * volSumIndex * (1 - percent);

        var endpy = volSumStartPy + space * volSumIndex;
        if (startIsUp)
        {
            contextD.fillStyle = 'OrangeRed';
        }
        else
        {
            contextD.fillStyle = 'SlateBlue';
        }
        startIsUp = !startIsUp;
        var pyheight = endpy - startpy;
        var font6 = 'bold ' + 6 * scale + 'px 宋体';
        contextD.font = font6
        contextD.fillRect(startpx, startpy, space * 0.8, pyheight);
        //标记累计成交量
        var num = 1000000;
        var numUnit = "万手"
        if ((parseFloat(volList[i].volSum) / num) > 1000)
        {
            num = 10000000000;
            numUnit = "亿手"
        }
        if (startIsUp)
        {
            contextD.fillText((parseFloat(volList[i].volSum) / num).toFixed(2), startpx + space * 0.4, volSumStartPy + space * volSumIndex + space / 2);
        }
        else
        {
            contextD.fillText((parseFloat(volList[i].volSum) / num).toFixed(2), startpx + space * 0.4, startpy-space/2);

        }
    }
    contextD.beginPath()
    contextD.moveTo(offsetXSpace, space * yIndex + space * dateIndex + space * volSumIndex)
    contextD.lineTo(space * xIndex + space, space * yIndex + space * dateIndex + space * volSumIndex)
    contextD.stroke();
    contextD.font = font10
    contextD.fillText("累计成交量", space * xIndex + space * 3, space * yIndex + space * dateIndex + space * volSumIndex / 2);
    contextD.fillText((parseFloat(minVolSum) / num).toFixed(2) + numUnit, space * xIndex + space * 3, space * yIndex + space * dateIndex + space * volSumIndex);
    contextD.fillText((parseFloat(maxVolSum) / num).toFixed(2) + numUnit, space * xIndex + space * 3, space * yIndex + space * dateIndex);

    contextD.restore();
}
//画K线 成交量 和 维斯波
function DrawKLine(curList, stockInfo)
{
    //索引偏移量
    var offsetX = 6;
    var offsetY = 1;
    offsetKY = offsetY;
    var offsetXL = 5;
    var offsetYL = 3;
    //日期占格
    var dateIndex = 2;
    //成交量占据格数
    var volumeIndex = 3;
    //维斯波占格
    var waveIndex = 3;
    //点数图为标准的维斯波占格
    var waveDotIndex = 3;
    //供需指数占格
    var sdIndex = 3;
    var sumIndex = offsetYL + dateIndex + volumeIndex + waveIndex + waveDotIndex + sdIndex;
    space = minSpace * scale;

    //字体
    var fontmin = 'bold ' + 8 * scale + 'px 宋体';
    var font10 = 'bold ' + 10 * scale + 'px 宋体';
    var font12 = 'bold ' + 12 * scale + 'px 宋体';

    var yIndex = stockInfo.endIndex - stockInfo.startIndex + offsetY;
    var xIndex = curList.length + offsetX;
    //var spacex = canvasK.width / xIndex;
    //var spacey = canvasK.height / yIndex;
    // if (spacex > minSpace * scale && spacey > minSpace * scale)
    // {
    //     space = parseInt(spacey > spacex ? spacex : spacey) * scale;
    //     if (space > maxSpace * scale)
    //     {
    //         space = maxSpace * scale;
    //     }
    // }
    var spaceX = space; // / 2;
    canvasK.width = spaceX * (xIndex + offsetXL);
    canvasK.height = space * (yIndex + sumIndex);
    if (canvasK.height > window.innerHeight * 0.92)
    {
        space = parseInt(window.innerHeight * 0.92 / (yIndex + sumIndex)) * scale;
        canvasK.height = space * (yIndex + sumIndex);
        spaceX = space;
        canvasK.width = spaceX * (xIndex + offsetXL);
    }
    spaceK = space;

    var offsetXSpace = offsetX * spaceX;
    var offsetYSpace = offsetY * space;


    //抗锯齿 抗模糊
    var width = canvasK.width,
        height = canvasK.height;

    if (window.devicePixelRatio)
    {
        canvasK.style.width = width + "px";
        canvasK.style.height = height + "px";
        canvasK.height = height * devicePixelRatio * 2;
        canvasK.width = width * devicePixelRatio * 2;
        if (canvasK.width > 32767)
        {
            canvasK.width = width * devicePixelRatio;
            canvasK.height = height * devicePixelRatio;
            contextK.scale(devicePixelRatio, devicePixelRatio);
        }
        else
        {
            contextK.scale(devicePixelRatio * 2, devicePixelRatio * 2);
        }
    }

    //0点
    contextK.beginPath();
    contextK.arc(offsetXSpace, offsetYSpace, 2, 0, 2 * Math.PI);
    contextK.fillStyle = "black";
    contextK.fill();
    contextK.stroke();
    //绘制水平方向的网格线 点数图最低点向上取整
    for (var y = 0; y <= yIndex - offsetY; y++)
    {
        if (space > 8)
        {
            //开启路径
            contextK.lineWidth = 0.2
            contextK.beginPath()
            contextK.moveTo(offsetXSpace, space * y + offsetYSpace)
            contextK.lineTo(spaceX * xIndex, space * y + offsetYSpace)
            contextK.stroke()
        }
        //标刻度
        contextK.save();
        contextK.fillStyle = 'OrangeRed'
        contextK.font = (space > 8 ? font10 : fontmin);
        contextK.textBaseline = 'right';
        //设置文本的垂直对齐方式
        contextK.textAlign = 'right';
        //var maxPriceNew = ((parseInt(stockInfo.maxPrice / 0.05) + 1) * 0.05).toFixed(2);
        //var str = (Math.round(stockInfo.minPrice, 2) + (yIndex - y - offsetY) * latticeValue).toFixed(2);
        var str = (stockInfo.maxPrice - y * latticeValue).toFixed(2);
        contextK.fillText(str, offsetXSpace - spaceX / 2, y * space + offsetYSpace + space / 4);
        contextK.textAlign = 'left';

        //最后列刻度后移一格 好看点
        contextK.fillText(str, spaceX * xIndex + spaceX, y * space + offsetYSpace + space / 4);
        contextK.restore();
    }
    {
        // contextK.beginPath()
        // contextK.moveTo(offsetXSpace, space * yIndex + space * (dateIndex - 0.7))
        // contextK.lineTo(spaceX * xIndex, space * yIndex + space * (dateIndex - 0.7))
        // contextK.stroke()

        //成交量行
        contextK.beginPath()
        contextK.lineWidth = 1
        contextK.moveTo(offsetXSpace, space * yIndex + space * dateIndex + volumeIndex * space)
        contextK.lineTo(spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space)
        contextK.stroke()

        //成交量行标刻度
        // contextK.save();
        // contextK.fillStyle = 'red'
        // contextK.font = 'bold 10px 微软雅黑'
        // contextK.textBaseline = 'right';
        // //设置文本的垂直对齐方式
        // contextK.textAlign = 'right';
        // contextK.textAlign = 'left';
        // contextK.fillText(stockInfo.maxVolume, spaceX * xIndex, space * yIndex + space * (dateIndex - 0.7));
        // contextK.fillText(stockInfo.minVolume, spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space);
        contextK.save();
        contextK.fillStyle = 'green'
        contextK.font = font12
        contextK.textBaseline = 'right';
        //设置文本的垂直对齐方式
        contextK.textAlign = 'left';
        contextK.fillText(" 成交量", spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space / 2);
        contextK.fillText(" 常规维斯波", spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space / 2);
        contextK.fillText(" 点数维斯波", spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space / 2);
        contextK.fillText(" 供需指数", spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + sdIndex * space / 2);
        // code name 
        // var str1 = stockInfo.code + "           " + stockInfo.name;
        // var strWhith = contextD.measureText(str1).width;

        // contextK.fillText(str1, spaceX * xIndex - strWhith * 3, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + space * 1.5);
        // contextK.fillStyle = 'SlateBlue'
        // contextK.fillText("周期:" + cycle + "           缩放倍数: " + scale, spaceX * xIndex - strWhith * 3, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + space * 2.5);
        // contextK.fillStyle = 'green'
        // var strWhith = contextD.measureText(str1).width;

        // var str2 = "最低价:" + stockInfo.minPrice + "    " + "单格值:" + latticeValue;
        // contextK.fillText(str2, spaceX * xIndex - strWhith * 3, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + space * 3.5);
        // var rate = (latticeValue * 100 / stockInfo.maxPrice).toFixed(2) + "%~" + parseFloat(latticeValue * 100 / stockInfo.minPrice).toFixed(2) + "%";
        // var str3 = "最高价:" + stockInfo.maxPrice + "    格幅:" + rate;
        // contextK.fillText(str3, spaceX * xIndex - strWhith * 3, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + space * 4.5);

        // var memo1 = "说明:1、低量回测介入后，需要连续的渐次三高(低点、高点、量)确认";
        // var strWhith1 = contextD.measureText(memo1).width;
        // contextK.fillText(memo1, spaceX * xIndex - strWhith1 * 2, space * yIndex + (sumIndex - offsetYL + 1) * space);
        // var memo2 = "2、低量回测前面需要是长期盘整后的高量、高幅上涨形成的强势突破";
        // var strWhith2 = contextD.measureText(memo2).width;
        // contextK.fillText(memo2, spaceX * xIndex - strWhith2 * 2, space * yIndex + (sumIndex - offsetYL + 2) * space);
        //维斯波行
        contextK.beginPath()
        contextK.lineWidth = 1
        contextK.moveTo(offsetXSpace, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space)
        contextK.lineTo(spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space)
        contextK.stroke()

        contextK.beginPath()
        contextK.lineWidth = 1
        contextK.moveTo(offsetXSpace, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + sdIndex * space)
        contextK.lineTo(spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + sdIndex * space)
        contextK.stroke()

        contextK.beginPath()
        contextK.lineWidth = 1
        contextK.moveTo(offsetXSpace, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space)
        contextK.lineTo(spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space)
        contextK.stroke()

        contextK.restore();

    }
    //绘制垂直方向的网格线
    for (var x = 0; x <= xIndex - offsetX; x++)
    {
        if (x == 0 || x == xIndex - offsetX)
        {
            contextK.save();
            contextK.strokeStyle = 'black';
            //开启路径
            contextK.beginPath()
            contextK.lineWidth = 0.2
            contextK.moveTo(offsetXSpace + x * spaceX, offsetYSpace)
            contextK.lineTo(offsetXSpace + x * spaceX, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + sdIndex * space)
            contextK.stroke()
        }
        // else
        // {
        //     contextK.save();
        //     contextK.strokeStyle = 'white';
        //     //开启路径
        //     contextK.beginPath()
        //     contextK.moveTo(offsetXSpace + x * spaceX, offsetYSpace)
        //     contextK.lineTo(offsetXSpace + x * spaceX, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space)
        //     contextK.stroke()
        // }
    }
    //画K线
    contextK.font = font10
    contextK.textBaseline = 'middle';
    //设置文本的垂直对齐方式
    contextK.textAlign = 'center';
    tableK = new Array(curList.length);
    for (var i = 0; i < curList.length; i++)
    {
        contextK.save();
        contextK.lineWidth = 1;

        var item = curList[i];
        //var preItem = curList[i];
        //var prepreItem = curList[i];
        var rateP1 = 0;
        var rateP2 = 0;
        var rateV = 0;

        var px = (i + 0.5) * spaceX + offsetXSpace;
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var maxIndex = parseFloat(stockInfo.maxPrice) / parseFloat(latticeValue) + offsetY;
        var startpy = ((maxIndex - parseFloat(item.open) / parseFloat(latticeValue)) * space);
        var endpy = ((maxIndex - parseFloat(item.close) / parseFloat(latticeValue)) * space);

        var startpyHL = ((maxIndex - parseFloat(item.low) / parseFloat(latticeValue)) * space);
        var endpyHL = ((maxIndex - parseFloat(item.high) / parseFloat(latticeValue)) * space);
        if (item.close >= item.open)
        {
            contextK.fillStyle = 'OrangeRed';
            contextK.strokeStyle = 'OrangeRed';
        }
        else
        {
            contextK.fillStyle = 'SlateBlue';
            contextK.strokeStyle = 'SlateBlue';
        }
        var pyheight = endpy - startpy;
        if (Math.abs(pyheight) < 1)
        {
            if (startpy > endpy)
            {
                startpy = startpy + 1;
            }
            else
            {
                endpy = endpy + 1;
            }
            pyheight = 1;
        }
        if (isKline)
        {
            contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);

            // var lowhighRate = parseFloat((parseFloat(item.high) - parseFloat(item.low)) / item.low);
            // if ((lowhighRate > 0.08 && cycle.indexOf("分钟") == -1) || lowhighRate > 0.04 && cycle.indexOf("分钟") != -1)
            // {
            //     var lowhighMidPy = ((maxIndex - parseFloat(item.low + item.high) / (2 * parseFloat(latticeValue))) * space);
            //     contextK.fillStyle = 'green';
            //     contextK.fillRect(startpx, lowhighMidPy, spaceX * 0.8, 1);
            // }
        }
        else
        {
            if (startpy > endpy)
            {
                contextK.fillStyle = 'OrangeRed';
                contextK.strokeStyle = 'OrangeRed';
            }
            else
            {
                contextK.fillStyle = 'SlateBlue';
                contextK.strokeStyle = 'SlateBlue';
            }
            contextK.lineWidth = 2;
            contextK.fillRect(startpx, endpy, spaceX * 0.8, lineWidth);

        }
        contextK.beginPath()
        contextK.lineWidth = 1
        contextK.moveTo(px, startpyHL)
        contextK.lineTo(px, endpyHL)
        contextK.stroke()

        // if (i > 1)
        // {
        //     preItem = curList[i - 1];
        //     prepreItem = curList[i - 2];
        //     rateP1 = preItem.close / prepreItem.close - 1;
        //     rateP2 = item.close / preItem.close - 1;
        //     rateV = item.volume / preItem.volume;
        // }
        //标记可能的量价危险关系
        /*  
          var minRate = 0.01;
          if (cycle.indexOf("分钟") == -1)
          {
              minRate = 0.02
          }
          if (rateP1 > minRate && rateP2 / rateP1 > 1 && rateV < 1)
          {
              contextK.font = font10
              contextK.fillStyle = 'red'
              contextK.translate(startpx + space / 2, endpyHL - space * 1);
              contextK.rotate(90 * Math.PI / 180);
              contextK.fillText("危险上涨⇒", 0, 0);
              contextK.restore();
          }
          if (rateP2 < -minRate && rateP1 > 0 && Math.abs(rateP2 / rateP1) > 2 / 5 && rateV > 3 / 5)
          {
              contextK.font = font10
              contextK.fillStyle = 'blue'
              contextK.translate(startpx + space / 2, startpyHL + space * 1);
              contextK.rotate(90 * Math.PI / 180);
              contextK.fillText("⇐危险下跌", 0, 0);
              contextK.restore();
          }
          if (rateP1 > minRate && Math.abs(rateP2 / rateP1) < 2 / 3 && rateV > 1)
          {
              contextK.font = font10
              contextK.fillStyle = 'SlateBlue'
              contextK.translate(startpx + space / 2, startpyHL + space * 2);
              contextK.rotate(90 * Math.PI / 180);
              contextK.fillText("⇐放量滞涨", 0, 0);
              contextK.restore();
          }
          //滞跌必须是T型K线
          if (rateP1 < -minRate && Math.abs(rateP2 / rateP1) < 1 && rateV > 1 && (item.close > (item.high - item.low) * 1 / 2 + item.low))
          {
              contextK.font = font10
              contextK.fillStyle = 'OrangeRed'
              contextK.translate(startpx + space / 2, endpyHL - space * 2);
              contextK.rotate(90 * Math.PI / 180);
              contextK.fillText("放量滞跌⇒", 0, 0);
              contextK.restore();
          }
          //小量 小K线
          if (rateP1 > minRate && Math.abs(rateP2 / rateP1) < 2 / 5 && rateV < 1 / 2 && (item.high / item.low - 1) < minRate)
          {
              contextK.font = font12
              contextK.fillStyle = 'black'
              contextK.translate(startpx + space / 2, endpyHL + space * 4);
              contextK.rotate(90 * Math.PI / 180);
              contextK.fillText("介入点之低量回测", 0, 0);
              contextK.restore();
          }
          */
        if (i % 20 == 0)
        {
            //日期 旋转
            contextK.fillStyle = "green"
            contextK.font = font12;
            //translate 转换原点 这里一般填要转换点的位置
            //rotate 转换角度
            //fillText的位置不再是原来的位置，就是0,0

            var strDate = item.date
            //contextK.translate(startpx + spaceX / 2, yIndex * space + space * (dateIndex - 0.7) / 2);
            //contextK.rotate(90 * Math.PI / 180);
            contextK.fillText(strDate, startpx + spaceX / 2, yIndex * space + space / 2);
            contextK.restore();
        }
        var obj = {
            item: item,
            startpx: (i * spaceX + offsetXSpace),
            startpyHL: startpyHL + space / 2,
            endpyHL: endpyHL - space / 2,
            endpx: (i * spaceX + offsetXSpace + spaceX),
            space: space,
            spaceX: spaceX,
            offsetXSpace: offsetXSpace
        }
        tableK[i] = obj;
    }

    //买入线
    var obj = stockArray[code];
    if (obj.length == 5)
    {
        var buyPricePy = ((maxIndex - parseFloat(obj[4]) / parseFloat(latticeValue)) * space);
        contextK.save();
        contextK.strokeStyle = 'green'
        contextK.fillStyle = 'black'
        contextK.beginPath()
        contextK.lineWidth = 1
        contextK.moveTo(offsetXSpace, buyPricePy)
        contextK.lineTo(spaceX * xIndex, buyPricePy)
        contextK.stroke();
        contextK.fillText(obj[4], spaceX * xIndex + spaceX * 3, buyPricePy);
        contextK.restore();
    }
    //绘制成交量图
    var volumeStartpy = space * yIndex + space * dateIndex;
    for (var i = 0; i < curList.length; i++)
    {
        contextK.save();
        var item = curList[i];
        var preItem = curList[i];
        if (i > 0)
        {
            preItem = curList[i - 1];
        }
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var endpy = volumeStartpy + volumeIndex * space;
        //var percent = (item.volume - stockInfo.minVolume) / (stockInfo.maxVolume - stockInfo.minVolume);
        var percent = (item.volume) / (stockInfo.maxVolume);

        var startpy = endpy - percent * (volumeIndex + 2) * space;
        var pyheight = endpy - startpy;
        if (parseFloat(item.close) >= parseFloat(preItem.close))
        {
            contextK.fillStyle = 'OrangeRed';
        }
        else
        {
            contextK.fillStyle = 'SlateBlue';
        }
        if (isKline)
        {
            contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);

        }
        else
        {
            contextK.lineWidth = 2;

            if (parseFloat(item.close) >= parseFloat(item.open))
            {
                contextK.fillStyle = 'OrangeRed';
            }
            else
            {
                contextK.fillStyle = 'SlateBlue';
            }
            contextK.fillRect(startpx + 0.399 * spaceX, startpy, contextK.lineWidth, pyheight);
        }
        contextK.restore();
    }

    //绘制维斯波
    //获取维斯波数据
    var waveList = [];
    var waveObj = {
        volume: curList[0].volume,
        isUp: parseFloat(curList[0].close) > parseFloat(curList[0].open)
    };
    waveList.push(waveObj);
    var minWave = curList[0].volume;
    var maxWave = minWave;
    //涨跌幅
    var increaseRate = curList[1].close / curList[0].close - 1;

    for (var i = 1; i < curList.length; i++)
    {
        var wave = curList[i];
        var preWave = waveList[i - 1];
        increaseRate = wave.close / curList[i - 1].close - 1;
        var isUp = increaseRate > 0;
        //用于记录isUp的状态前后是否一致 主要用于小幅反向震动
        var isSame = true;
        var volume = parseInt(wave.volume);


        if (preWave.isUp == isUp)
        {

            volume = parseInt(preWave.volume) + parseInt(volume);

        }

        var obj = {
            volume: volume,
            isUp: isUp
        }
        if (parseInt(minWave) > volume)
        {
            minWave = volume;
        }
        if (parseInt(maxWave) < volume)
        {
            maxWave = volume;
        }
        waveList.push(obj);
    }
    var waveStartpy = space * yIndex + space * dateIndex + volumeIndex * space + space * 1 / 10;

    //绘制维斯波 根据收盘价之间的涨跌状态绘制维斯波
    for (var i = 1; i < waveList.length; i++)
    {
        contextK.save();
        var wave = waveList[i];
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var endpy = waveStartpy - space * 1 / 10 + waveIndex * space;
        //var percent = (wave.volume - minWave) / (maxWave - minWave);
        var percent = (wave.volume) / (maxWave);

        var startpy = endpy - percent * ((waveIndex - 1 / 10) * space);
        var pyheight = endpy - startpy;
        if (wave.isUp)
        {
            contextK.fillStyle = '#B40404';
        }
        else
        {
            contextK.fillStyle = '#0404B4';
        }
        if (isKline)
        {
            contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);

        }
        else
        {
            contextK.lineWidth = 2;

            contextK.fillRect(startpx + 0.399 * spaceX, startpy, contextK.lineWidth, pyheight);
        }
        contextK.restore();
    }

    //根据点数图的规则绘制维斯波
    //点数图数据处理
    //dotValueList
    dotValueList.reverse();
    var dotDateSortList = []; //点数图极点集合

    var isUp = dotValueList[0].isUp;
    var date = curList[0].date;
    // var item0 = {
    //     isUp: isUp,
    //     date: date,
    // }
    // dotDateSortList.push(item0);

    for (var i = 1; i < dotValueList.length; i++)
    {
        var obj = dotValueList[i];
        if (isUp != obj.isUp)
        {
            var item = {
                isUp: isUp,
                date: date,
            }
            dotDateSortList.push(item);

        }
        isUp = obj.isUp;
        date = obj.datas[0].date;
    }
    //最后一个
    var item = {
        isUp: isUp,
        date: date
    }
    dotDateSortList.push(item);

    //获取点数图维斯波数据

    var waveDotList = [];

    var minWaveDotVol = curList[0].volume;
    var maxWaveDotVol = minWaveDotVol;
    var dotIndex = 0;
    var dateSortObj = dotDateSortList[dotIndex];
    var dotIsUp = dateSortObj.isUp;
    var curVolume = 0;
    //var rate = 0.01;
    for (var i = 0; i < curList.length; i++)
    {
        var item = curList[i];
        var preItem = curList[i];

        if (i > 0)
        {
            preItem = curList[i - 1];
        }
        var date = dateSortObj.date;
        if (item.date == date)
        {
            //rate = rate + Math.abs(item.close / preItem.close - 1);
            curVolume = parseInt(curVolume) + parseInt(item.volume);
            var obj = {
                volume: curVolume,
                isUp: dotIsUp
            }
            waveDotList.push(obj);
            dotIndex = dotIndex + 1;
            if (dotDateSortList.length > dotIndex)
            {
                dateSortObj = dotDateSortList[dotIndex];
                dotIsUp = dateSortObj.isUp;
            }

            if (curVolume < minWaveDotVol)
            {
                minWaveDotVol = curVolume;
            }
            if (curVolume > maxWaveDotVol)
            {
                maxWaveDotVol = curVolume;
            }
            curVolume = 0;
        }
        else
        {
            //涨跌的第一个点
            //rate = Math.abs(item.close / preItem.close - 1);
            curVolume = parseInt(curVolume) + parseInt(item.volume);
            var obj = {
                volume: curVolume,
                isUp: dotIsUp
            }
            waveDotList.push(obj);
            if (curVolume < minWaveDotVol)
            {
                minWaveDotVol = curVolume;
            }
            if (curVolume > maxWaveDotVol)
            {
                maxWaveDotVol = curVolume;
            }
        }
    }
    var isHighLowConnect = document.getElementById('highLowConnect').checked;
    if (isHighLowConnect)
    {
        //绘制K线图高低点连线
        var maxIndex = parseFloat(stockInfo.maxPrice) / parseFloat(latticeValue) + offsetY;
        var lastDot = {
            px: 0.5 * spaceX + offsetXSpace,
            lowpx: ((maxIndex - parseFloat(curList[0].low) / parseFloat(latticeValue)) * space),
            highpx: ((maxIndex - parseFloat(curList[0].high) / parseFloat(latticeValue)) * space)
        };

        //连线高低点

        contextK.save();
        contextK.strokeStyle = 'green';
        contextK.lineWidth = 1;

        dotIndex = 0;
        dateSortObj = dotDateSortList[dotIndex];
        for (var i = 0; i < curList.length; i++)
        {
            var dot = curList[i];
            if (dot.date == dateSortObj.date)
            {
                var px = (i + 0.5) * spaceX + offsetXSpace;
                var lowpx = ((maxIndex - parseFloat(dot.low) / parseFloat(latticeValue)) * space);
                var highpx = ((maxIndex - parseFloat(dot.high) / parseFloat(latticeValue)) * space);
                if (dateSortObj.isUp)
                {
                    contextK.beginPath()
                    contextK.moveTo(lastDot.px, lastDot.lowpx)
                    contextK.lineTo(px, highpx)
                    contextK.stroke()
                }
                else
                {
                    contextK.beginPath()
                    contextK.moveTo(lastDot.px, lastDot.highpx)
                    contextK.lineTo(px, lowpx)
                    contextK.stroke()
                }

                lastDot = {
                    px,
                    lowpx,
                    highpx
                };

                dotIndex = dotIndex + 1;
                if (dotDateSortList.length > dotIndex)
                {
                    dateSortObj = dotDateSortList[dotIndex];
                }
            }
        }
        contextK.restore();
    }
    //绘制点数图标准维斯波
    var waveDotStartpy = space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + space * 1 / 10;
    for (var i = 1; i < waveDotList.length; i++)
    {
        contextK.save();
        var wave = waveDotList[i];
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var endpy = waveDotStartpy - space * 1 / 10 + waveDotIndex * space;
        //var percent = (wave.volume - minWaveDotVol) / (maxWaveDotVol - minWaveDotVol);
        var percent = (wave.volume) / (maxWaveDotVol);

        var startpy = endpy - percent * ((waveDotIndex - 1 / 10) * space);
        var pyheight = endpy - startpy;
        if (wave.isUp)
        {
            contextK.fillStyle = 'OrangeRed';
        }
        else
        {
            contextK.fillStyle = 'SlateBlue';
        }
        if (isKline)
        {
            contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);

        }
        else
        {
            contextK.lineWidth = 2;

            contextK.fillRect(startpx + 0.399 * spaceX, startpy, contextK.lineWidth, pyheight);
        }
        contextK.restore();
    }
    var sdStartpy = space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + waveDotIndex * space + space * 1 / 10;

    //绘制供需指数 供需指数=(高点-底点)/成交量
    var minValue = 10000000000;
    var maxValue = 0;
    for (var i = 0; i < curList.length; i++)
    {
        var item = curList[i];
        var value = (item.high - item.low) * 10000000000 / item.volume;
        if (value > maxValue)
        {
            maxValue = value;
        }
        if (value < minValue)
        {
            minValue = value;
        }
    }
    for (var i = 0; i < curList.length; i++)
    {
        var item = curList[i];
        var preItem = curList[i];
        if (i > 0)
        {
            preItem = curList[i - 1];
        }
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var value = (item.high - item.low) * 10000000000 / item.volume;
        var endpy = sdStartpy - space * 1 / 10 + sdIndex * space;
        //var percent = (value - minValue) / (maxValue - minValue);
        var percent = (value) / (maxValue);

        var startpy = endpy - percent * ((sdIndex - 1 / 10) * space);
        contextK.save();
        if (parseFloat(item.close) >= parseFloat(preItem.close))
        {
            contextK.fillStyle = 'OrangeRed';
        }
        else
        {
            contextK.fillStyle = 'SlateBlue';
        }
        var pyheight = endpy - startpy;
        contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);
        contextK.restore();

    }



    // 选择默认的tab页
    var titles = document.getElementById('tab-header').getElementsByTagName('li')
    for (var i = 0; i < titles.length; i++)
    {
        var li = titles[i];
        if (li.className.indexOf('selected') > -1)
        {
            var style;
            if (li.id == 0)
            {
                style = document.getElementById("kline").style;
            }
            else
            {
                style = document.getElementById("dotGraphic").style;
            }
            //tab靠后
            var ul = document.getElementById("tab-header").getElementsByTagName('ul')[0];
            var paddingLeft = style.width.replace('px', '');
            paddingLeft = (parseInt(paddingLeft) - 250) + "px"
            ul.style.paddingLeft = paddingLeft;
            scrollBottomAndRightTag(document.getElementById("right"));
        }
    }
    fillInfo();
}
//获取价格对应的单格值
function CalLatticeValue(basePrice)
{
    if (isPercentLattice)
    {
        if (basePrice < 100)
        {
            return parseFloat(basePrice * percentLatticeValue / 100).toFixed(1);
        }
        else if (basePrice > 100 && basePrice < 1000)
        {
            return parseFloat(basePrice * percentLatticeValue / 100).toFixed(0);
        }
        else
        {
            return parseFloat(basePrice * percentLatticeValue / 1000).toFixed(0) * 10;
        }
    }
    else
    {
        var percent = 3;
        if (cycle == "1分钟")
        {
            percent = 0.1;
        }
        if (cycle == "5分钟")
        {
            percent = 0.5;
        }
        if (cycle == "15分钟")
        {
            percent = 1;
        }
        if (cycle == "30分钟")
        {
            percent = 1.5;
        }
        if (cycle == "60分钟")
        {
            percent = 2;
        }
        if (cycle != "日" && cycle.indexOf("分钟") == -1)
        {
            percent = 7;
        }
        if (basePrice < 100)
        {
            var result = parseFloat(basePrice * percent / 100).toFixed(1);
            if (result == 0)
            {
                result = 0.05;
            }
            return result;
        }
        else if (basePrice > 100 && basePrice < 1000)
        {
            return parseFloat(basePrice * percent / 100).toFixed(0);
        }
        else
        {
            return parseFloat(basePrice * percent / 1000).toFixed(0) * 10;
        }
    }
    // if (basePrice <= 0.25)
    // {
    //     return 0.0625;
    // }
    // else if (basePrice > 0.25 && basePrice <= 1)
    // {
    //     return 0.125;
    // }
    // else if (basePrice > 1 && basePrice <= 5)
    // {
    //     return 0.25;
    // }
    // else if (basePrice > 5 && basePrice <= 20)
    // {
    //     return 0.5;
    // }
    // else if (basePrice > 20 && basePrice <= 100)
    // {
    //     return 1;
    // }
    // else if (basePrice > 100 && basePrice <= 200)
    // {
    //     return 2;
    // }
    // else if (basePrice > 200 && basePrice <= 500)
    // {
    //     return 4;
    // }
    // else if (basePrice > 500 && basePrice <= 1000)
    // {
    //     return 5;
    // }
    // else
    // {
    //     return parseInt(parseInt(basePrice * percentLatticeValue / 100) / 10) * 10 + 10;
    // }
}

// 获取当前实际的格值 按照涨算向下取整 跌则向上取整
function getGridIndexCurrent(stockInfo, curPrice, IsPreUp)
{
    if (IsPreUp)
    {
        return parseInt(Math.floor(curPrice / stockInfo.latticeValue));
    }
    else
    {
        return parseInt(Math.ceil(curPrice / stockInfo.latticeValue));
    }
}
//计算点数图
function CalculateDotGraphic(stockInfo, dataList)
{
    var dotValueList = [];
    //dataList.reverse();
    var status = true;
    var curPrice = 0;
    if (stockInfo.isClose)
    {
        status = parseFloat(dataList[0].close) > parseFloat(dataList[0].open);
        curPrice = dataList[0].close;
    }
    else
    {
        var data1 = dataList[0];
        var data2 = dataList[1];
        var increaseRate = data2.high / data1.low - 1;
        var decreaseRate = 1 - data2.low / data1.high;
        status = parseFloat(increaseRate) > parseFloat(decreaseRate);
        curPrice = status ? data1.high : data1.low;
    }

    //第一个点

    var y0 = getGridIndexCurrent(stockInfo, curPrice, status);
    var position0 = {
        x: 0,
        y: y0
    };
    var datas0 = [];
    datas0.unshift(dataList[0]);
    var dotValue0 = {
        position: position0,
        datas: datas0,
        isFill: false,
        isUp: status
    };
    //插入最前面
    dotValueList.unshift(dotValue0);

    for (var i = 1; i < dataList.length; i++)
    {

        var preDot = dotValueList[0];
        var curData = dataList[i];

        //前点涨
        if (preDot.isUp)
        {
            curPrice = stockInfo.isClose ? curData.close : curData.high;
            var y = getGridIndexCurrent(stockInfo, curPrice, true);
            //当前点也涨
            if (y > preDot.position.y)
            {
                for (var j = preDot.position.y + 1; j <= y; j++)
                {
                    var position = {
                        x: preDot.position.x,
                        y: j
                    };
                    var datas = [];
                    datas.unshift(curData);
                    var dotValue = {
                        position: position,
                        datas: datas,
                        isFill: true,
                        isUp: true
                    };
                    //插入最前面
                    dotValueList.unshift(dotValue);
                }
                //最后一点不是补充点
                dotValueList[0].isFill = false;

            }
            //当前点跌
            else if (y <= preDot.position.y)
            {
                curPrice = stockInfo.isClose ? curData.close : curData.low;
                var y = getGridIndexCurrent(stockInfo, curPrice, false);
                //是否转折

                //大于等于转折格数
                if (preDot.position.y - y >= stockInfo.dotInterval)
                {
                    //换列
                    var x = preDot.position.x + 1;
                    //不换列条件
                    var isNotTurn = isOneDotRebuild && stockInfo.dotInterval == 1 && (preDot.position.x >= 1) &&
                        (dotValueList.length > 2 && dotValueList[0].position.y <= dotValueList[2].position.y) &&
                        (y <= dotValueList[1].position.y) && preDot.position.x - 1 == dotValueList[1].position.x;
                    if (isNotTurn)
                    {
                        x = preDot.position.x;

                    }
                    for (var j = preDot.position.y - 1; j >= y; j--)
                    {
                        var position = {
                            x: x,
                            y: j
                        };
                        var datas = [];
                        datas.unshift(curData);
                        var dotValue = {
                            position: position,
                            datas: datas,
                            isFill: true,
                            isUp: false
                        };
                        //插入最前面
                        dotValueList.unshift(dotValue);
                    }
                    //最后一点不是补充点
                    dotValueList[0].isFill = false;
                }
                //不够转折 直接加入
                else
                {
                    //保持极值的点在第一位
                    if (dotValueList[0].datas.length > 0)
                    {
                        if ((stockInfo.isClose && dotValueList[0].datas[0].close < curData.close) || (!stockInfo.isClose && dotValueList[0].datas[0].high < curData.high))
                        {
                            dotValueList[0].datas.unshift(curData);
                        }
                        else
                        {
                            dotValueList[0].datas.splice(1, 0, curData)

                        }
                    }
                    else
                    {
                        dotValueList[0].datas.unshift(curData);
                    }
                }
            }

            //原地踏步
            // else
            // {
            //     //保持极值的点在第一位
            //     if (dotValueList[0].datas.length > 0)
            //     {
            //         if ((stockInfo.isClose && dotValueList[0].datas[0].close < curData.close) || (!stockInfo.isClose && dotValueList[0].datas[0].high < curData.high))
            //         {
            //             dotValueList[0].datas.unshift(curData);
            //         }
            //         else
            //         {
            //             dotValueList[0].datas.splice(1, 0, curData)

            //         }
            //     }
            //     else
            //     {
            //         dotValueList[0].datas.unshift(curData);
            //     }
            // }
        }
        //前点跌
        else
        {
            curPrice = stockInfo.isClose ? curData.close : curData.low;
            var y = getGridIndexCurrent(stockInfo, curPrice, false);
            //当前点也跌
            if (y < preDot.position.y)
            {
                for (var j = preDot.position.y - 1; j >= y; j--)
                {
                    var position = {
                        x: preDot.position.x,
                        y: j
                    };
                    var datas = [];
                    datas.unshift(curData);
                    var dotValue = {
                        position: position,
                        datas: datas,
                        isFill: true,
                        isUp: false
                    };
                    //插入最前面
                    dotValueList.unshift(dotValue);
                }
                //最后一点不是补充点
                dotValueList[0].isFill = false;
            }
            //当前点涨
            else if (y >= preDot.position.y)
            {
                curPrice = stockInfo.isClose ? curData.close : curData.high;
                var y = getGridIndexCurrent(stockInfo, curPrice, true);
                //是否转折

                //大于等于转折格数
                if (y - preDot.position.y >= stockInfo.dotInterval)
                {
                    //换列
                    var x = preDot.position.x + 1;
                    //不换列条件
                    var isNotTurn = isOneDotRebuild && stockInfo.dotInterval == 1 && (preDot.position.x >= 1) &&
                        (dotValueList.length > 2 && dotValueList[0].position.y <= dotValueList[2].position.y) &&
                        (y <= dotValueList[1].position.y) && preDot.position.x - 1 == dotValueList[1].position.x;
                    if (isNotTurn)
                    {
                        x = preDot.position.x;

                    }

                    for (var j = preDot.position.y + 1; j <= y; j++)
                    {
                        var position = {
                            x: x,
                            y: j
                        };
                        var datas = [];
                        datas.unshift(curData);
                        var dotValue = {
                            position: position,
                            datas: datas,
                            isFill: true,
                            isUp: true
                        };
                        //插入最前面
                        dotValueList.unshift(dotValue);
                    }
                    //最后一点不是补充点
                    dotValueList[0].isFill = false;
                }
                //不够转折 直接加入
                else
                {
                    //保持极值的点在第一位
                    if (dotValueList[0].datas.length > 0)
                    {
                        if ((stockInfo.isClose && dotValueList[0].datas[0].close > curData.close) || (!stockInfo.isClose && dotValueList[0].datas[0].low > curData.low))
                        {
                            dotValueList[0].datas.unshift(curData);
                        }
                        else
                        {
                            dotValueList[0].datas.splice(1, 0, curData)

                        }
                    }
                    else
                    {
                        dotValueList[0].datas.unshift(curData);
                    }
                }
            }
            //原地踏步
            // else
            // {
            //     //保持极值的点在第一位
            //     if (dotValueList[0].datas.length > 0)
            //     {
            //         if ((stockInfo.isClose && dotValueList[0].datas[0].close > curData.close) || (!stockInfo.isClose && dotValueList[0].datas[0].low > curData.low))
            //         {
            //             dotValueList[0].datas.unshift(curData);
            //         }
            //         else
            //         {
            //             dotValueList[0].datas.splice(1, 0, curData)

            //         }
            //     }
            //     else
            //     {
            //         dotValueList[0].datas.unshift(curData);
            //     }
            // }
        }
    }
    return dotValueList;
}
//数据前期处理
function InitData(result)
{
    var returnValue = {};
    //读取数据
    //var result = ReadStockData(code);
    /*处理数据*/
    //1、获取最高、最低点

    var dataList = [];
    var length = result.list.length;
    //挑取时间范围内的数据
    for (var i = 0; i < length; i++)
    {
        var item = result.list[i];
        var date = item.date;
        if (isOnline)
        {
            dataList.push(item);
        }
        else
        {
            if (date >= beginDate && date <= endDate && item.close > 0)
            {
                dataList.push(item);
            }
            else
            {
                continue;
            }
        }
    }


    //调换时间顺序为从最早到今天
    dataList.reverse();
    //复权处理
    /*复权处理
    涨跌幅算法
        复权因子 = 当日的前收盘价 / 昨天的收盘
        向前复权价 = 实际价 * 每一次的复权因子
        (以最后一个点(今天)为基准点向前推)
        向后复权价 = 实际价 / 每一次的复权因子
        (以第一个点为基准点向前推)
    */
    //前复权 昨收价要有值 以今天为基准 改昨天的所有价格
    if (rehabilitation == 1 && dataList[0].lastClose > 0)
    {
        for (var i = dataList.length - 1; i >= 1; i--)
        {
            var item = dataList[i - 1];

            var rehabilitationValue = dataList[i].lastClose / item.close;
            item.close = (item.close * rehabilitationValue).toFixed(2);
            item.high = (item.high * rehabilitationValue).toFixed(2);
            item.low = (item.low * rehabilitationValue).toFixed(2);
            item.open = (item.open * rehabilitationValue).toFixed(2);
            item.lastClose = (item.lastClose * rehabilitationValue).toFixed(2);
        }
    }
    //后复权 以第一天为基准 改明天的价
    else if (rehabilitation == 2 && dataList[0].lastClose > 0)
    {
        for (var i = 0; i < dataList.length - 1; i++)
        {
            var item = dataList[i + 1];
            var rehabilitationValue = item.lastClose / dataList[i].close;
            item.close = (item.close / rehabilitationValue).toFixed(2);
            item.high = (item.high / rehabilitationValue).toFixed(2);
            item.low = (item.low / rehabilitationValue).toFixed(2);
            item.open = (item.open / rehabilitationValue).toFixed(2);
            item.lastClose = (item.lastClose / rehabilitationValue).toFixed(2);
        }
    }
    //最大、最小价格、成交量
    var minPrice = dataList[0].low;
    var maxPrice = dataList[0].high;
    var minVolume = dataList[0].volume;
    var maxVolume = dataList[0].volume;
    for (var i = 1; i < dataList.length; i++)
    {
        var item = dataList[i];

        if (parseFloat(item.low) < parseFloat(minPrice))
        {
            minPrice = item.low;
        }
        if (parseFloat(item.high) > parseFloat(maxPrice))
        {
            maxPrice = item.high;
        }
        if (parseInt(item.volume) < parseInt(minVolume))
        {
            minVolume = item.volume;
        }
        if (parseInt(item.volume) > parseInt(maxVolume))
        {
            maxVolume = item.volume;
        }
    }
    
    curList = dataList;
    //单格值
    if (latticeValue == 0 || isPercentLattice)
    {
        //latticeValue = CalLatticeValue((parseFloat(minPrice) + parseFloat(maxPrice)) / 2);
        latticeValue = CalLatticeValue(curList[curList.length - 1].close);
        var gridNum = parseInt((maxPrice - minPrice) / latticeValue);
        // if (gridNum < 5)
        // {
        //     latticeValue = (latticeValue * 0.7).toFixed(2);
        // }
        document.getElementById("latticeValue").value = latticeValue;
        latticeValue
    }
    //最大最小价格修正
    minPrice=(parseInt(minPrice/latticeValue))*latticeValue;
    maxPrice=(parseInt(maxPrice/latticeValue)+1)*latticeValue;

    //转折格数
    //股票基本信息
    stockInfo = {
        code: result.code,
        name: result.name,
        latticeValue: latticeValue,
        isClose: isClose,
        dotInterval: dotInterval,
        minPrice: minPrice,
        maxPrice: maxPrice,
        minVolume: minVolume,
        maxVolume: maxVolume
    };
    //开始值向下取整
    var startIndex = getGridIndexCurrent(stockInfo, minPrice, true);
    //结束值向上取整
    //var maxPriceNew = ((parseInt(maxPrice / 0.05) + 1) * 0.05).toFixed(2);
    var endIndex = getGridIndexCurrent(stockInfo, maxPrice, false);

    stockInfo.startIndex = startIndex;
    stockInfo.endIndex = endIndex;
    returnValue.stockInfo = stockInfo;

    //计算每个点的位置、涨跌状态
    //console.log("数据条数:"+dataList.length)
    var dotValueList = CalculateDotGraphic(stockInfo, dataList);
    returnValue.dotValueList = dotValueList;
    //读完后删除js引用
    //var delFile = document.getElementById(code);
    //delFile.parentNode.removeChild(delFile);
    return returnValue;
}
//排序的函数
function objKeySort(arys)
{
    //先用Object内置类的keys方法获取要排序对象的属性名，再利用Array原型上的sort方法对获取的属性名进行排序，newkey是一个数组
    var newkey = Object.keys(arys).sort();
    //console.log('newkey='+newkey);
    var newObj = {}; //创建一个新的对象，用于存放排好序的键值对
    for (var i = 0; i < newkey.length; i++)
    {
        //遍历newkey数组
        newObj[newkey[i]] = arys[newkey[i]];
        //向新创建的对象中按照排好的顺序依次增加键值对
    }
    return newObj; //返回排好序的新对象
}
//加载股票列表
function loadStockList()
{
    //var file = keypairFileList["stock_list.csv"];
    //var reader = new FileReader();
    //读取文本文件
    //reader.readAsText(file);
    //reader.onload = function(e)
    //{
    //stockArray=new Array(stockList.length);
    if (isOnlineStockList)
    {
        var url = "http://26.push2.eastmoney.com/api/qt/clist/get?cb=jQuery&pn=1&pz=5000&po=1&np=1&fltt=2&invt=2&fid=f3&fs=m:0+t:6,m:0+t:13,m:0+t:80,m:1+t:2,m:1+t:23&fields=f12,f13,f14";

        return getUrlContent("GET", url, "text").then(function(result)
        {
            if (result == null || result == "")
            {
                //alert("未获取到数据,请检查网络连接状况或者浏览器是否允许跨域访问!");
                return;
            }
            //东方财富的数据不是标准json
            //获得字符串的开始位置
            var start = result.indexOf('[');
            var end = result.indexOf(']');
            var regstr = new RegExp('"', "g");
            var regstr1 = new RegExp('{', "g");
            var regstr2 = new RegExp('}', "g");
            var regstr3 = new RegExp('f12', "g");
            var regstr4 = new RegExp('f13', "g");
            var regstr5 = new RegExp('f14', "g");
            var regstr6 = new RegExp(':', "g");
            var str = result.substring(start + 1, end).replace(regstr, '')
            str = str.replace(regstr1, '')
            str = str.replace(regstr2, '')
            str = str.replace(regstr3, '')
            str = str.replace(regstr4, '')
            str = str.replace(regstr5, '')
            str = str.replace(regstr6, '')

            var dataList = str.split(',');

            var reg = new RegExp('-', "g");
            var reg1 = new RegExp(':', "g");
            var reg2 = new RegExp(' ', "g");
            var lastClose = 0;
            var list = [];
            var distance = 3;

            //指数 自选
            //commonStocklistData
            $('#stock_list_self').empty();
            for (var i = 0; i < commonStocklistData.length; i++)
            {
                var obj = commonStocklistData[i]; //.split(",");
                var code = obj[0];
                var name = obj[1];
                var spell = obj[2];
                html = "<option label=\"" + name + "." + spell + "\" value=\"" + code + "\"  id=\"" + name + "\" />";
                $('#stock_list_self').append(html);
                stockArray[code] = obj;
            }
            for (var i = 0; i < dataList.length / distance; i++)
            {

                var code0 = dataList[i * distance];
                var type0 = dataList[i * distance + 1];
                var name0 = dataList[i * distance + 2];
                var spell = makePy(name0)[0];
                if (code0 == "000001" || stockArray[code0] != null)
                {
                    continue
                }
                type0 = (type0 == "1" ? "sh" : "sz")
                var obj = [];
                obj[0] = code0;
                obj[1] = name0;
                obj[2] = spell;
                obj[3] = type0;
                stockArray[code0] = obj;
            }
            $('#stock_list').empty();
            stockArray = objKeySort(stockArray)
            for (var tag in stockArray)
            {
                var obj = stockArray[tag]
                html = "<option label=\"" + obj[1] + "." + obj[2] + "\" value=\"" + obj[0] + "\"  id=\"" + obj[1] + "\" />";
                $('#stock_list').append(html);
            }
        });
    }
    else
    {
        $('#stock_list_self').empty();
        for (var i = 0; i < commonStocklistData.length; i++)
        {
            var obj = commonStocklistData[i]; //.split(",");
            var code = obj[0];
            var name = obj[1];
            var spell = obj[2];
            html = "<option label=\"" + name + "." + spell + "\" value=\"" + code + "\"  id=\"" + name + "\" />";
            $('#stock_list_self').append(html);
            stockArray[code] = obj;
        }

        var stockList = stocklistData;
        $('#stock_list').empty();
        //添加下拉列表数据
        for (var i = 0; i < stockList.length; i++)
        {
            var obj = stockList[i]; //.split(",");
            var code = obj[0];
            var name = obj[1];
            var spell = obj[2];
            html = "<option label=\"" + name + "." + spell + "\" value=\"" + code + "\"  id=\"" + name + "\" />";
            $('#stock_list').append(html);
            stockArray[code] = obj;
        }
    }
}
//加载本地数据文件列表
function loadFileList()
{

    var reader = new FileReader();
    var fileList = document.getElementById("filePicker").files;
    for (var i = 0; i < fileList.length; i++)
    {
        var name = fileList[i].name;
        keypairFileList[name] = fileList[i];
    }
    isOnlineStockList = false;
    loadStockList();
    code = document.getElementById("stock_list_self").options[0].value;
    document.getElementById("code_input_self").value = code;
    document.getElementById("code_input").value = code;
    generate();
}
//异步调用实现顺序执行
function ReadStockData()
{
    if (!isOnline)
    {
        return new Promise(function(resolve, reject)
        {
            var file = keypairFileList[code + ".csv"];
            var reader = new FileReader()
            reader.readAsText(file)
            reader.onload = function()
            {
                var dataList = this.result.split("\n");
                resolve(dataList)
            }
        })
    }
    else
    {
        return getOnlineData();
    }
}
//开始画图
function StartDraw(dataList)
{
    var resultList = {};
    var list = [];
    //拆分csv数据
    var reg = new RegExp('-', "g");
    for (var i = 0; i < dataList.length; i++)
    {
        var data = dataList[i].split(",");
        if (data[0] == "")
        {
            continue;
        }
        //日期,股票代码,名称,收盘价,最高价,最低价,开盘价,前收盘,成交量,成交金额
        var item = {
            date: data[0].replace(reg, ''),
            close: parseFloat(data[3]),
            high: parseFloat(data[4]),
            low: parseFloat(data[5]),
            open: parseFloat(data[6]),
            lastClose: parseFloat(data[7]),
            volume: data[8],
            amount: parseFloat(data[9]),
            turnoverRate: parseFloat(data[10])
        };
        list.push(item);
    }
    resultList.list = list;
    resultList.code = code;
    resultList.name = name;

    var returnValue = InitData(resultList);
    DrawPointAndFigure(returnValue);
    DrawKLine(curList, returnValue.stockInfo);
}

function DrawOX()
{
    canvasD = document.getElementById("dotGraphic");
    contextD = canvasD.getContext("2d");
    //canvasD.onmousemove = onMouseMoveD;
    canvasD.addEventListener("mousemove", onMouseMoveD);

    //canvasD.onkeydown = onKeydownD;
    //作画前清理画布
    contextD.clearRect(0, 0, canvasD.width, canvasD.height);
    contextD.strokeStyle = "rgb(0,0,0)";
    contextD.lineWidth = 1;
    canvasD.width = 900;
    canvasD.height = 600;

    canvasK = document.getElementById("kline");
    contextK = canvasK.getContext("2d");
    //canvasK.onmousemove = onMouseMoveK;
    canvasK.addEventListener("mousemove", onMouseMoveK);

    //作画前清理画布
    contextK.clearRect(0, 0, canvasK.width, canvasK.height);
    contextK.strokeStyle = "rgb(0,0,0)";
    contextK.lineWidth = 1;
    canvasK.width = 900;
    canvasK.height = 600;

    //document.write("<script  id=\"" + code + "\" type=\"text\/javascript\" src=\"./data/" + code + ".js\"><\/script>");
    //document.close();

    //等待加载完成
    //document.onreadystatechange = completeLoading;
    var returnValue = {};
    //加载状态为complete时移除loading效果
    //function completeLoading()
    //{
    //if (document.readyState == "complete")
    //{

    //重要
    //本地数据的读取是异步进行的,这里的意思是：
    //ReadStockData函数执行完成后在执行StartDraw
    //这就达到了单线程处理的目的
    ReadStockData().then(function(result)
    { //处理 result
        StartDraw(result);
        doc = document.getElementById("right");
        scrollBottomAndRightTag(doc)

    });

}

function scrollBottomAndRightTag(doc)
{
    doc.scrollTop = 0;
    doc.scrollLeft = 99999;
}

function scrollBottomAndRightDocument()
{
    var height = 99999;
    if (typeof window.pageYOffset != 'undefined')
    {
        window.pageYOffset = height;
        window.pageXOffset = height;
    }
    if (typeof document.compatMode != 'undefined' &&
        document.compatMode != 'BackCompat')
    {
        document.documentElement.scrollTop = height;
        document.documentElement.scrollLeft = height;
    }
    if (typeof document.body != 'undefined')
    {
        document.body.scrollTop = height;
        document.body.scrollLeft = height;
    }
}

function onKeydownD(e)
{
    if (e.keyCode == 33) //up
    {

    }
    if (e.keyCode == 34) //down
    {

    }
}

function onMouseMoveD(e)
{
    drawmouseCross(e);
    var tip = document.getElementById("dotGraphic_tip");
    tip.style.display = 'none';
    var px = e.layerX;
    var py = e.layerY;
    var x = parseInt(px / spaceD);
    var y = parseInt(py / spaceD);
    try
    {

        var curDot = table[x][y];

        if (typeof(curDot) != "undefined" && curDot != 0)
        {
            drawToolTipD(curDot, px, py);
        }
    }
    catch (err)
    {
        return;
    }
}
//绘制tooltip提示文字
function drawToolTipD(dotValue, x, y)
{
    var fill = (dotValue.isFill ? "[填]" : "") + "[" + dotValue.position.x + "]";
    var colorClose = "black";
    var colorHigh = "black";
    var colorLow = "black";
    var colorDefault = "black";
    if (isClose)
    {
        colorClose = "red";
    }
    else
    {
        if (dotValue.isUp)
        {
            colorHigh = "red";
        }
        else
        {
            colorLow = "red";
        }
    }
    var strList = "<br/>数据列表：<br/>";
    var row = 1;
    for (var i = 0; i < dotValue.datas.length; i++)
    {
        var date = dotValue.datas[i].date;
        if (cycle == "fmin")
        {
            date = date.substring(4, 12)
        }
        var str6 = date + ":";
        var price = dotValue.datas[i].close;
        if (!isClose)
        {
            if (dotValue.isUp)
            {
                price = dotValue.datas[i].high;
            }
            else
            {
                price = dotValue.datas[i].low;
            }
        }
        strList += str6 + parseFloat(price).toFixed(2) + "<br/>";
        row = row + 1;
    }
    var volume = ((dotValue.datas[0].volume) / 10000).toFixed(2);
    var amount = ((dotValue.datas[0].amount) / 100000000).toFixed(2);
    var date = dotValue.datas[0].date;
    // if (cycle == "fmin")
    // {
    //     date = date.substring(4, 12)
    // }
    var tipHtml = '<div><b>' + date + fill + '</b><br/>' +
        '<font color="' + colorClose + '">' + "收盘价:" + dotValue.datas[0].close + '</font><br/>' +
        '<font color="' + colorHigh + '">' + "最高价:" + dotValue.datas[0].high + '</font><br/>' +
        '<font color="' + colorLow + '">' + "最低价:" + dotValue.datas[0].low + '</font><br/>' +
        '<font color="' + colorDefault + '">' + "昨收价:" + dotValue.datas[0].lastClose + '</font><br/>' +
        '<font color="' + colorDefault + '">' + "开盘价:" + dotValue.datas[0].open + '</font><br/>' +

        '成交量:' + volume + '万手<br/>' +
        '成交额:' + amount + '亿元<br/>';
    row = row + 7;
    tipHtml = tipHtml + strList + "</div";

    var width = 130;
    var height = row * 17;

    var tip = document.getElementById("dotGraphic_tip");
    tip.style.cssText = 'font-family:Arial,"宋体";font-size:8pt';
    tip.style.position = 'absolute';
    tip.style.zIndex = 4;
    tip.style.backgroundColor = 'white';
    tip.style.border = '1px solid gray';
    tip.style.width = width + 'px';
    tip.style.height = height + 'px';
    var left = document.getElementById('left');
    if ((x + width + 80) > canvasD.offsetWidth)
    {
        tip.style.left = (x - width - 20) + 'px';
    }
    else if ((x + width + 80) < left.with)
    {
        tip.style.left = (x + width + 20) + 'px';
    }
    else
    {
        tip.style.left = (x + 20) + 'px';
    }
    if ((y + height) > canvasD.offsetHeight)
    {
        tip.style.top = (y - 10 - height) + 'px';
    }
    else
    {
        tip.style.top = (y + 5) + 'px';
    }
    tip.style.display = 'block';
    //tipHtml="测试"
    //tip.style.height="20px";
    tip.innerHTML = tipHtml;
}


function onMouseMoveK(e)
{
    drawmouseCross(e);
    try
    {
        var tip = document.getElementById("kline_tip");
        tip.style.display = 'none';
        var px = e.layerX;
        var py = e.layerY;
        var x = parseInt(px / spaceK);
        var y = parseInt(py / spaceK);
        if (x >= 0 && y >= 0)
        {
            drawToolTipK(tip, px, py);
        }
    }
    catch (err)
    {
        return;
    }

}

function drawToolTipK(tip, x, y)
{
    tip.style.display = 'none';
    var i = parseInt((x - tableK[0].offsetXSpace) / tableK[0].spaceX);
    var dot = tableK[i];
    //是否处于K线范围内
    if (!(y >= dot.endpyHL && y <= dot.startpyHL) || (x < dot.startpx + dot.spaceX / 3 || x > dot.endpx - dot.spaceX / 3))
    {
        return;
    }
    var obj = dot.item;
    var volume = ((obj.volume) / 10000).toFixed(2);
    var amount = ((obj.amount) / 100000000).toFixed(2);
    var date = obj.date;
    // if (cycle == "fmin")
    // {
    //     date = date.substring(4, 12)
    // }
    var tipHtml = '<div><b>日期:' + date +
        "</b><br/>收盘:" + obj.close +
        "<br/>最高价:" + obj.high +
        "<br/>最低:" + obj.low +
        "<br/>昨收:" + obj.lastClose +
        "<br/>开盘:" + obj.open +
        "<br/><font color=\"" + ((obj.close > obj.lastClose) ? "red" : "blue") + "\">涨幅:" + parseFloat((obj.close / (obj.lastClose == 0 ? obj.open : obj.lastClose)) * 100 - 100).toFixed(2) +
        "%</font><br/>涨幅差:" + parseFloat(obj.close - obj.lastClose).toFixed(2) +
        "<br/>振幅:" + (parseFloat(obj.high - obj.low) / obj.low * 100).toFixed(2) +
        "%<br/>振幅差:" + parseFloat(obj.high - obj.low).toFixed(2) +
        "<br/>量:" + volume +
        "万手<br/>换手率:" + obj.turnoverRate + "%<br/>" +
        "额:" + amount + "亿元";
    var row = 13;
    tipHtml = tipHtml + "</div";

    var width = 130;
    var height = row * 15;
    tip.style.cssText = 'font-family:Arial,"宋体";font-size:8pt';
    tip.style.position = 'absolute';
    tip.style.zIndex = 100;
    tip.style.backgroundColor = 'white';
    tip.style.border = '1px solid gray';
    tip.style.width = width + 'px';
    tip.style.height = height + 'px';
    var left = document.getElementById('left');
    if ((x + width + 80) > canvasK.offsetWidth)
    {
        tip.style.left = (x - width - 20) + 'px';
    }
    else if ((x + width + 80) < left.with)
    {
        tip.style.left = (x + width + 20) + 'px';
    }
    else
    {
        tip.style.left = (x + 20) + 'px';
    }
    if ((y + height) > canvasK.offsetHeight)
    {
        tip.style.top = (y - height - 10) + 'px';
    }
    else
    {
        tip.style.top = (y + 10) + 'px';
    }
    tip.style.display = 'block';
    tip.innerHTML = tipHtml;

    //drawCrossScale(x, y);
}

function replaceStr(str, searchValue, replaceValue)
{
    var regExp = new RegExp(searchValue, "g");
    return str.replace(regExp, replaceValue);
}

var originalUserAgent = navigator.userAgent;

function fillInfo()
{
    //基本信息
    var codename = "<b>[" + code + "]    " + name + "</b></br>";
    var lastData = curList[curList.length - 1];
    var preData = curList[curList.length - 2];

    var dateStr = "今日:" + lastData.date;
    var rate = (latticeValue * 100 / stockInfo.maxPrice).toFixed(2) + "%~" + parseFloat(latticeValue * 100 / stockInfo.minPrice).toFixed(2) + "%";
    var rate1 = parseFloat(lastData.close / preData.close * 100 - 100).toFixed(2);
    var colorRate1 = rate1 > 0 ? "red" : "blue";
    var priceNowStr = "&nbsp;&nbsp;<p style=\"display:inline;font-weight:bolder; color:" + colorRate1 + "!important\">收:" + lastData.close + "</p>";
    var priceStr = "<p style=\"color:black !important\">高:" + lastData.high + "&nbsp;&nbsp;低:" + lastData.low + "&nbsp;&nbsp;换:" + lastData.turnoverRate + "%</p>";

    var rateStr = "<p style=\"color:black !important\">格幅:" + rate + "</p><p style=\"color:black !important\">缩放:" + (scale * 100).toFixed(0) + "%</font><p style=\"display:inline;font-weight:bolder; color:" + colorRate1 + "!important\">涨幅:" + rate1 + "%</p>";

    document.title = name + " 收:" + lastData.close;
    var profit = "";
    var obj = stockArray[code];
    if (obj.length == 5 && obj[4] !== "")
    {
        var ratep = (lastData.close / parseFloat(obj[4]) * 100 - 100).toFixed(2);
        var profitStr = (ratep > 0 ? "盈利" : "亏损");
        var colorp = (ratep > 0 ? "Crimson" : "DarkGreen");
        var fsize = "10pt";
        if (parseFloat(ratep) > 3)
        {
            fsize = "25pt";
        }
        if (parseFloat(ratep) < -2)
        {
            fsize = "25pt";
        }
        profit = "<p style=\"text-align: center;font-size:" + fsize + ";font-weight:bolder; color:" + colorp + "!important\">" + profitStr + ":" + Math.abs(ratep) + "%" + "[" + obj[4] + "]" + "</p>";
    }
    var strHtml = codename + dateStr + priceNowStr + priceStr + rateStr + profit;
    document.getElementById("baseInfo").innerHTML = strHtml;

    var iwc = "http://www.iwencai.com/unifiedwap/result?w=" + code + "&querytype=stock&issugs"
    //var sl = "https://m.xuangubao.cn/stocklabel/" + code + "." + (type == "sh" ? "ss" : "sz") + "?mine=true"
    var dfcf = "http://quote.eastmoney.com/" + type + code + ".html"
    var xgb = "https://xuangubao.cn/stock/" + code + (type == "sh" ? ".SS" : ".SZ");
    var xq = "https://xueqiu.com/S/" + type.toUpperCase() + code;
    var sl1 = "https://api-ddc-wscn.xuangubao.cn/extract/stock_risk/full_desc?stock_code=" + code + (type == "sh" ? ".ss" : ".sz");
    var indexList = ["000001", "399001", "399006", "399005", "000300"];
    var codeNew = code;
    if (indexList.indexOf(code) > -1)
    {
        codeNew = type + code;
    }
    var jrj = "http://stock.jrj.com.cn/share," + codeNew + ".shtml";
    document.getElementById("iwc").href = iwc;
    document.getElementById("dfcf").href = dfcf;
    document.getElementById("xgb").href = xgb;
    //document.getElementById("sl").href = sl;
    document.getElementById("xq").href = xq;
    document.getElementById("jrj").href = jrj;

    return getUrlContent("GET", sl1, "json").then(function(jsonStr)
    {
        if (jsonStr == null || jsonStr == "")
        {
            //alert("未获取到数据,请检查网络连接状况或者浏览器是否允许跨域访问!");
            return;
        }
        //console.log(jsonStr);
        if (jsonStr.message == "OK")
        {
            var risk_level = jsonStr.data.risk_level;
            switch (risk_level)
            {
                case 0:
                    risk_level = "<a style=\"color:green\">风险级别：安全";
                    break;
                case 1:
                    risk_level = "<a style=\"color:#ff7575;background-color:yellow;font-weight:bold\">风险级别：低风险";
                    break;
                case 2:
                    risk_level = "<a style=\"color:#FF5151;background-color:yellow;font-weight:bold\">风险级别：中风险";
                    break;
                case 3:
                    risk_level = "<a style=\"color:red;background-color:yellow;font-weight:bold\">风险级别：高风险";
                    break;

            }
            var html = "<a>星级：" + jsonStr.data.stars + "</a>" +
                risk_level + "</a></br>" +
                "<a style=\"color:red\">危险项：" + jsonStr.data.risk_count + "</a>" +
                "<a style=\"color:green\">安全项：" + jsonStr.data.safe_count + "</a>"
            $('#riskAssessment').html("");
            $('#riskAssessment').append(html);
            var htmlTable = "<table id=\"riskInfoTable\"><tr><th>星级</th><th>权重</th><th>类别</th><th>结论</th><th>描述</th></tr>";
            for (var i = 0; i < jsonStr.data.items.length; i++)
            {
                var item = jsonStr.data.items[i];
                var tr = "<tr>";
                var str = item["risk_name"];
                var reg1 = RegExp(/退市风险/);
                var reg2 = RegExp(/公司负面消息/);
                var reg3 = RegExp(/监管处罚/);
                if ((str.match(reg1) || str.match(reg2) || str.match(reg3) || parseInt(item["stars"]) > 0) && item["title"] != "无")
                {
                    tr = "<tr style=\"color:red;font-weight:bold\">";
                    htmlTable = htmlTable + tr +
                        "<th>" + item["stars"] + "</th>" +
                        "<th>" + item["weight"] + "</th>" +
                        "<th>" + item["risk_name"] + "</th>" +
                        "<th>" + item["title"] + "</th>" +
                        "<th>" + item["description"] + "</th>" +
                        "</tr>"
                }

            }
            htmlTable = htmlTable + "</table>";
            $('#riskAssessment').append(htmlTable);
        }
    });
}

function changeUserAgent(userAgent)
{
    Object.defineProperty(navigator, 'userAgent',
    {
        value: userAgent,
        writable: false
    });
}

function setUserAgent(window, userAgent)
{
    // Works on Firefox, Chrome, Opera and IE9+
    if (navigator.__defineGetter__)
    {
        navigator.__defineGetter__('userAgent', function()
        {
            return userAgent;
        });
    }
    else if (Object.defineProperty)
    {
        Object.defineProperty(navigator, 'userAgent',
        {
            get: function()
            {
                return userAgent;
            }
        });
    }
    // Works on Safari
    if (window.navigator.userAgent !== userAgent)
    {
        var userAgentProp = {
            get: function()
            {
                return userAgent;
            }
        };
        try
        {
            Object.defineProperty(window.navigator, 'userAgent', userAgentProp);
        }
        catch (e)
        {
            window.navigator = Object.create(navigator,
            {
                userAgent: userAgentProp
            });
        }
    }
}

function generate()
{
    code = document.getElementById("code_input").value;
    if (code == "" || code == "请输入股票代码")
    {
        alert("请输入股票代码");
        return;
    }
    dotInterval = 1;
    latticeValue = document.getElementById("latticeValue").value;
    latticeValue = latticeValue == "" ? 0 : latticeValue;
    var radio1 = document.getElementById("oneDot");
    var radio3 = document.getElementById("threeDot");
    var radio5 = document.getElementById("fiveDot");
    if (radio1.checked)
    {
        dotInterval = 1;
    }
    else if (radio3.checked)
    {
        dotInterval = 3;
    }
    else if (radio5.checked)
    {
        dotInterval = 5;
    }
    var reg = new RegExp('-', "g")
    beginDate = document.getElementById("beginDate").value.replace(reg, '');
    endDate = document.getElementById("endDate").value.replace(reg, '');
    var radioClose = document.getElementById("close");
    var radioHighLow = document.getElementById("highLow");
    isClose = 1;
    if (radioClose.checked)
    {
        isClose = 1;
    }
    else if (radioHighLow.checked)
    {
        isClose = 0;
    }
    var radiokline = document.getElementById("kline_g");
    if (radiokline.checked)
    {
        isKline = true;
    }
    else
    {
        isKline = false;

    }
    var isOnlineTag = document.getElementById("online");
    if (isOnlineTag.checked)
    {
        isOnline = true;
    }
    else
    {
        isOnline = false;
    }
    var strFile = JSON.stringify(keypairFileList);
    if (strFile == "{}" && !isOnline)
    {
        alert("离线模式请加载离线数据!");
        return;
    }
    cycle = document.getElementById("cycle").value;
    if (cycle != "日" && !isOnline)
    {
        alert("离线模式目前只支持日线数据,可以选择在线数据!");
        return;
    }
    if (cycle == "")
    {
        alert("请选择周期");
    }
    dataSource = document.getElementById("datasource").value;
    if (dataSource == "")
    {
        document.getElementById("datasource").value = "东方财富";
        dataSource = "东方财富";
    }
    // var oneDotRebuild = document.getElementById("oneDotRebuild");
    // if (oneDotRebuild.checked)
    // {
    //     isOneDotRebuild = true
    // }
    // else
    // {
    //     isOneDotRebuild = false
    // }
    var noRehabilitation = document.getElementById("noRehabilitation");
    var beforeRehabilitation = document.getElementById("beforeRehabilitation");
    var afterRehabilitation = document.getElementById("afterRehabilitation");
    if (noRehabilitation.checked)
    {
        rehabilitation = 0;
    }
    else if (beforeRehabilitation.checked)
    {
        rehabilitation = 1;
    }
    else if (afterRehabilitation.checked)
    {
        rehabilitation = 2;
    }
    var percentLatticeCheck = document.getElementById("percentLatticeCheck");
    var percentlatticeInput = document.getElementById("percentlatticeInput");
    isPercentLattice = percentLatticeCheck.checked;
    if (isPercentLattice)
    {
        percentLatticeValue = percentlatticeInput.value;
    }

    var obj = stockArray[code];
    name = obj[1];
    type = obj[3];
    DrawOX();
    document.getElementById("btGenerate").focus();
};
//未用
function fetchUrlContent(url)
{
    var url = "http://finance.sina.com.cn/realstock/company/sz002095/qianfuquan.js?d=2015-06-16";
    fetch(url)
        .then(response =>
        {
            //这个 response 就是服务器返回的可读数据流
            return response.text();
        })
        .then(data =>
        {
            //第二个 .then 中拿到的 data, 就是最终的数据
            document.getElementById('test').innerHTML = data;
        })
}

function getUrlContentGBKtoUTF8(method, url)
{
    console.log(url)
    // 返回一个Promise对象
    return new Promise(function(resolve)
    {
        var xhr = new XMLHttpRequest() // 创建异步请求

        // 异步请求状态发生改变时会执行这个函数
        xhr.onreadystatechange = function()
        {
            // status == 200 用来判断当前HTTP请求完成
            if (xhr.readyState == 4 && xhr.status == 200)
            {
                var dataView = new DataView(xhr.response);
                var decoder = new TextDecoder("gbk");
                var decodedString = decoder.decode(dataView);

                console.log(decodedString)
                resolve(decodedString)
            }
        }
        xhr.open(method, url) // 使用GET方法获取
        xhr.responseType = 'arraybuffer';
        xhr.send() // 发送异步请求 
    })
}

function getUrlContent(method, url, responseType)
{
    console.log(url)

    // 返回一个Promise对象
    return new Promise(function(resolve)
    {
        var xhr = new XMLHttpRequest() // 创建异步请求
        xhr.timeout = 3000;
        // 异步请求状态发生改变时会执行这个函数
        xhr.onreadystatechange = function()
        {
            // status == 200 用来判断当前HTTP请求完成
            if (xhr.readyState == 4)
            {
                if (xhr.status == 200)
                {
                    resolve(xhr.response)
                }
                else
                {
                    resolve("");
                }
            }
        }
        // xhr.timeout= function()
        // {
        //     //xhr.abort();
        //     //resolve("");
        // }
        xhr.open(method, url)
        xhr.responseType = responseType;
        xhr.send() // 发送异步请求 
    })
}

function getOnlineData()
{
    var len = 250;
    if (dataSource == "新浪")
    {
        var klt;
        if (cycle == "日")
        {
            klt = 240;
            var starttime = new Date(document.getElementById("beginDate").value).getTime();
            var endtime = new Date(document.getElementById("endDate").value).getTime();
            len = parseInt(Math.ceil((endtime - starttime) / (1000 * 60 * 60 * 24)) * 5 / 7);
        }
        else if (cycle == "周")
        {
            klt = 1200;
        }
        else if (cycle == "月" || cycle == "季" || cycle == "年")
        {
            alert("新浪股票月线及以上需要根据日线计算,无法抓取")
            return;
        }
        else
        {
            klt = parseInt(cycle.replace('分钟', ''))
        }
        var url = "http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=" + type + code + "&scale=" + klt + "&ma=no&datalen=" + len;

        return getUrlContent("GET", url, "json").then(function(result)
        {
            if (result == null || result == "")
            {
                //alert("未获取到数据,请检查网络连接状况或者浏览器是否允许跨域访问!");
                return;
            }
            var list = [];
            var reg = new RegExp('-', "g");
            var reg1 = new RegExp(':', "g");
            var reg2 = new RegExp(' ', "g");
            var lastClose = 0;
            for (var i = 0; i < result.length; i++)
            {
                if (i > 0)
                {
                    lastClose = result[i - 1]['close'];
                }
                var data = result[i];
                var date = data['day'].replace(reg, '').replace(reg1, '').replace(reg2, '').substring(0, 12);
                var item = date + "," + ",," + data['close'] + "," + data['high'] + "," + data['low'] + "," +
                    data['open'] + "," + lastClose + "," + data['volume'] + ",0";
                list.push(item);
            }
            list.reverse();
            return list;
        });
    }
    else if (dataSource == "东方财富")
    {
        var klt;
        if (cycle == "日")
        {
            klt = 101;
        }
        else if (cycle == "周")
        {
            klt = 102;
        }
        else if (cycle == "月")
        {
            klt = 103;
        }
        else
        {
            klt = parseInt(cycle.replace('分钟', ''))
        }

        var typeNum = (type == "sh" ? "1" : "0");
        var url = "http://push2his.eastmoney.com/api/qt/stock/kline/get?cb=jQuery&secid=" + typeNum + "." + code + "&fields1=f1%2Cf2%2Cf3%2Cf4%2Cf5&fields2=f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf61&klt=" + klt + "&fqt=" + rehabilitation + "&beg=" + beginDate + "&end=" + endDate;

        return getUrlContent("GET", url, "text").then(function(result)
        {
            if (result == null || result == "")
            {
                //alert("未获取到数据,请检查网络连接状况或者浏览器是否允许跨域访问!");
                return;
            }
            //东方财富的数据不是标准json
            //获得字符串的开始位置
            var start = result.indexOf('[');
            var end = result.indexOf(']');
            //日期,股票代码,名称,开盘价,收盘价,最高价,最低价,前收盘,成交量,成交金额 date,open,close,high,low,volume,amount
            var regstr = new RegExp('"', "g");
            var str = result.substring(start + 1, end).replace(regstr, '')
            var dataList = str.split(',');

            var reg = new RegExp('-', "g");
            var reg1 = new RegExp(':', "g");
            var reg2 = new RegExp(' ', "g");
            var lastClose = 0;
            var list = [];
            var distance = 8
            for (var i = 0; i < dataList.length / distance; i++)
            {
                if (i > 0)
                {
                    lastClose = dataList[(i - 1) * distance + 2];
                }
                var date = dataList[i * distance].replace(reg, '').replace(reg1, '').replace(reg2, '').substring(0, 12);
                var item = date + "," + ",," + dataList[i * distance + 2] + "," + dataList[i * distance + 3] + "," + dataList[i * distance + 4] + "," +
                    dataList[i * distance + 1] + "," + lastClose + "," + dataList[i * distance + 5]  + "," + dataList[i * distance + 6] + "," + dataList[i * distance + 7];
                list.push(item);
            }
            list.reverse();
            return list;
        });
    }
}