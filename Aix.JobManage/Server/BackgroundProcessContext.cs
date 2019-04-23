using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aix.JobManage.Server
{
   public class BackgroundProcessContext
    {
        public BackgroundProcessContext(CancellationToken cancellationToken, BackgroundJobServerOptions options)
        {
            CancellationToken = cancellationToken;
            Options = options;
        }
        public CancellationToken CancellationToken { get; }

        public BackgroundJobServerOptions Options { get; }

        public bool IsShutdownRequested => CancellationToken.IsCancellationRequested;
       
    }
}
