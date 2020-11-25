using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.Geolocator;
using WoWonder.Activities.BlockedUsers;
using WoWonder.Activities.Chat.Call;
using WoWonder.Activities.Chat.ChatWindow;
using WoWonder.Activities.Chat.Floating;
using WoWonder.Activities.Chat.GroupChat;
using WoWonder.Activities.Chat.MsgTabbes.Fragment;
using WoWonder.Activities.Chat.MsgTabbes.Services;
using WoWonder.Activities.Chat.OldTab;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.NearBy;
using WoWonder.Activities.Search;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.Tabbes;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Xamarin.Facebook.Ads;
using Console = System.Console;
using Exception = System.Exception;

namespace WoWonder.Activities.Chat.MsgTabbes
{
    //[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class MsgTabbedMainActivity : AndroidX.Fragment.App.Fragment, ServiceResultReceiver.IReceiver, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables

        public TabLayout Tabs;
        private SwipeViewPager ViewPager;
        //private FloatingActionButton FloatingActionFilter;
        //private FrameLayout FloatingActionButtonView;
        //private ImageView FloatingActionImageView;
        private TextView TxtAppName;
        private string FloatingActionTag;
        public LastChatFragment LastChatTab; 
        public LastCallsFragment LastCallsTab;
        private static MsgTabbedMainActivity Instance;
        //private string ImageType;
        public static ServiceResultReceiver Receiver;
        public static bool RunCall;
        private PowerManager.WakeLock Wl;
        //private Handler ExitHandler;
        //private bool RecentlyBackPressed;
        public LastMessagesFragment LastMessagesTab;
        public LastGroupChatsFragment LastGroupChatsTab;
        public LastPageChatsFragment LastPageChatsTab;
        //ToolBar 
        private ImageView DiscoverImageView, SearchImageView, MoreImageView;
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

        public InitFloating Floating;

