using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Java.Lang;
using Newtonsoft.Json;
using Refractored.Controls;
using WoWonder.Activities.Chat.Adapters;
using WoWonder.Activities.Chat.Call.Agora;
using WoWonder.Activities.Chat.Call.Twilio;
using WoWonder.Activities.Chat.ChatWindow;
using WoWonder.Activities.Chat.MsgTabbes;
using WoWonder.Activities.Chat.OldTab.Adapter;
using WoWonder.Activities.FriendRequest;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;
using Timer = System.Timers.Timer;

namespace WoWonder.Activities.Chat.OldTab
{
    public class LastMessagesFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback
    {
        #region Variables Basic

        public LastMessagesAdapter MAdapter;
        private MsgTabbedMainActivity GlobalContext;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        private bool OnlineUsers = true;
        public bool FirstRun = true;
        private string UserId = "";

        public Timer TimerCallingTimePassed;
        public int SecondPassed;
        public RecyclerViewOnScrollListener MainScrollEvent;

        public RelativeLayout LayoutFriendRequest;
        private CircleImageView FriendRequestImage1, FriendRequestImage2, FriendRequestImage3;
        private TextView TxTFriendRequest, TxtAllFriendRequest;
        private AdView BannerAd;

        private ItemTouchHelper MItemTouchHelper;
        private GetUsersListObject.User DataUserChat;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = MsgTabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.LastMessagesLayout, container, false); 
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        { 
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                OnlineUsers = MainSettings.SharedData?.GetBoolean("notifications_key", true) ?? true;

                InitComponent(view);
                SetRecyclerViewAdapters();

                Get_LastChat();

                // Run timer 
                TimerCallingTimePassed = new Timer { Interval = 1000 };
                TimerCallingTimePassed.Elapsed += TimerCallingTimePassed_Elapsed;
                TimerCallingTimePassed.Enabled = true;

                GlobalContext?.GetOneSignalNotification(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }


        private void TimerCallingTimePassed_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                SecondPassed++;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        public override void OnDestroy()
        {
            try
            {
                BannerAd?.Destroy();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                LayoutFriendRequest = (RelativeLayout)view.FindViewById(Resource.Id.layout_friend_Request);

                FriendRequestImage1 = view.FindViewById<CircleImageView>(Resource.Id.image_page_1);
                FriendRequestImage2 = view.FindViewById<CircleImageView>(Resource.Id.image_page_2);
                FriendRequestImage3 = view.FindViewById<CircleImageView>(Resource.Id.image_page_3);

                TxTFriendRequest = (TextView)view.FindViewById(Resource.Id.tv_Friends_connection);
                TxtAllFriendRequest = (TextView)view.FindViewById(Resource.Id.tv_Friends);

                if (AppSettings.ConnectivitySystem == 1)
                {
                    TxTFriendRequest.Text = Context.GetString(Resource.String.Lbl_FollowRequest);
                    TxtAllFriendRequest.Text = Context.GetString(Resource.String.Lbl_View_All_FollowRequest);
                }

                LayoutFriendRequest.Click += LayoutFriendRequestOnClick;
                LayoutFriendRequest.Visibility = ViewStates.Gone;

                LinearLayout adContainer = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                BannerAd = AdsFacebook.InitAdView(Activity, adContainer);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new LastMessagesAdapter(Activity, OnlineUsers) { MLastMessagesUser = new ObservableCollection<GetUsersListObject.User>(ListUtils.UserChatList) };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                MAdapter.CallItemClick += MAdapterOnCallItemClick;
                MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                MAdapter.MoreItemClick += MAdapterOnMoreItemClick;

                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager); 
                //MRecycler.HasFixedSize = true;
                //MRecycler.SetItemViewCacheSize(10);
                //MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                 
                MRecycler.SetAdapter(MAdapter);

                var callback = new SwipeItemTouchHelper(MAdapter);
                MItemTouchHelper = new ItemTouchHelper(callback);
                MItemTouchHelper.AttachToRecyclerView(MRecycler);
                callback.SetBgColorCode(Color.ParseColor(AppSettings.MainColor));

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<GetUsersListObject.User>(Activity, MAdapter, sizeProvider, 8);
                MRecycler.AddOnScrollListener(preLoader);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
                IsChatMessageLoaded = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

      
        #endregion

        #region Event
          
        //More Swipe Item
        private void MAdapterOnMoreItemClick(object sender, Holders.LastMessagesClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        DataUserChat = item;
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                        arrayAdapter.Add(Context.GetText(Resource.String.Lbl_View_Profile));
                        arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Block));

                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Delete Swipe Item
        private void MAdapterOnDeleteItemClick(object sender, Holders.LastMessagesClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    { 
                        var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_DeleteTheEntireConversation));
                        dialog.Content(GetText(Resource.String.Lbl_OnceYouDeleteConversation));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                if (!Methods.CheckConnectivity())
                                {
                                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                                    return;
                                }
                                 
                                var userToDelete = MAdapter.MLastMessagesUser.FirstOrDefault(a => a.UserId == item.UserId);
                                if (userToDelete != null)
                                {
                                    MAdapter.ItemsSwiped.Swiped = false;
                                    MAdapter.ItemsSwiped = null;

                                    var index = MAdapter.MLastMessagesUser.IndexOf(userToDelete);
                                    if (index > -1)
                                    {
                                        MAdapter.MLastMessagesUser.Remove(userToDelete);
                                        MAdapter.NotifyItemRemoved(index);
                                    }
                                }

                                var dbDatabase = new SqLiteDatabase();
                                dbDatabase.Delete_LastUsersChat(item.UserId , "user");
                                dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, item.UserId);
                                

