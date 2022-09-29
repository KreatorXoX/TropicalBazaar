using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;
using TropicalBazaar.Models.ViewModels;
using TropicalBazaar.Utility;

namespace TropicalBazaarWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        public ShoppingCartVM cartVM { get; set; }


        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // to get the unit of product 
            var shopcartWithProduct = _unitOfWork.ShoppingCart.GetAll(
                    u => u.AppuserId == claim.Value, includeForeignKeyProps: "Product");

            foreach (var productCart in shopcartWithProduct)
            {
                productCart.Product = _unitOfWork.Product.GetFirstOrDefault(
                    u => u.Id == productCart.ProductId, includeForeignKeyProps: "Category,Unit");
            }


            cartVM = new ShoppingCartVM()
            {
                AllProductsCart = shopcartWithProduct,
                OrderHeader = new()
            };

            foreach (var uniqueProduct in cartVM.AllProductsCart)
            {
                uniqueProduct.finalPrice = GetFinalPriceBasedOnQuantity(uniqueProduct.Count,
                    uniqueProduct.Product.Price, uniqueProduct.Product.Price50);

                cartVM.OrderHeader.OrderTotal += (uniqueProduct.finalPrice * uniqueProduct.Count);
            }

            return View(cartVM);
        }

        private double GetFinalPriceBasedOnQuantity(double quantity, double price, double price50)
        {
            if (quantity <= 50)
                return price;
            else
                return price50;
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, 1);
            _unitOfWork.Save();
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            if (cartFromDb.Count == 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                int count = _unitOfWork.ShoppingCart.GetAll(
                         u => u.AppuserId == cartFromDb.AppuserId).ToList().Count;

                HttpContext.Session.SetInt32(SD.SessionCart, count - 1);
            }
            else
                _unitOfWork.ShoppingCart.DecrementCount(cartFromDb, 1);

            _unitOfWork.Save();

            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();

            //count side to the basket case decrement.
            int count = _unitOfWork.ShoppingCart.GetAll(
                    u => u.AppuserId == cartFromDb.AppuserId).ToList().Count;

            HttpContext.Session.SetInt32(SD.SessionCart, count);

            return RedirectToAction("Index", "Cart");
        }

        //GET
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // to get the unit of product 
            var shopcartWithProduct = _unitOfWork.ShoppingCart.GetAll(
                    u => u.AppuserId == claim.Value, includeForeignKeyProps: "Product");

            foreach (var productCart in shopcartWithProduct)
            {
                productCart.Product = _unitOfWork.Product.GetFirstOrDefault(
                    u => u.Id == productCart.ProductId, includeForeignKeyProps: "Unit");
            }

            cartVM = new ShoppingCartVM()
            {
                AllProductsCart = shopcartWithProduct,
                OrderHeader = new()
            };

            cartVM.OrderHeader.Appuser = _unitOfWork.Appuser.GetFirstOrDefault(u => u.Id == claim.Value);

            //based on the logged in app user we already know the address and name info so its 
            // good to fill those same properties that we declared in OrderHeader.

            cartVM.OrderHeader.Name = cartVM.OrderHeader.Appuser.Name;
            cartVM.OrderHeader.Address = cartVM.OrderHeader.Appuser.Address;
            cartVM.OrderHeader.City = cartVM.OrderHeader.Appuser.City;
            cartVM.OrderHeader.State = cartVM.OrderHeader.Appuser.State;
            cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.Appuser.PostalCode;
            cartVM.OrderHeader.PhoneNumber = cartVM.OrderHeader.Appuser.PhoneNumber;


            foreach (var uniqueProduct in cartVM.AllProductsCart)
            {
                uniqueProduct.finalPrice = GetFinalPriceBasedOnQuantity(uniqueProduct.Count,
                    uniqueProduct.Product.Price, uniqueProduct.Product.Price50);

                cartVM.OrderHeader.OrderTotal += (uniqueProduct.finalPrice * uniqueProduct.Count);
            }

            return View(cartVM);
        }


        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM cartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //// to get the unit of product 
            //var shopcartWithProduct = _unitOfWork.ShoppingCart.GetAll(
            //        u => u.AppuserId == claim.Value, includeForeignKeyProps: "Product");

            //foreach (var productCart in shopcartWithProduct)
            //{
            //    productCart.Product = _unitOfWork.Product.GetFirstOrDefault(
            //        u => u.Id == productCart.ProductId, includeForeignKeyProps: "Unit");
            //}

            cartVM.AllProductsCart = _unitOfWork.ShoppingCart.GetAll(
                u => u.AppuserId == claim.Value, includeForeignKeyProps: "Product");


            cartVM.OrderHeader.OrderDate = DateTime.Now;
            cartVM.OrderHeader.AppuserId = claim.Value;


            foreach (var uniqueProduct in cartVM.AllProductsCart)
            {
                uniqueProduct.finalPrice = GetFinalPriceBasedOnQuantity(uniqueProduct.Count,
                    uniqueProduct.Product.Price, uniqueProduct.Product.Price50);

                cartVM.OrderHeader.OrderTotal += (uniqueProduct.finalPrice * uniqueProduct.Count);
            }

            Appuser appUser = _unitOfWork.Appuser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                cartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                cartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            // now that we uptaded the infos coming from the form post action!
            // we can add the orderheader to the db.

            _unitOfWork.OrderHeader.Add(cartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in cartVM.AllProductsCart)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = cartVM.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.finalPrice
                };

                _unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.Save();
            }

            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                // stripe settings(copy pasted from stripe website)
                var domain = "https://localhost:44342/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                    "card",
                },

                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={cartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                };


                foreach (var item in cartVM.AllProductsCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)item.finalPrice * 100,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                            },
                        },

                        //// Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                        //Price = "{{PRICE_ID}}",

                        Quantity = item.Count,
                    };

                    options.LineItems.Add(sessionLineItem);
                }





                var service = new SessionService();
                Session session = service.Create(options);

                //before returning the statuscoderesult we have information in the session 
                // that we need to populate our orderheader.
                _unitOfWork.OrderHeader.UpdateStripePaymentId(cartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = cartVM.OrderHeader.Id });
            }



        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == id, includeForeignKeyProps: "Appuser");


            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check stripe status to see if the payment is done or not.

                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                }
            }

            _emailSender.SendEmailAsync(orderHeader.Appuser.Email, "New Order - Tropical Bazaar",
                "<h1>Your order is taken !</h1> " +
                "<p>As soon as we ship your order you will be informed once again !</p>" +
                "<p> Thank you for using our services !</p>");

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.AppuserId ==
            orderHeader.AppuserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            HttpContext.Session.Clear();
            _unitOfWork.Save();

            return View(id);
        }
    }
}
