using Android.App;
using Android.OS;

namespace HuePod.Droid
{
    //[Activity(Label = "DemoActivity", MainLauncher = true, Icon = "@mipmap/icon")]
    public class DemoActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_demo);
            // Create your application here
        }
    }
}