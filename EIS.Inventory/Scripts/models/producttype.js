
function ViewModel() {
    var self = this;

    self.amazonMainCategories = ko.observableArray();
    self.amazonSubCategories = ko.observableArray();
    self.ebayMainCategories = ko.observableArray();
    self.ebaySubCategories = ko.observableArray();

    self.productTypes = ko.observableArray();
    self.productType = ko.observable();
    self.modelId = ko.observable();
    self.templateName = ko.observable();

    self.isEdit = ko.computed(function () {
        return self.modelId() != -1;
    })

    self.isDetails = ko.computed(function () {
        return self.templateName() == "detailsProductType";
    })

    self.modalTitle = ko.computed(function () {
        return self.templateName() == "detailsProductType" ? "Product Type Details"
            : (self.isEdit() ? "Edit Product Type" : "Create Product Type");
    })

    self.amazonMainCategoryChanged = function (productType, event) {

        if (!productType.AmazonMainCategoryCode())
            return false;

        $.ajax({
            url: GET_AMAZON_SUB_CATEGORIES_URL + productType.AmazonMainCategoryCode(),
            success: function (results) {
                self.amazonSubCategories(results);
            }
        });

        valueChanged(productType, event);
    }

    self.editProductType = function (productType, event) {
        self.templateName("entryProductType");

        // trigger the amazon main category changed
        self.amazonMainCategoryChanged(new ProductTypeModel(productType), event);
    }

    self.deleteProductType = function (productType, event) {
        $.confirm({
            title: "Delete Product Type",
            text: "Are you sure you want to delete this product type: <strong> " + productType.TypeName + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_PRODUCT_TYPE_URL, { Id: productType.Id }, function (result) {
                    if (result.Success) {
                        self.productTypes.remove(productType);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.saveProductType = function (productType, event) {
        if (!isValidateForm())
            return;

        showSpinner();
        $(event.target).addClass("disabled");

        // update the helper properties
        productType.AmazonMainCategoryName($("#AmazonMainCategory option:selected").text());
        productType.AmazonSubCategoryName($("#AmazonSubCategory option:selected").text());

        // mapped the ko object to plain JS object
        var productTypeData = ko.mapping.toJS(productType);
        productTypeData.modelId = self.modelId();

        $.ajax({
            type: "POST",
            url: SAVE_PRODUCT_TYPE_URL,
            data: JSON.stringify(productTypeData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $("#error-status").show();
                    $(event.target).removeClass("disabled");
                    return;
                }

                $("#error-status").hide();
                $("#success-status").show();
                setTimeout("$('#ProductTypeDialog').modal('hide');", 2000);

                if (self.modelId() == -1) {
                    productType.Id(result.Id);
                    self.productTypes.push(new ProductTypeModel(productType));
                    return true;
                }

                // find the old product type in the list
                var oldProductType = ko.utils.arrayFirst(self.productTypes(), function (item) {
                    return item.Id == productType.Id();
                })
                
                // replace the old value with the updated one
                self.productTypes.replace(oldProductType, productTypeData);
            },
            error: function (result) {
                $("#error-status").show();
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.loadData = function () {
        // load the main list
        $.ajax({
            url: GET_PRODUCT_TYPES_URL,
            success: function (results) {
                self.productTypes(results);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error in Product Types: " + jqXHR.responseText);
            }
        });

        // retrieve the list of amazon categories
        $.ajax({
            url: GET_AMAZON_MAIN_CATEGORIES_URL,
            success: function (results) {
                self.amazonMainCategories(results);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error in Amazon Categories: " + jqXHR.responseText);
            }
        });
    }

    self.loadModel = function () {
        if (self.modelId() == -1) {
            self.productType(createProductTypeModel());
            hideLoadingGif();
        }
        else {
            // load the file setting
            $.ajax({
                url: GET_PRODUCT_TYPE_URL + self.modelId(),
                success: function (result) {
                    self.productType(new ProductTypeModel(result));
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert("Error: " + jqXHR.responseText);
                },
                complete: function () {
                    hideLoadingGif();
                }
            });
        }
    }
}

function ProductTypeModel(productType) {
    var self = this;

    ko.mapping.fromJS(productType, {}, self);
}

function createProductTypeModel() {
    return new ProductTypeModel({
        Id: -1,
        TypeName: "",
        AmazonMainCategoryCode: "",
        AmazonSubCategoryCode: "",
        EbayMainCategoryCode: "",
        EbaySubCategoryCode: "",
        AmazonMainCategoryName: "",
        AmazonSubCategoryName: ""
    });
}