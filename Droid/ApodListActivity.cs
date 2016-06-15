
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HuePod.Nasa;

namespace HuePod.Droid
{
	[Activity(Label = "Huepod", MainLauncher = true, Icon = "@mipmap/icon")]
	public class ApodListActivity : Activity
	{
		Service _service;
		ListView _apodsListView;
		List<Apod> _apods;

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.ApodMainList);

			FindViews();

			_service = new Service();

			await LoadLastPictures();

			WireEvents();
		}

		void FindViews()
		{
			_apodsListView = FindViewById<ListView>(Resource.Id.apodsListView);
		}

		void WireEvents()
		{
			_apodsListView.ItemClick += (sender, e) => 
			{
				Intent i = new Intent(this, typeof(ApodDetailActivity));
				i.PutExtra("date", _apods[e.Position].Date.ToString());
				StartActivity(i);
			};
		}

		async Task LoadLastPictures()
		{
			_apods = await _service.GetLastAstronomicPictures();
			var adapter = new ApodListAdapter(this, _apods.ToArray());

			_apodsListView.Adapter = adapter;
		}
}
}

