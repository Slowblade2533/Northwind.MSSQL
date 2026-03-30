using System.ComponentModel.DataAnnotations;

namespace Northwind.MSSQL.DTO.Customers;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "CustomerID is required")]
    [StringLength(5, MinimumLength = 5, ErrorMessage = "CustomerID must be exactly 5 characters")]
    public string CustomerId { get; set; } = null!;

    [Required(ErrorMessage = "Company name is required")]
    [StringLength(40)]
    public string CompanyName { get; set; } = null!;

    [StringLength(30)]
    public string? ContactName { get; set; }

    [StringLength(30)]
    public string? ContactTitle { get; set; }

    [StringLength(15)]
    public string? City { get; set; }

    [StringLength(15)]
    public string? Country { get; set; }

    [StringLength(24)]
    public string? Phone { get; set; }
}
