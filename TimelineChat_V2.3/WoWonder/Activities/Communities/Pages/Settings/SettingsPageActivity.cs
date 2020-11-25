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
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Offers;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Pages.Settings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SettingsPageActivity : BaseActivity 
    {
        #region Variables Basic

        private SettingsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private AdView MAdView;
        private string PageId = "";
        private PageClass PageData;

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

                PageId = Intent?.GetStringExtra("PagesId");

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("PageData")))
                    PageData = JsonConvert.DeserializeObject<PageClass>(Intent?.GetStringExtra("PageData"));

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                AdsGoogle.Ad_Interstitial(this);
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
                MAdapter = new SettingsAdapter(this, "Page" , PageData);
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

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                PageId = null!;
                PageData = null!;
                MAdView = null!;
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
                            var intent = new Intent(this, typeof(PageGeneralActivity));
                            intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                            intent.PutExtra("PageId", PageId);
                            StartActivityForResult(intent , 1250);
                        }
                        else if (item.Id == 2) // PageInformation
                        {
                            var intent = new Intent(this, typeof(PageInfoActivity));
                            intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                            intent.PutExtra("PageId", PageId);
                            StartActivityForResult(intent, 1250);
                        }
                        else if (item.Id == 3) //ActionButtons
                        {
                            var intent = new Intent(this, typeof(PageActionButtonsActivity));
                            intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                            intent.PutExtra("PageId", PageId);
                            StartActivityForResult(intent, 1250);
                        }
                        else if (item.Id == 4) //SocialLinks
                        {
                            var intent = new Intent(this, typeof(PageSocialLinksActivity));
                            intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                            intent.PutExtra("PageId", PageId);
                            StartActivityForResult(intent, 1250);
                        }
                        else if (item.Id == 5)//OfferAJob
                        {
                            var intent = new Intent(this, typeof(OfferAJobActivity));
                            intent.PutExtra("PageId", PageId);
                            StartActivity(intent);
                        }
                        else if (item.Id == 6)//Offer
                        {
                            var intent = new Intent(this, typeof(CreateOffersActivity));
                            intent.PutExtra("PageId", PageId);
                            StartActivity(intent);
                        }
                        else if (item.Id == 7)//Admin
                        {
                            var intent = new Intent(this, typeof(PagesAdminActivity));
                            intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                            intent.PutExtra("PageId", PageId);
                            StartActivity(intent);
                        }
                        else if (item.Id == 8)//DeletePage
                        {
                            var intent = new Intent(this, typeof(DeleteCommunitiesActivity));
                            intent.PutExtra("Id", PageId);
                            intent.PutExtra("Type", "Page");
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
               
                if (requestCode == 2019 && resultCode == Result.Ok)
                {
                    var manged = PagesActivity.GetInstance().MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.MangedPages);
                    var dataListGroup = manged?.PagesModelClass.PagesList?.FirstOrDefault(a => a.PageId == PageId);
                    if (dataListGroup != null)
                    {
                        manged.PagesModelClass.PagesList.Remove(dataListGroup);
                        PagesActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                        ListUtils.MyPageList.Remove(dataListGroup);

                        Finish();
                    } 

                    Intent returnIntent = new Intent();
                    SetResult(Result.Ok, returnIntent);
                    Finish();
                }
                else if (requestCode == 1250 && resultCode == Result.Ok)
                {
                    var pageItem = data.GetStringExtra("pageItem") ?? "";
                    if (string.IsNullOrEmpty(pageItem))
                    {
                        PageData = JsonConvert.DeserializeObject<PageClass>(Intent?.GetStringExtra("pageItem"));
                        PageProfileActivity.PageData = PageData;
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