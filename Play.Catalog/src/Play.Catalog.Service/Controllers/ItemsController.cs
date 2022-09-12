using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{

    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;

        // private static int requestCounter;
        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {

            // Testing Awaiting return Internal Server!!! All Comment in this Page

/*             requestCounter++;
            Console.WriteLine($"request {requestCounter}: Starting..");

            if(requestCounter <= 2){
                Console.WriteLine($"request {requestCounter}: Delaying..");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            if(requestCounter <= 4){
                Console.WriteLine($"request {requestCounter}: 500(Internal Server Error).");
                return StatusCode(500);
            } */

            var items = (await itemsRepository.GetAllAsync())
            .Select(item => item.AsDto());

            // Console.WriteLine($"request {requestCounter}: 200 (Ok).");
            return Ok(items);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

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
            await itemsRepository.CreateAsync(item);

            await publishEndpoint.Publish( new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(Guid id, Updateitemdto updateitemdto)
        {
            var existingItem = await itemsRepository.GetAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }
            existingItem.Name = updateitemdto.Name;
            existingItem.Description = updateitemdto.Description;
            existingItem.Price = updateitemdto.Price;

            await itemsRepository.UpdateAsync(existingItem);

            await publishEndpoint.Publish( new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(item.Id);

            await publishEndpoint.Publish( new CatalogItemDeleted(id));

            return NoContent();
        }
    }


}