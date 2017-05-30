
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.dropShipFeeType = [
         { Id: 0, Name: "Zero" },
         { Id: 1, Name: "Order" },
         { Id: 2, Name: "Quantity" }
    ];
    self.returnsAcceptedOptions = [
        { Id: "ReturnsAccepted", Name: "Returns Accepted" },
        { Id: "ReturnsNotAccepted", Name: "No returns accepted" }
    ];
    self.refundOptions = [
        { Id: "MoneyBack", Name: "Money Back" },
        { Id: "MoneyBackOrReplacement", Name: "Money back or replacement (buyer's choice)" },
        { Id: "MoneyBackOrExchange", Name: "Money back or exchange (buyer's choice)" }
    ];
    self.returnsWithinOptions = [
        { Id: "Days_14", Name: "14 Days" },
        { Id: "Days_30", Name: "30 Days" },
        { Id: "Days_60", Name: "60 Days" }
    ];
    self.shippingCostPaidByOptions = [
         { Id: "Buyer", Name: "Buyer" },
         { Id: "Seller", Name: "Seller" }
    ];
    self.shippingTypes = [
         { Id: "Free", Name: "Free" },
         { Id: "Flat", Name: "Flat" },
         { Id: "Freight", Name: "Freight" }
    ];
    self.shippingServices = [
         { Id: "ShippingMethodStandard", Name: "ShippingMethodStandard" },
         { Id: "ShippingMethodExpress", Name: "ShippingMethodExpress" },
         { Id: "UPSGround", Name: "UPSGround" },
         { Id: "UPS3rdDay", Name: "UPS3rdDay" },
         { Id: "USPSPriority", Name: "USPSPriority" },
         { Id: "USPSParcel", Name: "USPSParcel" },
         { Id: "USPSMedia", Name: "USPSMedia" },
         { Id: "USPSFirstClass", Name: "USPSFirstClass" },
         { Id: "USPSExpressMail", Name: "USPSExpressMail" },
         { Id: "FedExGround", Name: "FedExGround" },
         { Id: "FedExHomeDelivery", Name: "FedExHomeDelivery" },
         { Id: "FedExExpressSaver", Name: "FedExExpressSaver" },
         { Id: "Pickup", Name: "Pickup" }
    ];

    self.inventoryUpdateFrequency = [
         { Id: "Daily", Name: "Daily" },
         { Id: "Weekly", Name: "Weekly" },
         { Id: "BiWeekly", Name: "Bi-Weekly" },
         { Id: "AlwaysInStock", Name: "Always In Stock" }
    ];

    self.modalTitle = ko.observable();
    self.isViewOnly = ko.observable(false);

    self.vendors = ko.observableArray();
    self.company = ko.observableArray();
    self.vendor = ko.observable();
    self.departments = ko.observableArray();
    self.defaultCompany = ko.observable();

    self.loadModel = function (modelId) {
        $("#loadingModal").show();
        if (modelId == -1) {
            self.vendor(createVendorModel(self.defaultCompany()));
            $("#loadingModal").hide();
        } else {
            $.ajax({
                url: GET_VENDOR_URL,
                data: { id: modelId },
                success: function (result) {

                    self.vendor(new VendorModel(result));
                    
                    var jsonData = JSON.parse(ko.toJSON(self.vendor));
                    var arrVendorDepartment = $.map(jsonData.VendorDepartments, function (el) { return el });

                    PopulateVendorDepartments(arrVendorDepartment);
                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });
        }
    }

    self.loadData = function () {
        $.ajax({
            url: GET_VENDORS_URL,
            success: function (results) {
                self.vendors(results);
            }
        });

        $.ajax({
            url: GET_COMPANY_URL,
            success: function (results) {
                self.company(results);

                // determine the default company
                var match = ko.utils.arrayFirst(results, function (item) {
                    return item.IsDefault == true;
                });
                if (match)
                    self.defaultCompany(match.Id);
            }
        });

        $.ajax({
            url: GET_DEPARTMENT_URL,
            success: function (results) {
                self.departments(results);
            }
        });
    }

    self.editVendor = function () {
        self.isViewOnly(false);
        self.modalTitle("Edit Vendor Details");
    }

    self.deleteVendor = function (vendor, event) {
        $.confirm({
            title: "Delete Vendor",
            text: "Are you sure you want to delete vendor: <strong> " + vendor.Name + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_VENDOR_URL, { id: vendor.Id }, function (result) {
                    if (result.Success) {
                        self.vendors.remove(vendor);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.saveVendor = function (vendor, event) {
        if (!isValidateForm())
            return;

        showSpinner();
        $(event.target).addClass("disabled");

        // mapped the ko object to plain JS object
        var vendorData = ko.mapping.toJS(vendor);

        var departments = JSON.parse(ko.toJSON(self.departments));
        var arrdepartments = $.map(departments, function (el) { return el });

        var updatedData = MapVendorDepartments(vendorData.VendorDepartments, arrdepartments, vendorData);

        vendorData.VendorDepartments = [];
        vendorData.VendorDepartments = updatedData;
        console.log(vendorData.VendorDepartments);

        $.ajax({
            type: "POST",
            url: SAVE_VENDOR_URL,
            data: JSON.stringify(vendorData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("error");
                    viewModel.message("Error occured in trying to save Vendor! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("success");
                viewModel.message("Vendor have been successfully saved!");
                setTimeout("$('#VendorDialog').modal('hide');", 2000);

                if (vendor.Id() == -1) {
                    self.vendors.push(result);
                    return true;
                }

                // find the vendor in the list
                var oldVendor = ko.utils.arrayFirst(self.vendors(), function (item) {
                    return item.Id == vendor.Id();
                })

                // replace the old value with the updated one
                self.vendors.replace(oldVendor, result);
            },
            error: function (result) {
                // set the alert message
                viewModel.type("error");
                viewModel.message("Error occurred in saving the vendor!");
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }
}

function VendorModel(vendor) {
    var self = this;
    var oldCost = vendor.ShippingServiceCost;
    
    var mapping = {
        "VendorDepartments": {
            create: function (options) {
                return new VendorDepartmentModel(options.data);
            }
        }
    };

    ko.mapping.fromJS(vendor, mapping, self);
    
    self.isReturnAcceptedSelected = ko.pureComputed(function () {
        return self.ReturnsAcceptedOption() == "ReturnsAccepted";
    });

    self.isAlwaysInStockSelected = ko.pureComputed(function () {
        return self.InventoryUpdateFrequency() == "AlwaysInStock";
    });

    self.isFreeShippingTypeSelected = ko.pureComputed(function () {
        if (self.ShippingType() == "Free") {
            oldCost = self.ShippingServiceCost();
            self.ShippingServiceCost(0);
            return true;
        } else {
            self.ShippingServiceCost(oldCost);
            return false;
        }
    });

}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
}

function VendorDepartment(vendordepartment) {
    var self = this;

    ko.mapping.fromJS(vendordepartment, {}, self);
}

function Department(department) {
    var self = this;

    ko.mapping.fromJS(department, {}, self);
}

function createVendorModel(defaultCompanyId) {
    console.log("Default companyid: " + defaultCompanyId)
    return new VendorModel({
        Id: -1,
        CompanyId: defaultCompanyId,
        Name: "",
        Email: "",
        VendorAddress: "",
        SuiteApartment: "",
        ContactPerson: "",
        City: "",
        ZipCode: "",
        PhoneNumber: "",
        SKUCodeStart: "",
        DropShipFeeType: "",
        DropShipFee: "",
        SafetyQty: 0,
        ReturnsAcceptedOption: "",
        RefundOption: "",
        ReturnsWithinOption: "",
        ShippingCostPaidByOption: "",
        ReturnPolicyDescription: "",
        ShippingType: "",
        ShippingService: "",
        ShippingServiceCost: "",
        IsFreeShipping: false,
        State: "",
        Website: "",
        FaxField: "",
        AvailableCarrier: "",
        PickupFrequency: "",
        ReturnPolicy: "",
        DaysToReturn: 0,
        ContactName: "",
        ShipFromAddress: "",
        ShipInfoCity: "",
        ShipInfoState: "",
        ShipInfoZipCode: "",
        ShipInfoPhone: "",
        PaymentTerms: "",
        CreditLimit: "",
        InventoryUpdateFrequency: "",
        AlwaysQuantity: "",
        AccountNumber: "",
        AccountType: "",
        ShippingMethod: "",
        VendorDepartments: []
    });
}

function PopulateVendorDepartments(vendorDepartments) {

    for (i = 0; i < vendorDepartments.length; i++) {
        $("#VDName_" + vendorDepartments[i].DepartmentId).val(vendorDepartments[i].Name);
        $("#VDEmail_" + vendorDepartments[i].DepartmentId).val(vendorDepartments[i].Email);
        $("#VDPhone_" + vendorDepartments[i].DepartmentId).val(vendorDepartments[i].PhoneNumber);
    }

}

function MapVendorDepartments(vendorDepartments, origin, vendor) {

    // Update existing records
    for (i = 0; i < vendorDepartments.length; i++) {
        vendorDepartments[i].Name = $("#VDName_" + vendorDepartments[i].DepartmentId).val();
        vendorDepartments[i].Email = $("#VDEmail_" + vendorDepartments[i].DepartmentId).val();
        vendorDepartments[i].PhoneNumber = $("#VDPhone_" + vendorDepartments[i].DepartmentId).val();
    }

    // Add new records
    for (i = 0; i < origin.length; i++) {

        var found = origin.some(function (dept) {
            if (vendorDepartments[i] != null)
                return dept.Id === vendorDepartments[i].DepartmentId;
            else {
                return false;
            }
        });
        
        if (!found) {
            vendorDepartments.push({
                Id: -1,
                VendorId: vendor.Id,
                DepartmentId: origin[i].Id,
                Name: $("#VDName_" + origin[i].Id).val(),
                Email: $("#VDEmail_" + origin[i].Id).val(),
                PhoneNumber: $("#VDPhone_" + origin[i].Id).val()
            });
        }
    }

    return vendorDepartments;
}

function VendorDepartmentModel(department) {
    var self = this;

    ko.mapping.fromJS(department, {}, self);
}