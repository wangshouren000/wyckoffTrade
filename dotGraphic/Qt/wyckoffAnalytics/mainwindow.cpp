#include "mainwindow.h"
#include "ui_mainwindow.h"
#include <QTimer>
#include <QtGui/qcolor.h>

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    this->setWindowState(Qt::WindowMaximized);
    //    const QSize MAIN_SIZE_MAX = QSize(16777215, 16777215);
    //    this->setMaximumSize(MAIN_SIZE_MAX);
    //    this->setWindowFlag(Qt::WindowMaximizeButtonHint, true);
    setMouseTracking(true);

    qTableWidget=new  QTableWidget(ui->centralWidget);
    ui->centralWidget->setStyleSheet("background-color:#FAF9B4");

    qpaintK= new QPaintKLine(ui->scrollArea);
    layoutK = new QGridLayout(ui->tabK);
    //qpaintK加入滚动条控件里面
    ui->scrollArea->setWidget(qpaintK);
    qpaintK->setWindowState(Qt::WindowMaximized);
    //注意这里的设置可以保证scrollArea的子控件填充满
    layoutK->addWidget(ui->scrollArea);
    ui->scrollArea->setWidgetResizable(false);

    layoutDot= new QGridLayout(ui->tabD);
    layoutDot->addWidget(ui->twDotGraphic);
    InitBasicData();


    QTimer *timer = new QTimer(this);

    //将定时器超时信号与槽(功能函数)联系起来

    connect( timer,SIGNAL(timeout()), this, SLOT(reFresh()) );
    timer->start(60000);
}

MainWindow::~MainWindow()
{
    delete qTableWidget;
    delete qpaintK;
    delete layoutK;
    delete layoutDot;
    delete ui;
}

void MainWindow::Load()
{
    //on_btChoose_clicked();
    ui->cmbChoose->setCurrentIndex(-1);
    ui->cmbMyStock->setCurrentIndex(3);
    //ui->tabWidget->setCurrentIndex(0);
}
void MainWindow::reFresh()
{
    //qDebug()<<"fresh";
    QDate now(QDate::currentDate());
    QTime time(QTime::currentTime());
    QTime begintime(9,30);
    QTime endtime(15,00);
    bool isAutoRefresh=(ui->chkAutoRefesh->checkState()==Qt::Checked);
    if(isAutoRefresh && now.dayOfWeek()>0 && now.dayOfWeek()<6 && time>=begintime && time<=endtime)
    {
        on_btGenerate_clicked();
    }
}
void MainWindow::InitBasicData()
{
    ui->cmbSource->addItem("东方财富");

    ui->cmbCycle->addItem("month");
    ui->cmbCycle->addItem("week");
    ui->cmbCycle->addItem("day");
    ui->cmbCycle->addItem("120min");
    ui->cmbCycle->addItem("60min");
    ui->cmbCycle->addItem("30min");
    ui->cmbCycle->addItem("15min");
    ui->cmbCycle->addItem("5min");
    ui->cmbCycle->addItem("1min");
    ui->cmbCycle->setCurrentIndex(2);

    QDate end(QDate::currentDate());
    QDate begin(end.addDays(-1200));

    ui->dtbegin->setDate(begin);
    ui->dtend->setDate(end);

    //QList<QPaintKLine::CurStock> stockList;
    getStockList(urlStockList,stockList);

    InitStockList(stockList);
    ui->lineEditStock->installEventFilter(this);

    //文本框输入浮点数
    ui->lineEdit->setValidator(new QDoubleValidator());
    ui->linePercentLatticeValue->setValidator(new QDoubleValidator());

    ui->linePercentLatticeValue->setVisible(false);

    ui->label_5->setStyleSheet("color:red;");

    connect(ui->labZJS, SIGNAL(linkActivated(QString)), this, SLOT(openZJSURL()));

    //加载公式列表
    QFile csvFile(qApp->applicationDirPath() +"/formula.csv");
    QStringList csvList;
    csvList.clear();
    if(!csvFile.exists())
    {
        QMessageBox::about(this, "csv文件", "未能加载公式列表，请检查formula.csv文件是否存在！");
        return;
    }
    if (csvFile.open(QIODevice::ReadOnly)) //对csv文件进行读写操作
    {
        QTextStream stream(&csvFile);
        while (!stream.atEnd())
        {
            csvList.push_back(stream.readLine()); //保存到List当中
        }

    }
    csvFile.close();
    if(csvList.length()==0)
    {
        return;
    }
    else
    {
        ui->cmbFormula->blockSignals(true);
        ui->cmbFormula->clear();

        for (int i=0;i<csvList.length();i++)
        {
            QList<QString> qlist=csvList[i].split(",");
            ui->cmbFormula->addItem(qlist[0]);
            ui->cmbFormula->setItemData(i,qlist[1]);
            mapFormula.insert(i,csvList[i]);
        }
        ui->cmbFormula->blockSignals(false);
        ui->cmbFormula->setCurrentIndex(0);
        on_cmbFormula_currentIndexChanged(0);
    }
}

