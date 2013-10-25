using System;
using System.IO;
using System.Web;

namespace Creuna.Sitecore.Mvc.WebFormsSupport
{
    public static class HttpResponseCaptureUtil
    {
        public static string CaptureOutput(this HttpContext ctx, IHttpHandler page)
        {
            var old = ctx.Response.Output;
            try
            {
                var writer = new StringWriter();                
                ctx.Response.Output = writer;
                page.ProcessRequest(ctx);
                return writer.ToString();
            }
            finally
            {
                ctx.Response.Output = old;
            }
        }
        
        /// <summary>
        /// NOTE! Doesn't work with redirects, response end etc. because Response.Flush is called
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string CaptureOutput(this HttpContext ctx, Action<HttpContext> action)
        {            
            ctx.Response.Flush();
            
            var currentFilter = ctx.Response.Filter;
            using (var outputStream = new MemoryStream())
            {
                try
                {
                    ctx.Response.Filter = outputStream;
                    action(ctx);
                    ctx.Response.Flush();

                    outputStream.Position = 0;
                    return ctx.Response.ContentEncoding.GetString(outputStream.ToArray());
                }
                finally
                {
                    ctx.Response.Filter = currentFilter;
                }
            }
        }
    }
}