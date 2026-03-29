using AppUser.Services;
using AppUser.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

namespace AppUser
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Poppins-Regular.ttf", "Poppins");
                    fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                    fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                });

            // Register Services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<POIService>();
            builder.Services.AddSingleton<AudioService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<POIListViewModel>();
            builder.Services.AddTransient<POIDetailViewModel>();
            builder.Services.AddTransient<AudioPlayerViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            // Register Pages
            builder.Services.AddTransient<Pages.LoginPage>();
            builder.Services.AddTransient<Pages.HomePage>();
            builder.Services.AddTransient<Pages.POIListPage>();
            builder.Services.AddTransient<Pages.POIDetailPage>();
            builder.Services.AddTransient<Pages.AudioPlayerPage>();
            builder.Services.AddTransient<Pages.ProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
