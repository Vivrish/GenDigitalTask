namespace OrderProcessor.Domain;

public class OrderEvent(string id, string product, decimal total, string currency)
{
    public string id { get; set; } = id;
    public string product { get; set; } = product;
    public decimal total { get; set; } = total;

    public override string ToString()
    {
        return
            $"{nameof(id)}: {id}, {nameof(product)}: {product}, {nameof(total)}: {total}, {nameof(currency)}: {currency}";
    }

    public string currency { get; set; } = currency;
}
    