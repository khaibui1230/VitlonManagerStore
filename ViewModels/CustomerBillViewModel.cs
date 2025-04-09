using System;
using System.Collections.Generic;
using QuanVitLonManager.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class CustomerBillViewModel
    {
        [Required]
        public required string OrderId { get; set; }

        [Required]
        public required string CustomerName { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        [Required]
        public required DateTime OrderDate { get; set; }

        [Required]
        public required decimal TotalAmount { get; set; }

        [Required]
        public required List<BillItemViewModel> Items { get; set; } = new();

        [Required]
        public required RestaurantInfo RestaurantInfo { get; set; }

        [Required]
        public required string QrCodeImageBase64 { get; set; }

        public string? TableNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class BillItemViewModel
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public decimal Price => UnitPrice;
    }
} 