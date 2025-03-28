using KidMartStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public class StatisticController : Controller
{
    // Đảm bảo rằng bạn có một tham chiếu đến ngữ cảnh cơ sở dữ liệu của bạn
    public KidMartStoreEntities database = new KidMartStoreEntities();

    public ActionResult Index()
    {
        // Lấy dữ liệu thống kê từ cơ sở dữ liệu
        var totalRevenue = database.Orders.Sum(o => o.Total);
        var totalProductsSold = database.Detail_Order.Sum(op => op.Quantity);
        var newCustomers = database.Orders.Select(o => o.ID_Customer).Distinct().Count();
        var totalOrders = database.Orders.Count();
        var totalProducts = database.Products.Sum(p => p.Quantity); // Lấy tổng số lượng sản phẩm

        // Truyền dữ liệu vào ViewBag
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalProductsSold = totalProductsSold;
        ViewBag.NewCustomers = newCustomers;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalProducts = totalProducts; // Truyền tổng số sản phẩm

        return View();
    }

    // Đảm bảo giải phóng tài nguyên của ngữ cảnh cơ sở dữ liệu khi bạn không cần nữa
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            database.Dispose();
        }
        base.Dispose(disposing);
    }
}