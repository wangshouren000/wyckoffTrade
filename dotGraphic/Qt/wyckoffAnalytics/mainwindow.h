#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QMessageBox>

#include <QWidget>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QFile>
#include <QEventLoop>
#include <QScrollArea>
#include <QLayout>
#include <QTextCodec>
#include <QStandardItemModel>
#include <QSortFilterProxyModel>
#include <QCompleter>
#include <QLineEdit>
#include <QTableWidget>
#include <QHeaderView>
#include <QtCore/qmath.h>
#include <QToolTip>
#include <QMouseEvent>
#include <QApplication>
#include <QProgressBar>
#include <QProcess>
#include <QProgressDialog>
#include <QDir>
#include <QJSEngine>
#include <QJsonDocument>
#include <QJsonArray>
#include <QJsonObject>
#include<QDesktopServices>
#include<QUrl>

#include "qpaintkline.h"

namespace Ui {
class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

    QString getContentFromUrl(const QString &arg1);
    void Load();

private slots:
    void on_btGenerate_clicked();
    void qTableWidget_cellClicked(int row,int column);
    void tbChooseStock_cellClicked(int row,int column);

    void wheelEvent(QWheelEvent *event);
    bool eventFilter(QObject *,QEvent *);    //对象监视函数，用来监视控件们的

    void on_cmbCycle_currentIndexChanged(int index);

    void on_chkPercentLattice_stateChanged(int arg1);

    void on_twDotGraphic_cellEntered(int row, int column);

    void on_btAddMyStock_clicked();

    void on_lineEditStock_textChanged(const QString &arg1);
    void on_cmbMyStock_currentIndexChanged(int index);

    void on_btDownload_clicked();

    void on_btChoose_clicked();

    void on_cmbChoose_currentIndexChanged(int index);
    void reFresh();

    void on_cmbChoose_activated(int index);

    void on_cmbMyStock_activated(int index);

    void on_cmbFormula_currentIndexChanged(int index);
    void openZJSURL();
private:
    Ui::MainWindow *ui;
    void getKData(const QString &url,QList<CycleData>& list);
    void getStockList(const QString &url,QList<QPaintKLine::CurStock>& stockList);
    void getMaxMinPriceVol(const QList<CycleData>& list);//获取最高最低价格成交量

    void DrawDotGraphic(QList<CycleData> &list);
    void CalculateDotGraphic(const QList<CycleData>& list,QList<QPaintKLine::DotValue> &dotValueList);
    int getGridIndexCurrent(double curPrice, bool IsPreUp);
    double CalLatticeValue(double basePrice);
    void setCurStock(int row,int type);
    void InitBasicData();
    void DrawKline(QList<CycleData>& list);
    QString getChineseSpell(QString& src);
    char ConvertSpell(int n);
    void InitStockList(QList<QPaintKLine::CurStock> &stockList);
    void readStockIndexFromcsv(QList<QPaintKLine::CurStock> &myStockList);
    QString urlStockList="http://26.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz=5000&po=1&np=1&fltt=2&invt=2&fid=f3&fs=m:0+t:6,m:0+t:13,m:0+t:80,m:1+t:2,m:1+t:23&fields=f12,f13,f14";

    bool In(wchar_t start, wchar_t end, wchar_t code);
    QPaintKLine *qpaintK;
    QGridLayout *layoutK;
    QGridLayout *layoutDot;
    int screnHeightK=1000;//屏幕高度
    int screnWidthK=2000;
    QTableWidget *qTableWidget;
    void keyPressEvent(QKeyEvent  *event);
    QList<CycleData> kList;
    bool isOneDotRebuild=false;
    //鼠标水平线
    QWidget *cross_H;
    const double EPSINON = 0.00001;
    static bool CompareQList(const QPaintKLine::CurStock &first, const QPaintKLine::CurStock &second)
    {
        if (first.code.toInt() < second.code.toInt())
        {
            return true;
        }
      return false;
    }

    QList<QPaintKLine::CurStock> stockList;//股票列表
    QMap<QString,int> stockPos;
    QMap<int,QString> mapFormula;//公式列表

    QString callJs(QString fileName,QString &params,QString &dataList);
};

#endif // MAINWINDOW_H
