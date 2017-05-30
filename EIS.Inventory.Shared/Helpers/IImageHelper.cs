
namespace EIS.Inventory.Shared.Helpers
{
    public interface IImageHelper
    {
        /// <summary>
        /// Save the eisProduct image to a file
        /// </summary>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="url">The URL of the image</param>
        /// <returns>Returns the image file name</returns>
        string SaveProductImage(string eisSKU, string url);

        /// <summary>
        /// Save the product image into a file
        /// </summary>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="imageData">The byte stream of the image to save</param>
        /// <returns>Returns the image's file name</returns>
        string SaveProductImage(string eisSKU, byte[] imageData);

        /// <summary>
        /// Delete the image with the specified file name
        /// </summary>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="fileName">The image's file name</param>
        void RemoveProductImage(string eisSKU, string fileName);

        /// <summary>
        /// Get the eisProduct image with the specified id and file name
        /// </summary>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="fileName">The image's file name</param>
        /// <returns></returns>
        string GetProductImageUri(string eisSKU, string fileName);

        /// <summary>
        /// Save the eisVendor image to a file
        /// </summary>
        /// <param name="eisSupplierSKU">The id of product or the EIS Supplier SKU</param>
        /// <param name="url">The URL of the image</param>
        /// <returns>Returns the image file name</returns>
        string SaveVendorProductImage(string eisSupplierSKU, string url);

        /// <summary>
        /// Save the eisVendor image into a file
        /// </summary>
        /// <param name="eisSupplierSKU">The id of product or the EIS Supplier SKU</param>
        /// <param name="imageData">The byte stream of the image to save</param>
        /// <returns>Returns the image's file name</returns>
        string SaveVendorProductImage(string eisSupplierSKU, byte[] imageData);

        /// <summary>
        /// Delete the image with the specified file name
        /// </summary>
        /// <param name="eisSupplierSKU">The id of product or the EIS Supplier SKU</param>
        /// <param name="fileName">The image's file name</param>
        void RemoveVendorProductImage(string eisSupplierSKU, string fileName);

        /// <summary>
        /// Get the eisVendor image with the specified id and file name
        /// </summary>
        /// <param name="eisSupplierSKU">The id of product or the EIS Supplier SKU</param>
        /// <param name="fileName">The image's file name</param>
        /// <returns></returns>
        string GetVendorProductImageUri(string eisSupplierSKU, string fileName);

        /// <summary>
        /// Save the customer image to a file
        /// </summary>
        /// <param name="customerId">The id of customerId</param>
        /// <param name="url">The URL of the image</param>
        /// <returns>Returns the image file name</returns>
        string SaveCustomerImage(int customerId, string url);

        /// <summary>
        /// Save the customer into a file
        /// </summary>
        /// <param name="customerId">The id of customerId</param>
        /// <param name="imageData">The byte stream of the image to save</param>
        /// <returns>Returns the image's file name</returns>
        string SaveCustomerImage(int customerId, byte[] imageData);

        /// <summary>
        /// Delete the image with the specified file name
        /// </summary>
        /// <param name="customerId">The id of customerId</param>
        /// <param name="fileName">The image's file name</param>
        void RemoveCustomerImage(int customerId, string fileName);

        /// <summary>
        /// Get the customer image with the specified id and file name
        /// </summary>
        /// <param name="customerId">The id of customerId</param>
        /// <param name="fileName">The image's file name</param>
        /// <returns></returns>
        string GetCustomerImageUri(int customerId, string fileName);
    }
}
