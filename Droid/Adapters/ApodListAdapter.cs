using System;
using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using HuePod.Nasa;
using Square.Picasso;

namespace HuePod.Droid
{
	public class ApodsAdapter : RecyclerView.Adapter
	{

		public static Typeface CustomFont;
		public static Typeface CustomFontBold;

		private readonly Context _context;
		private readonly Apod[] _items;

		public ApodsAdapter(Activity context, Apod[] items)
		{
			_items = items;
			if (CustomFont == null)
			{
				_context = context;
				CustomFont = Typeface.CreateFromAsset(context.Assets, "fonts/Tinos-Regular.ttf");
				CustomFontBold = Typeface.CreateFromAsset(context.Assets, "fonts/Tinos-Bold.ttf");
			}
		}


		public class ApodViewHolder : RecyclerView.ViewHolder
		{

			TextView _apodViewDateText;
			TextView _apodViewTitleText;
			ImageView _image;

			public ApodViewHolder(View view, Action<int> onClick) : base(view)
			{
				view.Click += (s, e) => onClick(base.Position);
				ApodViewDateText = view.FindViewById<TextView>(Resource.Id.apodViewDateText);
				ApodViewDateText.Typeface = CustomFont;

				ApodViewTitleText = view.FindViewById<TextView>(Resource.Id.apodViewTitleText);
				ApodViewTitleText.Typeface = CustomFontBold;

				Image = view.FindViewById<ImageView>(Resource.Id.apodViewImage);
			}

			public TextView ApodViewDateText
			{
				get
				{
					return _apodViewDateText;
				}

				set
				{
					_apodViewDateText = value;
				}
			}

			public TextView ApodViewTitleText
			{
				get
				{
					return _apodViewTitleText;
				}

				set
				{
					_apodViewTitleText = value;
				}
			}

			public ImageView Image
			{
				get
				{
					return _image;
				}

				set
				{
					_image = value;
				}
			}
		}

		public event EventHandler<int> ApodClick;

		private void OnClick(int position)
		{
			ApodClick?.Invoke(this, position);
		}

		public override int ItemCount
		{
			get
			{
				return _items.Length;
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var hldr = holder as ApodViewHolder;
			var apod = _items[position];

			hldr.ApodViewTitleText.Text = apod.Title;
			hldr.ApodViewDateText.Text = $"{apod.Date:yyyy MMM dd}";

			if (apod.MediaType == "image")
			{
				Picasso.With(_context).Load(apod.CloudinaryUrl).Into(hldr.Image);
			}
			else if (apod.MediaType == "video")
			{
				Picasso.With(_context).Load(Resource.Drawable.video).Into(hldr.Image);
			}

		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var ctx = parent.Context;
			var layoutInflater = LayoutInflater.From(ctx);
			var view = layoutInflater.Inflate(Resource.Layout.image_apod_item,parent,false);

			var vh = new ApodViewHolder(view, OnClick);
			return vh;
		}
	}

}