

namespace EIS.Inventory.Shared.Models
{
    public enum OrderPaymentStatus
    {
        NoPayment = 0,
        Authorized = 1,
        UnCleared  = 2,
        Charged = 3,
        PartialPayment = 4,
        PartialRefund = 5,
        FullRefund = 6,
        PaymentError = 7
    }
}
