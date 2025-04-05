using QuanVitLonManager.Models;
using System.Collections.Generic;

namespace QuanVitLonManager.Services
{
    public interface ICartService
    {
        List<CartItem> GetCart();
        void AddToCart(int menuItemId, int quantity, string notes = null);
        void UpdateQuantity(int menuItemId, int quantity);
        void RemoveFromCart(int menuItemId);
        void ClearCart();
        int GetCartItemCount();
        decimal GetCartTotal();
        void SetOrderType(CartOrderType orderType);
        CartOrderType GetOrderType();
        void SetTableNumber(string tableNumber);
        string GetTableNumber();
        void UpdateItemNotes(int menuItemId, string notes);
        void SaveCartToDatabase();
        void LoadCartFromDatabase();
    }
}