using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Server
{
    public interface IBackgroundProcess
    {
        Task Execute(BackgroundProcessContext context);
    }
}
