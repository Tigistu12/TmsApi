// exercise 3: the silent crash (optins pattern)
using System.ComponentModel.DataAnnotations;

public class PaymentOptions
{
    [Required]
    public required string GatewayUrl {get; init;}

    [Range(100, 100000)]
    public decimal MaxDepositBirr {get; init;}
}

