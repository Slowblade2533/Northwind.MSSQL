using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Northwind.MSSQL.Data;
using Northwind.MSSQL.DTO.Common;
using Northwind.MSSQL.DTO.Customers;
using Northwind.MSSQL.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Northwind.MSSQL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly NorthwindContext _context;
    public CustomersController(NorthwindContext context) => _context = context;

    // GET: api/<CustomersController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDTO>>> GetCustomers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100); // บังคับ max 100 ต่อหน้า

        var query = _context.Customers.AsNoTracking();
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.CustomerId) // ต้องมี Order เสมอเมื่อใช้ Skip/Take
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerDTO
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                ContactName = c.ContactName,
                ContactTitle = c.ContactTitle,
                City = c.City,
                Country = c.Country,
                Phone = c.Phone
            })
            .ToListAsync();

        return Ok(new PagedResult<CustomerDTO>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            HasNextPage = (page * pageSize) < totalCount
        });
    }

    // GET api/<CustomersController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomer(string id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == id)
            .Select(c => new CustomerDTO
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                ContactName = c.ContactName,
                ContactTitle = c.ContactTitle,
                City = c.City,
                Country = c.Country,
                Phone = c.Phone
            })
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Customer Not Found",
                Detail = $"Customer with ID {id} was not found."
            });
        }

        return customer;
    }

    // POST api/<CustomersController>
    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> PostCustomer(CreateCustomerRequest request)
    {
        if (await CustomerExistsAsync(request.CustomerId))
            return Conflict(new ProblemDetails { Title = "Customer already exists" });

        var customer = new Customer
        {
            CustomerId = request.CustomerId,
            CompanyName = request.CompanyName,
            ContactName = request.ContactName,
            ContactTitle = request.ContactTitle,
            City = request.City,
            Country = request.Country,
            Phone = request.Phone
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Map Entity → Response DTO
        var dto = new CustomerDTO
        {
            CustomerId = customer.CustomerId,
            CompanyName = customer.CompanyName,
            ContactName = customer.ContactName,
            ContactTitle = customer.ContactTitle,
            City = customer.City,
            Country = customer.Country,
            Phone = customer.Phone
        };

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, dto);
    }

    // PUT api/<CustomersController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCustomer(string id, UpdateCustomerRequest request)
    {
        var updatedCount = await _context.Customers
            .Where(c => c.CustomerId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.CompanyName, request.CompanyName)
                .SetProperty(c => c.ContactName, request.ContactName)
                .SetProperty(c => c.ContactTitle, request.ContactTitle)
                .SetProperty(c => c.City, request.City)
                .SetProperty(c => c.Country, request.Country)
                .SetProperty(c => c.Phone, request.Phone));

        if (updatedCount == 0) return NotFound();
        return NoContent();
    }

    // DELETE api/<CustomersController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var hasOrders = await _context.Orders
            .AnyAsync(o => o.CustomerId == id);

        if (hasOrders)
            return Conflict(new ProblemDetails
            {
                Title = "Cannot Delete Customer",
                Detail = $"Customer {id} has existing orders and cannot be deleted.",
                Status = 409
            });

        var deletedCount = await _context.Customers
            .Where(c => c.CustomerId == id)
            .ExecuteDeleteAsync();

        if (deletedCount == 0) return NotFound();

        return NoContent();
    }

    private async Task<bool> CustomerExistsAsync(string id) 
        => await _context.Customers.AnyAsync(e => e.CustomerId == id);
}
