
function ViewModel() {
    var self = this;

    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.orderGroups = ko.observableArray();
    self.selectedOrderGroup = ko.observable();
    self.order = ko.observable();
    self.markOrderExported = ko.observable();
    self.manualOrder = ko.observable(createManualOrder());
    self.marketplaceOrder = ko.observable(new MarketplaceOrderModel());
    self.manualOrderId = ko.observable(-1);
    self.orderShipment = ko.observable();
    self.carriers = ko.observableArray();
    self.orderStatus = ko.observableArray([{ Id: 5, Name: "Shipped" }, { Id: 3, Name: "Unshipped" }, { Id: 1, Name: "Pending" }, { Id: 7, Name: "Canceled" }])
    self.yesNoOptions = [new ItemModel({ Id: 1, Name: "Yes" }), new ItemModel({ Id: 0, Name: "No" })];
    self.paymentStatusOptions = [   new ItemModel({ Id: 0, Name: "NoPayment" }),
                                    new ItemModel({ Id: 1, Name: "Authorized" }),
                                    new ItemModel({ Id: 2, Name: "UnCleared" }),
                                    new ItemModel({ Id: 3, Name: "Charged" }),
                                    new ItemModel({ Id: 4, Name: "PartialPayment" }),
                                    new ItemModel({ Id: 5, Name: "PartialRefund" }),
                                    new ItemModel({ Id: 6, Name: "FullRefund" }),
                                    new ItemModel({ Id: 7, Name: "PaymentError" })];

    self.marketPlaceOptions = [new ItemModel({ Id: 0, Name: "Amazon" }),
                                new ItemModel({ Id: 1, Name: "ShipStation" }),
                                new ItemModel({ Id: 2, Name: "eBay" }),
                                new ItemModel({ Id: 3, Name: "Manual" })];

    self.vendors = ko.observableArray();

    //for custom Export
    self.customExport = ko.observable();
    // for filter dialog
    self.selectedOrderStatus = ko.observable();
    self.isExported = ko.observable();
    self.shippingAddress = ko.observable();
    self.shippingCity = ko.observable();
    self.shippingCountry = ko.observable();
    self.paymentStatus = ko.observable();
    self.marketplace = ko.observable();
    self.orderDateRange = ko.observable(["2010-01-01", moment()]);
    self.shipmentDateRange = ko.observable(["2010-01-01", moment()]);
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

    self.templates = ko.observableArray();

    self.loadData = function () {

        // retrieve the list of order groups
        $.ajax({
            url: GET_ORDER_GROUPS_URL,
            success: function (results) {
                self.orderGroups(ko.utils.arrayMap(results, function (item) {
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

        self.customExport().availableOrderFields.removeAll();
        self.customExport().availableOrderFields(getOrderFieldsArr()
        .concat(getOrderItemFieldArr()));

        self.customExport().selectedOrderFields.removeAll();


        $.each(arr, function (index, field) {
            // get the item with the item id
            var itemFound = $.grep(self.customExport().availableOrderFields(), function (item) {
                return item.Id() == field;
            });

            if (itemFound.length != 0) {
                self.customExport().availableOrderFields.remove(itemFound[0]);
                self.customExport().selectedOrderFields.push(itemFound[0]);
            }
        });

        self.customExport()
            .Delimiter(item.FileFormat)
            .SortBy(item.SortField)
            .OrderFields(item.Fields.split(","))
            .TemplateName(item.Name);

        $('#loadTemplateOrderModal').modal('hide');

    }

    self.deleteSelectedTemplate = function (item, event) {
        $.post(DELETE_TEMPLATE_URL, { id: item.Id }, function (result) {
            self.loadCustomReportTemplates();
        });
    }

    self.loadFilterModel = function (status, isExported, orderDateFrom, orderDateTo, shipmentDateFrom, shipmentDateTo) {
        self.selectedOrderStatus(status);
        self.isExported(isExported);

        // set the from and to order date range
        if (orderDateFrom) {
            self.orderDateRange([moment(orderDateFrom, "MM-DD-YYYY"), moment(orderDateTo, "MM-DD-YYYY")]);
        }

        // set the from and to shipment date range
        if (shipmentDateFrom) {
            self.shipmentDateRange([moment(shipmentDateFrom, "MM-DD-YYYY"), moment(shipmentDateTo, "MM-DD-YYYY")]);
        }
    }

    self.resetFilters = function () {
        //self.filterOrder({
        //    OrderStatus: "",
        //    IsExported: ""
        //});
        self.selectedOrderGroup(null);
        self.shippingAddress(null);
        self.shippingCity(null);
        self.shippingCountry(null);
        self.selectedOrderStatus(null);
        self.isExported(null);
        self.marketplace(null);
        self.paymentStatus(null);

        $("#filterForm #deletefilter").hide();
        $("#filterForm #filterName").val("");
        $("#filterForm #SearchFilterId").val("");
        $("#filterForm #orderDateRange").val("");
        $("#filterForm #shipmentDateRange").val("");
        return false;
    }

    self.loadModel = function (orderId) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_ORDER_URL,
            data: { orderId: orderId },
            success: function (result) {
                self.order(new OrderModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.loadManualOrder = function (orderId) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_ORDER_URL,
            data: { orderId: orderId },
            success: function (result) {
                self.manualOrder(new OrderModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.loadUnshippedOrderItems = function (orderId) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_UNSHIPPED_ORDERITEMS_URL,
            data: { orderId: orderId },
            success: function (results) {
                self.orderShipment(new OrderShipmentDetailModel(results));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.loadMarketplaceCarriers = function (marketplace) {
        $.ajax({
            url: GET_SHIPPING_CARRIERS_URL,
            data: { marketplace: marketplace },
            success: function (results) {
                self.carriers(results);
            }
        });
    }

    self.confirmOrderShipment = function () {
        if (!isValidateForm())
            return;

        // mapped the ko object to plain JS object
        var orderShiptmentData = ko.mapping.toJS(self.orderShipment());
        showSpinner();

        $.ajax({
            type: "POST",
            url: POST_ORDER_SHIPMENT_URL,
            data: JSON.stringify(orderShiptmentData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message(result.Error);
                    return false;
                }

                viewModel.type("success");
                viewModel.message(result.Success);
                setTimeout("$('#ConfirmShipmentDialog').modal('hide');", 3000);
                location.reload();
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.saveManualOrder = function (order, event) {
        console.log(order);
        if (!isValidateForm()) {
            self.type("error");
            self.message("Please filled-up the required fields with valid value.");
            return;
        }
        console.log("kigwa");

        // iterate to order items and compute its sharing for item tax and shipping price
        var totalItems = self.manualOrder().OrderItems().length, toBeShipped = 0;
        var hasZeroQty = false;
        $.each(self.manualOrder().OrderItems(), function (index, item) {
            if (item.QtyOrdered() == 0) {
                viewModel.type("danger");
                viewModel.message("Product item ordered quantity cannot be zero!");
                hasZeroQty = true;
                return false;
            }

            toBeShipped += parseInt(item.QtyOrdered());
            item.OrderId(self.manualOrder().OrderId());
            item.OrderItemId(self.manualOrder().OrderId() + "-" + (index + 1));
            item.ShippingPrice(self.manualOrder().ShippingPaid() / totalItems);
            item.ItemTax(self.manualOrder().TaxPaid() / totalItems);
            item.Price(item.TotalPrice());
        });

        if (hasZeroQty)
            return;

        showSpinner();
        $(event.target).addClass("disabled");

        var url = SAVE_MANUAL_ORDER_URL;
        if (self.manualOrderId() != -1)
            url = UPDATE_MANUAL_ORDER_URL;

        // mapped the ko object to plain JS object
        self.manualOrder().OrderTotal(self.manualOrder().OrderTotalPaid());
        self.manualOrder().NumOfItemsUnshipped(toBeShipped);
        var orderData = { orderId: self.manualOrderId(), orderModel: ko.mapping.toJS(self.manualOrder()) };

        $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(orderData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    viewModel.type("danger");
                    viewModel.message("Error occured in trying to save the Scheduled Task! <br/> " + result.Error)
                    $(event.target).removeClass("disabled");
                    return;
                }

                // set the alert message
                viewModel.type("info");
                viewModel.message("Manual Order has been successfully saved!");
                setTimeout("$('#OrderDialog').modal('hide');", 3000);
                location.reload();
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Manual Order! <br/> " + result)
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

 /********************************************************
*Author: Prince Jea C. Regencia
*Date: October 15, 2016
*Description: Custom Export Functions
*Revision:
********************************************************/
  self.loadCustomExportOrder = function (rowsSelected) {
            if (rowsSelected == 0) {
                self.type("error");
                self.message("No records selected! Please select first the product records you want to export");
            } else {
                // display the message for items selected
                self.type("warning");
                self.message("You have selected " + rowsSelected + " records to export.");
                $(".alertMsgStatus").removeClass("alert-error");
            }

            self.customExport(new CustomExportModel({
                TemplateName: "",
                Delimiter: ",",
                OrderFields: [],
                SortBy: "",
                SelectedEisOrderId: selectedEisOrderIds,
                ExcludedEisOrderId: unselectedEisOrderIds,
                IsAllOderItems: isSelectAllPages,
                RequestedDate: new Date(),
            }));
    }


    /********************************************************
    
    ********************************************************/
}

function OrderModel(order) {
    var self = this;

    var mapping = {
        "OrderItems": {
            create: function (options) {
                return new OrderItemModel(options.data);
            }
        }
    };
    ko.mapping.fromJS(order, mapping, self);
    self.PurchaseDate = ko.observable(convertDate(order.PurchaseDate || new Date()));
    self.LastUpdateDate = ko.observable(convertDate(order.LastUpdateDate || new Date()));
    self.modalTitle = order.OrderId;
    self.cityRegionZip = ko.observable(order.ShippingCity + ", " + order.ShippingStateOrRegion + " " + order.ShippingPostalCode)

    // for Manual Order properties
    var tax = 0, shipping = 0;
    $.each(order.OrderItems, function (index, item) {
        tax += item.ItemTax;
        shipping += item.ShippingPrice;
    })
    self.ShippingPaid = ko.observable(shipping);
    self.TaxPaid = ko.observable(tax);

    // compute for total amount paid - Manual Order
    self.OrderTotalPaid = ko.pureComputed(function () {
        var total = parseFloat(self.TaxPaid()) + parseFloat(self.ShippingPaid());
        // iterate to its order items
        $.each(self.OrderItems(), function (index, item) {
            total += (item.UnitPrice() * item.QtyOrdered());
        });

        return total;
    });

    // compute the order total tax
    self.orderTax = ko.pureComputed(function () {
        var taxTotal = 0;
        $.each(order.OrderItems, function (index, item) {
            taxTotal += (item.ItemTax + item.ShippingTax + item.GiftWrapTax);
        });

        return taxTotal;
    })

    // compute for the shipping price
    self.orderShippigPrice = ko.pureComputed(function () {
        var priceTotal = 0;
        $.each(order.OrderItems, function (index, item) {
            priceTotal += item.ShippingPrice;
        });

        return priceTotal;
    });

    // compute the order dicounts
    self.orderDiscount = ko.pureComputed(function () {
        var discountTotal = 0;
        $.each(order.OrderItems, function (index, item) {
            discountTotal += (item.ShippingDiscount + item.PromotionDiscount);
        });

        return discountTotal;
    })

    self.ShipmentCostString = ko.pureComputed(function () {
        return '$' + order.ShipmentCost;
    });

    self.ShipmentDateString = ko.pureComputed(function () {
        
        if (order.ShipmentDate != "/Date(-62135596800000)/") {
            return new Date(parseInt(order.ShipmentDate.replace('/Date(', ''))).toLocaleDateString();
        }

        return "";
    });

    self.skuValueChanged = function (orderItem, event) {
        var element = $(event.target);
        $.ajax({
            url: GET_PRODUCT_ITEM_URL,
            data: { eisSku: orderItem.SKU() },
            success: function (result) {
                $(element).attr("title", "")
                if (result.Error) {
                    $(element).parent().addClass("has-error");
                    $(element).attr("title", "This product was not found!")
                    orderItem.Title("")
                    orderItem.Description("");
                    orderItem.QtyAvailable(0);
                    orderItem.UnitPrice(0);
                    return;
                }

                $(element).parent().removeClass("has-error");
                $(element).parent().removeClass("has-warning");

                if (result.QtyAvailable == 0) {
                    $(element).parent().addClass("has-warning");
                    $(element).attr("title", "There is no stock left for this product!")
                }

                orderItem.MarketplaceItemId(result.SKU);
                orderItem.Title(result.Name)
                orderItem.Description(result.Description);
                orderItem.QtyAvailable(result.QtyAvailable);
                orderItem.UnitPrice(result.SellingPrice);
            }
        });
    }

    self.addOrderItem = function () {
        self.OrderItems.push(new OrderItemModel({ SKU: "", MarketplaceItemId: "", Title: "", Description: "", QtyAvailable: 0, QtyOrdered: 0, Price: 0, ShippingPrice: 0, ItemTax: 0, OrderItemId: "", OrderId: "" }));
    }

    self.deleteOrderItem = function (orderItem, event) {
        self.OrderItems.remove(orderItem);
    }


    self.marketPlaceUrl = ko.pureComputed(function () {
        if (order.Marketplace == "Amazon") {
            return "https://sellercentral.amazon.com/hz/orders/details?_encoding=UTF8&orderId=" + order.OrderId
           }
    })
}




function OrderItemModel(orderItem) {
    var self = this;

    ko.mapping.fromJS(orderItem, {}, self);
    self.Description = ko.observable(orderItem.Description || "");
    self.QtyAvailable = ko.observable(orderItem.QtyAvailable || "");
    self.UnitPrice = ko.observable(orderItem.Price / orderItem.QtyOrdered);

    self.TotalPrice = ko.pureComputed(function () {
        var totalPrice = parseFloat(self.QtyOrdered()) * parseFloat(self.UnitPrice());
        return isNaN(totalPrice) ? 0 : (totalPrice).toFixed(2);
    });

    self.marketplaceItemIdUrl = ko.pureComputed(function () {
        if (self.Marketplace() == "Amazon") {
            return "https://amazon.com/dp/" + self.MarketplaceItemId();
        } else if (self.Marketplace() == "eBay") {
            return "http://www.ebay.com/itm/" + self.MarketplaceItemId();
        } else {
            return "/product/edit/" + self.SKU();
        }
           
    })

    self.productDetailUrl = ko.pureComputed(function () {
        return "/product/edit/" + self.SKU();
    })

}

function OrderShipmentDetailModel(orderShipment) {
    var self = this;

    var mapping = {
        "OrderItems": {
            create: function (options) {
                return new OrderItemModel(options.data);
            }
        }
    };

    ko.mapping.fromJS(orderShipment, mapping, self);
}

function createManualOrder() {
    return new OrderModel({
        Marketplace: "Eshopo",
        OrderId: -1,
        BuyerName: "",
        BuyerEmail: "",
        ShippingAddressPhone: "",
        ShippingAddressName: "",
        ShippingAddressLine1: "",
        ShippingAddressLine2: "",
        ShippingCity: "",
        ShippingStateOrRegion: "",
        ShippingPostalCode: "",
        CompanyName: "",
        PurchaseDate: new Date(),
        LastUpdateDate: new Date(),
        NumOfItemsUnshipped: 0,
        OrderTotal: 0,
        ShippingPaid: 0,
        TaxPaid: 0,
        MarketplaceItemId: "",
        OrderItems: [{ SKU: "", MarketplaceItemId: "", Title: "", QtyAvailable: 0, QtyOrdered: 0, Price: 0, Description: "", ShippingPrice: 0, ItemTax: 0, OrderItemId: "", OrderId: "" }]
    });
}

function ItemModel(item) {
    var self = this;

    self.Id = ko.observable(item.Id);
    self.Name = ko.observable(item.Name);
}

function ItemFormatModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);
    self.IsChecked = ko.observable(item.IsChecked || false);

    self.selectAll = function (item, event) {
        $(event.target).select();
    }
}

function MarketplaceOrderModel() {
    var self = this;
    
    self.isImportingOrder = ko.observable(false);
    self.marketplace = ko.observable();
    self.marketplaceOrderIdsStr = ko.observable();
    self.marketplaceOrderIds = ko.pureComputed(function () {
        // return null if the fields is null or only whitespace
        if (self.marketplaceOrderIdsStr() == null || $.trim(self.marketplaceOrderIdsStr()) == '')
            return null;
        return self.marketplaceOrderIdsStr().split(",");
    });
    

    self.importMaketplaceOrders = function () {
        if (self.marketplaceOrderIds() == null)
            return false;
        
        showSpinner();
        self.isImportingOrder(true);
        $.ajax({
            traditional: true,
            url: IMPORT_MARKETPLACE_ORDERS_URL,
            data: { marketplace: self.marketplace(), marketplaceOrderIds: self.marketplaceOrderIds() },
            success: function (result) {
                if (result.IsSuccess) {
                    displayMessage("A total of " + result.Message + " marketplace orders have been imported. Please search and confirm the orders.");
                } else {
                    displayMessage("No orders have been found or there is a problem encountered by the system.", "warning")
                }
            },
            complete: function () {
                hideSpinner();
                setTimeout("$('#ImportMarketplaceOrderDialog').modal('hide');", 100);
                self.isImportingOrder(false);
            }
        });
    }
}

function MarkingOrderModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);

    self.toggleExportMarking = function (data, event) {
        self.IsExported($(event.target).val());
        return true;
    }

    self.hasSelected = ko.pureComputed(function () {
        if (isSelectAllPages)
            return true;

        if (self.EisOrderIds().length > 0)
            return true;

        return false;
    });

    self.submitMarkingOrders = function (model, event) {
        showSpinner();
        $.post(TOGGLE_ORDERS_EXPORT_URL,
            {
                IsSelectAllPages: isSelectAllPages,
                IsExported: model.IsExported(),
                EisOrderIds: isSelectAllPages ? unselectedEisOrderIds : model.EisOrderIds()
            },
            function (result) {
                location.reload();
            }
        );
    }

}

function unshippedOrder(source, orderId, marketplace) {
    $.confirm({
        title: "Unshipped Manual Order",
        text: "Are you sure you want to unshipped this Order ID: <strong> " + orderId + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(UNSHIPPED_ORDER_URL, { orderId: orderId, marketplace: marketplace }, function (result) {
                if (result.Success) {
                    $("#status_" + orderId).addClass("bg-Unshipped");
                    $("#status_" + orderId).text("Unshipped");
                }
            });
        },
        confirmButton: "Yes, I am",
        confirmButtonClass: "btn-warning"
    });

    return false;
}

function createShipmentLabel(source, orderId) {

    $.ajax({
        url: CREATE_SHIPMENT_LABEL,
        data: { orderId: orderId },
        beforeSend: function () {
            // let's show the loading gif
            $("#loadingDiv").show();
        },
        success: function (result) {          
            //window.open("data:application/pdf;base64," + result.base64ResultString);

            var pdfdata = 'data:application/octet-stream;base64,' + result.base64ResultString;

            var dlnk = document.getElementById('dwnldLnk');
            dlnk.href = pdfdata;

            dlnk.click();
        },
        complete: function () {
            // let's hide the loading gif
            $("#loadingDiv").hide();
        }
    });

    return false;
}


function postShipStation(source, orderId) {

    $.ajax({
        url: POST_SHIPSTATION,
        type: "POST",
        data: { orderId: orderId },
        beforeSend: function () {
            // let's show the loading gif
            $("#loadingDiv").show();
        },
        success: function (result) {
            if (result.result > 0) {
                viewModel.type("success");
                viewModel.message("Order successfully posted to ShipStation.");
            }
            else {
                $("div.alertMsgStatus").removeClass("alert-warning");
                viewModel.type("error");
                viewModel.message("Order not posted to ShipStation.");
            }
        },
        complete: function () {
            // let's hide the loading gif
            $("#loadingDiv").hide();
        }
    });

    return false;
}

function cancelOrder(source, orderId, marketplace) {
    $.confirm({
        title: "Cancel Manual Order",
        text: "Are you sure you want to cancel this Order ID: <strong> " + orderId + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(CANCEL_ORDER_URL, { orderId: orderId, marketplace: marketplace }, function (result) {
                if (result.Success) {
                    $("#status_" + orderId).addClass("bg-Canceled");
                    $("#status_" + orderId).text("Canceled");
                }
            });
        },
        confirmButton: "Yes, I am",
        confirmButtonClass: "btn-warning"
    });

    return false;
}

function updateOrderProducts(source, orderId) {
    $.confirm({
        title: "Update Order Produts",
        text: "Are you sure you want to update its order products for this Order ID: <strong> " + orderId + "</strong>",
        cancel: function () {
            return false;
        },
        confirm: function () {
            $.post(UPDATE_ORDER_PRODUCTS, { orderId: orderId }, function (result) {
                if (result.Success) {
                    location.reload();
                }
            });
        },
        confirmButton: "Yes, I am",
        confirmButtonClass: "btn-warning"
    });

    return false;
}

function getLatestMarketplaceOrderData(marketplace, marketplaceOrderId, eisOrderId) {
    $.ajax({
        url: GET_LATEST_MARKETPLACE_ORDER_URL,
        data: { "marketplace": marketplace, "marketplaceOrderId": marketplaceOrderId },
        beforeSend: function () {
            // let's show the loading gif
            $("#loadingDiv").show();
        },
        success: function (result) {
            if (result == null)
                return false;

            // set the value for each td's
            $("#tdBuyerName_" + eisOrderId).text(result.BuyerName);
            $("#tdShippingAddressLine1_" + eisOrderId).text(result.ShippingAddressLine1);
            $("#tdOrderTotal_" + eisOrderId).text(result.OrderTotal);
            $("#tdPurchaseDate_" + eisOrderId).text(result.PurchaseDate);
            $("#tdOrderStatus_" + eisOrderId).html("<span id='status_" + marketplaceOrderId +"' class='label bg-" + result.OrderStatus + "'>" + result.OrderStatus + "</span>");
        },
        complete: function () {
            // let's hide the loading gif
            $("#loadingDiv").hide();
        }
    });

    return false;
}

function CustomExportModel(model) {
    var self = this;

    ko.mapping.fromJS(model, {}, self);

    self.fileFormats = ko.observableArray(getFileFormats());
    self.availableOrderFields = ko.observableArray(getOrderFieldsArr().concat(getOrderItemFieldArr()));
    self.selectedOrderFields = ko.observableArray();
    self.selectedFieldsToAdd = ko.observableArray();
    self.selectedFieldsToRemove = ko.observableArray();

    self.addSelectedFields = function (model, event) {
        $.each(self.selectedFieldsToAdd(), function (index, field) {
            // get the item with the item id
            var itemFound = $.grep(self.availableOrderFields(), function (item) {
                return item.Id() == field;
            });

            if (itemFound.length != 0) {
                self.selectedOrderFields.push(itemFound[0]);
                self.availableOrderFields.remove(itemFound[0]);
                self.OrderFields.push(itemFound[0].Id());
            }
        });
    }

    self.removeSelectedFields = function (model, evet) {
        $.each(self.selectedFieldsToRemove(), function (index, field) {
            //get the item with the item id
            var itemFound = $.grep(self.selectedOrderFields(), function (item) {
                return item.Id() == field;
            });

            if (itemFound.length != 0) {
                self.availableOrderFields.push(itemFound[0]);
                self.selectedOrderFields.remove(itemFound[0]);
                self.OrderFields.remove(itemFound[0].Id());
            }

        });
    }

    self.saveTemplate = function (model, event) {
        if (self.OrderFields().length == 0 || self.TemplateName() == "" || self.SortBy() == null) {
            alert("Please complete the required fields before saving");
        }
        else {

            var model = {
                Name: self.TemplateName(),
                FileFormat: self.Delimiter(),
                SortField: self.SortBy(),
                Fields: self.OrderFields().join()
            };

            $.post(SAVE_TEMPLATE_URL, { model: model }, function (result) {
                document.getElementById("successAlert").style.display = 'inline';
                setTimeout(function () { document.getElementById("successAlert").style.display = 'none'; }, 3000);

            });
        }
    }

    self.downloadCustomExport = function (model, event) {
        if (!isValidFormData("custom-export-form")) {
            return;
        }

        if (self.OrderFields().length == 0) {
            $("div.alertMsgStatus").removeClass("alert-warning");
            viewModel.type("error");
            viewModel.message("Cannot export product with no fields selected. Please select product fields to export.");
            return false;
        }

        showSpinner();
        $(event.target).addClass("disabled");
        $("#customExportForm").submit();

        // set the alert message
        viewModel.type("info");
        viewModel.message("Custom export for orders has been started. Please wait for a moment.");
        setTimeout("$('#CustomExportOrderDialog').modal('hide');", 2000);
        hideSpinner();
    }


}

function getFileFormats() {
    return [
        new ItemFormatModel({ Id: ",", Name: "Comma Delimited" }),
        new ItemFormatModel({ Id: "\t", Name: "Tab Delimited" }),
        new ItemFormatModel({ Id: "|", Name: "Bar Delimited" }),
    ]
}

function getOrderFieldsArr() {
    return [
        new ItemFormatModel({ Id: "orders.OrderId", Name: "OrderId", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.Marketplace", Name: "Marketplace", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.OrderTotal", Name: "OrderTotal", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.NumOfItemsShipped", Name: "NumOfItemsShipped", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.NumOfItemsUnshipped", Name: "NumOfItemsUnshipped", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.OrderStatus", Name: "OrderStatus", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.PurchaseDate", Name: "PurchaseDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.LastUpdateDate", Name: "LastUpdateDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.PaymentMethod", Name: "PaymentMethod", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.BuyerName", Name: "BuyerName", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.BuyerEmail", Name: "BuyerEmail", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingAddressPhone", Name: "ShippingAddressPhone", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingAddressName", Name: "ShippingAddressName", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingAddressLine1", Name: "ShippingAddressLine1", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingAddressLine2", Name: "ShippingAddressLine2", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingAddressLine3", Name: "ShippingAddressLine3", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingCity", Name: "ShippingCity", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingStateOrRegion", Name: "ShippingStateOrRegion", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingPostalCode", Name: "ShippingPostalCode", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShipServiceLevel", Name: "ShipServiceLevel", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShipmentServiceCategory", Name: "ShipmentServiceCategory", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.EarliestShipDate", Name: "EarliestShipDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.LatestShipDate", Name: "LatestShipDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.EarliestDeliveryDate", Name: "EarliestDeliveryDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.LatestDeliveryDate", Name: "LatestDeliveryDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.OrderType", Name: "OrderType", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.SellerOrderId", Name: "SellerOrderId", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.MarketplaceId", Name: "MarketplaceId", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.PurchaseOrderNumber", Name: "PurchaseOrderNumber", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.SalesChannel", Name: "SalesChannel", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.AdjustmentAmount", Name: "AdjustmentAmount", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.AmountPaid", Name: "AmountPaid", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.PaymentOrRefundAmount", Name: "PaymentOrRefundAmount", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShipmentDate", Name: "ShipmentDate", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.CarrierCode", Name: "CarrierCode", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShippingMethod", Name: "ShippingMethod", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.TrackingNumber", Name: "TrackingNumber", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.ShipmentCost", Name: "ShipmentCost", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.Created", Name: "Created", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orders.CompanyName", Name: "CompanyName", IsChecked: false, Sort: "" })
    ];
}

function getOrderItemFieldArr() {
    return [
        new ItemFormatModel({ Id: "orderitems.OrderItemId", Name: "OrderItemId", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.ItemId", Name: "ItemId", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.SKU", Name: "SKU", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.Title", Name: "Title", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.QtyOrdered", Name: "QtyOrdered", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.QtyShipped", Name: "QtyShipped", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.OrderStatus", Name: "OrderStatus", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.Price", Name: "Price", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.ShippingPrice", Name: "ShippingPrice", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.GiftWrapPrice", Name: "GiftWrapPrice", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.itemTax", Name: "itemTax", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.ShippingTax", Name: "ShippingTax", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.GiftWrapTax", Name: "GiftWrapTax", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.ShippingDiscount", Name: "ShippingDiscount", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.PromotionDiscount", Name: "PromotionDiscount", IsChecked: false, Sort: "" }),
        new ItemFormatModel({ Id: "orderitems.ConditionNote", Name: "ConditionNote", IsChecked: false, Sort: "" }),
    ];
}



