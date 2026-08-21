using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Profynus.Application.DTO.User;
using Profynus.Application.Common.Exception;
using Profynus.Infrastructure.Cache.Context;
using Profynus.Infrastructure.Persistence.Context;

namespace Profynus.Application.User;

public class UserService(
    CacheService _redis,
    QueryDbContext _db
)
{
    private readonly string _keyUsernameList = "Profynus:API:UsernameList";

    public async Task<UsernameValidation> ValidateUsername(string username)
    {
        // Search key in Redis
        var usernamesCache = await _redis.GetAsync<List<UsernameValidation>>(_keyUsernameList);
        
        // Find occurrences
        if (usernamesCache != null && 
            usernamesCache.Any(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            return new UsernameValidation
            {
                Username = username,
                ValidUsername = true,
                AvailableUsername = false
            };
        }

        // Search on DB and reload cache
        var userWithUserName = _db.Users
            .FirstOrDefault(u => u.Username!.ToLower() == username.ToLower());
        
        if (userWithUserName == null)
        {   // Available username
            return new UsernameValidation
            {
                Username = username,
                ValidUsername = true,
                AvailableUsername = true
            };  
        }
        
        // Return not available username and refresh cache
        await RefreshUsernameCache();
        
        return new UsernameValidation
        {
            Username = username,
            ValidUsername = true,
            AvailableUsername = false
        };
    }
    public async Task RefreshUsernameCache()
    {
        var listedUsernames = await _db.Users
            .Select(u => 
                new UsernameValidation
                {
                    Username = u.Username!,
                    ValidUsername = true,
                    AvailableUsername = false
                })
            .ToListAsync();
        
        if (listedUsernames.Count > 0)
        {
            // Load cache
            await _redis.SetAsync(_keyUsernameList, listedUsernames, null);
        }
    }
   
    public async Task<UserProfile> GetUserDetails(
        HttpContext httpContext,
        CancellationToken ct = default)
    {
        // JWT middleware already validated the token and populated User claims
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? httpContext.User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedException("Invalid token claims.");

        var user = await _db.Users
                       .AsNoTracking()
                       .FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("User not found.");

        return new UserProfile(){
            UserId=    user.Id,
            Username = user.Username!,
            Email=     user.Email,
            FirstName= user.FirstName!,
            LastName = user.LastName!,
            AccountCreationDate = user.CreatedAt
        };
    }
}