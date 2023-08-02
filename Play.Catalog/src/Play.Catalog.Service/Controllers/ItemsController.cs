using System.Reflection.Metadata.Ecma335;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;

using System.Threading.Tasks;
using Play.Catalog.Service.Entities;
using Play.Common;
using MassTransit;
using Play.Catalog.Contracts;

namespace Play.Catalog.Service.Controller
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> itemRepository;
        private readonly IPublishEndpoint publishEndPoint;

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndPoint)
        {
            this.itemRepository = itemsRepository;
            this.publishEndPoint = publishEndPoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {

            var items = (await itemRepository.GetAllAsync())
                        .Select(item => item.AsDto());


            return Ok(items);

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {

            var item = await itemRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow

            };
            await itemRepository.CreateAsync(item);
            await publishEndPoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingitem = await itemRepository.GetAsync(id);

            if (existingitem == null)
            {
                return NotFound();
            }


            existingitem.Name = updateItemDto.Name;
            existingitem.Description = updateItemDto.Description;
            existingitem.Price = updateItemDto.Price;

            await itemRepository.UpdateAsync(existingitem);
            await publishEndPoint.Publish(new CatalogItemUpdated(existingitem.Id, existingitem.Name, existingitem.Description));
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var existingitem = await itemRepository.GetAsync(id);
            if (existingitem == null)
            {
                return NotFound();
            }

            await itemRepository.RemoveAsync(existingitem.Id);
            await publishEndPoint.Publish(new CatalogItemDeleted(existingitem.Id));
            return NoContent();
        }

    }

}