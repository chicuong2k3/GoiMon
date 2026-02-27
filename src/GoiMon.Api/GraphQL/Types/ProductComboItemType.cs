using HotChocolate.Types;
using GoiMon.Api.Domain.Entities;
using GoiMon.Api.DataLoaders;
using System.Threading;
using System.Threading.Tasks;


namespace GoiMon.Api.Legacy.GraphQL.Types;

public class ProductComboItemType : ObjectType<ProductComboItem>
{
    protected override void Configure(IObjectTypeDescriptor<ProductComboItem> descriptor)
    {
        descriptor.Name("ProductComboItem");
        descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.ProductId).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Qty).Type<NonNullType<IntType>>();
        descriptor.Field<Resolvers>(t => t.GetProductAsync(default!, default!, default))
            .Name("product")
            .Description("Resolved product details for this combo item.");
    }

    private class Resolvers
    {
        public async Task<GoiMon.Api.Domain.Entities.Product?> GetProductAsync(
            [Parent] ProductComboItem item,
            ProductByIdDataLoader loader,
            CancellationToken ct)
        {
            return await loader.LoadAsync(item.ProductId, ct);
        }
    }
}
