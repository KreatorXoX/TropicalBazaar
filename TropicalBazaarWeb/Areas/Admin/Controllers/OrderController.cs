using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using TropicalBazaar.DataAccess.Repository.IRepository;
using TropicalBazaar.Models;
using TropicalBazaar.Models.ViewModels;
using TropicalBazaar.Utility;

namespace TropicalBazaarWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {

        [BindProperty]
        public OrderVM orderVM { get; set; }
        // we binding this property because in post action we want this tobe returned.


        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            var allProducts = _unitOfWork.Product.GetAll(includeForeignKeyProps: "Category,Unit");

            orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeForeignKeyProps: "Appuser"),
                OrderDetail = _unitOfWork.OrderDetails.GetAll(u => u.OrderId == orderId, includeForeignKeyProps: "Product")
            };
            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayOrder()
        {
            orderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id, includeForeignKeyProps: "Appuser");
            orderVM.OrderDetail = _unitOfWork.OrderDetails.GetAll(u => u.OrderId == orderVM.OrderHeader.Id, includeForeignKeyProps: "Product");


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
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
            };


            foreach (var item in orderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price * 100,
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
            _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderId);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check stripe status to see if the payment is done or not.

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            else
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check stripe status to see if the payment is done or not.

                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id, tracked: false);

            orderFromDb.Name = orderVM.OrderHeader.Name;
            orderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderFromDb.Address = orderVM.OrderHeader.Address;
            orderFromDb.City = orderVM.OrderHeader.City;
            orderFromDb.State = orderVM.OrderHeader.State;
            orderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            if (orderVM.OrderHeader.Carrier != null)
                orderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            if (orderVM.OrderHeader.TrackingNumber != null)
                orderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Detailts Updated Successfully";

            return RedirectToAction("Details", "Order", new { orderId = orderFromDb.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessOrder()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated to In Process";

            return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id, tracked: false);

            orderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            orderFromDb.ShippingDate = DateTime.Now;
            orderFromDb.OrderStatus = SD.StatusShipped;

            if (orderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
                orderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated to Shipped";

            return RedirectToAction("Details", "Order", new { orderId = orderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id, tracked: false);

            if (orderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                // refund the money

                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFromDb.PaymentIntentId,

                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                // stripe takes care of the refund easly.

                _unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                // direct cancel
                _unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully";

            return RedirectToAction("Details", "Order", new { orderId = orderFromDb.Id });
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAllOrders(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeForeignKeyProps: "Appuser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.AppuserId == claim.Value, includeForeignKeyProps: "Appuser");
            }

            switch (status)
            {
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment || u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
