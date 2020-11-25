using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;

using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.Comment.Fragment;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Uri = Android.Net.Uri;
  
namespace WoWonder.Activities.Comment
{
    public class CommentClickListener : Java.Lang.Object , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        private readonly Activity MainContext;
        private CommentObjectExtra CommentObject;
        private string TypeDialog;
        private readonly string TypeClass;

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
        private CommentReplyClickEventArgs PostData;

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
        
        public CommentClickListener(Activity context, string typeClass)
        {
            MainContext = context;
            TypeClass = typeClass;
        }

        public void ProfilePostClick(ProfileClickEventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(MainContext, e.CommentClass.UserId, e.CommentClass.Publisher);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void MoreCommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "MoreComment";
                    CommentObject = e.CommentObject;

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_CopeText));
                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Report));

                    if (CommentObject?.Owner != null && CommentObject.Owner.Value || CommentObject?.Publisher?.UserId == UserDetails.UserId)
                    {
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Edit));
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Delete));
                    }

                    dialogList.Title(MainContext.GetString(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> Delete Comment
        private void DeleteCommentEvent(CommentObjectExtra item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteComment";
                    CommentObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
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

        //Event Menu >> Edit Comment
        private void EditCommentEvent(CommentObjectExtra item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "EditComment";
                    CommentObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialog.Title(Resource.String.Lbl_Edit);
                    dialog.Input(MainContext.GetString(Resource.String.Lbl_Write_comment), Methods.FunString.DecodeString(item.Text), this);
                    
                    dialog.InputType(InputTypes.TextFlagImeMultiLine);
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Update)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.Build().Show();
                    dialog.AlwaysCallSingleChoiceCallback();
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

        public void CommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            { 
                if (TypeClass == "Reply")
                {
                   var txtComment = ReplyCommentActivity.GetInstance().TxtComment;
                    if (txtComment != null)
                    {
                        txtComment.Text = "";
                        txtComment.Text = "@" + e.CommentObject.Publisher.Username + " ";
                    } 
                }
                else
                {
                    var intent = new Intent(MainContext, typeof(ReplyCommentActivity));
                    intent.PutExtra("CommentId", e.CommentObject.Id);
                    intent.PutExtra("CommentObject", JsonConvert.SerializeObject(e.CommentObject));
                    ReplyCommentActivity.ReplyCommentObject = e.Holder;
                    MainContext.StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void LikeCommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (e.Holder.LikeTextView?.Tag?.ToString() == "Liked")
                { 
                    e.Holder.LikeTextView.Text = MainContext.GetText(Resource.String.Btn_Like);
                    e.Holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    e.Holder.LikeTextView.Tag = "Like";

                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                    {
                        var x = e.CommentObject.Reaction.Count;
                        if (x > 0)
                            e.CommentObject.Reaction.Count--;
                        else
                            e.CommentObject.Reaction.Count = 0;

                        e.CommentObject.Reaction.IsReacted = false;
                        e.CommentObject.Reaction.Type = "";

                        e.Holder.CountRating.Text = e.CommentObject.Reaction.Count + " " + MainContext.GetString(Resource.String.Lbl_Reactions);
                        e.Holder.RatingBar.Rating = (float)Convert.ToDouble(e.CommentObject.Reaction.Type);
                        e.Holder.RatingText.Text = e.CommentObject.Reaction.Type;

                        //if (e.Holder.CountLike != null && e.CommentObject.Reaction.Count > 0)
                        //{
                        //    e.Holder.CountLikeSection.Visibility = ViewStates.Visible;
                        //    e.Holder.CountLike.Text = Methods.FunString.FormatPriceValue(e.CommentObject.Reaction.Count);
                        //}
                        //else
                        //{
                        //    e.Holder.CountLikeSection.Visibility = ViewStates.Gone;
                        //}

                        PollyController.RunRetryPolicyFunction(TypeClass == "Reply" ? new List<Func<Task>> {() => RequestsAsync.Comment.ReactionCommentAsync(e.CommentObject.Id, "" , "reaction_reply") } : new List<Func<Task>> {() => RequestsAsync.Comment.ReactionCommentAsync(e.CommentObject.Id, "") });
                    }
                    else
                    {
                        e.CommentObject.IsCommentLiked = false;

                        PollyController.RunRetryPolicyFunction(TypeClass == "Reply" ? new List<Func<Task>> {() => RequestsAsync.Comment.LikeUnLikeCommentAsync(e.CommentObject.Id, "reply_like")} : new List<Func<Task>> {() => RequestsAsync.Comment.LikeUnLikeCommentAsync(e.CommentObject.Id, "comment_like")});
                    } 
                }
                else
                {
                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                    {
                        new ReactionComment(MainContext, TypeClass)?.ClickDialog(e); 
                    }
                    else
                    { 
                        e.CommentObject.IsCommentLiked = true;

                        e.Holder.LikeTextView.Text = MainContext.GetText(Resource.String.Btn_Liked);
                        e.Holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        e.Holder.LikeTextView.Tag = "Liked";

                        PollyController.RunRetryPolicyFunction(TypeClass == "Reply" ? new List<Func<Task>> {() => RequestsAsync.Comment.LikeUnLikeCommentAsync(e.CommentObject.Id, "reply_like")} : new List<Func<Task>> {() => RequestsAsync.Comment.LikeUnLikeCommentAsync(e.CommentObject.Id, "comment_like")});
                    } 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void RateCommentClickAsync(CommentReplyClickEventArgs postData)
        {
            try
            {
                PostData = postData;
                //NativeFeedAdapter = nativeFeedAdapter;

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
                int apiStatus;
                dynamic respond;
                if (TypeClass == "Reply" && postData.Position != RecyclerView.NoPosition)
                    (apiStatus, respond) = await CustomAPI.GetReplyRatingAsync(postData.CommentObject.Id);
                else
                    (apiStatus, respond) = await CustomAPI.GetCommentRatingAsync(postData.CommentObject.Id);
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
                    return;
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
                    int apiStatus;
                    dynamic respond;
                    if (TypeClass == "Reply" && PostData.Position != RecyclerView.NoPosition)
                        (apiStatus, respond) = await CustomAPI.DeleteReplyRatingAsync(PostData.CommentObject.Id);
                    else
                        (apiStatus, respond) = await CustomAPI.DeleteCommentRatingAsync(PostData.CommentObject.Id);
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

                            PostData.CommentObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();
                            PostData.CommentObject.Reaction.IsReacted = false;
                            PostData.CommentObject.Reaction.Count = 0;
                            if (result.AllRatings.Count > 0)
                                PostData.CommentObject.Reaction.Count = result.AllRatings[0].All;
                            PostData.CommentObject.Reaction.Type = result.UserRating;

                            PostData.Holder.CountRating.Text = PostData.CommentObject.Reaction.Count + " " + MainContext.GetString(Resource.String.Lbl_Reactions);
                            PostData.Holder.RatingBar.Rating = (float)Convert.ToDouble(PostData.CommentObject.Reaction.Type);
                            PostData.Holder.RatingText.Text = PostData.CommentObject.Reaction.Type;

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
                int apiStatus;
                dynamic respond;
                if (TypeClass == "Reply" && PostData.Position != RecyclerView.NoPosition)
                    (apiStatus, respond) = await CustomAPI.AddReplyRatingAsync(PostData.CommentObject.Id, UserRatingBar.Rating.ToString());
                else
                    (apiStatus, respond) = await CustomAPI.AddCommentRatingAsync(PostData.CommentObject.Id, UserRatingBar.Rating.ToString());
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

                        PostData.CommentObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();
                        PostData.CommentObject.Reaction.IsReacted = false;
                        PostData.CommentObject.Reaction.Count = 0;
                        if (result.AllRatings.Count > 0)
                            PostData.CommentObject.Reaction.Count = result.AllRatings[0].All;
                        PostData.CommentObject.Reaction.Type = result.UserRating;

                        PostData.Holder.CountRating.Text = PostData.CommentObject.Reaction.Count + " " + MainContext.GetString(Resource.String.Lbl_Reactions);
                        PostData.Holder.RatingBar.Rating = (float)Convert.ToDouble(PostData.CommentObject.Reaction.Type);
                        PostData.Holder.RatingText.Text = PostData.CommentObject.Reaction.Type;
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
                MRecycler = dialogView.FindViewById<RecyclerView>(Resource.Id.recyler);

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
            (int apiStatus, var respond) = await CustomAPI.GetPostRateUsersAsync(PostData.CommentObject.Id, (TypeClass == "Reply" && PostData.Position != RecyclerView.NoPosition ? "reply" : "comment"), CurrentTab.ToString());
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
                }
                else
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
        public void DislikeCommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                e.CommentObject.IsCommentWondered = e.Holder.DislikeTextView?.Tag?.ToString() != "Disliked";

                PollyController.RunRetryPolicyFunction(TypeClass == "Reply" ? new List<Func<Task>> {() => RequestsAsync.Comment.DislikeUnDislikeCommentAsync(e.CommentObject.Id, "reply_dislike")}
                                                                            : new List<Func<Task>> {() => RequestsAsync.Comment.DislikeUnDislikeCommentAsync(e.CommentObject.Id, "comment_dislike")});

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder when e.CommentObject.IsCommentWondered:
                        {
                            e.Holder.DislikeTextView.Text = MainContext.GetString(Resource.String.Lbl_wondered);
                            e.Holder.DislikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            e.Holder.DislikeTextView.Tag = "Disliked";
                            break;
                        }
                    case PostButtonSystem.Wonder:
                        {
                            e.Holder.DislikeTextView.Text = MainContext.GetString(Resource.String.Btn_Wonder);
                            e.Holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                            e.Holder.DislikeTextView.Tag = "Dislike";
                            break;
                        }
                    case PostButtonSystem.DisLike when e.CommentObject.IsCommentWondered:
                        {
                            e.Holder.DislikeTextView.Text = MainContext.GetString(Resource.String.Lbl_disliked);
                            e.Holder.DislikeTextView.SetTextColor(Color.ParseColor("#f89823"));
                            e.Holder.DislikeTextView.Tag = "Disliked";
                            break;
                        }
                    case PostButtonSystem.DisLike:
                        {
                            e.Holder.DislikeTextView.Text = MainContext.GetString(Resource.String.Btn_Dislike);
                            e.Holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                            e.Holder.DislikeTextView.Tag = "Dislike";
                            break;
                        }
                }

                if (e.Holder.LikeTextView?.Tag?.ToString() == "Liked")
                {
                    e.CommentObject.IsCommentLiked = false;

                    e.Holder.LikeTextView.Text = MainContext.GetText(Resource.String.Btn_Like);
                    e.Holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    e.Holder.LikeTextView.Tag = "Like"; 
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void CountLikeCommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            { 
                var intent = new Intent(MainContext, typeof(ReactionCommentTabbedActivity));
                intent.PutExtra("TypeClass", TypeClass);
                intent.PutExtra("CommentObject", JsonConvert.SerializeObject(e.CommentObject));
                MainContext.StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void OpenImageLightBox(CommentObjectExtra item)
        {
            try
            {
                if (item == null)
                    return;
                string imageUrl;

                if (!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                {
                    imageUrl = item.CFile;
                }
                else
                {
                    if (!item.CFile.Contains(Client.WebsiteUrl))
                        item.CFile = WoWonderTools.GetTheFinalLink(item.CFile);

                    imageUrl = item.CFile;
                }

                MainContext.RunOnUiThread(() =>
                { 
                    var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, imageUrl.Split('/').Last(), imageUrl);
                    if (media.Contains("http"))
                    {
                        Intent intent = new Intent(Intent.ActionView, Uri.Parse(media));
                        MainContext.StartActivity(intent);
                    }
                    else
                    {
                        Java.IO.File file2 = new Java.IO.File(media);
                        var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                        Intent intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        MainContext.StartActivity(intent);
                    }

                    //var getImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                    //if (getImage != "File Dont Exists")
                    //{
                    //    Java.IO.File file2 = new Java.IO.File(getImage);
                    //    var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                    //    Intent intent = new Intent(Intent.ActionPick);
                    //    intent.SetAction(Intent.ActionView);
                    //    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    //    intent.SetDataAndType(photoUri, "image/*");
                    //    MainContext.StartActivity(intent);
                    //}
                    //else
                    //{
                    //    string filename = imageUrl.Split('/').Last();
                    //    string filePath = Path.Combine(Methods.Path.FolderDcimImage);
                    //    string mediaFile = filePath + "/" + filename;

                    //    if (!Directory.Exists(filePath))
                    //        Directory.CreateDirectory(filePath);

                    //    if (!File.Exists(mediaFile))
                    //    {
                    //        WebClient webClient = new WebClient();
                    //        AndHUD.Shared.Show(MainContext, MainContext.GetText(Resource.String.Lbl_Loading));

                    //        webClient.DownloadDataAsync(new Uri(imageUrl));
                    //        webClient.DownloadProgressChanged += (sender, args) =>
                    //        {
                    //            //var progress = args.ProgressPercentage;
                    //            // holder.loadingProgressview.Progress = progress;
                    //            //Show a progress
                    //            AndHUD.Shared.Show(MainContext, MainContext.GetText(Resource.String.Lbl_Loading));
                    //        };
                    //        webClient.DownloadDataCompleted += (s, e) =>
                    //        {
                    //            try
                    //            {
                    //                File.WriteAllBytes(mediaFile, e.Result);

                    //                getImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                    //                if (getImage != "File Dont Exists")
                    //                {
                    //                    Java.IO.File file2 = new Java.IO.File(getImage);

                    //                    Android.Net.Uri photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                    //                    Intent intent = new Intent(Intent.ActionPick);
                    //                    intent.SetAction(Intent.ActionView);
                    //                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    //                    intent.SetDataAndType(photoUri, "image/*");
                    //                    MainContext.StartActivity(intent);
                    //                }
                    //                else
                    //                {
                    //                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    //                }
                    //            }
                    //            catch (Exception exception)
                    //            {
                    //                Methods.DisplayReportResultTrack(exception);
                    //            }

                    //            //var mediaScanIntent = new Intent(Intent?.ActionMediaScannerScanFile);
                    //            //mediaScanIntent?.SetData(Uri.FromFile(new File(mediaFile)));
                    //            //Application.Context.SendBroadcast(mediaScanIntent);

                    //            // Tell the media scanner about the new file so that it is
                    //            // immediately available to the user.
                    //            MediaScannerConnection.ScanFile(Application.Context, new[] { mediaFile }, null, null);

                    //            AndHUD.Shared.Dismiss(MainContext);
                    //        };
                    //    }
                    //}
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlaySound(CommentReplyClickEventArgs args)
        {
            try
            {
                if (args.Holder.CommentAdapter.PositionSound != args.Position)
                {
                    var list = args.Holder.CommentAdapter.CommentList.Where(a => a.MediaPlayer != null).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            item.MediaIsPlaying = false;

                            if (item.MediaPlayer != null)
                            {
                                item.MediaPlayer.Stop();
                                item.MediaPlayer.Reset();
                            }
                            item.MediaPlayer = null!;
                            item.MediaTimer = null!;

                            item.MediaPlayer?.Release();
                            item.MediaPlayer = null!;
                        }

                        args.Holder.CommentAdapter.NotifyItemChanged(args.Holder.CommentAdapter.PositionSound, "WithoutBlobAudio");
                    }
                }

                var fileName = args.CommentObject.Record.Split('/').Last();
                var mediaFile = WoWonderTools.GetFile(args.CommentObject.PostId, Methods.Path.FolderDcimSound, fileName, args.CommentObject.Record);
                 
                if (string.IsNullOrEmpty(args.CommentObject.MediaDuration) || args.CommentObject.MediaDuration == "00:00")
                {
                    var duration = WoWonderTools.GetDuration(mediaFile);
                    args.Holder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                }
                else
                    args.Holder.DurationVoice.Text = args.CommentObject.MediaDuration;

                if (mediaFile.Contains("http"))
                    mediaFile = WoWonderTools.GetFile(args.CommentObject.PostId, Methods.Path.FolderDcimSound, fileName, args.CommentObject.Record);

                if (args.CommentObject.MediaPlayer == null)
                {
                    args.Holder.DurationVoice.Text = "00:00";
                    args.Holder.CommentAdapter.PositionSound = args.Position;
                    args.CommentObject.MediaPlayer = new MediaPlayer();
                    args.CommentObject.MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Music).Build());

                    args.CommentObject.MediaPlayer.Completion += (sender, e) =>
                    {
                        try
                        {
                            args.Holder.PlayButton.Tag = "Play";
                            args.Holder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                            args.CommentObject.MediaIsPlaying = false;

                            args.CommentObject.MediaPlayer.Stop();
                            args.CommentObject.MediaPlayer.Reset();
                            args.CommentObject.MediaPlayer = null!;

                            args.CommentObject.MediaTimer.Enabled = false;
                            args.CommentObject.MediaTimer.Stop();
                            args.CommentObject.MediaTimer = null!;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    };

                    args.CommentObject.MediaPlayer.Prepared += (s, ee) =>
                    {
                        try
                        {
                            args.CommentObject.MediaIsPlaying = true;
                            args.Holder.PlayButton.Tag = "Pause";
                            args.Holder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                             
                            args.CommentObject.MediaTimer ??= new Timer { Interval = 1000 };

                            args.CommentObject.MediaPlayer.Start();

                            //var durationOfSound = message.MediaPlayer.Duration;

                            args.CommentObject.MediaTimer.Elapsed += (sender, eventArgs) =>
                            {
                                args.Holder.CommentAdapter.ActivityContext.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (args.CommentObject.MediaTimer != null && args.CommentObject.MediaTimer.Enabled)
                                        {
                                            if (args.CommentObject.MediaPlayer.CurrentPosition <= args.CommentObject.MediaPlayer.Duration)
                                            {
                                                args.Holder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(args.CommentObject.MediaPlayer.CurrentPosition.ToString());
                                            }
                                            else
                                            {
                                                args.Holder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(args.CommentObject.MediaPlayer.Duration.ToString());

                                                args.Holder.PlayButton.Tag = "Play";
                                                args.Holder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                        args.Holder.PlayButton.Tag = "Play";
                                    }
                                });
                            };
                            args.CommentObject.MediaTimer.Start();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    };

                    if (mediaFile.Contains("http"))
                    {
                        args.CommentObject.MediaPlayer.SetDataSource(args.Holder.CommentAdapter.ActivityContext, Android.Net.Uri.Parse(mediaFile));
                        args.CommentObject.MediaPlayer.PrepareAsync();
                    }
                    else
                    {
                        Java.IO.File file2 = new Java.IO.File(mediaFile);
                        var photoUri = FileProvider.GetUriForFile(args.Holder.CommentAdapter.ActivityContext, args.Holder.CommentAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                        args.CommentObject.MediaPlayer.SetDataSource(args.Holder.CommentAdapter.ActivityContext, photoUri);
                        args.CommentObject.MediaPlayer.PrepareAsync();
                    }

                    //args.CommentObject.SoundViewHolder = soundViewHolder;
                }
                else
                {
                    if (args.Holder.PlayButton?.Tag?.ToString() == "Play")
                    {
                        args.Holder.PlayButton.Tag = "Pause";
                        args.Holder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                         
                        args.CommentObject.MediaIsPlaying = true;
                        args.CommentObject.MediaPlayer?.Start();

                        if (args.CommentObject.MediaTimer != null)
                        {
                            args.CommentObject.MediaTimer.Enabled = true;
                            args.CommentObject.MediaTimer.Start();
                        }
                    }
                    else if (args.Holder.PlayButton?.Tag?.ToString() == "Pause")
                    {
                        args.Holder.PlayButton.Tag = "Play";
                        args.Holder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                        args.CommentObject.MediaIsPlaying = false;
                        args.CommentObject.MediaPlayer?.Pause();

                        if (args.CommentObject.MediaTimer != null)
                        {
                            args.CommentObject.MediaTimer.Enabled = false;
                            args.CommentObject.MediaTimer.Stop();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == MainContext.GetString(Resource.String.Lbl_CopeText))
                {
                    Methods.CopyToClipboard(MainContext,Methods.FunString.DecodeString(CommentObject.Text));
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_Report))
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReportCommentAsync(CommentObject.Id) });

                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_YourReportPost), ToastLength.Short)?.Show();
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_Edit))
                {
                    EditCommentEvent(CommentObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_Delete))
                {
                    DeleteCommentEvent(CommentObject);
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
                    if (TypeDialog == "DeleteComment")
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                switch (TypeClass)
                                {
                                    case "Comment":
                                    {
                                        //TypeClass
                                        var adapterGlobal = CommentActivity.GetInstance()?.MAdapter;
                                        var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                        if (dataGlobal != null)
                                        { 
                                            var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
                                            if (index > -1)
                                            {
                                                adapterGlobal.CommentList.RemoveAt(index);
                                                adapterGlobal.NotifyItemRemoved(index);
                                            }
                                        } 

                                        var dataPost = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.PostId == CommentObject?.PostId).ToList();
                                        if (dataPost?.Count > 0)
                                        {
                                            foreach (var post in dataPost.Where(post => post.TypeView == PostModelType.CommentSection || post.TypeView == PostModelType.AddCommentSection))
                                            {
                                                TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView?.RemoveByRowIndex(post);
                                            }
                                        }

                                        var dataPost2 = WRecyclerView.GetInstance()?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.PostId == CommentObject?.PostId).ToList();
                                        if (dataPost2?.Count > 0)
                                        {
                                            foreach (var post in dataPost2.Where(post => post.TypeView == PostModelType.CommentSection || post.TypeView == PostModelType.AddCommentSection))
                                            { 
                                                WRecyclerView.GetInstance()?.RemoveByRowIndex(post);
                                            }
                                        }

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.DeleteCommentAsync(CommentObject.Id) });
                                        break; 
                                    }
                                    case "Reply":
                                    {
                                        //TypeClass
                                        var adapterGlobal = ReplyCommentActivity.GetInstance()?.MAdapter;
                                        var dataGlobal = adapterGlobal?.ReplyCommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                        if (dataGlobal != null)
                                        {

                                            var index = adapterGlobal.ReplyCommentList.IndexOf(dataGlobal);
                                            if (index > -1)
                                            {
                                                adapterGlobal.ReplyCommentList.RemoveAt(index);
                                                adapterGlobal.NotifyItemRemoved(index);
                                            }
                                        }

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.DeleteCommentAsync(CommentObject.Id, "delete_reply") });
                                        break;
                                    }
                                }

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short)?.Show();
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

        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (p1.Length() > 0)
                {
                    var strName = p1.ToString();

                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                    else
                    {
                        if (TypeClass == "Comment")
                        {
                            //TypeClass
                            var adapterGlobal = CommentActivity.GetInstance()?.MAdapter;
                            var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            {
                                dataGlobal.Text = strName;
                                var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    adapterGlobal.NotifyItemChanged(index);
                                }
                            }

                            var dataPost = WRecyclerView.GetInstance()?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == CommentObject.PostId).ToList();
                            if (dataPost?.Count > 0)
                            {
                                foreach (var post in dataPost)
                                {
                                    if (post.TypeView != PostModelType.CommentSection) 
                                        continue;

                                    var dataComment = post.PostData.GetPostComments?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                    if (dataComment != null)
                                    {
                                        dataComment.Text = strName;
                                        var index = post.PostData.GetPostComments.IndexOf(dataComment);
                                        if (index > -1)
                                        {
                                            WRecyclerView.GetInstance()?.NativeFeedAdapter.NotifyItemChanged(index);
                                        }
                                    }
                                }
                            }

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.EditCommentAsync(CommentObject.Id, strName) });
                        }
                        else if (TypeClass == "Reply")
                        {
                            //TypeClass
                            var adapterGlobal = ReplyCommentActivity.GetInstance()?.MAdapter;
                            var dataGlobal = adapterGlobal?.ReplyCommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            {
                                dataGlobal.Text = strName;
                                var index = adapterGlobal.ReplyCommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    adapterGlobal.NotifyItemChanged(index);
                                }
                            }

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.EditCommentAsync(CommentObject.Id, strName, "edit_reply") });
                        } 
                    }
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