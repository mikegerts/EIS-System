using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.Shippings
{
    public interface ILabelProvider
    {
        /// <summary>
        /// Get the name of the label provider
        /// </summary>
        string ProviderName { get;}
    }
}
