using KidMartStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminWebKidMart.Controllers
{
    public class FunctionController : Controller
    {
        private readonly KidMartStoreEntities database = new KidMartStoreEntities();
        public ActionResult AddNewProduct()
        {
            List<Category> categories = database.Categories.ToList();
            return View(categories);
        }
        [HttpPost]
        public ActionResult AddNewProduct(Product NewProduct)
        {
            try
            {
                if (NewProduct.UploadImage != null && NewProduct.UploadImage.ContentLength > 0)
                {
                    try
                    {
                        string newFileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(NewProduct.UploadImage.FileName);
                        string NewName = newFileName + extension;
                        string FileName = Path.GetFileName(NewName);
                        string path = Path.Combine(Server.MapPath("~/Image/Product/"), FileName);

                        string externalFolder = @"D:\ShopBanQuanAo\KidMartStore\KidMartStore\Image\Product\";
                        string externalPath = Path.Combine(externalFolder, FileName);
                        // Lưu đường dẫn vào thuộc tính Image
                        NewProduct.Image = FileName;
                        NewProduct.UploadImage.SaveAs(path);
                        NewProduct.UploadImage.SaveAs(externalPath);
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi hoặc xử lý lỗi
                        Console.WriteLine("Lỗi khi lưu tệp: " + ex.Message);
                    }
                }

                database.Products.Add(NewProduct);
                database.SaveChanges();
                return RedirectToAction("ManagerProduct", "Admin");
            }
            catch
            {
                return View("AddNewProduct");
            }
        }
        public ActionResult UpdateProduct(int id)
        {
            var product = database.Products.Find(id); // Adjust this line based on your data access method
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        [HttpPost]
        public ActionResult UpdateProduct(Product product)
        {
            if (ModelState.IsValid)
            {

                // Update the product in the database
                var existingProduct = database.Products.Find(product.ID_Product);
                if (existingProduct != null)
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Quantity = product.Quantity;

                    database.SaveChanges(); // Save changes to the database
                }
            } // Redirect to the product list
            return RedirectToAction("ManagerProduct", "Admin");
        }
        public ActionResult DeleteProduct(int id)
        {
            var product = database.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound(); // Or any other appropriate action
            }

            database.Products.Remove(product);
            database.SaveChanges();
            return RedirectToAction("ManagerProduct", "Admin"); // Redirect to the product management page
        }

        public ActionResult AddNewCategory(Category NewCategory)
        {
            if (ModelState.IsValid)
            {
                if (CategoryNameExists(NewCategory.Name_Category))
                {
                    ModelState.AddModelError("", "Tên danh mục đã tồn tại. Vui lòng chọn tên khác.");
                    return View(NewCategory);
                }

                database.Categories.Add(NewCategory);

                try
                {
                    using (var transaction = database.Database.BeginTransaction())
                    {
                        database.SaveChanges();
                        transaction.Commit();
                    }

                    return RedirectToAction("ManagerCategory", "Admin");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi hoặc hiển thị thông báo lỗi chi tiết hơn
                    ModelState.AddModelError("", "Lỗi khi thêm danh mục. Vui lòng thử lại.");
                    // Hoặc ghi log lỗi: LogError(ex, "Error adding new category.");
                }
            }
            return View(NewCategory);
        }

        private bool CategoryNameExists(string categoryName)
        {
            return database.Categories.Any(c => c.Name_Category == categoryName);
        }
        public ActionResult EditCategory(int id)
        {
            var category = database.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost]
        public ActionResult UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = database.Categories.Find(category.ID_Category);
                if (existingCategory != null)
                {
                    try
                    {
                        existingCategory.Name_Category = category.Name_Category;
                        database.SaveChanges();
                        return RedirectToAction("ManagerCategory", "Admin");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi khi cập nhật danh mục: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy danh mục.");
                }
            }
            return View(category);
        }
        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            try
            {
                var category = database.Categories.Find(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục." });
                }

                database.Categories.Remove(category);
                database.SaveChanges();

                return Json(new { success = true, message = "Đã xóa danh mục." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa danh mục: " + ex.Message });
            }
        }

        public ActionResult UpdateAccount(int id)
        {
            var customer = database.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public ActionResult UpdateAccount(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = database.Customers.Find(customer.ID_Customer);
                if (existingCustomer != null)
                {
                    try
                    {
                        // Sử dụng TryUpdateModel (tùy chọn)
                        TryUpdateModel(existingCustomer);
                        database.SaveChanges();
                        return RedirectToAction("ManagerAccount", "Admin");
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi khi lưu thay đổi
                        ModelState.AddModelError("", "Lỗi khi cập nhật tài khoản: " + ex.Message);
                        return View(customer); // Trả lại view với lỗi
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy khách hàng.");
                    return View(customer);
                }
            }
            return View(customer);
        }

        public ActionResult AddNewAccount()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddNewAccount(Customer NewCustomer)
        {
            try
            {
                database.Customers.Add(NewCustomer);
                database.SaveChanges();
                return RedirectToAction("ManagerAccount", "Admin");
            }
            catch
            {
                return View("AddNewAccount");
            }
        }
        public ActionResult DeleteAccount(int id)
        {
            var customer = database.Customers.Find(id);
            if (customer != null)
            {
                database.Customers.Remove(customer);
                database.SaveChanges();
            }
            return RedirectToAction("ManagerAccount", "Admin"); // Hoặc trang danh sách người dùng.
        }
    }
}