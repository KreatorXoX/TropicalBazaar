using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;
using TropicalBazaar.Utility;

namespace TropicalBazaarWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UnitController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnitController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Unit> objUnitList = _unitOfWork.Unit.GetAll();
            return View(objUnitList);
        }

        // Create
        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Unit obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Unit.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Unit Created Successfully";
                return RedirectToAction("Index");
            }
            else
                return View(obj);

        }

        //Edit
        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var unitFromDb = _unitOfWork.Unit.GetFirstOrDefault(u => u.Id == id);

            if (unitFromDb == null)
                return NotFound();

            return View(unitFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Unit obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Unit.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Unit Uptaded Successfully";
                return RedirectToAction("Index");
            }
            else
                return View(obj);
        }

        //Delete
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var unitFromDb = _unitOfWork.Unit.GetFirstOrDefault(u => u.Id == id);

            if (unitFromDb == null)
                return NotFound();

            return View(unitFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Unit obj)
        {
            _unitOfWork.Unit.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Unit Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
