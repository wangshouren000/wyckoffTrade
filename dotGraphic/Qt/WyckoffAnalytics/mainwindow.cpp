#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <QtGui/qcolor.h>

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    qTableWidget=new  QTableWidget(ui->centralWidget);

    ui->centralWidget->setStyleSheet("background-color:#FAF9B4");

    qpaintK= new QPaintKLine(ui->scrollArea);
    layoutK = new QGridLayout(ui->tabK);
    //qpaintK加入滚动条控件里面
    ui->scrollArea->setWidget(qpaintK);
    //注意这里的设置可以保证scrollArea的子控件填充满
    layoutK->addWidget(ui->scrollArea);
    ui->scrollArea->setWidgetResizable(false);

    layoutDot= new QGridLayout(ui->tabD);
    layoutDot->addWidget(ui->twDotGraphic);
    InitBasicData();
    setMouseTracking(true);
}

MainWindow::~MainWindow()
{
    delete qTableWidget;
    delete qpaintK;
    delete layoutK;
    delete layoutDot;
    delete ui;
}


void MainWindow::InitBasicData()
{
    //阻塞信号通道
    ui->cmbSource->blockSignals(true);

    ui->cmbSource->addItem("东方财富");
    ui->cmbSource->addItem("新浪");
    //再次打开信号通道
    ui->cmbSource->blockSignals(false);

    ui->cmbCycle->addItem("日");
    ui->cmbCycle->addItem("60分钟");
    ui->cmbCycle->addItem("30分钟");
    ui->cmbCycle->addItem("15分钟");
    ui->cmbCycle->addItem("5分钟");
    ui->cmbCycle->addItem("1分钟");
    ui->cmbCycle->addItem("周");
    ui->cmbCycle->addItem("月");

    QDate end(QDate::currentDate());
    QDate begin(end.addDays(-1200));

    ui->dtbegin->setDate(begin);
    ui->dtend->setDate(end);


    QList<QPaintKLine::CurStock> stockList;
    getStockList(urlStockList,stockList);
    //    QMap<QString, QString> list;
    //    for (int i=0;i<stockList.length();i++)
    //    {
    //        QPaintKLine::CurStock item=stockList[i];
    //        //ui->cmbStock->addItem(item.name,item.code);
    //        list.insert(item.name,item.code);
    //    }
    ui->cmbStock->clear();
    ui->cmbStock->blockSignals(true);
    //    foreach(const QString &str,list.keys())
    //    {
    //        ui->cmbStock->addItem(str,list.value(str));
    //    }
    //setMultipleCombo(stockList);
    InitcmbStock(stockList);
    ui->cmbStock->setCurrentIndex(0);
    ui->cmbStock->blockSignals(false);
    ui->cmbStock->installEventFilter(this);  //在窗体上为cmbStock安装过滤器

    //文本框输入浮点数
    ui->lineEdit->setValidator(new QDoubleValidator());
    ui->linePercentLatticeValue->setValidator(new QDoubleValidator());

    ui->linePercentLatticeValue->setVisible(false);
}

