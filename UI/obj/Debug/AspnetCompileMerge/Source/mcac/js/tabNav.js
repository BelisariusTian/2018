

function tabNav(name, cursel, n) {
    for (i = 1; i <= n; i++) {
        var menu = document.getElementById(name + i);
        var con = document.getElementById("content_" + name + "_" + i);
        menu.className = i == cursel ? "hover" : "";
        con.style.display = i == cursel ? "block" : "none";
    }
}

