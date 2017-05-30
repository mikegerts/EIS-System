
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();
    self.productGroupDetail = ko.observable();
    self.productGroups = ko.observableArray();
    self.selectedGroup = ko.observable();

    self.loadModel = function (modelId) {
        $("#loadingModal").show();
        if (modelId == -1) {
            self.productGroupDetail(new ProductGroupDetailModel({
                Id: modelId,
                Name: "",
                Description: "",
                FirstItemOnPage: 1,
                HasNextPage: false,
                HasPreviousPage: false,
                IsFirstPage: false,
                IsLastPage: false,
                LastItemOnPage: 0,
                PageCount: 0,
                PageNumber: 1,
                PageSize: 10,
                TotalItemCount: 0,
                Items: [{ EisSKU: "", Name: "", Description: "", Quantity: "" }]
            }));
            $("#loadingModal").hide();
        } else {
            // load the product group
            $.ajax({
                url: "/productgroup/_GetProductGroupDetails",
                data: {id: modelId },
                success: function (result) {
                    self.productGroupDetail(new ProductGroupDetailModel(result));
                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });
        }
    }

    self.loadData = function () {
        // load all the vendors for filtering product
        $.ajax({
            url: GET_PRODUCT_GROUPS_URL,
            success: function (results) {
                self.productGroups(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });
    }
}

function ProductGroupDetailModel(model) {
    var self = this;

    self.Id = ko.observable(model.Id);
    self.Name = ko.observable(model.Name);
    self.Description = ko.observable(model.Description);
    self.isCreate = ko.observable(model.Id < 1);
    self.pagedModel = ko.observable(new PagedDataModel(model));

    self.prevPagedData = function (model) {
        if (!model.HasPreviousPage())
            return;

        $.get("/productgroup/_GetProductGroupDetails", { id: model.Id, page: (model.PageNumber() - 1) }, function (result) {
            self.pagedModel(new PagedDataModel(result));
        });
    }

    self.nextPagedData = function (model) {
        if (!model.HasNextPage())
            return;

        $.get("/productgroup/_GetProductGroupDetails", { id: model.Id, page: (model.PageNumber() + 1) }, function (result) {
            self.pagedModel(new PagedDataModel(result));
        });
    }
    
    self.saveProductGroup = function (item, event) {
        console.log(item);
        // check if the required fields are filled-up
        if (!validateForm("#productgroupdialog-edit-form")) {
            return;
        }

        // check if it has unknown EisSKUs
        var hasError = $.grep(self.pagedModel().Items(), function (item) {
            return item.HasError() == true;
        });

        if (hasError.length != 0) {
            viewModel.type("error");
            viewModel.message("Unable to save the product group. Please remove the EisSKU(s) which are not found!");
            return false;
        }

        showSpinner();
        $(event.target).addClass("disabled");

        // iterate and get the list of deleted items
        var addedItems = [];
        $.each(self.pagedModel().AddedItems(), function (index, item) {
            addedItems.push(item.EisSKU());
        })

        // iterate and get the list of deleted items
        var deletedItems = [];
        $.each(self.pagedModel().DeletedItems(), function (index, item) {
            deletedItems.push(item.EisSKU());
        })
        
        $.ajax({
            type: "POST",
            url: SAVE_PRODUCT_GROUP_URL,
            data: JSON.stringify({ Id: self.Id(), Name: self.Name(), Description: self.Description(), AddedItems: addedItems, DeletedItems: deletedItems }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message("Error occured in trying to save the Product Group! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Product Group have been successfully saved!");
                location.reload();
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Product Group! <br/> " + result)
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }
}

function PagedDataModel(data) {
    var self = this;
    var mapping = {
        "Items": {
            create: function (options) {
                return new ItemModel(options.data)
            }
        }
    };

    ko.mapping.fromJS(data, mapping, self);
    self.AddedItems = ko.observableArray();
    self.DeletedItems = ko.observableArray();

    self.pageShowStatus = ko.pureComputed(function () {
        if (self.PageCount() == 0)
            return "No Page";
        return "Page: " + self.PageNumber() + " of " + self.PageCount();
    })

    self.addProductItem = function (data) {
        var itemModel = new ItemModel({ EisSKU: "", Name: "", Description: "", Quantity: "" });
        self.Items.push(itemModel);
        self.AddedItems.push(itemModel);
    }

    self.deleteProductItem = function (item, event) {
        self.Items.remove(item);
        self.AddedItems.remove(item);
        self.DeletedItems.push(item);
    }

    self.skuValueChanged = function (productItem, event) {
        var element = $(event.target);
        $.ajax({
            url: GET_PRODUCT_ITEM_URL,
            data: { eisSku: productItem.EisSKU() },
            success: function (result) {
                $(element).attr("title", "")
                if (result.Error) {
                    $(element).parent().addClass("has-error");
                    $(element).attr("title", "This product was not found!")
                    productItem.Name("")
                    productItem.Description("");
                    productItem.HasError(true);
                    return;
                }

                $(element).parent().removeClass("has-error");
                $(element).parent().removeClass("has-warning");

                if (result.QtyAvailable == 0) {
                    $(element).parent().addClass("has-warning");
                    $(element).attr("title", "There is no stock left for this product!")
                }

                productItem.Name(result.Name)
                productItem.Description(result.Description);
                productItem.HasError(false);
            }
        });
    }
}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.HasError = ko.observable(false); 
}

function deleteProductGroup(source, id, name) {
    $.confirm({
        title: "Delete Product Group",
        text: "Are you sure you want to delete product group: <strong> " + name + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(DELETE_PRODUCT_GROUP_URL, { id: id }, function (result) {
                if (result.Success) {
                    $(source).parent().parent().fadeOut();

                    // reload the page if there's no table records in the paged
                    if (($("#tblProductGroups > tbody > tr:visible").length - 1) == 0)
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