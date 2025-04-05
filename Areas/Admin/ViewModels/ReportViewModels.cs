using QuanVitLonManager.Models;
using System;
using System.Collections.Generic;

namespace QuanVitLonManager.Areas.Admin.ViewModels
{
    public class RevenueReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<DailyRevenueViewModel> DailyRevenue { get; set; } = new();
        public List<CategoryRevenueViewModel> CategoryRevenue { get; set; } = new();
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CategoryRevenueViewModel
    {
        public required string CategoryName { get; set; }
        public decimal Revenue { get; set; }
        public int ItemCount { get; set; }
    }

    public class ProductsReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public List<TopSellingItemViewModel> TopSellingItems { get; set; } = new();
        public List<CategorySalesViewModel> CategorySalesData { get; set; } = new();
    }

    public class CategorySalesViewModel
    {
        public required string CategoryName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomersReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<CustomerReportItem> Customers { get; set; } = new();
    }

    public class CustomerReportItem
    {
        public required string CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required string PhoneNumber { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopCustomerViewModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class OrdersByHourViewModel
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class OrderStatusReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public List<OrderStatusViewModel> OrderStatusData { get; set; } = new();
    }

    public class OrderStatusViewModel
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TopSellingItemViewModel
    {
        public int MenuItemId { get; set; }
        public required string MenuItemName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
    }
} 