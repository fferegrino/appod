using Android.App;
using Android.Widget;
using Android.OS;
using Square.Picasso;
using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;

namespace HuePod.Droid
{
	[Activity(Label = "POD detail")]
	public class ApodDetailActivity : Activity
	{
		int count = 1;

		ImageView _mainApodView;
		TextView _descriptionView;
		Button _loadApodButton;

		Service _service;

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			View decorView = Window.DecorView;
			var uiOptions = (int)decorView.SystemUiVisibility;
			var newUiOptions = (int)uiOptions;
			newUiOptions |= (int)SystemUiFlags.LowProfile;
			newUiOptions |= (int)SystemUiFlags.Fullscreen;
			newUiOptions |= (int)SystemUiFlags.HideNavigation;
			newUiOptions |= (int)SystemUiFlags.Immersive;
			decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

			SetContentView(Resource.Layout.ApodDetail);

			FindViews();

			_service = new Service();

			if (Intent.Extras != null)
			{
				var date = DateTime.Parse(Intent.Extras.GetString("date"));
				var apod = await _service.GetAstronomicPictureOf(date);
				_descriptionView.Text = apod.Explanation;
				Picasso.With(this).Load(apod.Url).Into(_mainApodView);
			}
		}

		void FindViews()
		{
			_mainApodView = FindViewById< ImageView > (Resource.Id.mainApodView);
			_descriptionView = FindViewById<TextView>(Resource.Id.descriptionView);
		}
	}

}


