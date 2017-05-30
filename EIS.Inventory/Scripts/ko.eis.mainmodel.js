// register the job type components
ko.components.register("ProductFileUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
	},
	template: '<a href="#"> \
			     <h3>Processing Product File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("AmazonGetInfo", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Amazon Get Info  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("BulkDeleteProduct", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Bulk Delete Products  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-red progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("KitFileUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Kit File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("ShadowFileUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Shadow File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("BlacklistedSkuUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Blacklisted SKU File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("BulkeBaySuggestedCategories", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing eBay Get Suggested Categories  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("VendorProductFileUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Vendor Product File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("BulkDeleteVendorProduct", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Bulk Delete Vendor Products  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-red progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("ShippingRateFileUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Shipping Rate File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("VendorInventoryFileUpload", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing Vendor Inventory File Upload  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("eBayProductsReListing", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing eBay Products ReListing  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});
ko.components.register("eBayProductsEndItem", {
    viewModel: function (params) {
        this.percentage = ko.observable(params.Percentage + "%");
        this.currentValue = ko.observable(params.Percentage);
    },
    template: '<a href="#"> \
			     <h3>Processing eBay Products EndItem  <small class="pull-right" data-bind="text: percentage"></small></h3> \
			     <div class="progress xs active"> \
				    <div class="progress-bar progress-bar-yellow progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-bind="style: { width: percentage }, attr: { \'aria-valuenow\' : currentValue}"> \
					    <span class="sr-only" data-bind="text: currentValue() + \'% Complete\'"></span> \
				    </div> \
			    </div>'
});


function EisMainViewModel() {
    var self = this;

    self.systemJobs = ko.observableArray();
}