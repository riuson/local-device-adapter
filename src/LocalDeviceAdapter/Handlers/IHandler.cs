using System;

namespace LocalDeviceAdapter.Handlers
{
    public interface IHandler : IDisposable
    {
        (bool success, object answer) Process(RemoteCommand command);
    }
}