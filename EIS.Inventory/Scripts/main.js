// First, checks if it isn't implemented yet.
if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

function isValidateForm() {
    var isValid = true;

    // find all elements in the form which has 'required' attribute
    $("#edit-form").find("input[required],select[required]").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).val()) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    // find all div elements which require values
    $("#edit-form").find("div[required]").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).attr("data-value")) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    return isValid;
}


function validateForm(selector) {
    var isValid = true;

    // find all elements in the form which has 'required' attribute
    $(selector).find("input[required],select[required]").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).val()) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    // find all div elements which require values
    $("#edit-form").find("div[required]").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).attr("data-value")) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    return isValid;
}



function isValidFormData(formId) {
    var isValid = true;

    // find all elements in the form which has 'required' attribute
    $("#" + formId).find("input[required],select[required][multiple!='multiple']").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).val()) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    // find all elements in the form which has 'required' attribute
    $("#" + formId).find("select[required][multiple]").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).attr("data-value")) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    // find all div elements which require values
    $("#" + formId).find("div[required]").each(function (index, elem) {
        // if there's no value, add class 'has-error'
        if (!$(elem).attr("data-value")) {
            isValid = false;
            $(elem).parents(".form-group").addClass("has-error");
        }
    });

    return isValid;
}

function displayMessage(message, msgClass) {
    if (msgClass === undefined)
        msgClass = "success";

    $("#msgStatus").removeClass("alert-success");
    $("#msgStatus").removeClass("alert-info");
    $("#msgStatus").removeClass("alert-warning");
    $("#msgStatus").removeClass("alert-danger");
    $("#msgStatus").addClass("alert-" + msgClass);
    $("#msgStatus").fadeIn("fast");
    $("#msgStatus").text(message);
}

function fadeOutMessage() {
    setTimeout(function () { $("#msgStatus").fadeOut(); }, 1800);
}

function valueChanged(data, event) {
    $(event.target).parents(".form-group").removeClass("has-error");
}

function rateFieldOnFocus(source) {
    if (source.value == 0)
        source.select();
}

function formatCurrency(value) {
    return "$" + value.toFixed(2);
}

function formatDate(date) {
    var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var dateStr = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();

    return dateStr + " " + months[date.getMonth()] + " " + date.getFullYear();
}

function convertDate(date) {
    
    if (date instanceof Date)
        return date;

    return new Date(parseInt(date.substr(6)));
}

function showSpinner() {
    $(".spinner").show();
}

function hideSpinner() {
    $(".spinner").hide();
}

function showLoadingGif() {
    $("#loadingDiv").show();
}

function hideLoadingGif() {
    $("#loadingDiv").hide();
}

function focusOnSearchFile() {
    $(".searchField").focus().select();
}

function getFormatFileSize(bytes) {
    if (typeof bytes !== 'number') {
        return '';
    }
    if (bytes >= 1000000000) {
        return (bytes / 1000000000).toFixed(2) + ' GB';
    }
    if (bytes >= 1000000) {
        return (bytes / 1000000).toFixed(2) + ' MB';
    }
    return (bytes / 1000).toFixed(2) + ' KB';
}