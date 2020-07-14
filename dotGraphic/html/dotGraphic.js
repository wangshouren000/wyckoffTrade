/*数据结构：
    日期,股票代码,名称,收盘价,最高价,最低价,开盘价,前收盘,成交量,成交金额
  在线数据来源:
        新浪："http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=sh000001&scale=5&ma=no&datalen=1023"
        (day,open,high,low,close,volume)
    可以获取5、10、30、60、240分钟数据，最多可获取1023个周期
  离线数据来源：
        网易：http://quotes.money.163.com/service/chddata.html?code=0000001&start=20000101&end=20200101&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP
        获取日线数据,数据可能会滞后一天


作者：苹果上的小豌豆
链接：https://www.jianshu.com/p/2f45fcb44771
来源：简书
著作权归作者所有。商业转载请联系作者获得授权，非商业转载请注明出处。
*/
var stockArray = {};
var name;
var code;
var type;
var isOnline = false;
var keypairFileList = {};
var canvas;
var context;
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
var minSpace = 17; //最小
var maxSpace = 25;
var space = minSpace;
var spaceDot = space;
var spaceK = space;
//存放点的信息
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
    var offsetX = 2;
    var offsetY = 2;

    //索引后面偏移量

    var offsetXL = 8;
    var offsetYL = 6;
    //初始化
    {
        //清理
        table = [];

        var dotValueList = returnValue.dotValueList;
        var latticeValue = returnValue.stockInfo.latticeValue;
        // 定义当前坐标

        minSpace = 15; //最小
        maxSpace = 25;
        var yIndex = returnValue.stockInfo.endIndex - returnValue.stockInfo.startIndex + offsetY;
        var xIndex = dotValueList[0].position.x + offsetX;
        var spacex = canvas.width / xIndex;
        var spacey = canvas.height / yIndex;
        if (spacex > minSpace && spacey > minSpace)
        {
            space = parseInt(spacey > spacex ? spacex : spacey);
            if (space > maxSpace)
            {
                space = maxSpace;
            }

        }
        spaceDot = space;
        canvas.width = space * (xIndex + offsetXL);
        canvas.height = space * (yIndex + offsetYL);
        table = SetTable(xIndex, yIndex);
        var offsetXSpace = offsetX * space;
        var offsetYSpace = offsetY * space;

        //抗锯齿 抗模糊
        var width = canvas.width,
            height = canvas.height;

        if (window.devicePixelRatio)
        {
            canvas.style.width = width + "px";
            canvas.style.height = height + "px";
            canvas.height = height * devicePixelRatio;
            canvas.width = width * devicePixelRatio;
            context.scale(devicePixelRatio, devicePixelRatio);
        }
        //0点
        context.beginPath();
        context.arc(offsetXSpace, offsetYSpace, 5, 0, 2 * Math.PI);
        context.fillStyle = "yellow";
        context.fill();
        context.stroke();
    }

    //绘制水平方向的网格线
    //线条比格子多1,且加入上面一格刻度，下面两格刻度
    for (var y = 0; y <= yIndex - offsetY; y++)
    {
        //开启路径
        context.beginPath()
        //水平线要多画一格(最高点是向下取整的,所以多一格,且最上方要封顶)
        context.moveTo(offsetXSpace, space * y + offsetYSpace)
        context.lineTo(space * xIndex + space, space * y + offsetYSpace)
        context.stroke();

        context.save();
        context.fillStyle = 'red'
        context.font = 'bold 10px 微软雅黑'
        context.textBaseline = 'right';
        //设置文本的垂直对齐方式
        context.textAlign = 'right';

        //标垂直刻度
        //注意：网格是从前偏移量的位置开始画的,所以最后列也是同等位移
        //offsetXSpace offsetYSpace
        var str = (Math.round(returnValue.stockInfo.minPrice, 2) + (yIndex - y - offsetY) * latticeValue).toFixed(2);
        context.fillText(str, offsetXSpace - space / 2, y * space + offsetYSpace);

        context.fillText(str, space * xIndex + space * 3, y * space + offsetYSpace);
    }
    //加两行 放日期
    // context.beginPath()
    // context.moveTo(offsetXSpace, space * yIndex + space)
    // context.lineTo(space * xIndex + space, space * yIndex + space)
    context.stroke();

    context.beginPath()
    context.moveTo(offsetXSpace, space * yIndex + space * 2)
    context.lineTo(space * xIndex + space, space * yIndex + space * 2)
    context.stroke();

    //绘制垂直方向的网格线
    //增加上下三格刻度
    for (var x = 0; x <= xIndex - offsetX + 1; x++)
    {
        //开启路径
        context.beginPath()
        context.moveTo(offsetXSpace + x * space, offsetYSpace)
        context.lineTo(offsetXSpace + x * space, space * yIndex + space * 2)
        context.stroke()

        //标顶部水平刻度
        context.save();
        context.fillStyle = 'black'
        context.font = 'bold 6px 微软雅黑'
        context.textBaseline = 'bottom';
        //设置文本的垂直对齐方式
        context.textAlign = 'center';
        var str = x;
        context.fillText(str, offsetXSpace + space * x, offsetYSpace - space / 2);
        context.restore();
    }
    //填充OX
    context.save();
    context.font = 'bold 15px 微软雅黑'
    context.textBaseline = 'middle';
    context.textAlign = 'center';

    var isUp = dotValueList[0].isUp;

    for (var i = 0; i < dotValueList.length; i++)
    {
        var dotValue = dotValueList[i];
        var curIsUp = dotValue.isUp;
        var x = (dotValue.position.x + offsetX);
        var y = yIndex - (dotValue.position.y - returnValue.stockInfo.startIndex) - 1;
        var px = x * space + space / 2;
        var py = y * space + space / 2;

        //记录某位置的值 备用
        table[x][y] = dotValue;

        if (dotValue.isUp)
        {
            //画X
            context.font = 'bold 15px 微软雅黑';
            if (dotValue.isFill)
            {
                context.fillStyle = 'lightgray';
                context.fillRect(px - space / 2, py - space / 2, space, space);
            }
            context.fillStyle = 'red';
            context.fillText("X", px, py);
        }
        else
        {
            //画O
            context.font = 'bold 15px 微软雅黑';
            if (dotValue.isFill)
            {
                context.fillStyle = 'lightgray';
                context.fillRect(px - space / 2, py - space / 2, space, space);
            }
            context.fillStyle = 'blue';
            context.fillText("O", px, py);
        }

        if (isUp != curIsUp)
        {
            isUp = curIsUp;
            context.save();
            //标记日期
            date = dotValue.datas[0].date;

            if (cycle == "日")
            {
                date = date.substring(0, 6)
            }
            else
            {
                date = date.substring(4, 12)
            }
            //date = date.substring(2, 6);
            //year = date.substring(0, 2);
            //month = date.substring(2, 4);
            context.font = 'bold 10px 微软雅黑';
            //context.fillText(month, space * x + space / 2, space * yIndex + space * 3 / 2);
            context.fillStyle = 'black';
            context.translate(space * x + space / 2, space * yIndex + space * 0.9);
            context.rotate(90 * Math.PI / 180);
            context.fillText(date, 0, 0);
            context.restore();

        }

    }

    //年、 月标记
    // context.font = 'bold 10px 微软雅黑';
    // context.fillStyle = 'black';
    // context.fillText("月", offsetXSpace - space / 2, space * yIndex + space / 2);
    // context.fillText("年", offsetXSpace - space / 2, space * yIndex + +space * 3 / 2);

    // context.fillText("月", space * xIndex + space / 2, space * yIndex + space / 2);
    // context.fillText("年", space * xIndex + space / 2, space * yIndex + space * 3 / 2);

    //标记 code name 最新日期
    context.textAlign = "left"
    context.fillStyle = 'blue';
    context.fillText("最新日期:" + dotValueList[0].datas[0].date + "    " +
        "[" + returnValue.stockInfo.code + "]" + returnValue.stockInfo.name, space * xIndex - space * 6, space * yIndex + space * 3);

    context.fillText("最低价:" + returnValue.stockInfo.minPrice + "    " + "单格值:" + latticeValue, space * xIndex - space * 6, space * yIndex + space * 4);
    var rate = (latticeValue * 100 / returnValue.stockInfo.maxPrice).toFixed(2) + "%~" + parseFloat(latticeValue * 100 / returnValue.stockInfo.minPrice).toFixed(2) + "%";

    context.fillText("最高价:" + returnValue.stockInfo.maxPrice + "    格幅:" + rate, space * xIndex - space * 6, space * yIndex + space * 5);
    context.fillStyle = 'red';

    context.fillStyle = 'blue';

    context.restore();
}

