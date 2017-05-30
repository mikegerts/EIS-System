
function ViewModel() {
    var self = this;

    self.vendors = ko.observableArray();
    self.downloadProducts = ko.observable({ VendorIds :  -1 });

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

    self.vendorChanged = function (vendor, event) {
        console.log(vendor);
    }
}

function ItemModel(item) {
    var self = this;

    self.Id = ko.observable(item.Id);
    self.Name = ko.observable(item.Name);
}