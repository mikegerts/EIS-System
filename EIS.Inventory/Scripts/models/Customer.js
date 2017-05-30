function ViewModel() {
    var self = this;

    self.type = ko.observable("");
    self.modalTitle = ko.observable();
    self.message = ko.observable();

    self.companies = ko.observableArray();
    self.accounttypes = ko.observableArray();

    self.selectedAccountType = ko.observable();
    self.selectedCompany = ko.observable();

    self.images = ko.observableArray();
    self.image = ko.observable();

    self.selected = ko.observable(null);
    self.wholeSaleSelected = ko.observable(null);

    self.init = ko.observable(1);
    self.wholeSaleInit = ko.observable(1);
    self.WholeSaleSearchSku = ko.observable("");
    self.selectedTaskType = ko.observable();
    self.scheduledTasks = ko.observableArray();
    self.WholeSalePriceHistories = ko.observableArray();
    self.scheduledTask = ko.observable();
    self.pagedExportedFile = ko.observable();
    self.exportToOptions = ko.observableArray(["None", "FTP"]);
    self.yesNos = ko.observableArray([{ Id: 1, Name: "Yes" }, { Id: 0, Name: "No" }]);

    self.loadData = function () {
        // load all the vendors for filtering product
        $.ajax({
            url: GET_ACCOUNTTYPES_URL,
            success: function (results) {
                self.accounttypes(ko.utils.arrayMap(results, function (item) {
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
    self.applyFilters = function () {
        showSpinner();
        $("#filterForm").submit();
        setTimeout("hideSpinner(); $('#FilterDialog').modal('hide');", 1400);

        // set the values in the show entries form
        $("#showEntryForm #AccountTypeId").val(viewModel.selectedAccountType());
        $("#showEntryForm #companyId").val(viewModel.selectedCompany());
        $("#showEntryForm #CustomerNumber").val($("#filterForm #CustomerNumber").val());
        $("#showEntryForm #CompanyName").val($("#filterForm #CompanyName").val());
        $("#showEntryForm #CustomerName").val($("#filterForm #CustomerName").val());
        $("#showEntryForm #EmailAddress").val($("#filterForm #EmailAddress").val());
        $("#showEntryForm #SearchString").val($("#filterForm #SearchString").val());

    }
    self.resetFilters = function () {
        self.selectedAccountType(null);
        self.selectedCompany(null);
        $("#filterForm #CustomerNumber").val("");
        $("#filterForm #SearchString").val("");
        $("#filterForm #CompanyName").val("");
        $("#filterForm #CustomerName").val("");
        $("#filterForm #EmailAddress").val("");

        $("#filterForm #deletefilter").hide();
        $("#filterForm #filterName").val("");
    }

    self.loadImages = function (customerId) {
        // load the product images
        $.ajax({
            url: GET_CUSTOMER_FILES_URL,
            data: { customerId: customerId },
            success: function (results) {
                self.images(results);
            }
        });
    }


    self.loadScheduleTasks = function () {
        // load all scheduled tasks
        $.ajax({
            url: GET_SCHEDULED_TASKS_URL,
            success: function (results) {
                self.scheduledTasks(ko.utils.arrayMap(results, function (item) {
                    return new ScheduledTaskListModel(item);
                }));
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
                    url: DELETE_CUSTOMER_FILE_URL,
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
    function ItemModel(item) {
        var self = this;

        ko.mapping.fromJS(item, {}, self);
        self.IsChecked = ko.observable(item.IsChecked || false);

        self.selectAll = function (item, event) {
            $(event.target).select();
        }
    }
    //Get href value og element
    self.getHref = function () {

        var target;
        var element = event.target.hash;
        target = element.substr(1);
        return target;
    };

    //Show Tabpanel
    self.showBlock = function () {

        var target = self.getHref();
        self.selected(target);
        self.init(2);
    };

    //Get href value og element
    self.getWholeSaleHref = function () {

        var target;
        var element = event.target.hash;
        target = element.substr(1);
        return target;
    };

    //Show Tabpanel
    self.showWholeSaleBlock = function () {

        var target = self.getWholeSaleHref();
        self.wholeSaleSelected(target);
        self.wholeSaleInit(2);
    };
    self.clearSearch = function () {
        self.WholeSaleSearchSku("");
        self.WholeSalePriceHistories([]);
    };
    self.searchWholeSaleSku = function () {
        
        var sku = self.WholeSaleSearchSku();

        $.ajax({
            url: GET_CUSTOMER_WHOLESALEPRICE_HISTORY,
            data: { WholeSaleSearchSku: sku },
            success: function (results) {
                
                self.WholeSalePriceHistories(ko.utils.arrayMap(results, function (item) {
                    return new WholeSaleHistoryListModel(item);
                }));
            }
        });

    }
    self.loadModel = function (modelId) {
        
        $("#loadingModal").show();

        self.exportToOptions(["None", "FTP", "Email"]);
        self.modalTitle((modelId == -1 ? "Add New" : "Edit") + "  Custom Export Sku Calculated Task");
        self.customFieldOptions = ko.observableArray(getExportFileFieldsArr());
        self.fileTypes = ko.observableArray([".csv"]);

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


    self.saveScheduledTask = function (task, event) {
        
        // set the selected currence
        task.Recurrence(task.recurrenceTemplate());

        var files = $("#fileuploadtaskField").get(0).files;
        var formData = new FormData();

        if (files.length == 0) {
            if (task.Id() == -1) {
                viewModel.type("danger");
                viewModel.message("Select appropriate file to save schedule.");
                return;
            }
        }
        else {
            var acceptFileTypes = ["csv"];
            var addedFileExtension = files[0].name.split(".").pop().toLowerCase();

            var isValidFile = $.inArray(addedFileExtension, acceptFileTypes) > -1;
            if (!isValidFile) {
                viewModel.type("danger");
                viewModel.message("Invalid file selection only csv allowed.");
                return;
            }

            formData.append("attachment", files[0]);
            formData.append("customerId", task.CustomerId());
            formData.append("Id", task.Id());
            formData.append("ImportFileName", task.ImportFileName());
        }

        
        if (!isValidateForm()) {
            return;
        }

        showSpinner();
        $(event.target).addClass("disabled");


        task.CustomFields.removeAll();
        task.FileHeaders.removeAll();

        // iterate to the customFields and set to the cleared arrays
        $.each(task.customFields(), function (index, item) {
            task.CustomFields.push(item.CustomField());
            task.FileHeaders.push(item.FileHeader());
        })
        
        // mapped the ko object to plain JS object
        var taskData = ko.mapping.toJS(task);

        
        if (files.length != 0 && task.Id() == -1) {
            SaveTaskFile(formData, taskData);
        }
        else if (files.length > 0 && task.Id() != -1) {
            SaveTaskFile(formData, taskData);
        }
        else if (files.length == 0 && task.Id() != -1) {
            SaveTask(taskData);
        }
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
                        $("#msgStatus").attr("class", "alert-success");
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

    self.checkFtpConnection = function (task, event) {
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
        CustomerId:$("#CustomerId").val(),
        ImportFileName: "",
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

function ScheduledTaskModel(task) {
    var self = this;

    ko.mapping.fromJS(task, {}, self);
    self.StartDate = ko.observable(convertDate(task.StartDate || new Date()));
    self.LastExecutedOn = ko.observable(convertDate(task.LastExecutedOn || new Date()));
    self.weekdays = ko.observableArray(getWeekdays(task.Days || []));
    self.recurrenceTemplate = ko.observable(task.Recurrence);


    self.recurrences = ko.observableArray(getRecurrences((task.Recurrence || []), false));
    self.itemFields = ko.observableArray(getExportFileFieldsArr(task.OrderFields || []));



    var customFieldsLength = self.CustomFields().length;
    self.customFields = ko.observableArray();
    if (customFieldsLength > 0) {
        for (var i = 0; i < customFieldsLength; i++) {
            self.customFields.push(new ItemModel({ FileHeader: self.FileHeaders()[i], CustomField: self.CustomFields()[i] }));
        }
    }
    else {

        var defaultHeaders = ["EisSKU"];
        for (var i = 0; i < 1; i++) {
            self.customFields.push(new ItemModel({ FileHeader: defaultHeaders[i], CustomField: defaultHeaders[i] }));
        }
        self.FileName = ko.observableArray(null);
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

function getWeekdays(days) {
    return [
        new ItemModel({ Id: "Monday", Name: "Monday", IsChecked: days.indexOf("Monday") > -1, Sort: "" }),
        new ItemModel({ Id: "Tuesday", Name: "Tuesday", IsChecked: days.indexOf("Tuesday") > -1, Sort: "" }),
        new ItemModel({ Id: "Wednesday", Name: "Wednesday", IsChecked: days.indexOf("Wednesday") > -1, Sort: "" }),
        new ItemModel({ Id: "Thursday", Name: "Thursday", IsChecked: days.indexOf("Thursday") > -1, Sort: "" }),
        new ItemModel({ Id: "Friday", Name: "Friday", IsChecked: days.indexOf("Friday") > -1, Sort: "" }),
        new ItemModel({ Id: "Saturday", Name: "Saturday", IsChecked: days.indexOf("Saturday") > -1, Sort: "" }),
        new ItemModel({ Id: "Sunday", Name: "Sunday", IsChecked: days.indexOf("Sunday") > -1, Sort: "" }),
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

function ItemModel(item) {
    var self = this;

    ko.mapping.fromJS(item, {}, self);

    self.selectAll = function (item, event) {
        $(event.target).select();
    }
}


function getExportFileFieldsArr(orderFields) {
    return [
 "EisSKU"
    ];
}

function SaveTaskFile(formData, taskData)
{
    var fileName = "";
    $.ajax({
        type: "POST",
        url: SAVE_SCHEDULED_FILE_TASK_URL,
        processData: false,
        data: formData,
        contentType: false,
        success: function (result) {
            
            if (!result.isUploaded) {
                viewModel.type("danger");
                viewModel.message("Error occured in trying to save the Scheduled Task! <br/> " + result.message)
                $(event.target).removeClass("disabled");
                return;
            }
            taskData.ImportFileName = result.filePath;
            SaveTask(taskData);

        },
        error: function (result) {
            
            var text = result.responseText;
            viewModel.type("danger");
            viewModel.message(text);
            $(event.target).removeClass("disabled");
        },
        complete: function () {
            hideSpinner();
            
        }
    });

}

function SaveTask(taskData)
{
    $.ajax({
        type: "POST",
        url: SAVE_SCHEDULED_TASK_URL,
        processData: false,
        data: JSON.stringify(taskData),
        contentType: "application/json;",
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
            
            var text = result.responseText;
            viewModel.type("danger");
            viewModel.message(text);
            $(event.target).removeClass("disabled");
        },
        complete: function () {
            hideSpinner();
        }
    });
}
function ScheduledTaskListModel(task) {
    var self = this;

    ko.mapping.fromJS(task, {}, self);
}

function WholeSaleHistoryListModel(item) {
    
    var self = this;

    ko.mapping.fromJS(item, {}, self);
}

function downloadUploadFileTemplate() {
    document.getElementById("download_frame").src = GET_FILE_TEMPLATE_URL + "?fileTemplateName=Customer_WholeSalePrice_EisSkuTemplate.csv";
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