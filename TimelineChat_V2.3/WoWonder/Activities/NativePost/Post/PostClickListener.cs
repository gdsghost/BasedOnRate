using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;

using Android.Support.V4.Graphics.Drawable;

using Android.Support.V7.Content.Res;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Exoplayer2;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.EditPost;
using WoWonder.Activities.Events;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.General;
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Market;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Share;
using WoWonder.Activities.Offers;
using WoWonder.Activities.PostData;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UsersPages;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Timer = System.Timers.Timer;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Post
{
    public interface IOnPostItemClickListener
    {
        void ProfilePostClick(ProfileClickEventArgs e, string type, string typeEvent);
        void CommentPostClick(GlobalClickEventArgs e ,string type = "Normal");
        void SharePostClick(GlobalClickEventArgs e, PostModelType clickType);
        void CopyLinkEvent(string text);
        void MorePostIconClick(GlobalClickEventArgs item);
        void ImagePostClick(GlobalClickEventArgs item);
        void YoutubePostClick(GlobalClickEventArgs item);
        void LinkPostClick(GlobalClickEventArgs item, string type);
        void ProductPostClick(GlobalClickEventArgs item);
        void FileDownloadPostClick(GlobalClickEventArgs item);
        void OpenFilePostClick(GlobalClickEventArgs item);
        void OpenFundingPostClick(GlobalClickEventArgs item);
        void VoicePostClick(GlobalClickEventArgs item);
        void EventItemPostClick(GlobalClickEventArgs item);
        void ArticleItemPostClick(ArticleDataObject item);
        void DataItemPostClick(GlobalClickEventArgs item);
        void SecondReactionButtonClick(GlobalClickEventArgs item);
        void SingleImagePostClick(GlobalClickEventArgs item);
        void MapPostClick(GlobalClickEventArgs item);
        void OffersPostClick(GlobalClickEventArgs item);
        void JobPostClick(GlobalClickEventArgs item);
        void InitFullscreenDialog(Uri videoUrl, SimpleExoPlayer videoPlayer);
        void OpenAllViewer(string type, string passedId, AdapterModelsClass item);
    }

    public interface IOnLoadMoreListener
    {
        void OnLoadMore(int currentPage);
    }

    public class PostClickListener : Java.Lang.Object, IOnPostItemClickListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity MainContext;
        private readonly NativeFeedType NativeFeedType;

        private PostDataObject DataObject;
        private string TypeDialog;
        public static bool OpenMyProfile;
        public static readonly int MaxProgress = 10000;

        private RatingBar UserRatingBar;
        private ImageView BtnDeleteRating;
        private TextView RatingCountText;
        private ProgressBar Star5Progress;
        private TextView Star5TextView;
        private ProgressBar Star4Progress;
        private TextView Star4TextView;
        private ProgressBar Star3Progress;
        private TextView Star3TextView;
        private ProgressBar Star2Progress;
        private TextView Star2TextView;
        private ProgressBar Star1Progress;
        private TextView Star1TextView;

        private float CurrentRating = 0;
        private GlobalClickEventArgs PostData;
        private NativePostAdapter NativeFeedAdapter;

        private Android.Support.V7.App.AlertDialog MRateListDialog;
        private int CurrentTab = 0;
        private LinearLayout CloseDialogBtn;
        private LinearLayout RateButtonAll;
        private LinearLayout RateButton5;
        private LinearLayout RateButton4;
        private LinearLayout RateButton3;
        private LinearLayout RateButton2;
        private LinearLayout RateButton1;
        private ProgressBar LoadingBar;
        private TextView EmptyText;
        private RecyclerView MRecycler;
        public RateUsersAdapter MAdapter;
        private LinearLayoutManager LayoutManager;

        public PostClickListener(Activity context , NativeFeedType nativeFeedType)
        {
            MainContext = context;
            NativeFeedType = nativeFeedType;
            OpenMyProfile = false;
        }

        public void ProfilePostClick(ProfileClickEventArgs e, string type, string typeEvent)
        {
            try
            {
                var username = e.View.FindViewById<TextView>(Resource.Id.username);
                if (username != null && username.Text.Contains(MainContext.GetText(Resource.String.Lbl_SharedPost)) && typeEvent == "Username")
                {
                    var intent = new Intent(MainContext, typeof(ViewFullPostActivity));
                    intent.PutExtra("Id", e.NewsFeedClass.ParentId);
                    intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                    MainContext.StartActivity(intent);
                }
                else if (username != null && username.Text == MainContext.GetText(Resource.String.Lbl_Anonymous))
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_SorryUserIsAnonymous), ToastLength.Short)?.Show();
                }
                else if (e.NewsFeedClass.PageId != null && e.NewsFeedClass.PageId != "0" && NativeFeedType != NativeFeedType.Page)
                {
                    var intent = new Intent(MainContext, typeof(PageProfileActivity));
                    intent.PutExtra("PageObject", JsonConvert.SerializeObject(e.NewsFeedClass.Publisher));
                    intent.PutExtra("PageId", e.NewsFeedClass.PageId);
                    MainContext.StartActivity(intent);
                }
                else if (e.NewsFeedClass.GroupId != null && e.NewsFeedClass.GroupId != "0" && NativeFeedType != NativeFeedType.Group)
                {
                    var intent = new Intent(MainContext, typeof(GroupProfileActivity));
                    intent.PutExtra("GroupObject", JsonConvert.SerializeObject(e.NewsFeedClass.GroupRecipient));
                    intent.PutExtra("GroupId", e.NewsFeedClass.GroupId);
                    MainContext.StartActivity(intent);
                }
                else
                {
                    if (type == "CommentClass")
                    {
                        WoWonderTools.OpenProfile(MainContext, e.CommentClass.UserId, e.CommentClass.Publisher);
                    }
                    else
                    {
                        WoWonderTools.OpenProfile(MainContext, e.NewsFeedClass.UserId, e.NewsFeedClass.Publisher);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public async Task RatePostClickAsync(GlobalClickEventArgs postData, NativePostAdapter nativeFeedAdapter)
        {
            try
            {
                PostData = postData;
                NativeFeedAdapter = nativeFeedAdapter;

                //Show Rating Dialog
                Android.Support.V7.App.AlertDialog.Builder dialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(MainContext);

                //Irrelevant code for customizing the buttons and title
                LayoutInflater inflater = (LayoutInflater)MainContext.GetSystemService(Context.LayoutInflaterService);
                View dialogView = inflater.Inflate(Resource.Layout.XRatingDialogLayout, null);

                dialogBuilder.SetView(dialogView);
                var MReactAlertDialog = dialogBuilder.Create();
                MReactAlertDialog.Window?.SetBackgroundDrawableResource(Resource.Xml.react_dialog_shape);

                Window window = MReactAlertDialog.Window;
                window?.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                // View Components Initialization
                UserRatingBar = dialogView.FindViewById<RatingBar>(Resource.Id.UserRatingBar);
                BtnDeleteRating = dialogView.FindViewById<ImageView>(Resource.Id.delRatingBtn);
                RatingCountText = dialogView.FindViewById<TextView>(Resource.Id.RatingCountText);
                Star5Progress = dialogView.FindViewById<ProgressBar>(Resource.Id.star5Progress);
                Star5TextView = dialogView.FindViewById<TextView>(Resource.Id.star5TextView);
                Star4Progress = dialogView.FindViewById<ProgressBar>(Resource.Id.star4Progress);
                Star4TextView = dialogView.FindViewById<TextView>(Resource.Id.star4TextView);
                Star3Progress = dialogView.FindViewById<ProgressBar>(Resource.Id.star3Progress);
                Star3TextView = dialogView.FindViewById<TextView>(Resource.Id.star3TextView);
                Star2Progress = dialogView.FindViewById<ProgressBar>(Resource.Id.star2Progress);
                Star2TextView = dialogView.FindViewById<TextView>(Resource.Id.star2TextView);
                Star1Progress = dialogView.FindViewById<ProgressBar>(Resource.Id.star1Progress);
                Star1TextView = dialogView.FindViewById<TextView>(Resource.Id.star1TextView);

                //Get Previous Ratings
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                //send api
                (int apiStatus, var respond) = await CustomAPI.GetPostRatingAsync(postData.NewsFeedClass.Id);
                if (apiStatus == 200)
                {
                    if (respond is PostRatingObject result)
                    {
                        UserRatingBar.Rating = (float)Convert.ToDouble(result.UserRating);
                        CurrentRating = (float)Convert.ToDouble(result.UserRating);
                        if (result.AllRatings.Count > 0)
                            RatingCountText.Text = result.AllRatings[0].All.ToString() + " User(s) Rated";
                        if (CurrentRating > 0)
                            BtnDeleteRating.Visibility = ViewStates.Visible;

                        foreach (var rating in result.AllRatings)
                        {
                            if (rating.Text == "5")
                            {
                                Star5Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star5TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "4")
                            {
                                Star4Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star4TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "3")
                            {
                                Star3Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star3TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "2")
                            {
                                Star2Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star2TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "1")
                            {
                                Star1Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star1TextView.Text = rating.Percentage;
                            }

                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, "Unable to load User Ratings.", ToastLength.Short)?.Show();
                    //return;
                }

                // Events for Dialog layout  
                UserRatingBar.RatingBarChange += UserRatingBar_Change;
                BtnDeleteRating.Click += BtnDeleteRating_Click;
                Star5Progress.Click += Star5Progress_Click;
                Star4Progress.Click += Star4Progress_Click;
                Star3Progress.Click += Star3Progress_Click;
                Star2Progress.Click += Star2Progress_Click;
                Star1Progress.Click += Star1Progress_Click;
                MReactAlertDialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void BtnDeleteRating_Click(object sender, EventArgs e)
        {
            try
            {
                var builder = new Android.Support.V7.App.AlertDialog.Builder(MainContext);
                builder.SetTitle("Delete Rating");
                builder.SetMessage("Are you sure you want to delete your rating?");
                builder.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Yes), async delegate
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        return;
                    }
                    //send api
                    (int apiStatus, var respond) = await CustomAPI.DeletePostRatingAsync(PostData.NewsFeedClass.Id);
                    if (apiStatus == 200)
                    {
                        if (respond is PostRatingObject result)
                        {
                            UserRatingBar.Rating = 0;
                            CurrentRating = 0;
                            BtnDeleteRating.Visibility = ViewStates.Gone;
                            if (result.AllRatings.Count > 0)
                                RatingCountText.Text = result.AllRatings[0].All.ToString() + " User(s) Rated";
                            else
                                RatingCountText.Text = "0 User(s) Rated";

                            Star5Progress.Progress = 0;
                            Star5TextView.Text = "0%";
                            Star4Progress.Progress = 0;
                            Star4TextView.Text = "0%";
                            Star3Progress.Progress = 0;
                            Star3TextView.Text = "0%";
                            Star2Progress.Progress = 0;
                            Star2TextView.Text = "0%";
                            Star1Progress.Progress = 0;
                            Star1TextView.Text = "0%";

                            foreach (var rating in result.AllRatings)
                            {
                                if (rating.Text == "5")
                                {
                                    Star5Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                    Star5TextView.Text = rating.Percentage;
                                }
                                else if (rating.Text == "4")
                                {
                                    Star4Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                    Star4TextView.Text = rating.Percentage;
                                }
                                else if (rating.Text == "3")
                                {
                                    Star3Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                    Star3TextView.Text = rating.Percentage;
                                }
                                else if (rating.Text == "2")
                                {
                                    Star2Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                    Star2TextView.Text = rating.Percentage;
                                }
                                else if (rating.Text == "1")
                                {
                                    Star1Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                    Star1TextView.Text = rating.Percentage;
                                }

                            }

                            PostData.NewsFeedClass.Reaction ??= new Reaction();
                            PostData.NewsFeedClass.Reaction.IsReacted = false;
                            PostData.NewsFeedClass.Reaction.Type = result.UserRating;
                            PostData.NewsFeedClass.Reaction.Count = 0;
                            if (result.AllRatings.Count > 0)
                                PostData.NewsFeedClass.Reaction.Count = result.AllRatings[0].All;

                            var dataGlobal = NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == PostData.NewsFeedClass.PostId).ToList();
                            if (dataGlobal?.Count > 0)
                            {
                                foreach (var dataClass in from dataClass in dataGlobal let index = NativeFeedAdapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                {
                                    dataClass.PostData = PostData.NewsFeedClass;
                                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                                        dataClass.PostData.PostLikes = PostData.NewsFeedClass.Reaction.Count.ToString();
                                    else
                                        dataClass.PostData.PostLikes = PostData.NewsFeedClass.PostLikes;
                                    dataClass.PostData.Reaction.Type = result.UserRating;
                                    NativeFeedAdapter.NotifyItemChanged(NativeFeedAdapter.ListDiffer.IndexOf(dataClass), "reaction");
                                }
                            }

                            var likeCount = PostData.View?.FindViewById<TextView>(Resource.Id.Likecount);
                            if (likeCount != null)
                            {
                                likeCount.Text = PostData.NewsFeedClass.Reaction.Count + " " + Application.Context.Resources?.GetString(Resource.String.Lbl_Reactions);
                            }

                            Toast.MakeText(MainContext, "Rating deleted!", ToastLength.Short)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(MainContext, "Unable to delete rating", ToastLength.Short)?.Show();
                        UserRatingBar.Rating = CurrentRating;
                        return;
                    }
                });
                builder.SetNegativeButton(MainContext.GetText(Resource.String.Lbl_No), delegate { });
                builder.Create().Show();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private async void UserRatingBar_Change(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            if (UserRatingBar.Rating > 0 && UserRatingBar.Rating != CurrentRating)
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                //send api
                (int apiStatus, var respond) = await CustomAPI.AddPostRatingAsync(PostData.NewsFeedClass.Id, UserRatingBar.Rating.ToString());
                if (apiStatus == 200)
                {
                    if (respond is PostRatingObject result)
                    {
                        CurrentRating = UserRatingBar.Rating;
                        BtnDeleteRating.Visibility = ViewStates.Visible;
                        if (result.AllRatings.Count > 0)
                            RatingCountText.Text = result.AllRatings[0].All.ToString() + " User(s) Rated";

                        Star5Progress.Progress = 0;
                        Star5TextView.Text = "0%";
                        Star4Progress.Progress = 0;
                        Star4TextView.Text = "0%";
                        Star3Progress.Progress = 0;
                        Star3TextView.Text = "0%";
                        Star2Progress.Progress = 0;
                        Star2TextView.Text = "0%";
                        Star1Progress.Progress = 0;
                        Star1TextView.Text = "0%";

                        foreach (var rating in result.AllRatings)
                        {
                            if (rating.Text == "5")
                            {
                                Star5Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star5TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "4")
                            {
                                Star4Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star4TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "3")
                            {
                                Star3Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star3TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "2")
                            {
                                Star2Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star2TextView.Text = rating.Percentage;
                            }
                            else if (rating.Text == "1")
                            {
                                Star1Progress.Progress = Convert.ToInt32(rating.PercentageNum);
                                Star1TextView.Text = rating.Percentage;
                            }

                        }

                        PostData.NewsFeedClass.Reaction ??= new Reaction();
                        PostData.NewsFeedClass.Reaction.IsReacted = true;
                        PostData.NewsFeedClass.Reaction.Type = result.UserRating;
                        PostData.NewsFeedClass.Reaction.Count = 0;
                        if (result.AllRatings.Count > 0)
                            PostData.NewsFeedClass.Reaction.Count = result.AllRatings[0].All;

                        var dataGlobal = NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == PostData.NewsFeedClass.PostId).ToList();
                        if (dataGlobal?.Count > 0)
                        {
                            foreach (var dataClass in from dataClass in dataGlobal let index = NativeFeedAdapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                            {
                                dataClass.PostData = PostData.NewsFeedClass;
                                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                                    dataClass.PostData.PostLikes = PostData.NewsFeedClass.Reaction.Count.ToString();
                                else
                                    dataClass.PostData.PostLikes = PostData.NewsFeedClass.PostLikes;
                                dataClass.PostData.Reaction.Type = result.UserRating;
                                NativeFeedAdapter.NotifyItemChanged(NativeFeedAdapter.ListDiffer.IndexOf(dataClass), "reaction");
                            }
                        }

                        var likeCount = PostData.View?.FindViewById<TextView>(Resource.Id.Likecount);
                        if (likeCount != null)
                        {
                            likeCount.Text = PostData.NewsFeedClass.Reaction.Count + " " + Application.Context.Resources?.GetString(Resource.String.Lbl_Reactions);
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, "Unable to post rating", ToastLength.Short)?.Show();
                    UserRatingBar.Rating = CurrentRating;
                    return;
                }
            }
        }
        private void Star5Progress_Click(object sender, EventArgs e)
        {
            CurrentTab = 5;
            InitializeDialog();
        }
        private void Star4Progress_Click(object sender, EventArgs e)
        {
            CurrentTab = 4;
            InitializeDialog();
        }
        private void Star3Progress_Click(object sender, EventArgs e)
        {
            CurrentTab = 3;
            InitializeDialog();
        }
        private void Star2Progress_Click(object sender, EventArgs e)
        {
            CurrentTab = 2;
            InitializeDialog();
        }
        private void Star1Progress_Click(object sender, EventArgs e)
        {
            CurrentTab = 1;
            InitializeDialog();
        }
        private void InitializeDialog()
        {
            try
            {
                //Show Rating Dialog
                Android.Support.V7.App.AlertDialog.Builder dialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(MainContext);

                //Irrelevant code for customizing the buttons and title
                LayoutInflater inflater = (LayoutInflater)MainContext.GetSystemService(Context.LayoutInflaterService);
                View dialogView = inflater.Inflate(Resource.Layout.XRateListDialogLayout, null);

                dialogBuilder.SetView(dialogView);
                MRateListDialog = dialogBuilder.Create();
                MRateListDialog.Window?.SetBackgroundDrawableResource(Resource.Xml.react_dialog_shape);

                Window window = MRateListDialog.Window;
                window?.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                WindowManagerLayoutParams wlp = window.Attributes;

                wlp.Gravity = GravityFlags.Top;
                wlp.Y = 50;
                window.Attributes = wlp;
                // View Components Initialization
                CloseDialogBtn = dialogView.FindViewById<LinearLayout>(Resource.Id.closeDialogBtn);
                RateButtonAll = dialogView.FindViewById<LinearLayout>(Resource.Id.ratebuttonAll);
                RateButton5 = dialogView.FindViewById<LinearLayout>(Resource.Id.ratebutton5);
                RateButton4 = dialogView.FindViewById<LinearLayout>(Resource.Id.ratebutton4);
                RateButton3 = dialogView.FindViewById<LinearLayout>(Resource.Id.ratebutton3);
                RateButton2 = dialogView.FindViewById<LinearLayout>(Resource.Id.ratebutton2);
                RateButton1 = dialogView.FindViewById<LinearLayout>(Resource.Id.ratebutton1);
                LoadingBar = dialogView.FindViewById<ProgressBar>(Resource.Id.loadingBar);
                EmptyText = dialogView.FindViewById<TextView>(Resource.Id.emptyText);
                MRecycler = dialogView.FindViewById<RecyclerView> (Resource.Id.recyler);

                // Events for Dialog layout  
                CloseDialogBtn.Click += CloseDialogBtn_Click;
                RateButtonAll.Click += RateButtonAll_Click;
                RateButton5.Click += RateButton5_Click;
                RateButton4.Click += RateButton4_Click;
                RateButton3.Click += RateButton3_Click;
                RateButton2.Click += RateButton2_Click;
                RateButton1.Click += RateButton1_Click;
                FocusSelectedTab();
                SetRecyclerViewAdapters();
                LoadPostRateUsersAsync();

                MRateListDialog.Show();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new RateUsersAdapter(MainContext, false, RateUsersAdapter.TypeTextSecondary.LastSeen)
                {
                    UserList = new ObservableCollection<UserDataObject>()
                };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(MainContext);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new Bumptech.Glide.Util.FixedPreloadSizeProvider(10, 10);
                var preLoader = new Library.Anjo.IntegrationRecyclerView.RecyclerViewPreloader<UserDataObject>(MainContext, MAdapter, sizeProvider, 10);
                //MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                //RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                //MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                //MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                //MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                //MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private async void LoadPostRateUsersAsync()
        {
            if (MAdapter.UserList.Count > 0)
            {
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();
            }
            EmptyText.Visibility = ViewStates.Gone;
            LoadingBar.Visibility = ViewStates.Visible;

            //Load Data
            if (!Methods.CheckConnectivity())
            {
                Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                LoadingBar.Visibility = ViewStates.Gone;
                EmptyText.Visibility = ViewStates.Visible;
                return;
            }
            (int apiStatus, var respond) = await CustomAPI.GetPostRateUsersAsync(PostData.NewsFeedClass.Id, "post", CurrentTab.ToString());
            if (apiStatus == 200)
            {
                if (respond is PostRateUsersObject result)
                {
                    int countLikeUserList = MAdapter?.UserList?.Count ?? 0;

                    //Rated Users List
                    var respondListLike = result.RateUsersList.Count;
                    if (respondListLike > 0)
                    {
                        if (countLikeUserList > 0)
                        {
                            foreach (var item in from item in result.RateUsersList let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                MAdapter.UserList.Add(item);
                            }

                            //RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countLikeUserList - 1, MAdapter.UserList.Count - countLikeUserList); });
                            LoadingBar.Visibility = ViewStates.Gone;
                            MAdapter.NotifyItemRangeInserted(countLikeUserList - 1, MAdapter.UserList.Count - countLikeUserList);
                        }
                        else
                        {
                            MAdapter.UserList = new ObservableCollection<UserDataObject>(result.RateUsersList);
                            //RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            LoadingBar.Visibility = ViewStates.Gone;
                            MAdapter.NotifyDataSetChanged();
                        }
                    }
                    else
                    {
                        LoadingBar.Visibility = ViewStates.Gone;
                        EmptyText.Visibility = ViewStates.Visible;
                    }
                } else
                {
                    LoadingBar.Visibility = ViewStates.Gone;
                    EmptyText.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                LoadingBar.Visibility = ViewStates.Gone;
                EmptyText.Visibility = ViewStates.Visible;
                Toast.MakeText(MainContext, "Unable to load Users List.", ToastLength.Short)?.Show();
            }
        }

        private void MAdapterOnItemClick(object sender, RateUsersAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.OpenProfile(MainContext, item.UserId, item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void CloseDialogBtn_Click(object sender, EventArgs e)
        {
            MRateListDialog.Dismiss();
        }

        private void FocusSelectedTab()
        {
            RateButtonAll.SetBackgroundResource(Resource.Drawable.rating_button_default_background);
            RateButton5.SetBackgroundResource(Resource.Drawable.rating_button_default_background);
            RateButton4.SetBackgroundResource(Resource.Drawable.rating_button_default_background);
            RateButton3.SetBackgroundResource(Resource.Drawable.rating_button_default_background);
            RateButton2.SetBackgroundResource(Resource.Drawable.rating_button_default_background);
            RateButton1.SetBackgroundResource(Resource.Drawable.rating_button_default_background);

            if (CurrentTab == 6)
                RateButtonAll.SetBackgroundResource(Resource.Drawable.rating_button_active_background);
            else if (CurrentTab == 5)
                RateButton5.SetBackgroundResource(Resource.Drawable.rating_button_active_background);
            else if (CurrentTab == 4)
                RateButton4.SetBackgroundResource(Resource.Drawable.rating_button_active_background);
            else if (CurrentTab == 3)
                RateButton3.SetBackgroundResource(Resource.Drawable.rating_button_active_background);
            else if (CurrentTab == 2)
                RateButton2.SetBackgroundResource(Resource.Drawable.rating_button_active_background);
            else if (CurrentTab == 1)
                RateButton1.SetBackgroundResource(Resource.Drawable.rating_button_active_background);
        }
        private void RateButtonAll_Click(object sender, EventArgs e)
        {
            if (CurrentTab != 6)
            {
                CurrentTab = 6;
                FocusSelectedTab();
                LoadPostRateUsersAsync();
            }
        }
        private void RateButton5_Click(object sender, EventArgs e)
        {
            if (CurrentTab != 5)
            {
                CurrentTab = 5;
                FocusSelectedTab();
                LoadPostRateUsersAsync();
            }
        }
        private void RateButton4_Click(object sender, EventArgs e)
        {
            if (CurrentTab != 4)
            {
                CurrentTab = 4;
                FocusSelectedTab();
                LoadPostRateUsersAsync();
            }
        }
        private void RateButton3_Click(object sender, EventArgs e)
        {
            if (CurrentTab != 3)
            {
                CurrentTab = 3;
                FocusSelectedTab();
                LoadPostRateUsersAsync();
            }
        }
        private void RateButton2_Click(object sender, EventArgs e)
        {
            if (CurrentTab != 2)
            {
                CurrentTab = 2;
                FocusSelectedTab();
                LoadPostRateUsersAsync();
            }
        }
        private void RateButton1_Click(object sender, EventArgs e)
        {
            if (CurrentTab != 1)
            {
                CurrentTab = 1;
                FocusSelectedTab();
                LoadPostRateUsersAsync();
            }
        }
        public void CommentPostClick(GlobalClickEventArgs e , string type = "Normal")
        {
            try
            {
                var intent = new Intent(MainContext, typeof(CommentActivity));
                intent.PutExtra("PostId", e.NewsFeedClass.Id);
                intent.PutExtra("Type", type); 
                intent.PutExtra("PostObject", JsonConvert.SerializeObject(e.NewsFeedClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public void SharePostClick(GlobalClickEventArgs e, PostModelType clickType)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("ItemData", JsonConvert.SerializeObject(e.NewsFeedClass));
                bundle.PutString("TypePost", JsonConvert.SerializeObject(clickType));
                var activity = (AppCompatActivity)MainContext;
                var searchFilter = new ShareBottomDialogFragment
                {
                    Arguments = bundle
                };
                searchFilter.Show(activity.SupportFragmentManager, "ShareFilter");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        //Event Menu >> Copy Link
        public void CopyLinkEvent(string text)
        {
            try
            { 
                Methods.CopyToClipboard(MainContext, text); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Event Menu >> Delete post
        private void DeletePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeletePost";
                    DataObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeletePost));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //ReportPost
        private void ReportPostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.IsPostReported = true;
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short)?.Show();
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(item.Id, "report") });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //SavePost 
        private async void SavePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.IsPostSaved = true;
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullySaved), ToastLength.Short)?.Show();
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Global.Post_Actions(item.Id, "save").ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is PostActionsObject actionsObject)
                        {
                            item.IsPostSaved = actionsObject.Code.ToString() != "0";
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //BoostPost 
        private async void BoostPostEvent(PostDataObject item)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser?.IsPro != "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var intent = new Intent(MainContext, typeof(GoProActivity));
                    MainContext.StartActivity(intent);
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    item.Boosted = "1";
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Global.Post_Actions(item.Id, "boost");
                    if (apiStatus == 200)
                    {
                        if (respond is PostActionsObject actionsObject)
                        {
                            MainContext.RunOnUiThread(() =>
                            {
                                try
                                {
                                    item.Boosted = actionsObject.Code.ToString();
                                    item.IsPostBoosted = actionsObject.Code.ToString();

                                    var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                    var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (dataGlobal?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.Boosted = actionsObject.Code.ToString();
                                            dataClass.PostData.IsPostBoosted = actionsObject.Code.ToString();
                                            adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass) , "BoostedPost");
                                        }

                                        var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.PromotePost);
                                        if (checkTextSection == null && item.Boosted == "1")
                                        {
                                            var collection = dataGlobal.FirstOrDefault()?.PostData;
                                            var adapterModels = new AdapterModelsClass
                                            {
                                                TypeView = PostModelType.PromotePost,
                                                Id = Convert.ToInt32((int)PostModelType.PromotePost + collection?.Id),
                                                PostData = collection,
                                                IsDefaultFeedPost = true
                                            };

                                            var headerPostIndex = adapterGlobal.ListDiffer.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                            if (headerPostIndex > -1)
                                            {
                                                adapterGlobal.ListDiffer.Insert(headerPostIndex, adapterModels);
                                                adapterGlobal.NotifyItemInserted(headerPostIndex);
                                            }
                                        }
                                        else
                                        {
                                            WRecyclerView.GetInstance().RemoveByRowIndex(checkTextSection);
                                        }
                                    }

                                    var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                    var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.Boosted = actionsObject.Code.ToString();
                                            dataClass.PostData.IsPostBoosted = actionsObject.Code.ToString();
                                            adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass), "BoostedPost");
                                        }

                                        var checkTextSection = data.FirstOrDefault(w => w.TypeView == PostModelType.PromotePost);
                                        if (checkTextSection == null && item.Boosted == "1")
                                        {
                                            var collection = data.FirstOrDefault()?.PostData;
                                            var adapterModels = new AdapterModelsClass
                                            {
                                                TypeView = PostModelType.PromotePost,
                                                Id = Convert.ToInt32((int)PostModelType.PromotePost + collection?.Id),
                                                PostData = collection,
                                                IsDefaultFeedPost = true
                                            };

                                            var headerPostIndex = adapter.ListDiffer.IndexOf(data.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                            if (headerPostIndex > -1)
                                            {
                                                adapter.ListDiffer.Insert(headerPostIndex, adapterModels);
                                                adapter.NotifyItemInserted(headerPostIndex);
                                            }
                                        }
                                        else
                                        {
                                            WRecyclerView.GetInstance().RemoveByRowIndex(checkTextSection);
                                        }
                                    }

                                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyBoosted), ToastLength.Short)?.Show();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e); 
                                }
                            });
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Status Comments Post 
        private async void StatusCommentsPostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.CommentsStatus = "1";
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Global.Post_Actions(item.Id, "disable_comments");
                    if (apiStatus == 200)
                    {
                        if (respond is PostActionsObject actionsObject)
                        {
                            MainContext.RunOnUiThread(() =>
                            {
                                try
                                {
                                    item.CommentsStatus = actionsObject.Code.ToString();

                                    var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                    var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (dataGlobal?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.CommentsStatus = actionsObject.Code.ToString();

                                            adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass));
                                        }
                                    }

                                    var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                    var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.CommentsStatus = actionsObject.Code.ToString();

                                            adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass));
                                        }
                                    }

                                    if (actionsObject.Code == 0)
                                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_PostCommentsDisabled), ToastLength.Short)?.Show();
                                    else
                                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_PostCommentsEnabled), ToastLength.Short)?.Show();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e); 
                                }
                            });
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void MorePostIconClick(GlobalClickEventArgs item)
        {
            try
            {
                DataObject = item.NewsFeedClass;

                var postType = PostFunctions.GetAdapterType(DataObject);

                TypeDialog = "MorePost";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                arrayAdapter.Add(!Convert.ToBoolean(item.NewsFeedClass.IsPostSaved) ? MainContext.GetString(Resource.String.Lbl_SavePost) : MainContext.GetString(Resource.String.Lbl_UnSavePost));

                if (!string.IsNullOrEmpty(item.NewsFeedClass.Orginaltext))
                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_CopeText));

                arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_CopeLink));

                if (!Convert.ToBoolean(item.NewsFeedClass.IsPostReported))
                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_ReportPost));

                if ((item.NewsFeedClass.UserId != "0" || item.NewsFeedClass.PageId != "0" || item.NewsFeedClass.GroupId != "0") && item.NewsFeedClass.Publisher.UserId == UserDetails.UserId)
                {
                    if (postType == PostModelType.ProductPost)
                    {
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EditProduct));
                    }
                    else if (postType == PostModelType.OfferPost)
                    {
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EditOffers));
                    }
                    else
                    {
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EditPost));
                    }

                    if (AppSettings.ShowAdvertisingPost)
                    {
                        switch (item.NewsFeedClass?.Boosted)
                        {
                            case "0":
                                arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_BoostPost));
                                break;
                            case "1":
                                arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_UnBoostPost));
                                break;
                        }
                    }
                     
                    switch (item.NewsFeedClass?.CommentsStatus)
                    {
                        case "0":
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EnableComments));
                            break;
                        case "1":
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_DisableComments));
                            break;
                    }

                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_DeletePost));
                }

                dialogList.Title(MainContext.GetString(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(MainContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void JobPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(JobsViewActivity));
                if (item.NewsFeedClass != null)
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job?.JobInfoClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void JobButtonPostClick(GlobalClickEventArgs item)
        {
            try
            {
                using var jobButton = item.View.FindViewById<Button>(Resource.Id.JobButton);
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext?.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                switch (jobButton?.Tag?.ToString())
                {
                    // Open Apply Job Activity 
                    case "ShowApply":
                    {
                        if (item.NewsFeedClass.Job != null && item.NewsFeedClass.Job.Value.JobInfoClass.ApplyCount == "0")
                        {
                            Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short)?.Show();
                            return;
                        }

                        var intent = new Intent(MainContext, typeof(ShowApplyJobActivity));
                        if (item.NewsFeedClass.Job != null)
                            intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job.Value.JobInfoClass));
                        MainContext.StartActivity(intent);
                        break;
                    }
                    case "Apply":
                    {
                        var intent = new Intent(MainContext, typeof(ApplyJobActivity));
                        if (item.NewsFeedClass.Job != null)
                            intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job.Value.JobInfoClass));
                        MainContext.StartActivity(intent);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void ImagePostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(MultiImagesPostViewerActivity));
                    intent.PutExtra("indexImage", item.Position.ToString()); // Index Image Show
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item.NewsFeedClass)); // PostDataObject
                    MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void SecondReactionButtonClick(GlobalClickEventArgs item)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (UserDetails.SoundControl)
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                var secondReactionText = item.View.FindViewById<TextView>(Resource.Id.SecondReactionText);

                if (AppSettings.PostButton == PostButtonSystem.Wonder)
                {
                    if (item.NewsFeedClass.IsWondered != null && (bool)item.NewsFeedClass.IsWondered)
                    {
                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        if (x > 0)
                            x--;
                        else
                            x = 0;

                        item.NewsFeedClass.IsWondered = false;
                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                        var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_wowonder);
                        var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Btn_Wonder);
                        secondReactionText.SetTextColor(Color.ParseColor("#666666"));
                    }
                    else
                    {
                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        x++;

                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                        item.NewsFeedClass.IsWondered = true;

                        var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_wowonder);
                        var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Lbl_wondered);
                        secondReactionText.SetTextColor(Color.ParseColor("#f89823"));

                        item.NewsFeedClass.Reaction ??= new Reaction();
                        if (item.NewsFeedClass.Reaction.IsReacted != null && item.NewsFeedClass.Reaction.IsReacted.Value)
                        {
                            item.NewsFeedClass.Reaction.IsReacted = false;
                        } 
                    }
                }
                else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                {
                    if (item.NewsFeedClass.IsWondered != null && item.NewsFeedClass.IsWondered.Value)
                    {
                        var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_dislike);
                        var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Btn_Dislike);
                        secondReactionText.SetTextColor(Color.ParseColor("#666666"));

                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        if (x > 0)
                            x--;
                        else
                            x = 0;

                        item.NewsFeedClass.IsWondered = false;
                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        x++;

                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                        item.NewsFeedClass.IsWondered = true;

                        Drawable unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_dislike);
                        Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Lbl_disliked);
                        secondReactionText.SetTextColor(Color.ParseColor("#f89823"));

                        item.NewsFeedClass.Reaction ??= new Reaction();
                        if (item.NewsFeedClass.Reaction.IsReacted != null && item.NewsFeedClass.Reaction.IsReacted.Value)
                        {
                            item.NewsFeedClass.Reaction.IsReacted = false;
                        }
                    }
                }

                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;

                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.NewsFeedClass.Id).ToList();
                if (dataGlobal?.Count > 0)
                {
                    foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                    {
                        dataClass.PostData = item.NewsFeedClass; 
                        adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass), "reaction");
                    }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(item.NewsFeedClass.Id, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(item.NewsFeedClass.Id, "dislike") });
                        break;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void SingleImagePostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(ImagePostViewerActivity));
                    intent.PutExtra("itemIndex", "00"); //PhotoAlbumObject
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item.NewsFeedClass)); // PostDataObject
                    MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void MapPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    // Create a Uri from an intent string. Use the result to create an Intent?. 
                    var uri = Uri.Parse("geo:" + item.NewsFeedClass.CurrentLatitude + "," + item.NewsFeedClass.CurrentLongitude);
                    var intent = new Intent(Intent.ActionView, uri);
                    intent.SetPackage("com.google.android.apps.maps");
                    intent.AddFlags(ActivityFlags.NewTask);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OffersPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(OffersViewActivity));
                    intent.PutExtra("OffersObject", JsonConvert.SerializeObject(item.NewsFeedClass.Offer?.OfferClass));
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenAllViewer(string type, string passedId, AdapterModelsClass item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(AllViewerActivity));
                intent.PutExtra("Type", type); //StoryModel , FollowersModel , GroupsModel , PagesModel , ImagesModel
                intent.PutExtra("PassedId", passedId);

                switch (type)
                {
                    case "StoryModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                        break;
                    case "FollowersModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.FollowersModel));
                        break;
                    case "GroupsModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.GroupsModel));
                        break;
                    case "PagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PagesModel));
                        break;
                    case "ImagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.ImagesModel));
                        break;
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void YoutubePostClick(GlobalClickEventArgs item)
        {
            MainApplication.GetInstance()?.NavigateTo(MainContext, typeof(YoutubePlayerActivity), item.NewsFeedClass);
        }

        public void LinkPostClick(GlobalClickEventArgs item, string type)
        {
            try
            {
                if (type == "LinkPost")
                {
                    if (item.NewsFeedClass.PostLink.Contains(Client.WebsiteUrl) && item.NewsFeedClass.PostLink.Contains("movies/watch/"))
                    {
                        var videoId = item.NewsFeedClass.PostLink.Split("movies/watch/").Last().Replace("/", "");
                        var intent = new Intent(MainContext, typeof(VideoViewerActivity));
                        //intent.PutExtra("Viewer_Video", JsonConvert.SerializeObject(item));
                        intent.PutExtra("VideoId", videoId);
                        MainContext.StartActivity(intent);
                    }
                    else
                    {
                        if (!item.NewsFeedClass.PostLink.Contains("http"))
                            item.NewsFeedClass.PostLink = "http://" + item.NewsFeedClass.PostLink;

                        if (item.NewsFeedClass.PostLink.Contains("tiktok"))
                            new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.PostTikTok);
                        else
                            new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.PostLink);
                    }
                }
                else
                {
                    if (!item.NewsFeedClass.Url.Contains("http"))
                        item.NewsFeedClass.Url = "http://" + item.NewsFeedClass.Url;

                    new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.Url); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void ProductPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ProductViewActivity));
                intent.PutExtra("Id", item.NewsFeedClass?.PostId);
                if (item?.NewsFeedClass?.Product != null)
                {
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.NewsFeedClass?.Product.Value.ProductClass));
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenFundingPostClick(GlobalClickEventArgs item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(FundingViewActivity));
                var postType = PostFunctions.GetAdapterType(item.NewsFeedClass);
                switch (postType)
                {
                    case PostModelType.FundingPost:
                    {
                        if (item.NewsFeedClass?.FundData != null)
                        {
                            if (item.NewsFeedClass?.FundData.Value.FundDataClass.UserData == null)
                                item.NewsFeedClass.FundData.Value.FundDataClass.UserData = item.NewsFeedClass.Publisher;

                            intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item.NewsFeedClass?.FundData.Value.FundDataClass));
                        }

                        break;
                    }
                    case PostModelType.PurpleFundPost:
                    {
                        if (item.NewsFeedClass?.Fund != null)
                        {
                            if (item.NewsFeedClass?.Fund.Value.PurpleFund.Fund.UserData == null)
                                item.NewsFeedClass.Fund.Value.PurpleFund.Fund.UserData = item.NewsFeedClass.Publisher;

                            intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item.NewsFeedClass?.Fund.Value.PurpleFund.Fund));
                        }

                        break;
                    }
                }

                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenFilePostClick(GlobalClickEventArgs item)
        {
            try
            {
                var fileSplit = item.NewsFeedClass.PostFileFull.Split('/').Last();
                string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                if (getFile != "File Dont Exists")
                {
                    File file2 = new File(getFile);
                    var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                    Intent openFile = new Intent(Intent.ActionView, photoUri);
                    openFile.SetFlags(ActivityFlags.NewTask);
                    openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                    MainContext.StartActivity(openFile);
                }
                else
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(item.NewsFeedClass.PostFileFull));
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_FileNotDeviceMemory), ToastLength.Long)?.Show();
            }
        }

        //Download
        public void FileDownloadPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.NewsFeedClass.PostFileFull))
                {
                    Methods.Path.Chack_MyFolder();

                    var fileSplit = item.NewsFeedClass.PostFileFull.Split('/').Last();
                    string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                    if (getFile != "File Dont Exists")
                    {
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_FileExists), ToastLength.Long)?.Show();
                    }
                    else
                    {
                        Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDcimFile, item.NewsFeedClass.PostFileFull);
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourFileIsDownloaded), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public void EventItemPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(EventViewActivity));
                if (item.NewsFeedClass.Event != null)
                    intent.PutExtra("EventView", JsonConvert.SerializeObject(item.NewsFeedClass.Event.Value.EventClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void ArticleItemPostClick(ArticleDataObject item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ArticlesViewActivity));
                intent.PutExtra("Id", item.Id);
                intent.PutExtra("ArticleObject", JsonConvert.SerializeObject(item));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void DataItemPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    if (item.NewsFeedClass.Reaction.Count > 0)
                    {
                        var intent = new Intent(MainContext, typeof(ReactionPostTabbedActivity));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(item.NewsFeedClass));
                        MainContext.StartActivity(intent);
                    }
                }
                else
                {
                    var intent = new Intent(MainContext, typeof(PostDataActivity));
                    intent.PutExtra("PostId", item.NewsFeedClass.Id);
                    intent.PutExtra("PostType", "post_likes");
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        private GlobalClickEventArgs ItemVoicePost; 
        public void VoicePostClick(GlobalClickEventArgs item)
        {
            try
            {
                var instance = WRecyclerView.GetInstance();
                if (instance != null)
                {
                    instance?.StopVideo();
                    instance.ViewHolderVoicePlayer = item.HolderSound.ItemView;
                    instance.IsVoicePlayed = true;
                }
               
                ItemVoicePost = item;

                if (item.HolderSound.PostAdapter.PositionSound != item.Position)
                { 
                    var list = item.HolderSound.PostAdapter.ListDiffer.Where(a => a.TypeView == PostModelType.VoicePost && a.VoicePlayer != null).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var modelsClass in list)
                        {
                            modelsClass.MediaIsPlaying = false;

                            if (modelsClass.VoicePlayer != null)
                            {
                                modelsClass.VoicePlayer.Stop();
                                modelsClass.VoicePlayer.Reset();
                            }
                            modelsClass.VoicePlayer = null;
                            modelsClass.Timer = null;

                            modelsClass.VoicePlayer?.Release();
                            modelsClass.VoicePlayer = null;
                        }

                        item.HolderSound.PostAdapter.NotifyItemChanged(item.HolderSound.PostAdapter.PositionSound, "WithoutBlobAudio");
                    }
                }
                 
                if (item.AdapterModelsClass.VoicePlayer == null) 
                {
                    item.HolderSound.PostAdapter.PositionSound = item.Position;

                    //item.HolderSound.SeekBar.Max = 10000;
                    item.AdapterModelsClass.VoicePlayer = new MediaPlayer();
                    item.AdapterModelsClass.VoicePlayer.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Music)?.Build());
                    item.AdapterModelsClass.VoicePlayer.Completion += (sender, args) =>
                    {
                        try
                        {
                            item.HolderSound.LoadingProgressView.Visibility = ViewStates.Gone;
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;

                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_play);
                            item.HolderSound.PlayButton.Tag = "Play";
                            item.AdapterModelsClass.VoicePlayer.Stop();

                            item.AdapterModelsClass.MediaIsPlaying = false;

                            item.AdapterModelsClass.VoicePlayer.Stop();
                            item.AdapterModelsClass.VoicePlayer.Reset();
                            item.AdapterModelsClass.VoicePlayer = null;
                             
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                item.HolderSound.SeekBar.SetProgress(0, true);
                            else // For API < 24 
                                item.HolderSound.SeekBar.Progress = 0;

                            if (item.AdapterModelsClass.Timer == null) return;
                            item.AdapterModelsClass.Timer.Enabled = false;
                            item.AdapterModelsClass.Timer.Stop();
                            item.AdapterModelsClass.Timer = null;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };

                    item.AdapterModelsClass.VoicePlayer.Prepared += (o, eventArgs) =>
                    {
                        try
                        {
                            item.AdapterModelsClass.MediaIsPlaying = true;
                            
                            item.AdapterModelsClass.VoicePlayer.Start();
                            item.HolderSound.PlayButton.Tag = "Pause";
                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_pause);
                            item.HolderSound.LoadingProgressView.Visibility = ViewStates.Gone;
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;

                            if (item.AdapterModelsClass.Timer == null)
                            {
                                item.AdapterModelsClass.Timer = new Timer { Interval = 1000, Enabled = true };
                                item.AdapterModelsClass.Timer.Elapsed += TimerOnElapsed;
                                item.AdapterModelsClass.Timer.Start();
                            }
                            else
                            {
                                item.AdapterModelsClass.Timer.Enabled = true;
                                item.AdapterModelsClass.Timer.Start();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };

                    item.HolderSound.PlayButton.Visibility = ViewStates.Gone;
                    item.HolderSound.LoadingProgressView.Visibility = ViewStates.Visible;

                    var url = !string.IsNullOrEmpty(item.NewsFeedClass.PostFileFull) ? item.NewsFeedClass.PostFileFull : item.NewsFeedClass.PostRecord;

                    if (!string.IsNullOrEmpty(url) && (url.Contains("file://") || url.Contains("content://") || url.Contains("storage") || url.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.NewsFeedClass.PostFileFull);
                        var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                        item.AdapterModelsClass.VoicePlayer.SetDataSource(MainContext, photoUri);
                        item.AdapterModelsClass.VoicePlayer.Prepare();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.NewsFeedClass.PostRecord) && !item.NewsFeedClass.PostRecord.Contains(Client.WebsiteUrl))
                            url = WoWonderTools.GetTheFinalLink(url);

                        item.AdapterModelsClass.VoicePlayer.SetDataSource(MainContext, Uri.Parse(url));
                        item.AdapterModelsClass.VoicePlayer.PrepareAsync();

                    }

                    item.HolderSound.SeekBar.Max = 10000;

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        item.HolderSound.SeekBar.SetProgress(0, true);
                    else  // For API < 24 
                        item.HolderSound.SeekBar.Progress = 0;

                    item.HolderSound.SeekBar.StartTrackingTouch += (sender, args) =>
                    {
                        try
                        {
                            if (item.AdapterModelsClass.Timer != null)
                            {   
                                item.AdapterModelsClass.Timer.Enabled = false;
                                item.AdapterModelsClass.Timer.Stop();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };

                    item.HolderSound.SeekBar.StopTrackingTouch += (sender, args) =>
                    {
                        try
                        {
                            if (item.AdapterModelsClass.Timer != null)
                            {   
                                item.AdapterModelsClass.Timer.Enabled = false;
                                item.AdapterModelsClass.Timer.Stop();
                            }

                            int seek = args.SeekBar.Progress;

                            int totalDuration = item.AdapterModelsClass.VoicePlayer.Duration;
                            var currentPosition = ProgressToTimer(seek, totalDuration);

                            // forward or backward to certain seconds
                            item.AdapterModelsClass.VoicePlayer.SeekTo((int)currentPosition);

                            if (item.AdapterModelsClass.Timer != null)
                            {   
                                // update timer progress again
                                item.AdapterModelsClass.Timer.Enabled = true;
                                item.AdapterModelsClass.Timer.Start();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };
                }
                else
                {
                    switch (item.HolderSound.PlayButton?.Tag?.ToString())
                    {
                        case "Play":
                        {
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;
                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_pause);
                            item.HolderSound.PlayButton.Tag = "Pause";
                            item.AdapterModelsClass.VoicePlayer?.Start();
                         
                            item.AdapterModelsClass.MediaIsPlaying = true;
                         
                            if (item.AdapterModelsClass.Timer != null)
                            {
                                item.AdapterModelsClass.Timer.Enabled = true;
                                item.AdapterModelsClass.Timer.Start();
                            }

                            break;
                        }
                        case "Pause":
                        {
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;
                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_play);
                            item.HolderSound.PlayButton.Tag = "Play";
                            item.AdapterModelsClass.VoicePlayer?.Pause();

                            item.AdapterModelsClass.MediaIsPlaying = false; 

                            if (item.AdapterModelsClass.Timer == null) return;
                            item.AdapterModelsClass.Timer.Enabled = false;
                            item.AdapterModelsClass.Timer.Stop();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        private long ProgressToTimer(int progress, int totalDuration)
        {
            try
            {
                totalDuration /= 1000;
                var currentDuration = (int)((double)progress / MaxProgress * totalDuration);

                // return current duration in milliseconds
                return currentDuration * 1000;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
                return 0;
            }
        }

        private int GetProgressSeekBar(int currentDuration, int totalDuration)
        {
            try
            {
                // calculating percentage
                double progress = (double)currentDuration / totalDuration * MaxProgress;
                if (progress >= 0)
                {
                    // return percentage
                    return Convert.ToInt32(progress);
                }
                return 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
                return 0;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            MainContext.RunOnUiThread(() =>
            {
                try
                {
                    if (ItemVoicePost.AdapterModelsClass.VoicePlayer != null)
                    {
                        int totalDuration = ItemVoicePost.AdapterModelsClass.VoicePlayer.Duration;
                        int currentDuration = ItemVoicePost.AdapterModelsClass.VoicePlayer.CurrentPosition;

                        // Updating progress bar
                        int progress = GetProgressSeekBar(currentDuration, totalDuration);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        {
                            ItemVoicePost.HolderSound.SeekBar.SetProgress(progress, true);
                        }
                        else
                        {
                            // For API < 24 
                            ItemVoicePost.HolderSound.SeekBar.Progress = progress;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception); 
                }
            });
        }

        //Event Menu >> Edit Info Post if user == is_owner  
        private void EditInfoPost_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditPostActivity));
                intent.PutExtra("PostId", item.Id);
                intent.PutExtra("PostItem", JsonConvert.SerializeObject(item));
                MainContext.StartActivityForResult(intent, 3950);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Event Menu >> Edit Info Product if user == is_owner  
        private void EditInfoProduct_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditProductActivity));
                if (item.Product != null)
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.Product.Value.ProductClass));
                MainContext.StartActivityForResult(intent, 3500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Event Menu >> Edit Info Offers if user == is_owner  
        private void EditInfoOffers_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditOffersActivity));
                intent.PutExtra("OfferId", item.OfferId);
                if (item.Offer != null)
                    intent.PutExtra("OfferItem", JsonConvert.SerializeObject(item.Offer.Value.OfferClass));
                MainContext.StartActivityForResult(intent, 4000); //wael
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenImageLightBox(PostDataObject item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ImagePostViewerActivity));
                intent.PutExtra("itemIndex", "00"); //PhotoAlbumObject
                intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item)); // PostDataObject
                MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void InitFullscreenDialog(Uri videoUrl, SimpleExoPlayer videoPlayer)
        {
            try
            {
                // videoPlayer?.PlayWhenReady = false;

                Intent intent = new Intent(MainContext, typeof(VideoFullScreenActivity));
                intent.PutExtra("videoUrl", videoUrl.ToString());
                //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                MainContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == MainContext.GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent(DataObject.Url);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_CopeText))
                {
                    CopyLinkEvent(Methods.FunString.DecodeString(DataObject.Orginaltext));
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditPost))
                {
                    EditInfoPost_OnClick(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditProduct))
                {
                    EditInfoProduct_OnClick(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditOffers))
                {
                    EditInfoOffers_OnClick(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_ReportPost))
                {
                    ReportPostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_DeletePost))
                {
                    DeletePostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_BoostPost) || text == MainContext.GetString(Resource.String.Lbl_UnBoostPost))
                {
                    BoostPostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EnableComments) || text == MainContext.GetString(Resource.String.Lbl_DisableComments))
                {
                    StatusCommentsPostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_SavePost) || text == MainContext.GetString(Resource.String.Lbl_UnSavePost))
                {
                    SavePostEvent(DataObject);
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
                    if (TypeDialog == "DeletePost")
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (!Methods.CheckConnectivity())
                                {
                                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                    return;
                                }
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(DataObject.Id, "delete") });
                                 
                                var feedTab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                                if (DataObject.SharedInfo.SharedInfoClass != null)
                                {
                                    var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id || a.PostData?.Id == DataObject?.SharedInfo.SharedInfoClass?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }
                                else
                                {
                                    var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }

                                feedTab?.MainRecyclerView?.StopVideo();

                                var profileActivity = MyProfileActivity.GetInstance();
                                if (DataObject.SharedInfo.SharedInfoClass != null)
                                {
                                    var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id || a.PostData?.Id == DataObject?.SharedInfo.SharedInfoClass?.Id).ToList();
                                    if (data?.Count > 0)
                                    { 
                                        foreach (var post in data)
                                        {
                                            profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }
                                else
                                {
                                    var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }

                                var recyclerView = WRecyclerView.GetInstance();
                                if (DataObject.SharedInfo.SharedInfoClass != null)
                                {
                                    var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id || a.PostData?.Id == DataObject?.SharedInfo.SharedInfoClass?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            recyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }
                                else
                                {
                                    var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            recyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }

                                recyclerView?.StopVideo();

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short)?.Show(); 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e); 
                            }
                        });
                    }
                    else
                    {
                        if (p1 == DialogAction.Positive)
                        {
                        }
                        else if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }
                    }
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

        #endregion MaterialDialog

    }
}