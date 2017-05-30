// Global Variables
var WeightFromDirty = false;
var WeightToDirty = false;


function ViewModel() {
    var self = this;

    self.UnitList = [
         { Value: "ounces", Text: "ounces" }
    ];
    self.type = ko.observable("");
    self.message = ko.observable();

    self.modalTitle = ko.observable();
    self.isViewOnly = ko.observable(false);

    self.shippingrates = ko.observableArray();
    self.shippingrate = ko.observable();
        
    self.loadModel = function (modelId) {
        $("#loadingModal").show();
        if (modelId == -1 || modelId == undefined) {
            self.shippingrate(createShippingRateModel());
            $("#loadingModal").hide();

        } else {
            $.ajax({
                url: GET_SHIPPINGRATE_URL,
                data: { id: modelId },
                success: function (result) {
                    self.shippingrate(new ShippingRateModel(result));

                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });            
        }
        
    }

    self.loadData = function () {       
        $.ajax({
            url: GET_ALLSHIPPINGRATES_URL,
            success: function (results) {
                self.shippingrates(results);

                bindProductiCheckBox();
                bindiCheckBoxClicked();


                bindCurrentPageClicked();
                bindSelectAllPagesCheckBoxClicked();

            }
        });
    }

    self.editShippingRate = function () {
        self.isViewOnly(false);
        self.modalTitle("Edit Shipping Rate Details");
    }

    self.deleteShippingRate = function (shippingrate, event) {
        $.confirm({
            title: "Delete shipping rate",
            text: "Are you sure you want to delete weight: <strong> " + shippingrate.WeightFrom + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_SHIPPINGRATE_URL, { id: shippingrate.Id }, function (result) {
                    if (result.Success) {
                        self.shippingrates.remove(shippingrate);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.saveShippingRate = function (shippingrate, event) {
        if (!isValidateForm())
            return;
        
        showSpinner();
        $(event.target).addClass("disabled");

        // mapped the ko object to plain JS object
        var shippingrateData = ko.mapping.toJS(shippingrate);
        
        var inputWeightFrom = $("#WeightFrom").val();
        var inputWeightTo = $("#WeightTo").val();
        
        if (CheckDuplicateWeights(inputWeightFrom, inputWeightTo, self.shippingrates, self.shippingrate(), WeightFromDirty, WeightToDirty)) {
            alert("Duplicate Weight!");
            hideSpinner();
            $(event.target).removeClass("disabled");
            return;
        }

        if (CheckWeightIntegrity(inputWeightFrom, inputWeightTo)) {
            alert("Incorrect Weight!");
            hideSpinner();
            $(event.target).removeClass("disabled");
            return;
        }

        if (shippingrateData.WeightTo == 0) {
            shippingrateData.WeightTo = null;
        }

        $.ajax({
            type: "POST",
            url: SAVE_SHIPPINGRATE_URL,
            data: JSON.stringify(shippingrateData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("error");
                    viewModel.message("Error occured in trying to save shipping rate! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("success");
                viewModel.message("Shipping Rate have been successfully saved!");
                setTimeout("$('#ShippingRateDialog').modal('hide');", 2000);

                if (shippingrate.Id() == -1) {
                    self.shippingrates.push(result);
                    return true;
                }

                // find the shippingrate in the list
                var oldShippingRate = ko.utils.arrayFirst(self.shippingrates(), function (item) {
                    return item.Id == shippingrate.Id();
                })

                // replace the old value with the updated one
                self.shippingrates.replace(oldShippingRate, shippingrateData);
            },
            error: function (result) {
                // set the alert message
                viewModel.type("error");
                viewModel.message("Error occurred in saving the shipping rate!");
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
                WeightFromDirty = false;
                WeightToDirty = false;
            }
        });
    }
}

function CheckDuplicateWeights(inputWeightFrom, inputWeightTo, shippingratesKO, shippingRateData, WeightFromDirty, WeightToDirty) {
    var returnValue = false;
    var shippingRates = JSON.parse(ko.toJSON(shippingratesKO));
    var arrShippingRate = $.map(shippingRates, function (el) { return el });
    var shippingRateDataJSON = JSON.parse(ko.toJSON(shippingRateData));
    
    if (shippingRateDataJSON.Id == -1 || WeightFromDirty) {
        if ($.grep(arrShippingRate, function (el) {
            return (el.WeightFrom == inputWeightFrom ||
                    (el.WeightTo != null && el.WeightTo != 0
                    && el.WeightFrom <= inputWeightFrom
                    && inputWeightFrom <= el.WeightTo));
        }).length != 0) {
            returnValue = true;
        } 
    }
    
    if (shippingRateDataJSON.Id == -1 || WeightToDirty) {
        if ($.grep(arrShippingRate, function (el) {
            return (el.WeightFrom == inputWeightTo ||
                    (el.WeightTo != null && el.WeightTo != 0
                    && el.WeightFrom <= inputWeightTo
                    && inputWeightTo <= el.WeightTo));
        }).length != 0) {
            returnValue = true;
        } 
    }

    return returnValue;
}

function CheckWeightIntegrity(inputWeightFrom, inputWeightTo) {
    var returnValue = false;
    if (inputWeightTo != 0 && parseInt(inputWeightFrom) >= parseInt(inputWeightTo)) {
        returnValue = true;
    }

    return returnValue;
}

function ShippingRateModel(shippingrate) {
    var self = this;
    
    ko.mapping.fromJS(shippingrate, {}, self);
    
    // subscriptions
    self.WeightFrom.subscribe(function () {
        WeightFromDirty = true;
    });
    self.WeightTo.subscribe(function () {
        WeightToDirty = true;
    });
}

function ItemModel(item) {
    var self = this;

    self.Id = ko.observable(item.Id);
    self.Name = ko.observable(item.Name);
    self.Mode = ko.observable(item.Mode);
    //ko.mapping.fromJS(item, {}, self);
}

function createShippingRateModel() {
    return new ShippingRateModel({
        Id: -1,
        WeightFrom: 0,
        WeightTo: null,
        Unit: "",
        Rate: 0.00,
    });
}

function setFileUploadDialogTexts(fileType) {
    var height = 330;
    $("#jobTypeUpload").val(9);
    $("#uploadModalTitle").text("Upload Shipping Rate File");
    $("#uploadFileTemplateName").val("ShippingRate_Template.csv");
    $("#uploadNote").html("Upload shipping rate file into the system.");

    // resize the dialog
    $("#fileUploadDialogForm").css({ "height": height + "px" });
}

function downloadUploadFileTemplate() {
    document.getElementById("download_frame").src = GET_FILE_TEMPLATE_URL + "?fileTemplateName=" + $("#uploadFileTemplateName").val();
}