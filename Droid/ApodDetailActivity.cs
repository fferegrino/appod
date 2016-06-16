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
        private Button _loadApodButton;

        private ImageView _mainApodView;

        private Service _service;
        private int count = 1;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var decorView = Window.DecorView;
            var uiOptions = (int) decorView.SystemUiVisibility;
            var newUiOptions = uiOptions;
            newUiOptions |= (int) SystemUiFlags.LowProfile;
            newUiOptions |= (int) SystemUiFlags.Fullscreen;
            newUiOptions |= (int) SystemUiFlags.HideNavigation;
            newUiOptions |= (int) SystemUiFlags.Immersive;
            decorView.SystemUiVisibility = (StatusBarVisibility) newUiOptions;

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

        private void FindViews()
        {
            _mainApodView = FindViewById<ImageView>(Resource.Id.mainApodView);
            _descriptionView = FindViewById<TextView>(Resource.Id.descriptionView);
        }
    }
}