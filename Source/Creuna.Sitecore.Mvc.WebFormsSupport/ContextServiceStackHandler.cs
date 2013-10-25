using Sitecore.Mvc.Common;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    /// <summary>
    /// Enables capture and resume of context based information. Inherits from ContextService only to get access to the protected members.
    /// Since ContextService is a wrapper around static methods it works.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContextServiceStackHandler<T> : ContextService, IStackHandler where T : class
    {
        private T[] _value;
        public void Capture()
        {
            _value = GetStack<T>().ToArray();
        }

        public void Resume()
        {
            var list = GetStack<T>();
            list.Clear();
            list.AddRange(_value);
        }
    }
}