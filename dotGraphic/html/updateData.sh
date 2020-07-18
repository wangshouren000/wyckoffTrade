#!/bin/bash
# 功能：
source /opt/shell/progressbar.sh
cd dataDay
i=1
echo "开始更新股票数据，数据源来自http://quotes.money.163.com/service/chddata.html"
sum=$(cat stock_list.csv |wc -l)
for line in $(cat stock_list.csv)
do
    ProgressBar $i $sum
    arr=(${line//,/ })
    code=${arr[0]}
    type=${arr[3]}
    #最近的一条数据
    lastDate=""
    if [ ! -f "$code.csv" ]; then
        lastDate="1990-01-01"
        touch $code.csv
    else
        str=$(head -n 1 $code.csv)
        strArr=(${str//,/ })
        lastDate=${strArr[0]}
    fi
    lastDate=${lastDate//-/}
    startDate=$(date -d "$lastDate +1 days" "+%Y%m%d")
    endDate=$(date -d now "+%Y%m%d")
    yesterday=$(date -d "$endDate -1 days" "+%Y%m%d")
    let i=i+1
    if [[ "$startDate" == "$endDate" ||  "$startDate" == "$yesterday"  ]];then
        continue
    fi
    
    if [ "$type" == "sh" ];then
        type="0"
    else
        type="1"
    fi
    
    newcode=$type$code
    url="http://quotes.money.163.com/service/chddata.html?code=$newcode&start=$startDate&end=$endDate&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;VOTURNOVER;VATURNOVER"
    wget -O temp.csv -q  $url

    # 替换 ,' None

    sed -i "s/,'/,/g" temp.csv
    sed -i "s/None/0/g" temp.csv
    #编码转换
    iconv -f GBK -t utf-8 temp.csv >temp1.csv
    #删除第一行
    sed -i '1d' temp1.csv
    cat temp1.csv $code.csv>temp2.csv
    rm $code.csv
    mv temp2.csv $code.csv
done
echo "结束数据更新!"