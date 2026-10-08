using BLL.DTOs.Pagination;
using BLL.DTOs.Promotion;
using BLL.DTOs.Promotion.PromotionList;
using Microsoft.AspNetCore.Mvc;
using PresentationMVC.Models.Promotion;
using PresentationMVC.Services;

namespace PresentationMVC.Controllers
{
    public class StaffPromotionController : Controller
    {
        private readonly PromotionApiClient _promotionApiClient;

        public StaffPromotionController(PromotionApiClient promotionApiClient)
        {
            _promotionApiClient = promotionApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] StaffPromotionIndexViewModel model)
        {
            // if (!User.IsInRole("Admin"))
            // {
            //     TempData["AccessDeniedMessage"] = "Yêu cầu quyền Quản trị viên (Admin) để quản lý Promotion!";
            //     return RedirectToAction("Index", "Home");
            // }

            var request = new PromotionListRequest
            {
                SearchTerm = model.SearchName,
                StartAt = model.StartAt,
                EndAt = model.EndAt,
                DiscountType = model.DiscountType,
                MinDiscount = model.MinDiscount,
                MaxDiscount = model.MaxDiscount,
                IncludeDeleted = model.IncludeDeleted,
                IncludeDisable = model.IncludeDisabled,
                IncludeActive = model.IncludeActive,
                IncludeUpcoming = model.IncludeUpcoming,
                IncludeExpired = model.IncludeExpired,
                IncludeOutOfStock = model.IncludeOutOfStock,
                Pagination = new PaginationRequest
                {
                    Page = model.PageNumber,
                    PageSize = model.PageSize
                }
            };

            model.PromotionData = await _promotionApiClient.GetPromotionsListAsync(request);
            model.MaxCost = model.PromotionData.MaxCost;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Table([FromQuery] StaffPromotionIndexViewModel model)
        {
            var request = new PromotionListRequest
            {
                SearchTerm = model.SearchName,
                StartAt = model.StartAt,
                EndAt = model.EndAt,
                DiscountType = model.DiscountType,
                MinDiscount = model.MinDiscount,
                MaxDiscount = model.MaxDiscount,
                IncludeDeleted = model.IncludeDeleted,
                IncludeDisable = model.IncludeDisabled,
                IncludeActive = model.IncludeActive,
                IncludeUpcoming = model.IncludeUpcoming,
                IncludeExpired = model.IncludeExpired,
                IncludeOutOfStock = model.IncludeOutOfStock,
                Pagination = new PaginationRequest
                {
                    Page = model.PageNumber,
                    PageSize = model.PageSize
                }
            };

            model.PromotionData = await _promotionApiClient.GetPromotionsListAsync(request);
            model.MaxCost = model.PromotionData.MaxCost;
            return PartialView("_PromotionTable", model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // if (!User.IsInRole("Admin"))
            // {
            //     TempData["AccessDeniedMessage"] = "Yêu cầu quyền Quản trị viên (Admin) để quản lý Promotion!";
            //     return RedirectToAction("Index", "Home");
            // }

            var dto = new PromotionCreateDto
            {
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(7)
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PromotionCreateDto dto)
        {
            // if (!User.IsInRole("Admin"))
            // {
            //     TempData["AccessDeniedMessage"] = "Yêu cầu quyền Quản trị viên (Admin) để quản lý Promotion!";
            //     return RedirectToAction("Index", "Home");
            // }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _promotionApiClient.CreatePromotionAsync(dto);
                TempData["SuccessMessage"] = "Tạo Promotion thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id, string returnUrl)
        {
            var p = await _promotionApiClient.GetPromotionByIdAsync(id);

            if (p == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");
            return View(p);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // if (!User.IsInRole("Admin"))
            // {
            //     TempData["AccessDeniedMessage"] = "Yêu cầu quyền Quản trị viên (Admin) để quản lý Promotion!";
            //     return RedirectToAction("Index", "Home");
            // }

            var p = await _promotionApiClient.GetPromotionByIdAsync(id);

            if (p == null) return NotFound();

            var dto = new PromotionUpdateDto
            {
                Name = p.Name!,
                PromotionType = p.PromotionType,
                Percentage = p.Percentage,
                SalePrice = p.SalePrice,
                ThresholdPrice = p.ThresholdPrice,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                IsReservedStock = p.IsReservedStock,
                MaxReservedStock = p.MaxReservedStock,
                IsLimitedTime = p.IsLimitedTime,
                IsDisable = p.IsDisabled
            };

            ViewBag.Id = id;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, PromotionUpdateDto dto)
        {
            // if (!User.IsInRole("Admin"))
            // {
            //     TempData["AccessDeniedMessage"] = "Yêu cầu quyền Quản trị viên (Admin) để quản lý Promotion!";
            //     return RedirectToAction("Index", "Home");
            // }

            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                return View(dto);
            }

            try
            {
                await _promotionApiClient.UpdatePromotionAsync(id, dto);
                TempData["SuccessMessage"] = "Cập nhật Promotion thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Id = id;
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            // if (!User.IsInRole("Admin"))
            // {
            //     TempData["AccessDeniedMessage"] = "Yêu cầu quyền Quản trị viên (Admin) để quản lý Promotion!";
            //     return RedirectToAction("Index", "Home");
            // }

            try
            {
                await _promotionApiClient.DeletePromotionAsync(id);
                TempData["SuccessMessage"] = "Xoá/Disable Promotion thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
