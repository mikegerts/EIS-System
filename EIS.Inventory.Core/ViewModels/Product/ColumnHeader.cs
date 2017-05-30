namespace EIS.Inventory.Core.ViewModels
{
    public class ColumnHeader
    {
        public ColumnHeader(string prefix, string column, bool isUseFriendlyName = false)
        {
            DbColumnName = column;
            FileHeaderName = isUseFriendlyName ? DbColumnName 
                : string.Format("{0}-{1}", getProductKind(prefix), DbColumnName);
        }
        public string FileHeaderName { get; private set; }
        public string DbColumnName { get; set; }

        private string getProductKind(string tableName)
        {
            switch (tableName)
            {
                case "p": return "General";
                case "pa": return "Amazon";
                case "pe": return "eBay";
                case "pbc": return "BigCommerce";
                default: return tableName;
            }
        }
    }
}