function DrawKLine(curList, stockInfo)
{
    //索引偏移量
    var offsetX = 4;
    var offsetY = 0;
    var offsetXL = 10;
    var offsetYL = 3;
    //成交量占据格数
    var volumeIndex = 5;
    //波形图占格
    var waveIndex = 5;
    var dateIndex = 2;
    //minSpace=20;
    //maxSpace = 27;
    space = minSpace;

    var yIndex = stockInfo.endIndex - stockInfo.startIndex + offsetY;
    var xIndex = curList.length + offsetX;
    var spacex = canvasK.width / xIndex;
    var spacey = canvasK.height / yIndex;
    if (spacex > minSpace && spacey > minSpace)
    {
        space = parseInt(spacey > spacex ? spacex : spacey);
        if (space > maxSpace)
        {
            space = maxSpace;
        }
    }
    space = spaceK;
    var spaceX = space / 2;
    canvasK.width = spaceX * (xIndex + offsetXL);
    canvasK.height = space * (yIndex + dateIndex + offsetYL + volumeIndex + waveIndex);
    var offsetXSpace = offsetX * spaceX;
    var offsetYSpace = offsetY * space;


    //抗锯齿 抗模糊
    var width = canvasK.width,
        height = canvasK.height;

    if (window.devicePixelRatio)
    {
        canvasK.style.width = width + "px";
        canvasK.style.height = height + "px";
        canvasK.height = height * devicePixelRatio;
        canvasK.width = width * devicePixelRatio;
        contextK.scale(devicePixelRatio, devicePixelRatio);
    }

    //绘制水平方向的网格线 点数图最低点向上取整,在K线图上补回来
    for (var y = 0; y <= yIndex - offsetY; y++)
    {
        //开启路径
        contextK.beginPath()
        contextK.moveTo(offsetXSpace, space * y + offsetYSpace)
        contextK.lineTo(spaceX * xIndex, space * y + offsetYSpace)
        contextK.stroke()

        //标刻度
        contextK.save();
        contextK.fillStyle = 'red'
        contextK.font = 'bold 10px 微软雅黑'
        contextK.textBaseline = 'right';
        //设置文本的垂直对齐方式
        contextK.textAlign = 'right';
        var str = (Math.round(stockInfo.minPrice, 2) + (yIndex - y - offsetY) * latticeValue).toFixed(2);
        contextK.fillText(str, offsetXSpace - spaceX / 2, y * space + offsetYSpace);
        contextK.textAlign = 'left';

        //最后列刻度后移一格 好看点
        contextK.fillText(str, spaceX * xIndex, y * space + offsetYSpace);
        contextK.restore();
    }
    {
        // contextK.beginPath()
        // contextK.moveTo(offsetXSpace, space * yIndex + space * (dateIndex - 0.7))
        // contextK.lineTo(spaceX * xIndex, space * yIndex + space * (dateIndex - 0.7))
        // contextK.stroke()

        //成交量行
        contextK.beginPath()
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
        contextK.font = 'bold 15px 微软雅黑'
        contextK.textBaseline = 'right';
        //设置文本的垂直对齐方式
        contextK.textAlign = 'left';
        contextK.fillText(" 成交量", spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space / 2);
        contextK.fillText(" 波形图", spaceX * xIndex, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space / 2);
        // code name 
        contextK.fillText(stockInfo.code + " " + stockInfo.name, spaceX * xIndex * 0.9, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space + space * 1.5);
        //波形图行
        contextK.beginPath()
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
            contextK.moveTo(offsetXSpace + x * spaceX, offsetYSpace)
            contextK.lineTo(offsetXSpace + x * spaceX, space * yIndex + space * dateIndex + volumeIndex * space + waveIndex * space)
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
    contextK.font = 'bold 15px 微软雅黑'
    contextK.textBaseline = 'middle';
    //设置文本的垂直对齐方式
    contextK.textAlign = 'center';
    tableK = new Array(curList.length);
    for (var i = 0; i < curList.length; i++)
    {
        contextK.save();
        contextK.lineWidth = 1;

        var item = curList[i];
        var px = (i + 0.5) * spaceX + offsetXSpace;
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var startpy = ((yIndex - parseFloat(item.open) / parseFloat(latticeValue) + stockInfo.startIndex) * space);
        //var endpx = (i + 0.7) * space;
        var endpy = ((yIndex - parseFloat(item.close) / parseFloat(latticeValue) + stockInfo.startIndex) * space);

        var startpyHL = ((yIndex - parseFloat(item.low) / parseFloat(latticeValue) + stockInfo.startIndex) * space);
        var endpyHL = ((yIndex - parseFloat(item.high) / parseFloat(latticeValue) + stockInfo.startIndex) * space);
        if (startpy > endpy)
        {
            contextK.fillStyle = 'red';
            contextK.strokeStyle = 'red';
        }
        else
        {
            contextK.fillStyle = 'blue';
            contextK.strokeStyle = 'blue';
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
                contextK.fillStyle = 'MediumSlateBlue';
                contextK.strokeStyle = 'MediumSlateBlue';
            }
            contextK.lineWidth = lineWidth;
            contextK.fillRect(startpx, endpy, spaceX * 0.8, lineWidth);

        }
        contextK.beginPath()
        contextK.moveTo(px, startpyHL)
        contextK.lineTo(px, endpyHL)
        contextK.stroke()

        if (i % 20 == 0)
        {
            //日期 旋转
            contextK.fillStyle = "black"
            contextK.font = 'bold 12px 微软雅黑';
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
            startpyHL: startpyHL,
            endpyHL: endpyHL,
            endpx: (i * spaceX + offsetXSpace + spaceX),
            space: space,
            spaceX: spaceX,
            offsetXSpace: offsetXSpace
        }
        tableK[i] = obj;
    }

    //绘制成交量图
    var volumeStartpy = space * yIndex + space * dateIndex;
    for (var i = 0; i < curList.length; i++)
    {
        contextK.save();
        var item = curList[i];
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var endpy = volumeStartpy + volumeIndex * space;
        var percent = (item.volume - stockInfo.minVolume) / (stockInfo.maxVolume - stockInfo.minVolume);

        var startpy = endpy - percent * volumeIndex * space;
        var pyheight = endpy - startpy;
        if (item.close > item.open)
        {
            contextK.fillStyle = 'red';
        }
        else
        {
            contextK.fillStyle = 'blue';
        }
        if (isKline)
        {
            contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);

        }
        else
        {
            if (item.close > item.open)
            {
                contextK.fillStyle = 'OrangeRed';
            }
            else
            {
                contextK.fillStyle = 'MediumSlateBlue';
            }
            contextK.fillRect(startpx + 0.399 * spaceX, startpy, lineWidth, pyheight);
        }
        contextK.restore();
    }

    //绘制波形图
    //获取波形图数据
    var waveList = [];
    var waveObj = {
        volume: curList[0].volume,
        isUp: curList[0].close > curList[0].open
    };
    waveList.push(waveObj);
    var minWave = curList[0].volume;
    var maxWave = curList[0].volume;
    for (var i = 1; i < curList.length; i++)
    {
        var wave = curList[i];
        var preWave = waveList[i - 1];
        var isUp = wave.close > wave.open;
        var volume = wave.volume;
        if (preWave.isUp == isUp)
        {
            volume = parseInt(preWave.volume) + parseInt(wave.volume);
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

    //绘制波形图
    for (var i = 1; i < waveList.length; i++)
    {
        contextK.save();
        var wave = waveList[i];
        var startpx = (i + 0.1) * spaceX + offsetXSpace;
        var endpy = waveStartpy - space * 1 / 10 + waveIndex * space;
        var percent = (wave.volume - minWave) / (maxWave - minWave);

        var startpy = endpy - percent * ((waveIndex - 1 / 10) * space);
        var pyheight = endpy - startpy;
        if (wave.isUp)
        {
            contextK.fillStyle = 'OrangeRed';
        }
        else
        {
            contextK.fillStyle = 'MediumSlateBlue';
        }
        if (isKline)
        {
            contextK.fillRect(startpx, startpy, spaceX * 0.8, pyheight);

        }
        else
        {
            contextK.fillRect(startpx + 0.399 * spaceX, startpy, lineWidth, pyheight);
        }
        contextK.restore();
    }
    var titles = document.getElementById('tab-header').getElementsByTagName('li')
    // 遍历
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
            paddingLeft = (parseInt(paddingLeft) - 150) + "px"
            ul.style.paddingLeft = paddingLeft;
            scrollBottomAndRightTag(document.getElementById("right"));
        }
    }
}
//获取价格对应的单格值
function CalLatticeValue(basePrice)
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
        return parseInt(basePrice / 50);
    }
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

