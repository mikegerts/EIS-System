function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.customExport = ko.observable();
    self.companies = ko.observableArray();
    self.vendors = ko.observableArray();
    self.selectedVendor = ko.observable();
    self.selectedFileVendor = ko.observable();
    self.selectedCompany = ko.observable();
    self.images = ko.observableArray();
    self.image = ko.observable();

    self.loadData = function () {
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

    self.loadCustomExportProduct = function (rowsSelected) {
        
        console.log(rowsSelected);
        if (rowsSelected == 0) {
            self.type("error");
            self.message("No records selected! Please select first the product records you want to export");
        } else {
            // display the message for items selected
            self.type("warning");
            self.message("You have selected " + rowsSelected + " records to export.");
            $(".alertMsgStatus").removeClass("alert-error");
        }

        // set the excluded and selected vendor product EisSupplierSKUs if there's any
        var vendorId = $("#vendorId").val();
        var companyId = $("#companyId").val();
        var withEisLink = $("#withEisSKULink").val();
        var searchString = $("#SearchString").val();
        var qtyFrom = $("#inventoryQtyFrom").val();
        var qtyTo = $("#inventoryQtyTo").val();

        self.customExport(new CustomExportModel({
            Delimiter: ",",
            SortBy: "",
            SearchString: searchString,
            ProductFields: [],
            SelectedEisSKUs: selectedProductEisSKUs,
            ExcludedEisSKUs: unselectedProductEisSKUs,
            IsAllProductItems: isSelectAllPages,
            VendorId: vendorId,
            CompanyId: companyId,
            WithEisSKULink: withEisLink,
            QuantityFrom: qtyFrom,
            QuantityTo: qtyTo,
            RequestedDate: new Date(),
        }));
    }

    self.applyFilters = function () {
        showSpinner();
        $("#filterForm").submit();
        setTimeout("hideSpinner(); $('#FilterDialog').modal('hide');", 1400);

        // set the values in the show entries form
        $("#showEntryForm #vendorId").val(viewModel.selectedVendor());
        $("#showEntryForm #companyId").val(viewModel.selectedCompany());
        $("#showEntryForm #withEisSKULink").val($("#withEisSKULink").val());
        $("#showEntryForm #inventoryQtyFrom").val($("#filterForm #inventoryQtyFrom").val());
        $("#showEntryForm #inventoryQtyTo").val($("#filterForm #inventoryQtyTo").val());
    }

    self.resetFilters = function () {
        self.selectedVendor(null);
        self.selectedCompany(null);
        $("#filterForm #withEisSKULink").val(-1);
        $("#filterForm #SearchString").val("");
        $("#filterForm #inventoryQtyFrom").val("");
        $("#filterForm #inventoryQtyTo").val("");
        $("#filterForm #deletefilter").hide();
        $("#filterForm #filterName").val("");
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
}

function ProductLinkViewModel() {
    var self = this;

    self.eisProductLinks = ko.observableArray();
    self.eisProductResults = ko.observableArray();
    self.selectedEisProducts = ko.observableArray();
    self.eisSupplierSKU = ko.observable();
    self.keyword = ko.observable();

    self.loadData = function (eisSupplierSKU) {
        self.eisSupplierSKU(eisSupplierSKU);
        $.ajax({
            url: GET_EIS_PRODUCT_LINKS_URL + "?eisSupplierSKU=" + eisSupplierSKU,
            success: function (results) {
                self.eisProductLinks(results);
            }
        });
    }

    self.searchEisProducts = function (data, event) {
        if (self.keyword() == null)
            return false;

        $.ajax({
            url: SEARCH_EIS_PRODUCT_URL + "?keyword=" + self.keyword(),
            beforeSend: function () {
                $(event.target).text("Searching...")
            },
            success: function (results) {
                self.eisProductResults.removeAll();
                if (results.length > 0) {
                    self.eisProductResults(results);
                } else {
                    self.eisProductResults.push({ Id: "", DisplayName: "No results found" })
                }
            },
            complete: function () {
                $(event.target).text("Search Products")
            }
        });
    }

    self.addEisProductLinks = function (model, event) {
        // show the spiner and disable the button
        showSpinner();
        $(event.target).addClass("disabled");

        $.ajax({
            type: "POST",
            url: ADD_EIS_PRODUCT_LINKS_URL,
            data: JSON.stringify({ eisSupplierSKU: self.eisSupplierSKU(), selectedEisSKUs: self.selectedEisProducts() }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $(event.target).removeClass("disabled");
                    return;
                }

                // reload the product links and close the dialog
                self.loadData(self.eisSupplierSKU());
                setTimeout("$('#ProductLinkDialog').modal('hide');", 1000);
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.deleteEisProductLink = function (item) {
        $.confirm({
            title: "Delete EIS Product Link",
            text: "Are you sure you want to delete this product link from this vendor product?<br/><strong> " + item.EisSKU + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_PRODUCT_LINK_URL, { eisSKU: item.EisSKU, eisSupplierSKU: item.EisSupplierSKU }, function (result) {
                    if (result.IsSuccess) {
                        self.eisProductLinks.remove(item);
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
}

function ActionsViewModel() {
    var self = this;

    self.submitAction = function (object, event) {
        var selectedActionUrl = $("#vendorProductAction").val();
        if (selectedActionUrl == "")
            return false;

        if ($(event.target).hasClass("disabled"))
            return false;

        $(event.target).addClass("disabled");
        $(event.target).addClass("fa-spin");

        $.ajax({
            type: "POST",
            url: selectedActionUrl,
            contentType: "application/json",
            success: function (result) {
                if (result.Success) {
                    displayMessage(result.Success, "success");
                } else {
                    displayMessage(result.Error, "error");
                }

                // reload the vendor product links
                productLinkViewModel.loadData(eisSupplierSKU);
            },
            complete: function () {
                $(event.target).removeClass("disabled");
                $(event.target).removeClass("fa-spin");
                setTimeout(function () { fadeOutMessage() }, 6000);
            }
        });
    }
}

function PendingOrderViewModel() {
    var self = this;

    self.pendingOrders = ko.observableArray();
    self.isLoading = ko.observable(true);
    
    self.loadData = function (eisSupplierSKU) {
        $.ajax({
            url: GET_PENDING_ORDERS_URL + "?eisSupplierSKU=" + eisSupplierSKU,
            success: function (results) {
                self.pendingOrders(results);
                self.isLoading(false);
            }
        });
    }
}

function deleteVendorProduct(source, id) {
    $.confirm({
        title: "Delete Vendor Product",
        text: "Are you sure you want to delete vendor product: <br/><strong> " + id + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(DELETE_VENDOR_PRODUCT_URL, { id: id }, function (result) {
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

    return false;
}


function CustomExportModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);

    self.downloadCustomExport = function (model, event) {
        if (!isValidFormData("custom-export-form")) {
            return;
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

}


function CustomExportModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);
    self.availableProductFields = ko.observableArray(getVendorProductFieldsArr());
    self.selectedProductFields = ko.observableArray();
    self.selectedFieldsToAdd = ko.observableArray();
    self.selectedFieldsToRemove = ko.observableArray();
    self.fileFormats = ko.observableArray(getFileFormats());

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
}

function validateEisSKU(source) {
    if (!source.value)
        return false;

    $.getJSON(VALIDATE_EISSKU_URL + "?eisSKU=" + source.value, function (result) {
        if (result.IsExist) {
            $(source).parents(".form-group").removeClass("has-error").addClass("has-success")
            $(source).parent().find(".form-control-feedback").removeClass("glyphicon-remove").addClass("glyphicon-ok");
            $(source).val(result.EisSKU);
        } else {
            $(source).parents(".form-group").removeClass("has-success").addClass("has-error");
            $(source).parent().find(".form-control-feedback").removeClass("glyphicon-ok").addClass("glyphicon-remove");
        }
    });
}

function deleteBulkVendorProducts() {
    var postData = {
        vendorId: $("#vendorId").val(),
        companyId: $("#companyId").val(),
        withEisSKULink: $("#withEisSKULink").val(),
        searchString: $("#SearchString").val(),
        quantityFrom: $("#inventoryQtyFrom").val(),
        quantityTo: $("#inventoryQtyTo").val(),
        selectedModelIds: selectedProductEisSKUs,
        excludedModelIds: unselectedProductEisSKUs,
        isAllSelectedItems: isSelectAllPages,
    };

    $.confirm({
        title: "Bulk Delete Vendor Products",
        text: "Are you sure you want to delete <strong>" + recordsSelected + "</strong> vendor product items?",
        cancel: function () {
            return false;
        },
        confirm: function () {
            displayMessage(("EIS is on currently deleting " + recordsSelected + " vendor products. This will take a while"), "warning");
            $.post(DELETE_BULK_VENDOR_PRODUCTS_URL, { model: postData }, function (result) {
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


function setFileUploadDialogTexts(fileType) {
    var height = 330;
    if (fileType == "Product") {
        $("#jobTypeUpload").val(7);
        $("#uploadModalTitle").text("Upload Vendor Product File");
        $("#uploadFileTemplateName").val("EIS_VendorProductFile_Template.csv");
        $("#uploadNote").html("Upload vendor product file into the system. <br />" +
            "Please make it sure that either the <strong>EisSupplierSKU</strong> field or the <strong>SupplierSKU and VendorId</strong> fields are present in the upload file.");
        $("#divProduct").show();
        $("#divInventory").hide();
    }
    else if (fileType == "Inventory")
    {
        $("#jobTypeUpload").val(10);
        $("#uploadModalTitle").text("Upload Vendor Inventory File");
        $("#uploadFileTemplateName").val("EIS_VendorInventoryFile_Template.csv");
        $("#uploadNote").html("Upload vendor inventory file into the system. <br />" +
            "Please make it sure that either the <strong>EisSupplierSKU</strong> field or the <strong>SupplierSKU and VendorId</strong> fields are present in the upload file.");

        $("#divInventory").show();
        $("#divProduct").hide();
        ViewModel.selectedFileVendor = null;
    }
    // resize the dialog
    $("#fileUploadDialogForm").css({ "height": height + "px" });
}

function downloadUploadFileTemplate() {
    document.getElementById("download_frame").src = GET_FILE_TEMPLATE_URL + "?fileTemplateName=" + $("#uploadFileTemplateName").val();
}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.IsChecked = ko.observable(item.IsChecked || false);

    self.selectAll = function (item, event) {
        $(event.target).select();
    }
}

function getVendorProductFieldsArr() {
    return [
        new ItemModel({ Id: "vendorproducts.EisSupplierSKU", Name: "EisSupplierSKU", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.SupplierSKU", Name: "SupplierSKU", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.VendorId", Name: "VendorId", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.Name", Name: "Name", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.Description", Name: "Description", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.ShortDescription", Name: "ShortDescription", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.SupplierPrice", Name: "SupplierPrice", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.Quantity", Name: "Quantity", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.MinPack", Name: "MinPack", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.UPC", Name: "UPC", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.Category", Name: "Category", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.Weight", Name: "Weight", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.WeightUnit", Name: "WeightUnit", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.Shipping", Name: "Shipping", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.VendorMOQ", Name: "VendorMOQ", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.VendorMOQType", Name: "VendorMOQType", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "vendorproducts.IsAutoLinkToEisSKU", Name: "IsAutoLinkToEisSKU", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "1", Name: "ImageUrls", IsChecked: false, Sort: "" }),
        new ItemModel({ Id: "2", Name: "EisSKULinks", IsChecked: false, Sort: "" })
    ];
}

function getFileFormats() {
    return [
        new ItemModel({ Id: ",", Name: "Comma Delimited" }),
        new ItemModel({ Id: "\t", Name: "Tab Delimited" }),
        new ItemModel({ Id: "|", Name: "Bar Delimited" }),
    ]
}