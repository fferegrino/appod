using System;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Square.Picasso;

namespace HuePod.Droid
{
    [Activity(Label = "POD detail",
		Theme = "@style/ApodTheme")]
    public class ApodDetailActivity : Activity
    {

		public static Typeface CustomFont;
        private TextView _descriptionView;
		TextView _copyrightView;

        private ImageView _mainApodView;

        private Service _service;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.apod_detail_activity);

            FindViews();

            _service = new Service();

			if (CustomFont == null)
			{
				CustomFont = Typeface.CreateFromAsset(Assets, "fonts/Tinos-Regular.ttf");
			}

            if (Intent.Extras != null)
            {
				await LoadApod(Intent.Extras);
            }

			Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

		private void FindViews()
        {
            _mainApodView = FindViewById<ImageView>(Resource.Id.mainApodView);
            _descriptionView = FindViewById<TextView>(Resource.Id.descriptionView);
			_copyrightView = FindViewById<TextView>(Resource.Id.copyrightView);
        }


		async Task LoadApod(Bundle extras)
		{
			var date = DateTime.Parse(extras.GetString("date"));
			var apod = await _service.GetAstronomicPictureOf(date);

			_descriptionView.Text = apod.Explanation;
			_descriptionView.Typeface = CustomFont;

			ActionBar.Title = apod.Date.ToShortDateString();
			if (apod.Copyright != null)
			{
				_copyrightView.Text = "Copyright: " + apod.Copyright;
				_copyrightView.Typeface = CustomFont;
				_copyrightView.Visibility = ViewStates.Visible;
			}
			else
			{
				_copyrightView.Visibility = ViewStates.Invisible;
			}

			if (apod.MediaType == "image")
			{
				Picasso.With(this).Load(apod.Url).Into(_mainApodView);
			}
		}

		void MakeZenMode()
		{
			var decorView = Window.DecorView;

			var flags = SystemUiFlags.LayoutStable
									 | SystemUiFlags.LayoutHideNavigation
									 | SystemUiFlags.LayoutFullscreen
									 | SystemUiFlags.HideNavigation
									 | SystemUiFlags.Fullscreen
									 | SystemUiFlags.Immersive;
			
			decorView.SystemUiVisibility = (StatusBarVisibility)flags;
			_descriptionView.Visibility = ViewStates.Invisible;
		}


		void DecorView_SystemUiVisibilityChange(object sender, View.SystemUiVisibilityChangeEventArgs e)
		{
			if (e.Visibility == StatusBarVisibility.Visible)
			{
				_descriptionView.Visibility = ViewStates.Visible;
				var flags = SystemUiFlags.LayoutStable;
				Window.DecorView.SystemUiVisibility = (StatusBarVisibility)flags;
			}
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