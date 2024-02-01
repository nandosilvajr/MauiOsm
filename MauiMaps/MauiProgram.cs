using CommunityToolkit.Maui;
using Mapsui.ViewModels;
using MauiMaps.Controls;
using MauiMaps.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using DotNet.Meteor.HotReload.Plugin;
using Refit;

namespace MauiMaps;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            #if DEBUG
            .EnableHotReload();
            #endif
        builder.Services.AddSingleton<MainViewModel>(); 
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddRefitClient<IItineroServiceAPI>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://10.0.2.2:50419"));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
    
    public static HttpClientHandler GetInsecureHandler()
    {
        HttpClientHandler handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert.Issuer.Equals("CN=10.0.2.2"))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
        return handler;
    }
}