using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Java.Lang;
using WoWonder.Activities.Chat.Call.Agora;
using WoWonder.Activities.Chat.Call.Twilio;
using WoWonder.Activities.Chat.MsgTabbes.Adapter;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace WoWonder.Activities.Chat.MsgTabbes.Fragment
{
    public class LastCallsFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback
    {
        #region Variables Basic

        public LastCallsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private Classes.CallUser DataUser;
        private AdView BannerAd;
        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);

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

                InitComponent(view);
                SetRecyclerViewAdapters();
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
                Get_CallUser();
                base.OnResume();
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
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

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
                MAdapter = new LastCallsAdapter(Activity) { MCallUser = new ObservableCollection<Classes.CallUser>() };
                MAdapter.CallClick += MAdapterOnCallClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events 

        private void MAdapterOnCallClick(object sender, LastCallsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        DataUser = item;

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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Call
         
        private void Get_CallUser()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.Get_CallUserList();
                if (localList?.Count > 0)
                {
                    int countList = MAdapter.MCallUser.Count;
                    if (countList > 0)
                    {
                        foreach (var item in from item in localList let check = MAdapter.MCallUser.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            MAdapter.MCallUser.Insert(0, item);
                        }
                    }
                    else
                    {
                        MAdapter.MCallUser = new ObservableCollection<Classes.CallUser>(localList.OrderBy(a => a.Id));
                    }

                    MAdapter.NotifyDataSetChanged();
                }
                

                ShowEmptyPage();
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
                if (MAdapter.MCallUser.Count > 0)
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
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoCall);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
        
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            { 
                if (itemString.ToString() == Context.GetText(Resource.String.Lbl_Voice_call))
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

                    intentVideoCall.PutExtra("UserID", DataUser.UserId);
                    intentVideoCall.PutExtra("avatar", DataUser.Avatar);
                    intentVideoCall.PutExtra("name", DataUser.Name);
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

                    intentVideoCall.PutExtra("UserID", DataUser.UserId);
                    intentVideoCall.PutExtra("avatar", DataUser.Avatar);
                    intentVideoCall.PutExtra("name", DataUser.Name);
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