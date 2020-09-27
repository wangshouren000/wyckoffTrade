#ifndef CYCLEDATA_H
#define CYCLEDATA_H
#include <QString>
#include <stdio.h>

class CycleData
{
public:
    CycleData();
    QString date;
    QString name;
    QString code;
    double high;
    double low;
    double open;
    double close;
    double vol;
    double amount;
    double lastClose;
    double turnoverRate;
};

#endif // CYCLEDATA_H
