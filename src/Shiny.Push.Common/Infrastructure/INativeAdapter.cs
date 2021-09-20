using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Shiny.Push.Infrastructure
{
    public interface INativeAdapter
    {
        Task<PushAccessState> RequestAccess();
        Task UnRegister();

        Func<IReadOnlyDictionary<string, string>, Task>? OnReceived { get; set; }
        Func<string, Task>? OnTokenRefreshed { get; set; }
    }
}
