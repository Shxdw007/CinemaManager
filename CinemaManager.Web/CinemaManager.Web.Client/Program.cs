using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://localhost:7028/") });

builder.Services.AddScoped<CinemaManager.Web.Client.Services.ApiService>();

await builder.Build().RunAsync();