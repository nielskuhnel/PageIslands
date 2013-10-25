using System.IO;
using System.Web.UI;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    internal class ScopeInfo
    {
        public Control ControlTarget { get; set; }
        public TextWriter Writer { get; set; }
    }
}