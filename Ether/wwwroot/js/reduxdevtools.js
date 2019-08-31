window.BlazorRedux = {
    reduxDevTools: null,
    init: function () {
        var self = window.BlazorRedux;
        var ext = window.__REDUX_DEVTOOLS_EXTENSION__;

        if (!ext) {
            console.warn("Redux DevTools extension is not installed.");
            return;
        }

        self.reduxDevTools = ext.connect();
        if (!self.reduxDevTools) {
            console.warn("Could not connect to Redux DevTools.");
            return;
        }
    },
    send: function (action, data, state) {
        var self = window.BlazorRedux;
        self.reduxDevTools.send({type: action, payload: data}, state);
    },
    sendInitial: function (state) {
        var self = window.BlazorRedux;
        self.reduxDevTools.init(state);
    }

};

window.BlazorRedux.init();