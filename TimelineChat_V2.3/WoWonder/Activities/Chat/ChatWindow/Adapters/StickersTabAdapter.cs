using System.Collections.Generic;
using AndroidX.Fragment.App;
using Java.Lang;
using String = Java.Lang.String;

namespace WoWonder.Activities.Chat.ChatWindow.Adapters
{
    public class StickersTabAdapter : FragmentPagerAdapter
    {
        public List<AndroidX.Fragment.App.Fragment> Fragments { get; set; }
        public List<string> FragmentNames { get; set; }


#pragma warning disable 618
        public StickersTabAdapter(FragmentManager fm) : base(fm)
#pragma warning restore 618
        {
            Fragments = new List<AndroidX.Fragment.App.Fragment>();
            FragmentNames = new List<string>();
        }

        public StickersTabAdapter(FragmentManager fm, int behavior) : base(fm, behavior)
        {
            Fragments = new List<AndroidX.Fragment.App.Fragment>();
            FragmentNames = new List<string>();
        }

        public void AddFragment(AndroidX.Fragment.App.Fragment fragment, string name)
        {
            Fragments.Add(fragment);
            FragmentNames.Add(name);
        }

        public override int Count
        {
            get { return Fragments.Count; }
        }

        public override AndroidX.Fragment.App.Fragment GetItem(int position)
        {
            return Fragments[position];
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(FragmentNames[position]);
        }
         
    }
}