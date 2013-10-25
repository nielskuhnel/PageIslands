Web Forms support in Sitecore MVC (Page Islands)
================================================

## What it does ##

This package enables ASP.NET Web Forms based components to be used in MVC.NET context in Sitecore. 
In this way web forms based modules like Web Forms for Marketers and legacy components can be used in MVC based Sitecore solutions.
As web forms components can now be used in MVC layouts it is more tractable to migrate an existing Sitecore solution to MVC as the existing components can be reused initially and then ported gradually to MVC.

[Charles Turano](http://www.hhogdev.com/Blog/2012/December/MVC-WebForms.aspx) has already devised a nice solution for using MVC components in Web Forms based layouts. With this package allowing the opposite, developers now have two approaches that compliments each other nicely when migrating an existing forms based solution to MVC.

In this way more projects can hopefully benefit from the joy of MVC.

The package makes it possible to add classes that derive from System.Web.UI.Control in MVC views, either directly 
using HtmlHelper extensions or by means of placeholders using Sitecore's rendering engine. For the latter case a processor for the "mvc.getRenderer" pipeline is included.
Two important cases of controls are sub layouts based on user controls and control based renderings like those from Web Forms For Marketers.

When controls define placeholders (\<sc:Placeholder /\>) the package replaces them, and the MVC rendering pipeline takes over. This allows controls and MVC flavored renderings to be added to each other's placeholders in any combination as the editor would expect.

The package is new, and there may be some edge cases where adjustments are necessary. For instance the controls may rely on information from parent containers like the Page that used to be the layout. In these cases the MVC APIs can be used to obtain the required context information, but it may require refactoring.
Of course, it may also contain bugs. Any feedback is appreciated.


## How to use ##

Add "Creuna.Sitecore.Mvc.WebFormsSupport.dll" to you bin folder, and `<processor type="Creuna.Sitecore.Mvc.WebFormsSupport.GetControlRenderer, Creuna.Sitecore.Mvc.WebFormsSupport" />` to the mvc.getRenderer pipeline and you are good to go. You can also just install the package from this repository ("Web Forms support in Sitecore MVC.zip").

If you intend to use multiple controls in the same layout you should wrap the places where Web Forms components may occur in your views with

```
@using (Html.BeginFormsContext())
{
		...
}
```

If no controls are present the code above does nothing. If they are, the content is wrapped in a form with hidden fields for viewstate, scripts and all the other things a page normally generates.

If you need to include a Control or User Control statically in your view you can simply write

```
@Html.RenderUserControl("path-to-user-control.ascx")
@Html.RenderControl(new MyLegacyControl())
```

Since these methods are extension methods you need to include `@using Creuna.Sitecore.Mvc.WebFormsSupport` in the top of your view.

## How it works ##
MVC and Web Forms represent two very different paradigms in web development and they are incompatible. On an overall level, MVC takes a request stream, generates a representation of the data needed for the response (the model) and renders it sequentially back to a stream without looking back (view). In Web Forms you have interacting controls with states and events, and the request and response streams are (ideally) abstracted away in a lower tier. Conceptually the controls both exist in markup and on the server, and all the plumbing required to make this work is exactly why MVC and web forms are incompatible. However, they have the ASP.NET stack in common and this package works by exploiting some low-level features of ASP.NET based on the following observations:

  1. A Page is required to host the controls in Web Forms
  2. A Page is not present in MVC context
  3. A Page is an IHttpHandler with the method ProcessRequest (it is hidden from IntelliSense with [EditorBrowsable(EditorBrowsableState.Never)], but it's there)
  4. HttpResponse.Output is a TextWriter that has a public setter, thus it can be redirected and captured.
  
Creating a page and a form to host Web Forms controls is actually quite easy, `var page = new Page(); page.Controls.Add(new Form());`. Unfortunately that is not enough to make it go through its life cycle, wake up its controls and do all the other magic that happens behind the scenes when you request a page. Normally when a page is requested it is assigned to the HttpContext’s Handler property and then later its ProcessRequest method is called, making it render to the HttpContext’s Response. This is also what this package does. 

Unfortunately the output of a view is collected in a TextWriter so if we just call ProcessRequest on the Page it is rendered to the Response’s output stream before any output from the view, so we need to capture the output from the page and write it to the view's TextWriter. Since HttpResponse.Output has a public setter that is in fact quite easy, since we can just map it to a temporary StringWriter and set the output back to whatever it was when we’re done rendering the page. As the page thinks it was just requested normally, hidden fields for viewstate, scripts etc. is now in the StringWriter and this can be written in the view. Furthermore, the page is rendered to the current HttpContext’s  HttpResponse so Response.Redirect, Response.End and setting cookies works.

Using this approach rendering a single control is quite easy. However, things get a little more complicated if multiple controls are used in a view. The simple approach would create separate forms but then the other controls would lose their viewstates on postback. To solve this the controls are collected and added to the same page together with normal output from the view. LiteralControls with the view content is added between the other controls on the page, and generally this approach requires quite some bookkeeping, stacks, resetting StringWriters and some other plumbing, but it works and is implemented in the package. It is not completely transparent since Sitecore internally uses stacks to keep track of the state of placeholders, renderings and other aspects of the current state. The controls are added to the page in the correct context but when they are rendered later the context is wrong. To remediate this the different contexts are captured, and the class StackState can be used to resume them if they are needed in the controls by wrapping the code in `using( StackState.Resume(this) ) { }`.

Reflection is used to set Sitecore.Context.Page.Page. This is needed as Sitecore.Web.UI.WebControls.Sublayout use it to load user controls it doesn't have a public setter. 

## About performance ##
Since the page is rendered in the same request and no parsing is used the performance hit should be minimal, but it hasn't been extensively tested. Any feedback is highly appreciated.

  
  

\- Niels Kühnel (@nielskuhnel) 2013

The development is sponsored by [Creuna](http://www.creuna.dk)