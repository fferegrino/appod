using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Util;
using Java.Interop;

namespace HuePod.Droid
{
	[Android.Runtime.Register("HuePod.Droid.FabScrollAwareBehavior")]
	public class FabScrollAwareBehavior : CoordinatorLayout.Behavior
	{
		public FabScrollAwareBehavior(Context context, IAttributeSet attrs) : base() {
		}

		public override bool OnStartNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, Android.Views.View directTargetChild, Android.Views.View target, int nestedScrollAxes)
		{

			return nestedScrollAxes == Android.Support.V4.View.ViewCompat.ScrollAxisVertical ||
					 base.OnStartNestedScroll(coordinatorLayout, child, directTargetChild, target, nestedScrollAxes);
		}

		public override void OnNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, Android.Views.View target, int dxConsumed, int dyConsumed, int dxUnconsumed, int dyUnconsumed)
		{
			base.OnNestedScroll(coordinatorLayout, child, target, dxConsumed, dyConsumed, dxUnconsumed, dyUnconsumed);

			var fabChild = child.JavaCast<FloatingActionButton>();

			if (dyConsumed > 0 && fabChild.Visibility == Android.Views.ViewStates.Visible)
				{
					fabChild.Hide();
				}
			else if(dyConsumed < 0 && fabChild.Visibility != Android.Views.ViewStates.Visible) {
					fabChild.Show();
				}
		}

	}
}

