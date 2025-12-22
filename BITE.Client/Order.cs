using System;

namespace BITE.Client;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string FoodItem { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
}