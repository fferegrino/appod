using System.Diagnostics;
using Android.App;
using Android.Views;
using Android.Widget;
using HuePod.Nasa;
using Square.Picasso;

namespace HuePod.Droid
{
    public class ApodListAdapter : BaseAdapter<Apod>
    {
        private readonly Activity _context;
        private readonly Apod[] _items;
        //ImageLoader _loader;

        public ApodListAdapter(Activity context, Apod[] items)
        {
            _items = items;
            _context = context;
            //_loader = new ImageLoader(context,64,40);
        }


        //public override int GetItemViewType(int position)
        //{
        //	return base.GetItemViewType(position);
        //}

        //public override int ViewTypeCount => 2;

        public override Apod this[int position]
        {
            get { return _items[position]; }
        }

        public override int Count
        {
            get { return _items.Length; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.NormalApodView, null);
            }

            var apod = _items[position];

            view.FindViewById<TextView>(Resource.Id.apodNormalText).Text = apod.Date.ToShortDateString();
            var image = view.FindViewById<ImageView>(Resource.Id.apodNormalImage);
            Debug.Write($"{apod.Date:yyyy/MM/dd} / {apod.MediaType} - {apod.Url}");
            if (apod.MediaType == "image")
            {
                Picasso.With(_context).Load(apod.Url).Into(image);
            }

            return view;
        }
    }
}