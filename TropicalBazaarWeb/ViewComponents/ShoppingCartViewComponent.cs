using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Utility;

namespace TropicalBazaarWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            if (claim != null)
            {
                var sessionINT = HttpContext.Session.GetInt32(SD.SessionCart);

                if (sessionINT != null)
                    return View(sessionINT);
                else
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(u => u.AppuserId == claim.Value).ToList().Count);

                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
