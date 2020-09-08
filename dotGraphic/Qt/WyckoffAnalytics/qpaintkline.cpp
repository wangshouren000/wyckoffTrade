#include "qpaintkline.h"

QPaintKLine::QPaintKLine(QWidget *parent) : QWidget(parent)
{
    background = QColor("#FAF9B4");
    setMouseTracking (true);//启动鼠标??否则mouseMoveEvent不起作用???
}
/*
K线图
*/
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
    int realscreenHeight=screenHeight-offSetHeight;
    painter.setPen(QPen(Qt::black, 1));

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
            heightOfK=1;
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

        painter.fillRect(rect, painter.brush());
        //添加K线信息到map 用于悬停显示
        KPosition kp;
        kp.data=cylcleData;
        kp.beginX=beginX;
        kp.beginY=highY;
        kp.endY=lowY;
        positionMap.insert(i,kp);

        //绘制成交量
        int volHeight=(int)offSetHeight*(cylcleData.vol-curStock.minVol)/(curStock.maxVol-curStock.minVol);
        int beginVY=screenHeight-volHeight;
        QRect rectVol(beginX, beginVY, klineWith*scale, volHeight);
        painter.fillRect(rectVol, painter.brush());
    }
    //划线
    //不知道为什么paintEvent总是被调用，而有时curStock.latticeValue=0未被赋值
    //会死循环，先这么判断
    if(curStock.latticeValue>0)
    {
        double price=curStock.minPrice;
        while(price<=curStock.maxPrice)
        {
            painter.setPen(QPen(Qt::black, 1));
            int linY=realscreenHeight-realscreenHeight*(price-curStock.minPrice)/diffP;
            //int rowHeight=realscreenHeight*curStock.latticeValue/diffP;
            painter.drawLine(0, linY,realscreenWidth,linY);
            QPointF qpf(realscreenWidth+10,linY);
            painter.setPen(QPen(Qt::red, 1));
            painter.setFont(QFont( "Times", 10 ));
            painter.drawText(qpf,QString::number(price,'d',2));
            price+=curStock.latticeValue;
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
                "量："+QString::number(kp.data.vol/10000,'d',2)+"万手\n"+
                "换手："+QString::number(kp.data.turnoverRate)+"%\n"+
                "额："+ QString::number(kp.data.amount/100000000,'d',2)+"亿元\n";
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
