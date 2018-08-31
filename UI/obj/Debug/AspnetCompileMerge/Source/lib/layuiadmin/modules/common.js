/** layuiAdmin.std-v1.0.0-beta8 LPPL License By http://www.layui.com/admin/ */;
layui.define(function (e) {
    var i = (layui.$, layui.layer, layui.laytpl, layui.setter, layui.view, layui.admin);
    i.events.logout = function () {
        i.req({ url: "/home/logout", type: "get", data: { token: localStorage.token.replace('"', '').replace('"', '') }, done: function (e) { i.exit(function () { location.href = "/admin/login.html" }) } })
    }, e("common", {})
});