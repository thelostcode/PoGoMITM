var pogoMITM = pogoMITM || {};

pogoMITM.models = new function () {
    this.rawContextsList = new RawContextsList();
    ko.applyBindings(this.rawContextsList, document.body);

    function RawContextsList() {
        var self = this;
        var previousActiveItem;

        self.toolbarDownloadRawRequest = ko.observable("#");
        self.toolbarDownloadRawRequestEnabled = ko.observable(false);
        self.toolbarDownloadDecodedRequest = ko.observable("#");
        self.toolbarDownloadDecodedRequestEnabled = ko.observable(false);

        self.toolbarDownloadRawResponse = ko.observable("#");
        self.toolbarDownloadRawResponseEnabled = ko.observable(false);
        self.toolbarDownloadDecodedResponse = ko.observable("#");
        self.toolbarDownloadDecodedResponseEnabled = ko.observable(false);

        self.toolbarDownloadJson = ko.observable("#");
        self.toolbarDownloadJsonEnabled = ko.observable(false);

        self.rawContextListItems = ko.observableArray([]);

        self.addItem = function (guid, requestTime, host) {
            self.rawContextListItems.push({
                Guid: guid,
                RequestTime: requestTime,
                Host: host,
                IsActive: ko.observable(false)
            });
        };

        self.addRange = function (listOfItems) {
            for (var i = 0; i < listOfItems.length; i++) {
                var item = listOfItems[i];
                self.addItem(item.Guid, item.RequestTime, item.Host);
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
                        try {

                            if (data.RequestEnvelope &&
                                data.RequestEnvelope.Unknown6 &&
                                data.RequestEnvelope.Unknown6.length > 0 &&
                                data.RequestEnvelope.Unknown6[0].Unknown2 &&
                                data.RequestEnvelope.Unknown6[0].Unknown2.EncryptedSignature &&
                                data.RequestEnvelope.Unknown6[0].Unknown2.EncryptedSignature.length > 0) {
                                var test = p.d(data.RequestEnvelope.Unknown6[0].Unknown2.EncryptedSignature);
                                var test1 = new Uint8Array(test);
                                var test2 = Array.from(test1);

                                $.ajax({
                                    url: "/details/signature",
                                    data: { Bytes: JSON.stringify(test2) },
                                    method: "POST"
                                }).done(function (result) {
                                    if (result && result.success) {
                                        data.DecryptedSignature = result.signature;
                                        console.log(result.signature);
                                        $(".jsonViewer").JSONView(data, { collapsed: true });
                                    }
                                });
                            }
                        } catch (e) {

                        }
                        if (previousActiveItem) previousActiveItem.IsActive(false);
                        item.IsActive(true);
                        previousActiveItem = item;
                        $(".jsonViewer").JSONView(data, { collapsed: true });
                        if (data.RequestBodyLength > 0) {
                            self.toolbarDownloadRawRequest("/download/request/raw/" + item.Guid);
                            self.toolbarDownloadRawRequestEnabled(true);
                            self.toolbarDownloadDecodedRequest("/download/request/decoded/" + item.Guid);
                            self.toolbarDownloadDecodedRequestEnabled(true);
                        } else {
                            self.toolbarDownloadRawRequestEnabled(false);
                            self.toolbarDownloadDecodedRequestEnabled(false);
                        }
                        if (data.ResponseBodyLength > 0) {
                            self.toolbarDownloadRawResponse("/download/response/raw/" + item.Guid);
                            self.toolbarDownloadRawResponseEnabled(true);
                            self.toolbarDownloadDecodedResponse("/download/response/decoded/" + item.Guid);
                            self.toolbarDownloadDecodedResponseEnabled(true);
                        } else {
                            self.toolbarDownloadRawResponseEnabled(false);
                            self.toolbarDownloadDecodedResponseEnabled(false);

                        }
                        self.toolbarDownloadJson("/download/json/" + item.Guid);
                        self.toolbarDownloadJsonEnabled(true);
                    } else {
                        $(".jsonViewer").html("An error occured");
                        self.toolbarDownloadRawRequestEnabled(false);
                        self.toolbarDownloadDecodedRequestEnabled(false);
                        self.toolbarDownloadRawResponseEnabled(false);
                        self.toolbarDownloadDecodedResponseEnabled(true);
                        self.toolbarDownloadJsonEnabled(false);
                    }
                })
                .fail(function () {
                    $(".jsonViewer").html("An error occured");
                    self.toolbarDownloadRawRequestEnabled(false);
                    self.toolbarDownloadRawResponseEnabled(false);
                    self.toolbarDownloadJsonEnabled(false);
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
            pogoMITM.models.rawContextsList.addItem(vm.Guid, vm.RequestTime, vm.Host);
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
        $(".jsonViewer").height(contentHeight - 40);
    }

    $(window).on("resize", resizeRawContextsContainer);
    resizeRawContextsContainer();

});