function set_reset(){
	$('.background').css('background',$('[name=background]').val());
	$('.news').css('background',$('[name=news]').val());
	$('.sports').css('background',$('[name=sports]').val());
	$('.information').css('background',$('[name=information]').val());
	$('.drama').css('background',$('[name=drama]').val());
	$('.music').css('background',$('[name=music]').val());
	$('.variety').css('background',$('[name=variety]').val());
	$('.movie').css('background',$('[name=movie]').val());
	$('.anime').css('background',$('[name=anime]').val());
	$('.documentary').css('background',$('[name=documentary]').val());
	$('.theater').css('background',$('[name=theater]').val());

	$('.education').css('background',$('[name=education]').val());
	$('.welfare').css('background',$('[name=welfare]').val());
	$('.extension').css('background',$('[name=extension]').val());
	$('.other').css('background',$('[name=other]').val());
	$('.none').css('background',$('[name=none]').val());
	$('.nothing').css('background',$('[name=nothing]').val());

	if (paint==true){
		$('.reserved').css('border','none').css('background',$('[name=reserved]').val());
		$('.disable').css('border','none').css('background',$('[name=disable]').val());
	}else{
		$('.reserved').css('border','3px dotted '+$('[name=reserved]').val()).css('background','transparent');
		$('.disable').css('border','3px dotted '+$('[name=disable]').val()).css('background','transparent');
	}
	$('.partially').css('background',$('[name=partially]').val());
	$('.partially').css('border-color',$('[name=partially_border]').val());
	$('.shortage').css('background',$('[name=shortage]').val());
	$('.shortage').css('border-color',$('[name=shortage_border]').val());
	$.each(
		sort.split(",").reverse(),
		function(index, value) {
			$('#'+value).prependTo("#service ul");
		}
	);
}

$(function(){
	$("#menu_ico").click(function(){
		$(".sidemenu").addClass("visible");
		$(".obfuscator").addClass("visible");
	});
	$(".obfuscator").click(function(){
		$(".sidemenu").removeClass("visible");
		$(".obfuscator").removeClass("visible");
	});
	
	$("#tab-bar label").click(function(){
		$("#tab-bar label").removeClass("active");
		$(this).addClass("active");
		$(".tab-bar").hide();
		$("."+$(this).data("val")).show();
		console.log($(this).data("val"));
	});

	$('#service li').not('.DTV').hide();			//É^Éuêÿë÷
	$('[name=service]').change(function() {
		$('#service li').hide();
		$('.'+$(this).attr("id")).show();
		$('#tab label').removeClass("active");
		if($(this).prop('checked')){
			$('[for='+$(this).attr("id")+']').addClass("active");
		}
	});

	$("#service ul").disableSelection();					//ï¿Ç—ë÷Ç¶
	$("#service ul").sortable({handle: ".handle"});
	sort = $("#service ul").sortable("toArray").join(",");		//èâä˙ílï€ë∂


	$('.genre [type=color],[name=background]').change(function(){
		$('.'+$(this).attr("name")).css('background',$(this).val());
	});
	
	$('[name=reserved],[name=disable]').change(function(){
		if ($('#paint').prop('checked')){
			$('.'+$(this).attr("name")).css('background',$(this).val());
		}else{
			$('.'+$(this).attr("name")).css('border-color',$(this).val());
		}
	});
	$('[name=partially],[name=shortage]').change(function(){
			$('.'+$(this).attr("name")).css('background',$(this).val());
	});
	$('[name=partially_border]').change(function(){
			$('.partially').css('border-color',$(this).val());
	});
	$('[name=shortage_border]').change(function(){
			$('.shortage').css('border-color',$(this).val());
	});
	$('#paint').change(function(){
		if ($(this).prop('checked')){
			$('.reserved').css('border','none').css('background',$('[name=reserved]').val());
			$('.disable').css('border','none').css('background',$('[name=disable]').val());
		}else{
			$('.reserved').css('border','3px dotted '+$('[name=reserved]').val()).css('background','transparent');
			$('.disable').css('border','3px dotted '+$('[name=disable]').val()).css('background','transparent');
		}
	});
	$("#hoge").change(function(){
		if($(this).prop('checked')){
			$("#sidemenu").css("left","0");
			$("#wrap").css("margin-left","300px");
		}else{
			$("#sidemenu").css("left","-300px");
			$("#wrap").css("margin-left","0");
		}
	});

});
