using System.Diagnostics;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using HuePod.Nasa;
using Square.Picasso;

namespace HuePod.Droid
{
    public class ApodListAdapter : BaseAdapter<Apod>
    {
        public static Typeface CustomFont;

        private readonly Activity _context;
        private readonly Apod[] _items;

        const int ImageView = 1;
        const int OtherView = 2;
        //ImageLoader _loader;

        public ApodListAdapter(Activity context, Apod[] items)
        {
            _items = items;
            _context = context;
            if (CustomFont == null)
            {
                CustomFont = Typeface.CreateFromAsset(context.Assets, "fonts/Tinos-Regular.ttf");
            }
        }

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
                var layoutInflater = _context.LayoutInflater;
                //var type = GetItemViewType(position);
                view = layoutInflater.Inflate(Resource.Layout.image_apod_item, null);
            }


            var apod = _items[position];

            var apodViewDateText = view.FindViewById<TextView>(Resource.Id.apodViewDateText);
            apodViewDateText.Text = $"{apod.Date:yyyy MMM dd}";
            apodViewDateText.Typeface = CustomFont;

            var apodViewTitleText = view.FindViewById<TextView>(Resource.Id.apodViewTitleText);
            apodViewTitleText.Text = apod.Title;
            apodViewTitleText.Typeface = CustomFont;

            var image = view.FindViewById<ImageView>(Resource.Id.apodViewImage);
            Debug.Write($"{apod.Date:yyyy/MM/dd} / {apod.MediaType} - {apod.Url}");
            if (apod.MediaType == "image")
            {
                Picasso.With(_context).Load(apod.Url).Into(image);
            }

            return view;
        }
    }
}