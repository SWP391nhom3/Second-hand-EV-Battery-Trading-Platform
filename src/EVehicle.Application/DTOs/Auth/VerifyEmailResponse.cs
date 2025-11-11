namespace EVehicle.Application.DTOs.Auth;

/// <summary>
/// DTO cho response verify email
/// </summary>
public class VerifyEmailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
}

