using System;

namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Initializer : IHandlerInitializer
    {
        private readonly Func<Handler> _createHandler;

        public Initializer(Func<Handler> createHandler)
        {
            this._createHandler = createHandler;
        }

        public string Path => "/Uart";

        public IHandler CreateHandler()
        {
            return this._createHandler();
        }
    }
}