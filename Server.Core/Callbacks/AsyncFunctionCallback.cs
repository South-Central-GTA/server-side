using System;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Elements.Pools;

namespace Server.Core.Callbacks;

public class AsyncFunctionCallback<T> : IAsyncBaseObjectCallback<T> where T : IBaseObject
{
    private readonly Func<T, Task> _callback;

    public AsyncFunctionCallback(Func<T, Task> callback)
    {
        _callback = callback;
    }

    public Task OnBaseObject(T baseObject)
    {
        return _callback(baseObject);
    }
}