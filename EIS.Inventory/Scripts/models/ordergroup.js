function ViewModel() {
    var self = this;

    self.type = ko.observable("");
    self.message = ko.observable();
    self.orderGroup = ko.observable();
    self.orderGroups = ko.observableArray();
    self.selectedOrderGroup = ko.observable();

    self.loadModel = function (modelId) {
        $("#loadingModal").show();
        if (modelId == -1) {
            self.orderGroup(new OrderGroupModel({
                Id: modelId,
                Name: "",
                Description: "",
                Orders: [],
                isCreate: true
            }));
            $("#loadingModal").hide();
        } else {
            // load the product group
            $.ajax({
                url: GET_ORDER_GROUP_URL,
                data: { id: modelId },
                success: function (result) {
                    self.orderGroup(new OrderGroupModel(result));
                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });
        }
    }

    self.loadData = function () {
        $.ajax({
            url: GET_ORDER_GROUPS_URL,
            success: function (results) {
                self.orderGroups(ko.utils.arrayMap(results, function (item) {
                    return new ItemModel(item);
                }));
            }
        });
    }


}

function OrderGroupModel(model) {
    var self = this;

    var mapping = {
        "Orders": {
            create: function (options) {
                return new ItemModel(options.data, [], []);
            }
        }
    };
    ko.mapping.fromJS(model, mapping, self);
    self.isCreate = ko.observable(model.isCreate);
    self.PagedItem = ko.observable();

    self.addOrderItem = function () {
        self.Orders.push(new ItemModel({ OrderId: "", BuyerName: "", MarketPlace: "" }));
    }

    self.deleteOrderItem = function (item, event) {
        self.Orders.remove(item);
    }

    self.orderIdValueChanged = function (orderItem, event) {
        var element = $(event.target);
        $.ajax({
            url: GET_ORDER_ITEM_URL,
            data: { orderId: orderItem.OrderId() },
            success: function (result) {
                $(element).attr("title", "")
                if (result.Error) {
                    $(element).parent().addClass("has-error");
                    $(element).attr("title", "This order was not found!")
                    orderItem.BuyerName("")
                    orderItem.MarketPlace("");
                    orderItem.HasError(true);
                    return;
                }

                $(element).parent().removeClass("has-error");
                $(element).parent().removeClass("has-warning");

                orderItem.BuyerName(result.BuyerName)
                orderItem.MarketPlace(result.MarketPlace);
                orderItem.HasError(false);
            }
        });
    }

    self.saveOrderGroup = function (item, event) {
        if (!isValidateForm()) {
            return;
        }
        var hasError = $.grep(self.Orders(), function (item) {
            return item.HasError() == true;
        });
        console.log(hasError);
        if (hasError.length != 0) {
            viewModel.type("warning");
            viewModel.message("Unable to save the order group. Please remove the not found EIS Order Id's.");
            return false;
        }

        showSpinner();
        $(event.target).addClass("disabled");

        // mapped the ko object to plain JS object
        var modelData = ko.mapping.toJS(item);

        $.ajax({
            type: "POST",
            url: SAVE_ORDER_GROUP_URL,
            data: JSON.stringify(modelData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message("Error occured in trying to save the Order Group! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Order Group have been successfully saved!");
                setTimeout("$('#OrderGroupDialog').modal('hide');", 2000);
                location.reload();
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Order Group! <br/> " + result)
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.nextPage = function (pagedModel, event) {
        var next = pagedModel.CurrentPageIndex() + 1;
        if (next > pagedModel.TotalPageCount())
            return false;

        $("#loadingModal").show();
        $.ajax({
            url: GET_ORDER_ITEMS_URL,
            data: { groupId: pagedModel.ModelId(), page: next },
            success: function (result) {
                self.PagedItem(new PagedItemModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.prevPage = function (pagedModel, event) {
        var prev = pagedModel.CurrentPageIndex() - 1;
        if (prev <= 0)
            return false;

        $("#loadingModal").show();
        $.ajax({
            url: GET_ORDER_ITEMS_URL,
            data: { groupId: pagedModel.ModelId(), page: prev },
            success: function (result) {
                self.PagedItem(new PagedItemModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }
}


function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.HasError = ko.observable(false);
}

function PagedItemModel(pagedItem) {
    var self = this;

    var mapping = {
        "Items": {
            create: function (options) {
                return new ItemModel(options.data);
            }
        }
    };
    ko.mapping.fromJS(pagedItem, mapping, self);

    self.pageShowStatus = ko.pureComputed(function () {
        if (self.TotalPageCount() == 0)
            return "No Results";
        return "Page: " + self.CurrentPageIndex() + " of " + self.TotalPageCount();
    })

    self.hasPrevPage = ko.pureComputed(function () {
        return self.CurrentPageIndex() != 1;
    });

    self.hasNextPage = ko.pureComputed(function () {
        return self.CurrentPageIndex() < self.TotalPageCount();
    });
}

function deleteOrderGroup(source, id, name) {
    $.confirm({
        title: "Delete Order Group",
        text: "Are you sure you want to delete order group: <strong> " + name + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(DELETE_ORDER_GROUP_URL, { id: id }, function (result) {
                if (result.Success) {
                    $(source).parent().parent().fadeOut();

                    // reload the page if there's no table records in the paged
                    if (($("#tblOrderGroups > tbody > tr:visible").length - 1) == 0)
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
