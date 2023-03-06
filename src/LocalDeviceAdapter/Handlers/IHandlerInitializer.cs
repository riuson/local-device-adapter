namespace LocalDeviceAdapter.Handlers
{
    public interface IHandlerInitializer
    {
        string Path { get; }

        IHandler CreateHandler();
    }
}