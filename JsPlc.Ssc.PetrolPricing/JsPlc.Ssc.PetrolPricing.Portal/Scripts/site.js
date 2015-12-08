jQuery(function ($) {
    var $active = $('#accordion .panel-collapse.in').prev().addClass('active');
    $active.find('a').prepend('<i class="fa fa-minus-square"></i>');
    $('#accordion .panel-heading').not($active).find('a').prepend('<i class="fa fa-plus-square"></i>');
    $('#accordion').on('show.bs.collapse', function (e) {
        $('#accordion .panel-heading.active').removeClass('active').find('.fa fa-minus-square').toggleClass('fa fa-plus-square');
        $(e.target).prev().addClass('active').find('.fa fa-plus-square').toggleClass('fa fa-minus-square');
    })
});



