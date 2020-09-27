#!/bin/bash
# 功能：
killall wyckoffAnalytics;
cd /opt/wyckoffAnalytics;
nohup ./wyckoffAnalytics >/dev/null 2>&1 &

