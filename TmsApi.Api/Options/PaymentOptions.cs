// exercise 3: the silent crash (optins pattern)
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
public class PaymentOptions
{
    [Required]
    public required string GatewayUrl {get; init;}

    [Range(100, 100000)]
    public decimal MaxDepositBirr {get; init;}
}

public class PaymentService
{

   private readonly PaymentOptions _options;
   public PaymentService(IOptions<PaymentOptions> options)
   {
       _options = options.Value;
   }
   public void ProcessPayment()
   {
       Console.WriteLine($"Gateway: {_options.GatewayUrl}");
       Console.WriteLine($"Max Deposit: {_options.MaxDepositBirr}");
   }
}