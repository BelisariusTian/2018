/**
 * 验证身份证号码
 * @param {} num 
 * @returns {} 
 */
function isIdCardNo(num) {

    num = num.toUpperCase();           //身份证号码为15位或者18位，15位时全为数字，18位前17位为数字，最后一位是校验位，可能为数字或字符X。       
   
    if (!(/(^\d{15}$)|(^\d{17}([0-9]|X)$)/.test(num))) {
        alert('输入的身份证号长度不对，或者号码不符合规定！\n15位号码应全为数字，18位号码末位可以为数字或X。');
        return false;
    } //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。
    //下面分别分析出生日期和校验位
    var len, re; len = num.length; if (len == 15) {
        re = new RegExp(/^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})$/);
        var arrSplit = num.match(re);  //检查生日日期是否正确
        var dtmBirth = new Date('19' + arrSplit[2] + '/' + arrSplit[3] + '/' + arrSplit[4]);
        var bGoodDay; bGoodDay = (dtmBirth.getYear() == Number(arrSplit[2])) && ((dtmBirth.getMonth() + 1) == Number(arrSplit[3])) && (dtmBirth.getDate() == Number(arrSplit[4]));
        if (!bGoodDay) {
            layer.alert('输入的身份证号里出生日期不对！');
            return false;
        } else { //将15位身份证转成18位 //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。       
            var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
            var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
            var nTemp = 0, i;
            num = num.substr(0, 6) + '19' + num.substr(6, num.length - 6);
            for (i = 0; i < 17; i++) {
                nTemp += num.substr(i, 1) * arrInt[i];
            }
            num += arrCh[nTemp % 11];
            return true;
        }
    }
    if (len == 18) {
        re = new RegExp(/^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9]|X)$/);
        var arrSplit = num.match(re);  //检查生日日期是否正确
        var dtmBirth = new Date(arrSplit[2] + "/" + arrSplit[3] + "/" + arrSplit[4]);
        var bGoodDay; bGoodDay = (dtmBirth.getFullYear() == Number(arrSplit[2])) && ((dtmBirth.getMonth() + 1) == Number(arrSplit[3])) && (dtmBirth.getDate() == Number(arrSplit[4]));
        if (!bGoodDay) {
            //alert(dtmBirth.getYear());
            //alert(arrSplit[2]);
            layer.alert('输入的身份证号里出生日期不对！');
            return false;
        }
        else { //检验18位身份证的校验码是否正确。 //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。
            var valnum;
            var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
            var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
            var nTemp = 0, i;
            for (i = 0; i < 17; i++) {
                nTemp += num.substr(i, 1) * arrInt[i];
            }
            valnum = arrCh[nTemp % 11];
            if (valnum != num.substr(17, 1)) {
                layer.alert('18位身份证的校验码不正确！应该为：' + valnum);
                return false;
            }
            return true;
        }
    } return false;
}





/**
 * 检查邮箱
 * @param {} str 
 * @returns {} 
 */
function checkEmail(str) {
    var
 re = /^(\w-*\.*)+@(\w-?)+(\.\w{2,})+$/
    if (re.test(str)) {
        return true;
    } else {
        return false;
    }
}





/**
 * 检查手机号码
 * @returns {} 
 */
function checkPhone(phone) {
    if (!(/^1(3|4|5|7|8)\d{9}$/.test(phone))) {
        alert("手机号码有误，请重填");
        return false;
    }
    return true;
}


/** 
* json对象转字符串形式 
*/
function json2str(o) {
    var arr = [];
    var fmt = function (s) {
        if (typeof s == 'object' && s != null) return json2str(s);
        return /^(string|number)$/.test(typeof s) ? "'" + s + "'" : s;
    }
    for (var i in o) arr.push("'" + i + "':" + fmt(o[i]));
    return '{' + arr.join(',') + '}';
}
/**
 * 判断是否为正整数
 * @param {} input 
 * @returns {} 
 */
function checkRate(nubmer) {
    if(isNaN(nubmer)||nubmer<=0||!(/^\d+$/.test(nubmer))) {
        alert("请输入正确的数值,只允许输入整数!");
        return false;
    }
    return true;
}
/**
 * 添加浏览足迹
 * @param {} Type 类别 1产品分期 0其他
 * @param {} SId 产品分期ID
 * @returns {} 
 */
function addFootPrint(Type,SId) {
    $.ajax({
        type: "POST",
        url: "/Service/AddFootPrint",
        data: { B_Url: location.href, UserId: getCookie('UserId'), B_Title: $("title").html(), Type: Type, SId: SId },
        dataType: "json",
        success: function (data) {
            console.log(data.message);
        }
    });
}
/**
 * 获取cookie
 * @param {} name 
 * @returns {} 
 */
function getCookie(name)
{
    var arr,reg=new RegExp("(^| )"+name+"=([^;]*)(;|$)");
    if(arr=document.cookie.match(reg))
        return unescape(arr[2]);
    else
        return null;
}
/**
 * 设置cookie
 * @param {} name 
 * @param {} value 
 * @returns {} 
 */
function setCookie(name, value) {
    var Days = 30;
    var exp = new Date();
    exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
    document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
}


/**
 * 检测字符串是否含有html标签
 * @param {} htmlStr 
 * @returns {} 
 */
function checkHtml(htmlStr) {
    var reg = /<[^>]+>/g;
    return reg.test(htmlStr);
}


//将序列化成json的日期转成日期格式
function changeDateFormat(cellval) {

    if (cellval == null || cellval.length == 0) {
        return "";
    }
    var date = new Date(parseInt(cellval.replace("/Date(", "").replace(")/", ""), 10));
    var month = date.getMonth() + 1 < 10 ? '0' + (date.getMonth() + 1) : date.getMonth() + 1;
    var currentDate = date.getDate() < 10 ? '0' + date.getDate() : date.getDate();


    var h = date.getHours() < 10 ? '0' + date.getHours() + ":" : date.getHours() + ":";

    var ms = date.getMinutes() < 10 ? '0' + date.getMinutes() + ":" : date.getMinutes() + ":";

    var s = date.getSeconds() < 10 ? '0' + date.getSeconds() : date.getSeconds();


    return date.getFullYear() + '-' + month + '-' + currentDate + "  " + h + ms + s;
}


//将序列化成json的日期转成日期格式,  这个是不要时分秒的
function changeShortDateFormat(cellval) {

    if (cellval == null || cellval.length == 0) {
        return "";
    }
    var date = new Date(parseInt(cellval.replace("/Date(", "").replace(")/", ""), 10));
    var month = date.getMonth() + 1 < 10 ? '0' + (date.getMonth() + 1) : date.getMonth() + 1;
    var currentDate = date.getDate() < 10 ? '0' + date.getDate() : date.getDate();

    return date.getFullYear() + '-' + month + '-' + currentDate;
}


//比较时间大小
function CompareDateMaxMin(time1, time2) {

    //比大小时间
    var timeOne = new Date(time1);
    var timeTwo = new Date(time2);

    if (timeOne.getTime() > timeTwo.getTime()) {
        return true
    } else {
        return false
    }

}
