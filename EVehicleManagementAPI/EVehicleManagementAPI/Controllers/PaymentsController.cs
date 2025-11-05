using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EVehicleManagementAPI.DBconnect;
using EVehicleManagementAPI.Options;
using EVehicleManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EVehicleManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly EVehicleDbContext _db;
        private readonly PayOsService _payOsService;
        private readonly PayOsOptions _options;
        private readonly IWebHostEnvironment _env;

        public PaymentsController(EVehicleDbContext db, PayOsService payOsService, IOptions<PayOsOptions> options, IWebHostEnvironment env)
        {
            _db = db;
            _payOsService = payOsService;
            _options = options.Value;
            _env = env;
        }

        public class CreateCheckoutRequest
        {
            public int PostId { get; set; }
            public int PackageId { get; set; }
        }

        public class CreateCheckoutResponse
        {
            public string CheckoutUrl { get; set; } = string.Empty;
            public string OrderCode { get; set; } = string.Empty;
            public string TransferContent { get; set; } = string.Empty;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutRequest request, CancellationToken ct)
        {
            var post = await _db.Posts.FindAsync(new object[] { request.PostId }, ct);
            if (post == null) return NotFound("Post not found");

            var package = await _db.PostPackages.FirstOrDefaultAsync(p => p.PackageId == request.PackageId, ct);
            if (package == null) return NotFound("Package not found");

            // Enforce admin-approved status before checkout
            if (!string.Equals(post.Status, "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Post is not approved yet");
            }

            var packageCode = package.PriorityLevel switch
            {
                1 => "BASIC",
                2 => "STANDARD",
                3 => "PREMIUM",
                _ => "BASIC"
            };

            var transferContent = _payOsService.GenerateTransferContent(post.PostId, packageCode);

            // Create a local Payment entity (pending)
            var payment = new Models.Payment
            {
                BuyerId = post.MemberId,
                SellerId = post.MemberId, // for package purchase, both can be same member
                Amount = package.Price,
                Method = "PayOS",
                TransferContent = transferContent,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);

            // Create PostPackageSub (pending, dates to be set after paid)
            var sub = new Models.PostPackageSub
            {
                PostId = post.PostId,
                PackageId = package.PackageId,
                MemberId = post.MemberId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now, // will update after paid
                PaymentId = payment.Id,
                Status = "PENDING"
            };
            _db.PostPackageSubs.Add(sub);
            await _db.SaveChangesAsync(ct);

            var orderCode = $"POST{post.PostId}-PKG{package.PackageId}-PAY{payment.Id}";

            var amountVnd = (long)decimal.Round(package.Price, 0, MidpointRounding.AwayFromZero);
            var (checkoutUrl, actualOrderCode) = await _payOsService.CreateOrderAsync(
                amount: amountVnd,
                description: transferContent,
                orderCode: orderCode,
                returnUrl: _options.ReturnUrl,
                cancelUrl: _options.CancelUrl,
                ct: ct);

            return Ok(new CreateCheckoutResponse
            {
                CheckoutUrl = checkoutUrl,
                OrderCode = actualOrderCode,
                TransferContent = transferContent
            });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var rawBody = await reader.ReadToEndAsync();
            var signature = Request.Headers["x-signature"].FirstOrDefault();

            if (!_payOsService.VerifyWebhookSignature(_options.ChecksumKey, rawBody, signature)
                && !(_env.IsDevelopment() && _options.SkipSignatureValidation))
            {
                return Unauthorized();
            }

            using var doc = JsonDocument.Parse(rawBody);
            var data = doc.RootElement.GetProperty("data");
            var status = data.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
            var orderCode = data.TryGetProperty("orderCode", out var orderEl) ? orderEl.GetString() : null;
            var amount = data.TryGetProperty("amount", out var amountEl) ? amountEl.GetInt64() : (long?)null;

            if (string.IsNullOrEmpty(orderCode) || amount == null)
            {
                return BadRequest("Missing orderCode or amount");
            }

            // Resolve identifiers from orderCode
            // Expected: POST{postId}-PKG{packageId}-PAY{paymentId}
            int? paymentId = null;
            var payIdx = orderCode.LastIndexOf("-PAY", StringComparison.OrdinalIgnoreCase);
            if (payIdx >= 0)
            {
                var idPart = orderCode.Substring(payIdx + 4);
                if (int.TryParse(idPart, out var pid)) paymentId = pid;
            }

            if (paymentId == null) return BadRequest("Invalid orderCode format");

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId.Value);
            if (payment == null) return NotFound();

            if (string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "SUCCEEDED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = "Completed";

                var sub = await _db.PostPackageSubs.FirstOrDefaultAsync(s => s.PaymentId == payment.Id);
                if (sub != null)
                {
                    sub.Status = "ACTIVE";
                    sub.StartDate = DateTime.Now;
                    var pkg = await _db.PostPackages.FirstOrDefaultAsync(p => p.PackageId == sub.PackageId);
                    var days = pkg?.DurationDay ?? 0;
                    sub.EndDate = sub.StartDate.AddDays(days);

                    if (sub.PostId.HasValue)
                    {
                        var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == sub.PostId.Value);
                        if (post != null)
                        {
                            post.Status = "ACTIVE"; // publish
                            post.UpdatedAt = DateTime.Now;
                            post.ExpiryDate = sub.EndDate;
                        }
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(new { ok = true });
            }
            else if (string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = "Failed";
                await _db.SaveChangesAsync();
                return Ok(new { ok = true });
            }

            return Ok(new { ok = true });
        }

        // Dev-only: simulate webhook without PayOS
        [HttpPost("webhook/test")]
        public async Task<IActionResult> WebhookTest([FromBody] JsonElement body)
        {
            if (!_env.IsDevelopment()) return NotFound();

            var status = body.TryGetProperty("status", out var sEl) ? sEl.GetString() : "SUCCESS";
            var orderCode = body.TryGetProperty("orderCode", out var oEl) ? oEl.GetString() : null;
            int? paymentId = null;
            if (string.IsNullOrEmpty(orderCode))
            {
                paymentId = body.TryGetProperty("paymentId", out var pEl) && pEl.TryGetInt32(out var id) ? id : (int?)null;
            }
            else
            {
                var payIdx = orderCode.LastIndexOf("-PAY", StringComparison.OrdinalIgnoreCase);
                if (payIdx >= 0)
                {
                    var idPart = orderCode.Substring(payIdx + 4);
                    if (int.TryParse(idPart, out var pid)) paymentId = pid;
                }
            }

            if (paymentId == null) return BadRequest("Missing paymentId/orderCode");

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId.Value);
            if (payment == null) return NotFound();

            if (string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = "Completed";
                var sub = await _db.PostPackageSubs.FirstOrDefaultAsync(s => s.PaymentId == payment.Id);
                if (sub != null)
                {
                    sub.Status = "ACTIVE";
                    sub.StartDate = DateTime.Now;
                    var pkg = await _db.PostPackages.FirstOrDefaultAsync(p => p.PackageId == sub.PackageId);
                    var days = pkg?.DurationDay ?? 0;
                    sub.EndDate = sub.StartDate.AddDays(days);

                    if (sub.PostId.HasValue)
                    {
                        var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == sub.PostId.Value);
                        if (post != null)
                        {
                            post.Status = "ACTIVE";
                            post.UpdatedAt = DateTime.Now;
                            post.ExpiryDate = sub.EndDate;
                        }
                    }
                }
                await _db.SaveChangesAsync();
                return Ok(new { ok = true });
            }

            payment.Status = "Failed";
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
    }
}


