// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace HuePod.iOS
{
	[Register ("ApodDetailViewController")]
	partial class ApodDetailViewController
	{
		[Outlet]
		UIKit.UIView BgView { get; set; }

		[Outlet]
		UIKit.UILabel DescriptionLabel { get; set; }

		[Outlet]
		UIKit.UIImageView FullImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DescriptionLabel != null) {
				DescriptionLabel.Dispose ();
				DescriptionLabel = null;
			}

			if (BgView != null) {
				BgView.Dispose ();
				BgView = null;
			}

			if (FullImage != null) {
				FullImage.Dispose ();
				FullImage = null;
			}
		}
	}
}
