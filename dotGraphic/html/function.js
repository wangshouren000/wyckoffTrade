//H(hue)色相，S(saturation)饱和度，以及L(lightness)亮度。

//rgb转hsl

// r,g,b范围为[0,255],转换成h范围为[0,360]
// s,l为百分比形式，范围是[0,100],可根据需求做相应调整
function rgbtohsl(r,g,b){
    r=r/255;
    g=g/255;
    b=b/255;

    var min=Math.min(r,g,b);
    var max=Math.max(r,g,b);
    var l=(min+max)/2;
    var difference = max-min;
    var h,s,l;
    if(max==min){
        h=0;
        s=0;
    }else{
        s=l>0.5?difference/(2.0-max-min):difference/(max+min);
        switch(max){
            case r: h=(g-b)/difference+(g < b ? 6 : 0);break;
            case g: h=2.0+(b-r)/difference;break;
            case b: h=4.0+(r-g)/difference;break;
        }
        h=Math.round(h*60);
    }
    s=Math.round(s*100);//转换成百分比的形式
    l=Math.round(l*100);
    return [h,s,l];
}

//rgb转hsv
// r,g,b范围为[0,255],转换成h范围为[0,360]
// s,v为百分比形式，范围是[0,100],可根据需求做相应调整
function rgbtohsv(r,g,b){
    r=r/255;
    g=g/255;
    b=b/255;
    var h,s,v;
    var min=Math.min(r,g,b);
    var max=v=Math.max(r,g,b);
    var l=(min+max)/2;
    var difference = max-min;
    
    if(max==min){
        h=0;
    }else{
        switch(max){
            case r: h=(g-b)/difference+(g < b ? 6 : 0);break;
            case g: h=2.0+(b-r)/difference;break;
            case b: h=4.0+(r-g)/difference;break;
        }
        h=Math.round(h*60);
    }
    if(max==0){
        s=0;
    }else{
        s=1-min/max;
    }
    s=Math.round(s*100);
    v=Math.round(v*100);
    return [h,s,v];
}

//hsl转rgb
//输入的h范围为[0,360],s,l为百分比形式的数值,范围是[0,100] 
//输出r,g,b范围为[0,255],可根据需求做相应调整
function hsltorgb(h,s,l){
    var h=h/360;
    var s=s/100;
    var l=l/100;
    var rgb=[];

    if(s==0){
        rgb=[Math.round(l*255),Math.round(l*255),Math.round(l*255)];
    }else{
        var q=l>=0.5?(l+s-l*s):(l*(1+s));
        var p=2*l-q;
        var tr=rgb[0]=h+1/3;
        var tg=rgb[1]=h;
        var tb=rgb[2]=h-1/3;
        for(var i=0; i<rgb.length;i++){
            var tc=rgb[i];
            //console.log(tc);
            if(tc<0){
                tc=tc+1;
            }else if(tc>1){
                tc=tc-1;
            }
            switch(true){
                case (tc<(1/6)):
                    tc=p+(q-p)*6*tc;
                    break;
                case ((1/6)<=tc && tc<0.5):
                    tc=q;
                    break;
                case (0.5<=tc && tc<(2/3)):
                    tc=p+(q-p)*(4-6*tc);
                    break;
                default:
                    tc=p;
                    break;
            }
            rgb[i]=Math.round(tc*255);
        }
    }
    
    return rgb;
}
//hsb转rgb
//输入的h范围为[0,360],s,l为百分比形式的数值,范围是[0,100] 
//输出r,g,b范围为[0,255],可根据需求做相应调整
function hsvtorgb(h,s,v){
    var s=s/100;
    var v=v/100;
    var h1=Math.floor(h/60) % 6;
    var f=h/60-h1;
    var p=v*(1-s);
    var q=v*(1-f*s);
    var t=v*(1-(1-f)*s);
    var r,g,b;
    switch(h1){
        case 0:
            r=v;
            g=t;
            b=p;
            break;
        case 1:
            r=q;
            g=v;
            b=p;
            break;
        case 2:
            r=p;
            g=v;
            b=t;
            break;
        case 3:
            r=p;
            g=q;
            b=v;
            break;
        case 4:
            r=t;
            g=p;
            b=v;
            break;
        case 5:
            r=v;
            g=p;
            b=q;
            break;
    }
    return [Math.round(r*255),Math.round(g*255),Math.round(b*255)];
}