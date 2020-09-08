#!/bin/bash
# 功能：
#type="sz"
#strsh="60"
#spell=$(hanz2piny --s ${arr[1]})
#spell=$(echo $spell|sed 's/\([a-z]*[1-9]*\)//g')
#if [[ "$code" =~ ^$strsh.* ]] ;then
#    type="sh"
#fi
if [[  "$1" != "" ]] ;then
    str=$(cat stock_list.csv|grep "$1")
    arr=(${str//,/ })
    replaceStr="\n[\"${arr[0]}\",\"${arr[1]}\",\"${arr[2]}\",\"${arr[3]}\",\"$2\"],"
    sed -i 's/=\[/=\['"$replaceStr"'/g' common_stock_list.js
else
    echo "请输入股票代码"
fi