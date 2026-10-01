using AutoMapper;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using BLL.DTOs.Promotion;
using BLL.DTOs.Validators.PromotionValidator;
using BLL.DTOs.Customer;
using BLL.DTOs.Validators.CustomerValidator;
using BLL.DTOs.Staff;
using BLL.DTOs.Validators.StaffValidator;
namespace BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDataAccessLayer(connectionString);
            // Register business logic services here
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductVariantService, ProductVariantService>();
            services.AddScoped<IProductReviewService, ProductReviewService>();
            
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPromotionService, PromotionService>();

            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();

            services.AddScoped<ICartItemService, CartItemService>();

            // Register Validators
            services.AddScoped<IValidator<PromotionCreateDto>, PromotionCreateDtoValidator>();
            services.AddScoped<IValidator<PromotionUpdateDto>, PromotionUpdateDtoValidator>();
            services.AddScoped<IValidator<CustomerCreateDto>, CustomerCreateDtoValidator>();
            services.AddScoped<IValidator<CustomerUpdateDto>, CustomerUpdateDtoValidator>();
            services.AddScoped<IValidator<StaffCreateDto>, StaffCreateDtoValidator>();
            services.AddScoped<IValidator<StaffUpdateDto>, StaffUpdateDtoValidator>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });
            return services;
        }
    }
}
