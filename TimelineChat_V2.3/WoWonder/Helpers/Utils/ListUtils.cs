using System;
using System.Collections.ObjectModel;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using WoWonderClient.Classes.Games;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;

namespace WoWonder.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************

        public static GetSiteSettingsObject.ConfigObject SettingsSiteList;

        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();

        public static ObservableCollection<UserDataObject> MyProfileList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<UserDataObject> MyFollowersList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<PageClass> MyPageList = new ObservableCollection<PageClass>();
        public static ObservableCollection<GroupClass> MyGroupList = new ObservableCollection<GroupClass>();
       
        public static ObservableCollection<Classes.Family> FamilyList = new ObservableCollection<Classes.Family>();
         
        public static ObservableCollection<PostDataObject> ListCachedDataAlbum = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<ArticleDataObject> ListCachedDataArticle = new ObservableCollection<ArticleDataObject>();
        public static ObservableCollection<ProductDataObject> ListCachedDataProduct = new ObservableCollection<ProductDataObject>();
        public static ObservableCollection<ProductDataObject> ListCachedDataMyProduct = new ObservableCollection<ProductDataObject>();
        public static ObservableCollection<GetMoviesObject.Movie> ListCachedDataMovie = new ObservableCollection<GetMoviesObject.Movie>();
        public static ObservableCollection<UserDataObject> ListCachedDataNearby = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<GiftObject.DataGiftObject> GiftsList = new ObservableCollection<GiftObject.DataGiftObject>();
        public static ObservableCollection<PostDataObject> ListCachedDataMyPhotos = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<PostDataObject> ListCachedDataMyVideos = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<GamesDataObject> ListCachedDataGames = new ObservableCollection<GamesDataObject>();
        public static ObservableCollection<GamesDataObject> ListCachedDataMyGames = new ObservableCollection<GamesDataObject>();
        public static ObservableCollection<GroupClass> SuggestedGroupList = new ObservableCollection<GroupClass>();
        public static ObservableCollection<UserDataObject> SuggestedUserList = new ObservableCollection<UserDataObject>();

        public static ObservableCollection<ChatObject> UserList = new ObservableCollection<ChatObject>();
        public static ObservableCollection<GetUsersListObject.User> UserChatList = new ObservableCollection<GetUsersListObject.User>();
        public static ObservableCollection<UserDataObject> FriendRequestsList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<GroupChatRequest> GroupRequestsList = new ObservableCollection<GroupChatRequest>();


        public static void ClearAllList()
        {
            try
            {
                DataUserLoginList.Clear(); 
                MyProfileList.Clear();
                MyFollowersList.Clear();
                MyPageList.Clear();
                MyGroupList.Clear(); 
                FamilyList.Clear();
                ListCachedDataAlbum.Clear();
                ListCachedDataArticle.Clear();
                ListCachedDataProduct.Clear();
                ListCachedDataMyProduct.Clear();
                ListCachedDataMovie.Clear();
                ListCachedDataNearby.Clear();
                GiftsList.Clear();
                ListCachedDataMyPhotos.Clear();
                ListCachedDataMyVideos.Clear();
                ListCachedDataGames.Clear();
                ListCachedDataMyGames.Clear();
                SuggestedGroupList.Clear();
                SuggestedUserList.Clear();

                UserList.Clear();
                UserChatList.Clear();
                FriendRequestsList.Clear();
                GroupRequestsList.Clear();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        //{
        //    try
        //    {
        //        items.ToList().ForEach(collection.Add);
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}


        //public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        //{
        //    var list = new List<List<T>>();

        //    for (int i = 0; i < locations.Count; i += nSize)
        //    {
        //        list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
        //    }

        //    return list;
        //}

        //public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        //{
        //    var enumerable = source as T[] ?? source.ToArray();

        //    return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        //}


        //public static T Cast<T>(object myobj)
        //{
        //    Type objectType = myobj.GetType();
        //    Type target = typeof(UserDataObject);
        //    object x = Activator.CreateInstance(target, false);

        //    var z = from source in objectType.GetMembers().ToList()
        //            where source.MemberType == MemberTypes.Property
        //            select source;

        //    var d = from source in target.GetMembers().ToList()
        //            where source.MemberType == MemberTypes.Property
        //            select source;

        //    List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name)).ToList();
        //    PropertyInfo propertyInfo;

        //    object value;
        //    foreach (var memberInfo in members)
        //    {
        //        propertyInfo = typeof(T).GetProperty(memberInfo.Name);
        //        value = myobj.GetType().GetProperty(memberInfo.Name)?.GetValue(myobj, null);

        //        if (propertyInfo != null)
        //            propertyInfo.SetValue(x, value, null);
        //    }
        //    return (T)x;
        //}


    }
}