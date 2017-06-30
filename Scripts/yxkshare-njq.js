(function($){
	$.fn.inShare = function(options){
		var defaults = {
			domain:"http://widget.itnode.cn/",
			localurl : encodeURIComponent(window.location.href),
			siteid : $("#a-share").attr("uid"),
            surl : $("#a-share").attr("surl"),
            simg : $("#a-share").attr("simg"),
            stxt : $("#a-share").attr("stxt"),
            scount : $("#a-share").attr("scount"),
			nametxt:{"txtc":"&copy;\u4f18\u4eab\u5ba2"},
			popHeight :445,
			hotjson:{
				"icons":[
					{"name":"新浪微博","title":"分享到新浪微博","style":"sinaminiblog","other":1},
					{"name":"人人网","title":"分享到人人网","style":"renren","other":2},
					{"name":"腾讯微博","title":"分享到腾讯微博","style":"qqminiblog","other":3},
                    {"name":"开心网","title":"分享到开心网","style":"kaixin","other":4}
				],
				"bigicons":[
					{"name":"新浪微博","title":"分享到新浪微博","style":"sinaminiblog","other":1},
					{"name":"人人网","title":"分享到人人网","style":"renren","other":2},
					{"name":"腾讯微博","title":"分享到腾讯微博","style":"qqminiblog","other":3},
					{"name":"开心网","title":"分享到开心网","style":"kaixin","other":4}
				]
			}
		}
		var options = $.extend(defaults, options);
		this.each(function(){
			var obj = $(this);
			obj.html('loading...');
			var a = $("<div>").attr("id","in-share-custom");
			var b = $("<div>").attr("id","in-share-box");
			var c = $("<div>").addClass("in-share-morebox");
			var d = $("<div>").attr("id","in-share-cp");
			var e = $('<iframe style="z-index:999999" frameBorder="0" scrolling="no" hspace="0" width="100%" height="100%" id="in-share-iframe" allowTransparency="true"></iframe>');
			var pagebigimgs = "";
			var opentype = "";
			var imgflag = 0;
			var findimgflag = 0;
			var adpic = "";
			var adimg = new Image();
			var ccimg = new Image();
			//if ($("link[href$=share.css]").length == 0){
			var css_href = options.domain + 'share.css';
			var styleTag = document.createElement("link");
			styleTag.setAttribute('type', 'text/css');
			styleTag.setAttribute('rel', 'stylesheet');
			styleTag.setAttribute('href', css_href);
			$("body")[0].appendChild(styleTag);
			//}
			$("a").bind("focus",function() {
				if(this.blur) {this.blur()};
			});
			//$(document.createElement("link")).attr({"rel":"stylesheet","type":"text/css","href":"Css/share.css"}).appendTo("body");
			//alert(options.hotjson["icons"][1]["name"]);
        
			$.each(options.hotjson["icons"], function(index, value){	
                $("<a>").attr("href","javascript:void(0)").attr("title",value.title).attr("platform",value.other).attr("platformname",value.name).addClass("in-share-icon1 in-share-"+value.style).appendTo(a);
            });
			$.each(options.hotjson["bigicons"], function(index, value){
                $("<a>").attr("href","javascript:void(0)").attr("title",value.title).attr("platform",value.other).attr("platformname",value.name).addClass("in-share-icon2").append($("<em>").addClass(value.style)).append($("<span>").html(value.name)).appendTo(c);
            });
			c.wrapInner($("<div>").addClass("in-share-moreinnerbox"));
			obj.empty();
			a.append($("<span>").addClass("in-share-count")).appendTo(obj);
			b.append($("<div>").addClass("in-share-sbox").append($("<a>").attr("href","javascript:void(0)").addClass("in-share-close").bind("click",function(){
				b.hide();
			})).append($("<div>").attr("id","in-share-ad")).append(c)).hide().appendTo(obj);
			d.append($("<div>").addClass("in-share-cpbd").append($("<a>").attr("href","javascript:void(0)").addClass("in-s-close").html("X").bind("click",function(){
				d.hide();
				e.attr("src","");
			})).append($("<div>").attr("class","in-share-iframeloading")).append(e).append($("<div>").addClass("in-s-foot").html(options.nametxt.txtc))).hide().appendTo($("body"));
			$(".in-share-more,#in-share-ad").hide();
            if(options.scount=="false")
            {
                $(".in-share-count").hide();
            }
            var url="";
            if(options.surl!=null)
            {
                url = options.surl;
            }else{
                url = options.localurl;
            }
            
			$.getJSON(options.domain + "config.aspx?time="+new Date().getTime()+"&siteid=" + options.siteid + "&url=" + url + "&jsoncallback=?",
			function (data){
				$.each(data["share"], function(index, value){
					$(".in-share-count").html(value.sharenum);
					if(value.bigpic != ""){
						$("#in-share-ad").append($("<img>").attr("src",value.bigpic).attr("alt",value.bigpicalt));
						if(value.bigpiclink != ""){
							$("#in-share-ad").wrapInner($("<a>").attr("href",value.bigpiclink).attr("target","_blank"));
						}
						$("#in-share-ad").show();
						adimg.src = value.bigpic;
						adpic = "ok";
					}
					if(value.smallpic != ""){
						opentype = (value.opentype != "")?value.opentype:"click";
						ccimg.src = value.smallpic+"?"+new Date().getTime();
						$(ccimg).load(function(){
							$("<a>").addClass("in-share-more").attr("href","javascript:void(0)").attr("title",value.smallpicalt).css("background-image","url("+value.smallpic+")").css("width",ccimg.width).bind(opentype,function(){
								b.show();
							}).show().insertBefore(".in-share-count");ccimg.src = null;
						});
						
					}	
				});
			});
			a.find("a").not(".in-share-more").bind("click",function(){
				showPop($(this).attr("platform"),$(this).attr("platformname"));
			});
			c.find("a").bind("click",function(){
				showPop($(this).attr("platform"),$(this).attr("platformname"));
			});
			$(window).bind("resize scroll",function(){popPosition(d)});
			
			//alert(pagebigimgs);
			function getAbsoluteUrl(url) {
				var img = new Image();
				img.src = url; // 设置相对路径给Image, 此时会发送出请求
				url = img.src; // 此时相对路径已经变成绝对路径
				img.src = null; // 取消请求
				return url;
			}
			function showPop(i,iname){
				if(!findimgflag){
					$('img').not("#in-share-ad img").each(function (index) {
						if ($(this).width() >= 200) {
							var imgrate = $(this).width()/$(this).height();
							pagebigimgs = pagebigimgs + ((imgflag)?";":"")+ getAbsoluteUrl($(this).attr('src')) +","+imgrate;
							imgflag ++;
						}
					});
					if(adpic != ""){
						adpic = adimg.src +","+ adimg.width/adimg.height;
						pagebigimgs = adpic +((pagebigimgs)?";":"") + pagebigimgs;
					}
					findimgflag ++;
				}
				$(".in-share-iframeloading").show();
				$("#in-share-iframe").attr("height",options.popHeight-24);
				$("#in-share-iframe").attr("width",600);
				var pagetitle = encodeURI(document.title);
				var pagedisc = encodeURI($('meta[name=description]').attr("content"));
                var url="";
                if(options.surl!=null)
                {
                    url = options.surl;
                }else{
                    url = options.localurl;
                }
                var simg="";
                if(options.simg!=null)
                {
                    simg = decodeURIComponent(options.simg);
                    var ssimg = new Image();
                    ssimg.src = simg;
                    simg = ssimg.src +","+ ssimg.width/ssimg.height;

                }else{
                    simg = pagebigimgs;
                }

                var stxt="";
                if(options.stxt!=null)
                {
                    stxt = options.stxt;
                }else{
                    stxt = pagedisc;
                }

				e.attr("src",options.domain + "share.aspx?siteid="+options.siteid+"&snsname="+iname+"&url="+url+"&snsid="+i+"&shareimg="+simg+"&pagetitle="+pagetitle+"&pagedesc="+stxt);
				$("#in-share-iframe").bind("load",function(){
					$(".in-share-iframeloading").hide();
					//$("#in-share-iframe").show();
				})
				popPosition(d);
				//b.hide();
				d.fadeIn();
			}
			function getUrlParam(name){
				var reg = new RegExp("(^|&)"+ name +"=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
				var r = window.location.search.substr(1).match(reg);  //匹配目标参数
				if (r!=null) return unescape(r[2]); return null; //返回参数值
			}
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
				if ( thisqs ) {
						var pairs = thisqs.split('&');
						for ( i=0;i<pairs.length;i++ ) {
								var pair = pairs[i].split('=');
								if (pair[0] != "yxkfrom" && pair[0] != "yxksnsid") { 
									amp = (counter) ? '&' : '';
									url = url + amp + pair[0] + '=' + pair[1];
									counter++;
								}
						}
				}
				//alert(url);
				$.getJSON(options.domain + "traffic.aspx?siteid=" + options.siteid + "&url=" + url + "&yxkfrom=" + yxkfrom + "&yxksnsid=" + yxksnsid + "&pagetitle=" + pagetitle + "&jsoncallback=?",
				  function (data) {
					  
				});

			}
			
		});
		function popPosition(pop){
			var pleft = ($(window).width()-pop.width())/2+$(window).scrollLeft();
			var ptop = ($(window).height()-pop.height())/2+$(window).scrollTop();
			pop.css("position","absolute")
			.css("left",pleft<0?0:pleft)
			.css("top",ptop<0?0:ptop);
		}
	};
})(jQuery);
$(document).ready(function(){jQuery('#a-share').inShare();})