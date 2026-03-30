using System.ComponentModel.DataAnnotations;

namespace Northwind.MSSQL.DTO.Customers;

public class UpdateCustomerRequest
{
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
