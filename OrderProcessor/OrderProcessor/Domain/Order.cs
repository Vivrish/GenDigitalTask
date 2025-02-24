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
    public required OrderStatus Status { get; set; }

    public Order(string orderCode, string productCode, decimal totalPrice, string currency)
    {
        OrderCode = orderCode;
        ProductCode = productCode;
        TotalPrice = totalPrice;
        Currency = currency;
        Status = OrderStatus.WaitingForPayment;
    }

    public Order(decimal paidAmount)
    {
        PaidAmount = paidAmount;
        Status = OrderStatus.WaitingForProduct;
    }
}