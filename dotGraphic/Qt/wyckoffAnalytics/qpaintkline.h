#ifndef QPAINTKLINE_H
#define QPAINTKLINE_H

#include <QWidget>
#include <QPainter>
#include <QToolTip>
#include <QMap>
#include <QMouseEvent>
#include <QDate>

#include "cycledata.h"

class QPaintKLine : public QWidget
{
    Q_OBJECT
public:
    explicit QPaintKLine(QWidget *parent = nullptr);

signals:

public slots:
public:
    QList<CycleData> dataList;//k线数据
    struct CurStock
    {
        QString code;
        QString name;
        QString type;
        double maxPrice;
        double minPrice;
        double maxVol;
        double minVol;
        QString cycle;
        QString rehabilitation;
        QString spellCode;
        double latticeValue=0;//单格对应价格
        bool isClose=false;//收盘价还是高低价
        int dotInterval=1;//转折格数 1 3 5
    };
    CurStock curStock;
    int screenHeight=1000;//屏幕高度
    int screenWidth=2000;
    int offSetWidth=100;//宽度偏移量
    int offSetHeight=100;//高度偏移量

    QColor  background; //背景颜色
    int klineWith=15;//K线宽度
    int klineDistance=6;//K线间距
    double scale=1;//缩放倍数
    struct KPosition
    {
        int beginX;
        int beginY;
        int endY;
        CycleData data;
    };
    struct DotValue
    {
        int x;
        int y;
        QList<CycleData> datas;
        bool isFill;
        bool isUp;
    };
    QList<DotValue> dotValueList;

    QMap<int,KPosition> positionMap;//添加K线信息到map 用于悬停显示
    QMap<QString,int> lowHighPoint;
    bool isConnectHighLow=false;
protected:
    //绘图事件
    void paintEvent(QPaintEvent *event);
    void mouseMoveEvent(QMouseEvent *event);//鼠标进入的时候发送信号显示
    //画背景
    void drawBg(QPainter *painter);
    //画刻度
    void drawScaleLine(QPainter *painter);

private:

};

#endif // QPAINTKLINE_H
