using Foundation;
using System;
using UIKit;

namespace HuePod.iOS
{
    public partial class ApodTableViewCell : UITableViewCell
    {
        public ApodTableViewCell (IntPtr handle) : base (handle)
        {
        }

		public UILabel ApodTitleLabel => apodTitleLabel;

		public UILabel ApodDateLabel => apodDateLabel;

		public UIImageView ApodImageView => apodImageView;
    }
}