using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Net.Http;
using HuePod.Nasa;
using System.Threading.Tasks;
using AFNetworking;

namespace HuePod.iOS
{
    public partial class ApodTableViewController : UITableViewController
    {

        private List<Apod> _apods;
        private Service _service;

        public ApodTableViewController(IntPtr handle) : base(handle)
        {
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

            //var imageTask = LoadImageFromURL(apod.Url);
            //imageTask.Wait();
            //apodCell.ImageView.Image = imageTask.Result;
            if (apod.MediaType == "image")
            {
                apodCell.ImageView.SetImageUrl(new NSUrl(apod.Url), UIImage.FromFile("achede.jpg"));
            }



            //[imageView setImageWithURLRequest: request placeholderImage: nil success:^ (NSURLRequest * request, NSHTTPURLResponse * response, UIImage * image) {
            //blockImageView.image = image; } failure: nil];

            return apodCell;
        }


        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <returns>The image.</returns>
        /// <param name="imageUrl">Image URL.</param>
        public static async Task<UIImage> LoadImageFromURL(string imageUrl)
        {
            var httpClient = new HttpClient();

            Task<byte[]> contentsTask = httpClient.GetByteArrayAsync(imageUrl);

            // await! control returns to the caller and the task continues to run on another thread
            var contents = await contentsTask;

            System.Diagnostics.Debug.WriteLine(imageUrl);

            // load from bytes
            return UIImage.LoadFromData(NSData.FromArray(contents));
        }
    }
}