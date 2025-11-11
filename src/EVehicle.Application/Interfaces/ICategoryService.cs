using EVehicle.Application.DTOs.Categories;
using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Category Service
/// </summary>
public interface ICategoryService
{
    Task<BaseResponse<List<CategoryResponse>>> GetAllCategoriesAsync();
}

