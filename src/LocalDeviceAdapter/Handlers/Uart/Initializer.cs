namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Initializer : IHandlerInitializer
    {
        public string Path => "/Uart";

        public IHandler CreateHandler()
        {
            return new Handler();
        }
    }
}