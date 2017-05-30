
function ViewModel() {
    var self = this;
    
    var todayDate = new Date();
    todayDate.setDate(todayDate.getDate() - 7);
    self.vendors = ko.observableArray();
    self.filterDateFrom = ko.observable(todayDate);
    self.filterDateTo = ko.observable(new Date());
    self.filterDateBy = ko.observable();
    self.selectedVendor = ko.observable();
    self.orderIds = ko.observable();

    self.loadData = function () {
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
}

function ItemModel(item) {
    var self = this;

    self.Id = ko.observable(item.Id);
    self.Name = ko.observable(item.Name);
}