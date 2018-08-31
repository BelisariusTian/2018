'use strict';

var tool = {
    config: {
        domain: "http://" + location.host
    },
    checkWXUser: function checkWXUser() {
        //检查微信用户是否授权
        if (typeof store.get("token") == "undefined") {
            var token = tool.getHashParameter("token");
            if (typeof token == "undefined") {
                store.set("oauthUrl", location.href.split('#')[0]);
                var url = location.origin + "/OAuth2?goUrl=" + encodeURI(location.href.split('#')[0]);
                location.href = url;
                return;
            }
            store.set("token", token);
            location.href = store.get("oauthUrl");
        }
    },
    getHashParameter: function getHashParameter(key) {
        //获得单个哈希参数
        var params = tool.getHashParameters();
        return params[key];
    },
    getHashParameters: function getHashParameters() {
        //获得多个哈希参数
        var arr = (location.hash || "").replace(/^\#/, '').split("&");
        var params = {};
        for (var i = 0; i < arr.length; i++) {
            var data = arr[i].split("=");
            if (data.length == 2) {
                params[data[0]] = data[1];
            }
        }
        return params;
    },
    initPicker: function initPicker() {
        //初始化picker
        picker.getIndustryData(), //绑定所属行业的数据
            picker.getCityData(); //绑定城市数据
        picker.getAreaData(); //绑定地区数据

        $("#area_code_sel").picker({
            title: "选择国家或地区",
            cols: [{
                textAlign: "center",
                values: ["+86", "+852", "+853", "+886"],
                displayValues: ["中国(+86)", "中国香港(+852)", "中国澳门(+853)", "中国台湾(+886)"],
                onChange: function onChange(v, c) {
                    // $(a.params.vAreaCode).val("海外" === c ? "0" : c),
                    //     "+86" === c ? $(a.params.vTogglePanel).show() : ($(a.params.vTogglePanel).hide(),
                    //         $(a.params.vImgCode).val(""), $(a.params.vCode).val(""))
                }
            }]
        });
    },
    init: function init() {
        //初始化
        tool.isWeiXin();
        tool.checkWXUser();
        tool.setToken();
        tool.checkguanzhu();
    },
    QS: function QS(name) {
        //获取url参数
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var reg_rewrite = new RegExp("(^|/)" + name + "/([^/]*)(/|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        var q = window.location.pathname.substr(1).match(reg_rewrite);
        if (r != null) {
            return unescape(r[2]);
        } else if (q != null) {
            return unescape(q[2]);
        } else {
            return null;
        }
    },
    checkPhone: function checkPhone(phone) {
        //检测手机
        if (!/^1(3|4|5|7|8)\d{9}$/.test(phone.trim())) {
            alert("手机号码不正确");
            return false;
        }
        return true;
    },
    getverifyCode: function getverifyCode(phone) {
        //获取短信验证码
        /* ajax 请求短信接口，拿到返回来的验证码，与用户输入的进行对比*/
        var params = {
            phone: phone.trim(),
            token: store.get("token")
        };
        $.ajax({
            type: "POST", //
            dataType: "json", //
            url: "/WeChat/UAR/GetSMSCode", //url
            data: params,
            success: function success(result) {
                layer.msg(result.msg);
            },
            error: function error(e) {
                layer.msg("网络异常！");
                return false;
            }
        });
    },
    checkverifyCode: function checkverifyCode(phone, code) {
        var flag = false;
        var params = {
            phone: phone.trim(),
            code: code.trim(),
            token: store.get("token")
        };
        $.ajax({
            type: "POST", //
            async: false,
            dataType: "json", //
            url: "/WeChat/UAR/VerificationSMSCode", //url
            data: params,
            success: function success(result) {
                if (result.code == 0) {
                    return true;
                } else {
                    return false;
                }
            },
            error: function error(e) {
                layer.msg("网络异常！");
                return false;
            }
        });
    },
    isWeiXin: function isWeiXin() {
        //判断是否是微信浏览器打开
        var ua = window.navigator.userAgent.toLowerCase();
        if (ua.match(/MicroMessenger/i) == 'micromessenger') {
            return true;
        } else {
            return false;
        }
    },
    checkEmail: function checkEmail(email) {
        var re = /^(\w-*\.*)+@(\w-?)+(\.\w{2,})+$/;
        if (re.test(email.trim())) {
            return true;
        } else {
            return false;
        }
    },
    checkIDCard: function checkIDCard(num) {
        num = num.toUpperCase().trim(); //身份证号码为15位或者18位，15位时全为数字，18位前17位为数字，最后一位是校验位，可能为数字或字符X。       
        if (!/(^\d{15}$)|(^\d{17}([0-9]|X)$)/.test(num)) {
            layer.msg('输入的身份证号长度不对，或者号码不符合规定！\n15位号码应全为数字，18位号码末位可以为数字或X。');
            return false;
        } //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。
        //下面分别分析出生日期和校验位
        var len, re; len = num.length; if (len == 15) {
            re = new RegExp(/^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})$/);
            var arrSplit = num.match(re); //检查生日日期是否正确
            var dtmBirth = new Date('19' + arrSplit[2] + '/' + arrSplit[3] + '/' + arrSplit[4]);
            var bGoodDay; bGoodDay = dtmBirth.getYear() == Number(arrSplit[2]) && dtmBirth.getMonth() + 1 == Number(arrSplit[3]) && dtmBirth.getDate() == Number(arrSplit[4]);
            if (!bGoodDay) {
                layer.msg('输入的身份证号里出生日期不对！');
                return false;
            } else {
                //将15位身份证转成18位 //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。       
                var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
                var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
                var nTemp = 0,
                    i;
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
            var arrSplit = num.match(re); //检查生日日期是否正确
            var dtmBirth = new Date(arrSplit[2] + "/" + arrSplit[3] + "/" + arrSplit[4]);
            var bGoodDay; bGoodDay = dtmBirth.getFullYear() == Number(arrSplit[2]) && dtmBirth.getMonth() + 1 == Number(arrSplit[3]) && dtmBirth.getDate() == Number(arrSplit[4]);
            if (!bGoodDay) {
                layer.msg(dtmBirth.getYear());
                layer.msg(arrSplit[2]);
                layer.msg('输入的身份证号里出生日期不对!');
                return false;
            } else {
                //检验18位身份证的校验码是否正确。 //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。
                var valnum;
                var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
                var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
                var nTemp = 0,
                    i;
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
    },
    getWXConfig: function getWXConfig() {
        return new Promise(function (resolve, reject) {
            var d = typeof layui.jquery == "undefined" ? $ : layui.jquery;
            d.post('/OAuth2/GetWXConfig', function (data) {
                resolve(wxconfig(data.data));
                wx.error(function (res) {
                    reject(JSON.stringify(res));
                    // config信息验证失败会执行error函数，如签名过期导致验证失败，具体错误信息可以打开config的debug模式查看，也可以在返回的res参数中查看，对于SPA可以在这里更新签名。  
                });
            });
        });
    },
    wxShare: function wxShare(title, link, desc, imgUrl, type, dataUrl) {
        tool.getWXConfig().then(function (data) {
            wx.ready(function () {
                wx.onMenuShareQZone({
                    title: title, // 分享标题  
                    link: decodeURIComponent(link.split('#')[0]), // 分享链接  
                    imgUrl: imgUrl || location.origin + "/images/mcac_logo1.png", // 分享图标  
                    desc: desc,
                    success: function success(res) {
                        // 用户确认分享后执行的回调函数  
                    },
                    cancel: function cancel(res) {
                        // 用户取消分享后执行的回调函数  
                    }
                });
                wx.onMenuShareWeibo({
                    title: title, // 分享标题  
                    link: decodeURIComponent(link.split('#')[0]), // 分享链接  
                    imgUrl: imgUrl || location.origin + "/images/mcac_logo1.png", // 分享图标  
                    desc: desc,
                    success: function success(res) {
                        // 用户确认分享后执行的回调函数  
                    },
                    cancel: function cancel(res) {
                        // 用户取消分享后执行的回调函数  
                    }
                });
                wx.onMenuShareQQ({
                    title: title, // 分享标题  
                    link: decodeURIComponent(link.split('#')[0]), // 分享链接  
                    imgUrl: imgUrl || location.origin + "/images/mcac_logo1.png", // 分享图标  
                    desc: desc,
                    success: function success(res) {
                        // 用户确认分享后执行的回调函数  
                    },
                    cancel: function cancel(res) {
                        // 用户取消分享后执行的回调函数  
                    }
                });
                wx.onMenuShareAppMessage({
                    title: title, // 分享标题
                    desc: desc, // 分享描述
                    link: decodeURIComponent(link.split('#')[0]), // 分享链接
                    imgUrl: imgUrl || location.origin + "/images/mcac_logo1.png", // 分享图标
                    type: type || "", // 分享类型,music、video或link，不填默认为link
                    dataUrl: dataUrl || "", // 如果type是music或video，则要提供数据链接，默认为空
                    success: function success() { },
                    cancel: function cancel() {
                        // 用户取消分享后执行的回调函数
                    }
                });
                wx.onMenuShareTimeline({
                    title: title, // 分享标题  
                    link: decodeURIComponent(link.split('#')[0]), // 分享链接  
                    desc: desc,
                    imgUrl: imgUrl || location.origin + "/images/mcac_logo1.png", // 分享图标  
                    success: function success(res) {
                        // 用户确认分享后执行的回调函数  
                    },
                    cancel: function cancel(res) {
                        // 用户取消分享后执行的回调函数  
                    }
                });
            });
        }).catch(function (error) {
            //alert(error);
        });
    },
    oauth2: function oauth2() {
        store.set("oauthUrl", location.href.split('#')[0]);
        var url = location.origin + "/OAuth2?goUrl=" + encodeURI(location.href.split('#')[0]);
        location.href = url;
    },
    setToken: function setToken() {
        var token = tool.getHashParameter("token");
        if (token && typeof token != "undefined" && JSON.stringify(token) != "{}") {
            store.set("token", token);
            location.href = store.get("oauthUrl");
        }
    },
    showGZWE: function showGZWE() {
        //页面层-自定义
        layer.open({
            type: 1,
            title: false,
            closeBtn: 0,
            shadeClose: true,
            skin: 'yourclass',
            content: $('#gzwe').html()
        });
    },
    checkguanzhu: function checkguanzhu() {
        var d = typeof layui.jquery == "undefined" ? $ : layui.jquery;
        d.ajax({
            url: '/WeChat/UserCenter/GetUserInfo',
            data: { token: store.get("token") },
            datatype: 'JSON',
            type: 'GET',
            async: false,
            success: function (result) {
                if (result.code == -8) {
                    tool.oauth2();
                    return;
                }
                if (result.data.state != 1) {
                    var rq = (result.data.qr_code != null || result.data.qr_code != "") ? result.data.qr_code : "/images/qrcode_for_gh.jpg";
                    d(d('body')[0]).after('<div style="display: none" id="gzwe"><div><span class="gzwo">MCAC智能路由器</span><img style="margin: 0 auto; display: block; width: 60%" src="/images/qrcode_for_gh.jpg" /><span class="gzts">长按识别或扫描二维码关注我们</span></div></div><div class="gzgzh-bottom" onclick="tool.showGZWE()" style="display: flex;"><span class="masked">关注公众号保持联系→→</span><a href="javascript:;"><i>点击关注</i></a></div>')
                    
                    //alert('<div><span class="gzwo">MCAC智能路由器</span><img style="margin: 0 auto; display: block; width: 60 % " src=' + rq + ' /><span class="gzts">长按识别或扫描二维码关注我们</span></div>');
                }
            },
            error: function () {
                if (store.get('token')) {
                    layer.msg('个人信息获取失败');
                } 
            }
        })
    },
      showGZWE: function showGZWE() {
        //页面层-自定义
        layer.open({
            type: 1,
            title: false,
            closeBtn: 0,
            shadeClose: true,
            skin: 'yourclass',
            content: $('#gzwe').html()
        });
    },
};
