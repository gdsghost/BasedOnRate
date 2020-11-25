using System;
using Android.OS;
using Android.Views;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.WalkTroutPage
{
    public class AnimFragment2 : Android.Support.V4.App.Fragment
    { 
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Animination_Fragment2, container, false);
            return view;
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}