void MainWindow::InitcmbStock(QList<QPaintKLine::CurStock> &stockList)
{
    ui->cmbStock->setEditable(true);

    //隐藏标题栏
    qTableWidget->setWindowFlags(Qt::FramelessWindowHint);
    //不可编辑表格
    qTableWidget->setEditTriggers(QAbstractItemView::NoEditTriggers);
    /*设置表格为整行选中*/
    qTableWidget->setSelectionBehavior(QAbstractItemView::SelectRows);

    /*单行选中*/
    qTableWidget->setSelectionMode(QAbstractItemView::SingleSelection);

    qTableWidget->verticalHeader()->setVisible(false);
    qTableWidget->horizontalHeader()->setVisible(false);

    qTableWidget->setColumnCount(4); //设置列数
    //qTableWidge
    qTableWidget->horizontalHeader()->sectionResizeMode(QHeaderView::Stretch);
    qTableWidget->setGeometry(QRect(40, 108, 400, 500));
    //连接信号，简单讲把qTableWidget的cellClicked事件绑定到当前类的函数qTableWidget_cellClicked
    //这样qTableWidget点击后就可以与当前主界面交互了
    connect(qTableWidget, SIGNAL(cellClicked(int,int)), this, SLOT(qTableWidget_cellClicked(int,int)));
    QList<QPaintKLine::CurStock> myStockList;
    readStockIndexFromcsv(myStockList);
    int myRow=0;
    if(&myStockList==NULL ||myStockList.length()==0)
    {
        qTableWidget->setRowCount(stockList.length());    //设置行数
    }
    else
    {
        myRow=myStockList.length();
        qTableWidget->setRowCount(stockList.length()+myRow);    //设置行数
        for (int i=0;i<myStockList.length();i++)
        {
            QPaintKLine::CurStock item=myStockList[i];
            qTableWidget->setItem(i,0,new QTableWidgetItem(item.code));
            qTableWidget->setItem(i,1,new QTableWidgetItem(item.name));
            qTableWidget->setItem(i,2,new QTableWidgetItem(item.spellCode));
            qTableWidget->setItem(i,3,new QTableWidgetItem(item.type));
        }
    }


    for (int i=0;i<stockList.length();i++)
    {
        QPaintKLine::CurStock item=stockList[i];
        qTableWidget->setItem(i+myRow,0,new QTableWidgetItem(item.code));
        qTableWidget->setItem(i+myRow,1,new QTableWidgetItem(item.name));
        qTableWidget->setItem(i+myRow,2,new QTableWidgetItem(item.spellCode));
        qTableWidget->setItem(i+myRow,3,new QTableWidgetItem(item.type));
    }
    qTableWidget->hide();

}
void MainWindow::readStockIndexFromcsv(QList<QPaintKLine::CurStock> &myStockList)
{

    QFile csvFile(qApp->applicationDirPath() +"/myStock.csv");
    QStringList csvList;
    csvList.clear();
    if(!csvFile.exists())
    {
        QMessageBox::about(NULL, "csv文件", "未能加载自选股列表,请确定根目录下myStock.csv文件是否存在！");
        return;
    }
    if (csvFile.open(QIODevice::ReadOnly)) //对csv文件进行读写操作
    {
        QTextStream stream(&csvFile);
        while (!stream.atEnd())
        {
            csvList.push_back(stream.readLine()); //保存到List当中
        }
        csvFile.close();
    }

    if(&csvList==NULL || csvList.length()==0)
    {
        return;
    }
    else
    {
        for (int i=0;i<csvList.length();i++)
        {
            QList<QString> qlist=csvList[i].split(",");
            if(qlist.length()>3)
            {
                QPaintKLine::CurStock item;
                item.code=qlist[0];
                item.name=qlist[1];
                item.spellCode=qlist[2];
                item.type=qlist[3];
                myStockList.append(item);
            }
        }

    }

}
QString MainWindow::getContentFromUrl(const QString &url)
{
    QUrl qurl(url);
    QNetworkAccessManager manager;
    QEventLoop loop;
    //qDebug() << "Reading code form " << arg1;

    QNetworkReply *reply = manager.get(QNetworkRequest(qurl));  //发送get请求
    connect(reply,&QNetworkReply::finished,&loop,&QEventLoop::quit); //请求结束并下载完成后，退出子事件循环

    //开启子事件循环
    loop.exec();

    //将读到的信息写入文件
    QString result = reply->readAll();
    return result;
}

void MainWindow::getStockList(const QString &url,QList<QPaintKLine::CurStock>& stockList)
{
    QString result=getContentFromUrl(url);
    if(result=="")
    {
        QMessageBox::information(NULL, "warn!!!", "网络连接失败,请检查网络后再试!");
        return;
    }
    int start=result.indexOf("[");
    int end=result.indexOf("]");
    result=result.mid(start+1,end-start-1);

    result=result.replace('"', "");
    result=result.replace("{", "");
    result=result.replace("}", "");
    result=result.replace("f12", "");
    result=result.replace("f13", "");
    result=result.replace("f14", "");
    result=result.replace(":", "");
    QStringList qslist= result.split(",");
    //qDebug() <<qslist1;
    int distance=3;
    for (int i=0;i<qslist.length()/distance;i++)
    {
        QPaintKLine::CurStock item;
        item.code = qslist[i * distance];
        item.type = (qslist[i * distance + 1]=="1"?"sh":"sz");
        item.name = qslist[i * distance + 2];
        item.spellCode = getChineseSpell(item.name);
        stockList.append(item);
    }
}

void MainWindow::getKData(const QString &url,QList<CycleData>& list)
{
    //QList<CycleData> list;
    QString result=getContentFromUrl(url);
    //    QFile file(FILE_NAME);
    //    file.open(QIODevice::WriteOnly);
    //    QTextStream out(&file);
    //    out << code << endl;
    //    file.close();
    //    qDebug() << "Finished, the code have written to " << FILE_NAME;
    //qDebug() <<result;
    //qDebug() <<url;
    if(result=="")
    {
        QMessageBox::information(NULL, "warn!!!", "网络连接失败,请检查网络后再试!");
        return;
    }
    int start=result.indexOf("[");
    int end=result.indexOf("]");
    if(start==-1 ||end ==-1)
    {
        return;
    }
    QString result1=result.mid(start+1,end-start-1);
    QStringList qslist1= result1.split("\",");
    if(qslist1.length()==0)
    {
        return;
    }
    double lastClose;
    for(int i=0;i<qslist1.length();i++)
    {
        QStringList strList=qslist1[i].replace("\"","").split(",");
        //qDebug() <<str;
        CycleData cycleData;
        //日期,开盘价,收盘价,最高价,最低价,前收盘,成交量,成交金额,turnoverRate
        //("2020-08-24", "6.54", "6.55", "6.61", "6.40", "129204", "83730839.00", "0.66")
        if(i==0)
        {
            lastClose=strList[2].toDouble();
        }
        else
        {
            lastClose=(qslist1[i-1].replace("\"","").split(","))[2].toDouble();
        }
        cycleData.code=qpaintK->curStock.code;
        cycleData.name=qpaintK->curStock.name;
        cycleData.date=strList[0];
        cycleData.close=strList[2].toDouble();
        cycleData.high=strList[3].toDouble();
        cycleData.low=strList[4].toDouble();
        cycleData.open=strList[1].toDouble();
        cycleData.lastClose=lastClose;
        cycleData.vol=strList[5].toDouble();
        cycleData.amount=strList[6].toDouble();
        cycleData.turnoverRate=strList[7].toDouble();
        list.append(cycleData);
    }

}

