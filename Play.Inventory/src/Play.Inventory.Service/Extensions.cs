using Play.Inventory.Service.Entities;



namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InvetoryItemDto AsDto(this InventoryItem item, string Name, string Description)
        {
            return new InvetoryItemDto(item.CatalogItemId, Name, Description, item.Quantity, item.AcquaredDate);
        }
    }
}