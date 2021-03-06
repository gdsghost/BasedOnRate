﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Com.Tuyenmonkey.Textdecorator;
using Java.Util;
using Refractored.Controls;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Movies.Adapters
{
    //wael update Adapter
    public class MoviesCommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public string EmptyState = "Wo_Empty_State";
        private static Activity ActivityContext;

        public ObservableCollection<CommentsMoviesObject> CommentList = new ObservableCollection<CommentsMoviesObject>();
        private readonly string ThemeColor;
        private readonly RecyclerScrollListener MainScrollEvent;
        private string ApiIdParameter { get; }
        private static string Type;

        public MoviesCommentAdapter(Activity context, RecyclerView mainRecyclerView, string themeColor, string moviesId, string type)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                var mainRecyclerView1 = mainRecyclerView;
                ThemeColor = themeColor;
                ApiIdParameter = moviesId;
                Type = type;

                var mainLinearLayoutManager = new LinearLayoutManager(context);
                mainRecyclerView1.SetLayoutManager(mainLinearLayoutManager);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentsMoviesObject>(context, this, sizeProvider, 8);
                mainRecyclerView1.AddOnScrollListener(preLoader);

                mainRecyclerView1.SetAdapter(this);
                mainRecyclerView1.HasFixedSize = true;
                mainRecyclerView1.SetItemViewCacheSize(10);
                mainRecyclerView1.ClearAnimation();
                mainRecyclerView1.GetLayoutManager().ItemPrefetchEnabled = true;
                mainRecyclerView1.SetItemViewCacheSize(10);

                MainScrollEvent = new RecyclerScrollListener();
                mainRecyclerView1.AddOnScrollListener(MainScrollEvent);
                MainScrollEvent.LoadMoreEvent += MainScrollEvent_LoadMoreEvent;
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CommentList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case 0:
                        return new MoviesCommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), ThemeColor);
                    case 666:
                        return new AdapterHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new MoviesCommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), ThemeColor);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        private static void LoadCommentData(CommentsMoviesObject item, RecyclerView.ViewHolder viewHolder, int position = 0)
        {
            try
            {
                if (!(viewHolder is MoviesCommentAdapterViewHolder holder))
                    return;

                if (AppSettings.FlowDirectionRightToLeft)
                    holder.BubbleLayout.LayoutDirection = LayoutDirection.Rtl;

                if (string.IsNullOrEmpty(item.Text) || string.IsNullOrWhiteSpace(item.Text))
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }
                else
                {
                    var description = Methods.FunString.DecodeString(item.Text);
                    var readMoreOption = new StReadMoreOption.Builder()
                        .TextLength(250, StReadMoreOption.TypeCharacter)
                        .MoreLabel(ActivityContext.GetText(Resource.String.Lbl_ReadMore))
                        .LessLabel(ActivityContext.GetText(Resource.String.Lbl_ReadLess))
                        .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                        .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                        .LabelUnderLine(true)
                        .Build();
                    readMoreOption.AddReadMoreTo(holder.CommentText, new Java.Lang.String(description));
                }
                  
                holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Posted), false);
                holder.UserName.Text = item.UserData.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = item.UserData.Name;
                var textIsPro = string.Empty;

                if (item.UserData.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (item.UserData.IsPro == "0")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, item.UserData.Name.Length);

                if (item.UserData.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (item.UserData.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                if (item.Replies?.Count > 0)
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + item.Replies.Count + ")";

                if (item.IsCommentLiked)
                {
                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                    holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    holder.LikeTextView.Tag = "Liked";
                }
                else
                {
                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                    holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#000000"));
                    holder.LikeTextView.Tag = "Like";
                }

                holder.TimeTextView.Tag = "true";

                if (holder.Image.HasOnClickListeners)
                    return;

                var postEventListener = new MoviesCommentClickListener(ActivityContext, Type);

                //Create an Event 
                holder.MainView.LongClick += (sender, e) => postEventListener.MoreCommentReplyPostClick(new CommentReplyMoviesClickEventArgs { CommentObject = item, Position = position, View = holder.MainView });

                holder.Image.Click += (sender, args) => postEventListener.ProfileClick(new CommentReplyMoviesClickEventArgs { Holder = holder, CommentObject = item, Position = position, View = holder.MainView });

                switch (Type)
                {
                    case "Comment":
                        holder.ReplyTextView.Click += (sender, args) => VideoViewerActivity.GetInstance()?.CommentReplyClick(item);
                        break;
                    case "Reply":
                        holder.ReplyTextView.Click += (sender, args) => VideoViewerActivity.GetInstance()?.ReplyOnReplyClick(item);
                        break;
                }

                holder.LikeTextView.Click += delegate
                {
                    try
                    {
                        if (!Methods.CheckConnectivity())
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            return;
                        }

                        if (holder.LikeTextView?.Tag?.ToString() == "Liked")
                        {
                            item.IsCommentLiked = false;

                            holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                            holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#000000"));
                            holder.LikeTextView.Tag = "Like";

                            //sent api Dislike comment reply_like
                            switch (Type)
                            {
                                case "Comment":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.LikeUnLikeCommentAsync(item.MovieId, item.Id, false) });
                                    break;
                                case "Reply":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.LikeUnLikeCommentAsync(item.MovieId, item.Id, false, "reply_like") });
                                    break;
                            }
                        }
                        else
                        {
                            item.IsCommentLiked = true;

                            holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                            holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.LikeTextView.Tag = "Liked";

                            //sent api like comment 
                            switch (Type)
                            {
                                case "Comment":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.LikeUnLikeCommentAsync(item.MovieId, item.Id, true) });
                                    break;
                                case "Reply":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.LikeUnLikeCommentAsync(item.MovieId, item.Id, true, "reply_like") });
                                    break;
                            }
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

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder.ItemViewType == 666)
                {
                    if (!(viewHolder is AdapterHolders.EmptyStateAdapterViewHolder emptyHolder))
                        return;

                    emptyHolder.EmptyText.Text = ActivityContext.GetText(Resource.String.Lbl_NoComments);
                    return;
                }

                if (!(viewHolder is MoviesCommentAdapterViewHolder holder))
                    return;

                var item = CommentList[position];
                if (item == null)
                    return;

                LoadCommentData(item, holder, position);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public CommentsMoviesObject GetItem(int position)
        {
            return CommentList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                if (CommentList[position].Text != EmptyState)
                    return 0;
                else if (CommentList[position].Text == EmptyState)
                    return 666;
                else
                    return 0;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommentList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (item.Text != EmptyState)
                    {
                        if (!string.IsNullOrEmpty(item.UserData.Avatar))
                            d.Add(item.UserData.Avatar);

                        return d;
                    }

                    return Collections.SingletonList(p0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }

        private class RecyclerScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            public bool IsLoading { get; set; }


            private LinearLayoutManager LayoutManager;

            public RecyclerScrollListener()
            {
                IsLoading = false;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);


                if (LayoutManager == null)
                    LayoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();

                var visibleItemCount = recyclerView.ChildCount;
                var totalItemCount = recyclerView.GetAdapter().ItemCount;

                var pastItems = LayoutManager.FindFirstVisibleItemPosition();

                if (visibleItemCount + pastItems + 6 < totalItemCount)
                    return;

                if (IsLoading)
                    return;

                LoadMoreEvent?.Invoke(this, null);
            }
        }

        private void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                MainScrollEvent.IsLoading = true;

                var item = CommentList?.LastOrDefault();
                if (item == null)
                    return;

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                switch (Type)
                {
                    case "Comment":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => FetchMoviesApiComments(ApiIdParameter, item.Id) });
                        break;
                    case "Reply":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => FetchMoviesApiReply(item.CommId, item.Id) });
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async Task FetchMoviesApiComments(string moviesId, string offset)
        {
            if (string.IsNullOrEmpty(moviesId))
                return;

            int countList = CommentList.Count;
            var (apiStatus, respond) = await RequestsAsync.Movies.GetComments(moviesId, "25", offset);
            if (apiStatus == 200)
            {
                if (respond is GetCommentsMoviesObject result)
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                CommentList.Add(item);
                            }

                            ActivityContext.RunOnUiThread(() => { NotifyItemRangeInserted(countList, CommentList.Count - countList); });
                        }
                        else
                        {
                            CommentList = new ObservableCollection<CommentsMoviesObject>(result.Data);
                            ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                        }
                    }
                }
            }
            else Methods.DisplayReportResult(ActivityContext, respond);

            MainScrollEvent.IsLoading = false;

            if (CommentList.Count > 0)
            {
                var emptyStateChecker = CommentList.FirstOrDefault(a => a.Text == EmptyState);
                if (emptyStateChecker != null && CommentList.Count > 1)
                {
                    CommentList.Remove(emptyStateChecker);
                    ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                }
            }
            else
            {
                CommentList.Clear();
                var d = new CommentsMoviesObject { Text = EmptyState };
                CommentList.Add(d);
                ActivityContext.RunOnUiThread(NotifyDataSetChanged);
            }
        }

        public async Task FetchMoviesApiReply(string commentId, string offset)
        {
            if (string.IsNullOrEmpty(commentId))
                return;

            int countList = CommentList.Count;
            var (apiStatus, respond) = await RequestsAsync.Movies.GetReply(commentId, "25", offset);
            if (apiStatus == 200)
            {
                if (respond is GetCommentsMoviesObject result)
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                CommentList.Add(item);
                            }

                            ActivityContext.RunOnUiThread(() => { NotifyItemRangeInserted(countList, CommentList.Count - countList); });
                        }
                        else
                        {
                            CommentList = new ObservableCollection<CommentsMoviesObject>(result.Data);
                            ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                        }
                    }
                }
            }
            else Methods.DisplayReportResult(ActivityContext, respond);

            MainScrollEvent.IsLoading = false;

            if (CommentList.Count > 0)
            {
                var emptyStateChecker = CommentList.FirstOrDefault(a => a.Text == EmptyState);
                if (emptyStateChecker != null && CommentList.Count > 1)
                {
                    CommentList.Remove(emptyStateChecker);
                    ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                }
            }
            else
            {
                CommentList.Clear();
                var d = new CommentsMoviesObject { Text = EmptyState };
                CommentList.Add(d);
                ActivityContext.RunOnUiThread(NotifyDataSetChanged);
            }
        }
    }

    public class MoviesCommentAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public LinearLayout BubbleLayout { get; private set; }
        public CircleImageView Image { get; private set; }
        public SuperTextView CommentText { get; private set; }
        public TextView TimeTextView { get; private set; }
        public TextView UserName { get; private set; }
        public TextView ReplyTextView { get; private set; }
        public TextView LikeTextView { get; private set; }

        #endregion

        public MoviesCommentAdapterViewHolder(View itemView, string themeColor) : base(itemView)
        {
            try
            {
                MainView = itemView;

                BubbleLayout = MainView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<SuperTextView>(Resource.Id.active);
                CommentText?.SetTextInfo(CommentText);
                UserName = MainView.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = MainView.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = MainView.FindViewById<TextView>(Resource.Id.Like);

                var font = Typeface.CreateFromAsset(MainView.Context.Resources?.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.SetBackgroundResource(Resource.Drawable.comment_rounded_right_layout);

                if (themeColor != "Dark")
                    return;

                ReplyTextView.SetTextColor(Color.White);
                LikeTextView.SetTextColor(Color.White);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class CommentReplyMoviesClickEventArgs : EventArgs
    {
        public int Position { get; set; }
        public View View { get; set; }
        public RecyclerView.ViewHolder Holder { get; set; }
        public CommentsMoviesObject CommentObject { get; set; }
    }

}
