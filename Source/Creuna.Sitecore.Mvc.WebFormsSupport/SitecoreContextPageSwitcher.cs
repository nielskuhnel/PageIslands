using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Sitecore;
using Sitecore.Layouts;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    class SitecoreContextPageSwitcher : IDisposable
    {
        static FieldInfo _contextSitePageSetter = typeof(PageContext).GetField("page", BindingFlags.Instance | BindingFlags.NonPublic);

        private Page _oldPage;

        public SitecoreContextPageSwitcher(Page page)
        {            
            _oldPage = Context.Site.Page.Page;
            //Shhh... This is required because Sitecore.Web.UI.WebControls uses Context.Site.Page to load user controls
            //          (Context.Site.Page is (normally) null in MVC Context and it doesn't have a (public) setter
            //          Here it is set.
            _contextSitePageSetter.SetValue(Context.Site.Page, page);
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                //Set it back
                _contextSitePageSetter.SetValue(Context.Site.Page, _oldPage);

            }
        }
    }
}
