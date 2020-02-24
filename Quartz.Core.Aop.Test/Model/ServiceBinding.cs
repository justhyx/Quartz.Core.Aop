using Microsoft.Extensions.DependencyInjection;
using Quartz.Core.Aop.Extension;
using Quartz.Core.Aop.Extension.Interface;
using Quartz.Core.Aop.Test.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz.Core.Aop.Test.Model
{
    public static class ServiceBinding
    {
        public static void JobClassBinding(this IServiceCollection services)
        {
            services.AddScoped<IJobRepository,ServerRepository>();
        }
    }
}