void MainWindow::InitStockList(QList<QPaintKLine::CurStock> &stockList)
{
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

    qTableWidget->setGeometry(QRect(40, 140, 300, 500));
    //连接信号，简单讲把qTableWidget的cellClicked事件绑定到当前类的函数qTableWidget_cellClicked
    //这样qTableWidget点击后就可以与当前主界面交互了
    connect(qTableWidget, SIGNAL(cellClicked(int,int)), this, SLOT(qTableWidget_cellClicked(int,int)));
    qTableWidget->setRowCount(stockList.length());    //设置行数

    for (int i=0;i<stockList.length();i++)
    {
        QPaintKLine::CurStock item=stockList[i];
        qTableWidget->setItem(i,0,new QTableWidgetItem(item.code));
        qTableWidget->setItem(i,1,new QTableWidgetItem(item.name));
        qTableWidget->setItem(i,2,new QTableWidgetItem(item.spellCode));
        qTableWidget->setItem(i,3,new QTableWidgetItem(item.type));
        stockPos.insert(item.code,i);
    }
    //自适应列宽度
    qTableWidget->horizontalHeader()->sectionResizeMode(QHeaderView::Stretch);
    qTableWidget->horizontalHeader()->setSectionResizeMode(0, QHeaderView::ResizeToContents);     //然后设置要根据内容使用宽度的列
    qTableWidget->horizontalHeader()->setSectionResizeMode(1, QHeaderView::ResizeToContents);     //然后设置要根据内容使用宽度的列
    qTableWidget->horizontalHeader()->setSectionResizeMode(2, QHeaderView::ResizeToContents);     //然后设置要根据内容使用宽度的列
    qTableWidget->horizontalHeader()->setSectionResizeMode(3, QHeaderView::ResizeToContents);     //然后设置要根据内容使用宽度的列

    qTableWidget->hide();

    //自选股
    QList<QPaintKLine::CurStock> myStockList;
    readStockIndexFromcsv(myStockList);
    if(myStockList.length()>0)
    {
        ui->cmbMyStock->clear();
        for (int i=0;i<myStockList.length();i++)
        {
            QPaintKLine::CurStock item=myStockList[i];
            ui->cmbMyStock->addItem(item.name);
            ui->cmbMyStock->setItemData(i,item.code);
        }
    }

    //选股区域
    //隐藏标题栏
    ui->tbChooseStock->setWindowFlags(Qt::FramelessWindowHint);
    //不可编辑表格
    ui->tbChooseStock->setEditTriggers(QAbstractItemView::NoEditTriggers);
    /*设置表格为整行选中*/
    ui->tbChooseStock->setSelectionBehavior(QAbstractItemView::SelectRows);

    /*单行选中*/
    ui->tbChooseStock->setSelectionMode(QAbstractItemView::SingleSelection);

    ui->tbChooseStock->verticalHeader()->setVisible(false);
    ui->tbChooseStock->horizontalHeader()->setVisible(false);
    //连接信号，简单讲把qTableWidget的cellClicked事件绑定到当前类的函数qTableWidget_cellClicked
    //这样qTableWidget点击后就可以与当前主界面交互了
    connect(ui->tbChooseStock, SIGNAL(cellClicked(int,int)), this, SLOT(tbChooseStock_cellClicked(int,int)));

}
void MainWindow::readStockIndexFromcsv(QList<QPaintKLine::CurStock> &myStockList)
{

    QFile csvFile(qApp->applicationDirPath() +"/myStock.csv");
    QStringList csvList;
    csvList.clear();
    if(!csvFile.exists())
    {
        QMessageBox::about(this, "csv文件", "未能加载自选股列表,请确定根目录下myStock.csv文件是否存在！");
        return;
    }
    if (csvFile.open(QIODevice::ReadOnly)) //对csv文件进行读写操作
    {
        QTextStream stream(&csvFile);
        while (!stream.atEnd())
        {
            csvList.push_back(stream.readLine()); //保存到List当中
        }
    }
    csvFile.close();
    if(csvList.length()==0)
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
        QMessageBox::information(this, "warn!!!", "网络连接失败,请检查网络后再试!");
        return;
    }
    QJsonDocument jd = QJsonDocument::fromJson(result.toUtf8());
    if(jd.isObject())
    {
        QJsonObject jo=jd.object();
        QJsonValue jdata = jo.value("data");
        QJsonArray jsonArr=jdata["diff"].toArray();
        if(jsonArr.count()<2)
        {
            return;
        }
        //    result=result.replace("f12", "");
        //    result=result.replace("f13", "");
        //    result=result.replace("f14", "");
        //    result=result.replace(":", "");
        //QStringList qslist= result.split(",");
        //qDebug() <<qslist1;
        //int distance=3;
        for (int i=0;i<jsonArr.count();i++)
        {
            QPaintKLine::CurStock item;
            QJsonObject jobj=jsonArr[i].toObject();
            //QJsonValue jdata = jobj.value("data");
            item.code = jobj.value("f12").toString();
            item.type = (jobj.value("f13").toInt()==1?"sh":"sz");
            item.name = jobj.value("f14").toString();
            item.spellCode = getChineseSpell(item.name);
            stockList.append(item);
        }
    }
    //code排序
    qSort(stockList.begin(), stockList.end(), CompareQList);
}
//排列判断

