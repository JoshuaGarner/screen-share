class ScreenShare {
    desktopElement: HTMLElement;
    span: HTMLElement;
    timerToken: number;
    mousedownEvt: MouseEvent;
    mouseMoveEvt: MouseEvent;
    doubleClick: boolean;

    constructor(element: HTMLElement) {
        this.desktopElement = element;
        this.desktopElement.innerHTML += "The time is: ";
        this.span = document.createElement("span");
        this.desktopElement.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }

    rightclick(pointerEvent: PointerEvent) {
        this.cancelUiEvts(pointerEvent);
        const px = pointerEvent.offsetX ? pointerEvent.offsetX : pointerEvent.pageX - this.desktopElement.offsetLeft;
        const py = pointerEvent.offsetY ? pointerEvent.offsetY : pointerEvent.pageY - this.desktopElement.offsetTop;
        const event = {
            eventType: "Mouse",
            action: "RightClick",
            PointerY: py,
            PointerX: px
        };
        console.log(event);
    }

   

    mouseMove(mouseEvent: MouseEvent) {
        this.mouseMoveEvt = mouseEvent;
    }


    mouseMoveUpdate(mousePos: any) {

        const event = {
            eventType: "Mouse",
            action: "Move",
            PointerY: mousePos.y,
            PointerX: mousePos.x
        };
        // console.log(event);


    }

    cancelUiEvts(e: MouseEvent) {
        e.stopPropagation();
        e.preventDefault();
    }

    getMousePos(desktopElement: HTMLElement, mouseEvent: MouseEvent) {
        const rect = desktopElement.getBoundingClientRect();
        return {
            x: mouseEvent.clientX - rect.left,
            y: mouseEvent.clientY - rect.top
        };
    };

    handleMouseDown(event: MouseEvent) {
        this.cancelUiEvts(event);
        if (event.button === 0) {
            console.log("Mouse Down");
            const packetEvent = {
                eventType: "Mouse",
                action: "Down"
            };
        }
        
    }

    handleMouseUp(event: MouseEvent) {
        this.cancelUiEvts(event);
        if (event.button === 0) {
            console.log("Mouse Up");
            const packetEvent = {
                eventType: "Mouse",
                action: "Up"
            };
        }
    }

    handleClick(event: MouseEvent) {
        if (event.button === 0) {
            console.log("Left click detected");
            const packetEvent = {
                eventType: "Mouse",
                action: "LeftClick"
            };
        }
    }
    handleDoubleClick(event: MouseEvent) {
        if (event.button === 0) {
            console.log("Left click detected");
            const packetEvent = {
                eventType: "Mouse",
                action: "LeftDblClick"
            };
        }
    }
    requestFrame() {
        const packetEvent = {
            eventType: "Frame",
            action: "Full"
        };
    }
    start() {
        this.desktopElement.addEventListener("contextmenu",
            e => {

                this.rightclick(e);
            });

        this.desktopElement.addEventListener("mousemove",
            evt => {

                var mousePos = this.getMousePos(this.desktopElement, evt);
                this.mouseMoveUpdate(mousePos);

            },
            false);

        this.desktopElement.addEventListener("mousedown",
            evt => {

                this.handleMouseDown(evt);
            },
            false);
        var clickCount = 0;
        this.desktopElement.addEventListener("click",
            evt => {
                clickCount++;
                var singleClickTimer;
                if (clickCount === 1) {
                    singleClickTimer = setTimeout(() => {
                        clickCount = 0;
                        this.handleClick(evt);
                    }, 400);
                } else if (clickCount === 2) {
                    clearTimeout(singleClickTimer);
                    clickCount = 0;
                    this.handleDoubleClick(evt);
                }
            },
            false);
        this.desktopElement.addEventListener("mouseup",
            evt => {
            
               this.handleMouseUp(evt);
            },
            false);

        this.desktopElement.addEventListener("keydown",
            e => {
                console.log(e.keyCode);
            },
            false);
        this.desktopElement.addEventListener("keyup",
            e => {
                console.log(e.keyCode);
            },
            false);
        setTimeout(() => { this.requestFrame(); }, 200);
    }

    stop() {
        clearTimeout(this.timerToken);
    }

}

window.onload = () => {
    var el = document.getElementById("desktop");
    var screenShare = new ScreenShare(el);
    screenShare.start();
};