void MainWindow::getMaxMinPriceVol(const QList<CycleData>& list)
{
    qpaintK->curStock.maxVol=list[0].vol;
    qpaintK->curStock.minVol=list[0].vol;
    qpaintK->curStock.maxPrice=list[0].close;
    qpaintK->curStock.minPrice=list[0].close;

    for(int i=0;i<list.length();i++)
    {
        CycleData cycleData=list[i];
        if(cycleData.vol<qpaintK->curStock.minVol)
        {
            qpaintK->curStock.minVol=cycleData.vol;
        }
        if(cycleData.vol>qpaintK->curStock.maxVol)
        {
            qpaintK->curStock.maxVol=cycleData.vol;
        }

        if(cycleData.low<qpaintK->curStock.minPrice)
        {
            qpaintK->curStock.minPrice=cycleData.low;
        }
        if(cycleData.high>qpaintK->curStock.maxPrice)
        {
            qpaintK->curStock.maxPrice=cycleData.high;
        }
    }
}
//获取价格对应的单格值
double MainWindow::CalLatticeValue(double basePrice)
{
    if (ui->chkPercentLattice->checkState()==Qt::Checked)
    {
        if (basePrice < 100)
        {
            return QString::number((basePrice * ui->linePercentLatticeValue->text().toDouble() / 100),'d',1).toDouble();
        }
        else if (basePrice > 100 && basePrice < 1000)
        {
            return (int)(basePrice * ui->linePercentLatticeValue->text().toDouble() / 100);
        }
        else
        {
            return (int)(basePrice * ui->linePercentLatticeValue->text().toDouble() / 1000) * 10;
        }
    }
    else
    {
        int percent = 3;
        if (qpaintK->curStock.cycle == "1分钟")
        {
            percent = 0.1;
        }
        if (qpaintK->curStock.cycle == "5分钟")
        {
            percent = 0.5;
        }
        if (qpaintK->curStock.cycle == "15分钟")
        {
            percent = 1;
        }
        if (qpaintK->curStock.cycle == "30分钟")
        {
            percent = 1.5;
        }
        if (qpaintK->curStock.cycle == "60分钟")
        {
            percent = 2;
        }
        if (qpaintK->curStock.cycle != "日" && qpaintK->curStock.cycle.indexOf("分钟") == -1)
        {
            percent = 7;
        }
        if (basePrice < 100)
        {
            double d=(double(int(basePrice * percent) / 10)+1)/10;
            double result =QString::number(d,'d',2).toDouble();
            if (result == 0)
            {
                result = 0.05;
            }
            return result;
        }
        else if (basePrice > 100 && basePrice < 1000)
        {
            return (int)(basePrice * percent / 100);
        }
        else
        {
            return ((int)(basePrice * percent / 1000)) * 10;
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
int MainWindow::getGridIndexCurrent(double curPrice, bool IsPreUp)
{
    //double newPrice = QString::number((((int)(curPrice / 0.05)) * 0.05),'d',2).toDouble();

    if (IsPreUp)
    {
        return qFloor(curPrice / qpaintK->curStock.latticeValue);
    }
    else
    {
        return qCeil(curPrice / qpaintK->curStock.latticeValue);
    }
}

void MainWindow::CalculateDotGraphic(const QList<CycleData>& list,QList<DotValue> &dotValueList)
{
    bool status = true;
    double curPrice = 0;
    if (qpaintK->curStock.isClose)
    {
        status = list[0].close > list[0].open;
        curPrice = list[0].close;
    }
    else
    {
        CycleData data1 = list[0];
        CycleData data2 = list[1];
        double increaseRate = data2.high / data1.low - 1;
        double decreaseRate = 1 - data2.low / data1.high;
        status = increaseRate > decreaseRate;
        curPrice = status ? data1.high : data1.low;
    }

    //第一个点

    int y0 = getGridIndexCurrent(curPrice, status);

    QList<CycleData> datas0;
    datas0.append(list[0]);
    DotValue dotValue0;
    dotValue0.x=0;
    dotValue0.y=y0;
    dotValue0.datas=datas0;
    dotValue0.isFill=false;
    dotValue0.isUp=status;

    //插入最前面
    dotValueList.prepend(dotValue0);

    for (int i = 1; i < list.length(); i++)
    {

        DotValue preDot = dotValueList[0];
        CycleData curData = list[i];

        //前点涨
        if (preDot.isUp)
        {
            curPrice = qpaintK->curStock.isClose ? curData.close : curData.high;
            int y = getGridIndexCurrent(curPrice, true);
            //当前点也涨
            if (y > preDot.y)
            {
                for (int j = preDot.y + 1; j <= y; j++)
                {
                    int x1=preDot.x;
                    int y1=j;
                    QList<CycleData> datas1;
                    datas1.prepend(curData);
                    DotValue dotValue1;
                    dotValue1.x=x1;
                    dotValue1.y=y1;
                    dotValue1.datas=datas1;
                    dotValue1.isFill=true;
                    dotValue1.isUp=true;

                    //插入最前面
                    dotValueList.prepend(dotValue1);
                }
                //最后一点不是补充点
                dotValueList[0].isFill = false;

            }
            //当前点跌
            else if (y <= preDot.y)
            {
                curPrice = qpaintK->curStock.isClose ? curData.close : curData.low;
                int y = getGridIndexCurrent(curPrice, false);
                //是否转折

                //大于等于转折格数
                if (preDot.y - y >= qpaintK->curStock.dotInterval)
                {
                    //换列
                    int x = preDot.x + 1;
                    //不换列条件
                    bool isNotTurn = isOneDotRebuild && qpaintK->curStock.dotInterval == 1 && (preDot.x >= 1) &&
                            (dotValueList.length() > 2 && dotValueList[0].y <= dotValueList[2].y) &&
                            (y <= dotValueList[1].y) && preDot.x - 1 == dotValueList[1].x;
                    if (isNotTurn)
                    {
                        x = preDot.x;

                    }
                    for (int j = preDot.y - 1; j >= y; j--)
                    {
                        int x1=x;
                        int y1=j;
                        QList<CycleData> datas1;
                        datas1.prepend(curData);
                        DotValue dotValue1;
                        dotValue1.x=x1;
                        dotValue1.y=y1;
                        dotValue1.datas=datas1;
                        dotValue1.isFill=true;
                        dotValue1.isUp=false;
                        //插入最前面
                        dotValueList.prepend(dotValue1);
                    }
                    //最后一点不是补充点
                    dotValueList[0].isFill = false;
                }
                //不够转折 直接加入
                else
                {
                    //保持极值的点在第一位
                    if (dotValueList[0].datas.length() > 0)
                    {
                        if ((qpaintK->curStock.isClose && dotValueList[0].datas[0].close < curData.close) || (!qpaintK->curStock.isClose && dotValueList[0].datas[0].high < curData.high))
                        {
                            dotValueList[0].datas.prepend(curData);
                        }
                        else
                        {
                            dotValueList[0].datas.insert(1, curData);

                        }
                    }
                    else
                    {
                        dotValueList[0].datas.prepend(curData);
                    }
                }
            }
        }
        //前点跌
        else
        {
            curPrice = qpaintK->curStock.isClose ? curData.close : curData.low;
            int y = getGridIndexCurrent(curPrice, false);
            //当前点也跌
            if (y < preDot.y)
            {
                for (int j = preDot.y - 1; j >= y; j--)
                {
                    //插入最前面

                    int x1=preDot.x;
                    int y1=j;
                    QList<CycleData> datas1;
                    datas1.prepend(curData);
                    DotValue dotValue1;
                    dotValue1.x=x1;
                    dotValue1.y=y1;
                    dotValue1.datas=datas1;
                    dotValue1.isFill=true;
                    dotValue1.isUp=false;
                    //插入最前面
                    dotValueList.prepend(dotValue1);
                }
                //最后一点不是补充点
                dotValueList[0].isFill = false;
            }
            //当前点涨
            else if (y >= preDot.y)
            {
                curPrice = qpaintK->curStock.isClose ? curData.close : curData.high;
                int y = getGridIndexCurrent( curPrice, true);
                //是否转折

                //大于等于转折格数
                if (y - preDot.y >= qpaintK->curStock.dotInterval)
                {
                    //换列
                    int x = preDot.x + 1;
                    //不换列条件
                    bool isNotTurn = isOneDotRebuild && qpaintK->curStock.dotInterval == 1 && (preDot.x >= 1) &&
                            (dotValueList.length() > 2 && dotValueList[0].y <= dotValueList[2].y) &&
                            (y <= dotValueList[1].y) && preDot.x - 1 == dotValueList[1].x;
                    if (isNotTurn)
                    {
                        x = preDot.x;

                    }

                    for (int j = preDot.y + 1; j <= y; j++)
                    {
                        int x1=x;
                        int y1=j;
                        QList<CycleData> datas1;
                        datas1.prepend(curData);
                        DotValue dotValue1;
                        dotValue1.x=x1;
                        dotValue1.y=y1;
                        dotValue1.datas=datas1;
                        dotValue1.isFill=true;
                        dotValue1.isUp=true;
                        //插入最前面
                        dotValueList.prepend(dotValue1);
                    }
                    //最后一点不是补充点
                    dotValueList[0].isFill = false;
                }
                //不够转折 直接加入
                else
                {
                    //保持极值的点在第一位
                    if (dotValueList[0].datas.length() > 0)
                    {
                        if ((qpaintK->curStock.isClose && dotValueList[0].datas[0].close > curData.close) || (!qpaintK->curStock.isClose && dotValueList[0].datas[0].low > curData.low))
                        {
                            dotValueList[0].datas.prepend(curData);
                        }
                        else
                        {
                            dotValueList[0].datas.insert(1, curData);
                        }
                    }
                    else
                    {
                        dotValueList[0].datas.prepend(curData);
                    }
                }
            }
        }
    }
}

void MainWindow::DrawKline(QList<CycleData>& list)
{
    //K线图
    //delete layoutK;
    //delete qpaintK;
    //滚动条控件
    //注意这里的设置可以保证scrollArea的子控件显示滚动条
    if(list.length()==0)
    {
        return;
    }
    qpaintK->dataList=list;
    getMaxMinPriceVol(list);
    screnWidthK=list.length()*(qpaintK->klineWith+qpaintK->klineDistance)*qpaintK->scale+qpaintK->offSetWidth;
    screnHeightK=ui->scrollArea->height()-20;

    qpaintK->setObjectName(QString::fromUtf8("qpaintK"));
    qpaintK->resize(screnWidthK,screnHeightK);
    qpaintK->screenWidth=screnWidthK;
    qpaintK->screenHeight=screnHeightK;

    //反转
    if (qpaintK->curStock.latticeValue == 0 || ui->chkPercentLattice->checkState()==Qt::Checked)
    {
        //latticeValue = CalLatticeValue((parseFloat(minPrice) + parseFloat(maxPrice)) / 2);
        qpaintK->curStock.latticeValue = CalLatticeValue(qpaintK->curStock.minPrice+qpaintK->curStock.maxPrice/2);

        ui->lineEdit->setText(QString::number(qpaintK->curStock.latticeValue));
    }
    else
    {
        qpaintK->curStock.latticeValue= ui->lineEdit->text().toDouble();
    }
    //修正最小最大值
    qpaintK->curStock.minPrice=((int)(qpaintK->curStock.minPrice/qpaintK->curStock.latticeValue))*qpaintK->curStock.latticeValue;
    qpaintK->curStock.maxPrice=(((int)(qpaintK->curStock.maxPrice/qpaintK->curStock.latticeValue))+1)*qpaintK->curStock.latticeValue;

    //绘画K线
    qpaintK->update();
    //转折点
    if(ui->radOne->isChecked())
    {
        qpaintK->curStock.dotInterval=1;
    }
    if(ui->radThree->isChecked())
    {
        qpaintK->curStock.dotInterval=3;
    }
    if(ui->radFive->isChecked())
    {
        qpaintK->curStock.dotInterval=5;
    }
}

void MainWindow::DrawDotGraphic(QList<CycleData> &list)
{
    //获取点数图数据
    QList<DotValue> dotValueList;
    CalculateDotGraphic(list, dotValueList);
    int startIndex = getGridIndexCurrent( qpaintK->curStock.minPrice, false);
    //结束值向上取整
    int endIndex = getGridIndexCurrent(qpaintK->curStock.maxPrice, true);

    //绘制点数图
    int rectWidth=20;
    int row=endIndex-startIndex;
    int col=dotValueList[0].x;
    if(row>36)
    {
        rectWidth=10;
    }
    ui->twDotGraphic->clear();
    ui->twDotGraphic->setHorizontalScrollBarPolicy(Qt::ScrollBarAsNeeded);
    ui->twDotGraphic->setVerticalScrollBarPolicy(Qt::ScrollBarAsNeeded);

    ui->twDotGraphic->setEditTriggers(QAbstractItemView::NoEditTriggers);
    //隐藏垂直表头
    ui->twDotGraphic->verticalHeader()->setVisible(false);
    ui->twDotGraphic->horizontalHeader()->setVisible(false);

    ui->twDotGraphic->setRowCount(row+3);    //设置行数 +3是索引要+1,然后前后两列留作坐标
    ui->twDotGraphic->setColumnCount(col+3); //设置列数
    //ui->twDotGraphic->verticalHeader()->setDefaultSectionSize(rectWidth);
    //ui->twDotGraphic->horizontalHeader()->setDefaultSectionSize(rectWidth);
    for (int i=1;i<col+2;i++)
    {
        ui->twDotGraphic->setColumnWidth(i, rectWidth);

    }
    for (int i=0;i<row+3;i++)
    {
        ui->twDotGraphic->setRowHeight(i, rectWidth);
    }
    ui->twDotGraphic->setFont( QFont( "Times", 6 ) );
    for (int i=0;i<dotValueList.length();i++)
    {
        DotValue dotValue=dotValueList[i];
        int x=dotValue.x+1;
        int y=row-(dotValue.y-startIndex)+1;
        QString str="[列"+QString::number(x)+"]"+"\n收盘价："+dotValue.datas[0].code+"\n"
                +"收盘价："+QString::number(dotValue.datas[0].close)+"\n"
                +"最高价："+QString::number(dotValue.datas[0].high)+"\n"
                +"最低价："+QString::number(dotValue.datas[0].low)+"\n"
                +"开盘价："+QString::number(dotValue.datas[0].open)+"\n"
                +"昨收价："+QString::number(dotValue.datas[0].lastClose)+"\n"
                +"成交量："+QString::number(dotValue.datas[0].vol/10000,'d',2)+"万手\n"
                +"成交额："+QString::number(dotValue.datas[0].amount/100000000,'d',2)+"亿元\n数据列表：\n";
        for(int j=0;j<dotValue.datas.length();j++)
        {
            str=str+dotValue.datas[j].date+":"+QString::number(dotValue.datas[j].close)+"\n";
        }
        if(dotValue.isUp)
        {
            QTableWidgetItem *item=new QTableWidgetItem("×");

            item->setData(3,str);
            ui->twDotGraphic->setItem(y,x,item);
            ui->twDotGraphic->item(y,x)->setForeground(QBrush(QColor(Qt::red)));
        }
        else
        {
            QTableWidgetItem *item=new QTableWidgetItem("○");
            item->setData(3,str);
            ui->twDotGraphic->setItem(y,x,item);
            ui->twDotGraphic->item(y,x)->setForeground(QBrush(QColor(Qt::blue)));
        }
        if(dotValue.isFill)
        {
            ui->twDotGraphic->item(y,x)->setBackground(QBrush(QColor(Qt::lightGray)));
        }
        ui->twDotGraphic->item(y,x)->setTextAlignment(Qt::AlignHCenter|Qt::AlignVCenter);
    }
    //坐标
    ui->twDotGraphic->setColumnWidth(0,rectWidth*4);
    ui->twDotGraphic->setColumnWidth(col+2,rectWidth*4);

    ui->twDotGraphic->setFont( QFont( "Times", 10 ) );

    for(int i=0;i<row+3;i++)
    {
        QString str=QString::number(qpaintK->curStock.minPrice+i*qpaintK->curStock.latticeValue);
        QTableWidgetItem *item=new QTableWidgetItem(str);
        QTableWidgetItem *item1=new QTableWidgetItem(str);

        ui->twDotGraphic->setItem(row+3-i-1,0,item);
        ui->twDotGraphic->setItem(row+3-i-1,col+2,item1);
        ui->twDotGraphic->item(row+3-i-1,0)->setForeground(QBrush(QColor(Qt::red)));
        ui->twDotGraphic->item(row+3-i-1,col+2)->setForeground(QBrush(QColor(Qt::red)));

    }
    //滚动到最近的点
    QTableWidgetItem * lastItem = ui->twDotGraphic->item(row-(dotValueList[0].y-startIndex)+1,dotValueList[0].x+2);
    ui->twDotGraphic->scrollToItem(lastItem);
    //cross_H->setGeometry(QRect(ui->centralWidget->width()*0.1, 300, ui->scrollArea->width(), 1));
}

bool MainWindow::eventFilter(QObject *watched, QEvent *event)
{
    if (watched==ui->cmbStock)
    {
        if (event->type()==QEvent::FocusIn)
        {
            ui->cmbStock->setEditText("");
            qTableWidget->show();
        }
        if (event->type()==QEvent::FocusOut)
        {
            qTableWidget->hide();
        }
    }
}

void MainWindow::mouseMoveEvent(QMouseEvent *event)
{
    //cross_H->setGeometry(QRect(ui->centralWidget->width()*0.1, event->pos().y(), ui->scrollArea->width(), 1));
}

void MainWindow::on_cmbSource_currentIndexChanged(const QString &arg1)
{

}

void MainWindow::on_cmbStock_editTextChanged(const QString &arg1)
{
    for(int i=0;i<qTableWidget->rowCount();i++)
    {
        QString code=qTableWidget->item(i,0)->text();
        QString name=qTableWidget->item(i,1)->text();
        QString spellCode=qTableWidget->item(i,2)->text();
        bool isHide=!(code.contains(arg1,Qt::CaseInsensitive) || name.contains(arg1,Qt::CaseInsensitive) || spellCode.contains(arg1,Qt::CaseInsensitive));
        if(isHide)
        {
            qTableWidget->setRowHidden(i,true);
        }
        else
        {
            qTableWidget->setRowHidden(i,false);
        }
    }
    qTableWidget->show();
    ui->cmbStock->setFocusPolicy(Qt::StrongFocus);
}

void MainWindow::qTableWidget_cellClicked(int row,int col)
{
    qpaintK->curStock.code=qTableWidget->item(row,0)->text();
    qpaintK->curStock.name=qTableWidget->item(row,1)->text();
    qpaintK->curStock.type=qTableWidget->item(row,3)->text();
    ui->cmbStock->setEditText(qpaintK->curStock.name);
    qpaintK->curStock.cycle=ui->cmbCycle->currentText();
    qpaintK->curStock.rehabilitation="1";
    qpaintK->curStock.latticeValue=0;
    qTableWidget->hide();
    on_btGenerate_clicked();
}

void MainWindow::keyPressEvent(QKeyEvent  *event)
{
    if(event->key()==Qt::Key_Escape)
    {
        qTableWidget->hide();
    }
}

void MainWindow::on_btGenerate_clicked()
{
    QString source=ui->cmbSource->currentText();
    if(source=="新浪")
    {
        QMessageBox::information(NULL, "warn!!!", source+"数据源暂时未开放");
        return;
    }
    if(qpaintK->curStock.code=="")
    {
        return;
    }
    QString klt="101";
    if (qpaintK->curStock.cycle == "日")
    {
        klt = "101";
    }
    else if (qpaintK->curStock.cycle == "周")
    {
        klt = "102";
    }
    else if (qpaintK->curStock.cycle == "月")
    {
        klt = "103";
    }
    else
    {
        klt = qpaintK->curStock.cycle.replace("分钟", "");
    }
    qpaintK->curStock.cycle=ui->cmbCycle->currentText();
    qpaintK->curStock.isClose=(ui->radClose->isChecked());
    qpaintK->scale=1;
    QString typeNum = (qpaintK->curStock.type=="sh"?"1":"0");
    QString url="http://push2his.eastmoney.com/api/qt/stock/kline/get?cb=jQuery&secid="+
            typeNum+"."+qpaintK->curStock.code+
            "&fields1=f1%2Cf2%2Cf3%2Cf4%2Cf5&fields2=f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf61&"+
            "klt="+klt+
            "&fqt="+qpaintK->curStock.rehabilitation+
            "&beg="+ui->dtbegin->date().toString("yyyyMMdd")+"&end="+ui->dtend->date().toString("yyyyMMdd");

    //抓取K线数据
    QList<CycleData> list;

    getKData(url,list);
    if(list.length()>0)
    {
        kList=list;

        //K线图
        DrawKline(list);
        DrawDotGraphic(list);
    }
    ui->lablinkXQ->setOpenExternalLinks(true);
    QString urlXQ="https://xueqiu.com/S/"+qpaintK->curStock.type.toUpper()+qpaintK->curStock.code;
    ui->lablinkXQ->setText("<a href=\""+urlXQ+"\">雪球");

    ui->lablinkXGB->setOpenExternalLinks(true);
    QString urlXGB="https://xuangubao.cn/stock/"+qpaintK->curStock.code+"."+(qpaintK->curStock.type=="sh"?"SS":"SZ");
    ui->lablinkXGB->setText("<a href=\""+urlXGB+"\">选股宝");

    ui->lablinkDFCF->setOpenExternalLinks(true);
    QString urlDFCF="http://quote.eastmoney.com/"+qpaintK->curStock.type+qpaintK->curStock.code+".html";
    ui->lablinkDFCF->setText("<a href=\""+urlDFCF+"\">东方财富");
}
//滚轮事件
void MainWindow::wheelEvent(QWheelEvent *event)
{
    if(ui->tabWidget->currentIndex()==0)
    {
        if(event->delta() > 0)
        {
            qpaintK->scale=QString::number(qpaintK->scale*1.1,'d',2).toDouble();
            DrawKline(kList);
        }
        else
        {
            qpaintK->scale=QString::number(qpaintK->scale*0.9,'d',2).toDouble();
            DrawKline(kList);
        }
    }
}

void MainWindow::on_cmbCycle_currentIndexChanged(int index)
{
    if(ui->cmbCycle->currentText()=="周")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-5000));
        ui->dtbegin->setDate(begin);
    }
    if(ui->cmbCycle->currentText()=="月")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-10000));
        ui->dtbegin->setDate(begin);
    }
    if(ui->cmbCycle->currentText()=="日")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-1200));
        ui->dtbegin->setDate(begin);
    }
    else if(ui->cmbCycle->currentText().indexOf("分钟")>0)
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-7));
        ui->dtbegin->setDate(begin);
    }
    ui->lineEdit->setText("");
    qpaintK->curStock.latticeValue=0;
    qpaintK->curStock.cycle=ui->cmbCycle->currentText();
    on_btGenerate_clicked();
}

