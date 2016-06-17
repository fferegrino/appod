using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Square.Picasso;

namespace HuePod.Droid
{
    [Activity(Label = "POD detail")]
    public class ApodDetailActivity : Activity
    {
        
        private TextView _descriptionView;
		TextView _copyrightView;

        private ImageView _mainApodView;

        private Service _service;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			MakeZenMode();

			SetContentView(Resource.Layout.apod_detail_activity);

            FindViews();

            _service = new Service();

            if (Intent.Extras != null)
            {
                var date = DateTime.Parse(Intent.Extras.GetString("date"));
                var apod = await _service.GetAstronomicPictureOf(date);
                _descriptionView.Text = apod.Explanation;
				ActionBar.Title = apod.Date.ToShortDateString();
				if (apod.Copyright != null)
				{
					_copyrightView.Text = apod.Copyright;
					_copyrightView.Visibility = ViewStates.Visible;
				}
                Picasso.With(this).Load(apod.Url).Into(_mainApodView);
            }
        }

		private void FindViews()
        {
            _mainApodView = FindViewById<ImageView>(Resource.Id.mainApodView);
            _descriptionView = FindViewById<TextView>(Resource.Id.descriptionView);
			_copyrightView = FindViewById<TextView>(Resource.Id.copyrightView);
        }

		void MakeZenMode()
		{
			var decorView = Window.DecorView;
			var uiOptions = (int)decorView.SystemUiVisibility;
			var newUiOptions = uiOptions;
			newUiOptions |= (int)SystemUiFlags.LowProfile;
			newUiOptions |= (int)SystemUiFlags.Fullscreen;
			newUiOptions |= (int)SystemUiFlags.HideNavigation;
			newUiOptions |= (int)SystemUiFlags.Immersive;
			decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
		}

		public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.detail_menu, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
		{
			if (item.ItemId == Resource.Id.hide_ui_menu)
			{
				MakeZenMode();
				return true;
			}
			return base.OnOptionsItemSelected(item);
		}
    }
}