using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace dtr_nne.Middleware;

public class Auth(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<Auth.AuthSettings> authSettingsOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly AuthSettings _authSettings = authSettingsOptions.Value;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (!Request.Headers.TryGetValue("Authorization", out var tokenValue))
        {
            return AuthenticateResult.Fail("Unauthorized");
        }
        
        var authToken = tokenValue.ToString().Split(" ")[^1];
        
        if (!authToken.Equals(_authSettings.Token)) return AuthenticateResult.Fail("Unauthorized");
        
        var claims = new[] { new Claim(ClaimTypes.Name, "User") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
    
    public class AuthSettings
    {
        public string Token { get; set; } = string.Empty;
    }
}