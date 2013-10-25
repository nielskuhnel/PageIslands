using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Mvc.Presentation;
using Sitecore.Web.UI.WebControls;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public class WebFormsMvcContext
    {
        private static readonly string ItemKey = typeof(WebFormsMvcContext).FullName;

        private readonly HttpContext _httpContext;
        private int _nestingDepth;
        public bool Executing { get; private set; }
        public Page Page { get; private set; }
        protected HtmlForm Form { get; private set; }
        internal readonly Stack<ScopeInfo> Scopes = new Stack<ScopeInfo>();

        public static WebFormsMvcContext GetCurrent(HtmlHelper helper)
        {
            return GetCurrent(helper.ViewContext.HttpContext.ApplicationInstance.Context);
        }

        public static WebFormsMvcContext GetCurrent(HttpContext ctx)
        {
            return (ctx.Items[ItemKey] ?? (ctx.Items[ItemKey] = new WebFormsMvcContext(ctx))) as WebFormsMvcContext;
        }

        public WebFormsMvcContext(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }


        internal string SliceOutput(bool addControl)
        {
            var writer = Scopes.Peek().Writer as StringWriter;
            if (writer != null)
            {
                var capturedOutput = writer.ToString();
                if (addControl && !string.IsNullOrEmpty(capturedOutput))
                {
                    Scopes.Peek().ControlTarget.Controls.Add(new LiteralControl(capturedOutput));
                }
                writer.GetStringBuilder().Clear();

                return capturedOutput;
            }

            return null;
        }

        public void AddControl(Control control)
        {
            SliceOutput(true);
            Scopes.Peek().ControlTarget.Controls.Add(control);
            StackState.Capture(control);

            if (Executing)
            {
                BuildControl(control);
            }
        }


        public bool IsActive
        {
            get { return _nestingDepth > 0; }
        }

        public bool TryEnter(TextWriter writer, string scopeId = null)
        {
            //TryExit has been called setting nesting depth to 0, and Page is being processed. Don't do anything.
            if (Executing) return false;


            if (_nestingDepth++ == 0 )
            {
                Page = new Page();
                Page.Controls.Add(Form = new NamingContainerForm());

                if (scopeId != null)
                {
                    Form.ID += "_" + scopeId.Replace("-", "");
                }
                Scopes.Push(new ScopeInfo { ControlTarget = Form, Writer = writer });
                return true;
            }

            return false;
        }



        public bool TryExit(TextWriter writer)
        {
            //TryExit has been called setting nesting depth to 0, and Page is being processed. Don't do anything.
            if (Executing) return false;

            if (--_nestingDepth == 0 )
            {
                try
                {
                    if (Form.Controls.Count > 0)
                    {
                        SliceOutput(true);
                        Executing = true;
                        using (new SitecoreContextPageSwitcher(Page))
                        {
                            InitializeEventHandlers();
                            var output = _httpContext.CaptureOutput(Page);
                            writer.Write(output);
                        }
                    }
                    else
                    {
                        writer.Write(SliceOutput(false));
                    }
                }
                finally
                {
                    Page.Dispose();
                    _httpContext.Items.Remove(ItemKey);
                    Executing = false;
                }

                return true;
            }

            return false;
        }

        private void InitializeEventHandlers()
        {
            switch (Settings.LayoutPageEvent)
            {
                case "preInit":
                    Page.PreInit += BuildPage;
                    break;
                case "init":
                    Page.Init += BuildPage;
                    break;
                case "load":
                    Page.Load += BuildPage;
                    break;
            }
        }

        private void BuildPage(object sender, EventArgs e)
        {
            BuildControl(Page);
        }

        private static void BuildControl(Control ctrl)
        {
            Context.Page.Expand(ctrl);
            ReplaceAndRenderPlaceholders(ctrl);
        }

        internal static void ReplaceAndRenderPlaceholders(Control parent)
        {
            foreach (var placeholder in FindControls<Placeholder>(parent).ToArray())
            {
                var path = PlaceholderContext.Current.PlaceholderPath;
                var mvcPlaceholder = new MvcPlaceholder() { Key = placeholder.Key };
                placeholder.Controls.Add(mvcPlaceholder);

                mvcPlaceholder.Expand();
            }
        }

        private static IEnumerable<T> FindControls<T>(Control parent) where T : Control
        {
            foreach (Control c in parent.Controls)
            {
                var tc = c as T;
                if (tc != null) yield return tc;
                else
                {
                    foreach (var cc in FindControls<T>(c))
                    {
                        yield return cc;
                    }
                }
            }
        }

        public class NamingContainerForm : HtmlForm, INamingContainer
        {

        }
    }
}