/* 
功能：长期横盘突破
      1、在确定的周期范围内，最后一周期的最高点高于此范围内的高点(不包含最后一周期)
      2、而且在确定的周期范围内，最高点和最低点的振幅比例小于确定的振幅范围
      其中params：
           cycle_num: 确定的周期范围
           rate ：    确定的振幅范围
*/
function calculate(params,list)
{
    var paramList=params.split('\n');
    var dataList = list.split('\n');
    
    var cycle_num=paramList[0];
    var rate=paramList[1];
    var code=paramList[paramList.length-2];
    var name=paramList[paramList.length-1];
    var result=""
    
    var minPrice=9999999999;
    var maxPrice=0;

    for (var i =dataList.length-1-cycle_num;i<dataList.length-2;i++)
    {
        var item=dataList[i].split(',');
        var high=parseFloat(item[4]);
        var low=parseFloat(item[5]);
        if(high > maxPrice)
        {
            maxPrice=high;
        }
        if(minPrice > low)
        {
            minPrice=low;
        }
    }
    var rate_1=parseFloat((maxPrice/minPrice).toFixed(2));
    var rate_2=parseFloat((1+rate/100).toFixed(2));
    var lastItem=dataList[dataList.length-2].split(',');
    var lastHigh=lastItem[4];
    //横盘振幅范围
    if(rate_2 > rate_1)
    {
        //本周期破横盘高点
        if(parseFloat(lastHigh) >= maxPrice)
        {
            result=code+","+name;
        }
    }
    //console.log(lastItem);
    return result;
}