void MainWindow::getKData(const QString &url,QList<CycleData>& list)
{
    //QList<CycleData> list;
    QString result=getContentFromUrl(url);
    if(result=="")
    {
        QMessageBox::information(this, "warn!!!", "网络连接失败,请检查网络后再试!");
        return;
    }
    //    int start=result.indexOf("[");
    //    int end=result.indexOf("]");
    //    if(start==-1 ||end ==-1)
    //    {
    //        return;
    //    }
    //    QString result1=result.mid(start+1,end-start-1);
    QJsonDocument jd = QJsonDocument::fromJson(result.toUtf8());
    if(jd.isObject())
    {
        QJsonObject jo=jd.object();
        QJsonValue jdata = jo.value("data");
        QJsonArray jsonArr=jdata["klines"].toArray();
        if(jsonArr.count()<2)
        {
            return;
        }
        double lastClose;
        for(int i=0;i<jsonArr.count();i++)
        {
            QStringList strList=jsonArr[i].toString().split(",");
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
                lastClose=(jsonArr[i-1].toString().split(","))[2].toDouble();
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
            return static_cast<int>(basePrice * ui->linePercentLatticeValue->text().toDouble() / 100);
        }
        else
        {
            return static_cast<int>(basePrice * ui->linePercentLatticeValue->text().toDouble() / 1000) * 10;
        }
    }
    else
    {
        double percent = 3;
        if (qpaintK->curStock.cycle == "1min")
        {
            percent = 0.1;
        }
        if (qpaintK->curStock.cycle == "5min")
        {
            percent = 0.5;
        }
        if (qpaintK->curStock.cycle == "15min")
        {
            percent = 1;
        }
        if (qpaintK->curStock.cycle == "30min")
        {
            percent = 1.5;
        }
        if (qpaintK->curStock.cycle == "60min")
        {
            percent = 2;
        }
        if (qpaintK->curStock.cycle == "120min")
        {
            percent = 4;
        }
        if (qpaintK->curStock.cycle != "day" && qpaintK->curStock.cycle.indexOf("min") == -1)
        {
            percent = 7;
        }
        if (basePrice < 100)
        {
            double d=(double(int(basePrice * percent) / 10)+1)/10;
            double result =QString::number(d,'d',2).toDouble();
            if ((result >= - EPSINON) && (result <= EPSINON))
            {
                result = 0.05;
            }
            return result;
        }
        else if (basePrice > 100 && basePrice < 1000)
        {
            return static_cast<int>(basePrice * percent / 100);
        }
        else
        {
            return (static_cast<int>(basePrice * percent / 1000)) * 10;
        }
    }
    /*
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
    */
}
// 获取当前实际的格值 按照涨算向下取整 跌则向上取整
int MainWindow::getGridIndexCurrent(double curPrice, bool IsPreUp)
{
    if (IsPreUp)
    {
        return qFloor(curPrice / qpaintK->curStock.latticeValue);
    }
    else
    {
        return qCeil(curPrice / qpaintK->curStock.latticeValue);
    }
}

void MainWindow::CalculateDotGraphic(const QList<CycleData>& list,QList<QPaintKLine::DotValue> &dotValueList)
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
        if(list.length()>1)
        {
            CycleData data1 = list[0];
            CycleData data2 = list[1];
            double increaseRate = data2.high / data1.low - 1;
            double decreaseRate = 1 - data2.low / data1.high;
            status = increaseRate > decreaseRate;
            curPrice = status ? data1.high : data1.low;
        }
        else
        {
            return;
        }
    }

    //第一个点

    int y0 = getGridIndexCurrent(curPrice, status);

    QList<CycleData> datas0;
    datas0.append(list[0]);
    QPaintKLine::DotValue dotValue0;
    dotValue0.x=0;
    dotValue0.y=y0;
    dotValue0.datas=datas0;
    dotValue0.isFill=false;
    dotValue0.isUp=status;

    //插入最前面
    dotValueList.prepend(dotValue0);

    for (int i = 1; i < list.length(); i++)
    {

        QPaintKLine::DotValue preDot = dotValueList[0];
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
                    QPaintKLine::DotValue dotValue1;
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
                        QPaintKLine::DotValue dotValue1;
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
                    QPaintKLine::DotValue dotValue1;
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
                        QPaintKLine::DotValue dotValue1;
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
    if(list.length()==0)
    {
        return;
    }
    qpaintK->dataList=list;
    getMaxMinPriceVol(list);
    screnWidthK=static_cast<int>(list.length()*(qpaintK->klineWith+qpaintK->klineDistance)*qpaintK->scale+qpaintK->offSetWidth);
    screnHeightK=ui->scrollArea->height()-20;

    qpaintK->setObjectName(QString::fromUtf8("qpaintK"));
    qpaintK->resize(screnWidthK,screnHeightK);
    qpaintK->screenWidth=screnWidthK;
    qpaintK->screenHeight=screnHeightK;

    //反转
    if (((qpaintK->curStock.latticeValue >= - EPSINON) && (qpaintK->curStock.latticeValue <= EPSINON)) || ui->chkPercentLattice->checkState()==Qt::Checked||ui->lineEdit->text()=="")
    {
        qpaintK->curStock.latticeValue = CalLatticeValue(qpaintK->curStock.minPrice+qpaintK->curStock.maxPrice/2);
        ui->lineEdit->setText(QString::number(qpaintK->curStock.latticeValue));
    }
    else
    {
        qpaintK->curStock.latticeValue= ui->lineEdit->text().toDouble();
    }
    //修正最小最大值
    qpaintK->curStock.minPrice=(static_cast<int>(qpaintK->curStock.minPrice/qpaintK->curStock.latticeValue))*qpaintK->curStock.latticeValue;
    qpaintK->curStock.maxPrice=((static_cast<int>(qpaintK->curStock.maxPrice/qpaintK->curStock.latticeValue))+1)*qpaintK->curStock.latticeValue;

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
    //获取点数图数据
    QList<QPaintKLine::DotValue> dotValueList;
    CalculateDotGraphic(list, dotValueList);
    qpaintK->dotValueList=dotValueList;
    //绘画K线
    qpaintK->update();
}

