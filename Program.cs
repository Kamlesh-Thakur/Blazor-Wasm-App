using BlazorWasmApp;
using BlazorWasmApp.Handlers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:44372/") });

// Register the custom AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
