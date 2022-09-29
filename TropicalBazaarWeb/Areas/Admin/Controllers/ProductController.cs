using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models.ViewModels;
using TropicalBazaar.Utility;

namespace TropicalBazaarWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment; // to get the wwwroot path.

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }


        //Edit and Create Upsert
        //GET
        public IActionResult Upsert(int? id)
        {
            // we are using viewmodel approach instead of using it like this 
            //and passing data via viewdata or viewbag.

            //Product product = new();

            //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(
            //    u => new SelectListItem
            //    {
            //        Text = u.Name,
            //        Value = u.Id.ToString(),
            //    });


            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(
                    u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString(),
                    }
                    ),
                UnitList = _unitOfWork.Unit.GetAll().Select(
                    u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString(),
                    }
                    )
            };

            if (id == null || id == 0)
            {
                // create product if id null / 0
                // ViewBag.listOfCategories = CategoryList; passing a data from controller to view.
                // ViewData["categoryList"] = CategoryList;
                // @(ViewData["categoryList"] as IEnumerable<SelectListItem>) when using in the view page.


                return View(productVM);
            }
            else
            {
                //if id is not null we gonna update the properties in here.
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);


                return View(productVM);

            }


        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploadPath = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    // now we have the new guid filename, upload path and the extension of the file.
                    // we gonna check if the recieved obj has image url or not.
                    //if obj has image url we need to first delete the old img and then add the new image.

                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        // we use trimstart becayse in our imageurl path we have a backslash begore the images but its not needed
                        //filestream path.
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                    TempData["success"] = "Product Created Successfully";

                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                    TempData["success"] = "Product Updated Successfully";
                }

                _unitOfWork.Save();
                return RedirectToAction("Index");

            }



            else
                return View(obj);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetProducts()
        {
            var productList = _unitOfWork.Product.GetAll(includeForeignKeyProps: "Category,Unit");
            return Json(new { data = productList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return Json(new { success = "false", message = "Error !! Id doesnt exist" });

            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if (productFromDb == null)
                return Json(new { success = false, message = "Error !! Product doesnt exist" });


            if (productFromDb.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productFromDb.ImageUrl.TrimStart('\\'));
                // we use trimstart becayse in our imageurl path we have a backslash begore the images but its not needed
                //filestream path.
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _unitOfWork.Product.Remove(productFromDb);
            _unitOfWork.Save();



            return Json(new { success = true, message = "Product deleted successfully!!" });
        }

        #endregion
    }
}
