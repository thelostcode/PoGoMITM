$(function () {
    //// Declare a proxy to reference the hub.
    //var notifications = $.signalR.notificationHub;
    //// Create a function that the hub can call to broadcast messages.
    
    //notifications.client.foo = function (message) {
    //    console.log(name, message);
    //};

    var notifications = $.connection.notificationHub;

    notifications.client.sendMessage = function(message) {
        console.log(message);
    };
    console.log(notifications);
    $.connection.hub.start().done(function () {
        notifications.server.sendMessage("test");
    });
});