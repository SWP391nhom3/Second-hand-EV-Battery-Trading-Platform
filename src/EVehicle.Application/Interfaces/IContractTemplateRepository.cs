using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho ContractTemplate entity
/// </summary>
public interface IContractTemplateRepository
{
    /// <summary>
    /// Lấy ContractTemplate theo ID
    /// </summary>
    Task<ContractTemplate?> GetByIdAsync(int templateId);

    /// <summary>
    /// Lấy tất cả ContractTemplates
    /// </summary>
    Task<List<ContractTemplate>> GetAllAsync();

    /// <summary>
    /// Lấy ContractTemplates theo CategoryId
    /// </summary>
    Task<List<ContractTemplate>> GetByCategoryIdAsync(int? categoryId);

    /// <summary>
    /// Lấy ContractTemplates active
    /// </summary>
    Task<List<ContractTemplate>> GetActiveTemplatesAsync(int? categoryId = null);
}

