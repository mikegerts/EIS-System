
function ViewModel() {
    var self = this;
    
    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.purchaseOrderDetail = ko.observable();
    self.purchaseOrder = ko.observable();
    self.vendors = ko.observableArray();
    self.selectedVendor = ko.observable();

    self.dateRange = ko.observable(["2010-01-01", moment()]);
    self.minDate = "2010-01-01";
    self.ranges = {
        'Today': [moment(), moment()],
        'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
        'Last 7 Days': [moment().subtract('days', 6), moment()],
        'Last 30 Days': [moment().subtract('days', 29), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')],
        'All Time': 'all-time'
    }
    
    self.managePurchaseOrder = function (modelId) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_PURCHASE_ORDER_URL,
            data: { id: modelId },
            success: function (result) {
                self.purchaseOrderDetail(new PurchaseOrderModel(result));
                
                // get the paged order items
                $.ajax({
                    url: GET_PURCHASE_ORDER_ITEMS_URL,
                    data: { poId: modelId, page: 1 },
                    success: function (result) {
                        self.purchaseOrderDetail().PagedItem(new PagedItemModel(result, [], []));
                    }
                });
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });       
    }

    self.savePurchaseOrder = function(po, event){
        if (!isValidateForm()) {
            return;
        }

        // iterate to purchase order items and compute its sharing for item tax and shipping price
        var totalItems = self.purchaseOrder().Items().length;
        var hasZeroQty = false;
        $.each(self.purchaseOrder().Items(), function (index, item) {
            if (item.Qty() == 0) {
                viewModel.type("danger");
                viewModel.message("Purchase order item quantity can not be zero!");
                hasZeroQty = true;
                return false;
            }

            item.PurchaseOrderId(self.purchaseOrder().Id());
            item.EisOrderId(self.purchaseOrder().Id());
            item.ShippingPrices(self.purchaseOrder().ShippingPaid() / totalItems);
            item.Taxes(self.purchaseOrder().TaxPaid() / totalItems);
        });

        if (hasZeroQty)
            return;

        showSpinner();
        $(event.target).addClass("disabled");

        var url = UPDATE_PURCHASE_ORDER_URL;
        if (self.purchaseOrder().isCreate())
            url = SAVE_PURCHASE_ORDER_URL;

        // mapped the ko object to plain JS object
        var poData = { poId: self.purchaseOrder().Id(), model: ko.mapping.toJS(self.purchaseOrder()) };

        $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(poData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message("Error occured in trying to save the Purchase Order! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Purchase Order has been successfully saved!");
                setTimeout("$('#PurchaseOrdeDialog').modal('hide');", 3000);
                location.reload();
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Purchase Order! <br/> " + result)
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.loadData = function (startDate, endDate) {
        if (startDate) {
            self.dateRange([moment(startDate, "MM-DD-YYYY"), moment(endDate, "MM-DD-YYYY")]);
        }

        // load all the vendors
        $.ajax({
            url: GET_VENDORS_URL,
            success: function (results) {
                self.vendors(ko.utils.arrayMap(results, function (item) {
                    return new VendorModel(item);
                }));
            }
        });
    }

    self.loadModel = function (modelId) {
        $("#loadingModal").show();
        if (modelId == -1) {
            // get the generated purchase order id from the server
            $.ajax({
                url: GENERATE_PO_ID_URL,
                success: function (result) {
                    self.purchaseOrder(createPurchaseOrder(result));
                    $("#loadingModal").hide();
                }
            });
            return false;
        }
        else {
            $.ajax({
                url: GET_PURCHASE_ORDER_URL,
                data: { id: modelId },
                success: function (result) {
                    self.purchaseOrder(new PurchaseOrderModel(result));
                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });
        }
    }

    self.prevPage = function (pagedModel, event) {
        var previous = pagedModel.CurrentPageIndex() - 1;
        if (previous <= 0)
            return false;

        $("#loadingModal").show();
        $.ajax({
            url: GET_PURCHASE_ORDER_ITEMS_URL,
            data: { poId: pagedModel.ModelId(), page: previous },
            success: function (result) {
                self.PagedItem(new PagedItemModel(result, self.markedOrderItems(), self.un));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.resetFilters = function () {
        self.selectedVendor(null);
        $("#filterForm #OrderStatus").val("");
        $("#filterForm #FromDate").val("");
        $("#filterForm #ToDate").val("");
    }
}

function PurchaseOrderModel(purchaseOrder) {
    var self = this;

    var mapping = {
        "Items": {
            create: function (options) {
                return new ItemModel(options.data, [], []);
            }
        }
    };
    ko.mapping.fromJS(purchaseOrder, mapping, self);
    self.PagedItem = ko.observable();
    self.isCreate = ko.observable(purchaseOrder.isCreate);
    self.markedOrderItems = ko.observableArray();
    self.unmarkedOrderItems = ko.observableArray();

    // for Manual Create Purchase Order
    var tax = 0, shipping = 0;
    if (purchaseOrder.Items) {
        $.each(purchaseOrder.Items, function (index, item) {
            tax += item.Taxes;
            shipping += item.ShippingPrices;
        })
    }
    self.ShippingPaid = ko.observable(shipping);
    self.TaxPaid = ko.observable(tax);

    self.MarkedOrderItemIds = ko.pureComputed(function () {
        return $.map(self.PagedItem().Items(), function (item) {
            return item.IsPaid() == true ? item.Id() : null;
        });
    })
    self.UnMarkedOrderItemIds = ko.pureComputed(function () {
        return $.map(self.PagedItem().Items(), function (item) {
            return item.IsPaid() == false ? item.Id() : null;
        });
    })
    
    self.itemIsPaidClicked = function (item, event) {
        if (item.IsPaid()) {
            self.markedOrderItems.push(item.Id());
            self.unmarkedOrderItems.remove(item.Id());
        } else {
            self.markedOrderItems.remove(item.Id());
            self.unmarkedOrderItems.push(item.Id());
        }
    }

    self.selectAllClicked = function (item, event) {
        // iterate to the paged items and marked the IsPaid to TRUE
        $.each(self.PagedItem().Items(), function (index, item) {
            item.IsPaid(true);
        })
    }

    self.resetClicked = function (item, event) {
        // iterate to the paged items and marked the IsPaid to False
        $.each(self.PagedItem().Items(), function (index, item) {
            item.IsPaid(false);
        })
    }

    self.vendorChanged = function (order, event) {
        if (!order.VendorId())
            return false;

        // get the vendor details
        $.ajax({
            url: GET_VENDOR_URL,
            data: { id: order.VendorId() },
            success: function (result) {
                self.ContactPerson(result.ContactPerson);
                self.VendorAddress(result.Address);
                self.PhoneNumber(result.PhoneNumber);
            }
        });
    }

    // compute for total amount paid of Purchase Order
    self.poTotalPaid = ko.pureComputed(function () {
        var total = parseFloat(self.TaxPaid()) + parseFloat(self.ShippingPaid());
        
        // iterate to its order items
        if (self.Items() != null) {
            $.each(self.Items(), function (index, item) {
                total += (item.UnitPrice() * item.Qty());
            });
        }
        return total;
    });

    // compute the purchase order total tax
    self.poTax = ko.pureComputed(function () {
        var taxTotal = 0;

        if (self.Items() != null) {
            $.each(self.Items(), function (index, item) {
                taxTotal += item.Taxes;
            });
        }

        return taxTotal;
    })

    // compute for the shipping price
    self.poShippigPrice = ko.pureComputed(function () {
        var priceTotal = 0;

        if (self.Items() != null) {
            $.each(self.Items(), function (index, item) {
                priceTotal += item.ShippingPrices;
            });
        }

        return priceTotal;
    });

    self.skuValueChanged = function (item, event) {
        var element = $(event.target);
        $.ajax({
            url: GET_PRODUCT_ITEM_URL,
            data: { eisSku: item.SKU() },
            success: function (result) {
                $(element).attr("title", "")
                if (result.Error) {
                    $(element).parent().addClass("has-error");
                    $(element).attr("title", "This product was not found!")
                    item.ItemName("")
                    item.Description("");
                    item.QtyAvailable(0);
                    item.UnitPrice(0);
                    return;
                }

                $(element).parent().removeClass("has-error");
                $(element).parent().removeClass("has-warning");

                if (result.QtyAvailable == 0) {
                    $(element).parent().addClass("has-warning");
                    $(element).attr("title", "There is no stock left for this product!")
                }

                item.ItemName(result.Name)
                item.Description(result.Description);
                item.QtyAvailable(result.QtyAvailable);
                item.UnitPrice(result.SupplierPrice);
            }
        });
    }

    self.addOrderItem = function () {
        self.Items.push(new ItemModel(
            {
                PurchaseOrderId: "",
                EisOrderId: "",
                SKU: "",
                ItemName: "",
                Description: "",
                QtyAvailable: 0,
                Qty: 0,
                UnitPrice: 0,
                ShippingPrices: 0,
                Taxes: 0,
                IsPaid: false
            }, [], []));
    }

    self.deleteOrderItem = function (item, event) {
        self.Items.remove(item);
    }

    self.managePurchaseOrderPaid = function (item, event) {
        if (self.markedOrderItems().length == 0)
            return false;

        showSpinner();
        $.ajax({
            type: "POST",
            url: UPDATE_PO_ITEMS_PAID_URL,
            data: JSON.stringify({ poId: self.Id(), paidPoItems: self.markedOrderItems(), unpaidPoItems: self.UnMarkedOrderItemIds() }),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message("Error occured in trying to mark the PO items as paid! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Marked PO items has been successfully saved!");
                setTimeout("$('#PurchaseOrderDetailDialog').modal('hide');", 2000);
                location.reload();
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the marked PO items! <br/> " + result)
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
            url: GET_PURCHASE_ORDER_ITEMS_URL,
            data: { poId: pagedModel.ModelId(), page: next },
            success: function (result) {
                self.PagedItem(new PagedItemModel(result, self.markedOrderItems(), self.unmarkedOrderItems()));
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
            url: GET_PURCHASE_ORDER_ITEMS_URL,
            data: { poId: pagedModel.ModelId(), page: prev },
            success: function (result) {
                self.PagedItem(new PagedItemModel(result, self.markedOrderItems(), self.unmarkedOrderItems()));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }
}

function PagedItemModel(pagedItem, markedOrderItems, unmarkedOrderItems) {
    var self = this;

    var mapping = {
        "Items": {
            create: function (options) {
                return new ItemModel(options.data, markedOrderItems, unmarkedOrderItems);
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

function ItemModel(item, markedOrderItems, unmarkedOrderItems) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.IsPaid(item.IsPaid ? (unmarkedOrderItems.indexOf(item.Id) == -1) : (markedOrderItems.indexOf(item.Id) > -1));

    self.TotalPrice = ko.pureComputed(function () {
        var totalPrice = parseFloat(self.Qty()) * parseFloat(self.UnitPrice());
        return isNaN(totalPrice) ? 0 : (totalPrice).toFixed(2);
    });
}
    
function VendorModel(vendor) {
    var self = this;

    self.Id = ko.observable(vendor.Id);
    self.Name = ko.observable(vendor.Name);
}

function createPurchaseOrder(poId) {
    return new PurchaseOrderModel({
        Id: poId,
        ContactPerson: "",
        VendorId: -1,
        VendorAddress: "",
        PhoneNumber: "",
        PaymentStatus: 0,
        Items: [],
        IsManual: true,
        isCreate: true
    });
}

function deleteBillings() {
    var postData = {
        isSelectAllPages: isSelectAllPages,
        billingIds: (isSelectAllPages ? unselectedModelIds : selectedModelIds)
    };

    $.confirm({
        title: "Bulk Delete Billings",
        text: "Are you sure you want to delete <strong>" + recordsSelected + "</strong> billing items?",
        cancel: function () {
            return false;
        },
        confirm: function () {
            // check if there's selected billings to delete
            if (postData.billingIds.length <= 0)
                return false;

            displayMessage(("EIS is on currently deleting " + recordsSelected + " billings. This will take a while"), "warning");
            $.post(DELETE_BILLING_URL, postData, function (result) {
                if (result.Success) {
                    displayMessage(result.Success + " Reloadin the page...", "success");
                    setTimeout(function () { location.reload(); }, 1000);
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