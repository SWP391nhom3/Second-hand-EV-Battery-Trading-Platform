using EVehicle.Domain.Entities;
using System.Security.Claims;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho JWT Service
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Tạo Access Token (JWT) từ thông tin user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Tạo Refresh Token từ thông tin user
    /// </summary>
    string GenerateRefreshToken(User user);

    /// <summary>
    /// Validate và parse JWT token (access token)
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Validate refresh token
    /// </summary>
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);

    /// <summary>
    /// Lấy thời gian hết hạn của access token
    /// </summary>
    DateTime GetAccessTokenExpiry();

    /// <summary>
    /// Lấy thời gian hết hạn của refresh token
    /// </summary>
    DateTime GetRefreshTokenExpiry();
}

