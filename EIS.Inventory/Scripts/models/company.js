
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.credentials = ko.observableArray();
    self.vendors = ko.observableArray();

    self.loadModel = function (modelId) {
        if (modelId == -1) {
            $("#loadingModal").show();
            self.company(createCompanyModel());
            $("#loadingModal").hide();
        } else {                        
            $.ajax({
                url: GET_COMP_VENDORS_URL,
                data: { id: modelId },
                success: function (result) {
                    self.vendors(result);
                },
                complete: function () {
                    //$("#loadingModal").hide();
                }
            });

            $.ajax({
                url: GET_COMP_CREDENTIAL_URL,
                data: { id: modelId },
                success: function (result) {
                    self.credentials(result);

                },
                complete: function () {
                    //$("#loadingModal").hide();
                }
            });
        }
        
    }
}

function deleteCompany(event, id, name) {
    $.confirm({
        title: "Delete Company",
        text: "Are you sure you want to delete this company: <strong> " + name + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(DELETE_COMPANY_URL, { id: id }, function (result) {
                if (result.Success) {
                    $(event).parent().parent().fadeOut();

                    // reload the page if there's no table records in the paged
                    if (($("#tblCompanies > tbody > tr:visible").length - 1) == 0)
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

function CompanyModel(company) {
    var self = this;

    var mapping = {
        "Vendors": {
            create: function (options) {
                return new ItemModel(options.data);
            }
        },
        "MarketplaceCredentials": {
            create: function (options) {
                return new ItemModel(options.data);
            }
        }
    }
    ko.mapping.fromJS(company, mapping, self);
}

function ItemModel(item) {
    var self = this;

    self.Id = ko.observable(item.Id);
    self.Name = ko.observable(item.Name);
    self.Mode = ko.observable(item.Mode);
    //ko.mapping.fromJS(item, {}, self);
}

function createCompanyModel() {
    return new CompanyModel({
        Id: -1,
        Name: "",
        Currency: "",
        Abbreviation: "",
        Email: "",
        Website: "",
        Address: "",
        City: "",
        State: "",
        ZipCode: "",
        Phone: "",
        IsDefault: "",
        MarketplaceCredentials: [],
        Vendors: []
    });
}