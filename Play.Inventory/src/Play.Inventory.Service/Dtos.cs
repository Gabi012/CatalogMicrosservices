
using System;

namespace Play.Inventory.Service
{

    public record GrantItemsDto(Guid UserId, Guid CatalogItemId, int Quantity);
    public record InvetoryItemDto(Guid CatalogItemId, string Name, string Description ,int Quantity, DateTimeOffset AcquiredDate);
    public record CatalogItemDto(Guid Id, string Name, string Description);
}