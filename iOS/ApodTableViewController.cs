using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Net.Http;
using HuePod.Nasa;
using System.Threading.Tasks;
using SDWebImage;

namespace HuePod.iOS
{
    public partial class ApodTableViewController : UITableViewController
    {

        private List<Apod> _apods;
        private Service _service;

        public ApodTableViewController(IntPtr handle) : base(handle)
        {
			Title = "APOD";
			NavigationItem.Title = "Astronomy Picture Of the Day";

			View.BackgroundColor = UIColor.FromRGB(244,244,255);
			TableView.BackgroundColor = UIColor.FromRGB(244, 244, 255);


            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 140;
            _apods = new List<Apod>();
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            _service = new Service();
            await LoadLastPictures();
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }



        private async Task LoadLastPictures()
        {
            _apods = await _service.GetLastAstronomicPictures(10);
            base.TableView.ReloadData();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _apods.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("ApodTableViewCell", indexPath);
            var apodCell = cell as ApodTableViewCell;

            var apod = _apods[indexPath.Row];

            apodCell.ApodTitleLabel.Text = apod.Title;
            apodCell.ApodDateLabel.Text = $"{apod.Date:yyyy MMM dd}";
			apodCell.BackgroundColor = UIColor.FromRGB(244, 244, 255);

            //var imageTask = LoadImageFromURL(apod.Url);
            //imageTask.Wait();
            //apodCell.ImageView.Image = imageTask.Result;
            if (apod.MediaType == "image")
            {
				apodCell.ApodImageView.SetImage(new NSUrl(apod.Url));
            }

            //[imageView setImageWithURLRequest: request placeholderImage: nil success:^ (NSURLRequest * request, NSHTTPURLResponse * response, UIImage * image) {
            //blockImageView.image = image; } failure: nil];

            return apodCell;
        }

		static UIImage FromUrl(string uri)
		{
			using (var url = new NSUrl(uri))
			using (var data = NSData.FromUrl(url))
				return UIImage.LoadFromData(data);
		}
    }
}