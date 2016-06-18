
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HuePod.Droid
{
	[Activity(Label = "ApodAboutActivity",
		Theme = "@style/ApodTheme")]
	public class ApodAboutActivity : Activity
	{
		public static Typeface CustomFont;

		TextView _aboutTextView;
		TextView _authorTextView;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.apod_about_activity);

			FindViews();

			if (CustomFont == null)
			{
				CustomFont = Typeface.CreateFromAsset(Assets, "fonts/Tinos-Regular.ttf");
			}

			_aboutTextView.Typeface = CustomFont;
			_authorTextView.Typeface = CustomFont;

			_authorTextView.TextFormatted = Android.Text.Html.FromHtml(Resources.GetString( Resource.String.author_text));
		}

		void FindViews()
		{
			_aboutTextView = FindViewById<TextView>(Resource.Id.aboutTextView);
			_authorTextView = FindViewById<TextView>(Resource.Id.authorTextView);
		}
}
}

