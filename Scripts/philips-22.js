﻿/** Copyright 2012 Itnode, Version No 1.4.4 **/
(function ($) {
    $.fn.inShare = function (options) {
        var defaults = {
            //domain: "http://widget.itnode.cn/",
            domain: "http://47.92.147.251/",
            localurl: encodeURIComponent(window.location.href),
            nametxt: { "txtc": "&copy;\u4f18\u4eab\u5ba2" },
            popHeight: 445
        }
        var options = $.extend(defaults, options);
        var mvbg = $("<div>").addClass("in-share-cvbg modal");
        this.each(function () {
            var sharetag = new Array('Sina_weibo', 'Renren', 'Tencent_weibo', 'Kaixin001'); //20141209
            var obj = $(this);
            var siteid = obj.attr('uid');
            var surl = obj.attr("surl");
            var simg = obj.attr("simg");
            var stxt = obj.attr("stxt");
            var stitle = null;
            var scount = obj.attr("scount");
            obj.html('loading...');
            //构造体
            var ma = $("<ul>").addClass("in-share-custom");
            var mli = $("<li>").addClass("in-share-li");
            var mb = $("<div>").addClass("in-share-box");
            var mc = $("<div>").addClass("in-share-cp").css({ 'zIndex': 100000, 'background': '#fff' }); //弹窗
            var e = $('<iframe style="z-index:999999" frameBorder="0" scrolling="no" hspace="0" width="100%" height="100%" id="in-share-iframe" allowTransparency="true"></iframe>');
            //内部变量
            var pagebigimgs = "";
            var opentype = "";
            var imgflag = 0;
            var findimgflag = 0;
            var adpic = "";
            var adimg = new Image();
            var ccimg = new Image();
            //构造开始
            if ($('link[href$="H5share.min.css"]').length == 0) {//构造样式
                //var css_href = options.domain + 'share.css';
                var css_href = options.domain+'Content/H5share.min.css';
                var styleTag = document.createElement("link");
                styleTag.setAttribute('type', 'text/css');
                styleTag.setAttribute('rel', 'stylesheet');
                styleTag.setAttribute('href', css_href);
                $("body")[0].appendChild(styleTag);
            }
            if ($('link[href$="iconfont.css"]').length == 0) {//构造样式
                //var css_href = options.domain + 'share.css';
                var css_href = options.domain+'fonts/iconfont.css';
                var styleTag = document.createElement("link");
                styleTag.setAttribute('type', 'text/css');
                styleTag.setAttribute('rel', 'stylesheet');
                styleTag.setAttribute('href', css_href);
                $("body")[0].appendChild(styleTag);
            }
            //分享按钮
            $.getJSON(options.domain + "share/getsiteinfo", { 'siteid': siteid, 'time': new Date().getTime(), 'jsoncallback': '?' },
                function (data) {
                    //pageinfo.sitename = data.sitename;
                    if(data.isweibo) {
                        $("<a>").attr("href", "javascript:void(0)").attr("title", '').attr("platform", '').attr("platformname", '').addClass("in-share-icon1 iconfont sina icon-xinlang-copy").appendTo(mli);
                        $("<div>").addClass("in-share-count").appendTo(mli);
                        ma.append(mli);
                    }
                    if(data.isweixin) {
                        $("<a>").attr("href", "javascript:void(0)").attr("title", '').attr("platform", '').attr("platformname", '').addClass("in-share-icon1 iconfont weixin icon-pengyouquan1").appendTo(mli);
                        $("<div>").addClass("in-share-count").appendTo(mli);
                        ma.append(mli);
                    }
                    if(data.sitetype == 1){
                        surl = options.localurl;
                        simg = data.sharepic1+','+data.sharepic2+','+data.sharepic3+','+data.sharepic4+','+data.sharepic5;
                        stxt = data.sharecontent;
                        stitle = data.sitename;
                    }
                    //初始化添加事件
                    obj.find(".in-share-li a").not(".in-share-count").bind("click", function () {

                        var vw = '600px';
                        var vh = '435px';
                        var tb = options.domain + 'Share/Index';
                        if ($(this).hasClass('weixin')) { vw = '250px'; vh = '240px'; tb = options.domain + 'qrcode.html'; }
                        if (isMobile() && !$(this).hasClass('weixin')) { vw = '100%'; vh = '350px'; }
                        showshareboxIframe($(this).attr("platform"), $(this).attr("platformname"), surl, simg, stxt,stitle, vh, vw, options.domain, siteid, tb);

                        mvbg.fadeIn();


                    });
            });
            /*$.each(options.hotjson["icons"], function (index, value) {
                $("<a>").attr("href", "javascript:void(0)").attr("title", value.title).attr("platform", value.other).attr("platformname", value.name).addClass("in-share-icon1 iconfont " + value.style).appendTo(mli);
                $("<div>").addClass("in-share-count").appendTo(mli);
                ma.append(mli);
            });*/
            obj.empty();
            //ma.append(mli).append($("<li>").addClass("in-share-count").append($("<a>").attr("href", "javascript:void(0)").attr("class", 'num'))).appendTo(obj);
            ma.appendTo(obj);

            if ($('body').find('.in-share-cp').length == 0) {//判断是否已经存在
                mc.append($("<div>").addClass("in-share-cpbd").append($("<a>").attr("href", "javascript:void(0)").addClass("in-s-close").bind('click', function () {
                    mc.hide(); mvbg.hide();
                    $("#in-share-iframe").attr("src", "");
                })).append($("<div>").attr("class", "in-share-iframeloading")).append(e).append($("<div>").addClass("in-s-foot").html())).hide().appendTo($("body"));
                mvbg.bind('click', function () {
                    $(this).hide(); mc.hide();
                }).hide().appendTo($("body")[0]);
            }
            obj.find(".in-share-count").html('<a href="javascript:void(0)" class="num">0</a>');

            obj.find("a").bind("focus", function () {
                if (this.blur) { this.blur() };
            });
            obj.find(".in-share-ad,.in-share-more").hide(); //初始化隐藏
            //导入数据
            if (scount == "false") {
                obj.find(".in-share-count").hide();
            }
            var url = "";
            if (surl != null) {
                url = surl;
            } else {
                url = options.localurl;
            }
            $.getJSON(options.domain + "Share/GetURLCount", { 'url': url, 'siteid': siteid, 'time': new Date().getTime(), 'jsoncallback': '?' },
                function (data) {
                    obj.find(".in-share-count").first().html('<a href="javascript:void(0)" class="num">' + data.sinacount + '</a>');
                    obj.find(".in-share-count").last().html('<a href="javascript:void(0)" class="num">' + data.weibocount + '</a>');
            });
            
            $(window).bind("resize scroll", function () { showshareboxIframePosition(mc) });


            //alert(getUrlParam("yxkfrom"));
            if (getUrlParam("yxkfrom") != null && getUrlParam("yxksnsid") != null) {
                var yxkfrom = getUrlParam("yxkfrom");
                var yxksnsid = getUrlParam("yxksnsid");
                var pagetitle = encodeURI(document.title);
                allurl = options.localurl;
                var counter = 0;
                var url = allurl.split('?')[0];
                var amp = '';
                var thisqs = allurl.split('?')[1];
                if (thisqs) {
                    var pairs = thisqs.split('&');
                    for (i = 0; i < pairs.length; i++) {
                        var pair = pairs[i].split('=');
                        if (pair[0] != "yxkfrom" && pair[0] != "yxksnsid") {
                            amp = (counter) ? '&' : '';
                            url = url + amp + pair[0] + '=' + pair[1];
                            counter++;
                        }
                    }
                }
                //alert(url);
                $.getJSON(options.domain + "traffic.aspx?siteid=" + siteid + "&url=" + url + "&yxkfrom=" + yxkfrom + "&yxksnsid=" + yxksnsid + "&pagetitle=" + pagetitle + "&jsoncallback=?",
                    function (data) {

                    });

            }

        });
    };
})(jQuery);
var havecheck = 0;
var pagebigimgs = "";
var imgflag = 0;
// 是否为移动端
function isMobile() {
    var sUserAgent = navigator.userAgent.toLowerCase();
    // var bIsIpad = sUserAgent.match(/ipad/i) == "ipad";
    var bIsIphoneOs = sUserAgent.match(/iphone os/i) == "iphone os";
    var bIsMidp = sUserAgent.match(/midp/i) == "midp";
    var bIsUc7 = sUserAgent.match(/rv:1.2.3.4/i) == "rv:1.2.3.4";
    var bIsUc = sUserAgent.match(/ucweb/i) == "ucweb";
    var bIsNokia = sUserAgent.match(/nokia/i) == "nokia";
    var bIsAndroid = sUserAgent.match(/android/i) == "android";
    var bIsCE = sUserAgent.match(/windows ce/i) == "windows ce";
    var bIsWM = sUserAgent.match(/windows mobile/i) == "windows mobile";

    /*document.writeln("您的浏览设备为：");*/
    if (bIsIphoneOs || bIsMidp || bIsUc7 || bIsUc || bIsNokia || bIsAndroid || bIsCE || bIsWM) {
        /* 链接到不同的网址  这个是手机的 */
        return true;

    } else {
        /* 链接到不同的网址  这个是PC的 */
        return false;
    }
}
function showshareboxIframePosition(pop) {
    var pleft = ($(window).width() - pop.width()) / 2 + $(window).scrollLeft();
    var ptop = ($(window).height() - pop.height()) / 2 + $(window).scrollTop();
    pop.css("position", "absolute")
        .css("left", pleft < 0 ? 0 : pleft)
        .css("top", ptop < 0 ? 0 : ptop);
}
//alert(pagebigimgs);
function getAbsoluteUrl(url) {
    var img = new Image();
    img.src = url; // 设置相对路径给Image, 此时会发送出请求
    url = img.src; // 此时相对路径已经变成绝对路径
    img.src = null; // 取消请求
    return url;
}
function getUrlParam(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
    var r = window.location.search.substr(1).match(reg);  //匹配目标参数
    if (r != null) return unescape(r[2]); return null; //返回参数值
}
function showshareboxIframe(i, iname, surl, simg, stxt,stitle, sheight, swidth, sdomain, siteid, tb) {
    var adpic = '';
    if (!havecheck) {
        $('img').not(".in-share-ad img").each(function (index) {
            if ($(this).width() >= 120 && imgflag <= 4) {
                var imgrate = $(this).width() / $(this).height();
                pagebigimgs = pagebigimgs + ((imgflag) ? ";" : "") + encodeURIComponent(getAbsoluteUrl($(this).attr('src'))) + "," + imgrate;
                imgflag++;
            }
        });

        if ($(".in-share-ad img").length) {
            for (i = 0; i < $(".in-share-ad img").length; i++) {
                var aimg = $(".in-share-ad img").eq(i);
                var yyimg = new Image();
                yyimg.src = aimg.attr('src');
                adpic = adpic + ((i) ? ";" : "") + yyimg.src + "," + yyimg.width / yyimg.height;
                yyimg = null;
            }
            pagebigimgs = adpic + ((pagebigimgs) ? ";" : "") + pagebigimgs;
        }
        havecheck++;
    }
    $(".in-share-iframeloading").show();
    $("#in-share-iframe").attr("height", sheight);
    $("#in-share-iframe").attr("width", swidth);
    var pagetitle = (stitle!=null)?encodeURI(stitle):encodeURI(document.title);
    var pagedisc = encodeURI($('meta[name=description]').attr("content"));
    var url = "";
    var rurl = "";
    if (surl != null) {
        url = surl;
    } else {
        url = encodeURIComponent(window.location.href);
    };
    rurl = encodeURIComponent(window.location.href);
    //var simg="";
    if (simg != null) {
        simg = decodeURIComponent(simg);
        var ssimg = new Image();
        ssimg.src = simg;
        simg = ssimg.src + "," + ssimg.width / ssimg.height;

    } else {
        simg = pagebigimgs;
    }
    //var stxt="";
    if (stxt == null) {
        stxt = pagedisc;
    }
    $("#in-share-iframe").attr("src", tb + "?siteid=" + siteid + "&snsname=" + iname + "&url=" + url + "&rurl=" + rurl + "&snsid=" + i + "&shareimg=" + simg + "&pagetitle=" + pagetitle + "&pagedesc=" + stxt);
    $("#in-share-iframe").bind("load", function () {
        $(".in-share-iframeloading").hide();
        //in-share-iframe").show();
        var $test = $("#in-share-iframe").contents().find(".qr_content");
        if (isMobile()) { $test.prepend("<h2>长按二维码保存至相册</h2>") }
    });
    showshareboxIframePosition($(".in-share-cp"));
    //b.hide();
    $(".in-share-cp").fadeIn();
}
jQuery(document).ready(function () { jQuery('.a-share').inShare(); })