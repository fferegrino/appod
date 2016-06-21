
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace HuePod.Droid
{
	[Activity(Label = "About",
		Theme = "@style/ApodTheme")]
	public class ApodAboutActivity : AppCompatActivity
	{
		public static Typeface CustomFont;

		TextView _aboutTextView;
		TextView _authorTextView;
		TextView _sourceTextView;
		TextView _titleTextView;

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
			_titleTextView.Typeface = CustomFont;
			_sourceTextView.Typeface = CustomFont;

			_sourceTextView.Click += (sender, e) =>
			{
				StartActivity(
					new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://github.com/fferegrino/appod")));
			};

			_authorTextView.Click += (sender, e) =>
			{
				StartActivity(
					new Intent(Intent.ActionView, Android.Net.Uri.Parse("http://thatcsharpguy.com")));
			};

			_authorTextView.TextFormatted = Android.Text.Html.FromHtml(Resources.GetString(Resource.String.author_text));
			_sourceTextView.TextFormatted = Android.Text.Html.FromHtml(Resources.GetString(Resource.String.source_code_text));
		}

		void FindViews()
		{
			_aboutTextView = FindViewById<TextView>(Resource.Id.aboutTextView);
			_authorTextView = FindViewById<TextView>(Resource.Id.authorTextView);
			_titleTextView = FindViewById<TextView>(Resource.Id.titleTextView);
			_sourceTextView = FindViewById<TextView>(Resource.Id.sourceTextView);
		}
}
}

