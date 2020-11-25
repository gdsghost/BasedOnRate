using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Ads.DoubleClick;
using Android.OS;


using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using WoWonder.Activities.FriendRequest;
using WoWonder.Activities.General;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
 
namespace WoWonder.Activities.Tabbes.Fragment
{
    public class TrendingFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic

        private TabbedMainActivity GlobalContext;
        public RecyclerView ProUserRecyclerView;
        private RecyclerView PageRecyclerView , LastActivitiesRecyclerView;
        public ImageView FriendRequestImage1, FriendRequestImage2, FriendRequestImage3, FriendRequestCount; 
        public RelativeLayout LayoutFriendRequest;
        public LinearLayout LayoutSuggestionProUsers;
        public LinearLayout LayoutSuggestionPromotedPage;
        private LinearLayout LayoutSuggestionLastActivities;
        private LastActivitiesAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private NestedScrollView NestedScrolled;
        //private RecyclerViewOnScrollListener MainScrollEvent;
        private TextView TxTMoreLastActivities, IconMoreLastActivities , TxTFriendRequest , TxtAllFriendRequest;
        private PublisherAdView PublisherAdView;
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
                View view = inflater.Inflate(Resource.Layout.Tab_Trending_Layout, container, false);

                InitComponent(view);
                SetRecyclerViewAdapters();
                 
                if (!AppSettings.SetTabOnButton)
                {
                    var parasms = (LinearLayout.LayoutParams)NestedScrolled.LayoutParameters;

                    // Check if we're running on Android 5.0 or higher
                    parasms.TopMargin = (int)Build.VERSION.SdkInt < 23 ? 120 : 225;

                    NestedScrolled.LayoutParameters = parasms;
                }
                 
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ProUserRecyclerView = (RecyclerView)view.FindViewById(Resource.Id.proRecyler);
                PageRecyclerView = (RecyclerView)view.FindViewById(Resource.Id.pagerecyler);
                LastActivitiesRecyclerView = (RecyclerView)view.FindViewById(Resource.Id.lastactivitiesRecyler);

                NestedScrolled = (NestedScrollView)view.FindViewById(Resource.Id.nestedScrollView);

                FriendRequestImage1 = (ImageView)view.FindViewById(Resource.Id.image_page_1);
                FriendRequestImage2 = (ImageView)view.FindViewById(Resource.Id.image_page_2);
                FriendRequestImage3 = (ImageView)view.FindViewById(Resource.Id.image_page_3);
                FriendRequestCount = (ImageView)view.FindViewById(Resource.Id.count_view);

                TxTFriendRequest = (TextView)view.FindViewById(Resource.Id.tv_Friends_connection);
                TxtAllFriendRequest = (TextView)view.FindViewById(Resource.Id.tv_Friends);