function CalculateOneDotGraphic(stockInfo, dataList)
{
    var dotValueList = [];
    dataList.reverse();
    var status = true;
    var curPrice = 0;
    if (stockInfo.isClose)
    {
        status = (dataList[0].close > dataList[0].open);
        curPrice = dataList[0].close;
    }
    else
    {
        var data1 = dataList[0];
        var data2 = dataList[1];
        var increaseRate = data2.high / data1.low - 1;
        var decreaseRate = 1 - data2.low / data1.high;
        status = increaseRate > decreaseRate;
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
            else if (y < preDot.position.y)
            {
                curPrice = stockInfo.isClose ? curData.close : curData.low;
                var y = getGridIndexCurrent(stockInfo, curPrice, false);
                //是否转折

                //大于等于转折格数
                if (preDot.position.y - y >= stockInfo.dotInterval)
                {
                    //换列
                    for (var j = preDot.position.y - 1; j >= y; j--)
                    {
                        var position = {
                            x: preDot.position.x + 1,
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
                    //保持最大值的点在第一位
                    if (dotValueList[0].datas.length > 0)
                    {
                        if (dotValueList[0].datas[0].close < curData.close)
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
            else
            {
                //保持最大值的点在第一位
                if (dotValueList[0].datas.length > 0)
                {
                    if (dotValueList[0].datas[0].close < curData.close)
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
            else if (y > preDot.position.y)
            {
                curPrice = stockInfo.isClose ? curData.close : curData.high;
                var y = getGridIndexCurrent(stockInfo, curPrice, true);
                //是否转折

                //大于等于转折格数
                if (y - preDot.position.y >= stockInfo.dotInterval)
                {
                    //换列
                    for (var j = preDot.position.y + 1; j <= y; j++)
                    {
                        var position = {
                            x: preDot.position.x + 1,
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
                    //保持最大值的点在第一位
                    if (dotValueList[0].datas.length > 0)
                    {
                        if (dotValueList[0].datas[0].close < curData.close)
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
            else
            {
                //保持最大值的点在第一位
                if (dotValueList[0].datas.length > 0)
                {
                    if (dotValueList[0].datas[0].close < curData.close)
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
    var minPrice = result.list[0].low;
    var maxPrice = result.list[0].high;

    var minVolume = result.list[0].volume;
    var maxVolume = result.list[0].volume;
    var dataList = [];
    var length = result.list.length; // > 1000 ? 1000 : result.list.length;
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
    if (latticeValue == 0)
    {
        latticeValue = CalLatticeValue((parseFloat(minPrice) + parseFloat(maxPrice)) / 2);
        var gridNum = parseInt((maxPrice - minPrice) / latticeValue);
        if (gridNum < 5)
        {
            latticeValue = ((maxPrice - minPrice) / 5).toFixed(2);
        }
        document.getElementById("latticeValue").value = latticeValue;
        latticeValue
    }
    //转折格数
    //股票基本信息
    var stockInfo = {
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
    var endIndex = getGridIndexCurrent(stockInfo, maxPrice, false);

    stockInfo.startIndex = startIndex;
    stockInfo.endIndex = endIndex;
    returnValue.stockInfo = stockInfo;
    //复权处理
    /*未完待续*/

    //计算每个点的位置、涨跌状态
    //console.log("数据条数:"+dataList.length)
    var dotValueList = CalculateOneDotGraphic(stockInfo, dataList);
    returnValue.dotValueList = dotValueList;
    //读完后删除js引用
    //var delFile = document.getElementById(code);
    //delFile.parentNode.removeChild(delFile);
    return returnValue;
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
    var stockList = stocklistData;
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
    //}
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
    //loadStockList();
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
            close: data[3],
            high: data[4],
            low: data[5],
            open: data[6],
            lastClose: data[7],
            volume: data[8],
            amount: data[9]
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
    canvas = document.getElementById("dotGraphic");
    context = canvas.getContext("2d");
    canvas.onmousemove = onMouseMoveD;
    canvas.onkeydown = onKeydownD;
    //作画前清理画布
    context.clearRect(0, 0, canvas.width, canvas.height);
    context.strokeStyle = "rgb(0,0,0)";
    context.lineWidth = 1;
    canvas.width = 900;
    canvas.height = 600;

    canvasK = document.getElementById("kline");
    contextK = canvasK.getContext("2d");
    canvasK.onmousemove = onMouseMoveK;

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
    doc.scrollTop = 99999;
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
    var tip = document.getElementById("dotGraphic_tip");
    tip.style.display = 'none';
    var px = e.layerX;
    var py = e.layerY;
    var x = parseInt(px / spaceDot);
    var y = parseInt(py / spaceDot);
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
    var fill = (dotValue.isFill ? "[填]" : "");
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
    var volume = ((dotValue.datas[0].volume) / 1000000).toFixed(2);
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
    tip.style.left = x + 'px';
    tip.style.top = (y + 5) + 'px';

    tip.style.display = 'block';
    //tipHtml="测试"
    //tip.style.height="20px";
    tip.innerHTML = tipHtml;
}

function onMouseMoveK(e)
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

function drawToolTipK(tip, x, y)
{
    var i = parseInt((x - tableK[0].offsetXSpace) / tableK[0].spaceX);
    var dot = tableK[i];
    //是否处于K线范围内
    if (!(y >= dot.endpyHL && y <= dot.startpyHL))
    {
        return;
    }
    var obj = dot.item;
    var volume = ((obj.volume) / 1000000).toFixed(2);
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
        "<br/>涨幅:" + parseFloat((obj.close / obj.lastClose) * 100 - 100).toFixed(2) +
        "%<br/>幅差:" + parseFloat(obj.close - obj.lastClose).toFixed(2) +
        "<br/>振幅:" + parseFloat(obj.high - obj.low).toFixed(2) +
        "<br/>量(万手):" + volume + "<br/>" +
        "额(亿元):" + amount;
    var row = 11;
    tipHtml = tipHtml + "</div";

    var width = 120;
    var height = row * 15;
    tip.style.cssText = 'font-family:Arial,"宋体";font-size:8pt';
    tip.style.position = 'absolute';
    tip.style.zIndex = 4;
    tip.style.backgroundColor = 'white';
    tip.style.border = '1px solid gray';
    tip.style.width = width + 'px';
    tip.style.height = height + 'px';
    tip.style.left = (x + 20) + 'px';
    tip.style.top = (y + 5) + 'px';

    tip.style.display = 'block';
    tip.innerHTML = tipHtml;
}


function generate()
{
    code = document.getElementById("search-input").value;
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
    var obj = stockArray[code];
    name = obj[1];
    type = obj[3];
    DrawOX();
};
//未用
function download()
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

function getUrlContent(method, url)
{
    // 返回一个Promise对象
    return new Promise(function(resolve)
    {
        var xhr = new XMLHttpRequest() // 创建异步请求
        //xhr.timeout = 3000;
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
        xhr.responseType = 'json';
        xhr.send() // 发送异步请求 
    })
}

function getOnlineData()
{
    var scale;
    if (cycle == "日")
    {
        scale = 240;
    }
    else if (cycle == "5分钟")
    {
        scale = 5;
    }
    else if (cycle == "30分钟")
    {
        scale = 30;
    }
    else if (cycle == "60分钟")
    {
        scale = 60;
    }
    var url = "http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=" + type + code + "&scale=" + scale + "&ma=no&datalen=1023";

    return getUrlContent("GET", url).then(function(result)
    {
        if (result == null || result == "")
        {
            alert("未获取到数据,请检查网络连接状况或者浏览器是否允许跨域访问!");
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