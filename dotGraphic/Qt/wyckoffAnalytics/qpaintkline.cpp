#include "qpaintkline.h"

QPaintKLine::QPaintKLine(QWidget *parent) : QWidget(parent)
{
    background = QColor("#FAF9B4");
    setMouseTracking (true);//启动鼠标??否则mouseMoveEvent不起作用???
}
/*K线图*/
void QPaintKLine::paintEvent(QPaintEvent *event)
{
    QPainter painter(this);
    const QRect & rect = this->rect();
    painter.eraseRect(rect);
    painter.setRenderHints(QPainter::Antialiasing);
    //绘制背景
    drawBg(&painter);

    //单位价格对应Y轴长度
    double diffP=curStock.maxPrice-curStock.minPrice;
    int realscreenWidth=screenWidth-offSetWidth;
    int realscreenHeight=screenHeight-offSetHeight*1;
    painter.setPen(QPen(Qt::black, 1));
    //绘制K线图、成交量
    for (int i=0;i<dataList.length();i++)
    {
        CycleData cylcleData=dataList[i];
        CycleData preCylcleData=dataList[i];
        if(i>0)
        {
            preCylcleData=dataList[i-1];
        }
        bool isUp=(cylcleData.open<cylcleData.close);
        int beginX=(klineWith+klineDistance)*scale*i+klineDistance*scale;
        int highY=realscreenHeight-realscreenHeight*(cylcleData.high-curStock.minPrice)/diffP;
        int lowY=realscreenHeight-realscreenHeight*(cylcleData.low-curStock.minPrice)/diffP;
        int openY=realscreenHeight-realscreenHeight*(cylcleData.open-curStock.minPrice)/diffP;
        int closeY=realscreenHeight-realscreenHeight*(cylcleData.close-curStock.minPrice)/diffP;
        int heightOfK=realscreenHeight*(cylcleData.open-cylcleData.close)/diffP;
        if(heightOfK==0)
        {
            heightOfK=3;
            if(cylcleData.close>preCylcleData.close)
            {
                isUp=true;
            }
        }
        //竖线
        painter.drawLine(beginX+((int)klineWith*scale/2),lowY, beginX+((int)klineWith*scale/2),highY);
        QRect rect(beginX, isUp?closeY:openY, ((int)klineWith*scale), qAbs(heightOfK));
        if(isUp)
        {
            QColor red("#ff4500");
            painter.setBrush(red);
        }
        else
        {
            QColor blue("#6a5acd");
            painter.setBrush(blue);
        }
        //绘制纵向分隔线
        QStringList strDateList=cylcleData.date.mid(0,10).split('-');
        if(strDateList.length()==3 && curStock.cycle=="day" && scale>0.3)
        {
            QDate dt(strDateList[0].toInt(),strDateList[1].toInt(),strDateList[2].toInt());
            int dayOfWeek = dt.dayOfWeek();
            //每个周一
            if(dayOfWeek==1)
            {
                painter.drawLine(beginX-((int)(klineDistance/2)*scale),screenHeight, beginX-((int)(klineDistance/2)*scale),0);
            }
        }
        //今天
        if( i==(dataList.length()-1)&& curStock.cycle=="day")
        {
            painter.drawLine(beginX+((int)(klineWith+klineDistance/2)*scale),screenHeight, beginX+((int)(klineWith+klineDistance/2)*scale),0);

            QPointF qpf(realscreenWidth-40,lowY+100);
            int fontSize=10;
            painter.setFont(QFont( "Times",  fontSize,QFont::Bold));
            painter.setPen(QPen(Qt::black, 1));
            QStringList strDateList=dataList[i].date.mid(0,10).split('-');
            QDate dt(strDateList[0].toInt(),strDateList[1].toInt(),strDateList[2].toInt());
            int dayOfWeek = dt.weekNumber();
            painter.drawText(qpf,"第"+QString::number(dayOfWeek)+"周");
        }


        painter.fillRect(rect, painter.brush());
        //添加K线信息到map 用于悬停显示
        KPosition kp;
        kp.data=cylcleData;
        kp.beginX=beginX;
        kp.beginY=highY;
        kp.endY=lowY;
        positionMap.insert(i,kp);
        lowHighPoint.insert(cylcleData.date,i);
        //绘制成交量
        int volHeight=(int)offSetHeight*(cylcleData.vol-curStock.minVol)/(curStock.maxVol-curStock.minVol);
        int beginVY=screenHeight-volHeight;
        QRect rectVol(beginX, beginVY, klineWith*scale, volHeight);
        painter.fillRect(rectVol, painter.brush());

        //        //绘制维斯波成交量
        //        int volHeightW=(int)offSetHeight*(cylcleData.vol-curStock.minVol)/(curStock.maxVol-curStock.minVol);
        //        int beginVYW=screenHeight-volHeight;
        //        int beginXW=(klineWith+klineDistance)*scale*i+klineDistance*scale;
        //        if(isUp)
        //        {
        //            QColor red("#ff4540");
        //            painter.setBrush(red);
        //        }
        //        else
        //        {
        //            QColor blue("#6a5aff");
        //            painter.setBrush(blue);
        //        }
        //        QRect rectVolW(beginXW, beginVYW, klineWith*scale, volHeightW);
        //        painter.fillRect(rectVolW, painter.brush());
    }
    //绘制水平线
    //不知道为什么paintEvent总是被调用，而有时curStock.latticeValue=0未被赋值
    //会死循环，先这么判断
    if(curStock.latticeValue>0)
    {
        double price=curStock.minPrice;
        while(price<=curStock.maxPrice)
        {
            painter.setPen(QPen(Qt::black, 1));
            int linY=realscreenHeight-realscreenHeight*(price-curStock.minPrice)/diffP;
            QPointF qpf(realscreenWidth+10,linY);
            int fontSize=(12*scale<8?8:12*scale);
            fontSize=fontSize>12?12:fontSize;
            int lineSpace=realscreenHeight*(curStock.latticeValue)/diffP;
            if(lineSpace<10)
            {
                fontSize=5;
            }
            painter.setFont(QFont( "Times",  fontSize));
            painter.drawLine(0, linY,realscreenWidth,linY);
            painter.setPen(QPen(Qt::red, 1));
            painter.drawText(qpf,QString::number(price,'d',2));
            price+=curStock.latticeValue;
        }
    }

    //把点数图每一列合计连接高低点
    if(dotValueList.length()>0 && isConnectHighLow)
    {
        QList<QList<DotValue>> dotValueColList;
        QList<DotValue> list0;
        dotValueColList.append(list0);
        bool status=dotValueList[0].isUp;
        //qDebug()<<dotValueList[0].datas[0].date;
        for (int i=0;i<dotValueList.length();i++)
        {
            DotValue dot=dotValueList[i];
            if(status==dot.isUp)
            {
                dotValueColList[dotValueColList.length()-1].append(dot);
            }
            else
            {
                status=!status;
                QList<DotValue> list;
                dotValueColList.append(list);
                dotValueColList[dotValueColList.length()-1].append(dot);
            }
        }
        DotValue start=dotValueColList[dotValueColList.length()-1][0];
        DotValue end=start;
        painter.setPen(QPen(Qt::black, 1));

        for (int i=dotValueColList.length()-2;i>=0;i--)
        {
            QList<DotValue> list=dotValueColList[i];
            end=dotValueColList[i][0];

            KPosition kp1=positionMap[lowHighPoint[start.datas[0].date]];
            KPosition kp2=positionMap[lowHighPoint[end.datas[0].date]];
            bool isUp=start.isUp;
            painter.drawLine(kp1.beginX+((int)klineWith*scale/2), isUp?kp1.beginY:kp1.endY,kp2.beginX+((int)klineWith*scale/2),isUp?kp2.endY:kp2.beginY);
            start=dotValueColList[i][0];
        }
    }
}

