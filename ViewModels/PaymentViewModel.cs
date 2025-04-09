using QuanVitLonManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        [Required]
        public required string Notes { get; set; }
        
        [Required]
        public required List<PaymentItemViewModel> Items { get; set; }
    }

    public class PaymentItemViewModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        public decimal Subtotal { get; set; }
    }
} 