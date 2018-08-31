/**
 * 判断是否是微信浏览器打开
 * @returns {} 
 */
function isWeiXin() {
    var ua = window.navigator.userAgent.toLowerCase();
    if (ua.match(/MicroMessenger/i) == 'micromessenger') {
        return true;
    } else {
        return false;
    }
}


function PageQuery(q) {
    if (q.length > 1) this.q = q.substring(1, q.length);
    else this.q = null;
    this.keyValuePairs = new Array();
    if (q) {
        for (var i = 0; i < this.q.split("&").length; i++) {
            this.keyValuePairs[i] = this.q.split("&")[i];
        }
    }
    this.getKeyValuePairs = function () { return this.keyValuePairs; }
    this.getValue = function (s) {
        for (var j = 0; j < this.keyValuePairs.length; j++) {
            if (this.keyValuePairs[j].split("=")[0] == s)
                return this.keyValuePairs[j].split("=")[1];
        }
        return false;
    }
    this.getParameters = function () {
        var a = new Array(this.getLength());
        for (var j = 0; j < this.keyValuePairs.length; j++) {
            a[j] = this.keyValuePairs[j].split("=")[0];
        }
        return a;
    }
    this.getLength = function () { return this.keyValuePairs.length; }
}
/**
 * 获取浏览器URL参数
 * @returns {} 
 */
function queryString(key) {
    var page = new PageQuery(window.location.search);
    return unescape(page.getValue(key));
}

/**
 * 验证身份证号码
 * @param {} num 
 * @returns {} 
 */
function isIdCardNo(num) {
    num = num.toUpperCase();           //身份证号码为15位或者18位，15位时全为数字，18位前17位为数字，最后一位是校验位，可能为数字或字符X。       
    if (!(/(^\d{15}$)|(^\d{17}([0-9]|X)$)/.test(num))) {
        layer.msg('输入的身份证号长度不对，或者号码不符合规定！\n15位号码应全为数字，18位号码末位可以为数字或X。');
        return false;
    } //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。
    //下面分别分析出生日期和校验位
    var len, re; len = num.length; if (len == 15) {
        re = new RegExp(/^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})$/);
        var arrSplit = num.match(re);  //检查生日日期是否正确
        var dtmBirth = new Date('19' + arrSplit[2] + '/' + arrSplit[3] + '/' + arrSplit[4]);
        var bGoodDay; bGoodDay = (dtmBirth.getYear() == Number(arrSplit[2])) && ((dtmBirth.getMonth() + 1) == Number(arrSplit[3])) && (dtmBirth.getDate() == Number(arrSplit[4]));
        if (!bGoodDay) {
            layer.msg('输入的身份证号里出生日期不对！');
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
            layer.msg(dtmBirth.getYear());
            layer.msg(arrSplit[2]);
            layer.msg('输入的身份证号里出生日期不对！');
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
                layer.msg('18位身份证的校验码不正确！最后一位建议为：' + valnum);
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
        layer.msg("手机号码有误，请重填");
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
    if (isNaN(nubmer) || nubmer <= 0 || !(/^\d+$/.test(nubmer))) {
        layer.msg("请输入正确的数值,只允许输入整数!");
        return false;
    }
    return true;
}

function checkRate1(nubmer) {
    if (isNaN(nubmer) || nubmer <= 0 || number > 10 || !(/^\d+$/.test(nubmer))) {
        layer.msg("请输入正确的数值,只允许输入1-10的整数!");
        return false;
    }
    return true;
}


/**
 * 获取cookie
 * @param {} name 
 * @returns {} 
 */
function getCookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
    if (arr = document.cookie.match(reg))
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

/**
 * 获取当前时间
 * @param {} seperator1 年月日格式 例如传入 '-' 将得到 2017-06-06
 * @param {} seperator2 
 * @returns {} 
 */
function getNowFormatDate(seperator1) {
    var date = new Date();
    if (typeof (seperator1) == 'undefined') {
        seperator1 = "-";
    }
    var seperator2 = ":";
    var month = date.getMonth() + 1;
    var strDate = date.getDate();
    var strMinutes = date.getMinutes();
    var strgetHours = date.getHours();

    if (month >= 1 && month <= 9) {
        month = "0" + month;
    }
    if (strDate >= 0 && strDate <= 9) {
        strDate = "0" + strDate;
    }
    if (strMinutes >= 0 && strMinutes <= 9) {
        strMinutes = "0" + strMinutes;
    }
    if (strgetHours >= 0 && strgetHours <= 9) {
        strgetHours = "0" + strgetHours;
    }
    var currentdate = date.getFullYear() + seperator1 + month + seperator1 + strDate
        + " " + date.getHours() + seperator2 + strMinutes
        + seperator2 + date.getSeconds();
    if (seperator1 == "月") {
        currentdate = month + '月' + strDate + '日' + "  " + strgetHours + seperator2 + strMinutes;
    }
    return currentdate;
}
/**
 * 时间对比（第一个时间是否大于第二个时间）
 * @param {} date1 
 * @param {} date2 
 * @returns {} 
 */
function DateCompare(date1, date2) {
    var oDate1 = new Date(date1);
    var oDate2 = new Date(date2);
    if (oDate1.getTime() > oDate2.getTime()) {
        return true;
    } else {
        return false;
    }
}

function changeDateFormat(cellval) {
    if (cellval == null) {
        return "";
    }
    var date = new Date(parseInt(cellval.replace("/Date(", "").replace(")/", ""), 10));
    var month = date.getMonth() + 1 < 10 ? '0' + (date.getMonth() + 1) : date.getMonth() + 1;
    var currentDate = date.getDate() < 10 ? '0' + date.getDate() : date.getDate();
    return date.getFullYear() + '-' + month + '-' + currentDate;
};

//数组包含
Array.prototype.contains = function (needle) {
    for (i in this) {
        if (this[i] == needle) return true;
    }
    return false;
}
//数据移除
Array.prototype.removeByValue = function (val) {
    for (var i = 0; i < this.length; i++) {
        if (this[i] == val) {
            this.splice(i, 1);
            break;
        }
    }
}
//数组去空值
Array.prototype.notempty = function () {
    return this.filter(t => t != undefined && t !== null && t !== "");
}


//数组转成字符串
function arrayToString(arr, separator) {
    if (!separator) separator = "&";//separator为null则默认为空
    return arr.join(separator);
}


//checkbox 选中项值
function checkBox(e) {
    var ele = document.getElementsByName(e);
    var sss = [];
    for (k in ele) {
        if (ele[k].checked) {
            sss.push(ele[k].value);
        }
    }
    return sss;
}