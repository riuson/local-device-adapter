namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Initializer : IHandlerInitializer
    {
        public string Path => "/Info";

        public IHandler CreateHandler()
        {
            return new Handler();
        }
    }
}