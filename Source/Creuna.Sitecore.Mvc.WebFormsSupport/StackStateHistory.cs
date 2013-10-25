using System;
using System.Linq;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    internal class StackStateHistory
    {
        private IStackHandler[] _state;

        public StackStateHistory()
        {
            _state = StackState.Handlers.Select(x => x()).ToArray();
            foreach (var handler in _state)
            {
                handler.Capture();
            }
        }

        public IDisposable Resume()
        {
            return new Overrider(this);
        }

        class Overrider : IDisposable
        {
            private readonly StackStateHistory _original;

            public Overrider(StackStateHistory owner)
            {
                _original = new StackStateHistory();
                foreach (var handler in owner._state)
                {
                    handler.Resume();
                }
            }

            public void Dispose()
            {
                foreach (var handler in _original._state)
                {
                    handler.Resume();
                }
            }
        }
    }
}