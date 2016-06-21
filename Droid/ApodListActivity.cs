using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using HuePod.Nasa;

namespace HuePod.Droid
{
	[Activity(Label = "Appod", 
        MainLauncher = true, 
        Icon = "@mipmap/icon", 
        Theme = "@style/ApodTheme")]
	public class ApodListActivity : AppCompatActivity
    { 
		private List<Apod> _apods;
		private RecyclerView _apodsView;
		private Service _service;
		private FloatingActionButton _fab;

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			UserDialogs.Init(this);

			Title = "Astronomy Picture of the Day";

			SetContentView(Resource.Layout.apod_list_activity);

			FindViews();

			_service = new Service();

			await LoadLastPictures();



			WireEvents();
		}

		private void FindViews()
		{
			_apodsView = FindViewById<RecyclerView>(Resource.Id.apodsListView);
			_fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
		}

		private void WireEvents()
		{
			_fab.Click += (s, a) =>
			{
				var today = DateTime.Today;
				var picker = new DatePickerDialog(this, (ss, aa) =>
				{
					System.Diagnostics.Debug.WriteLine($"{aa.Date:dd/MM/yyyy} - {today:dd/MM/yyyy}");
					if (aa.Date <= today)
					{
						StartDetailActivity(aa.Date);
					}
					else
					{
						var t = Toast.MakeText(this, "Oh! I can't predict the future", ToastLength.Short);
						t.Show();
					}

				}, today.Year, today.Month - 1, today.Day);
				picker.Show();
			};
		}

		void StartDetailActivity(DateTime date)
		{
			var i = new Intent(this, typeof(ApodDetailActivity));
			i.PutExtra("date", date.ToString());
			StartActivity(i);
		}

		private async Task LoadLastPictures()
		{
			_apods = await _service.GetLastAstronomicPictures(10);

			var adapter = new ApodsAdapter(this, _apods.ToArray());

			adapter.ApodClick += (sender, position) =>
			{
				var apod = _apods[position];
				if (apod.MediaType == "image")
				{
					StartDetailActivity(apod.Date);
				}
				else 
				{
					StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(apod.Url)));
				}
			};

			_apodsView.SetAdapter( adapter);
			_apodsView.SetLayoutManager(new LinearLayoutManager(this));
		}

		public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
		{
			
			MenuInflater.Inflate(Resource.Menu.main_menu, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
		{
			//if (item.ItemId == Resource.Id.open_calendar_menu)
			//{
			//	var today = DateTime.Today;
			//	var picker = new DatePickerDialog(this, (s, a) =>
			//	{
			//		System.Diagnostics.Debug.WriteLine($"{a.Date:dd/MM/yyyy} - {today:dd/MM/yyyy}");
			//		if (a.Date <= today)
			//		{
			//			StartDetailActivity(a.Date);
			//		}
			//		else 
			//		{
			//			var t = Toast.MakeText(this, "Oh! I can't predict the future", ToastLength.Short);
			//			t.Show();
			//		}

			//	}, today.Year, today.Month - 1, today.Day);
			//	picker.Show();
			//	return true;
			//}

			if (item.ItemId == Resource.Id.view_about_menu)
			{
				var i = new Intent(this, typeof(ApodAboutActivity));
				StartActivity(i);
				return true;
			}
			return base.OnOptionsItemSelected(item);
		}
	}
}