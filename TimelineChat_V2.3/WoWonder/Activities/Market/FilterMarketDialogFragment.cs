using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;

using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.NearbyBusiness;
using WoWonder.Activities.NearbyShops;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Market
{
    public class FilterMarketDialogFragment : BottomSheetDialogFragment, SeekBar.IOnSeekBarChangeListener
    {
        #region Variables Basic

        private TabbedMarketActivity ContextMarket;
        private NearbyShopsActivity ContextNearbyShops;
        private NearbyBusinessActivity ContextNearbyBusiness;

        private TextView IconBack, IconDistance, TxtDistanceCount;
        private SeekBar DistanceBar;
        private Button BtnApply;
        private int DistanceCount;
        private string TypeFilter;

        #endregion

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater.Inflate(Resource.Layout.BottomSheetMarketFilter, container, false);

                TypeFilter = Arguments.GetString("TypeFilter");
                switch (TypeFilter)
                {
                    case "Market":
                        ContextMarket = (TabbedMarketActivity)Activity;
                        break;
                    case "NearbyShops":
                        ContextNearbyShops = (NearbyShopsActivity)Activity;
                        break;
                    case "NearbyBusiness":
                        ContextNearbyBusiness = (NearbyBusinessActivity)Activity;
                        break;
                } 
                
                InitComponent(view);
                  
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
         
        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropright : IonIconsFonts.IosArrowDropleft);
                IconBack.Click += IconBackOnClick;

                IconDistance = view.FindViewById<TextView>(Resource.Id.IconDistance);
                TxtDistanceCount = view.FindViewById<TextView>(Resource.Id.Distancenumber);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDistance,FontAwesomeIcon.StreetView);
                    
                DistanceBar = view.FindViewById<SeekBar>(Resource.Id.distanceSeeker);
                DistanceBar.SetOnSeekBarChangeListener(this);

                if (TypeFilter == "Market")
                {
                    DistanceBar.Max = 300;

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        DistanceBar.SetProgress(string.IsNullOrEmpty(UserDetails.MarketDistanceCount) ? 300 : Convert.ToInt32(UserDetails.MarketDistanceCount), true);
                    else  // For API < 24 
                        DistanceBar.Progress = string.IsNullOrEmpty(UserDetails.MarketDistanceCount) ? 300 : Convert.ToInt32(UserDetails.MarketDistanceCount); 
                }
                else if (TypeFilter == "NearbyShops")
                {
                    DistanceBar.Max = 1000;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        DistanceBar.SetProgress(string.IsNullOrEmpty(UserDetails.NearbyShopsDistanceCount) ? 1000 : Convert.ToInt32(UserDetails.NearbyShopsDistanceCount), true);
                    else  // For API < 24 
                        DistanceBar.Progress = string.IsNullOrEmpty(UserDetails.NearbyShopsDistanceCount) ? 1000 : Convert.ToInt32(UserDetails.NearbyShopsDistanceCount);
                }
                else if (TypeFilter == "NearbyBusiness")
                {
                    DistanceBar.Max = 1000;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        DistanceBar.SetProgress(string.IsNullOrEmpty(UserDetails.NearbyBusinessDistanceCount) ? 1000 : Convert.ToInt32(UserDetails.NearbyBusinessDistanceCount), true);
                    else  // For API < 24 
                        DistanceBar.Progress = string.IsNullOrEmpty(UserDetails.NearbyBusinessDistanceCount) ? 1000 : Convert.ToInt32(UserDetails.NearbyBusinessDistanceCount);
                } 

                BtnApply = view.FindViewById<Button>(Resource.Id.ApplyButton);
                BtnApply.Click += BtnApplyOnClick; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Event

        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (TypeFilter == "Market")
                {
                    UserDetails.MarketDistanceCount = DistanceCount.ToString();

                    ContextMarket.MarketTab.MAdapter.MarketList.Clear();
                    ContextMarket.MarketTab.MAdapter.NotifyDataSetChanged();

                    ContextMarket.MarketTab.MainScrollEvent.IsLoading = false;
                    ContextMarket.MarketTab.SwipeRefreshLayout.Refreshing = true;
                     
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextMarket.GetMarket() });
                }
                else if (TypeFilter == "NearbyShops")
                {
                    UserDetails.NearbyShopsDistanceCount = DistanceCount.ToString();

                    ContextNearbyShops.MAdapter.NearbyShopsList.Clear();
                    ContextNearbyShops.MAdapter.NotifyDataSetChanged();

                    ContextNearbyShops.MainScrollEvent.IsLoading = false;
                    ContextNearbyShops.SwipeRefreshLayout.Refreshing = true;

                    ContextNearbyShops.StartApiService();
                }
                else if (TypeFilter == "NearbyBusiness")
                {
                    UserDetails.NearbyBusinessDistanceCount = DistanceCount.ToString();

                    ContextNearbyBusiness.MAdapter.NearbyBusinessList.Clear();
                    ContextNearbyBusiness.MAdapter.NotifyDataSetChanged();

                    ContextNearbyBusiness.MainScrollEvent.IsLoading = false;
                    ContextNearbyBusiness.SwipeRefreshLayout.Refreshing = true;

                    ContextNearbyBusiness.StartApiService();
                } 
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region SeekBar

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            try
            {
                TxtDistanceCount.Text = progress + " " + GetText(Resource.String.Lbl_km);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {

        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                DistanceCount = seekBar.Progress;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
    }
}