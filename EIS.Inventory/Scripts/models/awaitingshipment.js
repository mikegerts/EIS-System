
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();
    
    self.serviceTypes = ko.observableArray([
         new Group("USPS", [
             new Option("USPS First Class Mail", "1"),
             new Option("USPS Priority Mail", "2")
         ]),
         new Group("FedEx", [
             new Option("FedEx Ground®", "6"),
             new Option("FedEx Home Delivery®", "7"),
             new Option("FedEx SmartPost Parcel", "8")
         ])
    ]);
    self.addressTypes = ko.observableArray([
        new ItemModel({ Id: 1, Name: 'Residential Address' }),
        new ItemModel({ Id: 0, Name: 'Commercial Address' })
    ]);
    self.packageTypes = ko.observableArray([
        new ItemModel({ Id: 0, Name: 'Package' }),
        new ItemModel({ Id: 1, Name: 'Flat Rate Envelope' }),
        new ItemModel({ Id: 2, Name: 'Large Flat Rate Box' }),
        new ItemModel({ Id: 3, Name: 'Medium Flat Rate Box' }),
        new ItemModel({ Id: 4, Name: 'Regional Rate Box A' }),
        new ItemModel({ Id: 5, Name: 'Regional Rate Box B' }),
        new ItemModel({ Id: 6, Name: 'Small Flat Rate Box' })
    ]);
    self.confirmationTypes = ko.observableArray([
        new ItemModel({ Id: 0, Name: 'No Confirmation' }),
        new ItemModel({ Id: 1, Name: 'Delivery' }),
        new ItemModel({ Id: 2, Name: 'Signature' }),
        new ItemModel({ Id: 3, Name: 'Adult Signature' }),

    ]);
    self.insuranceTypes = ko.observableArray([
        new ItemModel({ Id: 0, Name: 'None' }),
        new ItemModel({ Id: 1, Name: 'Shipsurance Discount Insurance' }),
        new ItemModel({ Id: 2, Name: 'Carrier Insurance' }),
        new ItemModel({ Id: 3, Name: 'Other (external)' })
    ]);

    self.shippingLocations = ko.observableArray();
    self.orderProductDetail = ko.observable();
    self.selectedShipingLocation = ko.observable();
    self.rateCalculator = ko.observable();

    self.loadModel = function (modelId) {
        var shipFromLocation = $.grep(self.shippingLocations(), function (item) {
            return item.Id() == self.selectedShipingLocation();
        });
        self.rateCalculator(new RateCalculator(ko.toJS(self.orderProductDetail()), shipFromLocation[0]));
        $("#loadingModal").hide();
    }

    self.loadData = function () {
        // load all shipping locations
        $.ajax({
            url: '/shipping/_getallshippinglocations',
            success: function (results) {
                self.shippingLocations(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });
    }

    self.setOrderProductDetail = function (shippingDetail) {
        console.dir(shippingDetail);
    }
}

function OrderProductDetail(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);

    self.selectedServiceType = ko.observable(6);
    self.selectedPackageType = ko.observable(2);
    self.selectedConfirmationType = ko.observable();
    self.selectedInsuranceType = ko.observable();
    self.selectedAddressType = ko.observable(1);
    self.weightInPounds = ko.observable();
    self.weightInOunces = ko.observable();
    self.length = ko.observable();
    self.width = ko.observable();
    self.height = ko.observable();
}

function RateCalculator(model, shipFromLocation) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);

    self.selectedShippingRate = ko.observable();
    self.shipFromName = "("+ shipFromLocation.Name() +")";
    self.fromAddress = shipFromLocation.FromAddressDetails;
    self.endiciaShippingRates = ko.observableArray();
    self.fedExShippingRates = ko.observableArray();
    self.isSignatureRequiredOnDelivery = ko.pureComputed(function () {
        if (self.selectedConfirmationType() == 2 || self.selectedConfirmationType() == 3)
            return true;
        else
            return false;
    });
    self.counter = ko.observable(0);
    self.isRetrieving = ko.pureComputed(function () {
        return self.counter() < 2;
    });

    self.shippingRateSelected = function (rate, event) {
        $(event.currentTarget).parent().find("tr").removeClass("selected");
        $(event.currentTarget).addClass("selected");
        self.selectedShippingRate(rate);
    }

    self.chooseSelectedShippingRate = function () {
        viewModel.setOrderProductDetail(self.selectedShippingRate(),
            {
                ShipDate: new Date(),
                PackageType: self.selectedPackageType(),
                ConfirmationType: self.selectedConfirmationType(),
                InsuranceType: self.selectedInsuranceType(),
                Length: self.length(),
                Width: self.width(),
                Height: self.height(),
                Weight: weightValue,
                SignatureRequiredOnDelivery: self.isSignatureRequiredOnDelivery(),
            });
    }

    self.reset = function () {
        self.selectedShippingRate(null);
        self.fedExShippingRates([]);
        self.endiciaShippingRates([]);
        self.length(null);
        self.width(null);
        self.height(null);
        self.weightInPounds(null);
        self.weightInOunces(null);
        self.selectedServiceType(0);
        self.selectedPackageType(0);
        self.selectedConfirmationType(0);
        self.selectedInsuranceType(0);
        self.selectedAddressType(1);
        self.ShippingPostalCode(model.ShippingPostalCode);
    }

    self.getShippingRates = function () {
        $(".rates-panel").removeClass("active");
        self.fedExShippingRates([]);
        self.endiciaShippingRates([]);
        self.selectedShippingRate(null);
        self.counter(0);
        var weightValue = self.weightInOunces();
        var weightUnit = "ounces"

        if (self.weightInPounds() != 0 || self.weightInPounds() == null) {
            weightValue = self.weightInPounds();
            weightUnit = "pounds";
        }

        var shipmentDetail = {
            TransactionId: self.EisOrderId(),
            ShipDate: new Date(),
            PackageType: self.selectedPackageType(),
            ConfirmationType: self.selectedConfirmationType(),
            InsuranceType: self.selectedInsuranceType(),
            OriginAddress: ko.toJS(self.fromAddress),
            DestinationAddress: {
                Line1: self.ShippingAddressLine1(),
                Line2: self.ShippingAddressLine2(),
                City: self.ShippingCity(),
                StateOrRegion: self.ShippingStateOrRegion(),
                PostalCode: self.ShippingPostalCode(),
                CountryCode: "US",
                IsResidential: self.selectedAddressType()
            },
            Packages: [{
                Length: self.length(),
                Width: self.width(),
                Height: self.height(),
                Weight: weightValue,
                SignatureRequiredOnDelivery: self.isSignatureRequiredOnDelivery()
            }]
        };

        // get the rates for FedEx
        getShippingRates("FedEx", shipmentDetail, self.fedExShippingRates, self.counter);

        // get the rates for Endicia
        getShippingRates("Endicia", shipmentDetail, self.endiciaShippingRates, self.counter);
    }

    // load the shipping rates upon showing the dialog
    self.getShippingRates();
}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
}

function Group(label, children) {
    var self = this;
    self.label = ko.observable(label);
    self.children = ko.observableArray(children);
}

function Option(label, value) {
    var self = this;
    self.label = ko.observable(label);
    self.value = ko.observable(value);
}

function getShippingRates(provider, shipmentDetail, container, counter) {
    // get the shipping rates
    $.ajax({
        type: "POST",
        url: "/shipping/_CalculateShipmentRates",
        contentType: "application/json",
        data: JSON.stringify({ provider: provider, shipment: shipmentDetail }),
        success: function (results) {
            container(results)
        },
        complete: function () {
            counter(counter() + 1);
        }
    });
}