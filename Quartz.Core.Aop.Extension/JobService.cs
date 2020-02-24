using Quartz.Core.Aop.Extension.Attributes;
using Quartz.Core.Aop.Extension.Interface;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Core.Aop.Extension
{
    public class JobService : IJobService
    {
        readonly ISchedulerFactory _schedulerFactory;
        IScheduler _scheduler;
        readonly IJobFactory _jobFactory;
    

        public JobService(ISchedulerFactory SchedulerFactory, IJobFactory jobfactory)
        {
            _schedulerFactory = SchedulerFactory;
            _jobFactory = jobfactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var jobAttributeType = typeof(ScheduleAttribute);
            var JobRepositoryInterface = typeof(IJobRepository);

            ITrigger trigger;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //1、通过调度工厂获得调度器
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;
          
            foreach (Assembly assembly in assemblies)
            {
                assembly.GetTypes().Where(p => p.GetInterfaces().Contains(JobRepositoryInterface)).AsParallel().ForAll(async Repository =>
                {
                    
                    foreach (MethodInfo method in Repository.GetMethods().Where(p => p.IsDefined(jobAttributeType, true)).ToArray())
                    {
                        var jobAttr = (ScheduleAttribute)method.GetCustomAttributes(jobAttributeType, true).FirstOrDefault();
                        //3、匹配一个触发器 这里随便改你想要的频率
                        var cron = jobAttr.Cron switch
                        {
                            ScheduleCron.Test => "0/20 * * * * ? *",
                            ScheduleCron.HighFrequency => "0 */3 * * * ?",
                            ScheduleCron.Weekly => "0 0 1 1 * ?",
                            _ => "0 0 1 * * ?",
                        };
                        trigger = TriggerBuilder.Create().WithCronSchedule(cron).Build();
                        string fullname = $"{Repository.FullName}.{method.Name}";
                        IDictionary<string, object> dic = new Dictionary<string, object>
                        {
                            { "ClassName", Repository.FullName },
                            { "MethodName", method.Name },
                            { "fullname", fullname }
                        };

                        //4、创建任务
                        var jobDetail = JobBuilder
                            .Create<JobRunner>()
                            .SetJobData(new JobDataMap(dic))                         
                            .WithIdentity(fullname)
                            .WithDescription($"Job[{fullname}] Execute!")
                            .Build();

                        //5、将触发器和任务器绑定到调度器中
                        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                    }
                });
            }

            await _scheduler.Start(cancellationToken);  //2、开启调度器
        }

   
        public async Task PuaseScheduler(CancellationToken cancellationToken = default)
        {
            await _scheduler.PauseAll(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler?.Shutdown(cancellationToken);
        }

        public void Dispose()
        {
            if (_scheduler != null) _scheduler.Shutdown();
            Console.WriteLine(nameof(JobService) + ":" + nameof(Dispose));

        }
    }
}
