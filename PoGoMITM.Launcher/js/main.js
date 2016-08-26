var pogoMITM = pogoMITM || {};

pogoMITM.models = new function () {
    this.rawContextsList = new RawContextsList();
    ko.applyBindings(this.rawContextsList);

    function RawContextsList() {
        var self = this;
        var previousActiveItem;

        self.rawContextListItems = ko.observableArray([]);

        self.addItem = function (guid, requestTime, host) {
            self.rawContextListItems.push({
                Guid: guid,
                RequestTime: requestTime,
                Host: host,
                IsActive: ko.observable(false)
            });
        };

        self.addRange = function(listOfItems) {
            for (var i = 0; i < listOfItems.length; i++) {
                var item = listOfItems[i];
                self.addItem(item.Guid, item.RequestTime, item.RequestUri);
            }
        };

        self.loadRequestDetails = function (item) {
            $(".jsonViewer").html("Loading...");
            $.ajax({
                url: "/details/" + item.Guid,
                method: "GET"
            })
                .done(function (data) {
                    if (data) {
                        if (previousActiveItem) previousActiveItem.IsActive(false);
                        item.IsActive(true);
                        previousActiveItem = item;
                        $(".jsonViewer").JSONView(data, { collapsed: true });
                    }
                })
                .fail(function () {
                    $(".jsonViewer").html("An error occured");
                });
        }
    }
}

pogoMITM.signalR = {
    init: function () {
        var notifications = $.connection.notificationHub;

        notifications.client.sendMessage = function (message) {
            //console.log(message);
        };
        notifications.client.rc = function (vm) {
            //console.log(vm);
            pogoMITM.models.rawContextsList.addItem(vm.Guid, vm.RequestTime, vm.RequestUri);
        };
        $.connection.hub.start()
            .done(function () {
                // notifications.server.sendMessage("test");
            });
    }
};



$(function () {

    pogoMITM.signalR.init();

    function resizeRawContextsContainer() {
        var windowHeight = $(window).height();
        var contentHeight = windowHeight - $(".navbar").height() - 30;
        $(".rawContextsContainer").height(contentHeight);
        $(".jsonViewer").height(contentHeight);
    }

    $(window).on("resize", resizeRawContextsContainer);
    resizeRawContextsContainer();

});