                TxTMoreLastActivities = (TextView)view.FindViewById(Resource.Id.tv_lastactivities);
                IconMoreLastActivities = (TextView)view.FindViewById(Resource.Id.iv_more_lastactivities);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMoreLastActivities, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropleft : IonIconsFonts.IosArrowDropright);
                 
                LayoutSuggestionProUsers = (LinearLayout)view.FindViewById(Resource.Id.layout_suggestion_Friends);
                LayoutSuggestionLastActivities = (LinearLayout)view.FindViewById(Resource.Id.layout_suggestion_lastactivities);
                LayoutFriendRequest = (RelativeLayout)view.FindViewById(Resource.Id.layout_friend_Request);
                LayoutSuggestionPromotedPage = (LinearLayout)view.FindViewById(Resource.Id.layout_suggestion_PromotedPage);

                LastActivitiesRecyclerView.Visibility = ViewStates.Gone;
                LayoutSuggestionLastActivities.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowLastActivities)
                {
                    LastActivitiesRecyclerView.Visibility = ViewStates.Gone;
                    LayoutSuggestionLastActivities.Visibility = ViewStates.Gone;
                }

                if (AppSettings.ConnectivitySystem == 1)
                {
                    TxTFriendRequest.Text = Context.GetString(Resource.String.Lbl_FollowRequest);
                    TxtAllFriendRequest.Text = Context.GetString(Resource.String.Lbl_View_All_FollowRequest);
                }

                LayoutFriendRequest.Click += LayoutFriendRequestOnClick; 
                TxTMoreLastActivities.Click += MoreLastActivitiesOnClick;
                IconMoreLastActivities.Click += MoreLastActivitiesOnClick;

                PublisherAdView = view.FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view); 
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                ProUserRecyclerView.NestedScrollingEnabled = false;
                PageRecyclerView.NestedScrollingEnabled = false;

                GlobalContext.ProUsersAdapter = new ProUsersAdapter(Activity);
                GlobalContext.ProUsersAdapter.MProUsersList = new ObservableCollection<UserDataObject>();
                GlobalContext.ProUsersAdapter.ItemClick += ProUsersAdapterOnItemClick;
                ProUserRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                ProUserRecyclerView.SetAdapter(GlobalContext.ProUsersAdapter);
                ProUserRecyclerView.HasFixedSize = true;
                ProUserRecyclerView.SetItemViewCacheSize(10);
                ProUserRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;

                var isPro = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var dataOwner = GlobalContext.ProUsersAdapter.MProUsersList.FirstOrDefault(a => a.Type == "Your");
                    if (dataOwner == null)
                    {  
                        GlobalContext.ProUsersAdapter.MProUsersList.Insert(0, new UserDataObject
                        {
                            Avatar = UserDetails.Avatar,
                            Type = "Your",
                            Username = Context.GetText(Resource.String.Lbl_AddMe),
                        });

                        GlobalContext.ProUsersAdapter.NotifyDataSetChanged();
                    }
                }
                 
                //============================= Last Activities Users ================================== 
                MAdapter = new LastActivitiesAdapter(Activity)
                {
                    LastActivitiesList = new ObservableCollection<ActivityDataObject>(),
                };
                MAdapter.ItemClick += LastActivitiesAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                LastActivitiesRecyclerView.SetLayoutManager(LayoutManager);
                LastActivitiesRecyclerView.HasFixedSize = true;
                LastActivitiesRecyclerView.SetItemViewCacheSize(10);
                LastActivitiesRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProviderLastActivities = new FixedPreloadSizeProvider(10, 10);
                var preLoaderLastActivities = new RecyclerViewPreloader<ActivityDataObject>(Activity, MAdapter, sizeProviderLastActivities, 10);
                LastActivitiesRecyclerView.AddOnScrollListener(preLoaderLastActivities);
                LastActivitiesRecyclerView.SetAdapter(MAdapter);

                //RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                //MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                //MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                //LastActivitiesRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                //MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

      
        #endregion

        #region Events

        // Event Show all Friend Request 
        private void ProUsersAdapterOnItemClick(object sender, ProUsersAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = GlobalContext.ProUsersAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (item.Type == "Your")
                        {
                            var intent = new Intent(Activity, typeof(GoProActivity));
                            Activity.StartActivity(intent); 
                        }
                        else
                        {
                            WoWonderTools.OpenProfile(Activity, item.UserId, item);
                        } 
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //See all Last Activities 
        private void MoreLastActivitiesOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(LastActivitiesActivity));
                Activity.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }



        //Event Click Item last Activities and Open ViewFullPostActivity Or Profile   
        private void LastActivitiesAdapterOnItemClick(object sender, LastActivitiesAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    { 
                        if (item.ActivityType == "following" || item.ActivityType == "friend")
                        {
                            WoWonderTools.OpenProfile(Activity, item.UserId, item.Activator);
                        }
                        else
                        {
                            var intent = new Intent(Activity, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", item.PostId);
                            //intent.PutExtra("DataItem", JsonConvert.SerializeObject(item.PostData));
                            Activity.StartActivity(intent);
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LayoutFriendRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                if (TabbedMainActivity.GetInstance()?.FriendRequestsList.Count > 0)
                {
                    var intent = new Intent(Context, typeof(FriendRequestActivity));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        //private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Code get last id where LoadMore >>
        //        var item = MAdapter.LastActivitiesList.LastOrDefault();
        //        if (item != null && !string.IsNullOrEmpty(item.Id))
        //            StartApiService(item.Id);
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        #endregion

        #region Load Activities 

        public void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadActivitiesAsync(offset) });
        }

        private async Task LoadActivitiesAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.LastActivitiesList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Global.Get_Activities("6", offset);
                if (apiStatus == 200)
                {
                    if (respond is LastActivitiesObject result)
                    {
                        var respondList = result.Activities.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Activities let check = MAdapter.LastActivitiesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.LastActivitiesList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.LastActivitiesList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.LastActivitiesList = new ObservableCollection<ActivityDataObject>(result.Activities);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.LastActivitiesList.Count > 10 && !LastActivitiesRecyclerView.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreActivities), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                //MainScrollEvent.IsLoading = false;
                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                LastActivitiesRecyclerView.Visibility = ViewStates.Gone;
                LayoutSuggestionLastActivities.Visibility = ViewStates.Gone;

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.LastActivitiesList.Count > 0)
                {
                    LastActivitiesRecyclerView.Visibility = ViewStates.Visible;
                    LayoutSuggestionLastActivities.Visibility = ViewStates.Visible;
                }
                else
                {
                    LastActivitiesRecyclerView.Visibility = ViewStates.Gone;
                    LayoutSuggestionLastActivities.Visibility = ViewStates.Gone;
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