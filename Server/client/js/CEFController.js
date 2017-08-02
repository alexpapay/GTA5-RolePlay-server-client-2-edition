class WebBrowser {
    constructor(resourcePath) {
        this.path = resourcePath;
        this.open = false;
    }

    show() {
        if (this.open === false) {
            this.open = true;
            API.displaySubtitle("Вы говорите!", 5000);
            var resolution = API.getScreenResolution();
            this.browser = API.createCefBrowser(resolution.Width, resolution.Height, true);
            API.waitUntilCefBrowserInit(this.browser);
            API.setCefBrowserPosition(this.browser, 0, resolution.Height / 2 - 400);
            API.loadPageCefBrowser(this.browser, this.path);
            API.showCursor(false);
            API.setCefBrowserHeadless(this.browser, false);
            Browser = this.browser;
            API.displaySubtitle(" ", 1);
        }
    }

    destroy() {
        this.open = false;
        API.destroyCefBrowser(this.browser);
        API.showCursor(false);
    }

    eval(string) {
        this.browser.eval(string);
    }
}

var VoiceOn = new WebBrowser('');

API.onServerEventTrigger.connect(function (eventName, args) {

    if (eventName == "CEFDestroy") {
        VoiceOn.destroy();
    }
    else if (eventName === "CEFVoiceOn") {
        VoiceOn = new WebBrowser('client/html/Voice.html'); 
    }
});

API.onKeyDown.connect(function (sender, args) {

    if (args.KeyCode == Keys.X) {
        VoiceOn = new WebBrowser('client/html/Voice.html'); 
        //VoiceOn.destroy();
    }
});

API.onResourceStop.connect(function () {
    //if (VoiceOn != null) {
    //    VoiceOn.destroy();
    //}
});