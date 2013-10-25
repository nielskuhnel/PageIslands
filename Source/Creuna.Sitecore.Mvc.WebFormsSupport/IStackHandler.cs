using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sitecore.Common;
using Sitecore.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public interface IStackHandler
    {
        void Capture();

        void Resume();
    }
}
