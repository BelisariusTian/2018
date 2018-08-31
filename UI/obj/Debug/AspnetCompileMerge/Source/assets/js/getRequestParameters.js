function getParameters() {

    //返回当前 URL 的查询部分（问号 ? 之后的部分）。
    var urlParameters = location.search;
    //声明并初始化接收请求参数的对象
    var requestParameters = new Object();
    //如果该求青中有请求的参数，则获取请求的参数，否则打印提示此请求没有请求的参数
    if (urlParameters.indexOf('?') != -1) {
        //获取请求参数的字符串
        var parameters = decodeURI(urlParameters.substr(1));
        //将请求的参数以&分割中字符串数组
        parameterArray = parameters.split('&');

        //循环遍历，将请求的参数封装到请求参数的对象之中
        for (var i = 0; i < parameterArray.length; i++) {
            requestParameters[parameterArray[i].split('=')[0]] = (parameterArray[i].split('=')[1]);
        }
        console.info('theRequest is =====', requestParameters);
    }
    else {
        console.info('There is no request parameters');
    }
    return requestParameters;
}