void QPaintKLine::mouseMoveEvent(QMouseEvent *event)
{
    //QToolTip::hideText();
    int x=event->x();
    int y=event->y();

    int key=(int)((x-klineDistance*scale)/((klineWith+klineDistance)*scale));
    KPosition kp=positionMap[key];
    if(x>=kp.beginX && x<=kp.beginX+klineWith*scale && y>=kp.beginY && y<=kp.endY)
    {
        QString rate=QString::number((kp.data.close / (kp.data.lastClose == 0 ? kp.data.open : kp.data.lastClose)) * 100 - 100,'d',2);
        QString str="日期："+kp.data.date+"\n"+
                "收盘："+QString::number(kp.data.close)+"\n"+
                "最高："+QString::number(kp.data.high)+"\n"+
                "最低："+QString::number(kp.data.low)+"\n"+
                "昨收："+QString::number(kp.data.lastClose)+"\n"+
                "开盘："+QString::number(kp.data.open)+"\n"+
                "涨幅："+rate+"%\n"+
                "涨差："+QString::number(kp.data.close-kp.data.lastClose,'d',2)+"\n"+
                "振幅："+QString::number((kp.data.high - kp.data.low) / kp.data.low * 100,'d',2)+"%\n"+
                "振差："+QString::number((kp.data.high - kp.data.low),'d',2)+"\n"+
                "量： "+QString::number(kp.data.vol/10000,'d',2)+"万手\n"+
                "换手："+QString::number(kp.data.turnoverRate)+"%\n"+
                "额： "+ QString::number(kp.data.amount/100000000,'d',2)+"亿元\n";
        QToolTip::showText(QCursor::pos(),str);
    }
    else
    {
        if(curStock.code!="")
        {
            double diffP=curStock.maxPrice-curStock.minPrice;
            int realscreenHeight=screenHeight-offSetHeight;
            double price=(realscreenHeight-y)*diffP/realscreenHeight+curStock.minPrice;
            if(price>curStock.minPrice)
            {
                QToolTip::showText(QCursor::pos(),"价格线:"+QString::number(price,'d',2));
            }
        }
    }
}

void QPaintKLine::drawBg(QPainter *painter)
{
    painter->fillRect(this->rect(), background);
}
