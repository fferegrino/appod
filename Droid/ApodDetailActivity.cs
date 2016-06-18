using System;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using HuePod.Nasa;
using Java.Net;
using Square.Picasso;
using Environment = Android.OS.Environment;
using File = Java.IO.File;

namespace HuePod.Droid
{
    [Activity(Label = "POD detail",
        Theme = "@style/ApodTheme")]
    public class ApodDetailActivity : Activity
    {

        public static Typeface CustomFont;
        private TextView _descriptionView;
        TextView _copyrightView;
        private Apod _apod;

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
            _apod = apod;
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
            if (item.ItemId == Resource.Id.save_apod)
            {
                var permission = ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);

                if (permission != Permission.Granted)
                {
                    // We don't have permission so prompt the user
                    ActivityCompat.RequestPermissions(
                        this, new string[] { Manifest.Permission.WriteExternalStorage},10);
                }
                else
                {
                    var saveTarget = new SaveTarget(this, _apod.Date);
                    Picasso.With(this)
                        .Load(_apod.HdUrl)
                        .Into(saveTarget);
                    return true;
                }
            }
            return base.OnOptionsItemSelected(item);
        }

    }

    class SaveTarget : Java.Lang.Object, ITarget
    {
        private Context _context;
        private readonly DateTime _date;

        public SaveTarget(Context context, DateTime date)
        {
            _context = context;
            _date = date;
        }

        public void OnBitmapFailed(Drawable p0)
        {
            Toast.MakeText(_context, "Unable to download", ToastLength.Short).Show();
        }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
			var fileName = $"{_date:yyyyMMdd}.jpg";
			System.Diagnostics.Debug.WriteLine($"Downloading to {fileName}");

            var apodDir = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "APOD");
            var dir = new File(apodDir);

            if (!dir.Exists())
            {
                dir.Mkdirs();
            }

			System.Diagnostics.Debug.WriteLine($"Directory: {dir.AbsolutePath}");
            var filePath = System.IO.Path.Combine(apodDir, fileName);
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				p0.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
				System.Diagnostics.Debug.WriteLine($"Compressed: {dir.AbsolutePath}");
			}
            Toast.MakeText(_context, $"Saved to {filePath}", ToastLength.Short).Show();
        }

        public void OnPrepareLoad(Drawable p0)
        {
            Toast.MakeText(_context, "Downloading image", ToastLength.Short).Show();
        }
    }
}