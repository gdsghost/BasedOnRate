using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using RestSharp;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;

namespace WoWonder
{
    public class CustomAPI
    {
        public static async Task<(int, dynamic)> GetPostRatingAsync(string postId)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/get_post_ratings?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("post_id", postId);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> AddPostRatingAsync(string postId, string rating)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/add_post_rating?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("post_id", postId);
                request.AddParameter("rating", rating);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> DeletePostRatingAsync(string postId)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/delete_post_rating?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("post_id", postId);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        //Comment Rating
        public static async Task<(int, dynamic)> GetCommentRatingAsync(string commentId)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/get_comment_ratings?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("comment_id", commentId);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> AddCommentRatingAsync(string commentId, string rating)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/add_comment_rating?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("comment_id", commentId);
                request.AddParameter("rating", rating);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> DeleteCommentRatingAsync(string commentId)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/delete_comment_rating?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("comment_id", commentId);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        //Reply Ratings
        public static async Task<(int, dynamic)> GetReplyRatingAsync(string replyId)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/get_reply_ratings?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("reply_id", replyId);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> AddReplyRatingAsync(string replyId, string rating)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/add_reply_rating?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("reply_id", replyId);
                request.AddParameter("rating", rating);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> DeleteReplyRatingAsync(string replyId)
        {
            int api_status = 0;
            PostRatingObject result = new PostRatingObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/delete_reply_rating?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("reply_id", replyId);

                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRatingObject>(response.Content);
                api_status = result.Status;
                if (api_status != 200)
                {
                    return (api_status, JsonConvert.DeserializeObject<ErrorObject>(response.Content));
                }
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
        public static async Task<(int, dynamic)> GetPostRateUsersAsync(string postId, string type = "", string fetch = "")
        {
            int api_status = 0;
            PostRateUsersObject result = new PostRateUsersObject();
            try
            {
                var client = new RestClient(Client.WebsiteUrl + "/api/get_post_rate_users?access_token=" + Current.AccessToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("server_key", Client.ServerKey);
                request.AddParameter("type", type);
                request.AddParameter("id", postId);
                request.AddParameter("reaction", fetch);
                
                IRestResponse response = await client.ExecuteAsync(request);
                result = JsonConvert.DeserializeObject<PostRateUsersObject>(response.Content);
                api_status = result.Status;
            }
            catch (Exception ex)
            {
                //msg = "Something went wrong, please try again later";
            }
            return (api_status, result);
        }
    }

    public class PostRatingObject
    {
        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }
        [JsonProperty("user_rating", NullValueHandling = NullValueHandling.Ignore)]
        public string UserRating { get; set; }
        [JsonProperty("all_ratings", NullValueHandling = NullValueHandling.Ignore)]
        public List<PollsOptionObject> AllRatings { get; set; }

    }

    public class PostRateUsersObject
    {
        [JsonProperty("api_status")]
        public int Status { get; set; }

        [JsonProperty("rate_users_list")]
        public List<UserDataObject> RateUsersList { get; set; }
    }
}