using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.App;
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
using WoWonder.Activities.Chat.GroupChat;
using WoWonder.Activities.Chat.MsgTabbes.Adapter;
using WoWonder.Activities.Chat.PageChat;
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
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Requests;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace WoWonder.Activities.Chat.MsgTabbes.Fragment
{
    public class LastChatFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback
    {
        #region Variables Basic

        public LastChatsAdapter MAdapter;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        public RelativeLayout LayoutFriendRequest;
        private CircleImageView FriendRequestImage1, FriendRequestImage2, FriendRequestImage3;
        private TextView TxTFriendRequest, TxtAllFriendRequest;
        public bool OnlineUsers = true;
        public bool Run = true;
        public Timer TimerCallingTimePassed;
        public int SecondPassed;
        private MsgTabbedMainActivity GlobalContext;
        //private static string CountMessagesStatic;

        private ItemTouchHelper MItemTouchHelper;
        private ChatObject DataUserChat;

        private AdView BannerAd;
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

                OnlineUsers = MainSettings.SharedData?.GetBoolean("onlineuser_key", true) ?? true;

                InitComponent(view);
                SetRecyclerViewAdapters();

                StartApiService();

                // Run timer 
                TimerCallingTimePassed = new Timer { Interval = 1000 };
                TimerCallingTimePassed.Elapsed += TimerCallingTimePassedOnElapsed;
                TimerCallingTimePassed.Enabled = true;

                GlobalContext?.GetOneSignalNotification();

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadGeneralData });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);  
            }
        }

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
                MAdapter = new LastChatsAdapter(Activity, OnlineUsers) { ChatList = new ObservableCollection<ChatObject>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                MAdapter.CallItemClick += MAdapterOnCallItemClick;
                MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                MAdapter.MoreItemClick += MAdapterOnMoreItemClick;
                
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                var callback = new SwipeItemTouchHelper(MAdapter);
                MItemTouchHelper = new ItemTouchHelper(callback);
                MItemTouchHelper.AttachToRecyclerView(MRecycler);
                callback.SetBgColorCode(Color.ParseColor(AppSettings.MainColor));

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<ChatObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                if (ListUtils.UserList.Count > 0)
                {
                    MAdapter.ChatList = ListUtils.UserList;
                    MAdapter.NotifyDataSetChanged();
                    Activity.RunOnUiThread(GlobalContext.ShowEmptyPage);
                }

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events 

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.ChatList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.ChatTime))
                    StartApiService("all","",item.ChatTime);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
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
                    MAdapter.ChatList.Clear();
                    MAdapter.NotifyDataSetChanged();
                    ListUtils.UserList.Clear();

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.ClearAll_LastUsersChat();
                    dbDatabase.ClearAll_Messages();
                    

                    if (MAdapter.ChatList.Count == 0)
                    {
                        StartApiService();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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

                        switch (item.ChatType)
                        {
                            case "user":
                                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_View_Profile));
                                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Block));
                                break;
                            case "page":
                                
                                break;
                            case "group": 
                                if (item.Owner != null && item.Owner.Value)
                                    arrayAdapter.Add(GetText(Resource.String.Lbl_GroupInfo));
                                arrayAdapter.Add(GetText(Resource.String.Lbl_ExitGroup));
                                break;
                        }

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

                                switch (item.ChatType)
                                {
                                    case "user":
                                    {
                                        var userToDelete = MAdapter.ChatList.FirstOrDefault(a => a.UserId == item.UserId);
                                        if (userToDelete != null)
                                        {
                                            MAdapter.ItemsSwiped.Swiped = false;
                                            MAdapter.ItemsSwiped = null;

                                            var index = MAdapter.ChatList.IndexOf(userToDelete);
                                            if (index > -1)
                                            {
                                                MAdapter.ChatList.Remove(userToDelete);
                                                MAdapter.NotifyItemRemoved(index);
                                            }
                                        }

                                        var dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Delete_LastUsersChat(item.UserId, "user");
                                        dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, item.UserId);
                                        

                                        Methods.Path.DeleteAll_FolderUser(item.UserId);
                                             
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Delete_Conversation(item.UserId) });

                                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_TheConversationHasBeenDeleted), ToastLength.Long)?.Show();
                                        break;
                                    }
                                    case "page":
                                    {
                                        string userId = "";
                                        //remove item to my Group list  
                                        if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                                        {
                                            var checkPage = MAdapter?.ChatList.FirstOrDefault(a => a.PageId == item.PageId);
                                            if (checkPage != null)
                                            {
                                                var userAdminPage = item.UserId;
                                                if (userAdminPage == item.LastMessage.LastMessageClass.ToData.UserId)
                                                {
                                                    userId = item.LastMessage.LastMessageClass.UserData.UserId;
                                                    var data = MAdapter?.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass.UserData.UserId == userId);
                                                    if (data != null)
                                                    {
                                                        MAdapter.ChatList.Remove(data);
                                                        MAdapter.NotifyItemRemoved(MAdapter.ChatList.IndexOf(data));
                                                    }
                                                }
                                                else
                                                {
                                                    userId = item.LastMessage.LastMessageClass.ToData.UserId;
                                                    var data = MAdapter?.ChatList.FirstOrDefault(a => a.LastMessage.LastMessageClass.ToData.UserId == userId);
                                                    if (data != null)
                                                    {
                                                        MAdapter.ChatList.Remove(data);
                                                        MAdapter.NotifyItemRemoved(MAdapter.ChatList.IndexOf(data));
                                                    }
                                                }
                                            } 
                                        }
                                        else
                                        {
                                            var adapter = GlobalContext?.LastPageChatsTab.MAdapter;
                                            var data = adapter?.LastPageList?.FirstOrDefault(a => a.PageId == item.PageId);
                                            if (data != null)
                                            {
                                                if (data.LastMessage != null)
                                                    userId = data.IsPageOnwer != null && data.IsPageOnwer.Value ? data.LastMessage.FromId == UserDetails.UserId ? data.LastMessage.ToId : data.LastMessage.FromId : UserDetails.UserId ?? UserDetails.UserId;
                                                else
                                                    userId = data.IsPageOnwer != null && data.IsPageOnwer.Value ? data.UserId : UserDetails.UserId;
                                                 
                                                adapter.LastPageList.Remove(data);
                                                adapter.NotifyItemRemoved(adapter.LastPageList.IndexOf(data));
                                            } 
                                        }
                                         
                                        var dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Delete_LastUsersChat(item.PageId, "page", userId);
                                        

                                        Methods.Path.DeleteAll_FolderUser(item.PageId);

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.PageChat.DeletePageChat(item.PageId, userId) });
                                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_TheConversationHasBeenDeleted),ToastLength.Short)?.Show();

                                        break;
                                    } 
                                    case "group":
                                    {
                                        //remove item to my Group list  
                                        if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                                        {
                                            var data = MAdapter?.ChatList?.FirstOrDefault(a => a.GroupId == DataUserChat.GroupId);
                                            if (data != null)
                                            {
                                                MAdapter.ChatList.Remove(data);
                                                MAdapter.NotifyItemRemoved(MAdapter.ChatList.IndexOf(data));
                                            }
                                        }
                                        else
                                        {
                                            var adapter = GlobalContext?.LastGroupChatsTab.MAdapter;
                                            var data = adapter?.LastGroupList?.FirstOrDefault(a => a.GroupId == DataUserChat.GroupId);
                                            if (data != null)
                                            {
                                                adapter.LastGroupList.Remove(data);
                                                adapter.NotifyItemRemoved(adapter.LastGroupList.IndexOf(data));
                                            }
                                        }

                                        var dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Delete_LastUsersChat(item.GroupId, "group");
                                        dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, item.GroupId);
                                        

                                        Methods.Path.DeleteAll_FolderUser(item.GroupId);

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.ExitGroupChat(item.GroupId) });

                                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_GroupSuccessfullyLeaved),ToastLength.Short)?.Show();
                                        break;
                                    }
                                }
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
                        string timeNow = DateTime.Now.ToString("hh:mm");
                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string time = Convert.ToString(unixTimestamp);

                        if (AppSettings.EnableAudioCall && AppSettings.EnableVideoCall)
                        {
                            AlertDialog.Builder builder = new AlertDialog.Builder(Activity, Resource.Style.AlertDialogCustom);

                            builder.SetTitle(GetText(Resource.String.Lbl_Call));
                            builder.SetMessage(GetText(Resource.String.Lbl_Select_Type_Call));

                            builder.SetPositiveButton(GetText(Resource.String.Lbl_Voice_call), (o, args) =>
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
                                    StartActivity(intentVideoCall);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });

                            builder.SetNegativeButton(GetText(Resource.String.Lbl_Video_call), (o, args) =>
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
                            });

                            var alert = builder.Create();
                            alert.Show();
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
                        Activity.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (item.LastMessage.LastMessageClass != null && (item.LastMessage.LastMessageClass.ToId == UserDetails.UserId && item.LastMessage.LastMessageClass.FromId != UserDetails.UserId))
                                {
                                    item.LastMessage.LastMessageClass.Seen = "1";
                                    MAdapter.NotifyItemChanged(position);
                                }
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });

                        Intent intent = null;
                        switch (item.ChatType)
                        {
                            case "user":
                                item.LastMessage.LastMessageClass.ChatColor ??= AppSettings.MainColor;

                                var mainChatColor = item.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(item.LastMessage.LastMessageClass.ChatColor) : item.LastMessage.LastMessageClass.ChatColor ?? AppSettings.MainColor;

                                intent = new Intent(Context, typeof(ChatWindowActivity));
                                intent.PutExtra("UserID", item.UserId);
                                intent.PutExtra("TypeChat", "LastMessenger");
                                intent.PutExtra("ShowEmpty", "no");
                                intent.PutExtra("ColorChat", mainChatColor);
                                intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));
                                break;
                            case "page":
                                intent = new Intent(Context, typeof(PageChatWindowActivity));
                                intent.PutExtra("PageId", item.PageId);
                                intent.PutExtra("ShowEmpty", "no");
                                intent.PutExtra("TypeChat", "");
                                intent.PutExtra("PageObject", JsonConvert.SerializeObject(item));
                                break;
                            case "group":
                                intent = new Intent(Context, typeof(GroupChatWindowActivity));
                                intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item));
                                intent.PutExtra("ShowEmpty", "no");
                                intent.PutExtra("GroupId", item.GroupId);
                                break;
                        }
                        StartActivity(intent);
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

                        foreach (var user in MAdapter.ChatList)
                        {
                            int indexSwiped = MAdapter.ChatList.IndexOf(user);
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

        //Timer
        private void TimerCallingTimePassedOnElapsed(object sender, ElapsedEventArgs e)
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

        #endregion

        #region Load Chat

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType">all,users,pages,groups</param>
        /// <param name="userType">online,offline</param>
        /// <param name="offset"></param>
        public void StartApiService(string dataType = "all", string userType = "", string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadChatAsync(dataType, userType, offset) });
            }
            else
            { 
                Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }

            Activity.RunOnUiThread(GlobalContext.ShowEmptyPage);
        }

        private async Task LoadChatAsync(string dataType = "all", string userType = "", string offset = "0")
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetChatAsync(dataType, userType, offset, "20", offset, "20", offset, "20");
            if (apiStatus.Equals(200))
            {
                if (respond is LastChatObject result)
                {
                    if (!string.IsNullOrEmpty(offset) && offset != "0")
                        GlobalContext.LoadUserMessagesDuringScroll(result);
                    else
                        GlobalContext.LoadDataJsonLastChat(result);
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(GlobalContext.ShowEmptyPage);
        }
         
        #endregion

        //Get General Data Using Api >> Friend Requests and Group Chat Requests
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

                                        if (AppSettings.LastChatSystem == SystemApiGetLastChat.Old && GlobalContext.LastGroupChatsTab != null)
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

                                    //if (!string.IsNullOrEmpty(result.CountNewMessages) && result.CountNewMessages != "0" && result.CountNewMessages != CountMessagesStatic)
                                    //{
                                    //    CountMessagesStatic = result.CountNewMessages;
                                    //    Activity.RunOnUiThread(() =>
                                    //    {
                                    //        try
                                    //        {
                                    //            if (CountMessagesStatic != "0")
                                    //            {
                                    //                var tab = GlobalContext.Tabs.GetTabAt(0); //Lbl_Tab_Chats

                                    //                var textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                                    //                textView.Text = CountMessagesStatic;
                                    //                textView.Visibility = ViewStates.Visible;
                                    //            }
                                    //        }
                                    //        catch (Exception e)
                                    //        {
                                    //            Methods.DisplayReportResultTrack(e);
                                    //        }
                                    //    });
                                    //}
                                    //else
                                    //{
                                    //    Activity.RunOnUiThread(() =>
                                    //    {
                                    //        try
                                    //        {
                                    //            if (CountMessagesStatic == "0")
                                    //            {
                                    //                var tab = GlobalContext.Tabs.GetTabAt(0); //Lbl_Tab_Chats

                                    //                TextView textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                                    //                textView.Visibility = ViewStates.Gone;
                                    //            }
                                    //        }
                                    //        catch (Exception e)
                                    //        {
                                    //            Methods.DisplayReportResultTrack(e);
                                    //        }
                                    //    });
                                    //}
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


        #region MaterialDialog

        public async void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Context.GetText(Resource.String.Lbl_View_Profile))
                {
                    WoWonderTools.OpenProfile(Activity, DataUserChat.UserId, DataUserChat.UserData);
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
                            //dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(DataUserChat, "Delete"); 
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
                else if (itemString.ToString() == Context.GetText(Resource.String.Lbl_GroupInfo))
                {
                    Intent intent = new Intent(Activity, typeof(EditGroupChatActivity));
                    intent.PutExtra("GroupObject", JsonConvert.SerializeObject(DataUserChat.GroupId));
                    Activity.StartActivity(intent);
                }
                else if (itemString.ToString() == Context.GetText(Resource.String.Lbl_ExitGroup))
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                    }
                    else
                    {
                        var dialog = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                        dialog.Content(GetText(Resource.String.Lbl_AreYouSureExitGroup));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Exit)).OnPositive(async (materialDialog, action) =>
                        {
                            try
                            {
                                //Show a progress
                                AndHUD.Shared.Show(Activity, GetText(Resource.String.Lbl_Loading));

                                var (apiStatus, respond) = await RequestsAsync.GroupChat.ExitGroupChat(DataUserChat.GroupId);
                                if (apiStatus == 200)
                                {
                                    if (respond is AddOrRemoveUserToGroupObject result)
                                    {
                                        Console.WriteLine(result.MessageData);

                                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_GroupSuccessfullyLeaved),ToastLength.Short)?.Show();

                                        //remove item to my Group list  
                                        if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                                        { 
                                            var data = MAdapter?.ChatList?.FirstOrDefault(a => a.GroupId == DataUserChat.GroupId);
                                            if (data != null)
                                            {
                                                MAdapter.ChatList.Remove(data);
                                                MAdapter.NotifyItemRemoved(MAdapter.ChatList.IndexOf(data));
                                            }
                                        }
                                        else
                                        {
                                            var adapter = GlobalContext?.LastGroupChatsTab.MAdapter;
                                            var data = adapter?.LastGroupList?.FirstOrDefault(a => a.GroupId == DataUserChat.GroupId);
                                            if (data != null)
                                            {
                                                adapter.LastGroupList.Remove(data);
                                                adapter.NotifyItemRemoved(adapter.LastGroupList.IndexOf(data));
                                            }
                                        }

                                        AndHUD.Shared.ShowSuccess(Activity); 
                                    }
                                }
                                else Methods.DisplayReportResult(Activity, respond);

                                AndHUD.Shared.Dismiss();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
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