using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.Web;
using System.Web.Mvc;
using KidMartStore.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Helpers;
using System.Globalization;
using System.Data.Entity;
using Microsoft.Ajax.Utilities;
using System.Runtime.Remoting.Contexts;
using System.Web.UI;

namespace KidMartStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly KidMartStoreEntities database = new KidMartStoreEntities();
        public ActionResult Index(string category, string query, int page = 1)
        {
            int pageSize = 4;
            List<Product> products = database.Products.ToList();

            // Lọc theo danh mục nếu có
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Name_Category == category).ToList();
            }

            // Tìm kiếm sản phẩm nếu có từ khóa query
            if (!string.IsNullOrEmpty(query))
            {
                products = products.Where(p => p.Name.Contains(query) || p.Description.Contains(query)).ToList();
            }

            int totalProducts = products.Count(); // Tổng số sản phẩm
            products = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.Category = category;
            ViewBag.Query = query;

            var sliders = database.Sliders.Where(s => s.Active).OrderBy(s => s.Position).ToList();
            ViewBag.Sliders = sliders;

            return View(products);
        }
        public ActionResult Blog()
        {
            return View(); 
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult GioiThieu()
        {
            return View();
        }
        public ActionResult SanPham(int? page)
        {
            int pageSize = 16; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1; // Nếu không có page, mặc định là 1

            List<Product> products = database.Products.ToList();
            int totalProducts = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Kiểm tra giới hạn của pageNumber
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            products = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return View(products);
        }
        public ActionResult Profile()
        {
            var MaKH = Session["ID_Customer"];
            var customer = database.Customers.Find(MaKH);
            return View(customer);
        }
        [HttpPost]
        public ActionResult Profile(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var userId = Convert.ToInt64(Session["ID_Customer"]); // Retrieve the current user ID
                var user = database.Customers.Find(userId);

                if (user != null)
                {
                    user.Username = customer.Username;
                    user.Address = customer.Address;
                    user.Phone = customer.Phone;
                    // Update other properties

                    database.SaveChanges();

                    Session["Email"] = user.Email;
                    Session["Name"] = user.Username;
                    Session["Phone"] = user.Phone;
                    Session["Address"] = user.Address;

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Profile"); // Adjust as needed
                }
            }
            return View(customer);
        }

        public ActionResult Address()
        {
            return View();
        }
        public ActionResult ShowOrder()
        {
            if (Session["ID_Customer"] != null)
            {
                var customerID = Convert.ToInt64(Session["ID_Customer"]);
                var orders = database.Orders
                    .Where(d => d.ID_Customer == customerID)
                    .Include(o => o.Detail_Order.Select(p => p.Product))
                    .ToList();
                return View(orders);
            }
            if (Session["ID_Customer"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        public ActionResult CancelOrder(int id)
        {
            var order = database.Orders.FirstOrDefault(o => o.ID_Order == id);

            if (order != null && order.Status != "Đã giao")
            {
                // Lấy danh sách sản phẩm trong đơn hàng
                var orderDetails = database.Detail_Order.Where(od => od.ID_Order == id).ToList();

                foreach (var item in orderDetails)
                {
                    var product = database.Products.FirstOrDefault(p => p.ID_Product == item.ID_Product);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity; // Cộng lại số lượng sản phẩm vào kho
                    }
                }

                // Cập nhật trạng thái đơn hàng
                order.Status = "Đã hủy";
                database.SaveChanges();

                TempData["Message"] = "Đơn hàng đã được hủy thành công! Số lượng sản phẩm đã được cập nhật.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này!";
            }

            return RedirectToAction("ShowOrder");
        }

    }
}