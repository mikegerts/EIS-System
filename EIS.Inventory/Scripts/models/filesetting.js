
function ViewModel() {
    var self = this;

    self.columIndexes = ko.observableArray([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30]);
    self.fileTypes = [{ Id: 0, Name: "EXCEL" }, { Id: 1, Name: "CSV" }]
    self.yesNos = ko.observableArray([{ Id: 1, Name: "Yes" }, { Id: 0, Name: "No" }]);
    self.extensions = ko.observableArray([".xls", ".xlsx", ".csv"]);

    self.fileSettings = ko.observableArray();
    self.vendors = ko.observableArray();
    self.fileSetting = ko.observable();
    self.modelId = ko.observable();
    self.templateName = ko.observable();
    
    self.isEdit = ko.computed(function () {
        return self.modelId() != -1;
    })

    self.isDetails = ko.computed(function () {
        return self.templateName() == "detailsFileSetting";
    })

    self.modalTitle = ko.computed(function () {
        if (self.templateName() == "detailsFileSetting")
            return FILE_SETTING_NAME + " Details";
        else
            return (self.isEdit() ? "Edit " + FILE_SETTING_NAME : "Create " + FILE_SETTING_NAME);
    })

    self.fileTypeChanged = function (fileSetting, event) {
        if (fileSetting.FileType() == 0)
            self.extensions([".xls", ".xlsx"]);
        else
            self.extensions([".csv"]);

        valueChanged(fileSetting.FileType(), event);
    }

    self.editFileSetting = function (fileSetting, event) {        
        self.templateName("entryFileSetting");
    }

    self.deleteFileSetting = function (fileSetting, event) {
        $.confirm({
            title: "Delete File Setting",
            text: "Are you sure you want to delete file setting for vendor: <strong> " + fileSetting.VendorName + "</strong>",
            cancel: function () {
                return false;
            },
            confirm: function () {
                $.post(DELETE_FILE_SETTING_URL, { VendorId: fileSetting.VendorId }, function (result) {
                    if (result.Success) {
                        self.fileSettings.remove(fileSetting);
                    }
                });
            },
            confirmButton: "Yes, I am",
            confirmButtonClass: "btn-warning"
        });
    }

    self.saveFileSetting = function (fileSetting, event) {
        if (!isValidateForm())
            return;

        showSpinner();
        $(event.target).addClass("disabled");

        if (!self.isEdit()) {
            fileSetting.VendorName($("#VendorId option:selected").text());
        }
        
        // mapped the ko object to plain JS object
        var fileSettingData = ko.mapping.toJS(fileSetting);
        fileSettingData.modelId = self.modelId();

        $.ajax({
            type: "POST",
            url: SAVE_FILE_SETTING_URL,
            data: JSON.stringify(fileSettingData),
            contentType: "application/json",
            success: function (result) {
                if (result.Error) {
                    $("#error-status").show();
                    $(event.target).removeClass("disabled");
                    return;
                }

                $("#error-status").hide();
                $("#success-status").show();
                setTimeout("$('#FileSettingDialog').modal('hide');", 2000);

                if (self.modelId() == -1) {
                    self.fileSettings.push(fileSetting);
                    return true;
                }

                // find the old file setting in the list
                var oldFileSetting = ko.utils.arrayFirst(self.fileSettings(), function (item) {
                    return item.VendorId == fileSetting.VendorId();
                })

                // replace the old value with the updated one
                self.fileSettings.replace(oldFileSetting, fileSettingData);
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

    self.loadData = function (url) {
        $.ajax({
            url: url,
            success: function (results) {
                self.fileSettings(results);
            }
        });
    }

    self.loadModel = function () {
        if (self.modelId() == -1) {
            self.fileSetting(createFileSettingModel());
            hideLoadingGif();
        }
        else {
            // load the file setting
            $.ajax({
                url: GET_FILE_SETTING_URL + self.modelId(),
                success: function (result) {
                    self.fileSetting(new FileSettingModel(result));
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

    self.loadUnconfiguedVendors = function (url) {
        $.ajax({
            url: url,
            success: function (results) {
                self.vendors(results);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error: " + jqXHR.responseText);
            }
        });
    }

    self.onClosingDialog = function () {
        self.fileSetting(createFileSettingModel());
    }   
}

function FileSettingModel(setting) {
    var self = this;

    ko.mapping.fromJS(setting, {}, self);
    self.NextRunDate = ko.observable(convertDate(setting.NextRunDate || new Date()));

    //self.data = ko.observableArray(ko.utils.arrayMap(settings, function(setting){
    //    return new FileSettingModel(setting);
    //})
}

function createFileSettingModel() {
    return new FileSettingModel({
        VendorId: 0,
        VendorName: "",
        FileName: "",
        Extension: "",
        FilePath: "",
        TransferPath: "",
        ReadTime: new Date(),
        RowAt: "",
        FileType: "",
        NextRunDate: null,
        SKU: "",
        Name: "",
        Description: "",
        ShortDescription: "",
        Category: "",
        UPC: "",
        Cost: "",
        Quantity: "",
        Delimiter: ",",
        FtpServer: "",
        FtpUser: "",
        FtpPassword: "",
        FtpPort: "",
        RemoteFolder: "",
        IsDeleteFile: true
    });
}