        private RewardedVideoAd RewardedVideo;
        private InterstitialAd InterstitialAd;
        private TabbedMainActivity GlobalContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedMainActivity)Activity ?? TabbedMainActivity.GetInstance();
        }
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.MsgTabbed_Main_Page, container, false);

                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                Instance = this;
                Floating = new InitFloating();

                //Get Value And Set Toolbar
                InitComponent(view);
                //InitToolbar();

                //GetGeneralAppData();

                RunCall = false;

                SetService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {

                base.OnResume();
                AddOrRemoveEvent(true);
                SetWakeLock();
                RewardedVideoAd?.OnResume(Activity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                OffWakeLock();
                RewardedVideoAd?.OnPause(Activity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //public override void OnTrimMemory(TrimMemory level)
        //{
        //    try
        //    {
        //        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        //        base.OnTrimMemory(level);
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                RewardedVideoAd?.OnDestroy(Activity);
                RewardedVideo?.Destroy();
                InterstitialAd?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //public override void OnConfigurationChanged(Configuration newConfig)
        //{
        //    try
        //    {
        //        base.OnConfigurationChanged(newConfig);

        //        var currentNightMode = newConfig.UiMode & UiMode.NightMask;
        //        switch (currentNightMode)
        //        {
        //            case UiMode.NightNo:
        //                // Night mode is not active, we're using the light theme
        //                AppSettings.SetTabDarkTheme = false;
        //                break;
        //            case UiMode.NightYes:
        //                // Night mode is active, we're using dark theme
        //                AppSettings.SetTabDarkTheme = true;
        //                break;
        //        }

        //        SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        #endregion

        #region Menu

        //public override void OnBackPressed()
        //{
        //    try
        //    {
        //        ExitHandler ??= new Handler(Looper.MainLooper);
        //        if (RecentlyBackPressed)
        //        {
        //            ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
        //            RecentlyBackPressed = false;
        //            MoveTaskToBack(true);
        //            Finish();
        //        }
        //        else
        //        {
        //            RecentlyBackPressed = true;
        //            Toast.MakeText(Activity, GetString(Resource.String.press_again_exit), ToastLength.Long)?.Show();
        //            ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}
        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                DiscoverImageView = view.FindViewById<ImageView>(Resource.Id.discoverButton);
                SearchImageView = view.FindViewById<ImageView>(Resource.Id.searchButton);
                MoreImageView = view.FindViewById<ImageView>(Resource.Id.moreButton);
                TxtAppName = view.FindViewById<TextView>(Resource.Id.appName);
                //FloatingActionFilter = FindViewById<FloatingActionButton>(Resource.Id.floatingActionFilter);

                TxtAppName.Text = GetText(Resource.String.Lbl_Tab_Chats);

                //FloatingActionButtonView = view.FindViewById<FrameLayout>(Resource.Id.FloatingAction);
                //FloatingActionImageView = view.FindViewById<ImageView>(Resource.Id.Image);
                //FloatingActionTag = "lastMessages";
                //FloatingActionButtonView.Visibility = ViewStates.Visible;
                Tabs = view.FindViewById<TabLayout>(Resource.Id.tabsLayout);
                ViewPager = view.FindViewById<SwipeViewPager>(Resource.Id.viewpager);
                ViewPager.SetSwipeAble(false);

                SetUpViewPager(ViewPager);
                if (ViewPager != null) Tabs.SetupWithViewPager(ViewPager);

                //var tab = Tabs.GetTabAt(0); //Lbl_Tab_Chats 
                ////set custom view
                //tab.SetCustomView(Resource.Layout.IconBadgeLayout);
                //TextView textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                //textView.Visibility = ViewStates.Gone;
                //var d = Color.ParseColor("");

                Tabs.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.DimGray, Color.ParseColor(AppSettings.MainColor));
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
                    //FloatingActionButtonView.Click += FloatingActionButtonView_Click;
                    //FloatingActionFilter.Click += FloatingActionFilterOnClick;
                    DiscoverImageView.Click += DiscoverImageView_Click;
                    SearchImageView.Click += SearchImageView_Click;
                    MoreImageView.Click += MoreImageView_Click;


                }
                else
                {
                    //FloatingActionButtonView.Click -= FloatingActionButtonView_Click;
                    //FloatingActionFilter.Click -= FloatingActionFilterOnClick;
                    DiscoverImageView.Click -= DiscoverImageView_Click;
                    SearchImageView.Click -= SearchImageView_Click;
                    MoreImageView.Click -= MoreImageView_Click;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static MsgTabbedMainActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        #endregion

        #region Events

        private void MoreImageView_Click(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                if (FloatingActionTag == "lastMessages")
                {
                    if (AppSettings.LastChatSystem == SystemApiGetLastChat.Old)
                    {
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Blocked_User_List));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                    }
                    else
                    {
                        arrayAdapter.Add(GetText(Resource.String.Lbl_CreateNewGroup));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_GroupRequest));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Blocked_User_List));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                    }
                }
                else if (FloatingActionTag == "GroupChats")
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_CreateNewGroup));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_GroupRequest));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                }
                else if (FloatingActionTag == "PageChats")
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                } 
                else if (FloatingActionTag == "Call")
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Clear_call_log));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                }

                dialogList.Title(GetString(Resource.String.Lbl_Menu_More));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchImageView_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(SearchTabbedActivity));
                intent.PutExtra("Key", "");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DiscoverImageView_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Activity, typeof(PeopleNearByActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FloatingActionButtonView_Click()
        {
            try
            {
                if (FloatingActionTag == "lastMessages")
                {
                    var intent = new Intent(Activity, typeof(MyContactsActivity));
                    intent.PutExtra("ContactsType", "Following");
                    intent.PutExtra("UserId", UserDetails.UserId);
                    StartActivity(intent);
                }
                else if (FloatingActionTag == "GroupChats")
                {
                    var intent = new Intent(Activity, typeof(CreateGroupChatActivity));
                    StartActivity(intent);
                } 
                else if (FloatingActionTag == "Call")
                {
                    StartActivity(new Intent(Activity, typeof(AddNewCallActivity)));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FloatingActionButtonView_Tag()
        {
            try
            {
                if (FloatingActionTag == "lastMessages")
                {
                    FloatingActionTag = "lastMessages";
                    GlobalContext.FloatingActionButton.Tag = "lastMessages";
                    GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_add_user);
                    GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                }
                else if (FloatingActionTag == "GroupChats")
                {
                    FloatingActionTag = "GroupChats";
                    GlobalContext.FloatingActionButton.Tag = "GroupChats";
                    GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_add);
                    GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                }
                else if (FloatingActionTag == "Call")
                {
                    FloatingActionTag = "Call";
                    GlobalContext.FloatingActionButton.Tag = "Call";
                    GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_phone_user);
                    GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    FloatingActionTag = "lastMessages";
                    GlobalContext.FloatingActionButton.Tag = "lastMessages";
                    GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_add_user);
                    GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //show filter (Online/All) User 
        private void FloatingActionFilterOnClick(object sender, EventArgs e)
        {
            try
            {
                //FilterLastChatDialogFragment filterLastChat = new FilterLastChatDialogFragment();
                //filterLastChat.Show(SupportFragmentManager, filterLastChat.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        { 
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
  
                if (requestCode == InitFloating.ChatHeadDataRequestCode && InitFloating.CanDrawOverlays(Activity))
                {
                    Floating.FloatingShow(InitFloating.FloatingObject);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                 if (requestCode == 110)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Activity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                {
                    if (position == 0) // lastMessages
                    {
                        AdsGoogle.Ad_Interstitial(Activity);
                    } 
                    else if (position == 1) // Call
                    {
                        AdsGoogle.Ad_Interstitial(Activity);
                        //LastCallsTab.Get_CallUser();
                    }
                }
                else
                {
                    if (position == 0) // lastMessages
                    {
                        AdsGoogle.Ad_Interstitial(Activity);
                    }
                    else if (position == 1) // GroupChats
                    {
                        RewardedVideo = AdsFacebook.InitRewardVideo(Activity);
                        //if (AppSettings.EnableChatGroup)
                        //{
                        //    LastGroupChatsTab.StartApiService();
                        //}
                        //else if (AppSettings.EnableChatPage)
                        //{
                        //    LastPageChatsTab.StartApiService();
                        //}
                        //else
                        //{
                        //    LastStoriesTab.StartApiService();
                        //}
                    }
                    else if (position == 2) // PageChats
                    {
                        AdsGoogle.Ad_Interstitial(Activity);
                        //if (AppSettings.EnableChatPage)
                        //{
                        //    LastPageChatsTab.StartApiService();
                        //}
                        //else
                        //{
                        //    LastStoriesTab.StartApiService();
                        //}
                    }
                    else if (position == 3) // Call
                    {
                        InterstitialAd = AdsFacebook.InitInterstitial(Activity);
                        //LastCallsTab.Get_CallUser();
                    }
                }

                ViewPager_PageScrolled(position);

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ViewPager_PageScrolled(int position)
        {
            try
            {
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                {
                    if (position == 0) // lastMessages
                    {
                        if (GlobalContext.FloatingActionButton.Tag?.ToString() != "lastMessages")
                        {
                            GlobalContext.FloatingActionButton.Tag = "lastMessages";
                            GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.icon_profile_vector);
                            GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                            //FloatingActionFilter.Visibility = ViewStates.Visible;
                        }
                    } 
                    else if (position == 1) // Call
                    {
                        if (GlobalContext.FloatingActionButton.Tag?.ToString() != "Call")
                        {
                            GlobalContext.FloatingActionButton.Tag = "Call";
                            GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_phone_user);
                            GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                            //FloatingActionFilter.Visibility = ViewStates.Invisible;

                            //if (Tabs != null)
                            //{
                            //    var tab = Tabs.GetTabAt(0); //Lbl_Tab_Chats

                            //    var textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                            //    textView.Visibility = ViewStates.Gone;
                            //}
                        }
                    }
                }
                else
                {
                    //FloatingActionFilter.Visibility = ViewStates.Invisible;
                    if (position == 0) // lastMessages
                    {
                        if (GlobalContext.FloatingActionButton.Tag?.ToString() != "lastMessages")
                        {
                            FloatingActionTag = "lastMessages";
                            GlobalContext.FloatingActionButton.Tag = "lastMessages";
                            GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_add_user);
                            GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                            //FloatingActionFilter.Visibility = ViewStates.Visible;
                        }
                    }
                    else if (position == 1) // GroupChats
                    {
                        if (AppSettings.EnableChatGroup)
                        {
                            if (GlobalContext.FloatingActionButton.Tag?.ToString() != "GroupChats")
                            {
                                FloatingActionTag = "GroupChats";
                                GlobalContext.FloatingActionButton.Tag = "GroupChats";
                                GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_add);
                                GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                            }
                        }
                        else if (AppSettings.EnableChatPage)
                        {
                            if (GlobalContext.FloatingActionButton.Tag?.ToString() != "PageChats")
                            {
                                FloatingActionTag = "PageChats";
                                GlobalContext.FloatingActionButton.Tag = "PageChats";
                                GlobalContext.FloatingActionButton.Visibility = ViewStates.Invisible;
                            }
                        }
                        else
                        {
                            if (GlobalContext.FloatingActionButton.Tag?.ToString() != "Call")
                            {
                                FloatingActionTag = "Call";
                                GlobalContext.FloatingActionButton.Tag = "Call";
                                GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_phone_user);
                                GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                            }
                        }
                    }
                    else if (position == 2) // PageChats
                    {
                        if (AppSettings.EnableChatPage)
                        {
                            if (GlobalContext.FloatingActionButton.Tag?.ToString() != "PageChats")
                            {
                                FloatingActionTag = "PageChats";
                                GlobalContext.FloatingActionButton.Tag = "PageChats";
                                GlobalContext.FloatingActionButton.Visibility = ViewStates.Invisible;
                            }
                        }
                        else
                        {
                            if (GlobalContext.FloatingActionButton.Tag?.ToString() != "Call")
                            {
                                FloatingActionTag = "Call";
                                GlobalContext.FloatingActionButton.Tag = "Call";
                                GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_phone_user);
                                GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                            }
                        }
                    }
                    else if (position == 3) // Call
                    {
                        if (GlobalContext.FloatingActionButton.Tag?.ToString() != "Call")
                        {
                            FloatingActionTag = "Call";
                            GlobalContext.FloatingActionButton.Tag = "Call";
                            GlobalContext.FloatingActionButton.SetImageResource(Resource.Drawable.ic_phone_user);
                            GlobalContext.FloatingActionButton.Visibility = ViewStates.Visible;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetUpViewPager(SwipeViewPager viewPager)
        {
            try
            {
                LastCallsTab = new LastCallsFragment();

                MainTabAdapter adapter = new MainTabAdapter(ChildFragmentManager);
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                {
                    LastChatTab = new LastChatFragment();
                    adapter.AddFragment(LastChatTab, GetText(Resource.String.Lbl_Tab_Chats));
                }
                else if (AppSettings.LastChatSystem == SystemApiGetLastChat.Old)
                {
                    LastMessagesTab = new LastMessagesFragment();
                    LastGroupChatsTab = new LastGroupChatsFragment();
                    LastPageChatsTab = new LastPageChatsFragment();

                    adapter.AddFragment(LastMessagesTab, GetText(Resource.String.Lbl_Tab_Chats));

                    if (AppSettings.EnableChatGroup)
                        adapter.AddFragment(LastGroupChatsTab, GetText(Resource.String.Lbl_Tab_GroupChats));

                    if (AppSettings.EnableChatPage)
                        adapter.AddFragment(LastPageChatsTab, GetText(Resource.String.Lbl_Tab_PageChats));
                }
                 
                if (AppSettings.EnableAudioVideoCall)
                    adapter.AddFragment(LastCallsTab, GetText(Resource.String.Lbl_Tab_Calls));

                viewPager.CurrentItem = adapter.Count;
                viewPager.OffscreenPageLimit = adapter.Count;
                viewPager.Adapter = adapter;
                viewPager.Adapter.NotifyDataSetChanged();

                //ViewPager.PageScrolled += ViewPager_PageScrolled;
                ViewPager.PageSelected += ViewPagerOnPageSelected;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region WakeLock System

        public void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Activity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (Activity.CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Activity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(Activity).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)Activity.GetSystemService(Context.PowerService);
                    Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                    Wl.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOnWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)Activity.GetSystemService(Context.PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)Activity.GetSystemService(Context.PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ProximityScreenOff, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during Activity section..
                Wl?.Release();
                Wl = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service

        private void SetService()
        {
            try
            {
                try
                {
                    Receiver = new ServiceResultReceiver(new Handler(Looper.MainLooper));
                    Receiver.SetReceiver(this);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                var intent = new Intent(Activity, typeof(ChatApiService));
                intent.PutExtra("receiverTag", Receiver);
                Activity.StartService(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            //Toast.MakeText(Application.Context, "Result got ",ToastLength.Short)?.Show();

            try
            {
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                {
                    var result = JsonConvert.DeserializeObject<LastChatObject>(resultData.GetString("Json"));
                    if (result != null)
                    {
                        LoadDataJsonLastChat(result);
                    }
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<GetUsersListObject>(resultData.GetString("Json"));
                    if (result != null)
                    {
                        LoadDataJsonLastChat(result);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region LastChat

        public void LoadDataJsonLastChat(LastChatObject result)
        {
            try
            {
                if (LastChatTab.MAdapter == null) return;

                if (!LastChatTab.Run)
                    return;

                LastChatTab.Run = false;

                #region Last Messeges >> users , page , group

                try
                {
                    int countList = LastChatTab.MAdapter.ChatList.Count;

                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in result.Data)
                            {
                                ChatObject checkUser = null;
                                switch (item.ChatType)
                                {
                                    case "user":
                                        checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.UserId == item.UserId);
                                        break;
                                    case "page":
                                        var checkPage = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.PageId == item.PageId && a.LastMessage.LastMessageClass?.ToData?.UserId == item.LastMessage.LastMessageClass?.ToData?.UserId);
                                        if (checkPage != null)
                                        {
                                            var userAdminPage = item.UserId;
                                            if (userAdminPage == item.LastMessage.LastMessageClass.ToData.UserId)
                                            {
                                                var userId = item.LastMessage.LastMessageClass.UserData?.UserId;
                                                checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass.UserData?.UserId == userId);

                                                var name = item.LastMessage.LastMessageClass.UserData?.Name + "(" + item.PageName + ")";
                                                Console.WriteLine(name);
                                            }
                                            else
                                            {
                                                var userId = item.LastMessage.LastMessageClass.ToData.UserId;
                                                checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass.ToData.UserId == userId);

                                                var name = item.LastMessage.LastMessageClass.ToData.Name + "(" + item.PageName + ")";
                                                Console.WriteLine(name);
                                            }
                                        }
                                        else
                                            checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.PageId == item.PageId);
                                        break;
                                    case "group":
                                        checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.GroupId == item.GroupId);
                                        break;
                                }

                                if (checkUser == null)
                                {
                                    //checkUser.ChatTime = item.ChatTime;

                                    LastChatTab.MAdapter.ChatList.Insert(0, item);

                                    Activity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            LastChatTab.MAdapter.NotifyItemInserted(0);
                                            LastChatTab?.MRecycler.ScrollToPosition(LastChatTab.MAdapter.ChatList.IndexOf(item));

                                            if (item.LastMessage.LastMessageClass?.FromId != UserDetails.UserId)
                                            {
                                                var floating = new FloatingObject()
                                                {
                                                    ChatType = item.ChatType,
                                                    UserId = item.UserId,
                                                    PageId = item.PageId,
                                                    GroupId = item.GroupId,
                                                    Avatar = item.Avatar,
                                                    ChatColor = "",
                                                    LastSeen = item.Lastseen,
                                                    LastSeenUnixTime = item.LastseenUnixTime,
                                                    Name = item.Name,
                                                    MessageCount = item.LastMessage.LastMessageClass?.MessageCount ?? "1"
                                                };

                                                switch (item.ChatType)
                                                {
                                                    case "user":
                                                        floating.Name = item.Name;
                                                        break;
                                                    case "page":
                                                        var userAdminPage = item.UserId;
                                                        if (userAdminPage == item.LastMessage.LastMessageClass?.ToData?.UserId)
                                                        {
                                                            floating.Name = item.LastMessage.LastMessageClass?.UserData.Name + "(" + item.PageName + ")";
                                                        }
                                                        else
                                                        {
                                                            floating.Name = item.LastMessage.LastMessageClass?.ToData?.Name + "(" + item.PageName + ")";
                                                        }
                                                        break;
                                                    case "group":
                                                        floating.Name = item.GroupName;
                                                        break;
                                                }

                                                if (UserDetails.SChatHead && InitFloating.CanDrawOverlays(Activity) && Methods.AppLifecycleObserver.AppState == "Background")
                                                    Floating?.FloatingShow(floating);
                                                //else if (!InitFloating.CanDrawOverlays(Activity))
                                                //    DisplayChatHeadDialog();
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                                else
                                {
                                    Activity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            int index = -1;
                                            switch (item.ChatType)
                                            {
                                                case "user":
                                                    index = LastChatTab.MAdapter.ChatList.IndexOf(LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.UserId == item.UserId));
                                                    break;
                                                case "page":
                                                    var checkPage = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.PageId == item.PageId && a.LastMessage.LastMessageClass?.ToData?.UserId == item.LastMessage.LastMessageClass?.ToData?.UserId);
                                                    if (checkPage != null)
                                                    {
                                                        var userAdminPage = item.UserId;
                                                        if (userAdminPage == item.LastMessage.LastMessageClass?.ToData?.UserId)
                                                        {
                                                            var userId = item.LastMessage.LastMessageClass?.UserData?.UserId;
                                                            index = LastChatTab.MAdapter.ChatList.IndexOf(LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass?.UserData?.UserId == userId));
                                                        }
                                                        else
                                                        {
                                                            var userId = item.LastMessage.LastMessageClass?.ToData?.UserId;
                                                            index = LastChatTab.MAdapter.ChatList.IndexOf(LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass?.ToData?.UserId == userId));
                                                        }
                                                    }
                                                    else
                                                        index = LastChatTab.MAdapter.ChatList.IndexOf(LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.PageId == item.PageId));
                                                    break;
                                                case "group":
                                                    index = LastChatTab.MAdapter.ChatList.IndexOf(LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.GroupId == item.GroupId));
                                                    break;
                                            }

                                            if (item.LastMessage.LastMessageClass != null)
                                            {
                                                checkUser.LastMessage.LastMessageClass.ChatColor = item.LastMessage.LastMessageClass.ChatColor ?? AppSettings.MainColor;

                                                checkUser.LastMessage.LastMessageClass.Stickers = item.LastMessage.LastMessageClass.Stickers?.Replace(".mp4", ".gif") ?? "";

                                                if (checkUser.LastMessage.LastMessageClass?.Text != item.LastMessage.LastMessageClass?.Text || checkUser.LastMessage.LastMessageClass?.Media != item.LastMessage.LastMessageClass?.Media)
                                                {
                                                    checkUser.LastMessage = item.LastMessage;
                                                    checkUser.LastMessage.LastMessageClass.Media = item.LastMessage.LastMessageClass.Media;
                                                    checkUser.LastMessage.LastMessageClass.Text = item.LastMessage.LastMessageClass.Text;
                                                    checkUser.LastMessage.LastMessageClass.Stickers = item.LastMessage.LastMessageClass.Stickers;
                                                    checkUser.LastMessage.LastMessageClass.Seen = item.LastMessage.LastMessageClass.Seen;
                                                    checkUser.LastMessage.LastMessageClass.Lat = item.LastMessage.LastMessageClass.Lat;
                                                    checkUser.LastMessage.LastMessageClass.Lng = item.LastMessage.LastMessageClass.Lng;
                                                    checkUser.LastMessage.LastMessageClass.ProductId = item.LastMessage.LastMessageClass.ProductId;
                                                    checkUser.ChatTime = item.ChatTime;
                                                    checkUser.LastseenStatus = item.LastseenStatus;
                                                    checkUser.Name = item.Name;
                                                    checkUser.Showlastseen = item.Showlastseen;

                                                    if (index > 0 && checkUser.ChatType == item.ChatType)
                                                    {
                                                        LastChatTab.MAdapter.ChatList.Move(index, 0);
                                                        LastChatTab.MAdapter.NotifyItemMoved(index, 0);

                                                        if (item.LastMessage.LastMessageClass.FromId != UserDetails.UserId)
                                                        {
                                                            var floating = new FloatingObject()
                                                            {
                                                                ChatType = item.ChatType,
                                                                UserId = item.UserId,
                                                                PageId = item.PageId,
                                                                GroupId = item.GroupId,
                                                                Avatar = item.Avatar,
                                                                ChatColor = "",
                                                                LastSeen = item.Lastseen,
                                                                LastSeenUnixTime = item.LastseenUnixTime,
                                                                Name = item.Name,
                                                                MessageCount = item.LastMessage.LastMessageClass.MessageCount ?? "1"
                                                            };

                                                            switch (item.ChatType)
                                                            {
                                                                case "user":
                                                                    floating.Name = item.Name;
                                                                    break;
                                                                case "page":
                                                                    var userAdminPage = item.UserId;
                                                                    if (userAdminPage == item.LastMessage.LastMessageClass.ToData?.UserId)
                                                                    {
                                                                        floating.Name = item.LastMessage.LastMessageClass.UserData?.Name + "(" + item.PageName + ")";
                                                                    }
                                                                    else
                                                                    {
                                                                        floating.Name = item.LastMessage.LastMessageClass.ToData?.Name + "(" + item.PageName + ")";
                                                                    }
                                                                    break;
                                                                case "group":
                                                                    floating.Name = item.GroupName;
                                                                    break;
                                                            }

                                                            if (UserDetails.SChatHead && InitFloating.CanDrawOverlays(Activity) && Methods.AppLifecycleObserver.AppState == "Background")
                                                                Floating?.FloatingShow(floating);
                                                            //else if (!InitFloating.CanDrawOverlays(Activity))
                                                            //    DisplayChatHeadDialog();
                                                        }
                                                    }
                                                    else if (index == 0 && checkUser.ChatType == item.ChatType)
                                                    {
                                                        LastChatTab.MAdapter.NotifyItemChanged(index);
                                                    }
                                                }
                                                else if (checkUser.LastseenStatus?.ToLower() != item.LastseenStatus?.ToLower())
                                                {
                                                    checkUser.LastMessage = item.LastMessage;
                                                    checkUser.LastMessage.LastMessageClass.Media = item.LastMessage.LastMessageClass.Media;
                                                    checkUser.LastMessage.LastMessageClass.Text = item.LastMessage.LastMessageClass.Text;
                                                    checkUser.LastMessage.LastMessageClass.Stickers = item.LastMessage.LastMessageClass.Stickers;
                                                    checkUser.LastMessage.LastMessageClass.Seen = item.LastMessage.LastMessageClass.Seen;
                                                    checkUser.LastMessage.LastMessageClass.Lat = item.LastMessage.LastMessageClass.Lat;
                                                    checkUser.LastMessage.LastMessageClass.Lng = item.LastMessage.LastMessageClass.Lng;
                                                    checkUser.LastMessage.LastMessageClass.ProductId = item.LastMessage.LastMessageClass.ProductId;
                                                    checkUser.ChatTime = item.ChatTime;
                                                    checkUser.LastseenStatus = item.LastseenStatus;
                                                    checkUser.Name = item.Name;
                                                    checkUser.Showlastseen = item.Showlastseen;

                                                    if (index > -1 && checkUser.ChatType == item.ChatType)
                                                        LastChatTab.MAdapter.NotifyItemChanged(index);
                                                }
                                                else if (checkUser.ChatTime != item.ChatTime)
                                                {
                                                    checkUser.LastMessage = item.LastMessage;
                                                    checkUser.ChatTime = item.ChatTime;
                                                    checkUser.Showlastseen = item.Showlastseen;
                                                    checkUser.Name = item.Name;

                                                    if (index > -1 && checkUser.ChatType == item.ChatType)
                                                        LastChatTab.MAdapter.NotifyItemChanged(index);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                        }
                        else
                        {
                            LastChatTab.MAdapter.ChatList = new ObservableCollection<ChatObject>(result.Data);
                            Activity.RunOnUiThread(() => { LastChatTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (LastChatTab.MAdapter.ChatList.Count > 10 && !LastChatTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                #endregion

                #region Call >> Video_Call_User

                try
                {
                    if (AppSettings.EnableAudioVideoCall)
                    {
                        if (AppSettings.UseLibrary == SystemCall.Twilio)
                        {
                            bool twilioVideoCall = result.VideoCall ?? false;
                            bool twilioAudioCall = result.AudioCall ?? false;

                            if (AppSettings.EnableVideoCall)
                            {
                                #region Twilio Video call

                                if (twilioVideoCall && LastChatTab.SecondPassed >= 5 && !RunCall)
                                {
                                    var callUser = result.VideoCallUser?.CallUserClass;
                                    if (callUser != null)
                                    {
                                        RunCall = true;

                                        var userId = callUser.UserId;
                                        var avatar = callUser.Avatar;
                                        var name = callUser.Name;

                                        var videosData = callUser.Data;
                                        if (videosData != null)
                                        {
                                            var id = videosData.Id; //call_id
                                            var accessToken = videosData.AccessToken;
                                            var accessToken2 = videosData.AccessToken2;
                                            var fromId = videosData.FromId;
                                            var active = videosData.Active;
                                            var time = videosData.Called;
                                            var declined = videosData.Declined;
                                            var roomName = videosData.RoomName;

                                            Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                            intent.PutExtra("UserID", userId);
                                            intent.PutExtra("avatar", avatar);
                                            intent.PutExtra("name", name);
                                            intent.PutExtra("access_token", accessToken);
                                            intent.PutExtra("access_token_2", accessToken2);
                                            intent.PutExtra("from_id", fromId);
                                            intent.PutExtra("active", active);
                                            intent.PutExtra("time", time);
                                            intent.PutExtra("CallID", id);
                                            intent.PutExtra("status", declined);
                                            intent.PutExtra("room_name", roomName);
                                            intent.PutExtra("declined", declined);
                                            intent.PutExtra("type", "Twilio_video_call");

                                            string avatarSplit = avatar.Split('/').Last();
                                            var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                            if (getImg == "File Dont Exists")
                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                            if (LastChatTab.SecondPassed < 5)
                                                LastChatTab.TimerCallingTimePassed.Start();
                                            else
                                            {
                                                RunCall = false;
                                                LastChatTab.TimerCallingTimePassed.Stop();

                                                LastChatTab.SecondPassed = 0;

                                                Activity.RunOnUiThread(() =>
                                                {
                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                    {
                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                        StartActivity(intent);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                else if (twilioVideoCall == false && twilioAudioCall == false)
                                {
                                    if (LastChatTab.SecondPassed > 5)
                                    {
                                        RunCall = false;

                                        LastChatTab.SecondPassed = 0;

                                        if (VideoAudioComingCallActivity.IsActive)
                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                    }
                                }
                                else
                                {
                                    RunCall = false;
                                }

                                #endregion
                            }

                            if (AppSettings.EnableAudioCall)
                            {
                                #region Twilio Audio call

                                if (twilioAudioCall && !RunCall)
                                {
                                    var callUser = result.AudioCallUser?.CallUserClass;
                                    if (callUser != null)
                                    {
                                        RunCall = true;

                                        var userId = callUser.UserId;
                                        var avatar = callUser.Avatar;
                                        var name = callUser.Name;

                                        var videosData = callUser.Data;
                                        if (videosData != null)
                                        {
                                            var id = videosData.Id; //call_id
                                            var accessToken = videosData.AccessToken;
                                            var accessToken2 = videosData.AccessToken2;
                                            var fromId = videosData.FromId;
                                            var active = videosData.Active;
                                            var time = videosData.Called;
                                            var declined = videosData.Declined;
                                            var roomName = videosData.RoomName;

                                            Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                            intent.PutExtra("UserID", userId);
                                            intent.PutExtra("avatar", avatar);
                                            intent.PutExtra("name", name);
                                            intent.PutExtra("access_token", accessToken);
                                            intent.PutExtra("access_token_2", accessToken2);
                                            intent.PutExtra("from_id", fromId);
                                            intent.PutExtra("active", active);
                                            intent.PutExtra("time", time);
                                            intent.PutExtra("CallID", id);
                                            intent.PutExtra("status", declined);
                                            intent.PutExtra("room_name", roomName);
                                            intent.PutExtra("declined", declined);
                                            intent.PutExtra("type", "Twilio_audio_call");

                                            string avatarSplit = avatar.Split('/').Last();
                                            var getImg =
                                                Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage,
                                                    avatarSplit);
                                            if (getImg == "File Dont Exists")
                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(
                                                    Methods.Path.FolderDiskImage, avatar);

                                            if (LastChatTab.SecondPassed < 5)
                                                LastChatTab.TimerCallingTimePassed.Start();
                                            else
                                            {
                                                RunCall = false;

                                                LastChatTab.TimerCallingTimePassed.Stop();

                                                LastChatTab.SecondPassed = 0;

                                                Activity.RunOnUiThread(() =>
                                                {
                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                    {
                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                        StartActivity(intent);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                else if (twilioAudioCall == false && twilioVideoCall == false)
                                {
                                    if (LastChatTab.SecondPassed >= 5)
                                    {
                                        RunCall = false;

                                        if (VideoAudioComingCallActivity.IsActive)
                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                    }
                                }
                                else
                                {
                                    RunCall = false;

                                    if (VideoAudioComingCallActivity.IsActive)
                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                }

                                #endregion
                            }
                        }
                        else if (AppSettings.UseLibrary == SystemCall.Agora)
                        {
                            #region Agora Audio/Video call

                            var agoraCall = result.AgoraCall ?? false;
                            if (agoraCall && LastChatTab.SecondPassed >= 5 && !RunCall)
                            {
                                var callUser = result.AgoraCallData?.CallUserClass;

                                if (callUser != null)
                                {
                                    RunCall = true;

                                    var userId = callUser.UserId;
                                    var avatar = callUser.Avatar;
                                    var name = callUser.Name;

                                    var videosData = callUser.Data;
                                    if (videosData != null)
                                    {
                                        var id = videosData.Id; //call_id
                                        //var accessToken = videosData.AccessToken;
                                        //var accessToken2 = videosData.AccessToken2;
                                        var fromId = videosData.FromId;
                                        //var active = videosData.Active;
                                        var time = videosData.Called;
                                        //var declined = videosData.Declined;
                                        var roomName = videosData.RoomName;
                                        var type = videosData.Type;
                                        var status = videosData.Status;

                                        string avatarSplit = avatar.Split('/').Last();
                                        var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                        if (getImg == "File Dont Exists")
                                            Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                        if (type == "video")
                                        {
                                            if (AppSettings.EnableVideoCall)
                                            {
                                                Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                                intent.PutExtra("UserID", userId);
                                                intent.PutExtra("avatar", avatar);
                                                intent.PutExtra("name", name);
                                                intent.PutExtra("from_id", fromId);
                                                intent.PutExtra("status", status);
                                                intent.PutExtra("time", time);
                                                intent.PutExtra("CallID", id);
                                                intent.PutExtra("room_name", roomName);
                                                intent.PutExtra("type", "Agora_video_call_recieve");
                                                intent.PutExtra("declined", "0");

                                                if (!VideoAudioComingCallActivity.IsActive)
                                                {
                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                    StartActivity(intent);
                                                }
                                            }
                                        }
                                        else if (type == "audio")
                                        {
                                            if (AppSettings.EnableAudioCall)
                                            {
                                                Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                                intent.PutExtra("UserID", userId);
                                                intent.PutExtra("avatar", avatar);
                                                intent.PutExtra("name", name);
                                                intent.PutExtra("from_id", fromId);
                                                intent.PutExtra("status", status);
                                                intent.PutExtra("time", time);
                                                intent.PutExtra("CallID", id);
                                                intent.PutExtra("room_name", roomName);
                                                intent.PutExtra("type", "Agora_audio_call_recieve");
                                                intent.PutExtra("declined", "0");

                                                if (LastChatTab.SecondPassed < 5)
                                                    LastChatTab.TimerCallingTimePassed.Start();
                                                else
                                                {
                                                    RunCall = false;
                                                    LastChatTab.TimerCallingTimePassed.Stop();

                                                    LastChatTab.SecondPassed = 0;


                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                    {
                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                        StartActivity(intent);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (agoraCall == false)
                            {
                                if (LastChatTab.SecondPassed >= 5)
                                {
                                    RunCall = false;

                                    LastChatTab.SecondPassed = 0;

                                    if (VideoAudioComingCallActivity.IsActive)
                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                }
                            }
                            else
                            {
                                RunCall = false;

                                if (VideoAudioComingCallActivity.IsActive)
                                    VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                #endregion

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception e)
            {
                LastChatTab.Run = true;
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadDataJsonLastChat(GetUsersListObject result)
        {
            try
            {
                if (LastMessagesTab.MAdapter == null) return;

                if (LastMessagesTab.FirstRun && LastMessagesTab.MAdapter.MLastMessagesUser.Count > 0)
                {
                    LastMessagesTab.FirstRun = false;
                }

                #region Last Messeges >> users

                try
                {
                    int countList = LastMessagesTab.MAdapter.MLastMessagesUser.Count;

                    var respondList = result.Users?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in result.Users)
                            {
                                var checkUser = LastMessagesTab.MAdapter.MLastMessagesUser.FirstOrDefault(a => a.UserId == item.UserId);
                                if (checkUser == null)
                                {
                                    //checkUser.ChatTime = item.ChatTime;
                                    LastMessagesTab.MAdapter.MLastMessagesUser.Insert(0, item);

                                    Activity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            LastMessagesTab.MAdapter.NotifyItemInserted(0);
                                            LastMessagesTab?.MRecycler.ScrollToPosition(LastMessagesTab.MAdapter.MLastMessagesUser.IndexOf(item));


                                            if (item.LastMessage.FromId != UserDetails.UserId)
                                            {
                                                var floating = new FloatingObject()
                                                {
                                                    ChatType = "user",
                                                    UserId = item.UserId,
                                                    PageId = "",
                                                    GroupId = "",
                                                    Avatar = item.Avatar,
                                                    ChatColor = item.ChatColor,
                                                    LastSeen = item.Lastseen,
                                                    LastSeenUnixTime = item.LastseenUnixTime,
                                                    Name = item.Name,
                                                    MessageCount = "1"
                                                };

                                                if (UserDetails.SChatHead && InitFloating.CanDrawOverlays(Activity) && Methods.AppLifecycleObserver.AppState == "Background")
                                                    Floating?.FloatingShow(floating);
                                                //else if (!InitFloating.CanDrawOverlays(Activity))
                                                //    DisplayChatHeadDialog();
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                                else
                                {
                                    var index = LastMessagesTab.MAdapter.MLastMessagesUser.IndexOf(checkUser);
                                    checkUser.ChatTime = item.ChatTime;
                                    checkUser.UserId = item.UserId;
                                    checkUser.Username = item.Username;
                                    checkUser.Avatar = item.Avatar;
                                    checkUser.Cover = item.Cover;
                                    checkUser.LastseenTimeText = item.LastseenTimeText;
                                    checkUser.Lastseen = item.Lastseen;
                                    checkUser.Url = item.Url;
                                    checkUser.Name = item.Name;
                                    checkUser.LastseenUnixTime = item.LastseenUnixTime;
                                    checkUser.ChatColor = item.ChatColor ?? AppSettings.MainColor;
                                    checkUser.Verified = item.Verified;

                                    //last_message
                                    checkUser.LastMessage ??= new GetUsersListObject.LastMessage();

                                    checkUser.LastMessage.Stickers ??= "";

                                    checkUser.LastMessage.Id = item.LastMessage.Id;
                                    checkUser.LastMessage.FromId = item.LastMessage.FromId;
                                    checkUser.LastMessage.GroupId = item.LastMessage.GroupId;
                                    checkUser.LastMessage.ToId = item.LastMessage.ToId;
                                    checkUser.LastMessage.MediaFileName = item.LastMessage.MediaFileName;
                                    checkUser.LastMessage.MediaFileNames = item.LastMessage.MediaFileNames;
                                    checkUser.LastMessage.Time = item.LastMessage.Time;
                                    checkUser.LastMessage.Lat = item.LastMessage.Lat;
                                    checkUser.LastMessage.Lng = item.LastMessage.Lng;
                                    checkUser.LastMessage.ProductId = item.LastMessage.ProductId;

                                    if (checkUser.LastMessage.Seen != "2")
                                    {
                                        checkUser.LastMessage.Seen = item.LastMessage.Seen;
                                    }
                                    else if (item.LastMessage.Seen == "0" && checkUser.LastMessage.Seen == "2")
                                    {
                                        checkUser.LastMessage.Seen = "0";
                                    }
                                    else if (item.LastMessage.Seen == "1" && checkUser.LastMessage.Seen == "2")
                                    {
                                        checkUser.LastMessage.Seen = "0";
                                    }

                                    checkUser.LastMessage.DeletedOne = item.LastMessage.DeletedOne;
                                    checkUser.LastMessage.DeletedTwo = item.LastMessage.DeletedTwo;
                                    checkUser.LastMessage.SentPush = item.LastMessage.SentPush;
                                    checkUser.LastMessage.NotificationId = item.LastMessage.NotificationId;
                                    checkUser.LastMessage.TypeTwo = item.LastMessage.TypeTwo;
                                    checkUser.LastMessage.Stickers = item.LastMessage.Stickers?.Replace(".mp4", ".gif") ?? "";

                                    checkUser.OldAvatar = item.OldAvatar;
                                    checkUser.OldCover = item.OldCover;

                                    if (checkUser.LastMessage.Text != item.LastMessage.Text & string.IsNullOrEmpty(checkUser.LastMessage.MediaFileName))
                                    {
                                        checkUser.UserId = item.UserId;
                                        checkUser.Username = item.Username;
                                        checkUser.Avatar = item.Avatar;
                                        checkUser.Cover = item.Cover;
                                        checkUser.Lastseen = item.Lastseen;
                                        checkUser.Url = item.Url;
                                        checkUser.Name = item.Name;
                                        checkUser.ChatColor = item.ChatColor ?? AppSettings.MainColor;
                                        checkUser.Verified = item.Verified;
                                        checkUser.LastMessage.Text = item.LastMessage.Text;
                                        if (index > -1)
                                        {
                                            MessageController.UpdateRecyclerLastMessageView(checkUser.LastMessage, checkUser, index, this);

                                            LastMessagesTab.MAdapter.MLastMessagesUser.Move(index, 0);
                                            Activity.RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    LastMessagesTab.MAdapter.NotifyItemMoved(index, 0);

                                                    if (item.LastMessage.FromId != UserDetails.UserId)
                                                    {
                                                        var floating = new FloatingObject()
                                                        {
                                                            ChatType = "user",
                                                            UserId = item.UserId,
                                                            PageId = "",
                                                            GroupId = "",
                                                            Avatar = item.Avatar,
                                                            ChatColor = item.ChatColor ?? AppSettings.MainColor,
                                                            LastSeen = item.Lastseen,
                                                            LastSeenUnixTime = item.LastseenUnixTime,
                                                            Name = item.Name,
                                                            MessageCount = "1"
                                                        };

                                                        if (UserDetails.SChatHead && InitFloating.CanDrawOverlays(Activity) && Methods.AppLifecycleObserver.AppState == "Background")
                                                            Floating?.FloatingShow(floating);
                                                        //else if (!InitFloating.CanDrawOverlays(Activity))
                                                        //    DisplayChatHeadDialog();
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Methods.DisplayReportResultTrack(e);
                                                }
                                            });
                                        }
                                    }

                                    if (checkUser.LastMessage.Media != item.LastMessage.Media)
                                    {
                                        checkUser.LastMessage.Media = item.LastMessage.Media;
                                        if (index > -1)
                                        {
                                            checkUser.UserId = item.UserId;
                                            checkUser.Username = item.Username;
                                            checkUser.Avatar = item.Avatar;
                                            checkUser.Cover = item.Cover;
                                            checkUser.Lastseen = item.Lastseen;
                                            checkUser.Url = item.Url;
                                            checkUser.Name = item.Name;
                                            checkUser.ChatColor = item.ChatColor ?? AppSettings.MainColor;
                                            checkUser.Verified = item.Verified;
                                            MessageController.UpdateRecyclerLastMessageView(checkUser.LastMessage, checkUser, index, this);

                                            Activity.RunOnUiThread(() =>
                                            {
                                                LastMessagesTab.MAdapter.MLastMessagesUser.Move(index, 0);
                                                LastMessagesTab.MAdapter.NotifyItemMoved(index, 0);
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            LastMessagesTab.MAdapter.MLastMessagesUser = new ObservableCollection<GetUsersListObject.User>(result.Users);
                            Activity.RunOnUiThread(() => { LastMessagesTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (LastMessagesTab.MAdapter.MLastMessagesUser.Count > 10 && !LastMessagesTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                #endregion

                #region Call >> Video_Call_User

                try
                {
                    if (AppSettings.EnableAudioVideoCall)
                    {
                        if (AppSettings.UseLibrary == SystemCall.Twilio)
                        {
                            bool twilioVideoCall = result.VideoCall ?? false;
                            bool twilioAudioCall = result.AudioCall ?? false;

                            if (AppSettings.EnableVideoCall)
                            {
                                #region Twilio Video call

                                if (twilioVideoCall && LastMessagesTab.SecondPassed >= 5 && !RunCall)
                                {
                                    var callUser = result.VideoCallUser?.CallUserClass;
                                    if (callUser != null)
                                    {
                                        RunCall = true;

                                        var userId = callUser.UserId;
                                        var avatar = callUser.Avatar;
                                        var name = callUser.Name;

                                        var videosData = callUser.Data;
                                        if (videosData != null)
                                        {
                                            var id = videosData.Id; //call_id
                                            var accessToken = videosData.AccessToken;
                                            var accessToken2 = videosData.AccessToken2;
                                            var fromId = videosData.FromId;
                                            var active = videosData.Active;
                                            var time = videosData.Called;
                                            var declined = videosData.Declined;
                                            var roomName = videosData.RoomName;

                                            Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                            intent.PutExtra("UserID", userId);
                                            intent.PutExtra("avatar", avatar);
                                            intent.PutExtra("name", name);
                                            intent.PutExtra("access_token", accessToken);
                                            intent.PutExtra("access_token_2", accessToken2);
                                            intent.PutExtra("from_id", fromId);
                                            intent.PutExtra("active", active);
                                            intent.PutExtra("time", time);
                                            intent.PutExtra("CallID", id);
                                            intent.PutExtra("status", declined);
                                            intent.PutExtra("room_name", roomName);
                                            intent.PutExtra("declined", declined);
                                            intent.PutExtra("type", "Twilio_video_call");

                                            string avatarSplit = avatar.Split('/').Last();
                                            var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                            if (getImg == "File Dont Exists")
                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                            if (LastMessagesTab.SecondPassed < 5)
                                                LastMessagesTab.TimerCallingTimePassed.Start();
                                            else
                                            {
                                                RunCall = false;
                                                LastMessagesTab.TimerCallingTimePassed.Stop();

                                                LastMessagesTab.SecondPassed = 0;

                                                Activity.RunOnUiThread(() =>
                                                {
                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                    {
                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                        StartActivity(intent);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                else if (twilioVideoCall == false && twilioAudioCall == false)
                                {
                                    if (LastMessagesTab.SecondPassed > 5)
                                    {
                                        RunCall = false;

                                        LastMessagesTab.SecondPassed = 0;

                                        if (VideoAudioComingCallActivity.IsActive)
                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                    }
                                }
                                else
                                {
                                    RunCall = false;
                                }

                                #endregion
                            }

                            if (AppSettings.EnableAudioCall)
                            {
                                #region Twilio Audio call

                                if (twilioAudioCall && !RunCall)
                                {
                                    var callUser = result.AudioCallUser?.CallUserClass;
                                    if (callUser != null)
                                    {
                                        RunCall = true;

                                        var userId = callUser.UserId;
                                        var avatar = callUser.Avatar;
                                        var name = callUser.Name;

                                        var videosData = callUser.Data;
                                        if (videosData != null)
                                        {
                                            var id = videosData.Id; //call_id
                                            var accessToken = videosData.AccessToken;
                                            var accessToken2 = videosData.AccessToken2;
                                            var fromId = videosData.FromId;
                                            var active = videosData.Active;
                                            var time = videosData.Called;
                                            var declined = videosData.Declined;
                                            var roomName = videosData.RoomName;

                                            Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                            intent.PutExtra("UserID", userId);
                                            intent.PutExtra("avatar", avatar);
                                            intent.PutExtra("name", name);
                                            intent.PutExtra("access_token", accessToken);
                                            intent.PutExtra("access_token_2", accessToken2);
                                            intent.PutExtra("from_id", fromId);
                                            intent.PutExtra("active", active);
                                            intent.PutExtra("time", time);
                                            intent.PutExtra("CallID", id);
                                            intent.PutExtra("status", declined);
                                            intent.PutExtra("room_name", roomName);
                                            intent.PutExtra("declined", declined);
                                            intent.PutExtra("type", "Twilio_audio_call");

                                            string avatarSplit = avatar.Split('/').Last();
                                            var getImg =
                                                Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage,
                                                    avatarSplit);
                                            if (getImg == "File Dont Exists")
                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(
                                                    Methods.Path.FolderDiskImage, avatar);

                                            if (LastMessagesTab.SecondPassed < 5)
                                                LastMessagesTab.TimerCallingTimePassed.Start();
                                            else
                                            {
                                                RunCall = false;

                                                LastMessagesTab.TimerCallingTimePassed.Stop();

                                                LastMessagesTab.SecondPassed = 0;

                                                Activity.RunOnUiThread(() =>
                                                {
                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                    {
                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                        StartActivity(intent);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                else if (twilioAudioCall == false && twilioVideoCall == false)
                                {
                                    if (LastMessagesTab.SecondPassed >= 5)
                                    {
                                        RunCall = false;

                                        if (VideoAudioComingCallActivity.IsActive)
                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                    }
                                }
                                else
                                {
                                    RunCall = false;
                                }

                                #endregion
                            }
                        }
                        else if (AppSettings.UseLibrary == SystemCall.Agora)
                        {
                            #region Agora Audio/Video call

                            var agoraCall = result.AgoraCall ?? false;
                            if (agoraCall && LastMessagesTab.SecondPassed >= 5 && !RunCall)
                            {
                                var callUser = result.AgoraCallData?.CallUserClass;

                                if (callUser != null)
                                {
                                    RunCall = true;

                                    var userId = callUser.UserId;
                                    var avatar = callUser.Avatar;
                                    var name = callUser.Name;

                                    var videosData = callUser.Data;
                                    if (videosData != null)
                                    {
                                        var id = videosData.Id; //call_id
                                        //var accessToken = videosData.AccessToken;
                                        //var accessToken2 = videosData.AccessToken2;
                                        var fromId = videosData.FromId;
                                        //var active = videosData.Active;
                                        var time = videosData.Called;
                                        //var declined = videosData.Declined;
                                        var roomName = videosData.RoomName;
                                        var type = videosData.Type;
                                        var status = videosData.Status;

                                        string avatarSplit = avatar.Split('/').Last();
                                        var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                        if (getImg == "File Dont Exists")
                                            Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                        if (type == "video")
                                        {
                                            if (AppSettings.EnableVideoCall)
                                            {
                                                Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                                intent.PutExtra("UserID", userId);
                                                intent.PutExtra("avatar", avatar);
                                                intent.PutExtra("name", name);
                                                intent.PutExtra("from_id", fromId);
                                                intent.PutExtra("status", status);
                                                intent.PutExtra("time", time);
                                                intent.PutExtra("CallID", id);
                                                intent.PutExtra("room_name", roomName);
                                                intent.PutExtra("type", "Agora_video_call_recieve");
                                                intent.PutExtra("declined", "0");

                                                if (!VideoAudioComingCallActivity.IsActive)
                                                {
                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                    StartActivity(intent);
                                                }
                                            }
                                        }
                                        else if (type == "audio")
                                        {
                                            if (AppSettings.EnableAudioCall)
                                            {
                                                Intent intent = new Intent(Activity, typeof(VideoAudioComingCallActivity));
                                                intent.PutExtra("UserID", userId);
                                                intent.PutExtra("avatar", avatar);
                                                intent.PutExtra("name", name);
                                                intent.PutExtra("from_id", fromId);
                                                intent.PutExtra("status", status);
                                                intent.PutExtra("time", time);
                                                intent.PutExtra("CallID", id);
                                                intent.PutExtra("room_name", roomName);
                                                intent.PutExtra("type", "Agora_audio_call_recieve");
                                                intent.PutExtra("declined", "0");

                                                if (LastMessagesTab.SecondPassed < 5)
                                                    LastMessagesTab.TimerCallingTimePassed.Start();
                                                else
                                                {
                                                    RunCall = false;
                                                    LastMessagesTab.TimerCallingTimePassed.Stop();

                                                    LastMessagesTab.SecondPassed = 0;


                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                    {
                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                        StartActivity(intent);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (agoraCall == false)
                            {
                                if (LastMessagesTab.SecondPassed >= 5)
                                {
                                    RunCall = false;

                                    LastMessagesTab.SecondPassed = 0;

                                    if (VideoAudioComingCallActivity.IsActive)
                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();

                                }
                            }

                            #endregion
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                #endregion

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadUserMessagesDuringScroll(GetUsersListObject result)
        {
            try
            {
                int countList = LastMessagesTab.MAdapter.MLastMessagesUser.Count;

                var respondList = result.Users?.Count;
                if (respondList > 0)
                {
                    if (countList > 5)
                    {
                        foreach (var item in result.Users)
                        {
                            if (LastMessagesTab.MAdapter.MLastMessagesUser.FirstOrDefault(a => a.UserId == item.UserId) == null)
                                LastMessagesTab.MAdapter.MLastMessagesUser.Add(item);
                        }

                        Activity.RunOnUiThread(() => { LastMessagesTab.MAdapter.NotifyItemRangeInserted(countList, LastMessagesTab.MAdapter.ItemCount); });
                    }
                    else
                    {
                        LastMessagesTab.MAdapter.MLastMessagesUser = new ObservableCollection<GetUsersListObject.User>(result.Users);
                        Activity.RunOnUiThread(() => { LastMessagesTab.MAdapter.NotifyDataSetChanged(); });
                    }

                    ListUtils.UserChatList = LastMessagesTab.MAdapter.MLastMessagesUser;
                }
                else
                {
                    if (LastMessagesTab.MAdapter.MLastMessagesUser.Count > 10 && !LastMessagesTab.MRecycler.CanScrollVertically(1))
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadUserMessagesDuringScroll(LastChatObject result)
        {
            try
            {
                int countList = LastChatTab.MAdapter.ChatList.Count;

                var respondList = result.Data?.Count;
                if (respondList > 0)
                {
                    if (countList > 5)
                    {
                        foreach (var item in result.Data)
                        {
                            ChatObject checkUser = null;
                            switch (item.ChatType)
                            {
                                case "user":
                                    checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.UserId == item.UserId);
                                    break;
                                case "page":
                                    var checkPage = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.PageId == item.PageId && a.LastMessage.LastMessageClass?.ToData?.UserId == item.LastMessage.LastMessageClass?.ToData?.UserId);
                                    if (checkPage != null)
                                    {
                                        var userAdminPage = item.UserId;
                                        if (userAdminPage == item.LastMessage.LastMessageClass.ToData.UserId)
                                        {
                                            var userId = item.LastMessage.LastMessageClass.UserData?.UserId;
                                            checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass.UserData?.UserId == userId);

                                            var name = item.LastMessage.LastMessageClass.UserData?.Name + "(" + item.PageName + ")";
                                            Console.WriteLine(name);
                                        }
                                        else
                                        {
                                            var userId = item.LastMessage.LastMessageClass.ToData.UserId;
                                            checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass.ToData.UserId == userId);

                                            var name = item.LastMessage.LastMessageClass.ToData.Name + "(" + item.PageName + ")";
                                            Console.WriteLine(name);
                                        }
                                    }
                                    else
                                        checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.PageId == item.PageId);
                                    break;
                                //break;
                                case "group":
                                    checkUser = LastChatTab.MAdapter.ChatList.FirstOrDefault(a => a.GroupId == item.GroupId);
                                    break;
                            }

                            if (checkUser == null)
                            {
                                LastChatTab.MAdapter.ChatList.Add(item);
                            }
                        }

                        Activity.RunOnUiThread(() => { LastChatTab.MAdapter.NotifyItemRangeInserted(countList, LastChatTab.MAdapter.ItemCount); });
                    }
                    else
                    {
                        LastChatTab.MAdapter.ChatList = new ObservableCollection<ChatObject>(result.Data);
                        Activity.RunOnUiThread(() => { LastChatTab.MAdapter.NotifyDataSetChanged(); });
                    }

                    ListUtils.UserList = LastChatTab.MAdapter.ChatList;
                }
                else
                {
                    if (LastChatTab.MAdapter.ChatList.Count > 10 && !LastChatTab.MRecycler.CanScrollVertically(1))
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.Old)
                {
                    if (LastMessagesTab.SwipeRefreshLayout.Refreshing)
                        LastMessagesTab.SwipeRefreshLayout.Refreshing = false;

                    LastMessagesTab.MainScrollEvent.IsLoading = false;

                    if (LastMessagesTab.MAdapter.MLastMessagesUser.Count > 0)
                    {
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        ListUtils.UserChatList = LastMessagesTab.MAdapter.MLastMessagesUser;

                        LastMessagesTab.MRecycler.Visibility = ViewStates.Visible;
                        LastMessagesTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                        //Insert All data users to database
                        dbDatabase.Insert_Or_Update_LastUsersChat(Activity, LastMessagesTab.MAdapter.MLastMessagesUser);
                    }
                    else
                    {
                        LastMessagesTab.MRecycler.Visibility = ViewStates.Gone;
                        LastMessagesTab.MRecycler.Visibility = ViewStates.Gone;

                        LastMessagesTab.Inflated ??= LastMessagesTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(LastMessagesTab.Inflated, EmptyStateInflater.Type.NoMessages);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }

                        LastMessagesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else
                {
                    if (LastChatTab.SwipeRefreshLayout.Refreshing)
                        LastChatTab.SwipeRefreshLayout.Refreshing = false;

                    LastChatTab.MainScrollEvent.IsLoading = false;

                    if (LastChatTab.MAdapter.ChatList.Count > 0)
                    {
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        LastChatTab.MRecycler.Visibility = ViewStates.Visible;
                        LastChatTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                        //Insert All data users to database
                        dbDatabase.Insert_Or_Update_LastUsersChat(Activity, LastChatTab.MAdapter.ChatList);
                    }
                    else
                    {
                        LastChatTab.MRecycler.Visibility = ViewStates.Gone;

                        LastChatTab.Inflated ??= LastChatTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(LastChatTab.Inflated, EmptyStateInflater.Type.NoMessages);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }

                        LastChatTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }

                    LastChatTab.Run = true;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                if (AppSettings.LastChatSystem == SystemApiGetLastChat.Old)
                {
                    if (LastMessagesTab.SwipeRefreshLayout.Refreshing)
                        LastMessagesTab.SwipeRefreshLayout.Refreshing = false;

                    LastMessagesTab.MainScrollEvent.IsLoading = false;
                }
                else
                {
                    if (LastChatTab.SwipeRefreshLayout.Refreshing)
                        LastChatTab.SwipeRefreshLayout.Refreshing = false;

                    LastChatTab.MainScrollEvent.IsLoading = false;
                }
            }
        }

        #endregion

        //private void OpenDialogGallery(string typeImage)
        //{
        //    try
        //    {
        //        ImageType = typeImage;
        //        // Check if we're running on Android 5.0 or higher
        //        if ((int)Build.VERSION.SdkInt < 23)
        //        {
        //            Methods.Path.Chack_MyFolder();

        //            //Open Image 
        //            var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
        //            CropImage.Activity()
        //                .SetInitialCropWindowPaddingRatio(0)
        //                .SetAutoZoomEnabled(true)
        //                .SetMaxZoom(4)
        //                .SetGuidelines(CropImageView.Guidelines.On)
        //                .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
        //                .SetOutputUri(myUri).Start(Activity);
        //        }
        //        else
        //        {
        //            if (!CropImage.IsExplicitCameraPermissionRequired(Activity) && Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
        //                Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
        //            {
        //                Methods.Path.Chack_MyFolder();

        //                //Open Image 
        //                var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
        //                CropImage.Activity()
        //                    .SetInitialCropWindowPaddingRatio(0)
        //                    .SetAutoZoomEnabled(true)
        //                    .SetMaxZoom(4)
        //                    .SetGuidelines(CropImageView.Guidelines.On)
        //                    .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
        //                    .SetOutputUri(myUri).Start(Activity);
        //            }
        //            else
        //            {
        //                new PermissionsController(Activity).RequestPermission(108);
        //            }
        //        }  
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}
         
        private void GetGeneralAppData()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    if (Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                        Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted &&
                        Activity.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted &&
                        Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { CheckAndGetLocation });
                    }
                    else
                    {
                        // 100 >> Storage , 103 >> Camera , 105 >> Location , >> 102 RecordAudio
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage,
                            Manifest.Permission.Camera,
                            Manifest.Permission.AccessFineLocation,
                            Manifest.Permission.AccessCoarseLocation,
                            Manifest.Permission.AccessMediaLocation,
                        }, 123);
                    }
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { CheckAndGetLocation });
                }

                var sqlEntity = new SqLiteDatabase();
                var data = sqlEntity.Get_data_Login_Credentials();
                if (data != null && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);

                    if (!InitFloating.CanDrawOverlays(Activity))
                        DisplayChatHeadDialog();
                }

                sqlEntity.Get_MyProfile();

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(Activity) });

                LoadConfigSettings();

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(Activity) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CheckAndGetLocation()
        {
            try
            {
                var locationManager = (LocationManager)Activity.GetSystemService(Context.LocationService);
                if (!locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                {

                }
                else
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 50;
                    var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                    Console.WriteLine("Position Status: {0}", position.Timestamp);
                    Console.WriteLine("Position Latitude: {0}", position.Latitude);
                    Console.WriteLine("Position Longitude: {0}", position.Longitude);

                    UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                    UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);

                    if (Methods.CheckConnectivity())
                    {
                        var dictionaryProfile = new Dictionary<string, string>
                        {
                            {"lat", UserDetails.Lat},
                            {"lng", UserDetails.Lng},
                        };

                        var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.Lat = UserDetails.Lat;
                            dataUser.Lat = UserDetails.Lat;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dictionaryProfile) });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void GetOneSignalNotification()
        {
            try
            {
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                {
                    string userId = Activity.Intent?.GetStringExtra("UserID") ?? "Don't have type";
                    if (!string.IsNullOrEmpty(userId) && userId != "Don't have type")
                    {
                        var dataUser = LastChatTab?.MAdapter?.ChatList?.FirstOrDefault(a => a.UserId == userId && a.ChatType == "user");

                        Intent intent = new Intent(Activity, typeof(ChatWindowActivity));
                        intent.PutExtra("UserID", userId);

                        if (dataUser != null)
                        {
                            intent.PutExtra("TypeChat", "LastMessenger");
                            intent.PutExtra("ColorChat", dataUser.LastMessage.LastMessageClass.ChatColor);
                            intent.PutExtra("UserItem", JsonConvert.SerializeObject(dataUser));
                        }
                        else
                        {
                            intent.PutExtra("TypeChat", "OneSignalNotification");
                            intent.PutExtra("ColorChat", AppSettings.MainColor);
                        }

                        StartActivity(intent);
                    }
                }
                else
                {
                    string userId = Activity.Intent?.GetStringExtra("UserID") ?? "Don't have type";
                    if (!string.IsNullOrEmpty(userId) && userId != "Don't have type")
                    {
                        var dataUser = LastMessagesTab?.MAdapter?.MLastMessagesUser?.FirstOrDefault(a => a.UserId == userId);

                        Intent intent = new Intent(Activity, typeof(ChatWindowActivity));
                        intent.PutExtra("UserID", userId);

                        if (dataUser != null)
                        {
                            intent.PutExtra("TypeChat", "LastMessenger");
                            intent.PutExtra("ColorChat", dataUser.ChatColor);
                            intent.PutExtra("UserItem", JsonConvert.SerializeObject(dataUser));
                        }
                        else
                        {
                            intent.PutExtra("TypeChat", "OneSignalNotification");
                            intent.PutExtra("ColorChat", AppSettings.MainColor);
                        }
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InAppUpdate()
        {
            try
            {
                if (AppSettings.ShowSettingsUpdateManagerApp)
                    UpdateManagerApp.CheckUpdateApp(Activity, 4711, Activity.Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
        private void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview.GetBoolean(MainSettings.PrefKeyInAppReview, false);
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.Content(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Rate)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(Activity.PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();

                        MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == GetText(Resource.String.Lbl_CreateNewGroup))
                {
                    StartActivity(new Intent(Activity, typeof(CreateGroupActivity)));
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_GroupRequest))
                {
                    StartActivity(new Intent(Activity, typeof(GroupRequestActivity)));
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Blocked_User_List))
                {
                    StartActivity(new Intent(Activity, typeof(BlockedUsersActivity)));
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Settings))
                {
                    StartActivity(new Intent(Activity, typeof(GeneralAccountActivity)));
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Clear_call_log))
                {
                    var dialog = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(GetText(Resource.String.Lbl_Warning));
                    dialog.Content(GetText(Resource.String.Lbl_Clear_call_log));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            LastCallsTab?.MAdapter?.MCallUser?.Clear();
                            LastCallsTab?.MAdapter?.NotifyDataSetChanged();
                            LastCallsTab?.ShowEmptyPage();
                             
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.Clear_CallUser_List();

                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
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

        #region Chat Head

        private Dialog ChatHeadWindow;
        private static bool OpenDialog;
        private void DisplayChatHeadDialog()
        {
            try
            {
                if (OpenDialog && InitFloating.CanDrawOverlays(Activity))
                    return;

                ChatHeadWindow = new Dialog(Activity, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                ChatHeadWindow.SetContentView(Resource.Layout.ChatHeadDialogLayout);

                var subTitle1 = ChatHeadWindow.FindViewById<TextView>(Resource.Id.subTitle1);
                var btnNotNow = ChatHeadWindow.FindViewById<TextView>(Resource.Id.notNowButton);
                var btnGoToSettings = ChatHeadWindow.FindViewById<Button>(Resource.Id.goToSettingsButton);

                subTitle1.Text = GetText(Resource.String.Lbl_EnableChatHead_SubTitle1) + " " + AppSettings.ApplicationName + ", " + GetText(Resource.String.Lbl_EnableChatHead_SubTitle2);

                btnNotNow.Click += BtnNotNowOnClick;
                btnGoToSettings.Click += BtnGoToSettingsOnClick;

                ChatHeadWindow.Show();

                OpenDialog = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnGoToSettingsOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Floating.CheckPermission())
                {
                    Floating.OpenManagePermission();
                }

                if (ChatHeadWindow != null)
                {
                    ChatHeadWindow.Hide();
                    ChatHeadWindow.Dispose();
                    ChatHeadWindow = null;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnNotNowOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ChatHeadWindow != null)
                {
                    ChatHeadWindow.Hide();
                    ChatHeadWindow.Dispose();
                    ChatHeadWindow = null;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}