window.onload = function () {
    var hammertime = new Hammer(document.getElementById("nextcloud_container"));
    hammertime.get('swipe').set({ direction: Hammer.DIRECTION_HORIZONTAL });
    hammertime.on('swipeleft', function (ev) {
        window.external.notify('left');
    });
    hammertime.on('swiperight', function (ev) {
        window.external.notify('right');
    });
}