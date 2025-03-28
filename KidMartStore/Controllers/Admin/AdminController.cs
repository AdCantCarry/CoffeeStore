using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using KidMartStore.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace KidMartStore.Controllers
{
    public class AdminController : Controller
    {
        public KidMartStoreEntities database = new KidMartStoreEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Bỏ qua kiểm tra cho các trang Login và Logout
            if (filterContext.ActionDescriptor.ActionName != "Login" &&
                filterContext.ActionDescriptor.ActionName != "Logout")
            {
                if (Session["Email"] == null)
                {
                    filterContext.Result = RedirectToAction("Login", "Admin");
                }
            }
            base.OnActionExecuting(filterContext);
        }


        // GET: Admin
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra thông tin đăng nhập
                var checkUser = database.Customers.FirstOrDefault(u => u.Email == customer.Email && u.Password == customer.Password);
                if (checkUser != null)
                {
                    // Lưu tên người dùng vào session
                    Session["Email"] = checkUser.Email;
                    Session["Name"] = checkUser.Username;
                    Session["Address"] = checkUser.Address;
                    Session["Phone"] = checkUser.Phone;
                    Session["ID_Customer"] = checkUser.ID_Customer;
                    Session["Role"] = checkUser.Role;
                    if (checkUser.Role == "Khách Hàng")
                    {
                        ViewBag.Error = "Đây là tài khoản khách hàng";
                        return View();
                    }
                    if (checkUser.Role != "Khách Hàng")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
                else
                {
                    // Kiểm tra nếu email không tồn tại
                    var user = database.Customers.FirstOrDefault(u => u.Email == customer.Email);
                    if (user == null)
                    {
                        ViewBag.ErrorEmail = "Email không tồn tại";
                        return View();
                    }
                    else
                    {
                        ViewBag.ErrorPassword = "Sai mật khẩu";
                        return View();
                    }
                }
            }
            return View();
        }
        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa tất cả session khi đăng xuất
            return RedirectToAction("Login");
        }
        public ActionResult Dashboard()
        {
            // Lấy tổng số người dùng (nếu không có, trả về 0)
            var totalUsers = database.Customers?.Count(c => c.Role == "Khách Hàng") ?? 0;

            // Lấy tổng số nhân viên (nếu không có, trả về 0)
            var totalAdmins = database.Customers?.Count(c => c.Role == "Nhân Viên" || c.Role == "Quản Lý") ?? 0;

            // Lấy tổng số sản phẩm (nếu không có, trả về 0)
            var totalProducts = database.Products?.Count() ?? 0;

            // Lấy tổng số đơn hàng (nếu không có, trả về 0)
            var totalOrders = database.Orders?.Count() ?? 0;

            // Truyền dữ liệu qua ViewBag
            ViewBag.TotalAdmins = totalAdmins;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;

            return View();
        }
        public ActionResult ManagerAccount()
        {
            List<Customer> customers = database.Customers.ToList();
            return View(customers);
        }
        public ActionResult ExportCustomersToExcel()
        {
            var customers = database.Customers
                            .Where(c => c.Role == "Quản Lý" || c.Role == "Nhân Viên")
                            .ToList();
            if (customers.Count == 0)
            {
                return Content("Không có khách hàng nào phù hợp để xuất Excel.");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Danh sách khách hàng");

                // Thiết lập tiêu đề
                worksheet.Cells["A1:F1"].Merge = true;
                worksheet.Cells["A1"].Value = "DANH SÁCH KHÁCH HÀNG";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Định dạng cột tiêu đề
                string[] columnHeaders = { "ID", "Tên", "Email", "SĐT", "Địa chỉ", "Quyền" };
                for (int i = 0; i < columnHeaders.Length; i++)
                {
                    worksheet.Cells[2, i + 1].Value = columnHeaders[i];
                    worksheet.Cells[2, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[2, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[2, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    worksheet.Cells[2, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Thêm dữ liệu khách hàng vào bảng
                int row = 3;
                foreach (var customer in customers)
                {
                    worksheet.Cells[row, 1].Value = customer.ID_Customer;
                    worksheet.Cells[row, 2].Value = customer.Username;
                    worksheet.Cells[row, 3].Value = customer.Email;
                    worksheet.Cells[row, 4].Value = customer.Phone;
                    worksheet.Cells[row, 5].Value = customer.Address;
                    worksheet.Cells[row, 6].Value = customer.Role;
                    row++;
                }

                // Căn chỉnh cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Xuất file Excel
                byte[] fileContents = excelPackage.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachKhachHang.xlsx");
            }
        }
        public ActionResult ManagerAccountAdmin()
        {
            List<Customer> customers = database.Customers.ToList();
            return View(customers);
        }

        public ActionResult DeleteAccountAdmin(int id)
        {
            var customer = database.Customers.Find(id);
            if (customer != null)
            {
                database.Customers.Remove(customer);
                database.SaveChanges();
            }
            return RedirectToAction("ManagerAccountAdmin"); // Hoặc trang danh sách người dùng.
        }

        public ActionResult ManagerProduct()
        {
            List<Product> products = database.Products.ToList();
            return View(products);
        }
        
        public ActionResult ManagerCategory()
        {
            List<Category> categories = database.Categories.ToList();
            return View(categories);
        }

        public ActionResult ManagerOrders()
        {
            var orders = database.Orders.OrderByDescending(o => o.Order_Date).ToList();
            return View(orders);
        }
        public ActionResult ExportOrdersToExcel()
        {
            var orders = database.Orders.ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Danh sách đơn hàng");

                string[] columnHeaders = { "ID", "Khách hàng", "Ngày đặt", "Tổng tiền", "Trạng thái" };
                for (int i = 0; i < columnHeaders.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = columnHeaders[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cells[row, 1].Value = order.ID_Order;
                    worksheet.Cells[row, 2].Value = order.Customer.Username;
                    worksheet.Cells[row, 3].Value = order.Order_Date.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 4].Value = order.Total;
                    worksheet.Cells[row, 5].Value = order.Status;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                byte[] fileContents = excelPackage.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDonHang.xlsx");
            }
        }


        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = database.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                database.SaveChanges();
                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            return Json(new { success = false, message = "Đơn hàng không tồn tại!" });
        }
    }
}