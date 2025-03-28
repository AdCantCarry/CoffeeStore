using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KidMartStore.Models
{
    public class CartItem
    {
        public Product _product { get; set; }
        public int _quantity { get; set; }
    }
    public class Cart
    {
        List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }
        public void Add_Product_Cart(Product _pro, int _quan = 1)
        {
            var item = Items.FirstOrDefault(s => s._product.ID_Product == _pro.ID_Product);
            if (item == null) // nếu giỏ hàng trống thì thêm dòng hàng mới vào giỏ
                items.Add(new CartItem
                {
                    _product = _pro,
                   _quantity = _quan
                });
            else
                item._quantity += _quan; // tổng số lượng trong giỏ hàng được cộng dồn
        }
        public int TotalMoney()
        {
            var total = items.Sum(s => s._quantity * s._product.Price);
            return (int)(decimal)total;
        }
        public void Update_quantity(int id, int _new_quan)
        {
            var item = items.Find(s => s._product.ID_Product == id);
            if (item != null)
                item._quantity = _new_quan;
        }
        public void Remove_CartItem(int id)
        {
            items.RemoveAll(s => s._product.ID_Product == id);
        }
        public void ClearCart()
        {
            items.Clear();
        }

    }

}