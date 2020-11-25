using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables; 
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Luseen.Autolinklibrary; 
using Com.Tuyenmonkey.Textdecorator;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using Refractored.Controls;
using WoWonder.Activities.Articles.Adapters;
using WoWonder.Activities.Movies.Adapters;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Articles;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Comment
{
    public  class ReplyCommentBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic
       
        private LinearLayout BubbleLayout;
        private CircleImageView Image;
        private AutoLinkTextView CommentText;
        private TextView TimeTextView;
        private TextView UserName;
        private TextView ReplyTextView;
        private TextView LikeTextView;


        public ArticlesCommentAdapter MAdapterArticles;
        public MoviesCommentAdapter MAdapterMovies;
        private RecyclerView MRecycler;
        public EditText TxtComment;
        private ImageView ImgSent;

        private string Type, IdComment;
        private CommentsArticlesObject ArticlesObject;
        private CommentsMoviesObject MoviesObject;

        #endregion

        public override void SetupDialog(Dialog dialog, int style)
        {
            try
            {
                base.SetupDialog(dialog, style);
                View contentView = View.Inflate(Context, Resource.Layout.BottomSheetReplyCommentLayout, null);
                dialog.SetContentView(contentView);
                var parameters = (CoordinatorLayout.LayoutParams)((View)contentView.Parent).LayoutParameters;
                 
                Type = Arguments.GetString("Type");
                IdComment = Arguments.GetString("Id");

                switch (Type)
                {
                    case "Article":
                    {
                        if (Arguments.ContainsKey("Object"))
                            ArticlesObject = JsonConvert.DeserializeObject<CommentsArticlesObject>(Arguments.GetString("Object"));
                        break;
                    }
                    case "Movies":
                    {
                        if (Arguments.ContainsKey("Object"))
                            MoviesObject = JsonConvert.DeserializeObject<CommentsMoviesObject>(Arguments.GetString("Object"));
                        break;
                    }
                }
                    
                InitComponent(contentView);
                SetRecyclerViewAdapters();

                switch (Type)
                {
                    case "Article":
                        LoadCommentArticle();
                        break;
                    case "Movies":
                        LoadCommentMovies();
                        break;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                BubbleLayout = view.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                Image = view.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = view.FindViewById<AutoLinkTextView>(Resource.Id.active);
                UserName = view.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = view.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = view.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = view.FindViewById<TextView>(Resource.Id.Like);
                 
                var font = Typeface.CreateFromAsset(view.Context.Resources?.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.SetBackgroundResource(Resource.Drawable.comment_rounded_right_layout);

                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recycler_view);
                TxtComment = view.FindViewById<EditText>(Resource.Id.commenttext);

                ImgSent = view.FindViewById<ImageView>(Resource.Id.send);

                if (Type == "Article")
                    ImgSent.Click += ImgSentArticlesOnClick;
                else if (Type == "Movies")
                    ImgSent.Click += ImgSentMoviesOnClick;

                TxtComment.Text = "";
                Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ReplyTextView.Visibility = ViewStates.Invisible;

                LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                 
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
                switch (Type)
                {
                    case "Article":
                    {
                        MAdapterArticles = new ArticlesCommentAdapter(Activity, MRecycler, "Light", ArticlesObject.BlogId, "Reply")
                        {
                            CommentList = new ObservableCollection<CommentsArticlesObject>()
                        };

                        if (!Methods.CheckConnectivity())
                            Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        else 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapterArticles.FetchBlogsApiReply(IdComment, "0") });
                        break;
                    }
                    case "Movies":
                    {
                        MAdapterMovies = new MoviesCommentAdapter(Activity, MRecycler, "Light", MoviesObject.MovieId, "Reply")
                        {
                            CommentList = new ObservableCollection<CommentsMoviesObject>()
                        };

                        if (!Methods.CheckConnectivity())
                            Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapterMovies.FetchMoviesApiReply(IdComment, "0") });
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window?.SetLayout(width, height);
                    dialog.Window?.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {

                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Get Replies

        private void LoadCommentArticle()
        {
            try
            {
                if (ArticlesObject == null) return;

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.LayoutDirection = LayoutDirection.Rtl;

                var changer = new TextSanitizer(CommentText, Activity);
                changer.Load(Methods.FunString.DecodeString(ArticlesObject.Text));

                TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(ArticlesObject.Posted), false);
                UserName.Text = ArticlesObject.UserData.Name;

                GlideImageLoader.LoadImage(Activity, ArticlesObject.UserData.Avatar, Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = ArticlesObject.UserData.Name;
                var textIsPro = string.Empty;

                if (ArticlesObject.UserData.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (ArticlesObject.UserData.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, ArticlesObject.UserData.Name.Length);

                if (ArticlesObject.UserData.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (ArticlesObject.UserData.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                if (ArticlesObject.Replies?.Count > 0)
                    ReplyTextView.Text = Activity.GetText(Resource.String.Lbl_Reply) + " " + "(" + ArticlesObject.Replies + ")";

                if (ArticlesObject.IsCommentLiked)
                {
                    LikeTextView.Text = Activity.GetText(Resource.String.Btn_Liked);
                    LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    LikeTextView.Tag = "Liked";
                }
                else
                {
                    LikeTextView.Text = Activity.GetText(Resource.String.Btn_Like);
                    LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#000000"));
                    LikeTextView.Tag = "Like";
                }

                TimeTextView.Tag = "true";

                if (Image.HasOnClickListeners)
                    return;
                 
                //Create an Event 
                Image.Click += (sender, args) =>
                {
                    try
                    {
                        WoWonderTools.OpenProfile(Activity, ArticlesObject.UserData.UserId, ArticlesObject.UserData);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };
                 
                LikeTextView.Click += delegate
                {
                    try
                    {
                        if (!Methods.CheckConnectivity())
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            return;
                        }

                        if (LikeTextView?.Tag?.ToString() == "Liked")
                        { 
                            LikeTextView.Text = Activity.GetText(Resource.String.Btn_Like);
                            LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#000000"));
                            LikeTextView.Tag = "Like";

                            ArticlesObject.IsCommentLiked = false;
                            //sent api Dislike comment 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Article.LikeUnLikeCommentAsync(ArticlesObject.BlogId, ArticlesObject.Id, false) });

                        }
                        else
                        { 
                            LikeTextView.Text = Activity.GetText(Resource.String.Btn_Liked);
                            LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            LikeTextView.Tag = "Liked";

                            ArticlesObject.IsCommentLiked = true; 
                            //sent api like comment 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Article.LikeUnLikeCommentAsync(ArticlesObject.BlogId, ArticlesObject.Id, true) });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadCommentMovies()
        {
            try
            {
                if (MoviesObject == null) return;

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.LayoutDirection = LayoutDirection.Rtl;

                var changer = new TextSanitizer(CommentText, Activity);
                changer.Load(Methods.FunString.DecodeString(MoviesObject.Text));

                TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(MoviesObject.Posted), false);
                UserName.Text = MoviesObject.UserData.Name;

                GlideImageLoader.LoadImage(Activity, MoviesObject.UserData.Avatar, Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = MoviesObject.UserData.Name;
                var textIsPro = string.Empty;

                if (MoviesObject.UserData.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (MoviesObject.UserData.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, MoviesObject.UserData.Name.Length);

                if (MoviesObject.UserData.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (MoviesObject.UserData.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                if (MoviesObject.Replies?.Count > 0)
                    ReplyTextView.Text = Activity.GetText(Resource.String.Lbl_Reply) + " " + "(" + MoviesObject.Replies + ")";

                if (MoviesObject.IsCommentLiked)
                {
                    LikeTextView.Text = Activity.GetText(Resource.String.Btn_Liked);
                    LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    LikeTextView.Tag = "Liked";
                }
                else
                {
                    LikeTextView.Text = Activity.GetText(Resource.String.Btn_Like);
                    LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#000000"));
                    LikeTextView.Tag = "Like";
                }

                TimeTextView.Tag = "true";

                if (Image.HasOnClickListeners)
                    return;

                //Create an Event 
                Image.Click += (sender, args) =>
                {
                    try
                    {
                        WoWonderTools.OpenProfile(Activity, MoviesObject.UserData.UserId, MoviesObject.UserData);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                LikeTextView.Click += delegate
                {
                    try
                    {
                        if (!Methods.CheckConnectivity())
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            return;
                        }

                        if (LikeTextView?.Tag?.ToString() == "Liked")
                        {
                            LikeTextView.Text = Activity.GetText(Resource.String.Btn_Like);
                            LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#000000"));
                            LikeTextView.Tag = "Like";

                            MoviesObject.IsCommentLiked = false;
                            //sent api Dislike comment 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.LikeUnLikeCommentAsync(MoviesObject.MovieId, MoviesObject.Id, false) });
                        }
                        else
                        {
                            LikeTextView.Text = Activity.GetText(Resource.String.Btn_Liked);
                            LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            LikeTextView.Tag = "Liked";

                            MoviesObject.IsCommentLiked = true;
                            //sent api Dislike comment 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.LikeUnLikeCommentAsync(MoviesObject.MovieId, MoviesObject.Id, false) });

                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Api sent Comment Articles
        private async void ImgSentArticlesOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    CommentsArticlesObject comment = new CommentsArticlesObject
                    {
                        Id = unixTimestamp.ToString(),
                        BlogId = ArticlesObject.BlogId,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Likes = "0",
                        Posted = unixTimestamp.ToString(),
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsArticlesObject>()
                    };

                    MAdapterArticles.CommentList.Add(comment);

                    var index = MAdapterArticles.CommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        MAdapterArticles.NotifyItemInserted(index);
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapterArticles.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapterArticles.EmptyState)
                    {
                        MAdapterArticles.CommentList.Remove(dd);
                        MAdapterArticles.NotifyItemRemoved(MAdapterArticles.CommentList.IndexOf(dd));
                    }

                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    (int apiStatus, var respond) = await RequestsAsync.Article.CreateReply(ArticlesObject.BlogId, IdComment, text);
                    if (apiStatus == 200)
                    {
                        if (respond is GetCommentsArticlesObject result)
                        {
                            var date = MAdapterArticles.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapterArticles.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                            if (date != null)
                            {
                                date = result.Data[0];
                                date.Id = result.Data[0].Id;

                                index = MAdapterArticles.CommentList.IndexOf(MAdapterArticles.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                if (index > -1)
                                {
                                    MAdapterArticles.CommentList[index] = result.Data[0];

                                    //MAdapter.NotifyItemChanged(index);
                                    MRecycler.ScrollToPosition(index);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Api sent Comment Movies
        private async void ImgSentMoviesOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    CommentsMoviesObject comment = new CommentsMoviesObject
                    {
                        Id = unixTimestamp.ToString(),
                        MovieId = MoviesObject.MovieId,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Likes = "0",
                        Posted = unixTimestamp.ToString(),
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsMoviesObject>()
                    };

                    MAdapterMovies.CommentList.Add(comment);

                    var index = MAdapterMovies.CommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        MAdapterMovies.NotifyItemInserted(index);
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapterMovies.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapterMovies.EmptyState)
                    {
                        MAdapterMovies.CommentList.Remove(dd);
                        MAdapterMovies.NotifyItemRemoved(MAdapterMovies.CommentList.IndexOf(dd));
                    }

                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    (int apiStatus, var respond) = await RequestsAsync.Movies.CreateReply(MoviesObject.MovieId, IdComment, text);
                    if (apiStatus == 200)
                    {
                        if (respond is GetCommentsMoviesObject result)
                        {
                            var date = MAdapterMovies.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapterMovies.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                            if (date != null)
                            {
                                date = result.Data[0];
                                date.Id = result.Data[0].Id;

                                index = MAdapterMovies.CommentList.IndexOf(MAdapterMovies.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                if (index > -1)
                                {
                                    MAdapterMovies.CommentList[index] = result.Data[0];

                                    //MAdapter.NotifyItemChanged(index);
                                    MRecycler.ScrollToPosition(index);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
  
        #endregion
         
    }
}