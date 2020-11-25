using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Chat.Adapters;
using WoWonder.Activities.Chat.ChatWindow.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Requests;
using Exception = Java.Lang.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Chat.ChatWindow
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class StartedMessagesActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private MessageAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AdView MAdView;
        private string UserId;
        private AdapterModelsClassMessage SelectedItemPositions;

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

                UserId = Intent?.GetStringExtra("UserId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetMessages();

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
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
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
                    toolbar.Title = GetText(Resource.String.Lbl_StartedMessages);
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
                MAdapter = new MessageAdapter(this , UserId , false)
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (e.Type == Holders.TypeClick.Text || e.Type == Holders.TypeClick.Contact)
                    {
                        item.MesData.ShowTimeText = !item.MesData.ShowTimeText;
                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(item));
                    }
                    else if (e.Type == Holders.TypeClick.File)
                    {
                        var fileName = item.MesData.Media.Split('/').Last();
                        string imageFile = Methods.MultiMedia.CheckFileIfExits(item.MesData.Media);
                        if (imageFile != "File Dont Exists")
                        {
                            try
                            {
                                var extension = fileName.Split('.').Last();
                                string mimeType = MimeTypeMap.GetMimeType(extension);

                                Intent openFile = new Intent();
                                openFile.SetFlags(ActivityFlags.NewTask);
                                openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                                openFile.SetAction(Intent.ActionView);
                                openFile.SetDataAndType(Uri.Parse(imageFile), mimeType);
                                StartActivity(openFile);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }
                        else
                        {
                            var extension = fileName.Split('.').Last();
                            string mimeType = MimeTypeMap.GetMimeType(extension);

                            Intent i = new Intent(Intent.ActionView);
                            i.SetData(Uri.Parse(item.MesData.Media));
                            i.SetType(mimeType);
                            StartActivity(i);
                            // Toast.MakeText(MainActivity, MainActivity.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemLongClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    SelectedItemPositions = MAdapter.GetItem(e.Position);
                    if (SelectedItemPositions != null)
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (e.Type == Holders.TypeClick.Text)
                            arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                        if (SelectedItemPositions.MesData.Position == "right")
                        {
                            arrayAdapter.Add(GetText(Resource.String.Lbl_MessageInfo));
                            arrayAdapter.Add(GetText(Resource.String.Lbl_DeleteMessage));
                        }

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Forward));
                        arrayAdapter.Add(SelectedItemPositions.MesData.IsStarted ? GetText(Resource.String.Lbl_UnFavorite) : GetText(Resource.String.Lbl_Favorite));

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
         
        #endregion
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == GetText(Resource.String.Lbl_Copy))
                {
                    CopyItems();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_MessageInfo))
                {
                    var intent = new Intent(this, typeof(MessageInfoActivity));
                    intent.PutExtra("UserId", UserId);
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Forward))
                {
                    ForwardItems();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_DeleteMessage))
                {
                    DeleteMessageItems();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_UnFavorite) || itemString.ToString() == GetText(Resource.String.Lbl_Favorite))
                {
                    StarMessageItems();
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

        #region Selected

        //Copy Messages
        private void CopyItems()
        {
            try
            {
                string allText = "";
                if (SelectedItemPositions != null && !string.IsNullOrEmpty(SelectedItemPositions.MesData.Text))
                {
                    allText = SelectedItemPositions.MesData.Text;
                }
                Methods.CopyToClipboard(this, allText);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Forward Messages
        private void ForwardItems()
        {
            try
            { 
                if (SelectedItemPositions != null)
                {
                    var intent = new Intent(this, typeof(ForwardMessagesActivity));
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Delete Message
        private void DeleteMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    if (Methods.CheckConnectivity())
                    { 
                        var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                        if (index != -1)
                        {
                            MAdapter.DifferList.Remove(SelectedItemPositions);

                            MAdapter.NotifyItemRemoved(index);
                            MAdapter.NotifyItemRangeChanged(index, MAdapter.DifferList.Count);
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Delete_OneMessageUser(SelectedItemPositions.Id.ToString());
                        

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteMessage(SelectedItemPositions.Id.ToString()) }); 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Star Message 
        private void StarMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    SelectedItemPositions.MesData.IsStarted = !SelectedItemPositions.MesData.IsStarted;

                    var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                    if (index != -1)
                    {
                        MAdapter.NotifyItemChanged(index);
                    }

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.Insert_Or_Delete_To_one_StartedMessagesTable(SelectedItemPositions.MesData);
                    
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void GetMessages()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var list = await dbDatabase.GetStartedMessageList(UserDetails.UserId, UserId);
                if (list.Count > 0)
                {
                    MAdapter.DifferList = new ObservableCollection<AdapterModelsClassMessage>(list);
                    MAdapter.NotifyDataSetChanged();
                }
                
                
                
                ShowEmptyPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;

                if (MAdapter.DifferList.Count > 0)
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
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoStartedMessages);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}