void MainWindow::DrawDotGraphic(QList<CycleData> &list)
{

    int startIndex = getGridIndexCurrent( qpaintK->curStock.minPrice, false);
    //结束值向上取整
    int endIndex = getGridIndexCurrent(qpaintK->curStock.maxPrice, true);
    ui->twDotGraphic->setFont( QFont( "Times", static_cast<int>(12*qpaintK->scale) ) );

    //绘制点数图
    int rectWidth=static_cast<int>(20*qpaintK->scale);
    int row=endIndex-startIndex;
    if(qpaintK->dotValueList.length()==0)
    {
        return;
    }
    int col=qpaintK->dotValueList[0].x;
    if(row>36)
    {
        rectWidth=static_cast<int>(10*qpaintK->scale);
        ui->twDotGraphic->setFont( QFont( "Times", static_cast<int>(6*qpaintK->scale) ) );
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
    for (int i=1;i<col+2;i++)
    {
        ui->twDotGraphic->setColumnWidth(i, rectWidth);

    }
    for (int i=0;i<row+3;i++)
    {
        ui->twDotGraphic->setRowHeight(i, rectWidth);
    }
    for (int i=0;i<qpaintK->dotValueList.length();i++)
    {
        QPaintKLine::DotValue dotValue=qpaintK->dotValueList[i];
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
            str=str+dotValue.datas[j].date+":"+QString::number(dotValue.isUp?dotValue.datas[j].high:dotValue.datas[j].low)+"\n";
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
    QTableWidgetItem * lastItem = ui->twDotGraphic->item(row-(qpaintK->dotValueList[0].y-startIndex)+1,qpaintK->dotValueList[0].x+2);
    ui->twDotGraphic->scrollToItem(lastItem);

    if(list.length()>0)
    {
        QString strInfo="股票："+qpaintK->curStock.name+
                "\n代码："+qpaintK->curStock.code+
                "\n收    ："+QString::number(list[list.length()-1].close,'d',2)+"      高    ："+QString::number(list[list.length()-1].high,'d',2)+
                "\n开    ："+QString::number(list[list.length()-1].open,'d',2)+"      低    ："+QString::number(list[list.length()-1].low,'d',2)+
                "\n换手："+QString::number(list[list.length()-1].turnoverRate,'d',2)+"%   缩放："+QString::number(qpaintK->scale,'d',2);
        QString strInfo1="涨幅："+QString::number((list[list.length()-1].close/list[list.length()-1].lastClose-1)*100,'d',2)+"%";
        ui->labInfo->setFont(QFont( "Times", 10));
        ui->labInfo->setStyleSheet("color:red;");
        ui->labInfo->setText(strInfo);

        ui->labInfo1->setFont(QFont( "Times", 10));
        if(list[list.length()-1].close>list[list.length()-1].lastClose)
        {
            ui->labInfo1->setStyleSheet("color:red;");
        }
        else
        {
            ui->labInfo1->setStyleSheet("color:blue;");
        }
        ui->labInfo1->setText(strInfo1);
    }
}

bool MainWindow::eventFilter(QObject *watched, QEvent *event)
{
    if (watched==ui->lineEditStock)
    {
        if (event->type()==QEvent::FocusIn)
        {
            ui->lineEditStock->setText("");
            qpaintK->curStock.code="";
            qpaintK->curStock.name="";

            qTableWidget->show();
            qTableWidget->setGeometry(QRect(40, 140, 300, 500));

        }
        if (event->type()==QEvent::FocusOut)
        {
            qTableWidget->hide();
        }
    }
    return false;//这个地方不要返回true 否则绑定的控件会被无效的。。。。
}


void MainWindow::qTableWidget_cellClicked(int row,int col)
{
    setCurStock(row,1);
    qTableWidget->hide();
    on_btGenerate_clicked();
}
void MainWindow::setCurStock(int row,int type)
{
    if(type==0)
    {
        qpaintK->curStock.code=ui->cmbMyStock->currentData().toString();
        qpaintK->curStock.name=ui->cmbMyStock->currentText();
        QString type=(qpaintK->curStock.code.startsWith("6")?"sh":"sz");
        if(qpaintK->curStock.name=="上证指数")
        {
            type="sh";
        }
        qpaintK->curStock.type=type;
        ui->lineEditStock->setText(qpaintK->curStock.name);
        qpaintK->curStock.cycle=ui->cmbCycle->currentText();
        qpaintK->curStock.rehabilitation="1";
        qpaintK->curStock.latticeValue=0;
        qTableWidget->hide();
    }
    else if(type==1)
    {
        qpaintK->curStock.code=qTableWidget->item(row,0)->text();
        qpaintK->curStock.name=qTableWidget->item(row,1)->text();
        qpaintK->curStock.type=qTableWidget->item(row,3)->text();
        ui->lineEditStock->setText(qpaintK->curStock.name);
        qpaintK->curStock.cycle=ui->cmbCycle->currentText();
        qpaintK->curStock.rehabilitation="1";
        qpaintK->curStock.latticeValue=0;
    }
    else if(type==2)
    {
        qpaintK->curStock.code=ui->cmbChoose->currentData().toString();
        qpaintK->curStock.name=ui->cmbChoose->currentText();
        QString type=(qpaintK->curStock.code.startsWith("6")?"sh":"sz");
        if(qpaintK->curStock.name=="上证指数")
        {
            type="sh";
        }
        qpaintK->curStock.type=type;
        ui->lineEditStock->setText(qpaintK->curStock.name);
        qpaintK->curStock.cycle=ui->cmbCycle->currentText();
        qpaintK->curStock.rehabilitation="1";
        qpaintK->curStock.latticeValue=0;
        qTableWidget->hide();
    }
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

    if(qpaintK->curStock.code=="")
    {
        return;
    }
    QString klt="101";
    if (qpaintK->curStock.cycle == "day")
    {
        klt = "101";
    }
    else if (qpaintK->curStock.cycle == "week")
    {
        klt = "102";
    }
    else if (qpaintK->curStock.cycle == "month")
    {
        klt = "103";
    }
    else
    {
        klt = qpaintK->curStock.cycle.replace("min", "");
    }
    qpaintK->curStock.cycle=ui->cmbCycle->currentText();
    qpaintK->curStock.isClose=(ui->radClose->isChecked());
    qpaintK->scale=1;
    qpaintK->isConnectHighLow=(ui->chkConHighLow->checkState()==Qt::Checked);
    QString typeNum = (qpaintK->curStock.type=="sh"?"1":"0");
    QString url="http://push2his.eastmoney.com/api/qt/stock/kline/get?&secid="+
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
    ui->lablinkDFCF->setText("<a href=\""+urlDFCF+"\" rel=\"noreferrer\">东方财富");

    //ui->labZJS->setOpenExternalLinks(true);
    QString urlZJS="http://www.sse.com.cn/star/market/stocklist/info/index/index.shtml?COMPANY_CODE="+qpaintK->curStock.code;
    ui->labZJS->setText("<a href=\""+urlZJS+"\">证交所");

}
void MainWindow::openZJSURL()
{
    QString urlZJS;
    QUrl url(urlZJS);
    if(qpaintK->curStock.type=="sh")
    {
        urlZJS="http://www.sse.com.cn/market/price/trends/";
        //urlZJS="http://www.sse.com.cn/star/market/stocklist/info/index/index.shtml?COMPANY_CODE="+qpaintK->curStock.code;
        url.setUrl(urlZJS);
        //QString host="http://www.sse.com.cn/star/market/stocklist/";
        //QDesktopServices::openUrl(host);
    }
    else
    {
        urlZJS="http://www.szse.cn/certificate/individual/index.html?code="+qpaintK->curStock.code;
        url.setUrl(urlZJS);
    }

    QDesktopServices::openUrl(urlZJS);
}
//滚轮事件
void MainWindow::wheelEvent(QWheelEvent *event)
{
    if(event->delta() > 0)
    {
        qpaintK->scale=QString::number(qpaintK->scale*1.1,'d',2).toDouble();
        DrawKline(kList);
        DrawDotGraphic(kList);
    }
    else
    {
        qpaintK->scale=QString::number(qpaintK->scale*0.9,'d',2).toDouble();
        DrawKline(kList);
        DrawDotGraphic(kList);
    }
}

void MainWindow::on_cmbCycle_currentIndexChanged(int index)
{

    if(ui->cmbCycle->currentText()=="month")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-10000));
        ui->dtbegin->setDate(begin);
    }
    if(ui->cmbCycle->currentText()=="week")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-5000));
        ui->dtbegin->setDate(begin);
    }
    if(ui->cmbCycle->currentText()=="day")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-1200));
        ui->dtbegin->setDate(begin);
    }
    else if(ui->cmbCycle->currentText()=="120min")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-90));
        ui->dtbegin->setDate(begin);
    }
    else if(ui->cmbCycle->currentText()=="60min")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-50));
        ui->dtbegin->setDate(begin);
    }
    else if(ui->cmbCycle->currentText()=="30min")
    {
        QDate end(QDate::currentDate());
        QDate begin(end.addDays(-30));
        ui->dtbegin->setDate(begin);
    }
    else if(ui->cmbCycle->currentText()=="5min")
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

    if(!(ui->twDotGraphic->item(row,column) == nullptr))
    {
        QString str=ui->twDotGraphic->item(row,column)->data(3).toString();
        QToolTip::showText(QCursor::pos(),str);
    }
}

