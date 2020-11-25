using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using WoWonder.Activities.Base;
using WoWonder.Activities.Chat.GroupChat.Adapter;
using WoWonder.Activities.Chat.MsgTabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Chat.GroupChat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GroupRequestActivity : BaseActivity
    {
        #region Variables Basic

        private GroupRequestsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AdView MAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                MAdView?.Resume();
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        protected override void OnDestroy()
        {
            try
            {
                MAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_GroupRequest);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                }
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
                MAdapter = new GroupRequestsAdapter(this)
                {
                    GroupList = new ObservableCollection<GroupChatRequest>(ListUtils.GroupRequestsList)
                };

                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                //MRecycler.HasFixedSize = true;
                //MRecycler.SetItemViewCacheSize(10);
                //MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
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
                    MAdapter.AddButtonItemClick += MAdapterOnAddButtonItemClick;
                    MAdapter.DeleteButtonItemClick += MAdapterOnDeleteButtonItemClick;
                }
                else
                {
                    MAdapter.AddButtonItemClick -= MAdapterOnAddButtonItemClick;
                    MAdapter.DeleteButtonItemClick -= MAdapterOnDeleteButtonItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnAddButtonItemClick(object sender, GroupRequestsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.AcceptGroupChatRequest(item.GroupId) });

                        var index = MAdapter.GroupList.IndexOf(item);
                        if (index != -1)
                        {
                            MAdapter.GroupList.Remove(item);

                            MAdapter.NotifyItemRemoved(index);
                            MAdapter.NotifyItemRangeChanged(index, MAdapter.GroupList.Count);
                           
                            ListUtils.GroupRequestsList.Remove(item); 
                        }

                        ShowEmptyPage();
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnDeleteButtonItemClick(object sender, GroupRequestsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.GroupChat.RejectGroupChatRequest(item.GroupId) });

                        var index = MAdapter.GroupList.IndexOf(item);
                        if (index != -1)
                        {
                            MAdapter.GroupList.RemoveAt(index);

                            MRecycler.RemoveViewAt(index);
                            MAdapter.NotifyItemRemoved(index);
                            MAdapter.NotifyItemRangeChanged(index, MAdapter.GroupList.Count);

                            ListUtils.GroupRequestsList.Remove(item);
                        }

                        ShowEmptyPage();
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.GroupList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (AppSettings.LastChatSystem == SystemApiGetLastChat.Old)
                    {
                        var tabLayoutFriendRequest = MsgTabbedMainActivity.GetInstance()?.LastGroupChatsTab?.LayoutGroupRequest;
                        if (tabLayoutFriendRequest != null)
                            tabLayoutFriendRequest.Visibility = ViewStates.Gone;
                    }
                     
                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroupRequest);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (ListUtils.GroupRequestsList.Count == 0)
                {
                    if (MsgTabbedMainActivity.GetInstance()?.LastGroupChatsTab != null)
                        MsgTabbedMainActivity.GetInstance().LastGroupChatsTab.LayoutGroupRequest.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}