
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.templates = ko.observableArray();
    self.template = ko.observable("test");
    self.isAsinChanged = ko.observable(false);
    self.marketplaces = ko.observableArray();
    self.companies = ko.observableArray();
    self.vendors = ko.observableArray();
    self.productGroups = ko.observableArray();
    self.selectedVendor = ko.observable();
    self.selectedCompany = ko.observable();
    self.selectedProductGroup = ko.observable();
    self.images = ko.observableArray();
    self.image = ko.observable();
    self.yesNoOptions = [{ Id: true, Name: "Yes" }, { Id: false, Name: "No" }];    
    self.customExport = ko.observable();
    self.marketplaceFeed = ko.observable();
    self.vendorProductLinks = ko.observableArray();
    self.vendorProductResults = ko.observableArray();
    self.selectedVendorProducts = ko.observableArray();
    self.eisSKU = ko.observable();
    self.keyword = ko.observable();
    self.eBayCategoryModel = ko.observable(new eBayCategoryModel());
    self.bigcommerceproductcustomfields = ko.observableArray();
    self.BigCommerceCategoryModel = ko.observable(new BigCommerceCategoryModel());


    self.submitMarketplaceAction = function (object, event) {
        var selectedActionUrl = $("#marketplaceAction").val();
        if (selectedActionUrl == "")
            return false;

        if ($(event.target).hasClass("disabled"))
            return false;

        var eisSku = $("#EisSKU").val();
        $(event.target).addClass("disabled");
        $(event.target).addClass("fa-spin");
        
        self.type("info");
        self.message("Marketplace action is still on-going but you can still navigate to other page.");
        $("#messageStatus").removeClass("alert-success");

        $.ajax({
            type: "GET",
            url: selectedActionUrl,
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    self.type("error");
                    self.message(result.Error);
                    $("#messageStatus").removeClass("alert-info");
                    $("#messageStatus").removeClass("alert-success");
                    return;
                } else if (result.Success) {
                    self.type("success");
                    self.message(result.Success);
                } else {
                    $("#ProductTitle").val(result.ProductTitle);
                    $("#ASIN").val(result.ASIN);
                    $("#PackageQty").val(result.PackageQty);
                    $("#MapPrice").val(result.MapPrice);
                    $("#UPC").val(result.UPC);
                    $("#NumOfItems").val(result.NumOfItems);
                    $("#Condition").val(result.Condition);
                    $("#Brand").val(result.Brand);
                    $("#Color").val(result.Color);
                    $("#EAN").val(result.EAN);
                    $("#Label").val(result.Label);
                    $("#Model").val(result.Model);
                    $("#Size").val(result.Size);
                    $("#ProductGroup").val(result.ProductGroup);
                    $("#ProductTypeName").val(result.ProductTypeName);

                    self.type("success");
                    self.message("Product's info has been successfully retrieved!");
                }
            },
            complete: function () {
                $(event.target).removeClass("disabled");
                $(event.target).removeClass("fa-spin");
                setTimeout(function () { self.message(null) }, 5000);

                // reload the images
                self.loadImages(eisSku);
            }
        });
    }

    self.loadData = function () {
        // retrieve the list of product groups
        $.ajax({
            url: GET_PRODUCT_GROUPS_URL,
            success: function (results) {
                self.productGroups(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });

        // retrieve the list of marketplace
        $.ajax({
            url: GET_MARKETPLACES_URL,
            success: function (results) {
                self.marketplaces(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });

        // load all the vendors for filtering product
        $.ajax({
            url: GET_VENDORS_URL,
            success: function (results) {
                self.vendors(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });

        // load all the companies for filtering product
        $.ajax({
            url: GET_COMPANIES_URL,
            success: function (results) {
                self.companies(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });
    }

    self.loadDataModel = function (eisSKU) {
        self.eisSKU(eisSKU);
        // load the vendor products that links to this product
        $.ajax({
            url: GET_VENDDOR_PRODUCT_LINKS_URL + "?eisSKU=" + eisSKU,
            success: function (results) {
                self.vendorProductLinks(results);
            }
        });
    }

    self.loadImages = function (eisSKU) {
        // load the product images
        $.ajax({
            url: GET_PRODUCT_IMAGES_URL,
            data: { eisSku: eisSKU },
            success: function (results) {
                self.images(results);
            }
        });
    }

    self.searchVendorProducts = function (data, event) {
        if (self.keyword() == null)
            return false;

        $.ajax({
            url: SEARCH_VENDOR_PRODUCT_URL + "?keyword=" + self.keyword(),
            beforeSend: function () {
                $(event.target).text("Searching...")
            },
            success: function (results) {
                self.vendorProductResults.removeAll();
                if (results.length > 0) {
                    self.vendorProductResults(results);
                } else {
                    self.vendorProductResults.push({ Id: "", DisplayName: "No results found" })
                }
            },
            complete: function () {
                $(event.target).text("Search Vendor Products")
            }
        });
    }

    self.addVendorProductLinks = function (model, event) {
        // show the spiner and disable the button
        showSpinner();
        $(event.target).addClass("disabled");

        $.ajax({
            type: "POST",
            url: ADD_VENDOR_PRODUCT_LINKS_URL,
            data: JSON.stringify({ eisSKU: self.eisSKU(), selectedEisSupplierSKUs: self.selectedVendorProducts() }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $(event.target).removeClass("disabled");
                    return;
                }

                // reload the vendor product links and close the dialog
                self.loadDataModel(self.eisSKU());
                setTimeout("$('#ProductLinkDialog').modal('hide');", 1000);
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.deleteVendorProductLink = function (item) {
        $.confirm({
            title: "Delete Vendor Product Link",
            text: "Are you sure you want to delete this vendor product link from this product?<br/><strong> " + item.EisSupplierSKU + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_PRODUCT_LINK_URL, { eisSKU: item.EisSKU, EisSupplierSKU: item.EisSupplierSKU }, function (result) {
                    if (result.IsSuccess) {
                        self.vendorProductLinks.remove(item);
                    } else {
                        displayMessage(result.Error, "error")
                        fadeOutMessage();
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });

        return false;
    }


    self.loadMarketplaceFeedData = function (feedType) {
        if (recordsSelected == 0) {
            self.type("error");
            self.message("No records selected! Please select first the product records you want for marketplace/s feed.");
        } else {
            // display the message for items selected
            self.type("warning");
            self.message("You have selected " + recordsSelected + " records for marketplace/s feed.");
            $(".alertMsgStatus").removeClass("alert-error");
        }

        var modalTitle = "", modalInfoMsg = "", feedUrl ="";
        if (feedType == "product") {
            modalTitle = "Post Product Feed to Marketplaces";
            modalInfoMsg = "Please select lesser items to post as much as possible and take note that changes will take time to reflect in the product listing in marketplaces.";

            // update the link for submitting the feed
            feedUrl = SUBMIT_PRODUCT_FEED_URL;
        } else if (feedType == "revise") {
            modalTitle = "Post Revise Feed to Marketplaces";
            modalInfoMsg = "Please select lesser items to post as much as possible and  take note that changes will take time to reflect in the product listing in marketplaces.";

            // update the link for submitting the feed
            feedUrl = SUBMIT_REVISE_FEED_URL;
        } else if (feedType == "inventory") {
            modalTitle = "Post Inventory Feed to Marketplaces";
            modalInfoMsg = "Please select lesser items to post as much as possible and  take note that changes will take time to reflect in the product listing in marketplaces.";

            // update the link for submitting the feed
            feedUrl = SUBMIT_INVENTORY_FEED_URL;
        } else if (feedType == "price") {
            modalTitle = "Post Price Feed to Marketplaces";
            modalInfoMsg = "Please select lesser items to post as much as possible and  take note that changes will take time to reflect in the product listing in marketplaces.";

            // feedUrl the link for submitting the feed
            feedUrl = SUBMIT_PRICE_FEED_URL;
        } else if(feedType == "info") {
            modalTitle = "Get Info from Amazon";
            modalInfoMsg = "This action will get the proudcts' information of the selected items from Amazon.";
            feedUrl = SUBMIT_PRODUCTS_INFO_AMAZON_URL;
        } else if (feedType == "eBayCategories") {
            modalTitle = "Get eBay Suggested Categories";
            modalInfoMsg = "This action will get bulk eBay suggested categories for the selected products.";
            feedUrl = SUBMIT_eBAY_CATEGORIES_URL;
        } else if (feedType == "eBayEndItem") {
            modalTitle = "Post eBay End Product Listing";
            modalInfoMsg = "This action will end the product listing for the selected products on eBay.";
            feedUrl = SUBMIT_eBAY_END_PRODUCTS_URL;
        } else if (feedType == "ebayReListing") {
            modalTitle = "Post eBay Product ReListing";
            modalInfoMsg = "This action will relist the selected products on eBay.";
            feedUrl = "/product/_SubmiteBayProductReListing";
        } else {
            alert("Unknown feed type for marketplace: " + feedType);
        }
        
        self.marketplaceFeed(new MarketplaceFeedModel({
            modalTitle: modalTitle,
            modalInfoMsg: modalInfoMsg,
            feedUrl: feedUrl,
            feedType: feedType,
            Mode: "TEST",
            SearchString: "",
            Marketplaces: [],
            Companies: [],
            SelectedEisSKUs: [],
            ExcludedEisSKUs: [],
            IsAllProductItems: false,
            ProductGroupId: "",
            VendorId: -1,
            CompanyId: -1,
            QuantityFrom: 0,
            QuantityTo: 0,
            IsKit: null,
            WithImages: -1,
            SkuType: null,
            IsSKULinked: null,
            IsAmazonEnabled: null,
            HasASIN: null,
        }))
    }

    self.loadCustomReportTemplates = function () {
        $.ajax({
            url: GET_TEMPLATE_URL,
            success: function (results) {
                console.log(results);
                self.templates(results.templates);
            }
        });

    }

    self.loadSelectedTemplate = function (item, event) {
        //Update CustomExportModel binding
        var arr = item.Fields.split(",");

        self.customExport().availableProductFields.removeAll();
        self.customExport().availableProductFields(getProductFieldsArr([])
        .concat(getProductAmazonFieldsArr([]))
        .concat(getProducteBayFieldArr([])));

        self.customExport().selectedProductFields.removeAll();
    

        $.each(arr, function (index, field) {
            // get the item with the item id
            var itemFound = $.grep(self.customExport().availableProductFields(), function (item) {
                return item.Id() == field;
            });

            if (itemFound.length != 0) {
                self.customExport().availableProductFields.remove(itemFound[0]);
                self.customExport().selectedProductFields.push(itemFound[0]);
            }
        });
   
        self.customExport()
            .Delimiter(item.FileFormat)
            .SortBy(item.SortField)
            .ProductFields(item.Fields.split(","))
            .TemplateName(item.Name);

        $('#loadTemplateModal').modal('hide');

    }

    self.deleteSelectedTemplate = function (item,event) {
        $.post(DELETE_TEMPLATE_URL, { id: item.Id }, function (result) {
            self.loadCustomReportTemplates();
        });
    }
    
    self.loadCustomExportProduct = function (rowsSelected) {
        if (rowsSelected == 0) {
            self.type("error");
            self.message("No records selected! Please select first the product records you want to export");
        } else {
            // display the message for items selected
            self.type("warning");
            self.message("You have selected " + rowsSelected + " records to export. Please ensure that you have selected EisSKU product field.");
            $(".alertMsgStatus").removeClass("alert-error");
        }

        // set the excluded and selecte product EIS SKUs if there's any
        var groupId = $("#ProductGroupId").val();
        var vId = $("#vendorId").val();
        var cId = $("#companyId").val();
        var searchString = $("#SearchString").val();
        var qtyFrom = $("#inventoryQtyFrom").val();
        var qtyTo = $("#inventoryQtyTo").val();
        var isKit = $("#isKit").val();
        var withImages = $("#withImages").val();
        var skuType = $("#skuType").val();
        var isSKULinked = $("#isSKULinked").val();
        var isAmazonEnabled = $("#isAmazonEnabled").val();
        var hasASIN = $("#hasASIN").val();

        self.customExport(new CustomExportModel({
            TemplateName: "",
            Delimiter: ",",
            SortBy: "",
            SearchString: searchString,
            ProductFields: [],
            SelectedEisSKUs: selectedProductEisSKUs,
            ExcludedEisSKUs: unselectedProductEisSKUs,
            IsAllProductItems: isSelectAllPages,
            ProductGroupId: groupId,
            VendorId: vId,
            CompanyId: cId,
            QuantityFrom: qtyFrom,
            QuantityTo: qtyTo,
            RequestedDate: new Date(),
            IsKit: isKit,
            SkuType: skuType,
            IsSKULinked: isSKULinked,
            WithImages: withImages,
            IsAmazonEnabled: isAmazonEnabled,
            HasASIN: hasASIN,
        }));
    }

    self.resetFilters = function () {
        self.selectedVendor(null);
        self.selectedCompany(null);
        self.selectedProductGroup(null);

        $("#filterForm #SearchString").val("");
        $("#filterForm #inventoryQtyFrom").val("");
        $("#filterForm #inventoryQtyTo").val(""); 
        $("#filterForm #withImages").val(-1);
        $("#filterForm #IsKit").val("");
        $("#filterForm #SkuType").val("");
        $("#filterForm #IsSKULinked").val("");
        $("#filterForm #IsAmazonEnabled").val("");
        $("#filterForm #HasASIN").val("");
        $("#filterForm #deletefilter").hide();
        $("#filterForm #filterName").val("");
    }

    self.deleteImage = function (image, event) {
        $.confirm({
            title: "Delete Product Image",
            text: "Are you sure you want to delete this product image: <strong> " + image.Url + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.ajax({
                    type: "POST",
                    url: DELETE_PRODUCT_IMAGE_URL,
                    data: JSON.stringify({ id: image.Id }),
                    contentType: "application/json",
                    success: function (result) {
                        if (result.Error)
                            alert(result.Error);
                        else
                            self.images.remove(image);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });        
    }
    
    self.loadbigcommercecustomfields = function (EisSKU) {
        // load the product custom fields
        $.ajax({
            url: GET_BIGCOMMERCE_PRODUCTCUSTOMFIELDS_URL,
            data: { EisSKU: EisSKU },
            success: function (results) {                
                self.bigcommerceproductcustomfields(results.customFieldList);
            }
        });
    }
}

function MarketplaceFeedModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);
    self.marketplaceMode = ko.observable("TEST");
    self.modalTitle = ko.observable(model.modalTitle);
    self.modalInfoMsg = ko.observable(model.modalInfoMsg);
    self.feedUrl = ko.observable(model.feedUrl);
    self.feedType = ko.observable(model.feedType);
    self.hasRecordsSelected = ko.observable(recordsSelected != 0)

    self.toggleMarketplaceMode = function (data, event) {
        self.Mode($(event.target).val());
        return true;
    }

    self.isForeBayOnly = ko.pureComputed(function () {
        return self.feedType() === "eBayEndItem" || self.feedType() === "ebayReListing";
    });

    self.Marketplaces = ko.pureComputed(function () {
        var selectedMarketplaces = $.map(viewModel.marketplaces(), function (item) {
            return item.IsChecked() == true ? item.Id() : null;
        });

        if (selectedMarketplaces.length != 0) {
            $("div#marketplaces").parents(".form-group").removeClass("has-error");
        }

        // if the feed type is eBayEndItem, then let's explicityly set the marketplace to 'eBay'
        if (self.isForeBayOnly())
            return "eBay";
        return selectedMarketplaces;
    });

    self.Companies = ko.pureComputed(function () {
        var selectedcompanies = $.map(viewModel.companies(), function (item) {
            return item.IsChecked() == true ? item.Id() : null;
        });

        if (selectedcompanies.length != 0) {
            $("div#companies").parents(".form-group").removeClass("has-error");
        }
        return selectedcompanies;
    });

    self.submitMarketplaceFeed = function (model, event) {
        if (!isValidFormData("marketplace-feed-form"))
            return;

        // set the excluded and selecte product EIS SKUs if there's any
        self.ExcludedEisSKUs(unselectedProductEisSKUs);
        self.SelectedEisSKUs(selectedProductEisSKUs)
        self.IsAllProductItems(isSelectAllPages);
        self.SearchString($("#SearchString").val());
        self.ProductGroupId($("#ProductGroupId").val());
        self.VendorId($("#vendorId").val());
        self.CompanyId($("#companyId").val());
        self.QuantityFrom($("#inventoryQtyFrom").val());
        self.QuantityTo($("#inventoryQtyTo").val());
        self.IsKit($("#isKit").val());
        self.WithImages($("#withImages").val());
        self.SkuType($("#skuType").val());
        self.IsSKULinked($("#isSKULinked").val());
        self.IsAmazonEnabled($("#isAmazonEnabled").val());
        self.HasASIN($("#hasASIN").val());
        
        showSpinner();
        $(event.target).addClass("disabled");

        $.ajax({
            type: "POST",
            url: self.feedUrl(),
            data: JSON.stringify(ko.mapping.toJS(model)),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $("#alertMsgStatus").removeClass("alert-info").removeClass("alert-warning");
                    viewModel.type("danger");
                    viewModel.message(result.Error);
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Selected products have been posted to the marketplaces.");
                setTimeout("$('#MarketplaceDialog').modal('hide');", 2000);
            },
            error: function (result) {
                $("#alertMsgStatus").removeClass("alert-info").removeClass("alert-warning");
                viewModel.type("danger");
                viewModel.message(result.Error);
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.submitProductInfoFeed = function (model, event) {

        // set the excluded and selecte product EIS SKUs if there's any
        self.ExcludedEisSKUs(unselectedProductEisSKUs);
        self.SelectedEisSKUs(selectedProductEisSKUs)
        self.IsAllProductItems(isSelectAllPages);
        self.SearchString($("#SearchString").val());
        self.ProductGroupId($("#ProductGroupId").val());
        self.VendorId($("#vendorId").val());
        self.CompanyId($("#companyId").val());
        self.QuantityFrom($("#inventoryQtyFrom").val());
        self.QuantityTo($("#inventoryQtyTo").val());
        self.WithImages($("#withImages").val());
        self.IsKit($("#isKit").val());
        self.SkuType($("#skuType").val());
        self.IsSKULinked($("#isSKULinked").val());
        self.IsAmazonEnabled($("#isAmazonEnabled").val());
        self.HasASIN($("#hasASIN").val());

        showSpinner();
        $(event.target).addClass("disabled");


        $.ajax({
            type: "POST",
            url: model.feedUrl(),
            data: JSON.stringify(ko.mapping.toJS(model)),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $(".alertMsgStatus").removeClass("alert-warning");
                    viewModel.type("danger");
                    viewModel.message(result.Error);
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message(result.Success);
                setTimeout("$('#GetProductsInfoDialog').modal('hide');", 2000);


                // let's check if there is job id, this is specifically for eBay get suggested categories
                if (result.JobId != 0) {
                    // download the file
                    document.getElementById("download_frame").src = DOWNLOAD_eBAY_BULK_CATEGORIES_URL + "?jobId=" + result.JobId;
                }
            },
            error: function (result) {
                $(".alertMsgStatus").removeClass("alert-info").removeClass("alert-warning");
                viewModel.type("danger");
                viewModel.message(result.Error);
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }
}

function CustomExportModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);
    self.availableProductFields = ko.observableArray();
    self.selectedProductFields = ko.observableArray();
    self.selectedFieldsToAdd = ko.observableArray();
    self.selectedFieldsToRemove = ko.observableArray();
    self.fileFormats = ko.observableArray(getFileFormats());
    self.productKinds = ko.observableArray(["General", "Amazon", "eBay", "BigCommerce", "Buycom"]);

    self.loadAllProductFields = function () {
        self.availableProductFields(getProductFieldsArr([])
        .concat(getProductAmazonFieldsArr([]))
        .concat(getProducteBayFieldArr([]))
        .concat(getProductBigCommerceFieldArr([])));
    }

    self.productKindChanged = function (data, event) {
        var selectedKind = event.target.value;
        var availableFields = [];

        if (selectedKind == "General")
            availableFields = getProductFieldsArr(self.ProductFields());
        else if (selectedKind == "Amazon")
            availableFields = getProductAmazonFieldsArr(self.ProductFields());
        else if (selectedKind == "eBay")
            availableFields = getProducteBayFieldArr(self.ProductFields());
        else if (selectedKind == "BigCommerce")
            availableFields = getProductBigCommerceFieldArr(self.ProductFields());
        else if (selectedKind == "Buycom")
            availableFields = getProductBuycomFieldArr(self.ProductFields());
        else
            availableFields = getProductFieldsArr(self.ProductFields())
                .concat(getProductAmazonFieldsArr(self.ProductFields()))
                .concat(getProducteBayFieldArr(self.ProductFields()))
                .concat(getProductBigCommerceFieldArr(self.ProductFields()))
                .concat(getProductBuycomFieldArr(self.ProductFields()));

        var unselectedProductFields = $.grep(availableFields, function (item) {
            return item.IsChecked() === false;
        });

        self.availableProductFields(unselectedProductFields);
    }

    self.addSelectedFields = function (model, event) {
        $.each(self.selectedFieldsToAdd(), function (index, field) {
            // get the item with the item id
            var itemFound = $.grep(self.availableProductFields(), function (item) {
                return item.Id() == field;
            });
            
            if (itemFound.length != 0) {
                self.selectedProductFields.push(itemFound[0]);
                self.availableProductFields.remove(itemFound[0]);
                self.ProductFields.push(itemFound[0].Id());
            }
        });
    }
    self.removeSelectedFields = function (model, event) {
        $.each(self.selectedFieldsToRemove(), function (index, field) {
            // get the item with the item id
            var itemFound = $.grep(self.selectedProductFields(), function (item) {
                return item.Id() == field;
            });

            if (itemFound.length != 0) {
                self.availableProductFields.push(itemFound[0]);
                self.selectedProductFields.remove(itemFound[0]);
                self.ProductFields.remove(itemFound[0].Id());
            }
        });
    }
    self.downloadCustomExport = function (model, event) {
        if (!isValidFormData("custom-export-form")) {
            return;
        }

        if (self.ProductFields().length == 0) {
            $("div.alertMsgStatus").removeClass("alert-warning");
            viewModel.type("error");
            viewModel.message("Can not export product with no fields selected. Please select product fields to export.");
            return false;
        }

        showSpinner();
        $(event.target).addClass("disabled");
        $("#customExportForm").submit();

        // set the alert message
        viewModel.type("info");
        viewModel.message("Custom export for products has been started. Please wait for a moment.");
        setTimeout("$('#CustomExportDialog').modal('hide');", 2000);
        hideSpinner();
    }

    self.saveTemplate = function (model, event) {
        if (self.ProductFields().length == 0 || self.SortBy() == null|| self.TemplateName() == "") {
            alert("Please complete the required fields before saving");
        }
        else{
   
            var model = {
                Name: self.TemplateName(),
                FileFormat: self.Delimiter(),
                SortField: self.SortBy(),
                Fields: self.ProductFields().join()
            };

            $.post(SAVE_TEMPLATE_URL, { model: model }, function (result) {
                document.getElementById("successAlert").style.display = 'inline';
                setTimeout(function () { document.getElementById("successAlert").style.display = 'none'; }, 3000);

            });
        }
    }

    // load all product fields
    self.loadAllProductFields();
}


function deleteProduct(source, eisSku) {
    $.confirm({
        title: "Delete Product",
        text: "Are you sure you want to delete product: <strong> " + eisSku + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(DELETE_PRODUCT_URL, { id: eisSku }, function (result) {
                if (result.Success) {                    
                    $(source).parent().parent().fadeOut();

                    // reload the page if there's no table records in the paged
                    if (($("#tblProducts > tbody > tr:visible").length - 1) == 0)
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

    return false;
}

function deleteBulkProducts() {
    var postData = {
        productGroupId: $("#ProductGroupId").val(),
        vendorId: $("#vendorId").val(),
        companyId: $("#companyId").val(),
        searchString: $("#SearchString").val(),
        quantityFrom: $("#inventoryQtyFrom").val(),
        quantityTo: $("#inventoryQtyTo").val(),
        isKit: $("#isKit").val(),
        skuType: $("#skuType").val(),
        isSKULinked: $("#isSKULinked").val(),
        withImages: $("#withImages").val(),
        isAmazonEnabled: $("#isAmazonEnabled").val(),
        hasASIN: $("#hasASIN").val(),
        selectedEisSKUs: selectedProductEisSKUs,
        excludedEisSKUs: unselectedProductEisSKUs,
        isAllProductItems: isSelectAllPages,
    };

    $.confirm({
        title: "Bulk Delete Products",
        text: "Are you sure you want to delete <strong>" + recordsSelected + "</strong> product items?",
        cancel: function () {
            return false;
        },
        confirm: function () {
            displayMessage(("EIS is on currently deleting " + recordsSelected + " products. This will take a while"), "warning");
            $.post(DELETE_PRODUCTS_URL, { model: postData }, function (result) {
                if (result.Success) {
                    displayMessage(result.Success + " Reloadin the page...", "success");
                    setTimeout(function () { location.reload(); }, 4000);
                } else {
                    displayMessage(result.Error, "error")
                    fadeOutMessage();
                }
            });
        },
        confirmButton: "Yes, I am",
        confirmButtonClass: "btn-warning"
    });

    return false;
}

function eBayCategoryModel() {
    var self = this;

    self.keyword = ko.observable();
    self.selectedCategory = ko.observable();
    self.categories = ko.observableArray();

    self.findCategoryClicked = function (model, event) {
        $("#loadingModal").show();

        $.ajax({
            url: GET_EBAY_SUGGESTED_CATEGORIES_URL,
            data: { "keyword" : model.keyword() },
            success: function (results) {
                self.categories(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.useSelectedCategoryClicked = function (model, event) {        
        // set the value for ebay category
        $("#CategoryId").val(model.selectedCategory());
        $("#CategoryName").val($("#SuggestedeBayCategory option:selected").text());
    }
}

function BigCommerceCategoryModel() {
    var self = this;

    self.keyword = ko.observable();
    self.selectedCategory = ko.observable();
    self.categories = ko.observableArray();

    self.findCategoryClicked = function (model, event) {
        $("#loadingModalBigCommerce").show();

        $.ajax({
            url: GET_BIGCOMMERCE_SUGGESTED_CATEGORIES_URL,
            data: { "keyword": model.keyword() },
            success: function (results) {
                self.categories(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            },
            complete: function () {
                $("#loadingModalBigCommerce").hide();
            }
        });
    }

    self.useSelectedCategoryClicked = function (model, event) {
        // set the value for bigcommerce category
        if ($("#Categories").val() != "") {
            var addCategory = $("#Categories").val() + "," + model.selectedCategory();
            $("#Categories").val(addCategory);

            var categoryName = $("#CategoryName").val() + " | " + $("#SuggestedBigCommerceCategory option:selected").text();

            $("#CategoryName").val(categoryName);
        } else {
            $("#Categories").val(model.selectedCategory());
            $("#CategoryName").val($("#SuggestedBigCommerceCategory option:selected").text());
        }

    }

    self.downloadReportCategoriesClicked = function (model, event) {

        showSpinner();

        $("#bigCommerceCategoryForm").submit();

        // set the alert message
        viewModel.type("info");
        viewModel.message("BigCommerce export for categories has been started. Please wait for a moment.");
        setTimeout("$('#loadingModalBigCommerce').modal('hide');", 2000);

        hideSpinner();
        $("#loadingModalBigCommerce").hide();

        //$.ajax({
        //    url: EXPORT_BIGCOMMERCE_SUGGESTED_CATEGORIES_URL,
        //    type: "POST",
        //    success: function (results) {
                
        //    },
        //    complete: function () {
        //    }
        //});

    }
}

function ClearBigCommerceCategories() {
    $("#CategoryName").val("");
    $("#Categories").val("");
}

function TemplateViewModel(item) {
    var self = this;
    ko.mapping.fromJS(item, {}, self);

}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.IsChecked = ko.observable(item.IsChecked || false);

    self.selectAll = function (item, event) {
        $(event.target).select();
    }
}

function getProductFieldsArr(selectedFields) {
    return [
        new ItemModel({ Id: "p.EisSKU", Name: "EisSKU", IsChecked: selectedFields.indexOf("p.EisSKU") > -1, Sort: "" }),
        new ItemModel({ Id: "p.CompanyId", Name: "General-CompanyId", IsChecked: selectedFields.indexOf("p.CompanyId") > -1, Sort: "" }),
        new ItemModel({ Id: "p.Name", Name: "General-Name", IsChecked: selectedFields.indexOf("p.Name") > -1, Sort: "" }),
        new ItemModel({ Id: "p.Description", Name: "General-Description", IsChecked: selectedFields.indexOf("p.Description") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ShortDescription", Name: "General-ShortDescription", IsChecked: selectedFields.indexOf("p.ShortDescription") > -1, Sort: "" }),
        new ItemModel({ Id: "p.Category", Name: "General-Category", IsChecked: selectedFields.indexOf("p.Category") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ProductTypeId", Name: "General-ProductTypeId", IsChecked: selectedFields.indexOf("p.ProductTypeId") > -1, Sort: "" }),
        new ItemModel({ Id: "p.UPC", Name: "General-UPC", IsChecked: selectedFields.indexOf("p.UPC") > -1, Sort: "" }),
        new ItemModel({ Id: "p.SellerPrice", Name: "General-SellerPrice", IsChecked: selectedFields.indexOf("p.SellerPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "p.PkgLength", Name: "General-PkgLength", IsChecked: selectedFields.indexOf("p.PkgLength") > -1, Sort: "" }),
        new ItemModel({ Id: "p.PkgWidth", Name: "General-PkgWidth", IsChecked: selectedFields.indexOf("p.PkgWidth") > -1, Sort: "" }),
        new ItemModel({ Id: "p.PkgHeight", Name: "General-PkgHeight", IsChecked: selectedFields.indexOf("p.PkgHeight") > -1, Sort: "" }),
        new ItemModel({ Id: "p.PkgLenghtUnit", Name: "General-PkgLenghtUnit", IsChecked: selectedFields.indexOf("p.PkgLenghtUnit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.PkgWeight", Name: "General-PkgWeight", IsChecked: selectedFields.indexOf("p.PkgWeight") > -1, Sort: "" }),
        new ItemModel({ Id: "p.PkgWeightUnit", Name: "General-PkgWeightUnit", IsChecked: selectedFields.indexOf("p.PkgWeightUnit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ItemLength", Name: "General-ItemLength", IsChecked: selectedFields.indexOf("p.ItemLength") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ItemWidth", Name: "General-ItemWidth", IsChecked: selectedFields.indexOf("p.ItemWidth") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ItemHeight", Name: "General-ItemHeight", IsChecked: selectedFields.indexOf("p.ItemHeight") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ItemLenghtUnit", Name: "General-ItemLenghtUnit", IsChecked: selectedFields.indexOf("p.ItemLenghtUnit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ItemWeight", Name: "General-ItemWeight", IsChecked: selectedFields.indexOf("p.ItemWeight") > -1, Sort: "" }),
        new ItemModel({ Id: "p.ItemWeightUnit", Name: "General-ItemWeightUnit", IsChecked: selectedFields.indexOf("p.ItemWeightUnit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.EAN", Name: "General-EAN", IsChecked: selectedFields.indexOf("p.EAN") > -1, Sort: "" }),
        new ItemModel({ Id: "p.Brand", Name: "General-Brand", IsChecked: selectedFields.indexOf("p.Brand") > -1, Sort: "" }),
        new ItemModel({ Id: "p.Color", Name: "General-Color", IsChecked: selectedFields.indexOf("p.Color") > -1, Sort: "" }),
        new ItemModel({ Id: "p.Model", Name: "General-Model", IsChecked: selectedFields.indexOf("p.Model") > -1, Sort: "" }),
        new ItemModel({ Id: "p.IsKit", Name: "General-IsKit", IsChecked: selectedFields.indexOf("p.IsKit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.SkuType", Name: "General-SkuType", IsChecked: selectedFields.indexOf("p.SkuType") > -1, Sort: "" }),
        new ItemModel({ Id: "p.GuessedWeight", Name: "General-GuessedWeight", IsChecked: selectedFields.indexOf("p.GuessedWeight") > -1, Sort: "" }),
        new ItemModel({ Id: "p.GuessedWeightUnit", Name: "General-GuessedWeightUnit", IsChecked: selectedFields.indexOf("p.GuessedWeightUnit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.AccurateWeight", Name: "General-AccurateWeight", IsChecked: selectedFields.indexOf("p.AccurateWeight") > -1, Sort: "" }),
        new ItemModel({ Id: "p.AccurateWeightUnit", Name: "General-AccurateWeightUnit", IsChecked: selectedFields.indexOf("p.AccurateWeightUnit") > -1, Sort: "" }),
        new ItemModel({ Id: "p.GuessedShipping", Name: "General-GuessedShipping", IsChecked: selectedFields.indexOf("p.GuessedShipping") > -1, Sort: "" }),
        new ItemModel({ Id: "p.AccurateShipping", Name: "General-AccurateShipping", IsChecked: selectedFields.indexOf("p.AccurateShipping") > -1, Sort: "" }),
        new ItemModel({ Id: "p.IsBlacklisted", Name: "General-IsBlacklisted", IsChecked: selectedFields.indexOf("p.IsBlacklisted") > -1, Sort: "" }),
        // add these fiedls from vendorproducts
        new ItemModel({ Id: "vendor_product.EisSupplierSKU", Name: "EisSupplierSKU", IsChecked: selectedFields.indexOf("vendor_product.EisSupplierSKU") > -1, Sort: "" }),
        new ItemModel({ Id: "vendor_product.Quantity", Name: "Quantity", IsChecked: selectedFields.indexOf("vendor_product.Quantity") > -1, Sort: "" }),
        new ItemModel({ Id: "vendor_product.SupplierPrice", Name: "SupplierPrice", IsChecked: selectedFields.indexOf("vendor_product.SupplierPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "1", Name: "ImageUrls", IsChecked: selectedFields.indexOf("1") > -1, Sort: "" })
    ];
}

function getProductAmazonFieldsArr(selectedFields) {
    return [
        new ItemModel({ Id: "pa.ASIN", Name: "Amazon-ASIN", IsChecked: selectedFields.indexOf("pa.ASIN") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.Price", Name: "Amazon-Price", IsChecked: selectedFields.indexOf("pa.Price") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.LeadtimeShip", Name: "Amazon-LeadtimeShip", IsChecked: selectedFields.indexOf("pa.LeadtimeShip") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.PackageQty", Name: "Amazon-PackageQty", IsChecked: selectedFields.indexOf("pa.PackageQty") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.NumOfItems", Name: "Amazon-NumOfItems", IsChecked: selectedFields.indexOf("pa.NumOfItems") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.MaxOrderQty", Name: "Amazon-MaxOrderQty", IsChecked: selectedFields.indexOf("pa.MaxOrderQty") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.ProductTitle", Name: "Amazon-ProductTitle", IsChecked: selectedFields.indexOf("pa.ProductTitle") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.MapPrice", Name: "Amazon-MapPrice", IsChecked: selectedFields.indexOf("pa.MapPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.IsAllowGiftWrap", Name: "Amazon-IsAllowGiftWrap", IsChecked: selectedFields.indexOf("pa.IsAllowGiftWrap") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.IsAllowGiftMsg", Name: "Amazon-IsAllowGiftMsg", IsChecked: selectedFields.indexOf("pa.IsAllowGiftMsg") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.Condition", Name: "Amazon-Condition", IsChecked: selectedFields.indexOf("pa.Condition") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.ConditionNote", Name: "Amazon-ConditionNote", IsChecked: selectedFields.indexOf("pa.ConditionNote") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.FulfilledBy", Name: "Amazon-FulfilledBy", IsChecked: selectedFields.indexOf("pa.FulfilledBy") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.FbaSKU", Name: "Amazon-FbaSKU", IsChecked: selectedFields.indexOf("pa.FbaSKU") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.IsEnabled", Name: "Amazon-IsEnabled", IsChecked: selectedFields.indexOf("pa.IsEnabled") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.ProductGroup", Name: "Amazon-ProductGroup", IsChecked: selectedFields.indexOf("pa.ProductGroup") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.ProductTypeName", Name: "Amazon-ProductTypeName", IsChecked: selectedFields.indexOf("pa.ProductTypeName") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.TaxCode", Name: "Amazon-TaxCode", IsChecked: selectedFields.indexOf("pa.TaxCode") > -1, Sort: "" }),
        new ItemModel({ Id: "pa.SafetyQty", Name: "Amazon-SafetyQty", IsChecked: selectedFields.indexOf("pa.SafetyQty") > -1, Sort: "" }),
        //new ItemModel({ Id: "pa.WeightBox1", Name: "Amazon-WeightBox1", IsChecked: selectedFields.indexOf("pa.WeightBox1") > -1, Sort: "" }),
        //new ItemModel({ Id: "pa.WeightBox1Unit", Name: "Amazon-WeightBox1Unit", IsChecked: selectedFields.indexOf("pa.WeightBox1Unit") > -1, Sort: "" }),
        //new ItemModel({ Id: "pa.WeightBox2", Name: "Amazon-WeightBox2", IsChecked: selectedFields.indexOf("pa.WeightBox2") > -1, Sort: "" }),
        //new ItemModel({ Id: "pa.WeightBox2Unit", Name: "Amazon-WeightBox2Unit", IsChecked: selectedFields.indexOf("pa.WeightBox2Unit") > -1, Sort: "" })
    ];
}

function getProducteBayFieldArr(selectedFields) {
    return [
        new ItemModel({ Id: "pe.ItemId", Name: "eBay-ItemId", IsChecked: selectedFields.indexOf("pe.ItemId") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.Title", Name: "eBay-Title", IsChecked: selectedFields.indexOf("pe.Title") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.SubTitle", Name: "eBay-SubTitle", IsChecked: selectedFields.indexOf("pe.SubTitle") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.Description", Name: "eBay-Description", IsChecked: selectedFields.indexOf("pe.Description") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.ListingQuantity", Name: "eBay-ListingQuantity", IsChecked: selectedFields.indexOf("pe.ListingQuantity") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.CategoryId", Name: "eBay-CategoryId", IsChecked: selectedFields.indexOf("pe.CategoryId") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.StartPrice", Name: "eBay-StartPrice", IsChecked: selectedFields.indexOf("pe.StartPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.ReservePrice", Name: "eBay-ReservePrice", IsChecked: selectedFields.indexOf("pe.ReservePrice") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.BinPrice", Name: "eBay-BinPrice", IsChecked: selectedFields.indexOf("pe.BinPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.ListType", Name: "eBay-ListType", IsChecked: selectedFields.indexOf("pe.ListType") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.Duration", Name: "eBay-Duration", IsChecked: selectedFields.indexOf("pe.Duration") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.Location", Name: "eBay-Location", IsChecked: selectedFields.indexOf("pe.Location") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.Condition_", Name: "eBay-Condition", IsChecked: selectedFields.indexOf("pe.Condition_") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.DispatchTimeMax", Name: "eBay-DispatchTimeMax", IsChecked: selectedFields.indexOf("pe.DispatchTimeMax") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.IsBoldTitle", Name: "eBay-IsBoldTitle", IsChecked: selectedFields.indexOf("pe.IsBoldTitle") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.IsOutOfStockListing", Name: "eBay-IsOutOfStockListing", IsChecked: selectedFields.indexOf("pe.IsOutOfStockListing") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.IsRequireAutoPayment", Name: "eBay-IsRequireAutoPayment", IsChecked: selectedFields.indexOf("pe.IsRequireAutoPayment") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.IsEnabled", Name: "eBay-IsEnabled", IsChecked: selectedFields.indexOf("pe.IsEnabled") > -1, Sort: "" }),
        new ItemModel({ Id: "pe.SafetyQty", Name: "eBay-SafetyQty", IsChecked: selectedFields.indexOf("pe.SafetyQty") > -1, Sort: "" })
    ];
}

function getProductBigCommerceFieldArr(selectedFields) {
    return [
        new ItemModel({ Id: "pbc.ProductId", Name: "BigCommerce-ProductId", IsChecked: selectedFields.indexOf("pbc.ProductId") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.IsEnabled", Name: "BigCommerce-IsEnabled", IsChecked: selectedFields.indexOf("pbc.IsEnabled") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.Description", Name: "BigCommerce-Description", IsChecked: selectedFields.indexOf("pbc.Description") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.Title", Name: "BigCommerce-Title", IsChecked: selectedFields.indexOf("pbc.Title") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.Price", Name: "BigCommerce-Price", IsChecked: selectedFields.indexOf("pbc.Price") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.Condition", Name: "BigCommerce-Condition", IsChecked: selectedFields.indexOf("pbc.Condition") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.Categories", Name: "BigCommerce-Categories", IsChecked: selectedFields.indexOf("pbc.Categories") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.RetailPrice", Name: "BigCommerce-RetailPrice", IsChecked: selectedFields.indexOf("pbc.RetailPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.PrimaryImage", Name: "BigCommerce-PrimaryImage", IsChecked: selectedFields.indexOf("pbc.PrimaryImage") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.FixedCostShippingPrice", Name: "BigCommerce-FixedCostShippingPrice", IsChecked: selectedFields.indexOf("pbc.FixedCostShippingPrice") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.Brand", Name: "BigCommerce-Brand", IsChecked: selectedFields.indexOf("pbc.Brand") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.ProductsType", Name: "BigCommerce-ProductsType", IsChecked: selectedFields.indexOf("pbc.ProductsType") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.InventoryLevel", Name: "BigCommerce-InventoryLevel", IsChecked: selectedFields.indexOf("pbc.InventoryLevel") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.InventoryWarningLevel", Name: "BigCommerce-InventoryWarningLevel", IsChecked: selectedFields.indexOf("pbc.InventoryWarningLevel") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.InventoryTracking", Name: "BigCommerce-InventoryTracking", IsChecked: selectedFields.indexOf("pbc.InventoryTracking") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.OrderQuantityMinimum", Name: "BigCommerce-OrderQuantityMinimum", IsChecked: selectedFields.indexOf("pbc.OrderQuantityMinimum") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.OrderQuantityMaximum", Name: "BigCommerce-OrderQuantityMaximum", IsChecked: selectedFields.indexOf("pbc.OrderQuantityMaximum") > -1, Sort: "" }),
        new ItemModel({ Id: "pbc.CategoryId", Name: "BigCommerce-CategoryId", IsChecked: selectedFields.indexOf("pbc.CategoryId") > -1, Sort: "" }),
    ];
}

function getProductBuycomFieldArr(selectedFields) {
    return [];
}

function asinChanged() {
    viewModel.isAsinChanged(true);
}

function getFileFormats() {
    return [
        new ItemModel({ Id: ",", Name: "Comma Delimited" }),
        new ItemModel({ Id: "\t", Name: "Tab Delimited" }),
        new ItemModel({ Id: "|", Name: "Bar Delimited" }),
    ]
}

function setFileUploadDialogTexts(fileType) {
    var height = 240;
    $("#divDownloadTemplate").show(); // by default, only hide from product

    if (fileType == "Shadow") {
        $("#jobTypeUpload").val(4);
        $("#uploadModalTitle").text("Upload Shadow File");
        $("#uploadFileTemplateName").val("Shadow_Upload_Template.csv");
        $("#uploadNote").html("Upload Shadow file to update or load the shadow products.<br/>");
    } else if (fileType == "Kit") {
        $("#jobTypeUpload").val(3);
        $("#uploadModalTitle").text("Upload Kit File");
        $("#uploadFileTemplateName").val("Kit_Upload_Template.csv")
        $("#uploadNote").html("Upload Kit file to update Kit components. <br/>");
    } else if (fileType == "BlackListSKU") {
        $("#jobTypeUpload").val(5);
        $("#uploadModalTitle").text("Upload Blacklisted SKUs File");
        $("#uploadFileTemplateName").val("BlacklistedSKU_Template.csv");
        $("#uploadNote").html("Upload list of SKUs to be blacklisted. <br/>");
    } else if (fileType == "eBayCategories") {
        $("#jobTypeUpload").val(6);
        $("#uploadModalTitle").text("Get eBay Suggested Categories");
    } else { // = "Product"
        $("#jobTypeUpload").val(0);
        $("#uploadModalTitle").text("Upload Product File");
        $("#uploadFileTemplateName").val("EIS_ProductFile_Template.csv");
        $("#uploadNote").html("Upload product file to update EIS product and/or Amazon details. <br />" +
            "Please make it sure that either the <strong>EisSKU</strong> field or the <strong>CompanyId</strong> field is present in the upload file.");
       
        // hide the downloan template button div
        $("#divDownloadTemplate").hide();
    }

    // resize the dialog
    $("#fileUploadDialogForm").css({ "height": height + "px" });
}

function downloadUploadFileTemplate() {
    document.getElementById("download_frame").src = GET_FILE_TEMPLATE_URL + "?fileTemplateName=" + $("#uploadFileTemplateName").val();
}

function AddBGCustomField() {
    $(".customFieldContainer").append($(".customfieldtemplate").html());
}

function RemoveBGCustomField(object) {
    $(object).parent().addClass("hidden");

    $(object).parent().find(".customfieldname").attr("productId","-1");
}

function SubmitUpdateBigCommerceProduct(productID, eisSKU) {
        
    // Post Custom Fields for saving
    SaveCustomFields(productID, eisSKU);

    // Submit the model for saving
    $("#edit-form").submit();
}

function SaveCustomFields(productID, eisSKU) {
    var customFieldList = [];
    var eisSKU = eisSKU;
    var prodId = productID;
    
    $(".customFieldContainer .customfields").each(function (index) {
        var name = $(this).find(".customfieldname").val();
        var text = $(this).find(".customfieldtext").val();
        var id = $(this).find(".customfieldname").attr("data-id");
        var isDelete = $(this).find(".customfieldname").attr("productId");

        if (typeof isDelete != 'undefined' && isDelete == -1) {
            prodId = -1;
        } else {
            prodId = productID;
        }

        if (typeof id == 'undefined')
        {
            id = -1;
        }

        customFieldList.push({ Id: id, Name: name, Text: text, ProductId: prodId });
    });
        
    // Post
    $.post(GET_BIGCOMMERCE_UPDATECUSTOMFIELDS_URL, { id: eisSKU, model: customFieldList }, function (result) {
        console.log(result);
    });

}