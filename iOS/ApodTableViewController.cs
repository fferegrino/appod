using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using HuePod.Nasa;
using System.Threading.Tasks;

namespace HuePod.iOS
{
    public partial class ApodTableViewController : UITableViewController
    {

		private List<Apod> _apods;
		private Service _service;

        public ApodTableViewController (IntPtr handle) : base (handle)
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

			return apodCell;
		}
    }
}