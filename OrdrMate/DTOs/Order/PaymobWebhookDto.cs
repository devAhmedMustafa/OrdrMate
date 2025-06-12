namespace OrdrMate.DTOs.Order;

public class TransactionDto
{
    public string Type { get; set; } = null!;
    public TransactionDetails Obj { get; set; } = null!;
}

public class TransactionDetails
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public bool Pending { get; set; }
    public OrderDetails Order { get; set; } = null!;
}

public class OrderDetails
{
    public long Id { get; set; }
}

public class MerchantDetails
{
    public string CompanyName { get; set; } = null!;
    public string Country { get; set; } = null!;
}

public class SourceData
{
    public string Type { get; set; } = null!;
    public string Pan { get; set; } = null!;
}
