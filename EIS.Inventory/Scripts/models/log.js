
function ViewModel() {
    var self = this;

    self.requestReport = ko.observable();
    self.processingReport = ko.observable();
    self.log = ko.observable();

    self.loadModel = function (requestReportId) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_REQUEST_REPORT_URL,
            data: { requestReportId: requestReportId },
            success: function (result) {
                self.processingReport(new ProcessingReportModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.retrieveLog = function (log, event) {
        var logId = $(event.target).parent().data("id");
        $(event.target).parent().parent().find("tr").removeClass("selected");
        $(event.target).parent().addClass("selected");

        $.ajax({
            url: GET_LOG_URL,
            data: { id: logId },
            success: function (result) {
                self.log(result);
            }
        });
    }
}

function ProcessingReportModel(processingReport) {
    var self = this;

    ko.mapping.fromJS(processingReport, {}, self);

    self.errorResult = ko.observable();
    self.warningResult = ko.observable();
    self.messageSuccess = "{0} Products updated sucessfully".format(self.MessagesSuccessful());
    self.messageError = "{0} Products did not update successfully.".format(self.MessagesWithError());
    self.messageWarning = "{0} Products updated with warnings.".format(self.MessagesWithWarning())

    self.loadReportErrors = function (report, event) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_ERROR_RESULTS_URL,
            data: { requestReportId: report.TransactionId() },
            success: function (result) {
                self.errorResult(new ReportResultModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.prevErrorResultPage = function (reportResult, event) {
        var prevPage = reportResult.CurrentPageIndex() - 1;
        if (prevPage == 0)
            return false;

        $("#loadingModal").show();
        $.ajax({
            url: GET_ERROR_RESULTS_URL,
                data: { requestReportId: reportResult.TransactionId(), page: prevPage },
            success: function (result) {
                self.errorResult(new ReportResultModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.nextErrorResultPage = function (reportResult, event) {
        var nextPage = reportResult.CurrentPageIndex() + 1;
        $("#loadingModal").show();
        $.ajax({
            url: GET_ERROR_RESULTS_URL,
            data: { requestReportId: reportResult.TransactionId(), page: nextPage },
            success: function (result) {
                self.errorResult(new ReportResultModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.loadReportWarnings = function (report, event) {
        $("#loadingModal").show();
        $.ajax({
            url: GET_WARNING_RESULTS_URL,
            data: { requestReportId: report.TransactionId() },
            success: function (result) {
                self.warningResult(new ReportResultModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.nextWarningResultPage = function (reportResult, event) {
        var nextPage = reportResult.CurrentPageIndex() + 1;
        $("#loadingModal").show();
        $.ajax({
            url: GET_WARNING_RESULTS_URL,
            data: { requestReportId: reportResult.TransactionId(), page: nextPage },
            success: function (result) {
                self.warningResult(new ReportResultModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    self.prevWarningResultPage = function (reportResult, event) {
        var prevPage = reportResult.CurrentPageIndex() - 1;
        if (prevPage == 0)
            return false;
        $("#loadingModal").show();
        $.ajax({
            url: GET_WARNING_RESULTS_URL,
            data: { requestReportId: reportResult.TransactionId(), page: prevPage },
            success: function (result) {
                self.warningResult(new ReportResultModel(result));
            },
            complete: function () {
                $("#loadingModal").hide();
            }
        });
    }

    // load the error results if there's any
    self.loadReportErrors(self, null);
}

function ReportResultModel(reportResult) {
    var self = this;

    ko.mapping.fromJS(reportResult, {}, self);
    self.pageShowStatus = ko.computed(function () {
        if (self.TotalPageCount() == 0)
            return "No Page";
        return "Page: " + self.CurrentPageIndex() + " of " + self.TotalPageCount();
    })

    self.hasPrevPage = ko.computed(function () {
        return self.CurrentPageIndex() != 1;
    });
    
    self.hasNextPage = ko.computed(function () {
        return self.CurrentPageIndex() < self.TotalPageCount();
    });
}