using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Mvc.Helpers;
using Sitecore.Mvc.Presentation;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    /// <summary>
    /// Captures and resumes stack values. If you are using your own stack like context variables they must be registered here.
    /// </summary>
    public static class StackState
    {
        private static readonly string Key = typeof(StackState).FullName;
        
        public static List<Func<IStackHandler>> Handlers { get; set; }

        static StackState()
        {
            Handlers = new List<Func<IStackHandler>>()
                {
                    ()=>new ContextServiceStackHandler<ViewContext>(),
                    ()=>new ContextServiceStackHandler<PlaceholderContext>(),
                    ()=>new ContextServiceStackHandler<RenderingContext>()
                };
        }

        internal static Dictionary<object, StackStateHistory> CurrentHistory
        {
            get { return ThreadHelper.GetOrSetThreadData(Key, () => new Dictionary<object, StackStateHistory>()); }
            set { ThreadHelper.SetThreadData(Key, value); }
        }

        public static void Capture(object target)
        {
            CurrentHistory[target] =  new StackStateHistory();
        }

        public static IDisposable Resume(object target)
        {
            StackStateHistory history;
            if (!CurrentHistory.TryGetValue(target, out history))
            {
                throw new NullReferenceException("No stack state is recorded for target");
            }

            return history.Resume();
        }

        public static IDisposable Resume(Control target)
        {
            var c = target;
            while (c != null)
            {
                StackStateHistory history;
                if (CurrentHistory.TryGetValue(c, out history))
                {
                    return history.Resume();
                }
                c = c.Parent;
            }

            throw new NullReferenceException("No stack state is recorded for target");
        }
    }
}