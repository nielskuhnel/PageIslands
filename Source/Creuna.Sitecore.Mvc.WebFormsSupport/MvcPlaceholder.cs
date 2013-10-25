using System.ComponentModel;
using System.IO;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Pipelines.Response.RenderPlaceholder;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    [ToolboxData("<{0}:Placeholder runat=server></{0}:Placeholder>")]
    public class MvcPlaceholder : Control, IExpandable
    {
        [Description("Unique name identifying the placeholder on the page")]
        private string _key;
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                _key = value.ToLowerInvariant();
            }
        }

        protected override void CreateChildControls()
        {
            var ctx = WebFormsMvcContext.GetCurrent(Context);

            using (var sw = new StringWriter())
            using (StackState.Resume(this))
            {
                ctx.Scopes.Push(new ScopeInfo { ControlTarget = this, Writer = sw });
                PipelineService.Get()
                               .RunPipeline("mvc.renderPlaceholder", new RenderPlaceholderArgs(Key, sw));


                ctx.SliceOutput(true);

                ctx.Scopes.Pop();
            }

        }

        public void Expand()
        {
            EnsureChildControls();
        }
    }
}