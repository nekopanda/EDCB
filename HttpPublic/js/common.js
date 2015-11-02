$(function(){
	$("#menu_ico").click(function(){
		$(".sidemenu").addClass("visible");
		$(".obfuscator").addClass("visible");
	});
	$(".obfuscator").click(function(){
		$(".sidemenu").removeClass("visible");
		$(".obfuscator").removeClass("visible");
	});

	$('tr[data-href]').click(function(e){		//一覧の行をリンクに
		if(!$(e.target).is('.flag,.flag span,.count li')){
			window.location = $(e.target).closest('tr').data('href');
		};
	});
	$('.count[data-href]').click(function(){		//登録数クリックで検索
			window.location = $(this).data('href');
	});

	$('#usedef').change(function(){			//録画マージン
		if($(this).prop('checked')) {
			$(".margin input").prop('disabled', true);
			$(".margin").addClass("disabled");
		}else{
			$(".margin input").prop('disabled', false);
			$(".margin").removeClass("disabled");
		}
	});
	$('#smode').change(function(){			//指定サービス対象データ
		if ($(this).prop('checked')) {
			$(".mode").prop('disabled', true);
			$(".mode").addClass("disabled");
		}else{
			$(".mode").prop('disabled', false);
			$(".mode").removeClass("disabled");
		}
	});

	$('#partial').change(function(){		//部分受信同時出力で詳細表示
		if($(this).prop('checked')) {
			$(".partial").show();
		}else{
			$(".partial").hide();
		}
	});

	$('#content').change(function(){		//大分類ジャンル選択
		var content=$('#content').val();
                $('#content2 option').show();						//一度すべて表示
		if (content!="all"){									//選択ジャンル以外の中分類を
                	$('#content2 option[class != g'+content+']').hide();		//非表示
			$('#content2 option[class != g'+content+']').prop('selected', false);		//非選択
		}
	});
	$('#g_celar').click(function(){			//全ジャンル選択解除
		$('#content2 option').prop('selected', false);
	});

	$('#service').change(function(){		//ネットワーク選択
		var content=$('#service').val();
                $('#service2 option').show();		//一度すべて表示
		if(content!="all"){								//選択ネットワーク以外のサービスを
                	$('#service2 option:not(.'+content+')').hide();				//非表示
			$('#service2 option:not(.'+content+')').prop('selected', false);	//非選択
		}
	});

	$('#image').change(function(){			//EPG予約ネットワーク抽出
		if (!$(this).prop('checked')){
			if ($('#DTV').prop('checked')){
				$("#service2 option.DTV").removeClass("hide");
			}
			if ($('#SEG').prop('checked')){
				$("#service2 option.SEG").removeClass("hide");
			}
			if ($('#BS').prop('checked')){
				$("#service2 option.BS").removeClass("hide");
			}
			if ($('#CS').prop('checked')){
				$("#service2 option.CS").removeClass("hide");
			}
			if ($('#HD').prop('checked')){
				$("#service2 option.HD").removeClass("hide");
			}
			if ($('#S-other').prop('checked')){
				$("#service2 option.S-other").removeClass("hide");
			}
		}else{
			$("#service2 option:not(.image)").addClass("hide");
			$('#service2 option:not(.image)').prop('selected', false);
		}
	});
	$('.network').change(function(){		//検索ネットワーク抽出
		var idname = $(this).attr("id");
		if (!$(this).prop('checked')){
			$('#service2 option.'+idname).addClass("hide");
			$('#service2 option.'+idname).prop('selected', false);
		}else if ($('#image').prop('checked')){
			$('#service2 option.image.'+idname).removeClass("hide");
		}else{
			$('#service2 option.'+idname).removeClass("hide");
		}
	});

	$('#s_all').click(function(){			//表示ネットワークのサービス選択
		var content=$('#service').val();
		if(content=="all"){
		$('#service2 option').prop('selected', true);
		}else{
		$('#service2 option[class *= '+content+']').prop('selected', true);
		}
	});

	$('#select').click(function(){			//表示ネットワークのサービス選択
		$('#service2 option:not(.hide)').prop('selected', true);
	});

	$('#add').click(function(){			//検索でEPG予約追加
		$('#search').attr('method','POST').attr('action','autoaddepginfo.html').submit();
	});

	$(".flag span").click(function(){		//予約一覧等 有効/無効切替
		var a=this;
		var url;
		if ($(this).data("reserve")>0){						//reserveidで
			url="api/reservetoggle?id="+$(this).data("reserve");
		}else if ($(this).data("eid")>0){					//onid,tsid,sid,eidで
			url="api/oneclickadd?onid="+$(this).data("onid")+"&tsid="+$(this).data("tsid")+"&sid="+$(this).data("sid")+"&eid="+$(this).data("eid");
		}
		if (url){
			var xhr= new XMLHttpRequest();
			xhr.open("GET",url);
			xhr.send();
			xhr.onload=function(ev){
				var xml=xhr.responseXML;
				console.log(xml);
				if($(xml).find('success').length>0){
					var start=$(xml).find('start').text();
					var recmode=$(xml).find('recmode').text();
					var overlapmode=$(xml).find('overlapmode').text();
					var id=$(xml).find('reserveid').text();

					//親のtrのclass設定
					$(a).removeClass("addrec");
					$(a).parents("tr").removeClass();
					if(recmode==5){	//無効時
						$(a).parents("tr").addClass("disablerec");
					}else if(overlapmode==1){//チューナー不足
						$(a).parents("tr").addClass("partially");
					}else if(overlapmode==2){//一部録画
						$(a).parents("tr").addClass("shortage");
					}
					
					if (start==1){//番組開始済み
						$(a).removeClass("switch").addClass("rec");	//録画アイコン追加
						if (recmode==5){//無効時
							$(a).addClass("not").data("reserve", id);
						}else{
							$(a).addClass("recording").removeClass("not").data("reserve", 0).data("eid", 0);	//誤爆して無効にしないようにid,eidを0に
						}
					}else{
						$(a).removeClass("rec").addClass("switch");	//スイッチアイコン追加
						if(id){$(a).data("reserve", id);}
						if (recmode==5){
							$(a).removeClass("on");			//無効
						}else{
							$(a).addClass("on");			//有効
						}
					}
				}else{
					alert($(xml).find('err').text());
				}
			};
		}
	});

	$("[name=presetID]").change(function(){
		var id;
 		var url;
 		var tag='recpresetinfo'
 		var tagid='id'
		if ($(this).val()!=65535){
			url="api/EnumRecPreset";
		        id=$(this).val();
		}else{						//予約時
			if($("[name=id]").length>0){
				url="api/EnumReserveInfo";
				id=$("[name=id]").val();
				tag='reserveinfo'
				tagid='ID'
			}else{
				url="api/EnumAutoAdd";	
				id=$("form").data("autoid");
			}
		}
		var xhr= new XMLHttpRequest();
		xhr.open("GET",url);
		xhr.send();
		xhr.onload=function(ev){
			var xml=xhr.responseXML;
			$(xml).find(tag).each(function(){
				if($(this).find(tagid).text()==id){
					$("[name=recMode]").val(recMode=$(this).find('recMode').text());	//録画モード

					if($(this).find('tuijyuuFlag').text()==1){				//追従
						$("[name=tuijyuuFlag]").prop('checked',true);
					}else{
						$("[name=tuijyuuFlag]").prop('checked',false);
					}

					$("[name=priority]").val($(this).find('priority').text());		//優先度

					if($(this).find('pittariFlag').text()==1){				//ぴったり
						$("[name=pittariFlag]").prop('checked',true);
					}else{
						$("[name=pittariFlag]").prop('checked',false);
					}

					$("[name=suspendMode]").val($(this).find('suspendMode').text());	//録画後動作

					if($(this).find('rebootFlag').text()==1){				//復帰後
						$("[name=rebootFlag]").prop('checked',true);
					}else{
						$("[name=rebootFlag]").prop('checked',false);
					}

					if($(this).find('useMargineFlag').text()==0){				//録画マージン
						$("[name=useDefMarginFlag]").prop('checked',true);
						$(".margin input").prop('disabled', true);
						$(".margin").addClass("disabled");
					}else{
						$("[name=useDefMarginFlag]").prop('checked',false);
						$(".margin input").prop('disabled', false);
						$(".margin").removeClass("disabled");
					}

					$("[name=startMargin]").val($(this).find('startMargine').text());	//開始
					$("[name=endMargin]").val($(this).find('endMargine').text());		//終了

					if(($(this).find('serviceMode').text()%2)==0){				//指定サービス
						$("[name=serviceMode]").prop('checked',true);
						$(".mode").prop('disabled', true);
						$(".mode").addClass("disabled");
						$("[name=serviceMode_1]").prop('checked',false);
						$("[name=serviceMode_2]").prop('checked',false);
					}else{
						$("[name=serviceMode]").prop('checked',false);
						$(".mode").prop('disabled', false);
						$(".mode").removeClass("disabled");
						if((Math.floor($(this).find('serviceMode').text()/16)%2)!=0){	//字幕
							$("[name=serviceMode_1]").prop('checked',true);
						}else{
							$("[name=serviceMode_1]").prop('checked',false);
						}
						if((Math.floor($(this).find('serviceMode').text()/32)%2)!=0){	//データ
							$("[name=serviceMode_2]").prop('checked',true);
						}else{
							$("[name=serviceMode_2]").prop('checked',false);
						}
					}

					if($(this).find('continueRecFlag').text()==1){				//連続録画動作
						$("[name=continueRecFlag]").prop('checked',true);
					}else{
						$("[name=continueRecFlag]").prop('checked',false);
					}

					$("[name=tunerID]").val($(this).find('tunerID').text());		//チューナー
					
					var batfile;
					if($(this).find('batFilePath').text().length>0){			//bat
						batfile=$(this).find('batFilePath').text();
					}else{
						batfile='－'
					}
					$("#preset").empty().append('<ul><li>録画後実行bat</li><li id="batFile">' + batfile + '</li></ul>\n');
					
					if($(this).find('recFolderList').text().length>0){			//プリセット
						$(this).find('recFolderList').find('recFolderInfo').each(function(){
							var folderval=$(this).find('recFolder').text();
							var writeval=$(this).find('writePlugIn').text();
							var recnameval=$(this).find('recNamePlugIn').text();
							var folder; var write; var recname; var op;

							if($(this).find('recFolder').text().length>0){
								folder=$(this).find('recFolder').text();
							}else{
								folder='－';
							}

							if($(this).find('writePlugIn').text().length>0){
								write=$(this).find('writePlugIn').text();
							}else{
								write='－';
							}

							if($(this).find('recNamePlugIn').text().length>0){
								if($(this).find('recNamePlugIndll').text().length>0){
									recname=$(this).find('recNamePlugIndll').text();
									recnameval=$(this).find('recNamePlugIndll').text();
									op='<ul><li>オプション</li><li><input type="text" name="recName" value="'+$(this).find('recNamePlugInoption').text()+'" size="25"></li></ul>';
								}else{
									recname=$(this).find('recNamePlugIn').text();
								}
							}else{
								recname='－';
							}
							$("#preset").append('<input type=hidden name="recFolder" value="' + folderval + '"><input type=hidden name="writePlugIn" value="' + writeval + '"><input type=hidden name="recNamePlugIn" value="' + recnameval + '">' +
							'<ul><li>フォルダ</li><li">' + folder + '</li></ul><ul><li>出力PlugIn</li><li>' + write + '</li></ul><ul><li>ファイル名PlugIn</li><li>' + recname + '</li></ul>').append(op);
						})
					}else{
						$("#preset").append('<ul><li>フォルダ</li><li">－</li></ul><ul><li>出力PlugIn</li><li>－</li></ul><ul><li>ファイル名PlugIn</li><li>－</li></ul>');
					}

					if($(this).find('partialRecFlag').text()==1){				//部分受信
						$("#partial").prop('checked',true);
						$(".partial").show();
					}else{
						$("#partial").prop('checked',false);
						$(".partial").hide();
					}

					$("#partialpreset").empty();						//部分受信プリセット
					if($(this).find('partialRecFolder').text().length>0){
						$(this).find('partialRecFolder').find('recFolderInfo').each(function(){
							var folderval=$(this).find('recFolder').text();
							var writeval=$(this).find('writePlugIn').text();
							var recnameval=$(this).find('recNamePlugIn').text();
							var folder; var write; var recname; var op;
							if($(this).find('recFolder').text().length>0){
								folder=$(this).find('recFolder').text();
							}else{
								folder='－';
							}

							if($(this).find('writePlugIn').text().length>0){
								write=$(this).find('writePlugIn').text();
							}else{
								write='－';
							}

							if($(this).find('recNamePlugIn').text().length>0){
								if($(this).find('recNamePlugIndll').text().length>0){
									recname=$(this).find('recNamePlugIndll').text();
									recnameval=$(this).find('recNamePlugIndll').text();
									op='<ul><li>オプション</li><li><input type="text" name="partialrecName" value="'+$(this).find('recNamePlugInoption').text()+'" size="25"></li></ul>'
								}else{
									recname=$(this).find('recNamePlugIn').text();
								}
							}else{
								recname='－';
							}
							$("#partialpreset").append('<input type=hidden name="partialrecFolder" value="' + folderval + '"><input type=hidden name="partialwritePlugIn" value="' + writeval + '"><input type=hidden name="partialrecNamePlugIn" value="' + recnameval + '">' +
							'<ul><li>フォルダ</li><li">' + folder + '</li></ul><ul><li>出力PlugIn</li><li>' + write + '</li></ul><ul><li>ファイル名PlugIn</li><li>' + recname + '</li></ul>').append(op);
						})
					}else{
						$("#partialpreset").append('<ul><li>フォルダ</li><li">－</li></ul><ul><li>出力PlugIn</li><li>－</li></ul><ul><li>ファイル名PlugIn</li><li>－</li></ul>');
					}
				}
			})
		}
	});
});
