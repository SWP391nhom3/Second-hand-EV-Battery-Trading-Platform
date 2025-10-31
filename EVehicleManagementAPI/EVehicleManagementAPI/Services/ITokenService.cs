using System;

namespace EVehicleManagementAPI.Services
{
    public interface ITokenService
    {
        string CreateJwt(int accountId, string email, string role, DateTime? expires = null);
    }
}


