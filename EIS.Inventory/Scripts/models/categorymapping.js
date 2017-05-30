
function ViewModel() {
    var self = this;

    self.productTypes = ko.observableArray();
    self.productMappedCategories = ko.observableArray();
    self.productUnMappedCategories = ko.observableArray();
    self.selectedCategories = ko.observableArray(null);

    self.selectedProductTypeName = ko.observable();
    self.selectedProductTypeId = ko.observable();
    self.isRetrieving = ko.observable();

    self.addSelectedCategories = function (data, event) {
        showSpinner();
        //$(event.target).addClass("disabled");

        // mapped the ko object to plain JS object
        var categoriesData = ko.mapping.toJS(self.selectedCategories());
        self.selectedCategories(null);

        $.ajax({
            type: "POST",
            url: SAVE_SELECTED_CATEGORIES,
            data: JSON.stringify({ id: self.selectedProductTypeId(), categories: categoriesData }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $("#error-status").show();
                    return;
                }

                $("#error-status").hide();
                $("#success-status").show();
                setTimeout("$('#CategoryMappingDialog').modal('hide');", 2000);
                self.selectedCategories(null);
            },
            error: function (result) {
                $("#error-status").show();
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.deleteMappedCategory = function (category, event) {
        $.confirm({
            title: "Delete Mapped Category",
            text: "Are you sure you want to delete this product category: <strong> " + category.Category + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_MAPPED_CATEGORY_URL, { Id: category.ProductTypeId, Category: category.Category }, function (result) {
                    if (result.Success) {
                        self.productMappedCategories.remove(category);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.loadData = function () {        
        // retrieve all EIS product types
        $.ajax({
            url: GET_PRODUCT_TYPES_URL,
            success: function (results) {
                self.productTypes(results);

                $("#productTypesTree").jstree({
                    "themes": {
                        "theme": "default"
                    },
                    "plugins": ["themes", "html_data"]
                });
            }
        });
    }

    self.loadProductMappedCategories = function () {
        self.isRetrieving(true);
        self.productMappedCategories([]);

        $.ajax({
            url: GET_PRODUCT_MAPPED_CATEGORIES_URL + self.selectedProductTypeId(),
            success: function (results) {
                self.productMappedCategories(results);
            },
            complete: function (result) {
                self.isRetrieving(false);
            }
        });
    }

    self.loadUnMappedCategories = function () {
        $.ajax({
            url: GET_PRODUCT_UNMAPPED_CATEGORIES_URL,
            success: function (results) {
                self.productUnMappedCategories(results);
                self.selectedCategories(null);
            },
            complete: function () {
                hideLoadingGif();
            }
        });
    }
}