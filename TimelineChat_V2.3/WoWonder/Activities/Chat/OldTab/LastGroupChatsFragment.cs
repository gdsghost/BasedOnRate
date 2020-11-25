using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using Refractored.Controls;
using WoWonder.Activities.Chat.GroupChat;
using WoWonder.Activities.Chat.OldTab.Adapter;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Requests;
using Xamarin.Facebook.Ads;

namespace WoWonder.Activities.Chat.OldTab
{
    public class LastGroupChatsFragment : AndroidX.Fragment.App.Fragment 
    {
        #region Variables Basic

        public LastGroupChatsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        public RelativeLayout LayoutGroupRequest;
        public CircleImageView GroupRequestImage1, GroupRequestImage2, GroupRequestImage3;
        private TextView TitGroupRequest, DesGroupRequest;
        private AdView BannerAd;

        #endregion

        #region General

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

                InitComponent(view);
                SetRecyclerViewAdapters();

                StartApiService();
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
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                LayoutGroupRequest = (RelativeLayout)view.FindViewById(Resource.Id.layout_friend_Request);
                TitGroupRequest = (TextView) view.FindViewById(Resource.Id.tv_Friends_connection);
                DesGroupRequest = (TextView) view.FindViewById(Resource.Id.tv_Friends);

                TitGroupRequest.Text = GetText(Resource.String.Lbl_GroupRequest);
                DesGroupRequest.Text = GetText(Resource.String.Lbl_ViewAllInviteRequest);

                GroupRequestImage1 = view.FindViewById<CircleImageView>(Resource.Id.image_page_1);
                GroupRequestImage2 = view.FindViewById<CircleImageView>(Resource.Id.image_page_2);
                GroupRequestImage3 = view.FindViewById<CircleImageView>(Resource.Id.image_page_3);

                LayoutGroupRequest.Click += LayoutGroupRequestOnClick;
                LayoutGroupRequest.Visibility = ViewStates.Gone;

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
                MAdapter = new LastGroupChatsAdapter(Activity){LastGroupList =  new ObservableCollection<ChatObject>()};
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                 
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ChatObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

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
                var item = MAdapter.LastGroupList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.GroupId) && !MainScrollEvent.IsLoading)
                    StartApiService(item.GroupId);
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

                MAdapter.LastGroupList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterOnItemClick(object sender, LastGroupChatsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        Intent intent = new Intent(Context, typeof(GroupChatWindowActivity));
                        intent.PutExtra("GroupId", item.GroupId);
                        intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item));
                        intent.PutExtra("ShowEmpty", "no");
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void LayoutGroupRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ListUtils.GroupRequestsList.Count > 0)
                {
                    var intent = new Intent(Context, typeof(GroupRequestActivity));
                    Context.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Group Chat
         
        private void StartApiService(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroupChatAsync(offset) });
            }
            else
            {
                SwipeRefreshLayout.Refreshing = false;

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
        }

        private async Task LoadGroupChatAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            int countList = MAdapter.LastGroupList.Count;
            (int apiStatus, var respond) = await RequestsAsync.GroupChat.GetGroupChatList("20" , offset);
            if (apiStatus.Equals(200))
            {
                if (respond is GroupListObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = MAdapter?.LastGroupList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                            {
                                MAdapter?.LastGroupList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.LastGroupList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.LastGroupList =  new ObservableCollection<ChatObject>(result.Data);
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });  
                        }
                    }
                    else
                    {
                        if (MAdapter?.LastGroupList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreGroup),ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(ShowEmptyPage);
        }

        public void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.LastGroupList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}