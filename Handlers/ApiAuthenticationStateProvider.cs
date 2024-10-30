using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorWasmApp.Handlers;

public class ApiAuthenticationStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    private readonly IJSRuntime _js = js;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Extract the username from the token or set it directly if available
        var userName = await GetUserNameFromToken(token);

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], "jwtAuthType");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        // Extract the username from the token or set it directly if available
        var userName = await GetUserNameFromToken(token);

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "jwtAuthType");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");

        var identity = new ClaimsIdentity();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    private async Task<string?> GetUserNameFromToken(string token)
    {
        var extractedToken = JsonSerializer.Deserialize<LoginResponse>(token);

        if (extractedToken is null)
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(extractedToken.token);
        return jwtToken?.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
    }
}

public class LoginResponse
{
    public string token { get; set; } = string.Empty;
    public DateTime expiration { get; set; }
}