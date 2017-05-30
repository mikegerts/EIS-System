using System;
using EIS.Inventory.DAL.Database;

namespace EIS.Inventory.Core.Profits
{
    public interface IProfitProcessor
    {
        void Apply(object entity, int vendorId);
    }


    public class ProfitProcessor : IProfitProcessor
    {
        private readonly EisInventoryContext _context;

        public ProfitProcessor()
        {
            _context = new EisInventoryContext();
        }

        public void Apply(object entity, int vendorId)
        {
            throw new NotImplementedException();
        }
    }
}