void MainWindow::on_btAddMyStock_clicked()
{
    QList<QPaintKLine::CurStock>  myStockList;
    QString existCode="";
    readStockIndexFromcsv(myStockList);
    for (int i=0;i<myStockList.length();i++)
    {
        QPaintKLine::CurStock item=myStockList[i];
        if(item.code==qpaintK->curStock.code)
        {
            //QMessageBox::information(this, "warn!!!", "已存在股票,请不要重复添加！");
            //return;
            existCode=item.code;
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
    }
    outFile.close();
    if (outFile.open(QIODevice::WriteOnly))
    {
        for (int i=0;i<CSVList.length();i++)
        {
            if(existCode!="" && CSVList[i].contains(existCode))
            {
                continue;
            }
            outFile.write((CSVList[i]+"\n").toStdString().c_str());
            if((CSVList.length()<4 && i==CSVList.length()-1)|| i==3 )
            {
                outFile.write((line+"\n").toStdString().c_str());
            }
        }
    }
    outFile.close();
    QMessageBox::information(this, "warn!!!", "自选股添加成功！");

    qTableWidget->clear();
    stockList.clear();
    getStockList(urlStockList,stockList);
    InitStockList(stockList);
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
        if(static_cast<quint8>(buf[i]) < 0x80 ) //gbk的第一个字节都大于0x81，所以小于0x80的都是符号，字母，数字etc
            continue;
        array[j] = ((static_cast<quint8>(buf[i])) << 8) + static_cast<quint8>(buf[i+1]); //计算gbk编码
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

void MainWindow::on_lineEditStock_textChanged(const QString &arg1)
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
    qTableWidget->setGeometry(QRect(40, 140, 300, 500));

}

void MainWindow::on_cmbMyStock_currentIndexChanged(int index)
{
    //ui->cmbChoose->setCurrentIndex(-1);
    setCurStock(index,0);
    on_btGenerate_clicked();
}

void MainWindow::on_btDownload_clicked()
{
    if(qpaintK->curStock.cycle.indexOf("min")>0 ||qpaintK->curStock.cycle=="day")
    {
        QMessageBox::information(this, "warn!!!", "暂不支持日、分钟级别的周期下载!");
        return;
    }
    QFile stockListFile(qApp->applicationDirPath() +"/stocklist.csv");
    if (stockListFile.open(QIODevice::WriteOnly))
    {
        for (int i=0;i<stockList.length();i++)
        {
            QPaintKLine::CurStock item= stockList[i];
            stockListFile.write((item.code+","+item.name+","+item.type+"\n").toStdString().c_str());
        }
    }

    stockListFile.close();

    QProgressBar *qpb=new QProgressBar(this);
    qpb->setGeometry(QRect(500, 500, 800, 50));
    qpb->setRange(0,stockList.length()-1);
    qpb->setValue(0);
    qpb->setWindowTitle("正在下载"+qpaintK->curStock.cycle+"级别数据...");
    qpb->show();
    QString fullPath=qApp->applicationDirPath() +"/data/eastmoney/"+qpaintK->curStock.cycle;
    QDir dir(fullPath);
    if(!dir.exists())
    {
        dir.mkpath(fullPath);//创建多级目录
    }

    QString strBeginDate="19910101";
    QString klt="101";

    if (qpaintK->curStock.cycle == "day")
    {
        klt = "101";
        strBeginDate=QDate::currentDate().addDays(-300).toString("yyyyMMdd");
    }
    else if (qpaintK->curStock.cycle == "week")
    {
        klt = "102";
        strBeginDate="20180101";
    }
    else if (qpaintK->curStock.cycle == "month")
    {
        klt = "103";
        strBeginDate="20100101";
    }
    else
    {
        klt = qpaintK->curStock.cycle.replace("min", "");
        strBeginDate=QDate::currentDate().addDays(-30).toString("yyyyMMdd");
    }
    for (int i=0;i<stockList.length();i++)
    {
        qpb->setValue(i);
        QPaintKLine::CurStock item=stockList[i];
        QString typeNum = (item.type=="sh"?"1":"0");
        if(ui->chkIs_KC_CY->checkState()==Qt::Unchecked)
        {
            if(item.code.startsWith("300")>0 || item.code.startsWith("688")>0)
            {
                continue;
            }
        }
        QFile outFile(fullPath+"/"+item.type+"."+item.code+".csv");

        //        if(outFile.exists() && outFile.size()>1000)
        //        {
        //            continue;
        //        }



        QString url="http://push2his.eastmoney.com/api/qt/stock/kline/get?&secid="+
                typeNum+"."+item.code+
                "&fields1=f1%2Cf2%2Cf3%2Cf4%2Cf5&fields2=f51%2Cf52%2Cf53%2Cf54%2Cf55%2Cf56%2Cf57%2Cf61&"+
                "klt="+klt+
                "&fqt=1&beg="+strBeginDate+"&end="+ui->dtend->date().toString("yyyyMMdd");
        QString result=getContentFromUrl(url);
        if(result=="")
        {
            QMessageBox::information(this, "warn!!!", "网络连接失败,请检查网络后再试!");
            return;
        }
        QJsonDocument jd = QJsonDocument::fromJson(result.toUtf8());
        if(jd.isObject())
        {
            QJsonObject jo=jd.object();
            QJsonValue jdata = jo.value("data");
            QJsonArray jsonArr=jdata["klines"].toArray();
            if(jsonArr.count()<2)
            {
                continue;
            }
            QString lastLine="";
            if (outFile.open(QIODevice::WriteOnly))
            {
                QString lastClose;
                for(int i=0;i<jsonArr.count();i++)
                {
                    QStringList strList=jsonArr[i].toString().split(",");
                    //qDebug() <<str;
                    CycleData cycleData;
                    //日期,开盘价,收盘价,最高价,最低价,前收盘,成交量,成交金额,turnoverRate
                    //("2020-08-24", "6.54", "6.55", "6.61", "6.40", "129204", "83730839.00", "0.66")
                    if(i==0)
                    {
                        lastClose=strList[2];
                    }
                    else
                    {
                        lastClose=(jsonArr[i-1].toString().split(","))[2];
                    }
                    //code,name,date,close,high,low,open,lastclose,vol,amount,turnoverRate
                    QString line=item.code+","+item.name+","+strList[0]
                            +","+strList[2]+","+strList[3]+","+strList[4]
                            +","+strList[1]+","+lastClose+","+strList[5]
                            +","+strList[6]+","+strList[7];
                    outFile.write((line+"\n").toStdString().c_str());
                    lastLine=line;
                }
            }
            outFile.close();
        }
    }

    qpb->hide();
    QMessageBox::information(this, "info", "下载完成!");
}

void MainWindow::on_btChoose_clicked()
{
    QMap<QString,QString> mapOfChoose;
    /*
    for (int i=0;i<stockList.length();i++)
    {
        QPaintKLine::CurStock item=stockList[i];
        if(ui->chkIs_KC_CY->checkState()==Qt::Unchecked)
        {
            if(item.code.startsWith("300")>0 || item.code.startsWith("688")>0)
            {
                continue;
            }
        }
        QFile outFile(qApp->applicationDirPath() +"/data/eastmoney/month/"+item.type+"."+item.code+".csv");
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

        //code,name,date,close,high,low,open,lastclose,vol,amount,turnoverRate

        int cycle=ui->lineEditCycle->text().toInt();
        double rate=ui->lineEditRate->text().toDouble()/100;
        double minPrice=9999999999;
        double maxPrice=0;
        if(CSVList.length()==0 ||CSVList.length()<=cycle)
        {
            continue;
        }
        for (int i=CSVList.length()-cycle-1;i<CSVList.length()-1;i++)
        {
            QStringList data=CSVList[i].split(',');
            double high=data[4].toDouble();
            double low=data[5].toDouble();
            if(high>maxPrice)
            {
                maxPrice=high;
            }
            if(low<minPrice)
            {
                minPrice=low;
            }
        }
        QStringList curCycle=CSVList[CSVList.length()-1].split(',');

        if(maxPrice/minPrice<(1+rate) && curCycle[4].toDouble()>maxPrice)
        {
            mapOfChoose.insert(curCycle[0],curCycle[1]);
        }
    }*/
    //QProcess shell;
    QString params;
    QString message="";

    for (int i=0;i<ui->tbCondition->rowCount();i++)
    {
        QString text="";
        if(i>0)
        {
            text=ui->tbCondition->item(i,1)->text();
            if(text=="")
            {
                message+=QString::number(i)+"、";
            }
            params+=text+"\n";
        }
    }
    if(message!="")
    {
        QMessageBox::information(this, "warn!!!", "请输入第 "+message+"行参数");
        return;
    }

    QProgressBar *qpb=new QProgressBar(this);
    qpb->setGeometry(QRect(500, 500, 800, 50));
    qpb->setRange(0,stockList.length()-1);
    qpb->setValue(0);
    qpb->setWindowTitle("正在选股...");
    qpb->show();

    QStringList resultList;
    for (int i=0;i<stockList.length();i++)
    {
        qpb->setValue(i);
        QPaintKLine::CurStock item=stockList[i];
        if(ui->chkIs_KC_CY->checkState()==Qt::Unchecked)
        {
            if(item.code.startsWith("300")>0 || item.code.startsWith("688")>0)
            {
                continue;
            }
        }
        QComboBox *combox=((QComboBox*)ui->tbCondition->cellWidget(0, 1));
        QString cycle = combox->currentText();
        QFile outFile(qApp->applicationDirPath() +"/data/eastmoney/"+cycle+"/"+item.type+"."+item.code+".csv");
        QString CSVList;
        if (outFile.open(QIODevice::ReadOnly))
        {
            QTextStream stream(&outFile);
            CSVList=stream.readAll();
        }
        outFile.close();

        //code,name,date,close,high,low,open,lastclose,vol,amount,turnoverRate

        //int cycle=ui->lineEditCycle->text().toInt();
        //double rate=ui->lineEditRate->text().toDouble()/100;

        if(CSVList=="")
        {
            continue;
        }
        QString paramsIn=params;

        paramsIn+=item.code+"\n";
        paramsIn+=item.name;

        QString js=qApp->applicationDirPath()+ui->cmbFormula->currentData().toString();
        QString result=callJs(js,paramsIn,CSVList);
        if(result!="")
        {
            resultList.append(result);
        }
    }
    qpb->hide();
    for (int i=0;i<resultList.length()-1;i++)
    {
        QStringList item=resultList[i].split(',');
        mapOfChoose.insert(item[0],item[1]);
    }
    //填充选股结果到表格
    if(mapOfChoose.count()>0)
    {
        QMap<QString,QString>::Iterator  it;
        ui->tbChooseStock->setRowCount(mapOfChoose.count());
        ui->tbChooseStock->setColumnCount(2);
        int i=0;
        ui->cmbChoose->clear();
        for(it = mapOfChoose.begin();it != mapOfChoose.end();++it)
        {
            ui->tbChooseStock->setItem(i,0,new QTableWidgetItem(it.key()));
            ui->tbChooseStock->setItem(i,1,new QTableWidgetItem(it.value()));
            ui->cmbChoose->addItem(it.value());
            ui->cmbChoose->setItemData(i,it.key());
            i++;
        }
        ui->cmbChoose->setCurrentIndex(-1);

    }
}

void MainWindow::tbChooseStock_cellClicked(int row,int col)
{
    ui->cmbMyStock->setCurrentIndex(-1);
    ui->tabWidget->setCurrentIndex(0);
    qpaintK->curStock.code=ui->tbChooseStock->item(row,0)->text();
    ui->cmbCycle->setCurrentIndex(2);
    qTableWidget_cellClicked(stockPos[qpaintK->curStock.code],0);
    ui->cmbChoose->setCurrentIndex(row);
}

void MainWindow::on_cmbChoose_currentIndexChanged(int index)
{
    //ui->cmbMyStock->setCurrentIndex(-1);
    setCurStock(index,2);
    on_btGenerate_clicked();
}

void MainWindow::on_cmbChoose_activated(int index)
{
    ui->cmbMyStock->setCurrentIndex(-1);
}

void MainWindow::on_cmbMyStock_activated(int index)
{
    ui->cmbChoose->setCurrentIndex(-1);
}

void MainWindow::on_cmbFormula_currentIndexChanged(int index)
{
    QStringList paramsName=mapFormula[index].split(',');
    int row=paramsName.length()-1;
    ui->tbCondition->setRowCount(row);
    ui->tbCondition->setColumnCount(2);
    ui->tbCondition->verticalHeader()->setVisible(false);
    ui->tbCondition->horizontalHeader()->setVisible(false);
    ui->tbCondition->clear();
    ui->tbCondition->setItem(0,0,new QTableWidgetItem("周期"));
    QComboBox *colCmb = new QComboBox();

    colCmb->addItem("month");
    colCmb->addItem("week");
    ui->tbCondition->setCellWidget(0,1,colCmb);

    for(int i=1;i<row;i++)
    {
        QStringList params=paramsName[i+1].split(":");
        ui->tbCondition->setItem(i,0,new QTableWidgetItem(params[0]));
        if(params.length()==2)
        {
            ui->tbCondition->setItem(i,1,new QTableWidgetItem(params[1]));
        }
    }
    ui->tbCondition->horizontalHeader()->setSectionResizeMode(0, QHeaderView::ResizeToContents);

}
/*Qt通过js计算*/
QString MainWindow::callJs(QString fileName, QString &params,QString &dataList)
{

    QFile scriptFile(fileName);
    if (!scriptFile.open(QIODevice::ReadOnly | QIODevice::Text))
    {
        QMessageBox::information(this, "warn!!!", "加载脚本"+fileName+"失败");
        return "";
    }
    QTextStream stream(&scriptFile);
    QString JsContents = stream.readAll();
    QJSEngine JsEngine;
    //加载脚本
    JsEngine.evaluate(JsContents, fileName);
    QJSValue calculate = JsEngine.globalObject().property("calculate");
    // 执行js中的函数
    QJSValueList args;

    args<<params<<dataList;
    QJSValue resultValue = calculate.call(args);
    QString result = resultValue.toString();

    if (resultValue.isError())
    {
        qDebug()<<"执行"<<fileName<<params<<"错误:"+result;
        return "";
    }
    return result;
}
