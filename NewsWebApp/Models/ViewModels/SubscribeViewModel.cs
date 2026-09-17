using System.ComponentModel.DataAnnotations;

namespace NewsWebApp.Models.ViewModels
{
    public class SubscribeViewModel
    {
        //public decimal Price { get; set; } = 99.99m;
        //public int DurationInDays { get; set; } = 30;

        //public string? PaymentToken { get; set; }

        //[Required(ErrorMessage = "Cardholder name is required")]
        //[StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters")]
        //[Display(Name = "Cardholder Name")]
        //public string? CardHolderName { get; set; }

        //[Required(ErrorMessage = "Card number is required")]
        //[Range(1000000000000, 9999999999999, ErrorMessage = "Must be a valid 13 digit card number")]
        //[Display(Name = "Card Number")]
        //public long CardNumber { get; set; }

        //[Required(ErrorMessage = "Expiry date is required")]
        //[Display(Name = "Expiry MM/YY")]
        //[StringLength(5, MinimumLength = 5, ErrorMessage = "Expiry date must be in MM/YY format")]
        //public string? ExpiryDate { get; set; }

        //[Required(ErrorMessage = "CVV is required")]
        //[Display(Name = "CVV")]
        //[StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be 3 digits")]
        //public string? CVV { get; set; }

        public string? StripeToken { get; set; }



    }
}
