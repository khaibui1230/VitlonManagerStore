using System;
using System.Collections.Generic;
using QuanVitLonManager.Models;

namespace QuanVitLonManager.ViewModels
{
    public class CustomerBillViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string TableNumber { get; set; }
        public string Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BillItemViewModel> Items { get; set; }
        public RestaurantInfo RestaurantInfo { get; set; }
        public string QrCodeImageBase64 { get; set; }
    }

    public class BillItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }
} 