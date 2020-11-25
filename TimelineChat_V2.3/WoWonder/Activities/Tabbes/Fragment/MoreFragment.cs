using System;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Activities.Album;
using WoWonder.Activities.Articles;
using WoWonder.Activities.CommonThings;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.Events;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.Games;
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Market;
using WoWonder.Activities.Memories;
using WoWonder.Activities.Movies;
using WoWonder.Activities.MyPhoto;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.MyVideo;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NearBy;
using WoWonder.Activities.Offers;
using WoWonder.Activities.Pokes;
using WoWonder.Activities.PopularPosts;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.InvitationLinks;
using WoWonder.Activities.SettingsPreferences.MyInformation;
using WoWonder.Activities.SettingsPreferences.Notification;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.SettingsPreferences.Support;
using WoWonder.Activities.SettingsPreferences.TellFriend;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class MoreFragment : AndroidX.Fragment.App.Fragment,  MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        public MoreSectionAdapter MoreSectionAdapter;
        public MoreSectionAdapterTheme2 MoreSectionAdapterTheme2;
        private RecyclerView MoreRecylerView;
        public AdView MAdView;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            { 
                View view = inflater.Inflate(Resource.Layout.Tab_More_Layout, container, false);

                SetRecyclerViewAdapters(view);

                AddOrRemoveEvent(true);

                if (!AppSettings.SetTabOnButton)
                {
                    var parasms = (LinearLayout.LayoutParams)MoreRecylerView.LayoutParameters;
                    // Check if we're running on Android 5.0 or higher
                    parasms.TopMargin = (int)Build.VERSION.SdkInt < 23 ? 130 : 270;

                    MoreRecylerView.LayoutParameters = parasms;
                    MoreRecylerView.SetPadding(0, 0, 0, 0);
                }

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
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

        #endregion
          
        #region Functions

        private void SetRecyclerViewAdapters(View view)
        {
            try
            { 
                MoreRecylerView = (RecyclerView)view.FindViewById(Resource.Id.Recyler);
                MoreRecylerView.NestedScrollingEnabled = true;

                if (AppSettings.MoreTheme == MoreTheme.BeautyTheme)
                {
                    var layoutManager = new GridLayoutManager(Activity, 4);

                    MoreSectionAdapterTheme2 = new MoreSectionAdapterTheme2(Activity);

                    var countListFirstRow = MoreSectionAdapterTheme2.SectionList.Where(q => q.StyleRow == 0).ToList().Count;

                    layoutManager.SetSpanSizeLookup(new MySpanSizeLookup2(countListFirstRow, 1, 4));//20, 1, 4
                    MoreRecylerView.SetLayoutManager(layoutManager);
                    MoreRecylerView.SetAdapter(MoreSectionAdapterTheme2);
                }
                else
                {
                    MoreRecylerView.SetLayoutManager(new LinearLayoutManager(Activity));
                    MoreSectionAdapter = new MoreSectionAdapter(Activity);
                    MoreRecylerView.SetAdapter(MoreSectionAdapter);
                }
                //MoreRecylerView.HasFixedSize = true;
                MoreRecylerView.SetItemViewCacheSize(50);
                MoreRecylerView.GetLayoutManager().ItemPrefetchEnabled = true;

                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MoreRecylerView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    if (AppSettings.MoreTheme == MoreTheme.BeautyTheme)
                        MoreSectionAdapterTheme2.ItemClick += MoreSection_OnItemClick;
                    else
                        MoreSectionAdapter.ItemClick += MoreSection_OnItemClick;
                }
                else
                {
                    if (AppSettings.MoreTheme == MoreTheme.BeautyTheme)
                        MoreSectionAdapterTheme2.ItemClick -= MoreSection_OnItemClick;
                    else
                        MoreSectionAdapter.ItemClick -= MoreSection_OnItemClick;
                  
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Events

        //Event Open Intent Activity
        private void MoreSection_OnItemClick(object sender, MoreSectionAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MoreSectionAdapter?.GetItem(position);
                    
                    if (AppSettings.MoreTheme == MoreTheme.BeautyTheme)
                        item = MoreSectionAdapterTheme2.GetItem(position);
                    
                    if (item != null)
                    {
                        if (item.Id == 1) // My Profile
                        {
                            var intent = new Intent(Context, typeof(MyProfileActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 2) // Messages
                        {
                            Methods.App.OpenAppByPackageName(Context, AppSettings.MessengerPackageName, "OpenChatApp");
                        }
                        else if (item.Id == 3) // Contacts
                        {
                            var intent = new Intent(Context, typeof(MyContactsActivity));
                            intent.PutExtra("ContactsType", "Following");
                            intent.PutExtra("UserId", UserDetails.UserId);
                            StartActivity(intent);
                        }
                        else if (item.Id == 4) // Pokes
                        {
                            var intent = new Intent(Context, typeof(PokesActivity));
                            StartActivity(intent); 
                        }
                        else if (item.Id == 5) // Album
                        {
                            var intent = new Intent(Context, typeof(MyAlbumActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 6) // MyImages
                        {
                            var intent = new Intent(Context, typeof(MyPhotosActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 7) // MyVideos
                        { 
                            var intent = new Intent(Context, typeof(MyVideoActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 8) // Saved Posts
                        {
                            var intent = new Intent(Context, typeof(SavedPostsActivity)); 
                            StartActivity(intent);
                        }
                        else if (item.Id == 9) // Groups
                        {
                            var intent = new Intent(Context, typeof(GroupsActivity)); 
                            StartActivity(intent);
                        }
                        else if (item.Id == 10) // Pages
                        {
                            var intent = new Intent(Context, typeof(PagesActivity)); 
                            StartActivity(intent);
                        }
                        else if (item.Id == 11) // Blogs
                        {
                            StartActivity(new Intent(Context, typeof(ArticlesActivity)));
                        }
                        else if (item.Id == 12) // Market
                        {
                            StartActivity(new Intent(Context, typeof(TabbedMarketActivity)));
                        } 
                        else if (item.Id == 13) // Popular Posts
                        {
                            var intent = new Intent(Context, typeof(PopularPostsActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 14) // Events
                        {
                            var intent = new Intent(Context, typeof(EventMainActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 15) // Find Friends
                        {
                            var intent = new Intent(Context, typeof(PeopleNearByActivity));
                            StartActivity(intent);
                        } 
                        else if (item.Id == 16) // Movies
                        {
                            var intent = new Intent(Context, typeof(MoviesActivity));
                            StartActivity(intent);
                        } 
                        else if (item.Id == 17) // jobs
                        {
                            var intent = new Intent(Context, typeof(JobsActivity));
                            StartActivity(intent);  
                        } 
                        else if (item.Id == 18) // common things
                        { 
                            var intent = new Intent(Context, typeof(CommonThingsActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 19) // Fundings
                        {
                            var intent = new Intent(Context, typeof(FundingActivity));
                            StartActivity(intent); 
                        }
                        else if (item.Id == 20) // Games
                        {
                            var intent = new Intent(Context, typeof(GamesActivity));
                            StartActivity(intent); 
                        }
                        else if (item.Id == 80) // Help & Support
                        {
                            var intent = new Intent(Context, typeof(MemoriesActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 82) // Help & Support
                        {
                            var intent = new Intent(Context, typeof(OffersActivity));
                            StartActivity(intent);
                        }
                        //Settings Page
                        else if (item.Id == 21) // General Account
                        {
                            var intent = new Intent(Context, typeof(GeneralAccountActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 22) // Privacy
                        {
                            var intent = new Intent(Context, typeof(PrivacyActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 23) // Notification
                        {
                            var intent = new Intent(Context, typeof(MessegeNotificationActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 24) // InvitationLinks
                        {
                            var intent = new Intent(Context, typeof(InvitationLinksActivity));
                            StartActivity(intent);
                        } 
                        else if (item.Id == 25) // MyInformation
                        {
                            var intent = new Intent(Context, typeof(MyInformationActivity));
                            StartActivity(intent);
                        } 
                        else if (item.Id == 26) // Tell Friends
                        {
                            var intent = new Intent(Context, typeof(TellFriendActivity));
                            StartActivity(intent);
                        } 
                        else if (item.Id == 27) // Help & Support
                        {
                            var intent = new Intent(Context, typeof(SupportActivity));
                            StartActivity(intent);
                        }
                        else if (item.Id == 28) // Logout
                        {
                            var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                            dialog.Title(Resource.String.Lbl_Warning);
                            dialog.Content(Context.GetText(Resource.String.Lbl_Are_you_logout));
                            dialog.PositiveText(Context.GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                            dialog.NegativeText(Context.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                            dialog.AlwaysCallSingleChoiceCallback();
                            dialog.Build().Show();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Permissions

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        ApiRequest.Logout(Activity);
                    else
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Permission_is_denied),
                            ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region MaterialDialog
         
        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long)?.Show();
                        ApiRequest.Logout(Activity);
                    }
                    else
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long)?.Show();
                            ApiRequest.Logout(Activity);
                        }
                        else
                           new PermissionsController(Activity).RequestPermission(100); 
                    }
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion
        
    }
}