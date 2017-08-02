API.onKeyUp.connect(function (sender, args) {

    if (args.KeyCode == Keys.F1) {
        API.triggerServerEvent("onKeyUp", 0);
    }
    else if (args.KeyCode == Keys.F2) {

        if (API.isCursorShown()) {
            API.showCursor(false)
        }
        else API.showCursor(true);
    }
    else if (args.KeyCode == Keys.N) {
        API.triggerServerEvent("onKeyUp", 2);
    }
    else if (args.KeyCode == Keys.Q) {
        API.triggerServerEvent("onKeyUp", 3);
    }
    else if (args.KeyCode == Keys.Y) {
        API.triggerServerEvent("onKeyUp", 4);
    }
    else if (args.KeyCode == Keys.I) {
        API.triggerServerEvent("onKeyUp", 5);
    }
    else if (args.KeyCode == Keys.K) {
        API.triggerServerEvent("onKeyUp", 6);
    }
    else if (args.KeyCode == Keys.L) {
        API.triggerServerEvent("onKeyUp", 7);
    }    
    else if (args.KeyCode == Keys.E) {
        if (resource.hud.drawAnimationHUD) {
            API.triggerServerEvent("onKeyUp", 8);
        }
    }
    else if (args.KeyCode == Keys.D1) {
        API.triggerServerEvent("onKeyUp", 9);
    }
    else if (args.KeyCode == Keys.D2) {
        API.triggerServerEvent("onKeyUp", 10);
    }
    else if (args.KeyCode == Keys.F) {
        API.triggerServerEvent("onKeyUp", 11);
    }
    else if (args.KeyCode == Keys.D3) {
        API.triggerServerEvent("onKeyUp", 12);
    }
    else if (args.KeyCode == Keys.D4) {
        API.triggerServerEvent("onKeyUp", 13);
    }
    else if (args.KeyCode == Keys.D5) {
        API.triggerServerEvent("onKeyUp", 14);
    }
    else if (args.KeyCode == Keys.ControlKey) {
        API.triggerServerEvent("onKeyUp", 15);
    }
});