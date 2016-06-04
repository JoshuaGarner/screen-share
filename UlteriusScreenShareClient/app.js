var ScreenShare = (function () {
    function ScreenShare(element) {
        this.desktopElement = element;
        this.desktopElement.innerHTML += "The time is: ";
        this.span = document.createElement("span");
        this.desktopElement.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }
    ScreenShare.prototype.rightclick = function (pointerEvent) {
        this.cancelUiEvts(pointerEvent);
        var px = pointerEvent.offsetX ? pointerEvent.offsetX : pointerEvent.pageX - this.desktopElement.offsetLeft;
        var py = pointerEvent.offsetY ? pointerEvent.offsetY : pointerEvent.pageY - this.desktopElement.offsetTop;
        var event = {
            eventType: "Mouse",
            action: "RightClick",
            PointerY: py,
            PointerX: px
        };
        console.log(event);
    };
    ScreenShare.prototype.mouseMove = function (mouseEvent) {
        this.mouseMoveEvt = mouseEvent;
    };
    ScreenShare.prototype.mouseMoveUpdate = function (mousePos) {
        var event = {
            eventType: "Mouse",
            action: "Move",
            PointerY: mousePos.y,
            PointerX: mousePos.x
        };
        // console.log(event);
    };
    ScreenShare.prototype.cancelUiEvts = function (e) {
        e.stopPropagation();
        e.preventDefault();
    };
    ScreenShare.prototype.getMousePos = function (desktopElement, mouseEvent) {
        var rect = desktopElement.getBoundingClientRect();
        return {
            x: mouseEvent.clientX - rect.left,
            y: mouseEvent.clientY - rect.top
        };
    };
    ;
    ScreenShare.prototype.handleMouseDown = function (event) {
        this.cancelUiEvts(event);
        if (event.button === 0) {
            console.log("Mouse Down");
            var packetEvent = {
                eventType: "Mouse",
                action: "Down"
            };
        }
    };
    ScreenShare.prototype.handleMouseUp = function (event) {
        this.cancelUiEvts(event);
        if (event.button === 0) {
            console.log("Mouse Up");
            var packetEvent = {
                eventType: "Mouse",
                action: "Up"
            };
        }
    };
    ScreenShare.prototype.handleClick = function (event) {
        if (event.button === 0) {
            console.log("Left click detected");
            var packetEvent = {
                eventType: "Mouse",
                action: "LeftClick"
            };
        }
    };
    ScreenShare.prototype.handleDoubleClick = function (event) {
        if (event.button === 0) {
            console.log("Left click detected");
            var packetEvent = {
                eventType: "Mouse",
                action: "LeftDblClick"
            };
        }
    };
    ScreenShare.prototype.requestFrame = function () {
        var packetEvent = {
            eventType: "Frame",
            action: "Full"
        };
    };
    ScreenShare.prototype.start = function () {
        var _this = this;
        this.desktopElement.addEventListener("contextmenu", function (e) {
            _this.rightclick(e);
        });
        this.desktopElement.addEventListener("mousemove", function (evt) {
            var mousePos = _this.getMousePos(_this.desktopElement, evt);
            _this.mouseMoveUpdate(mousePos);
        }, false);
        this.desktopElement.addEventListener("mousedown", function (evt) {
            _this.handleMouseDown(evt);
        }, false);
        var clickCount = 0;
        this.desktopElement.addEventListener("click", function (evt) {
            clickCount++;
            var singleClickTimer;
            if (clickCount === 1) {
                singleClickTimer = setTimeout(function () {
                    clickCount = 0;
                    _this.handleClick(evt);
                }, 400);
            }
            else if (clickCount === 2) {
                clearTimeout(singleClickTimer);
                clickCount = 0;
                _this.handleDoubleClick(evt);
            }
        }, false);
        this.desktopElement.addEventListener("mouseup", function (evt) {
            _this.handleMouseUp(evt);
        }, false);
        this.desktopElement.addEventListener("keydown", function (e) {
            console.log(e.keyCode);
        }, false);
        this.desktopElement.addEventListener("keyup", function (e) {
            console.log(e.keyCode);
        }, false);
        setTimeout(function () { _this.requestFrame(); }, 200);
    };
    ScreenShare.prototype.stop = function () {
        clearTimeout(this.timerToken);
    };
    return ScreenShare;
}());
window.onload = function () {
    var el = document.getElementById("desktop");
    var screenShare = new ScreenShare(el);
    screenShare.start();
};
//# sourceMappingURL=app.js.map