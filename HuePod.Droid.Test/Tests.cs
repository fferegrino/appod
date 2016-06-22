using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace HuePod.Droid.Test
{
	[TestFixture]
	public class Tests
	{
		AndroidApp app;

		[SetUp]
		public void BeforeEachTest()
		{
			// TODO: If the Android app being tested is included in the solution then open
			// the Unit Tests window, right click Test Apps, select Add App Project
			// and select the app projects that should be tested.
			app = ConfigureApp
				.Android
				// TODO: Update this path to point to your Android app and uncomment the
				// code if the app is not included in the solution.
				//.ApkFile ("../../../Android/bin/Debug/UITestsAndroid.apk")
				.EnableLocalScreenshots()
				.StartApp();
		}

		[Test]
		public void ScreenShots()
		{
			
			app.WaitForElement("apodViewTitleText");
			app.ScrollDown("apodsListView", ScrollStrategy.Gesture);
			app.WaitForElement("apodViewTitleText");
			var home = app.TakeShot("First screen");
			app.Tap("apodViewTitleText");
			app.WaitForElement("descriptionView");
			var detail = app.TakeShot("Detail shot");
			app.Tap("Hide UI");
			var detail2 = app.TakeShot("Detail shot 2");
			app.Back();
			app.ScrollUp("apodsListView", ScrollStrategy.Gesture);
			app.WaitForElement("fab");
			app.Tap("fab");
			app.WaitForElement("OK");
			app.TakeShot("Calendar view");
			//var detail = app.Screenshot("Detail");


			//app.Query(e=> e.
		}
	}

	public static class ScreenshotExtensions
	{
		const string Location = "/Users/fferegrino/Documents/github/huepod/screens/android";
		public static FileInfo TakeShot(this AndroidApp app, string name)
		{
			var f = app.Screenshot(name);
			var newFileUrl = Path.Combine(Location, name + f.Extension);
			f.MoveTo(newFileUrl);
			return f;
		}
	}
}

