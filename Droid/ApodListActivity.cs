using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using HuePod.Nasa;

namespace HuePod.Droid
{
    [Activity(Label = "Huepod", MainLauncher = true, Icon = "@mipmap/icon")]
    public class ApodListActivity : Activity
    {
        private List<Apod> _apods;
        private ListView _apodsListView;
        private Service _service;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ApodMainList);

            FindViews();

            _service = new Service();

            await LoadLastPictures();

            WireEvents();
        }

        private void FindViews()
        {
            _apodsListView = FindViewById<ListView>(Resource.Id.apodsListView);
        }

        private void WireEvents()
        {
            _apodsListView.ItemClick += (sender, e) =>
            {
                var i = new Intent(this, typeof(ApodDetailActivity));
                i.PutExtra("date", _apods[e.Position].Date.ToString());
                StartActivity(i);
            };
        }

        private async Task LoadLastPictures()
        {
            _apods = await _service.GetLastAstronomicPictures();
            var adapter = new ApodListAdapter(this, _apods.ToArray());

            _apodsListView.Adapter = adapter;
        }
    }
}