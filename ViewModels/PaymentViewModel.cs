using QuanVitLonManager.Models;
using System;
using System.Collections.Generic;

namespace QuanVitLonManager.ViewModels
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Notes { get; set; }
        public List<PaymentItemViewModel> Items { get; set; }
    }

    public class PaymentItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
} 