
function ViewModel() {
    var self = this;
    
    // alert message
    self.type = ko.observable("");
    self.message = ko.observable();

    self.modalTitle = ko.observable();
    self.isViewOnly = ko.observable(false);
    self.spec = {};

    self.taskTypes = ko.observableArray([
        { Id: "GeneratePO", Name: "Generate Purchase Order" },
        { Id: "MarketplaceInventory", Name: "Marketplace Inventory" },
        { Id: "VendorProductFileInventory", Name: "VendorProduct File Inventory" },
        { Id: "CustomExportProduct", Name: "Custom Export Product" },
        { Id: "CustomExportOrder", Name: "Custom Export Orders" },
        { Id: "CustomImportOrder", Name: "Custom Import Orders" }]);
    self.selectedTaskType = ko.observable();
    self.exportToOptions = ko.observableArray(["None", "FTP"]);
    self.fileTypes = ko.observableArray([".csv", ".txt"]);
    self.yesNos = ko.observableArray([{ Id: 1, Name: "Yes" }, { Id: 0, Name: "No" }]);
    self.exportMarks = ko.observableArray(["Exported", "Not Exported"]);
    self.orderStatus = ko.observableArray([
        { Id: "5", Name: "Shipped" },
        { Id: "3", Name: "Unshipped" },
        { Id: "1", Name: "Pending" },
        { Id: "7", Name: "Canceled" },
        { Id: "3|5", Name: "Unshipped & Shipped" }])
    self.vendors = ko.observableArray();
    self.companies = ko.observableArray();
    self.scheduledTasks = ko.observableArray();
    self.scheduledTask = ko.observable();
    self.pagedExportedFile = ko.observable();

    self.loadModel = function (modelId) {
        $("#loadingModal").show();
        self.fileTypes = ko.observableArray([".csv", ".txt"]);

        // set the different options for respective task types
        if (self.selectedTaskType() == "CustomExportOrder") {
            self.exportToOptions(["None", "FTP"]);
            self.modalTitle((modelId == -1 ? "Add New" : "Edit") + " Custom Export Order Task");
            self.customFieldOptions = ko.observableArray(getOrderFieldsArr());
        } else if (self.selectedTaskType() == "GeneratePO") {
            self.exportToOptions(["None", "FTP", "Email"]);
            self.modalTitle((modelId == -1 ? "Add New" : "Edit") + "  Generate Purchase Order Task");
        } else if (self.selectedTaskType() == "MarketplaceInventory") {
            self.modalTitle((modelId == -1 ? "Add New" : "Edit") + "  Marketplace Inventory Task");
        } else if (self.selectedTaskType() == "VendorProductFileInventory") {
            self.modalTitle((modelId == -1 ? "Add New" : "Edit") + "  VendorProduct File Inventory Task");
        }
        else if (self.selectedTaskType() == "CustomImportOrder") {
            self.exportToOptions(["None", "FTP"]);
            self.modalTitle((modelId == -1 ? "Add New" : "Edit") + " Custom Import Order Task");
            self.customFieldOptions = ko.observableArray(getOrderImportFieldsArr());
            self.fileTypes = ko.observableArray([".csv"]);
        }
        else {
            self.exportToOptions(["None", "FTP", "Email"]);
            self.modalTitle((modelId == -1 ? "Add New" : "Edit") + "  Custom Export Product Task");
            self.customFieldOptions = ko.observableArray([
                "vendor_product.EisSupplierSKU",
                "vendor_product.Quantity",
                "vendor_product.SupplierPrice",
                "products.EisSKU",
                "products.Name",
                "products.Description",
                "products.UPC",
                "products.SellerPrice",
                "products.GuessedWeight",
                "products.AccurateWeight",
                "products.GuessedShipping",
                "products.AccurateShipping"]);
        }


        if (modelId == -1) {
            self.scheduledTask(createTaskModel(self.selectedTaskType()));            
            $("#loadingModal").hide();
        } else {
            $.ajax({
                url: GET_SCHEDULED_TASK_URL,
                data: { id: modelId },
                success: function (result) {
                    self.scheduledTask(new ScheduledTaskModel(result));
                },
                complete: function () {
                    $("#loadingModal").hide();
                }
            });
        }
    }

    self.loadData = function () {
        // load all scheduled tasks
        $.ajax({
            url: GET_SCHEDULED_TASKS_URL,
            success: function (results) {
                self.scheduledTasks(ko.utils.arrayMap(results, function (item) {
                    return new ScheduledTaskListModel(item);
                }));
            }
        });

        // load all the vendors for filtering product
        $.ajax({
            url: GET_VENDORS_URL,
            success: function (results) {
                self.vendors(ko.utils.arrayMap(results, function (item) {
                    return new OptionModel(item);
                }));
            }
        });
        
        // load all the companies
        $.ajax({
            url: GET_COMPANIES_URL,
            success: function (results) {
                self.companies(ko.utils.arrayMap(results, function (item) {
                    return new OptionModel(item);
                }));
            }
        });
    }

    self.saveScheduledTask = function (task, event) {
        
        // set the marketplace field for MarketplaceInventory task type
        if (task.TaskType() == "MarketplaceInventory") {
            var selectedFields = $.map(task.itemFields(), function (item) {
                return item.IsChecked() == true ? item.Id() : null;
            });

            if (task.Marketplaces != undefined)
                task.Marketplaces(selectedFields);
        }

        // check for selected company IDs if it is CustomExportProduct
        if (task.TaskType() == "CustomExportProduct" && task.CompanyIds().length == 0) {
            $("#CompanyIds").parents(".form-group").addClass("has-error");
            return false;
        }

        // set the selected currence
        task.Recurrence(task.recurrenceTemplate());

        if (!isValidateForm()) {
            return;
        }

        showSpinner();
        $(event.target).addClass("disabled");

        // repopulate the FileHeaders and CustomFields for "CustomExportProduct" and "CustomExportOrder" type
        if (task.TaskType() == "CustomExportProduct" || task.TaskType() == "CustomExportOrder" || task.TaskType() == "CustomImportOrder") {

            task.CustomFields.removeAll();
            task.FileHeaders.removeAll();
            
            // iterate to the customFields and set to the cleared arrays
            $.each(task.customFields(), function (index, item) {
                task.CustomFields.push(item.CustomField());
                task.FileHeaders.push(item.FileHeader());
            })
        }

        if (task.TaskType() == "CustomImportOrder") {
          
            if (task.customFields().length == 0)
            {
                viewModel.type("danger");
                viewModel.message("Enter custom field with required fields OrderId");
                return false;
            }
        }


        // mapped the ko object to plain JS object
        var taskData = ko.mapping.toJS(task);
        
        $.ajax({
            type: "POST",
            url: SAVE_SCHEDULED_TASK_URL,
            data: JSON.stringify(taskData),
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
                viewModel.message("Scheduled Task have been successfully saved!");
                setTimeout("$('#TaskDialog').modal('hide');", 2000);
            },
            error: function (result) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Scheduled Task! <br/> " + result)
                $(event.target).removeClass("disabled");
            },
            complete: function () {
                hideSpinner();
            }
        });
    }

    self.deleteScheduledTask = function (task, event) {
        $.confirm({
            title: "Delete Scheduled Task",
            text: "Are you sure you want to delete scheduled task: <strong> " + task.Name() + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_SCHEDULED_TASK_URL, { id: task.Id }, function (result) {
                    if (result.Success) {
                        self.scheduledTasks.remove(task);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.isRunNowScheduledTaks = function (task, event) {
        $.confirm({
            title: "Run Scheduled Task",
            text: "Are you sure you want to run it now?<br/> <strong> " + task.Name() + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.ajax({
                    url: EXECUTE_TASK_NOW_URL,
                    data: { taskId: task.Id() },
                    success: function (result) {
                        $("#msgStatus").show();
                        $("#msgStatus").text(result.Success);
                        setTimeout(function () { $("#msgStatus").fadeOut('slow'); }, 2000);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.loadTaskExportedFiles = function (taskId, taskType) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_TASK_EXPORTED_FILES_URL,
            data: { taskId: taskId },
            success: function (result) {
                self.pagedExportedFile(new PagedExportedFileModel(result, taskType));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.prevPage = function (pagedFile, event) {
        var previous = pagedFile.CurrentPageIndex() - 1;
        if (previous <= 0)
            return false;

        $("#loadingModal").show();
        $.ajax({
            url: GET_TASK_EXPORTED_FILES_URL,
            data: { taskId: pagedFile.ModelId(), page: previous },
            success: function (result) {
                self.pagedExportedFile(new PagedExportedFileModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.nextPage = function (pagedFile, event) {
        var next = pagedFile.CurrentPageIndex() + 1;
        if (next > pagedFile.TotalPageCount())
            return false;

        $("#loadingModal").show();
        $.ajax({
            url: GET_TASK_EXPORTED_FILES_URL,
            data: { taskId: pagedFile.ModelId(), page: next },
            success: function (result) {
                self.pagedExportedFile(new PagedExportedFileModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.checkFtpConnection = function(task, event){
        var postData = {
            Server: task.FtpServer(),
            UserName: task.FtpUser(),
            Password: task.FtpPassword(),
            Port: task.FtpPort(),
            RemoteFolder: task.RemoteFolder()
        };

        // check if already in checking mode via button state
        if ($(event.target).is('[disabled=disabled]'))
            return false;

        var origContent = $(event.target).html();
        $(event.target).text("Checking...");
        $(event.target).attr("disabled", true);

        // submit the post data
        $.ajax({
            type: "POST",
            contentType: "application/json",
            url: CHECK_FTP_CONNECTION,
            data: JSON.stringify(postData),
            success: function (result) {
                if (result.Severity == 1) {
                    $("#ftpMsgStatus").html(result.Description);
                    $("#ftpMsgStatus").attr("class", "alert-success");
                } else {
                    $("#ftpMsgStatus").html(result.Description);
                    $("#ftpMsgStatus").attr("class", "alert-error");
                }
            },
            complete: function (result) {
                $(event.target).html(origContent);
                $(event.target).attr("disabled", false);
            }
        });
    }

    self.checkFileFromFtp = function (task, event) {
        var postData = {
            Server: task.FtpServer(),
            UserName: task.FtpUser(),
            Password: task.FtpPassword(),
            Port: task.FtpPort(),
            RemoteFolder: task.RemoteFolder(),
            FileName: task.FileName() + task.FileType()
        };

        // check if already in checking mode via button state
        if ($(event.target).is('[disabled=disabled]'))
            return false;

        var origContent = $(event.target).html();
        $(event.target).text("Checking...");
        $(event.target).attr("disabled", true);

        // submit the post data
        $.ajax({
            type: "POST",
            contentType: "application/json",
            url: CHECK_FILE_FROM_FTP,
            data: JSON.stringify(postData),
            success: function (result) {
                if (result.Severity == 1) {
                    $("#ftpMsgStatus").html(result.Description);
                    $("#ftpMsgStatus").attr("class", "alert-success");
                } else {
                    $("#ftpMsgStatus").html(result.Description);
                    $("#ftpMsgStatus").attr("class", "alert-error");
                }
            },
            complete: function (result) {
                $(event.target).html(origContent);
                $(event.target).attr("disabled", false);
            }
        });
    }
}

function ScheduledTaskListModel(task) {
    var self = this;

    ko.mapping.fromJS(task, {}, self);
}

function ScheduledTaskModel(task) {
    var self = this;

    ko.mapping.fromJS(task, {}, self);
    self.StartDate = ko.observable(convertDate(task.StartDate || new Date()));    
    self.LastExecutedOn = ko.observable(convertDate(task.LastExecutedOn || new Date()));
    self.weekdays = ko.observableArray(getWeekdays(task.Days || []));
    self.recurrenceTemplate = ko.observable(task.Recurrence);

    if (task.TaskType == "MarketplaceInventory") {
        self.recurrences = ko.observableArray(getRecurrences((task.Recurrence || []), true));
        self.itemFields = ko.observableArray(getMarketplaces(task.Marketplaces || []));
        self.FileName = ko.observableArray(null);
    } else {
        self.recurrences = ko.observableArray(getRecurrences((task.Recurrence || []), false));
        self.itemFields = ko.observableArray(getOrderFieldsArr(task.OrderFields || []));
    }

    if (task.TaskType == "CustomExportProduct" || task.TaskType == "CustomExportOrder" || task.TaskType == "CustomImportOrder") {
        
        var customFieldsLength = self.CustomFields().length;
        self.customFields = ko.observableArray();
        if (customFieldsLength > 0) {
            for (var i = 0; i < customFieldsLength; i++) {
                self.customFields.push(new ItemModel({ FileHeader: self.FileHeaders()[i], CustomField: self.CustomFields()[i] }));
            }
        } else {

            if (task.TaskType == "CustomImportOrder") {
                var defaultHeaders = ["OrderId" ];
                for (var i = 0; i < 1; i++) {
                    self.customFields.push(new ItemModel({ FileHeader: "", CustomField: defaultHeaders[i] }));
                }
                self.FileName = ko.observableArray(null);
            }
            else {
                self.customFields.push(new ItemModel({ FileHeader: "", CustomField: "" }));
            }
        }
    }
    
    self.startTimeDate = ko.pureComputed(function () {
        return (moment(self.StartDate()).format("MM/DD/YYYY") + ' ' + moment(self.StartTimeDate()).format('hh:mm A'));
    });

    self.isFtpDetailsVisible = ko.pureComputed(function () {
        return self.ExportTo() == "FTP";
    })

    self.isEmailFieldsVisible = ko.pureComputed(function () {
        return self.ExportTo() == "Email";
    });

    self.Days = ko.pureComputed(function () {
        var selectedDays = $.map(self.weekdays(), function (item) {
            return item.IsChecked() == true ? item.Id() : null;
        });

        if (selectedDays.length != 0) {
            $("div#weekdays").parents(".form-group").removeClass("has-error");
        }
        return selectedDays;
    });

    self.toggleField = function (field, event) {
        var selectedFields = $.map(self.itemFields(), function (item) {
            return item.IsChecked() == true ? item.Id() : null;
        });


        if (!field.IsChecked()) { // the field has been checked
            field.Sort(selectedFields.length + 1)
        }
        else {
            field.Sort("");
        }

        // remove the .has-error from the parent DOM if there's any
        $("div#order-fields").parents(".form-group").removeClass("has-error");
    }

    self.vendorChanged = function (task, event) {
        $.getJSON(GET_VENDOR_EMAIL + task.VendorId(), function (result) {
            self.EmailTo(result);
        })

        valueChanged(task.VendorId(), event);
    }

    self.addCustomFieldRow = function () {
        self.customFields.push(new ItemModel({ FileHeader: "", CustomField: "" }));
    }

    self.removeCustomFieldRow = function (customFieldRow, event) {
        self.customFields.remove(customFieldRow);
    }

    self.toggleRecurrence = function (recurrence, event) {
        self.recurrenceTemplate(recurrence.Id());
    }
}

function OptionModel(item) {
    var self = this;

    self.Id = ko.observable(item.Id);
    self.Name = ko.observable(item.Name);
}

function PagedExportedFileModel(pagedFile, taskType) {
    var self = this;

    ko.mapping.fromJS(pagedFile, {}, self);
    self.taskType = ko.observable(taskType);

    self.downloadFile = function (file, event) {
        $("#form_" + file.Id()).submit();
    }

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

function ExportedFileModel(file) {
    var self = this;

    ko.mapping.fromJS(file, {}, self);
}

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);

    self.selectAll = function (item, event) {
        $(event.target).select();
    }
}

function createTaskModel(taskType) {
    return new ScheduledTaskModel({
        Id: -1,
        IsEnabled: true,
        Name: "",
        TaskType: taskType,
        StartDate: new Date(),
        StartTimeDate: new Date(),
        VendorId: "",
        CompanyId: "",
        LastExecutedOn: new Date(),
        Days: [],
        Recurrence: "Daily",
        OccurrAt: 1,
        ExportTo: "",
        FileName: "FILENAME-{0:MM}-{0:dd}-{0:yyyy}-{0:HH}-{0:mm}",
        FileType: "",
        FtpServer: "",
        FtpUser: "",
        FtpPassword: "",
        FtpPort: "",
        RemoteFolder: "",
        HasHeader: 1,
        ExportMarkIn: "",
        StatusIn: "",
        MarkOrderExport: 1,
        MarkPoGenerated: 1,
        IsDeleteFile: 1,
        IsDropNoOrderFile: 0,
        EmailSubject: "",
        EmailTo: "",
        EmailCc: "",
        ConfirmationEmailTos: "",
        OrderFields: [],
        Marketplaces: [],
        IsZeroOutQty: false,
        IsAddNewItem: false,
        IsLinkEisSKU: false,
        IsCreateEisSKUAndLink: false,
        IsAddDropShipFee: false,
        IsUseGuessedWeight: false,
        IsUseGuessedShipping: false,
        SKU: "",
        Quantity: "",
        SupplierPrice: "",
        ProductName: "",
        Description: "",
        Category: "",
        UPC: "",
        MinPack: "",
        CompanyIds: [],
        FileHeaders: [],
        CustomFields: []
    });
}

function getOrderFieldsArr(orderFields) {
    return [
        "vendorproducts.EisSupplierSKU",
        "vendorproducts.SupplierSKU",
        "vendorproducts.Name",
        // order products fields
        "orderproducts.Quantity",
        // order fields
        "orders.EisOrderId",
        "orders.OrderId",
        "orders.Marketplace",
        "orders.OrderStatus",
        "orders.PaymentMethod",
        "orders.BuyerName",
        "orders.BuyerEmail",
        "orders.ShippingAddressPhone",
        "orders.ShippingAddressName",
        "orders.ShippingAddressLine1",
        "orders.ShippingAddressLine2",
        "orders.ShippingAddressLine3",
        "orders.ShippingCity",
        "orders.ShippingStateOrRegion",
        "orders.ShippingPostalCode",
        "orders.ShipmentServiceCategory",
        "orders.EarliestShipDate",
        "orders.LatestShipDate",
        "orders.EarliestDeliveryDate",
        "orders.LatestDeliveryDate",
        "orders.PurchaseOrderNumber",
        "orders.AdjustmentAmount",
        "orders.AmountPaid",
        "orders.PaymentOrRefundAmount",
        "orders.ShipmentDate",
        "orders.CarrierCode",
        "orders.ShippingMethod",
        "orders.TrackingNumber",
        "orders.ShipmentCost",
        "orders.PaymentStatus",
        "orders.OrderNote",
        // order items
        "orderitems.OrderItemId",
        "orderitems.ItemId",
        "orderitems.SKU",
        "orderitems.Title",
        "orderitems.QtyOrdered",
        "orderitems.QtyShipped",
        "orderitems.Price",
        "orderitems.ShippingPrice",
        "orderitems.GiftWrapPrice",
        "orderitems.ItemTax",
        "orderitems.ShippingTax",
        "orderitems.GiftWrapTax",
        "orderitems.ShippingDiscount",
        "orderitems.PromotionDiscount",
        "orderitems.ConditionNote"];
}

function getOrderImportFieldsArr(orderFields) {
    return [
        "OrderId",
"EisOrderId",
"Marketplace",
"OrderTotal",
"OrderStatus",
"PaymentStatus",
"NumOfItemsShipped",
"NumOfItemsUnshipped",
"PurchaseDate",
"LastUpdateDate",
"PaymentMethod",
"CompanyName",
"BuyerName",
"BuyerEmail",
"ShippingAddressPhone",
"ShippingAddressName",
"ShippingAddressLine1",
"ShippingAddressLine2",
"ShippingAddressLine3",
"ShippingCity",
"ShippingStateOrRegion",
"ShippingPostalCode",
"ShipServiceLevel",
"ShipmentServiceCategory",
"ShipmentDate",
"CarrierCode",
"ShippingMethod",
"TrackingNumber",
"ShipmentCost",
"OrderNote",
"SKU",
"Title",
"QtyOrdered",
"QtyShipped",
"Price",
"ShippingPrice",
"GiftWrapPrice",
"ItemTax",
"ShippingTax",
"GiftWrapTax",
"ShippingDiscount",
"PromotionDiscount",
"ConditionNote"
];
}

function getWeekdays(days) {
    return [
        new ItemModel({ Id: "Monday", Name: "Monday", IsChecked: days.indexOf("Monday") >-1, Sort: "" }),
        new ItemModel({ Id: "Tuesday", Name: "Tuesday", IsChecked: days.indexOf("Tuesday") >-1, Sort: "" }),
        new ItemModel({ Id: "Wednesday", Name: "Wednesday", IsChecked: days.indexOf("Wednesday") >-1, Sort: "" }),
        new ItemModel({ Id: "Thursday", Name: "Thursday", IsChecked: days.indexOf("Thursday") >-1, Sort: "" }),
        new ItemModel({ Id: "Friday", Name: "Friday", IsChecked: days.indexOf("Friday") >-1, Sort: "" }),
        new ItemModel({ Id: "Saturday", Name: "Saturday", IsChecked: days.indexOf("Saturday") >-1, Sort: "" }),
        new ItemModel({ Id: "Sunday", Name: "Sunday", IsChecked: days.indexOf("Sunday") >-1, Sort: "" }),
    ];
}

function getRecurrences(occurrence, includeHourly) {
    var occurrences = [];
    if (includeHourly) {
        occurrences.push(new ItemModel({ Id: "Hourly", Name: "Hourly", IsChecked: occurrence == "Hourly", Sort: "" }));
        occurrences.push(new ItemModel({ Id: "Daily", Name: "Daily", IsChecked: occurrence == "Daily", Sort: "" }));
        occurrences.push(new ItemModel({ Id: "Custom", Name: "Custom", IsChecked: occurrence == "Custom", Sort: "" }));
    } else {
        occurrences.push(new ItemModel({ Id: "Hourly", Name: "Hourly", IsChecked: occurrence == "Hourly", Sort: "" }));
        occurrences.push(new ItemModel({ Id: "Daily", Name: "Daily", IsChecked: occurrence == "Daily", Sort: "" }));
        occurrences.push(new ItemModel({ Id: "Weekly", Name: "Weekly", IsChecked: occurrence == "Weekly", Sort: "" }));
        occurrences.push(new ItemModel({ Id: "Custom", Name: "Custom", IsChecked: occurrence == "Custom", Sort: "" }));
    }

    return occurrences;
}

function getMarketplaces(marketplaces) {
    return [
        new ItemModel({ Id: "Amazon", Name: "Amazon US", IsChecked: marketplaces.indexOf("Amazon") > -1, Sort: "" }),
        new ItemModel({ Id: "eBay", Name: "eBay", IsChecked: marketplaces.indexOf("eBay") > -1, Sort: "" }),
        new ItemModel({ Id: "BigCommerce", Name: "BigCommerce", IsChecked: marketplaces.indexOf("BigCommerce") > -1, Sort: "" }),
        new ItemModel({ Id: "Buy.com", Name: "Buy.com", IsChecked: marketplaces.indexOf("Buy.com") > -1, Sort: "" }),
    ];
}