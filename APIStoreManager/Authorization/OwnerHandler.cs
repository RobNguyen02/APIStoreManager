using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using APIStoreManager.Data;


namespace APIStoreManager.Authorization
{
    public class OwnerHandler : AuthorizationHandler<OwnerRequirement>
    {
        private readonly StoreManagerContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OwnerHandler(StoreManagerContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerRequirement requirement)
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return;

            var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
            if (routeData == null || !routeData.Values.TryGetValue("shopId", out var shopIdObj))
                return;

            if (!long.TryParse(shopIdObj?.ToString(), out long shopId))
                return;

            var shop = await _db.Shops.FindAsync(shopId);
            if (shop != null && shop.OwnerId == userId)
            {
                context.Succeed(requirement);
            }
        }
    }
}

