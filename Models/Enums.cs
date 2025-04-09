using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public enum OrderType
    {
        DineIn,
        TakeAway,
        Delivery
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Preparing,
        Ready,
        Delivered,
        Completed,
        Cancelled,
        Billing,
        Delivering
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        BankTransfer,
        EWallet,
        Card,
        MoMo,
        Banking
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded,
        Unpaid
    }

    public enum TableStatus
    {
        Available,
        Occupied,
        Reserved,
        Maintenance
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public enum DishOrderStatus
    {
        [Display(Name = "Đang chờ")]
        Pending,
        
        [Display(Name = "Đang chuẩn bị")]
        Preparing,
        
        [Display(Name = "Hoàn thành")]
        Completed,
        
        [Display(Name = "Đã hủy")]
        Cancelled
    }
} 