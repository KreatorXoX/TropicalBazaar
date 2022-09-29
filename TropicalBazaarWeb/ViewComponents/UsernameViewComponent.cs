using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TropicalBazaarWeb.ViewComponents
{
    public class UsernameViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return View(claim);

        }
    }
}
