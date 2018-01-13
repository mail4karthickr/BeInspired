using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;

namespace BeInspired
{
    [Activity(Label = "BeInspired", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private WebView webView;
        private ProgressDialog progressDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            webView = FindViewById<WebView>(Resource.Id.webView);
            webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            webView.SetWebViewClient(new HybridWebViewClient(this));

            // Render the view from the type generated from RazorView.cshtml
            var model = new Model1() { Text = "Text goes here" };
            var template = new RazorView() { Model = model };
            var page = template.GenerateString();

            // Load the rendered HTML into the view with a base URL 
            // that points to the root of the bundled Assets folder
        }

        protected override void OnResume()
        {
            webView.LoadUrl("https://www.beinspired.in/");
            base.OnResume();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if(e.Action == KeyEventActions.Down)
            {
                switch(keyCode)
                {
                    case Keycode.Back:
                        if(webView.CanGoBack())
                        {
                            webView.GoBack();
                        }
                        else
                        {
                            Finish();
                        }
                        return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        private class HybridWebViewClient : WebViewClient
        {
            private Activity mainActivity;
            private ProgressDialog progressDialog;

			public HybridWebViewClient(Activity mainActivity)
			{
				this.mainActivity = mainActivity;
			}
		
            public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
            {
				if (progressDialog == null)
				{
                    progressDialog = new ProgressDialog(mainActivity);
                    progressDialog.SetMessage("Loading..");	
				}
				if (!progressDialog.IsShowing)
					progressDialog.Show();
                base.OnPageStarted(view, url, favicon);
            }

            public override void OnPageFinished(WebView view, string url)
            {
                if (progressDialog != null && progressDialog.IsShowing)
						progressDialog.Dismiss();
				base.OnPageFinished(view, url);
            }

            public override bool ShouldOverrideUrlLoading(WebView webView, string url)
            {

                // If the URL is not our own custom scheme, just let the webView load the URL as usual
                var scheme = "hybrid:";

                if (!url.StartsWith(scheme))
                    return false;

                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(scheme.Length).Split('?');
                var method = resources[0];
                var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);

                if (method == "UpdateLabel")
                {
                    var textbox = parameters["textbox"];

                    // Add some text to our string here so that we know something
                    // happened on the native part of the round trip.
                    var prepended = $"C# says: {textbox}";

                    // Build some javascript using the C#-modified result
                    var js = $"SetLabelText('{prepended}');";

                    webView.LoadUrl("javascript:" + js);

				}

                return true;
            }
        }
    }
}
