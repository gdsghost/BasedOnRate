﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;


using Android.Views;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Groups.Settings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SettingsGroupActivity : BaseActivity 
    {
        #region Variables Basic

        private SettingsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private AdView MAdView;
        private string GroupId;
        private GroupClass GroupDataClass;
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                GroupId = Intent?.GetStringExtra("GroupId");

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("itemObject")))
                    GroupDataClass = JsonConvert.DeserializeObject<GroupClass>(Intent?.GetStringExtra("itemObject"));

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(this);
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
                RewardedVideoAd?.OnResume(this);
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
                RewardedVideoAd?.OnPause(this);
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

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                EmptyStateLayout.Visibility = ViewStates.Gone;

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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new SettingsAdapter(this, "Group" , null);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Settings);
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
          
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MAdapter.ItemClick += MAdapterOnItemClick;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                MAdView?.Destroy();
                RewardedVideoAd?.OnDestroy(this);

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                GroupId = null!;
                GroupDataClass = null!;
                MAdView = null!;
                RewardedVideoAd = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, SettingsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (item.Id == 1) // General
                        {
                            var intent = new Intent(this, typeof(GroupGeneralActivity));
                            intent.PutExtra("GroupData", JsonConvert.SerializeObject(GroupDataClass));
                            intent.PutExtra("GroupId", GroupId);
                            StartActivityForResult(intent, 1250);
                        }
                        else if (item.Id == 2) //Privacy
                        {
                            var intent = new Intent(this, typeof(GroupPrivacyActivity));
                            intent.PutExtra("GroupData", JsonConvert.SerializeObject(GroupDataClass));
                            intent.PutExtra("GroupId", GroupId);
                            StartActivityForResult(intent, 1250);
                        }
                        else if (item.Id == 3) //Members
                        {
                            var intent = new Intent(this, typeof(GroupMembersActivity));
                            intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                            intent.PutExtra("GroupId", GroupId);
                            StartActivity(intent); 
                        }
                        else if (item.Id == 4)//DeleteGroup
                        {
                            var intent = new Intent(this, typeof(DeleteCommunitiesActivity));
                            intent.PutExtra("Id", GroupId);
                            intent.PutExtra("Type", "Group");
                            StartActivityForResult(intent, 2019);
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                //If its from Camera or Gallery
                if (requestCode == 2019 && resultCode == Result.Ok)
                {
                    var manged = GroupsActivity.GetInstance().MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.MangedGroups);
                    var dataListGroup = manged?.MangedGroupsModel.GroupsList?.FirstOrDefault(a => a.GroupId == GroupId);
                    if (dataListGroup != null)
                    {
                        manged.MangedGroupsModel.GroupsList.Remove(dataListGroup);
                        GroupsActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                        ListUtils.MyGroupList.Remove(dataListGroup); 
                    }
                    Intent returnIntent = new Intent();
                    SetResult(Result.Ok, returnIntent);
                    Finish(); 
                }
                else if (requestCode == 1250 && resultCode == Result.Ok)
                {
                    var groupItem = data.GetStringExtra("groupItem") ?? "";
                    if (string.IsNullOrEmpty(groupItem))
                    {
                        GroupDataClass = JsonConvert.DeserializeObject<GroupClass>(Intent?.GetStringExtra("groupItem"));
                        GroupProfileActivity.GroupDataClass = GroupDataClass;
                    }
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