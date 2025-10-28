using Microsoft.Extensions.Logging;
using WG.PDFToolkit.Logger;

namespace WG.PdfTools
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Ubuntu-Regular.ttf", "UbuntuRegular");
                    fonts.AddFont("Ubuntu-Medium.ttf", "UbuntuMedium");
                }).ConfigureEssentials(essentials => essentials.UseVersionTracking());

            builder.Logging.AddFileLoggerProvider(Path.Combine(Environment.CurrentDirectory, "WG_PDFToolkit_Log.txt"));
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
