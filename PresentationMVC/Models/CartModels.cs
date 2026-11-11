using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models;

public sealed class CartItemCreateModel
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Required]
    public Guid ProductVariantId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}