                                Methods.Path.DeleteAll_FolderUser(item.UserId);

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Delete_Conversation(item.UserId) });

                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_TheConversationHasBeenDeleted), ToastLength.Long)?.Show(); 
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Call Swipe Item
        private void MAdapterOnCallItemClick(object sender, Holders.LastMessagesClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        DataUserChat = item;

                        string timeNow = DateTime.Now.ToString("hh:mm");
                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string time = Convert.ToString(unixTimestamp);

                        if (AppSettings.EnableAudioCall && AppSettings.EnableVideoCall)
                        {
                            var arrayAdapter = new List<string>();
                            var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                            arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Voice_call));
                            arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Video_call));

                            dialogList.Title(GetText(Resource.String.Lbl_Call));
                            //dialogList.Content(GetText(Resource.String.Lbl_Select_Type_Call));
                            dialogList.Items(arrayAdapter);
                            dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show(); 
                        }
                        else if (AppSettings.EnableAudioCall == false && AppSettings.EnableVideoCall) // Video Call On
                        {
                            try
                            {
                                Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                                if (AppSettings.UseLibrary == SystemCall.Agora)
                                {
                                    intentVideoCall = new Intent(Context, typeof(AgoraVideoCallActivity));
                                    intentVideoCall.PutExtra("type", "Agora_video_calling_start");
                                }
                                else if (AppSettings.UseLibrary == SystemCall.Twilio)
                                {
                                    intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                                    intentVideoCall.PutExtra("type", "Twilio_video_calling_start");
                                }

                                intentVideoCall.PutExtra("UserID", item.UserId);
                                intentVideoCall.PutExtra("avatar", item.Avatar);
                                intentVideoCall.PutExtra("name", item.Name);
                                intentVideoCall.PutExtra("time", timeNow);
                                intentVideoCall.PutExtra("CallID", time);
                                intentVideoCall.PutExtra("access_token", "YOUR_TOKEN");
                                intentVideoCall.PutExtra("access_token_2", "YOUR_TOKEN");
                                intentVideoCall.PutExtra("from_id", "0");
                                intentVideoCall.PutExtra("active", "0");
                                intentVideoCall.PutExtra("status", "0");
                                intentVideoCall.PutExtra("room_name", "TestRoom");
                                StartActivity(intentVideoCall);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }
                        else if (AppSettings.EnableAudioCall && AppSettings.EnableVideoCall == false) // Audio Call On
                        {
                            try
                            {
                                Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                                if (AppSettings.UseLibrary == SystemCall.Agora)
                                {
                                    intentVideoCall = new Intent(Context, typeof(AgoraAudioCallActivity));
                                    intentVideoCall.PutExtra("type", "Agora_audio_calling_start");
                                }
                                else if (AppSettings.UseLibrary == SystemCall.Twilio)
                                {
                                    intentVideoCall = new Intent(Context, typeof(TwilioAudioCallActivity));
                                    intentVideoCall.PutExtra("type", "Twilio_audio_calling_start");
                                }

                                intentVideoCall.PutExtra("UserID", item.UserId);
                                intentVideoCall.PutExtra("avatar", item.Avatar);
                                intentVideoCall.PutExtra("name", item.Name);
                                intentVideoCall.PutExtra("time", timeNow);
                                intentVideoCall.PutExtra("CallID", time);
                                intentVideoCall.PutExtra("access_token", "YOUR_TOKEN");
                                intentVideoCall.PutExtra("access_token_2", "YOUR_TOKEN");
                                intentVideoCall.PutExtra("from_id", "0");
                                intentVideoCall.PutExtra("active", "0");
                                intentVideoCall.PutExtra("status", "0");
                                intentVideoCall.PutExtra("room_name", "TestRoom");
                                StartActivity(intentVideoCall);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.MLastMessagesUser.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.ChatTime))
                   PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadLastMessagesApi(item.ChatTime) },3); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MainScrollEvent.IsLoading = false;

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();

                    if (SwipeRefreshLayout.Refreshing)
                        SwipeRefreshLayout.Refreshing = false;
                }
                else
                {
                    MAdapter.MLastMessagesUser.Clear();
                    MAdapter.NotifyDataSetChanged();
                    ListUtils.UserList.Clear();

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.ClearAll_LastUsersChat();
                    dbDatabase.ClearAll_Messages();
                    

                    UserDetails.OffsetLastChat = "0";

                    if (MAdapter.MLastMessagesUser.Count == 0)
                    {
                       // GlobalContext?.SetService();
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadDataApiLastChat }, 3);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterOnItemClick(object sender, Holders.LastMessagesClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        UserId = item.UserId;
                        Activity.RunOnUiThread(() =>
                        {
                            if (item.LastMessage.ToId == UserDetails.UserId && item.LastMessage.FromId != UserDetails.UserId)
                            {
                                item.LastMessage.Seen = "1";
                                MAdapter.NotifyItemChanged(position);
                            }  
                        });
                         
                        if (item.ChatColor == null)
                            item.ChatColor = AppSettings.MainColor;

                        var mainChatColor = item.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(item.ChatColor) : item.ChatColor ?? AppSettings.MainColor;

                        Intent intent = new Intent(Context, typeof(ChatWindowActivity));
                        intent.PutExtra("UserID", item.UserId);
                        intent.PutExtra("TypeChat", "LastMessenger");
                        intent.PutExtra("ShowEmpty", "no");
                        intent.PutExtra("ColorChat", mainChatColor);
                        intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        { 
                            StartActivity(intent);
                        }
                        else
                        {
                            //Check to see if any permission in our group is available, if one, then all are
                            if (Context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && Context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            { 
                                StartActivity(intent);
                            }
                            else
                            {
                                RequestPermissions(new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage
                                }, 101);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterOnItemLongClick(object sender, Holders.LastMessagesClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        // handle when double swipe
                        if (item.Swiped)
                        {
                            MAdapter.ItemsSwiped = null;
                            item.Swiped = false;
                            MAdapter.NotifyItemChanged(position);
                            return;
                        }

                        foreach (var user in MAdapter.MLastMessagesUser)
                        {
                            int indexSwiped = MAdapter.MLastMessagesUser.IndexOf(user);
                            if (indexSwiped <= -1) continue;
                            user.Swiped = false;
                            MAdapter.NotifyItemChanged(indexSwiped);
                        }

                        item.Swiped = true;
                        MAdapter.ItemsSwiped = item;
                        MAdapter.NotifyItemChanged(position);
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private void LayoutFriendRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ListUtils.FriendRequestsList.Count > 0)
                {
                    var intent = new Intent(Context, typeof(FriendRequestActivity));
                    Context.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Permissions

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        var data = MAdapter.MLastMessagesUser.FirstOrDefault(a => a.UserId == UserId);
                        if (data != null)
                        {
                            Intent intent = new Intent(Context, typeof(ChatWindowActivity));
                            intent.PutExtra("UserID", data.UserId);
                            intent.PutExtra("TypeChat", "LastMessenger");
                            intent.PutExtra("UserItem", JsonConvert.SerializeObject(data));
                            StartActivity(intent);
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region LoadData
         
        private void Get_LastChat()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                }
                else
                {
                    Activity.RequestPermissions(new[]
                    {
                        Manifest.Permission.ReadExternalStorage,
                        Manifest.Permission.WriteExternalStorage,
                        Manifest.Permission.Camera,
                        Manifest.Permission.RecordAudio,
                        Manifest.Permission.ModifyAudioSettings,
                    }, 123);
                }
                 
                if (MAdapter.MLastMessagesUser?.Count == 0)
                {
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                }
                 
                if (ListUtils.UserList.Count > 0)
                {
                    MAdapter.MLastMessagesUser = new ObservableCollection<GetUsersListObject.User>(ListUtils.UserChatList);
                    MAdapter.NotifyDataSetChanged();
                    Activity.RunOnUiThread(GlobalContext.ShowEmptyPage);
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadDataApiLastChat, LoadGeneralData }); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                SwipeRefreshLayout.Refreshing = false;
                //SwipeRefreshLayout.Enabled = false;
            }
        }

        //Get General Data Using Api >> Friend Requests
        private async Task LoadGeneralData()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (int apiStatus, var respond) = await RequestsAsync.Global.Get_General_Data(true, OnlineUsers, UserDetails.DeviceId, "0", "friend_requests,group_chat_requests,count_new_messages").ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                try
                                {
                                    // Friend Requests
                                    if (result.FriendRequests.Count > 0)
                                    {
                                        ListUtils.FriendRequestsList = new ObservableCollection<UserDataObject>(result.FriendRequests);

                                        LayoutFriendRequest.Visibility = ViewStates.Visible;
                                        try
                                        {
                                            for (var i = 0; i < 4; i++)
                                                switch (i)
                                                {
                                                    case 0:
                                                        GlideImageLoader.LoadImage(Activity, ListUtils.FriendRequestsList[i].Avatar, FriendRequestImage3, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                        break;
                                                    case 1:
                                                        GlideImageLoader.LoadImage(Activity, ListUtils.FriendRequestsList[i].Avatar, FriendRequestImage2, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                        break;
                                                    case 2:
                                                        GlideImageLoader.LoadImage(Activity, ListUtils.FriendRequestsList[i].Avatar, FriendRequestImage1, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                        break;
                                                }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    }
                                    else
                                    {
                                        LayoutFriendRequest.Visibility = ViewStates.Gone;
                                    }
                                     
                                    // Group Requests
                                    if (result.GroupChatRequests.Count > 0)
                                    {
                                        ListUtils.GroupRequestsList = new ObservableCollection<GroupChatRequest>(result.GroupChatRequests);

                                        if (GlobalContext.LastGroupChatsTab != null)
                                        {
                                            GlobalContext.LastGroupChatsTab.LayoutGroupRequest.Visibility = ViewStates.Visible;
                                            try
                                            {
                                                for (var i = 0; i < 4; i++)
                                                {
                                                    var item = result.GroupChatRequests[i];
                                                    var image = item.GroupTab.Avatar.Replace(Client.WebsiteUrl, "");
                                                    if (!image.Contains("http"))
                                                        item.GroupTab.Avatar = Client.WebsiteUrl + "/" + image;

                                                    switch (i)
                                                    {
                                                        case 0:
                                                            GlideImageLoader.LoadImage(Activity, item.GroupTab.Avatar, GlobalContext.LastGroupChatsTab.GroupRequestImage3, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                            break;
                                                        case 1:
                                                            GlideImageLoader.LoadImage(Activity, item.GroupTab.Avatar, GlobalContext.LastGroupChatsTab.GroupRequestImage2, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                            break;
                                                        case 2:
                                                            GlideImageLoader.LoadImage(Activity, item.GroupTab.Avatar, GlobalContext.LastGroupChatsTab.GroupRequestImage1, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                            break;
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (GlobalContext.LastGroupChatsTab != null)
                                            GlobalContext.LastGroupChatsTab.LayoutGroupRequest.Visibility = ViewStates.Gone;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool IsChatMessageLoaded;

        private async Task LoadLastMessagesApi(string x)
        {
            if (IsChatMessageLoaded)
                return;

            UserDetails.OffsetLastChat = MAdapter.MLastMessagesUser.Count > 0 ? MAdapter.MLastMessagesUser.LastOrDefault()?.ChatTime ?? "0" : "0";

            var (apiStatus, respond) = await RequestsAsync.Message.Get_users_list_Async(UserDetails.UserId, UserDetails.UserId, "35", x);
            if (apiStatus == 200 && respond is GetUsersListObject result)
            {
                if (result.Users.Count > 0)
                {
                    GlobalContext.LoadUserMessagesDuringScroll(result);
                    IsChatMessageLoaded = false;
                }
                else
                    IsChatMessageLoaded = true;
            }
            else
                Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(GlobalContext.ShowEmptyPage);
        }

        private async Task LoadDataApiLastChat()
        {
            var (apiStatus, respond) = await RequestsAsync.Message.Get_users_list_Async(UserDetails.UserId, UserDetails.UserId);
            if (apiStatus == 200 && respond is GetUsersListObject result)
            {
                if (result.Users.Count > 0)
                    GlobalContext.LoadDataJsonLastChat(result);
                else
                    IsChatMessageLoaded = true;
            }
            else
                Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(GlobalContext.ShowEmptyPage);
        }

        #endregion

        #region MaterialDialog
         
        public async void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Context.GetText(Resource.String.Lbl_View_Profile))
                { 
                    WoWonderTools.OpenProfile(Activity, DataUserChat.UserId, DataUserChat);
                }
                else if (itemString.ToString() == Context.GetText(Resource.String.Lbl_Block))
                {
                    if (Methods.CheckConnectivity())
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Global.Block_User(DataUserChat.UserId, true); //true >> "block" 
                        if (apiStatus == 200)
                        {
                            Methods.DisplayReportResultTrack(respond);

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(DataUserChat, "Delete"); 
                            dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, DataUserChat.UserId);
                            

                            Methods.Path.DeleteAll_FolderUser(DataUserChat.UserId);


                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Blocked_successfully),ToastLength.Short)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                    }
                }
                else if (itemString.ToString() == Context.GetText(Resource.String.Lbl_Voice_call))
                {
                    string timeNow = DateTime.Now.ToString("hh:mm");
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time = Convert.ToString(unixTimestamp);

                    Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                    if (AppSettings.UseLibrary == SystemCall.Agora)
                    {
                        intentVideoCall = new Intent(Context, typeof(AgoraAudioCallActivity));
                        intentVideoCall.PutExtra("type", "Agora_audio_calling_start");
                    }
                    else if (AppSettings.UseLibrary == SystemCall.Twilio)
                    {
                        intentVideoCall = new Intent(Context, typeof(TwilioAudioCallActivity));
                        intentVideoCall.PutExtra("type", "Twilio_audio_calling_start");
                    }

                    intentVideoCall.PutExtra("UserID", DataUserChat.UserId);
                    intentVideoCall.PutExtra("avatar", DataUserChat.Avatar);
                    intentVideoCall.PutExtra("name", DataUserChat.Name);
                    intentVideoCall.PutExtra("time", timeNow);
                    intentVideoCall.PutExtra("CallID", time);
                    StartActivity(intentVideoCall);
                }
                else if (itemString.ToString() == Context.GetText(Resource.String.Lbl_Video_call))
                {
                    string timeNow = DateTime.Now.ToString("hh:mm");
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time = Convert.ToString(unixTimestamp);

                    Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                    if (AppSettings.UseLibrary == SystemCall.Agora)
                    {
                        intentVideoCall = new Intent(Context, typeof(AgoraVideoCallActivity));
                        intentVideoCall.PutExtra("type", "Agora_video_calling_start");
                    }
                    else if (AppSettings.UseLibrary == SystemCall.Twilio)
                    {
                        intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                        intentVideoCall.PutExtra("type", "Twilio_video_calling_start");
                    }

                    intentVideoCall.PutExtra("UserID", DataUserChat.UserId);
                    intentVideoCall.PutExtra("avatar", DataUserChat.Avatar);
                    intentVideoCall.PutExtra("name", DataUserChat.Name);
                    intentVideoCall.PutExtra("time", timeNow);
                    intentVideoCall.PutExtra("CallID", time);
                    intentVideoCall.PutExtra("access_token", "YOUR_TOKEN");
                    intentVideoCall.PutExtra("access_token_2", "YOUR_TOKEN");
                    intentVideoCall.PutExtra("from_id", "0");
                    intentVideoCall.PutExtra("active", "0");
                    intentVideoCall.PutExtra("status", "0");
                    intentVideoCall.PutExtra("room_name", "TestRoom");
                    StartActivity(intentVideoCall);
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

    }
}