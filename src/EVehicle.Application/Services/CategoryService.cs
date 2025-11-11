using EVehicle.Application.DTOs.Categories;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Category Service implementation
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<List<CategoryResponse>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync();

            var responses = categories.Select(c => new CategoryResponse
            {
                Id = c.CategoryId,
                Text = c.Name,
                Code = c.Code
            }).ToList();

            return BaseResponse<List<CategoryResponse>>.SuccessResponse(
                responses,
                "Lấy danh sách danh mục thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục");
            return BaseResponse<List<CategoryResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách danh mục");
        }
    }
}

