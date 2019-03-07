var application = (function ($, module) {
    if ($ === undefined || $ === null) {
        throw new Error("jQuery was not found. Please ensure jQuery is referenced before the Data Core script files.");
    }
    if ($.connection === undefined || $.connection.hub === undefined) {
        throw new Error("The SignalR client was not found.  Please ensure the SignalR client JavaScript file is referenced before the application script files.");
    }

    module.signalr = module.signalr || {};

    var signalRReconnectionDelay = 5000;
    var signalRConnectedCallback = null;
    var signalRConnected = false;

    function initialiseSignalRConnection(callback, reconnectionDelay) {
        if (typeof callback === "function") {
            signalRConnectedCallback = callback;
        }
        if (typeof reconnectionDelay === "number") {
            signalRReconnectionDelay = reconnectionDelay;
        }

        if ($.connection.hub.state !== $.signalR.connectionState.disconnected) {
            return new $.Deferred().resolve().promise().done(function () {
                if (signalRConnectedCallback && typeof signalRConnectedCallback === "function") {
                    signalRConnectedCallback();
                }
            });
        }

        console.log("[SignalR] Connecting to SignalR.");
        return $.connection.hub.start()
            .done(function () {
                signalRConnected = true;
                console.log("[SignalR] Connected to SignalR: Connection ID=", $.connection.hub.id, ", Transport=", $.connection.hub.transport.name);
                if (signalRConnectedCallback && typeof signalRConnectedCallback === "function") {
                    signalRConnectedCallback();
                }
            })
            .fail(function () {
                signalRConnected = false;
                console.error("[SignalR] Unable to connect to SignalR.");
            });
    }

    // Configure general SignalR handlers.
    $(function() {
        $.connection.hub.error(function(error) {
            console.error("[SignalR]", error);
        });
        $.connection.hub.stateChanged(function(data) {
            var oldStateName;
            var newStateName;

            $.each($.signalR.connectionState,
                function(name, value) {
                    if (value === data.oldState) {
                        oldStateName = name;
                    }
                    if (value === data.newState) {
                        newStateName = name;
                    }
                });

            console.warn("[SignalR] Connection state changed from", oldStateName, "to", newStateName);

            if (data.oldState === $.signalR.connectionState.connected && data.newState === $.signalR.connectionState.reconnecting) {
                signalRConnected = false;
            } else if (data.oldState === $.signalR.connectionState.reconnecting && data.newState === $.signalR.connectionState.connected) {
                signalRConnected = true;
            }
        });
        $.connection.hub.disconnected(function () {
            signalRConnected = false;
            if (signalRReconnectionDelay > 1000) {
                setTimeout(function() {
                        console.log("[SignalR] Connecting to SignalR.");
                        $.connection.hub.start()
                            .done(function () {
                                signalRConnected = true;
                                console.log("[SignalR] Connected to SignalR: Connection ID=", $.connection.hub.id, ", Transport=", $.connection.hub.transport.name);
                                if (signalRConnectedCallback && typeof signalRConnectedCallback === "function") {
                                    signalRConnectedCallback();
                                }
                            })
                            .fail(function () {
                                signalRConnected = false;
                                console.error("[SignalR] Unable to connect to SignalR.");
                            });
                    },
                    signalRReconnectionDelay); // Restart connection after the configured reconnection delay.
            }
        });
    });

    module.signalr.init = initialiseSignalRConnection;
    module.signalr.isConnected = function() {
        return signalRConnected;
    };

    return module;
}(jQuery, application || {}));