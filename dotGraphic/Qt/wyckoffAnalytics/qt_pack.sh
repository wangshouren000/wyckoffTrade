#!/bin/bash
# 功能：
#!/bin/bash
if [ $# != 1 ] ; then
    echo "请输入可执行程序名称"
    exit 444;
fi
exe=$1 #你需要发布的程序名称
mkdir pack
despath=$PWD"/pack"
cp -v $1 $despath
cp -rfv $(cat pack_files) $despath

LibDir=$despath"/lib"

Target=$1

lib_array=($(ldd $Target | grep -o "/.*" | grep -o "/.*/[^[:space:]]*"|grep  Qt))

$(mkdir $LibDir)

for Variable in ${lib_array[@]}

do

cp "$Variable" $LibDir

done

cp $QT_PATH/lib/libQt5DBus.so.5 $LibDir/libQt5DBus.so.5
cp $QT_PATH/lib/libQt5XcbQpa.so.5 $LibDir/libQt5XcbQpa.so.5
mkdir $despath"/platforms/"
cp $QT_PATH/plugins/platforms/libqxcb.so $despath"/platforms/"

echo '
#!/bin/sh
appname=`basename $0 | sed s,\.sh$,,`

dirname=`dirname $0`
tmp="${dirname#?}"

if [ "${dirname%$tmp}" != "/" ]; then
dirname=$PWD/$dirname
fi
LD_LIBRARY_PATH=$dirname/lib
export LD_LIBRARY_PATH
export QT_DEBUG_PLUGINS=1
nohup $dirname/$appname $@ >/dev/null 2>&1 &
'>>$despath"/"$Target.sh

echo "
[Desktop Entry]
Version=1.0
Name=$Target
GenericName=$Target
Comment=$Target
Exec=$despath/$Target.sh
Icon=$despath/wa.ico
Type=Application
Categories=Application;
StartupNotify=true">>$despath"/"$Target.desktop