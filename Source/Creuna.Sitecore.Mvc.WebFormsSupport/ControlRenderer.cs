using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore.Mvc.Presentation;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public class ControlRenderer : Renderer
    {
        private readonly Control _control;
        private readonly string _id;

        public ControlRenderer(Control control, string id)
        {            
            _control = control;
            _id = id;
        }

        public override void Render(TextWriter writer)
        {
            var ctx = WebFormsMvcContext.GetCurrent(HttpContext.Current);
            //If there is no active WebFormsMvcContext the writer must be null. Otherwise the content of the writer is appended to the new form.
            ctx.TryEnter(ctx.IsActive ? writer : null, _id);
            try
            {               
                ctx.AddControl(_control);
            }
            finally
            {
                ctx.TryExit(writer);
            }
        }        
    }
}
