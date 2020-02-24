using Quartz.Core.Aop.Extension.Attributes;
using Quartz.Core.Aop.Extension.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz.Core.Aop.Test.Repository
{
    public class ServerRepository : IJobRepository
    {
        public string RepositoryName => typeof(ServerRepository).FullName;

        [Schedule(ScheduleCron.Test)]
        public void DoSomeMethod()
        {
            var d = DateTime.Now;
            Task.Delay(3000).ContinueWith(t =>
            {
                Debug.WriteLine(d.ToString(""));
            });

        }
    }
}
