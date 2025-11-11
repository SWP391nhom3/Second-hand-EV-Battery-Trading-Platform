namespace EVehicle.Application.DTOs.Contracts;

/// <summary>
/// Request DTO cho ký hợp đồng (UC29)
/// </summary>
public class ContractSignRequest
{
    /// <summary>
    /// Chữ ký điện tử (base64 encoded signature image hoặc OTP)
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Loại ký (SIGNATURE hoặc OTP)
    /// </summary>
    public string SignType { get; set; } = "SIGNATURE"; // SIGNATURE, OTP
}

