using Microsoft.Extensions.DependencyInjection;
using Quartz.Core.Aop.Extension.Interface;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz.Core.Aop.Extension
{
    public class JobAdapter : IJob, IDisposable
    {

        IServiceProvider _provider;

        public JobAdapter(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void Dispose()
        {
            _provider = null;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using IServiceScope scope = _provider.CreateScope();
            var jobType = context.JobDetail.JobType;
            var job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;
            await job.Execute(context);
        }
    }

    /// <summary>
    /// 此处有争议. 但不这样写,高频调用会报错!!!!  
    /// </summary>
    [DisallowConcurrentExecution]
    public class JobRunner : IJob
    {
        IEnumerable<IJobRepository> _repos;

        public JobRunner(IEnumerable<IJobRepository> repos)
        {
            Console.WriteLine("JobRunner Create");
            _repos = repos;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var jobData = context.JobDetail.JobDataMap;//获取Job中的参数

            var MethodName = jobData.GetString("MethodName");
           
            var classname = jobData.GetString("ClassName");
           
            var rep = _repos.FirstOrDefault(r => r.RepositoryName == classname);

            var methodinfo = rep.GetType().GetMethod(MethodName);

            return Task.Run(() =>
            {
                methodinfo.Invoke(rep, null);
            });
        }
    }

    public class JobAdapterFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public JobAdapterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            { 
                return _serviceProvider.GetRequiredService<JobAdapter>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }

        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
