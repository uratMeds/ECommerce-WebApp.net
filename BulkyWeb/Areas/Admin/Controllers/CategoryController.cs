using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Reflection.Metadata;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Telling that only Admin can access all the ctrl's action methods.
    // It means that if you type the url and you are not an Admin it won't work.
    public class CategoryController : Controller
    {
        // Controller Instantiation: When a request comes in that needs to be handled by the CategoryController,
        // the ASP.NET Core framework uses the DI container to create an instance of CategoryController.
        // It automatically provides an instance of ApplicationDbContext to the constructor.
        // Dependency Injection in Action: When the DI container creates an instance of CategoryController,
        // it sees that the constructor requires an ApplicationDbContext parameter.
        // It then provides an instance of ApplicationDbContext(configured in the ConfigureServices method)
        // to this constructor.This is how _db gets its value.

        // This is the old way to do it.
        // private readonly ApplicationDbContext _db;
        // public CategoryController(ApplicationDbContext db)
        // {
        //    _db = db;
        // }

        // This is the old way before UnitOfWork.
        // private readnoly ICategoryRepository _categoryRepo;
        // public CategoryController(ICategoryRepository db)
        // {
        //    _categoryRepo = db;
        // }

        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DO cannot exactly match the N.");
            }

            // if (obj.Name!=null && obj.Name.ToLower() == "test")
            // {  // In this case it doesn't show with a specific input, now it shows only in Summary.
            //     ModelState.AddModelError("", "test is an invalid value.");
            // }

            if (ModelState.IsValid) // Checks to see if it respectes Category conditions.
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id) // The param should match the name of the one used in asp-route-.
        {
            if (id == null || id == 0) { return NotFound(); }

            // Find() only works on PK not other fields.
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
            // Category? categoryFromDb1 = _db.Categories.FirstOrDefault(c => c.Id == id);
            // Category? categoryFromDb2 = _db.Categories.Where(c => c.Id == id).FirstOrDefault();

            if (categoryFromDb == null) { return NotFound(); }

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                // TempData only stores the info to the next render, after it goes away.
                TempData["success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id) // The param should match the name of the one used in asp-route-.
        {
            if (id == null || id == 0) { return NotFound(); }

            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null) { return NotFound(); }

            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")] // We specify it's name we make sure that it counts as Delete not DeletePOST.
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);

            if (obj == null) { return NotFound(); }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}