using Foundation;
using System;
using UIKit;

namespace HuePod.iOS
{
    public partial class ApodTableViewController : UITableViewController
    {
        public ApodTableViewController (IntPtr handle) : base (handle)
        {
        }

		public override nint NumberOfSections(UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection(UITableView tableView, nint section)
		{
			return 10;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell("ApodTableViewCell", indexPath);
			var apodCell = cell as ApodTableViewCell;

			apodCell.ApodTitleLabel.Text = "APOD " + indexPath.Row;

			return apodCell;
		}
    }
}