void MainWindow::on_chkPercentLattice_stateChanged(int arg1)
{
    if(ui->chkPercentLattice->checkState()==Qt::Checked)
    {
        ui->linePercentLatticeValue->setVisible(true);
        ui->linePercentLatticeValue->setText("5");
    }
    else
    {
        ui->linePercentLatticeValue->setVisible(false);
    }
}

void MainWindow::on_twDotGraphic_cellEntered(int row, int column)
{
    QToolTip::hideText();

    if(!(ui->twDotGraphic->item(row,column) == NULL))
    {
        QString str=ui->twDotGraphic->item(row,column)->data(3).toString();
        QToolTip::showText(QCursor::pos(),str);
    }
}

void MainWindow::on_btAddMyStock_clicked()
{
    QList<QPaintKLine::CurStock>  myStockList;
    readStockIndexFromcsv(myStockList);
    for (int i=0;i<myStockList.length();i++)
    {
        QPaintKLine::CurStock item=myStockList[i];
        if(item.code==qpaintK->curStock.code)
        {
            QMessageBox::information(NULL, "warn!!!", "已存在股票,请不要重复添加！");
            return;
        }
    }
    QFile outFile(qApp->applicationDirPath() +"/myStock.csv");
    QString line=qpaintK->curStock.code+","+qpaintK->curStock.name+","+getChineseSpell(qpaintK->curStock.name)+","+qpaintK->curStock.type;
    QStringList CSVList;
    if (outFile.open(QIODevice::ReadOnly))
    {
        QTextStream stream(&outFile);
        while (!stream.atEnd())
        {
            CSVList.append(stream.readLine());
        }
        outFile.close();
    }
    if (outFile.open(QIODevice::WriteOnly))
    {
        for (int i=0;i<CSVList.length();i++)
        {
            outFile.write((CSVList[i]+"\n").toStdString().c_str());
            if((CSVList.length()<3 && i==CSVList.length()-1)|| i==2 )
            {
                outFile.write((line+"\n").toStdString().c_str());
            }
        }
        outFile.close();
    }
    QMessageBox::information(NULL, "warn!!!", "自选股添加成功！");

    qTableWidget->clear();
    QList<QPaintKLine::CurStock> stockList;
    getStockList(urlStockList,stockList);
    InitcmbStock(stockList);
}

