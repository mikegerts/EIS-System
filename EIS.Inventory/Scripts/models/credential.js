
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();
    self.modalTitle = ko.observable();

    self.marketplaces = ko.observableArray();
    self.modes = ko.observableArray([
        { Id: "TEST", Name: "TEST" },
        { Id: "LIVE", Name: "LIVE" }
    ]);
    self.marketplaceTypes = ko.observableArray([        
        { Id: "Amazon", Name: "Amazon" },
        { Id: "eBay", Name: "eBay" },
        { Id: "ShipStation", Name: "ShipStation" },
        { Id: "BigCommerce", Name: "BigCommerce" }
    ]);
    self.selectedMarketplaceType = ko.observable();
    self.marketplaceCredentials = ko.observableArray();
    self.companies = ko.observableArray();
    self.messageTemplates = ko.observableArray();
    self.marketplaceCredential = ko.observable();

    self.loadModel = function (modelId) {
        if (modelId == -1) {
            self.marketplaceCredential(initMarketplaceCredential(self.selectedMarketplaceType()));
            viewModel.modalTitle("Add New " + self.selectedMarketplaceType() + " Credentials");
            $("#loadingModal").hide();
        } else {
            $("#loadingModal").show();

            // get the credential model
            $.ajax({
                url: GET_CREDENTIAL_URL,
                data: { id: modelId },
                success: function (result) {
                    self.marketplaceCredential(new CredentialViewModel(result));
                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });
        }
    }

    self.loadData = function () {
        // get the list of marketplaceCredentials
        $.ajax({
            url: GET_CREDENTIALS_URL,
            success: function (results) {
                self.marketplaceCredentials(ko.utils.arrayMap(results, function (item) {
                    return new CredentialViewModel(item);
                }));
            }
        });

        // get the list of companies
        $.ajax({
            url: GET_COMPANIES_URL,
            success: function (results) {
                self.companies(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });

        // get the list of eBay Descriptions message templates
        $.ajax({
            url: GET_MESSAGE_TEMPLATES_URL + "?messageType=0",
            success: function (results) {
                self.messageTemplates(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });
    }

    self.saveCredential = function (credential, event) {
        if (!isValidateForm()) {
            return;
        }

        showSpinner();
        $(event.target).addClass("disabled");

        // mapped the ko object to plain JS object
        var credentialData = ko.mapping.toJS(credential);

        $.ajax({
            type: "POST",
            url: SAVE_CREDENTIAL_URL,
            data: JSON.stringify(credentialData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message("Error occured in trying to save the Marketplace Credential! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Marketplace Credential have been successfully saved!");
                setTimeout("$('#CredentialDialog').modal('hide'); viewModel.message(null);", 2000);
                viewModel.loadData();
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Marketplace Credential! <br/> " + result)
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.deleteCredential = function (credential, event) {
        $.confirm({
            title: "Delete Marketplace Credential",
            text: "Are you sure you want to delete credential: <strong> " + credential.Name() + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_CREDENTIAL_URL, { id: credential.Id }, function (result) {
                    if (result.Success) {
                        self.marketplaceCredentials.remove(credential);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }
}

function CredentialViewModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);

    if (item.MarketplaceType == "Amazon") {
        self.marketplaces = ko.observableArray(createAmazonMarketplaces(item.MarketplaceId));
        self.ServiceEndPoint("https://mws.amazonservices.com");
    } else if (item.MarketplaceType == "eBay") {
        self.marketplaces = ko.observableArray(createeBayMarketplaces(item.MarketplaceId));
        self.ServiceEndPoint("https://api.ebay.com/wsapi");
    } else if (item.MarketplaceType == "ShipStation") {
        self.marketplaces = ko.observableArray(createShipStationMarketplaces(item.MarketplaceId));
        self.ServiceEndPoint("https://ssapi.shipstation.com/");
    } else if (item.MarketplaceType == "BigCommerce") {
        self.marketplaces = ko.observableArray(createBigCommerceMarketplaces(item.MarketplaceId));
        self.ServiceEndPoint("https://store-mxi5a3.mybigcommerce.com/api/v2/");
    }

    self.marketplaceValueChanged = function (value, event) {
        var marketplaceMatched = ko.utils.arrayFirst(self.marketplaces(), function (item) {
            return item.Id() == self.MarketplaceId();
        });

        // set the endpoint of the selected marketplace
        self.ServiceEndPoint(marketplaceMatched.ServiceEndPoint());
    }
}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
}

function initMarketplaceCredential(selectedMarketplaceType) {
    return new CredentialViewModel({
        Id: -1,
        Mode: "TEST",
        Name: "",
        MarketplaceType: selectedMarketplaceType,
        CompanyId: "",
        DescriptionTemplateId: "",
        IsEnabled: true,
        IsDefault: false,
        ServiceEndPoint: "",
        MarketplaceId: "",
        MerchantId: "",
        AccessKeyId: "",
        SecretKey: "",
        AssociateId: "",
        SearchAccessKeyId: "",
        SearchSecretKey: "",
        SiteId: 0,
        ApplicationId: "",
        DeveloperId: "",
        CertificationId: "",
        UserToken: "",
        ApiKey: "",
        ApiSecretKey: "",
        PayPalEmailAddress: "",
        Username: ""
    });
}

function createBigCommerceMarketplaces(marketplaceId) {
    return [
      new ItemModel({
          Id: "BigCommerce",
          Name: "BigCommerce API",
          ServiceEndPoint: "https://store-mxi5a3.mybigcommerce.com/api/v2/",
          IsSelected: marketplaceId == "BigCommerce"
      })];
}

function createShipStationMarketplaces(marketplaceId) {
    return [
      new ItemModel({
          Id: "ShipStation",
          Name: "ShipStation API",
          ServiceEndPoint: "https://ssapi.shipstation.com/",
          IsSelected: marketplaceId == "ShipStation"
      })];
}

function createeBayMarketplaces(marketplaceId) {
    return [
       new ItemModel({
           Id: "0",
           Name: "eBay United States",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "1"
       }),
       new ItemModel({
           Id: "2",
           Name: "eBay Canada (English)",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "2"
       }),
       new ItemModel({
           Id: "3",
           Name: "eBay UK",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "3"
       }),
       new ItemModel({
           Id: "15",
           Name: "eBay Australia",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "15"
       }),
       new ItemModel({
           Id: "16",
           Name: "eBay Austria",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "16"
       }),
       new ItemModel({
           Id: "23",
           Name: "eBay Belgium",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "23"
       }),
       new ItemModel({
           Id: "71",
           Name: "eBay France",
           ServiceEndPoint: "https://api.ebay.com/wsapi",
           IsSelected: marketplaceId == "71"
       })
    ];
}

function createAmazonMarketplaces(marketplaceId) {
    return [
        new ItemModel({
            Id: "ATVPDKIKX0DER",
            Name: "Amazon US",
            ServiceEndPoint: "https://mws.amazonservices.com",
            IsSelected: marketplaceId == "ATVPDKIKX0DER"
        }),
        new ItemModel({
            Id: "A2EUQ1WTGCTBG2",
            Name: "Amazon CA",
            ServiceEndPoint: "https://mws.amazonservices.ca",
            IsSelected: marketplaceId == "A2EUQ1WTGCTBG2"
        }),
        new ItemModel({
            Id: "A1PA6795UKMFR9",
            Name: "Amazon DE",
            ServiceEndPoint: "https://mws-eu.amazonservices.com",
            IsSelected: marketplaceId == "A1PA6795UKMFR9"
        }),
        new ItemModel({
            Id: "A1RKKUPIHCS9HS",
            Name: "Amazon ES",
            ServiceEndPoint: "https://mws-eu.amazonservices.com",
            IsSelected: marketplaceId == "A1RKKUPIHCS9HS"
        }),
        new ItemModel({
            Id: "A13V1IB3VIYZZH",
            Name: "Amazon FR",
            ServiceEndPoint: "https://mws-eu.amazonservices.com",
            IsSelected: marketplaceId == "A13V1IB3VIYZZH"
        }),
        new ItemModel({
            Id: "A21TJRUUN4KGV",
            Name: "Amazon IN",
            ServiceEndPoint: "https://mws.amazonservices.in",
            IsSelected: marketplaceId == "A21TJRUUN4KGV"
        }),
        new ItemModel({
            Id: "APJ6JRA9NG5V4",
            Name: "Amazon IT",
            ServiceEndPoint: "https://mws-eu.amazonservices.com",
            IsSelected: marketplaceId == "APJ6JRA9NG5V4"
        }),
        new ItemModel({
            Id: "A1F83G8C2ARO7P",
            Name: "Amazon UK",
            ServiceEndPoint: "https://mws-eu.amazonservices.com",
            IsSelected: marketplaceId == "A1F83G8C2ARO7P"
        }),
        new ItemModel({
            Id: "A1VC38T7YXB528",
            Name: "Amazon JP",
            ServiceEndPoint: "https://mws.amazonservices.jp",
            IsSelected: marketplaceId == "A1VC38T7YXB528"
        }),
        new ItemModel({
            Id: "AAHKV2X7AFYLW",
            Name: "Amazon CN",
            ServiceEndPoint: "https://mws.amazonservices.com.cn",
            IsSelected: marketplaceId == "AAHKV2X7AFYLW"
        })
    ];
}