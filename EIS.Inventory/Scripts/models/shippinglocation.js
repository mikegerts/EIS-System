function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.shippingLocation = ko.observable();
    self.usStates = ko.observableArray(["AA", "AB", "AE", "AK", "AL", "AP", "AR", "AS", "AZ", "BC", "CA", "CO", "CT", "DC", "DE",
        "FL", "FM", "GA", "GU", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MB", "MD",
        "ME", "MH", "MI", "MN", "MO", "MP", "MS", "MT", "NB", "NC", "ND", "NE", "NH", "NJ", "NL",
        "NM", "NS", "NT", "NU", "NV", "NY", "OH", "OK", "ON", "OR", "PA", "PE", "PR", "PW", "QC",
        "RI", "SA", "SC", "SD", "SK", "TN", "TX", "UT", "VA", "VI", "VT", "WA", "WI", "WV", "WY", "YT"]);
    
    self.loadModel = function (modelId) {
        if (modelId == null) {
            self.shippingLocation(new ShippingLocationModel(shippingLocationObj));
            $("#loadingModal").hide();
        }

        $.ajax({
            url: '/shipping/_GetShippingLocation',
            data: { id: modelId },
            success: function (result) {
                self.shippingLocation(new ShippingLocationModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }


    self.saveShippingLocation = function (model, event) {
        if (!isValidateForm())
            return;

        showSpinner();
        $(event.target).addClass("disabled");

        $.ajax({
            type: "POST",
            url: "/shipping/_saveshippinglocation",
            data: JSON.stringify(ko.mapping.toJS(self.shippingLocation())),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("error");
                    viewModel.message("Error occured in trying to save shipping location! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("success");
                viewModel.message("Shipping location have been successfully saved!");
                setTimeout("location.reload();", 2000);
            },
            error: function (result) {
                // set the alert message
                viewModel.type("error");
                viewModel.message("Error occurred in saving the shipping location!");
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }
}

function deleteModel(source, id, name) {
    $.confirm({
        title: "Delete Shipping Location",
        text: "Are you sure you want to delete shipping location: <strong> " + name + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post('/shipping/_DeleteShippingLocation', { id: id }, function (result) {
                if (result.Success) {
                    $(source).parent().parent().fadeOut();

                    // reload the page if there's no table records in the paged
                    if (($("#tblVendorProducts > tbody > tr:visible").length - 1) == 0)
                        location.reload();

                } else {
                    displayMessage(result.Error, "error")
                    fadeOutMessage();
                }
            });
        },
        confirmButton: "Yes, I am",
        confirmButtonClass: "btn-warning"
    });
}

function ShippingLocationModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);

    self.isEdit = function () {
        return self.Id() > 0;
    }


    self.addressChanged = function (address, event) {

        // apply changes to the address if there's any
        self.copyFromAddressDetails(self.IsReturnSame());

        // fire the parent change event
        valueChanged(address, event);
    }

    self.IsReturnSame.subscribe(function (value) {
        // apply changes to the address if there's any
        self.copyFromAddressDetails(value);
    });

    self.copyFromAddressDetails = function (isCopy) {
        if (!isCopy) return;

        // copy the FromAddress t ReturnAddress
        self.ReturnCompanyName(self.FromCompanyName());
        self.ReturnPhone(self.FromPhone());
        self.ReturnAddressDetails.Line1(self.FromAddressDetails.Line1());
        self.ReturnAddressDetails.Line2(self.FromAddressDetails.Line2());
        self.ReturnAddressDetails.CountryCode(self.FromAddressDetails.CountryCode());
        self.ReturnAddressDetails.City(self.FromAddressDetails.City());
        self.ReturnAddressDetails.StateOrRegion(self.FromAddressDetails.StateOrRegion());
        self.ReturnAddressDetails.PostalCode(self.FromAddressDetails.PostalCode());
        self.ReturnAddressDetails.IsResidential(self.FromAddressDetails.IsResidential());

        // trigger change event for the required fields
        $("#ReturnLine1").trigger("change");
        $("#ReturnCountryCode").trigger("change");
        $("#ReturnCity").trigger("change");
        $("#ReturnStateOrRegion").trigger("change");
        $("#ReturnPostalCode").trigger("change");
    }
}