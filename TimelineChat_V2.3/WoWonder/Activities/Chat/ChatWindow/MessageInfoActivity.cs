using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Chat.ChatWindow.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Chat.ChatWindow
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MessageInfoActivity : BaseActivity
    {
        #region Variables Basic

        private MessageAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private TextView TxtRead , TxtTimeRead, TxtDelivered, TxtTimeDelivered; 
        private string UserId = "";
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

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
                SetContentView(Resource.Layout.InfoMessagesLayout);
                 
                UserId = Intent?.GetStringExtra("UserId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadData();
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
                RewardedVideoAd?.OnResume(this);
                base.OnResume(); 
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
                RewardedVideoAd?.OnPause(this);
                base.OnPause(); 
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
                RewardedVideoAd?.OnDestroy(this);
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
                TxtRead = (TextView)FindViewById(Resource.Id.tvRead);
                TxtTimeRead = (TextView)FindViewById(Resource.Id.timeRead);
                TxtDelivered = (TextView)FindViewById(Resource.Id.tvDelivered);
                TxtTimeDelivered = (TextView)FindViewById(Resource.Id.timeDelivered); 
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
                    toolbar.Title = GetText(Resource.String.Lbl_MessageInfo);

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
                MAdapter = new MessageAdapter(this ,UserId , false)
                {
                   DifferList = new ObservableCollection<AdapterModelsClassMessage>()
                };
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
         
        #endregion

        #region Events
         
        #endregion

        #region Load Data 

        private void LoadData()
        {
            try
            {
                var item = JsonConvert.DeserializeObject<MessageDataExtra>(Intent?.GetStringExtra("SelectedItem"));
                if (item != null)
                {
                    MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                    {
                        MesData = item,
                        TypeView = item.ModelType
                    });
                    MAdapter.NotifyDataSetChanged();
                     
                    DateTime dateTime = Methods.Time.UnixTimeStampToDateTime(int.Parse(item.Time)); 
                    TxtTimeDelivered.Text = dateTime.ToLongDateString() + ", " + dateTime.ToShortTimeString();

                    if (item.Seen != "0")
                    {
                        DateTime dateTimeSeen = Methods.Time.UnixTimeStampToDateTime(int.Parse(item.Seen));
                        TxtTimeRead.Text = dateTimeSeen.ToLongDateString() + ", " + dateTimeSeen.ToShortTimeString();
                    }
                    else
                    {
                        TxtTimeRead.Text = "---";
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