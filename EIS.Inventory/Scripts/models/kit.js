
function KitViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.inventoryDependencies = ko.observableArray([
        { Id: 0, Name: "All Components" },
        { Id: 1, Name: "Main Component" },
        { Id: 2, Name: "Independent" }]);
    self.selectProduct = ko.observable(new SelectProductModel());
    self.kit = ko.observable(new KitModel({
        IsKit : false,
        ParentKitSKU: "",
        InventoryDependencyOn: "",
        TotalSupplierPrice: 0,
        TotalSellerPrice: 0,
        KitInventoryQty: 0,
        ComponentCount: 0,
        KitDetails: []
    }))
    self.kitDetail = ko.observable();

    self.loadModel = function (parentKitSku, childKitSku) {
        $.get(GET_KIT_DETAIL_URL + "?parentKitSku=" + parentKitSku + "&childKitSku=" + childKitSku, function (result) {
            self.kitDetail(new KitDetail(result));
        });
    }

    self.loadData = function (data) {
        self.kit(new KitModel(data));
        self.selectProduct().ParentKitSKU(data.ParentKitSKU);
    }

    self.loadKitDetails = function (parentKitSku) {
        $.get(GET_KIT_DETAILS_BY_PARENTKITSKU_URL + "?parentKitSku=" + parentKitSku, function (results) {
            self.kit().KitDetails(ko.utils.arrayMap(results, function (item) {
                return new KitDetail(item);
            }));
        });
    }

    self.deleteKitDetail = function (kitDetail) {
        $.confirm({
            title: "Delete Kit Component",
            text: "Are you sure you want to delete this Kit component: <strong> " + kitDetail.ChildKitSKU() + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.ajax({
                    type: "POST",
                    url: DELETE_KITDETAIL_URL,
                    data: JSON.stringify({ parentKitSku: kitDetail.ParentKitSKU(), childKitSku: kitDetail.ChildKitSKU() }),
                    contentType: "application/json",
                    success: function (result) {
                        if (result.Error)
                            alert(result.Error);
                        else
                            self.loadKitDetails(kitDetail.ParentKitSKU());
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.saveKit = function (kit) {
        var kitData = ko.mapping.toJS(kit);

        $.ajax({
            type: "POST",
            url: SAVE_KIT_URL,
            data: JSON.stringify({ parentKitSku: kit.ParentKitSKU(), model: kitData }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    alert(result.Error);
                } else {
                    self.kit(new KitModel(result));
                    viewModel.type("success");
                    viewModel.message("Kit changes have been successfully saved!");
                    setTimeout(function () { viewModel.message(null) }, 5000);
                }
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.saveKitDetail = function (kitDetail) {
        showSpinner();

        // parse it JS object
        var kitDetailData = ko.mapping.toJS(kitDetail);
                                
        $.ajax({
            type: "POST",
            url: SAVE_KIT_DETAIL_URL,
            data: JSON.stringify({model: kitDetailData }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    alert(result.Error);
                } else {
                    // set the alert message
                    kitViewModel.type("info");
                    kitViewModel.message("Kit component changes have been successfully saved!");
                    self.loadKitDetails(kitDetail.ParentKitSKU());
                    setTimeout("$('#EditKitComponentDialog').modal('hide');", 2000);
                }
            },
            complete: function () {
                hideSpinner();
            }
        });
    }
}

function KitModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.TotalSellerPrice = ko.observable();
    self.ComponentCount = ko.pureComputed(function () {
        if (self.KitDetails() == null)
            return 0;

        return self.KitDetails().length;
    });
    self.TotalSellerPrice = ko.pureComputed(function () {
        var total = 0;
        $.each(self.KitDetails(), function (i, item) {
            total += item.ProductSellerPrice();
        });
        return total.toFixed(2);
    });
    self.TotalSupplierPrice = ko.pureComputed(function () {
        var total = 0;
        $.each(self.KitDetails(), function (i, item) {
            total += item.ProductSupplierPrice();
        });
        return total.toFixed(2);
    });
    self.KitInventoryQty = ko.pureComputed(function () {
        var minQty = self.KitDetails().length > 0 ? 10000 : 0;
        $.each(self.KitDetails(), function (i, item) {
            minQty = item.ProductQuantity() < minQty ? item.ProductQuantity() : minQty;
        });
        return minQty;
    });
}

function SelectProductModel() {
    var self = this;

    self.ParentKitSKU = ko.observable();
    self.searchStr = ko.observable();
    self.isSelectCurrentPage = ko.observable(false);
    self.isExcludeKits = ko.observable(false);
    self.pagedProducts = ko.observable(new PagedDataModel({
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
        Items: []
    }));

    self.selectedProducts = ko.pureComputed(function () {
        var selectedItems = $.map(self.pagedProducts().Items(), function (item) {
            return item.IsChecked() == true ? item : null;
        });
        return selectedItems;
    });

    self.isSelectCurrentPage.subscribe(function (newValue) {
        $.each(self.pagedProducts().Items(), function (i, item) {
            item.IsChecked(newValue);
        })
    });

    self.searchProducts = function (model) {
        if (model.searchStr().length < 3)
            return;
        $.get(SEARCH_PAGED_PRODUCTS_URL, { searchStr: model.searchStr() }, function (results) {
            self.pagedProducts(new PagedDataModel(results));
        });
    }

    self.addSelectedProducts = function (model) {
        showSpinner();
        var kitDetails = [];
        $.each(self.selectedProducts(), function (i, item) {
            kitDetails.push({ ParentKitSKU: self.ParentKitSKU(), ChildKitSKU: item.EisSKU(), Quantity: item.Quantity() });
        });

        // submit to add the selected items
        $.ajax({
            type: "POST",
            url: ADD_KITDETAILS_URL,
            data: JSON.stringify({ parentKitSKU: self.ParentKitSKU(), models: kitDetails }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    alert(result.Error);
                } else {
                    // set the alert message
                    kitViewModel.type("info");
                    kitViewModel.message("Selected products have been successfully added to Kit!");
                    kitViewModel.loadKitDetails(result.ParentKitSKU);
                    kitViewModel.kit().IsKit(true);
                    setTimeout("$('#SelectProductDialog').modal('hide');", 2000);
                }
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.prevPagedData = function (model) {
        if (!model.HasPreviousPage())
            return;

        $.get(SEARCH_PAGED_PRODUCTS_URL, { searchStr: self.searchStr(), page: (model.PageNumber() -1)  }, function (results) {
            self.pagedProducts(new PagedDataModel(results));
        });
    }
    self.nextPagedData = function (model) {
        if (!model.HasNextPage())
            return;

        $.get(SEARCH_PAGED_PRODUCTS_URL, { searchStr: self.searchStr(), page: (model.PageNumber() + 1) }, function (results) {
            self.pagedProducts(new PagedDataModel(results));
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

    self.pageShowStatus = ko.pureComputed(function () {
        if (self.PageCount() == 0)
            return "No Page";
        return "Page: " + self.PageNumber() + " of " + self.PageCount();
    })
}

function KitDetail(kitDetail) {
    var self = this;

    ko.mapping.fromJS(kitDetail, {}, self);
}

function ItemModel(item) {
    var self = this;

    self.IsChecked = ko.observable();
    ko.mapping.fromJS(item, {}, self);
}