using System;

namespace EVehicleManagementAPI.Options
{
    public class PayOsOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty; // Or accessToken
        public string ChecksumKey { get; set; } = string.Empty; // For signature verification

        public string BaseUrl { get; set; } = "https://api.payos.vn"; // default

        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string WebhookPath { get; set; } = "/api/payments/webhook";

        // Local testing helpers
        public bool SkipSignatureValidation { get; set; } = false; // use only in Development
    }
}


