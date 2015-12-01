function time(){								//現時刻の位置
	date=(new Date()).getDate();
	hour=(new Date()).getHours();
	min =(new Date()).getMinutes();
	var t=0;if(hour<basehour){t=24;}
	return ((hour-basehour+t)*60+min)*oneminpx;
}

function line(){								//現時刻のライン表示
	$("#line").offset({top:time()+$("#tvguide").offset().top});
}

function minute(){
	line();
	if(date<10){date="0"+ date;}
	if(hour<10){hour="0"+hour;}
	if(min<10){min="0"+ min;}
	$("#line").text(min);							//ラインに分を表示
	var endtime="."+date+"-"+hour+"-"+min;
	$(endtime+" .end").addClass("mask");					//終了番組にマスク
	$(endtime+" .reserve").hide();						//録画予約を非表示
}
setInterval("minute()", 1000);

function jump(){
	$("#wrap_tv_3").animate({scrollTop:time()-marginmin*oneminpx}, 550, "swing");
}

$(function(){
	function move(){							//ヘッダの位置
		$("#head").offset({left:$("#tvguide").offset().left});		//チャンネル名
		$("#hour").offset({top:$("#tvguide").offset().top});		//時間帯
	}

	$(window).on("load resize", function(){					//ヘッダの微調整
		$(".jump").css("top",-1*$(".date .jump").height());
		$("#head").css("padding-top",$("#tvheader").height());
		$("#wrap_tv_1").css("padding-top",$("#tvheader").height()+$("#head").height());
		$("#wrap_tv_2").css("padding-left",$("#hour").width());
		move();
	});
	$("#wrap_tv_3").scroll(function(){					//スクロール連動
		move();
		line();
	});

	$("[name=hide]").change(function(){
		var val=$(this).val();
		if($(this).prop('checked')){
			$(".id-"+val).hide();
		}else{
			$(".id-"+val).show();
		}
	});


	$("[id^=g-]").click(function(){
		if($(this).hasClass("active")){
			$("[id^=g-]").removeClass("active");
			$(".content").show();
			$(".end").show();
			$(".cell").removeClass("nothing");
		}else{
			$("[id^=g-]").removeClass("active");
			$(this).addClass("active");
			$(".content").hide();
			$(".end").hide();
			$(".cell").addClass("nothing");
			$(".content."+$(this).data("value")).show().parent(".cell").removeClass("nothing");
		}
	});
	

	$(".cell").click(function(e){						//番組詳細表示
		if(!$(e.target).is("a,label,.nothing")){
			$(".popup").hide();
			if($(this).hasClass("clicked")){
				$(".cell").removeClass("clicked");
			}else{
				$(".cell").removeClass("clicked");
				$(this).addClass("clicked");
				$(".clicked > .popup").show();
			}
		}
	});


	if (hover==true){
		$(".cell").hover(						//マウスホバーで番組詳細表示
			function(){
				$(".popup").hide();
				$(this).addClass("clicked");
				$(".clicked > .popup").show();
			},function(){
				$(".popup").hide();
				$(this).removeClass("clicked");
		});
	}

	$(".jumpnow").click(function(){jump();});				//現時刻に移動

	function scroll(time){							//指定時間の位置
		var position=((time-1)*60-30)*oneminpx;
		$("#wrap_tv_3").animate({scrollTop:position}, 550, "swing");
	}

	$(".time a").click(function(){						//指定時間に移動
		scroll($(this).attr("value"));
	});
	$("[name=time]").change(function(){					//指定時間に移動(スマホ用)
		scroll($(this).val());
	});


	$(".pull").hover(							//サブメニュー表示
		function(){
			$(this).addClass("hover");
			$(".hover .jump").css("top",$("#submenu").height());
		}
		,function(){
			$(".hover .jump").css("top",-1*$(".date .jump").height());
			$(this).removeClass("hover");
		}
	);

	$("#menu_ico").click(function(){
		$(".sidemenu").addClass("visible");
		$(".obfuscator").addClass("visible");
	});
	$(".obfuscator").click(function(){
		$(".sidemenu").removeClass("visible");
		$(".obfuscator").removeClass("visible");
	});

	$(".toggle").click(function(){
		if($(this).hasClass("clicked")){
			$(this).removeClass("clicked");
		}else{
			$(this).addClass("clicked");
		}
	});

	$(".reserve label").click(function(){
		var a=this;
		var url;
		if ($(this).data("reserve")>0){
			url="api/reservetoggle?id="+$(this).data("reserve");
		}else if ($(this).data("eid")>0){
			url="api/oneclickadd?onid="+$(this).data("onid")+"&tsid="+$(this).data("tsid")+"&sid="+$(this).data("sid")+"&eid="+$(this).data("eid");
		}
		if (url){
			var xhr= new XMLHttpRequest();
			xhr.open("GET",url);
			xhr.send();
			xhr.onload=function(ev){
				var xml=xhr.responseXML;
				if($(xml).find('success').length>0){
					var start=$(xml).find('start').text();
					var recmode=$(xml).find('recmode').text();
					var overlapmode=$(xml).find('overlapmode').text();
					var id=$(xml).find('reserveid').text();

					var parents=$(a).parents(".cell")
					$(a).data("reserve", id);
					$(parents).children().removeClass('disable partially shortage reserved');
					if($(parents).find('.recable').length==0){
						$(parents).find('.start').append('<span class="mark recable"></span>');
					}
					if (recmode==5){
						$(a).html('有効');
						$(parents).children().not('.end').addClass('disable').find('.recable').addClass('no').html('無');
					}else{
						$(a).html('無効');
						$(parents).find('.recable').removeClass('no');
						if(overlapmode==1){
							$(parents).children().not('.end').addClass('partially').find('.recable').html('部');
						}else if(overlapmode==2){
							$(parents).children().not('.end').addClass('shortage').find('.recable').html('不');
						}else{
							$(parents).children().not('.end').addClass('reserved').find('.recable').html('録');
						}
					}
					$(".popup").hide();
					$(".cell").removeClass("clicked");
				}else{
					alert($(xml).find('err').text());
				}
			};
		}
	});

});
