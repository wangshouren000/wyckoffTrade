#!/bin/bash
# 功能：长期横盘突破
#      1、在确定的周期范围内，最后一周期的最高点高于此范围内的高点(不包含最后一周期)
#      2、而且在确定的周期范围内，最高点和最低点的振幅比例小于确定的振幅范围
#      其中：
#           cycle：    周期，如day,week,month
#           cycle_num: 确定的周期范围
#           rate ：    确定的振幅范围
if [ $# != 3 ] ; then
    echo "参数个数不对"
    exit 444;
else
    cycle=$1
    cycle_num=$2
    rate=$3
    

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
        p_arr=$cycle_num","$rate","$row","$cycle_num
        awk  -v in_param=$p_arr 'BEGIN{
            minPrice=9999999999;
            maxPrice=0;
            split(in_param,arr,",");
            p_cycle_num=arr[1];
            p_rate=arr[2];
            p_row=arr[3];
            p_cycle_num=arr[4];
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