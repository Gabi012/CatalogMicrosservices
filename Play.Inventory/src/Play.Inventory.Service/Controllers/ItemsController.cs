using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> InvetoryitemsRepository;
        private readonly IRepository<CatalogItem> catalogItemRepository;

        public ItemsController(IRepository<InventoryItem> InvetoryitemsRepository, IRepository<CatalogItem> catalogItemRepository)
        {
            this.InvetoryitemsRepository = InvetoryitemsRepository;
            this.catalogItemRepository = catalogItemRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvetoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            

            var InventoryItemsEntities = (await InvetoryitemsRepository.GetAllAsync(item => item.UserId == userId));
            var itemIds = InventoryItemsEntities.Select(item =>item.CatalogItemId);
            var catalogItemEtities = await catalogItemRepository.GetAllAsync(item => itemIds.Contains(item.Id));
            
            var inventoryItemDtos = InventoryItemsEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEtities.SingleOrDefault(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });



            return Ok(inventoryItemDtos);
        }
        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantsItemsDto)
        {
            var inventoryItem = await InvetoryitemsRepository.GetAsync(item => item.UserId == grantsItemsDto.UserId
                                                            && item.CatalogItemId == grantsItemsDto.CatalogItemId);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantsItemsDto.CatalogItemId,
                    UserId = grantsItemsDto.UserId,
                    Quantity = grantsItemsDto.Quantity,
                    AcquaredDate = DateTimeOffset.UtcNow

                };
                await InvetoryitemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantsItemsDto.Quantity;
                await InvetoryitemsRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
       
    }


}