#!/bin/bash
# 功能：大涨小跌,即前期大涨，后期小跌
#      1、在确定的周期范围内(大周期-小周期)，涨幅达到大涨
#      2、在确定的小周期范围内，涨幅达到小跌
#      其中：
#           cycle ：          周期，如day,week,month
#           big_cycle_num ：  确定的大周期范围
#           small_cycle_num ：确定的小周期范围
#           big_rate ：       大涨的值
#           small_rate ：     小跌的值
if [ $# != 5 ] ; then
    echo "参数个数不对"
    exit 444;
else
    cycle=$1
    big_cycle_num=$2
    small_cycle_num=$3
    big_rate=$4
    small_rate=$5

    result=""
    for line in $(cat stocklist.csv)
    do
        arr=(${line//,/ })
        code=${arr[0]}
        name=${arr[1]}
        type=${arr[2]}
        filepath="./data/eastmoney/$cycle/$type.$code.csv"
       if [ ! -f "$filepath" ]; then
            continue
       fi
        if [[ $code == 300* ]];then
            continue
        fi
        if [[ $code == 688* ]];then
            continue
        fi

        row=$(cat $filepath |wc -l)
        if [ $row -lt $cycle_num ];then
            continue
        fi
        p_arr=$big_cycle_num","$small_cycle_num","$big_rate","$small_rate
        awk  -v in_param=$p_arr 'BEGIN{
            minPrice=9999999999;
            maxPrice=0;
            split(in_param,arr," ");
            big_cycle_num=arr[1];
            small_cycle_num=arr[2];
            big_rate=arr[3];
            small_rate=arr[4];
        }
        {
            if(NR>=p_row-p_cycle_num && NR<p_row)
            {
                split($0,arr_b,",");
                high=arr_b[5];
                low=arr_b[6];

                if(high > maxPrice)
                {
                    maxPrice=high;
                }
                if(minPrice > low)
                {
                    minPrice=low;
                }
            }
        }
        END{
            split($0,lastCycle,",");
            rate_1=maxPrice/minPrice;
            rate_2=1+p_rate/100;
            if(rate_2 > rate_1)
            {
                if(lastCycle[5] >= maxPrice)
                {
                    print lastCycle[1]","lastCycle[2];
                }
            }
        }' $filepath   
    done    
fi