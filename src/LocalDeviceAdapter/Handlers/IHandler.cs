namespace LocalDeviceAdapter.Handlers
{
    public interface IHandler
    {
        (bool success, object answer) Process(RemoteCommand command);
    }
}