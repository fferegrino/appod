using Android.App;
using Android.Widget;
using Android.OS;
using Square.Picasso;

namespace HuePod.Droid
{
	[Activity(Label = "POD detail")]
	public class MainActivity : Activity
	{
		int count = 1;

		ImageView _apodView;
		TextView _descriptionView;
		Button _loadApodButton;

		Service _service;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			FindViews();

			_service = new Service();

			_loadApodButton.Click += async (sender, e) =>
			{
				var apod = await _service.GetAstronomicPictureOf();
				_descriptionView.Text = apod.Date.ToShortDateString() +" - "+  apod.Explanation;
				Picasso.With(this).Load(apod.Url).Into(_apodView);
			};
		}

		void FindViews()
		{
			_apodView = FindViewById<ImageView>(Resource.Id.apodView);
			_descriptionView = FindViewById<TextView>(Resource.Id.descriptionView);
			_loadApodButton = FindViewById<Button>(Resource.Id.loadApodButton);
		}
	}
}


