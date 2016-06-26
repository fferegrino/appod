// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace HuePod.iOS
{
	[Register ("ApodTableViewCell")]
	partial class ApodTableViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel apodTitleLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (apodTitleLabel != null) {
				apodTitleLabel.Dispose ();
				apodTitleLabel = null;
			}
		}
	}
}