QString MainWindow::getChineseSpell(QString& src)
{
    QTextCodec *codec4gbk = QTextCodec::codecForName("GBK"); //获取qt提供的gbk的解码器
    QByteArray buf = codec4gbk->fromUnicode(src); //qt用的unicode，转成gbk
    int size = buf.size();
    quint16 *array = new quint16[size+1];
    QString alphbats;

    for( int i = 0, j = 0; i < buf.size(); i++, j++ )
    {
        if( (quint8)buf[i] < 0x80 ) //gbk的第一个字节都大于0x81，所以小于0x80的都是符号，字母，数字etc
            continue;
        array[j] = (((quint8)buf[i]) << 8) + (quint8)buf[i+1]; //计算gbk编码
        i++;
        alphbats.append( ConvertSpell( array[j] ) ); //相当于查表，用gbk编码得到首拼音字母
    }
    delete [] array;
    return alphbats;
}

char MainWindow::ConvertSpell(int n)
{
    if (In(0xB0A1,0xB0C4,n)) return 'a';
    if (In(0XB0C5,0XB2C0,n)) return 'b';
    if (In(0xB2C1,0xB4ED,n)) return 'c';
    if (In(0xB4EE,0xB6E9,n)) return 'd';
    if (In(0xB6EA,0xB7A1,n)) return 'e';
    if (In(0xB7A2,0xB8c0,n)) return 'f';
    if (In(0xB8C1,0xB9FD,n)) return 'g';
    if (In(0xB9FE,0xBBF6,n)) return 'h';
    if (In(0xBBF7,0xBFA5,n)) return 'j';
    if (In(0xBFA6,0xC0AB,n)) return 'k';
    if (In(0xC0AC,0xC2E7,n)) return 'l';
    if (In(0xC2E8,0xC4C2,n)) return 'm';
    if (In(0xC4C3,0xC5B5,n)) return 'n';
    if (In(0xC5B6,0xC5BD,n)) return 'o';
    if (In(0xC5BE,0xC6D9,n)) return 'p';
    if (In(0xC6DA,0xC8BA,n)) return 'q';
    if (In(0xC8BB,0xC8F5,n)) return 'r';
    if (In(0xC8F6,0xCBF0,n)) return 's';
    if (In(0xCBFA,0xCDD9,n)) return 't';
    if (In(0xCDDA,0xCEF3,n)) return 'w';
    if (In(0xCEF4,0xD188,n)) return 'x';
    if (In(0xD1B9,0xD4D0,n)) return 'y';
    if (In(0xD4D1,0xD7F9,n)) return 'z';
    return '\0';
}

bool MainWindow::In(wchar_t start, wchar_t end, wchar_t code)
{
    if (code >= start && code <= end)
    {
        return true;
    }
    return false;
}
