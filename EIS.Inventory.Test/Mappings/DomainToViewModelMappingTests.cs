using AutoMapper;
using System.Linq;
using Xunit;
using EIS.Inventory.Core.Mappings;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using Ploeh.AutoFixture;

namespace EIS.Inventory.Test.Mappings
{
    public class DomainToViewModelMappingTests
    {
        public DomainToViewModelMappingTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
        }

        [Fact]
        public void Should_Mapped_VendorProdut_To_VendorProdutListDto()
        {
            // arrange
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var domain = fixture.Create<vendorproduct>();

            // act
            var model = Mapper.Map<VendorProductListDto>(domain);

            // assert
            Assert.Equal(domain.EisSupplierSKU, model.EisSupplierSKU);
            Assert.Equal(domain.SupplierSKU, model.SupplierSKU);
            Assert.Equal(domain.VendorName, model.VendorName);
            Assert.Equal(domain.CompanyName, model.CompanyName);
            Assert.Equal(domain.Name, model.Name);
            Assert.Equal(domain.Quantity, model.Quantity);
            Assert.Equal(domain.MinPack, model.MinPack);
        }
    }
}
