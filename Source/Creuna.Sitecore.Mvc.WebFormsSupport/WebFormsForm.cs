using System;
using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public class WebFormsForm : IDisposable
    {
        private readonly WebFormsMvcContext _context;
        private readonly WebPageBase _container;
        private readonly TextWriter _output;
        private readonly StringWriter _capturingWriter;


        public WebFormsForm(HtmlHelper helper, string formId)
        {
            _context = WebFormsMvcContext.GetCurrent(helper);
            _container = helper.ViewDataContainer as WebPageBase;
            _output = helper.ViewContext.Writer;
            if (_container != null)
            {
                _container.OutputStack.Push(_capturingWriter = new StringWriter());
            }            
            
            _context.TryEnter(_capturingWriter, formId);
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
                try
                {
                    if (_container != null)
                    {
                         _container.OutputStack.Pop();                        
                    }
                }
                finally
                {
                    _context.TryExit(_output);
                }
            }
        }

        public void EndForm()
        {
            Dispose(true);
        }
    }
}