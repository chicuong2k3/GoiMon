using HotChocolate.Types;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.GraphQL.Types;

public class ProductComboType : ObjectType<ProductCombo>
{
    protected override void Configure(IObjectTypeDescriptor<ProductCombo> descriptor)
    {
        descriptor.Name("ProductCombo");
        descriptor.Description("A product bundle composed of multiple products.");

        descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Name).Type<NonNullType<StringType>>().Description("Combo display name.");
        descriptor.Field(t => t.PriceCents).Type<NonNullType<IntType>>().Description("Combo price in cents.");
        descriptor.Field(t => t.Items)
            .Type<ListType<NonNullType<ProductComboItemType>>>()
            .Description("Items included in the combo.");
    }
}
