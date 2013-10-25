using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Web.UI.WebControls;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public static class HtmlExtensions
    {
        public static WebFormsForm BeginFormsContext(this HtmlHelper helper, string formId = null)
        {
            return new WebFormsForm(helper, formId);
        }

        public static HtmlString RenderControl(this HtmlHelper helper, Control ctrl)
        {            
            WebFormsMvcContext.GetCurrent(helper).AddControl(ctrl);

            return new HtmlString("");            
        }

        public static HtmlString RenderUserControl(this HtmlHelper helper, string virtualPath)
        {
            var ctx = WebFormsMvcContext.GetCurrent(helper);
            return helper.RenderControl(ctx.Page.LoadControl(virtualPath));
        }

    }
}