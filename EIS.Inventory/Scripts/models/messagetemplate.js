
function configureeBayDescriptionCKEditor(messageTypeId) {
    CKEDITOR.config.height = "250px";
    CKEDITOR.config.allowedContent = true;
    initializeCKEditorPlaceHolders(messageTypeId);
}

function initializeCKEditorPlaceHolders(messageTypeId) {
    var placeHolders = [];

    if (messageTypeId == 0) /*** eBayDesription **/ {
        placeHolders = getProducteBayPlaceHolders();
    } else if (messageTypeId == 1) /*** CustomExportOrder **/ {

    } else if (messageTypeId == 2) /*** CustomExportProduct **/ {

    } else if (messageTypeId == 3) /*** GeneratePO **/ {
        placeHolders = getGeneratePOPlaceHolders();
    }

    console.log("messagetype " + messageTypeId) 
    CKEDITOR.config.strinsert_strings = placeHolders;
}

function getProducteBayPlaceHolders() {
    var placeHolders = [];
    
    placeHolders.push(["[Product.EisSKU]", "Product.EisSKU"]);
    placeHolders.push(["[Product.Name]", "Product.Name"]);
    placeHolders.push(["[Product.Description]", "Product.Description"]);
    placeHolders.push(["[Product.ImageURL1]", "Product.ImageURL1"]);
    placeHolders.push(["[Product.ImageURL2]", "Product.ImageURL2"]);
    placeHolders.push(["[Product.ImageURL3]", "Product.ImageURL3"]);
    placeHolders.push(["[Product.ImageURL4]", "Product.ImageURL4"]);
    placeHolders.push(["[Product.ImageURL5]", "Product.ImageURL5"]);
    placeHolders.push(["[ProducteBay.ItemId]", "ProducteBay.ItemId"]);    
    placeHolders.push(["[ProducteBay.Title]", "ProducteBay.Title"]);
    placeHolders.push(["[ProducteBay.SubTitle]", "ProducteBay.SubTitle"]);
    placeHolders.push(["[ProducteBay.Description]", "ProducteBay.Description"]);
    placeHolders.push(["[Product.FactorQty]", "Product.FactorQty"]);

    return placeHolders;
}

function getGeneratePOPlaceHolders() {
    var placeHolders = [];

    placeHolders.push(["[Vendor.Name]", "Vendor.Name"]);
    placeHolders.push(["[Vendor.VendorAddress]", "Vendor.VendorAddress"]);
    placeHolders.push(["[Vendor.SuiteApartment]", "Vendor.SuiteApartment"]);
    placeHolders.push(["[Vendor.City]", "Vendor.City"]);
    placeHolders.push(["[Vendor.ZipCode]", "Vendor.ZipCode"]);
    placeHolders.push(["[Vendor.ContactPerson]", "Vendor.ContactPerson"]);
    placeHolders.push(["[Vendor.Email]", "Vendor.Email"]);
    placeHolders.push(["[Vendor.PhoneNumber]", "Vendor.PhoneNumber"]);
    placeHolders.push(["[Vendor.Website]", "Vendor.Website"]);

    placeHolders.push(["[Vendor.Title]", "ProducteBay.Title"]);
    placeHolders.push(["[ProducteBay.SubTitle]", "ProducteBay.SubTitle"]);
    placeHolders.push(["[ProducteBay.Description]", "ProducteBay.Description"]);
    placeHolders.push(["[Product.FactorQty]", "Product.FactorQty"]);

    return placeHolders;
}

function messageTypeChanged(source) {
    var messageTypeId = source.value;
    initializeCKEditorPlaceHolders(messageTypeId);
}