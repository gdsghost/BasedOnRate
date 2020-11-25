using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;

using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Fonts;
using AmulyaKhare.TextDrawableLib;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Tabbes.Adapters
{
    public class SectionItem
    {
        public int Id { get; set; }
        public string SectionName { get; set; }
        public string Icon { get; set; }

        public int IconAsImage { get; set; }
        public int StyleRow { get; set; }
        public Color IconColor { get; set; }
        public int BadgeCount { get; set; }
        public bool Badgevisibilty { get; set; }
    }

    public class MoreSectionAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<SectionItem> SectionList = new ObservableCollection<SectionItem>();

        public MoreSectionAdapter(Activity activityContext)
        {
            try
            {
                SectionList.Add(new SectionItem
                {
                    Id = 1,
                    SectionName = activityContext.GetText(Resource.String.Lbl_MyProfile),
                    BadgeCount = 0,
                    Badgevisibilty = false,
                    Icon = IonIconsFonts.Happy,
                    IconColor = Color.ParseColor("#047cac")
                });
                //if (AppSettings.MessengerIntegration)
                //    SectionList.Add(new SectionItem
                //    {
                //        Id = 2,
                //        SectionName = activityContext.GetText(Resource.String.Lbl_Messages),
                //        BadgeCount = 0,
                //        Badgevisibilty = false,
                //        Icon = IonIconsFonts.Chatbubbles,
                //        IconColor = Color.ParseColor("#03a9f4")
                //    });
                if (AppSettings.ShowUserContacts)
                {
                    string name = activityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends);
                    SectionList.Add(new SectionItem
                    {
                        Id = 3,
                        SectionName = name,
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.People,
                        IconColor = Color.ParseColor("#d80073")
                    });
                }
                if (AppSettings.ShowPokes)
                    SectionList.Add(new SectionItem
                    {
                        Id = 4,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Pokes),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Aperture,
                        IconColor = Color.ParseColor("#009688")
                    });
                if (AppSettings.ShowAlbum)
                    SectionList.Add(new SectionItem
                    {
                        Id = 5,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Albums),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Images,
                        IconColor = Color.ParseColor("#8bc34a")
                    });
                if (AppSettings.ShowMyPhoto)
                    SectionList.Add(new SectionItem
                    {
                        Id = 6,
                        SectionName = activityContext.GetText(Resource.String.Lbl_MyImages),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Camera,
                        IconColor = Color.ParseColor("#006064")
                    });
                if (AppSettings.ShowMyVideo)
                    SectionList.Add(new SectionItem
                    {
                        Id = 7,
                        SectionName = activityContext.GetText(Resource.String.Lbl_MyVideos),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Film,
                        IconColor = Color.ParseColor("#8e44ad")
                    });
                if (AppSettings.ShowSavedPost)
                    SectionList.Add(new SectionItem
                    {
                        Id = 8,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Saved_Posts),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Bookmark,
                        IconColor = Color.ParseColor("#673ab7")
                    });
                if (AppSettings.ShowCommunitiesGroups)
                    SectionList.Add(new SectionItem
                    {
                        Id = 9,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Groups),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Apps,
                        IconColor = Color.ParseColor("#03A9F4")
                    });
                if (AppSettings.ShowCommunitiesPages)
                    SectionList.Add(new SectionItem
                    {
                        Id = 10,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Pages),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Flag,
                        IconColor = Color.ParseColor("#f79f58")
                    });
                if (AppSettings.ShowArticles)
                    SectionList.Add(new SectionItem
                    {
                        Id = 11,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Blogs),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosBook,
                        IconColor = Color.ParseColor("#f35d4d")
                    });
                if (AppSettings.ShowMarket)
                    SectionList.Add(new SectionItem
                    {
                        Id = 12,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Marketplace),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosBriefcase,
                        IconColor = Color.ParseColor("#7d8250")
                    });
                if (AppSettings.ShowPopularPosts)
                    SectionList.Add(new SectionItem
                    {
                        Id = 13,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Popular_Posts),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Clipboard,
                        IconColor = Color.ParseColor("#8d73cc")
                    });
                if (AppSettings.ShowEvents)
                    SectionList.Add(new SectionItem
                    {
                        Id = 14,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Events),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Calendar,
                        IconColor = Color.ParseColor("#f25e4e")
                    });
                if (AppSettings.ShowNearBy)
                    SectionList.Add(new SectionItem
                    {
                        Id = 15,
                        SectionName = activityContext.GetText(Resource.String.Lbl_FindFriends),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Pin,
                        IconColor = Color.ParseColor("#b2c17c")
                    });
                if (AppSettings.ShowOffers)
                    SectionList.Add(new SectionItem
                    {
                        Id = 82,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Offers),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Pricetag,
                        IconColor = Color.ParseColor("#e91e63")
                    });
                if (AppSettings.ShowMovies)
                    SectionList.Add(new SectionItem
                    {
                        Id = 16,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Movies),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Film,
                        IconColor = Color.ParseColor("#8d73cc")
                    });

                if (AppSettings.ShowJobs)
                    SectionList.Add(new SectionItem
                    {
                        Id = 17,
                        SectionName = activityContext.GetText(Resource.String.Lbl_jobs),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosBriefcase,
                        IconColor = Color.ParseColor("#4caf50")
                    });

                if (AppSettings.ShowCommonThings)
                    SectionList.Add(new SectionItem
                    {
                        Id = 18,
                        SectionName = activityContext.GetText(Resource.String.Lbl_common_things),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.CheckmarkCircle,
                        IconColor = Color.ParseColor("#ff5991")
                    });
                if (AppSettings.ShowMemories)
                    SectionList.Add(new SectionItem
                    {
                        Id = 80,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Memories),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Timer,
                        IconColor = Color.ParseColor("#009da0")
                    });
                if (AppSettings.ShowFundings)
                    SectionList.Add(new SectionItem
                    {
                        Id = 19,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Funding),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.LogoUsd,
                        IconColor = Color.ParseColor("#673AB7")
                    });
                if (AppSettings.ShowGames)
                    SectionList.Add(new SectionItem
                    {
                        Id = 20,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Games),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.LogoGameControllerB,
                        IconColor = Color.ParseColor("#03A9F4")
                    });
                //Settings Page
                if (AppSettings.ShowSettingsGeneralAccount)
                    SectionList.Add(new SectionItem
                    {
                        Id = 21,
                        SectionName = activityContext.GetText(Resource.String.Lbl_GeneralAccount),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Settings,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsPrivacy)
                    SectionList.Add(new SectionItem
                    {
                        Id = 22,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Privacy),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Eye,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsNotification)
                    SectionList.Add(new SectionItem
                    {
                        Id = 23,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Notifications),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Notifications,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsInvitationLinks)
                    SectionList.Add(new SectionItem
                    {
                        Id = 24,
                        SectionName = activityContext.GetText(Resource.String.Lbl_InvitationLinks),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Link,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsMyInformation)
                    SectionList.Add(new SectionItem
                    {
                        Id = 25,
                        SectionName = activityContext.GetText(Resource.String.Lbl_MyInformation),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosPaper,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsInviteFriends)
                    SectionList.Add(new SectionItem
                    {
                        Id = 26,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Earnings),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosHome,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsHelpSupport)
                    SectionList.Add(new SectionItem
                    {
                        Id = 27,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Help_Support),
                        BadgeCount = 0,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Help,
                        IconColor = Color.ParseColor("#757575")
                    });
                SectionList.Add(new SectionItem
                {
                    Id = 28,
                    SectionName = activityContext.GetText(Resource.String.Lbl_Logout),
                    BadgeCount = 0,
                    Badgevisibilty = false,
                    Icon = IonIconsFonts.LogOut,
                    IconColor = Color.ParseColor("#d50000")
                }); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SectionList?.Count ?? 0;
 
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemClick;
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> ChannelSubscribed_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection_view, parent, false);
                var vh = new MoreSectionAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }


        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is MoreSectionAdapterViewHolder holder)
                {
                    if (AppSettings.FlowDirectionRightToLeft)
                    {
                        holder.LinearLayoutImage.LayoutDirection = LayoutDirection.Rtl;
                        holder.LinearLayoutMain.LayoutDirection = LayoutDirection.Rtl;
                        holder.Name.LayoutDirection = LayoutDirection.Rtl;
                    }
                     
                    var item = SectionList[position];
                    if (item != null)
                    { 
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.Icon, item.Icon);
                        holder.Icon.SetTextColor(item.IconColor);
                        holder.Name.Text = item.SectionName;

                        if (item.BadgeCount != 0)
                        {
                            var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound(item.BadgeCount.ToString(), Color.ParseColor(AppSettings.MainColor));
                            holder.Badge.SetImageDrawable(drawable);
                        }

                        holder.Badge.Visibility = item.Badgevisibilty ? ViewStates.Visible : ViewStates.Invisible;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public SectionItem GetItem(int position)
        {
            return SectionList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(MoreSectionAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MoreSectionAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class MoreSectionAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MoreSectionAdapterViewHolder(View itemView, Action<MoreSectionAdapterClickEventArgs> clickListener,Action<MoreSectionAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LinearLayoutMain = MainView.FindViewById<LinearLayout>(Resource.Id.main);
                LinearLayoutImage = MainView.FindViewById<RelativeLayout>(Resource.Id.imagecontainer);

                Icon = MainView.FindViewById<TextView>(Resource.Id.Icon);
                Name = MainView.FindViewById<TextView>(Resource.Id.section_name);
                Badge = MainView.FindViewById<ImageView>(Resource.Id.badge);

                itemView.Click += (sender, e) => clickListener(new MoreSectionAdapterClickEventArgs{View = itemView, Position = AdapterPosition});

                Console.WriteLine(longClickListener);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public View MainView { get; }

        public LinearLayout LinearLayoutMain { get; private set; }
        public RelativeLayout LinearLayoutImage { get; private set; }
        public TextView Icon { get; private set; }
        public TextView Name { get; private set; }
        public ImageView Badge { get; private set; }

        
    }

    public class MoreSectionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}