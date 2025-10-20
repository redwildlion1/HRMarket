namespace HRMarket.Configuration.Redis;

/// <summary>
/// Centralized cache key management to prevent key collisions
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "hrmarket";
    
    // Token-related keys
    public static class Tokens
    {
        private const string TokenPrefix = $"{Prefix}:tokens";
        
        public static string Blacklist(string token) => $"{TokenPrefix}:blacklist:{token}";
        public static string UserRevocation(Guid userId) => $"{TokenPrefix}:revoked:{userId}";
        public static string RefreshToken(Guid userId, string token) => $"{TokenPrefix}:refresh:{userId}:{token}";
    }
    
    // User-related keys
    public static class Users
    {
        private const string UserPrefix = $"{Prefix}:users";
        
        public static string Info(Guid userId) => $"{UserPrefix}:info:{userId}";
        public static string Roles(Guid userId) => $"{UserPrefix}:roles:{userId}";
        public static string Permissions(Guid userId) => $"{UserPrefix}:permissions:{userId}";
        public static string Session(Guid userId) => $"{UserPrefix}:session:{userId}";
    }
    
    // Firm-related keys
    public static class Firms
    {
        private const string FirmPrefix = $"{Prefix}:firms";
        
        public static string Info(Guid firmId) => $"{FirmPrefix}:info:{firmId}";
        public static string Owner(Guid firmId) => $"{FirmPrefix}:owner:{firmId}";
        public static string UserFirms(Guid userId) => $"{FirmPrefix}:user:{userId}";
    }
    
    // Rate limiting keys
    public static class RateLimits
    {
        private const string RateLimitPrefix = $"{Prefix}:ratelimit";
        
        public static string LoginAttempts(string email) => $"{RateLimitPrefix}:login:{email}";
        public static string ApiCalls(string ipAddress) => $"{RateLimitPrefix}:api:{ipAddress}";
    }
    
    // Session keys
    public static class Sessions
    {
        private const string SessionPrefix = $"{Prefix}:sessions";
        
        public static string ActiveSessions(Guid userId) => $"{SessionPrefix}:active:{userId}";
        public static string SessionData(string sessionId) => $"{SessionPrefix}:data:{sessionId}";
    }
    
    // Generic cache patterns
    public static string GetPattern(string category) => $"{Prefix}:{category}:*";
}