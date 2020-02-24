using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.Core.Aop.Extension.Interface;
using Quartz.Impl;
using Quartz.Spi;

namespace Quartz.Core.Aop.Extension
{
    public static class JobServiceExtension
    {
        public static void AddQuartzJob(this IServiceCollection services)
        {
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddSingleton<IJobFactory, JobAdapterFactory>();
            services.AddSingleton<IJobService, JobService>();
        
            services.AddTransient<JobAdapter>();
            services.AddTransient<JobRunner>();
          
        }

        public static void AddQuartzJob(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            using var service = app.ApplicationServices.GetRequiredService<IJobService>();
            lifetime.ApplicationStarted.Register(() => { service.StartAsync(default); });

            lifetime.ApplicationStopping.Register(() => { service.StopAsync(default); });
        }


    }
}
