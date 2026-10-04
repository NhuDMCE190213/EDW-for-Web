using BLL.DTOs.Promotion;
using BLL.DTOs.Promotion.PromotionList;
using BLL.Services.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using System.Net.Http.Json;
using System.Text.Json;

namespace Admin.Blazor.Services
{
    public class PromotionApiClient : IPromotionService
    {
        private readonly HttpClient _httpClient;

        public PromotionApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task HandleResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errors = await response.Content.ReadFromJsonAsync<List<ValidationFailure>>();
                    if (errors != null && errors.Any())
                    {
                        throw new ValidationException(errors);
                    }
                }
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task CreatePromotionAsync(PromotionCreateDto promotionCreateDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/promotions", promotionCreateDto);
            await HandleResponseAsync(response);
        }

        public async Task DeletePromotionAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/promotions/{id}");
            await HandleResponseAsync(response);
        }

        public async Task<List<PromotionDto>> GetActiveAndUpcomingPromotionsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<PromotionDto>>("api/promotions/active");
            return response ?? new List<PromotionDto>();
        }

        public async Task<PromotionListResponeDto> GetPromotionsListAsync(PromotionListRequest request)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(request.SearchTerm)) queryParams.Add($"SearchTerm={Uri.EscapeDataString(request.SearchTerm)}");
            if (request.StartAt.HasValue) queryParams.Add($"StartAt={request.StartAt.Value.ToString("O")}");
            if (request.EndAt.HasValue) queryParams.Add($"EndAt={request.EndAt.Value.ToString("O")}");
            if (request.IncludeDeleted) queryParams.Add($"IncludeDeleted={request.IncludeDeleted}");
            if (request.IncludeDisable) queryParams.Add($"IncludeDisable={request.IncludeDisable}");
            if (request.Pagination != null)
            {
                queryParams.Add($"Pagination.Page={request.Pagination.Page}");
                queryParams.Add($"Pagination.PageSize={request.Pagination.PageSize}");
            }

            var queryString = string.Join("&", queryParams);
            var url = $"api/promotions{(string.IsNullOrEmpty(queryString) ? "" : "?" + queryString)}";

            var response = await _httpClient.GetFromJsonAsync<PromotionListResponeDto>(url);
            return response ?? new PromotionListResponeDto();
        }

        public async Task UpdatePromotionAsync(Guid id, PromotionUpdateDto promotionUpdateDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/promotions/{id}", promotionUpdateDto);
            await HandleResponseAsync(response);
        }
    }
}

