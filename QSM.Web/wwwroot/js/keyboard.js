window.keyDownListener = {
    listener: null,
    initialize: function(helper) {
        this.listener = function (e) {
            helper.invokeMethodAsync("OnKeyDown", e.key);
        };
        document.addEventListener("keydown", this.listener);
    },
    dispose: function() {
        document.removeEventListener("keydown", this.listener);
    }
};

window.keyUpListener = {
    listener: null,
    initialize: function(helper) {
        this.listener = function (e) {
            helper.invokeMethodAsync("OnKeyUp", e.key);
        };
        document.addEventListener("keyup", this.listener);
    },
    dispose: function() {
        document.removeEventListener("keyup", this.listener);
    }
};
