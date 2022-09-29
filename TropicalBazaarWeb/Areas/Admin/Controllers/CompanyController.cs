using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;
using TropicalBazaar.Utility;

namespace TropicalBazaarWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }


        //Edit and Create Upsert
        //GET
        public IActionResult Upsert(int? id)
        {

            Company companyObj = new();


            if (id == null || id == 0)
            {
                return View(companyObj);
            }
            else
            {
                //if id is not null we gonna update the properties in here.
                companyObj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(companyObj);
            }


        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Company Created Successfully";

                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                    TempData["success"] = "Company Updated Successfully";
                }

                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
                return View(obj);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return Json(new { success = "false", message = "Error !! Id doesnt exist" });

            var companyFromDb = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

            if (companyFromDb == null)
                return Json(new { success = false, message = "Error !! Company doesnt exist" });


            _unitOfWork.Company.Remove(companyFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Company deleted successfully!!" });
        }

        #endregion
    }
}
