using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Mvc.Pipelines.Response.GetRenderer;
using Sitecore.Mvc.Presentation;
using Sitecore.Web.UI.WebControls;
using Rendering = Sitecore.Mvc.Presentation.Rendering;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public class GetControlRenderer : GetRendererProcessor
    {
        public override void Process(GetRendererArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.Result != null)
                return;
                 
            
            args.Result = GetRenderer(args.Rendering, args);
        }

        protected virtual Renderer GetRenderer(Rendering rendering, GetRendererArgs args)
        {
            if (rendering.RenderingItem == null
                || rendering.RenderingItem.InnerItem == null) return null;

            var settings = new RenderingSettings();
            settings.DataSource = rendering.DataSource;            
            settings.Parameters = rendering["Parameters"];
            settings.Placeholder = rendering.Placeholder;
            settings.Caching = RenderingCaching.Parse(rendering.RenderingItem.InnerItem);
            settings.Conditions = rendering.RenderingItem.Conditions;
            settings.MultiVariateTest = rendering.RenderingItem.MultiVariateTest;
                        
            var ctrl = rendering.RenderingItem.GetControl(settings);
            if (ctrl != null)
            {                                         
                return new ControlRenderer(ctrl, ""+rendering.UniqueId);
            }

            return null;
        }
    }
}
