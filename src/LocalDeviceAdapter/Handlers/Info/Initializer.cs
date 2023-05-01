using System;

namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Initializer : IHandlerInitializer
    {
        private readonly Func<Handler> _createHandler;

        public Initializer(
            Func<Handler> createHandler)
        {
            this._createHandler = createHandler;
        }

        public string Path => "/Info";

        public IHandler CreateHandler()
        {
            return this._createHandler();
        }
    }
}
