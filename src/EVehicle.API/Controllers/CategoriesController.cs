using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Categories;
using EVehicle.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý danh mục (dùng cho select2)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Categories")]
[AllowAnonymous]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả danh mục (dùng cho select2)
    /// </summary>
    /// <returns>Danh sách danh mục</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<List<CategoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var response = await _categoryService.GetAllCategoriesAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục");
            return StatusCode(500, BaseResponse<List<CategoryResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách danh mục"));
        }
    }
}

