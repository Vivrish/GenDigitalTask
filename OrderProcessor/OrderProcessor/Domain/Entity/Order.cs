using System.ComponentModel.DataAnnotations;

namespace OrderProcessor.Domain;

public enum OrderStatus
{
    WaitingForProduct, // Payment is made, but product is not created yet
    WaitingForPayment, // Product exists, but order is not fully paid yet
    Finished
}

public class Order
{
    [Key]
    public int Id { get; set; } 
    public string? OrderCode { get; set; }  
    public string? ProductCode { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public decimal? PaidAmount { get; set; } = 0;
    public OrderStatus Status { get; set; }
    
    public Order() {}

    public override string ToString()
    {
        return $"Order: {OrderCode}, Product: {ProductCode}, Total: {TotalPrice} {Currency}, Status: {Status}";
    }
}