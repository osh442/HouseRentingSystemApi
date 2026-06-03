using System.Diagnostics;
using System.Globalization;

namespace HouseRentingSystemApi.Middleware
{
    public static class StopwatchMiddlewareExtensions
    {
        public static IApplicationBuilder UseStopwatch(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var stopwatch = Stopwatch.StartNew();

                await next();

                stopwatch.Stop();
                var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                context.Response.Headers["X-Task-Duration-Seconds"] =
                    elapsedSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            });
        }
    }
}
