using System.ComponentModel;

namespace EIS.Inventory.Shared.Models
{
    public enum InventoryDependency
    {
        [Description("All Components")]
        AllComponensts = 0,

        [Description("Main Component")]
        MainComponent = 1,

        Independent = 2
    }
}
