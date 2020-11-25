//##############################################
//Cᴏᴘʏʀɪɢʜᴛ 2020 DᴏᴜɢʜᴏᴜᴢLɪɢʜᴛ Codecanyon Item 19703216
//Elin Doughouz >> https://www.facebook.com/Elindoughouz
//====================================================

//For the accuracy of the icon and logo, please use this website " https://appicon.co " and add images according to size in folders " mipmap " 

using WoWonder.Helpers.Model;

namespace WoWonder
{
    public static class AppSettings
    {
        /// <summary>
        /// Deep Links to App Content
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">demo.wowonder.com</string>
        /// </summary>
        //public static string TripleDesAppServiceProvider = "9mzWRy0lKsdQ8KBodhFGRNoO79lrpNIa/f4QPuKbDLUN4aWITMOkD2gaI4YbfQwBIUC3NpaxS42nyCO5GGEo4lgYpja4Yy0ycRpjkU+kP7DG2MXPjgYOu4qci4RFQgFBmy/ku9jFHIjoHY+w02Fx/zglV2qL9/bdQEiEoa79j6YtZdQmc/9leQB1YdjPvLp4pQO51UQd2pDNr7BCfyiGuZxaXB7VygzsiMbWlhoiSnnT7mktzp9VYBh2C+HsBBmFeITI+7vbeLyWQQROIREsnw==";
        public static string TripleDesAppServiceProvider = "9mzWRy0lKsdQ8KBodhFGRBP3ApEgBh27ImTl4vpklw9d/bxfcgiAr/p4Y71m3tpb7lWvo1d3gmIfx3u+EA1Vfq7Ler8BRQTyhgC4lRs7khQ5W4SJN1/KAfZXcGUm4+qZ1f0uc29B2mCzHKpfcFi0cpNQJ1f0u6F0a8Y/Jm2tKUYoKytbRe2XDY4OwTMSUA8Dv4zsDSX+fKSw/XUIqbGav0gdGLJVPAgyrkDed7e+pGzOPb1Uau2oHguJFTHbnYph2tnke97ybQnxV4WNqUrO5AG1u51eTW0sn80kMlIGwJJaeashkBvboEobIGyRIYtV7DJN/D9KccaxjQ0adypv7Q==";

        //Main Settings >>>>>
        //*********************************************************
        public static string Version = "2.3";
        public static string ApplicationName = "Based On Rate";
        public static string DatabaseName = "WowonderSocial";//New
        public static string YoutubeKey = "AIzaSyA-JSf9CU1cdMpgzROCCUpl4wOve9S94ZU";

        // Friend system = 0 , follow system = 1
        public static int ConnectivitySystem = 1;

        public static PostButtonSystem PostButton = PostButtonSystem.ReactionDefault;
        public static bool ShowTextShareButton = true;
        public static bool ShowShareButton = true; 

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#a84849";
        public static string StoryReadColor = "#808080";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar

        //Set Language User on site from phone 
        public static bool SetLangUser = true; 

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "64974c58-9993-40ed-b782-0814edc401ea";
        public static string MsgOneSignalAppId = "64974c58-9993-40ed-b782-0814edc401ea";

        // WalkThrough Settings >>
        //*********************************************************
        public static bool ShowWalkTroutPage = true;
        public static bool WalkThroughSetFlowAnimation = true;
        public static bool WalkThroughSetZoomAnimation = false;
        public static bool WalkThroughSetSlideOverAnimation = false;
        public static bool WalkThroughSetDepthAnimation = false;
        public static bool WalkThroughSetFadeAnimation = false;

        //Main Messenger settings
        //*********************************************************
        public static bool MessengerIntegration = true;
        public static string MessengerPackageName = "com.wowondermessenger.app"; //APK name on Google Play

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowAdMobBanner = false;
        public static bool ShowAdMobInterstitial = false;
        public static bool ShowAdMobRewardVideo = false;
        public static bool ShowAdMobNative = false;
        public static bool ShowAdMobNativePost = false;

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/3276797899";
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/8193070896";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/4917290429";

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3;
        public static int ShowAdMobRewardedVideoCount = 3;
        public static int ShowAdMobNativeCount = 40;

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false;  
        public static bool ShowFbRewardVideoAds = false; 
        public static bool ShowFbNativeAds = false; 
         
        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828";  
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492"; 
        public static string AdsFbNativeKey = "250485588986218_554706301897477"; 

        //Three times after entering the ad is displayed
        public static int ShowFbNativeAdsCount = 40;  
         
        //********************************************************* 
        public static bool EnableRegisterSystem = true;  
        public static bool ShowGenderOnRegister = true;

        //Set Theme Welcome Pages 
        //*********************************************************
        //Types >> Gradient or Video or Image
        /// <summary> 
        /// if Gradient you can change color from  ../values/colors.xml
        /// <color name="background_First_Layout_startColor">#777777</color>
        /// <color name="background_First_Layout_centerColor">#888888</color>
        /// <color name="background_First_Layout_endColor">#444444</color>
        /// </summary>
        public static string BackgroundScreenWelcomeType = "Image";

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;
         

        //Code Time Zone (true => Get from Internet , false => Get From #CodeTimeZone )
        //*********************************************************
        public static bool AutoCodeTimeZone = true;
        public static string CodeTimeZone = "UTC";

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file 
        //Facebook >> ../values/analytic.xml .. line 10-11 
        //Google >> ../values/analytic.xml .. line 15 
        //*********************************************************
        public static bool ShowFacebookLogin = false;
        public static bool ShowGoogleLogin = false;

        public static readonly string ClientId = "212666549038-1rh786dicdq3kknstpf7o89u31urssj4.apps.googleusercontent.com";

        //########################### 

        public static bool ShowTrendingPage = true;  
          
        //Main Slider settings
        //*********************************************************
        public static bool ShowAlbum = true;
        public static bool ShowArticles = true;
        public static bool ShowPokes = true;
        public static bool ShowCommunitiesGroups = true;
        public static bool ShowCommunitiesPages = true;
        public static bool ShowMarket = true;
        public static bool ShowPopularPosts = true;
        public static bool ShowMovies = true;
        public static bool ShowNearBy = true;
        public static bool ShowStory = true;
        public static bool ShowSavedPost = true;
        public static bool ShowUserContacts = true; 
        public static bool ShowJobs = true; 
        public static bool ShowCommonThings = true; 
        public static bool ShowFundings = true;
        public static bool ShowMyPhoto = true; 
        public static bool ShowMyVideo = true; 
        public static bool ShowGames = true;
        public static bool ShowMemories = true;  
        public static bool ShowOffers = true;  
        public static bool ShowNearbyShops = true;   

        public static bool ShowSuggestedGroup = true;
        public static bool ShowSuggestedUser = true;
         
        //count times after entering the Suggestion is displayed
        public static int ShowSuggestedGroupCount = 70; 
        public static int ShowSuggestedUserCount = 50;

        //allow download or not when share
        public static bool AllowDownloadMedia = true; 

        public static bool ShowAdvertise = false; //New >> Next Version

        public static bool ShowLive = true;   //New
        public static string AppIdAgoraLive = "c55b9bda665042809b61dfeb3f3832e0"; //#New

        //Events settings
        //*********************************************************  
        public static bool ShowEvents = true; 
        public static bool ShowEventGoing = true; 
        public static bool ShowEventInvited = true;  
        public static bool ShowEventInterested = true;  
        public static bool ShowEventPast = true; 

        //Set a story duration >> 7 Sec
        public static long StoryDuration = 7000L;
        //*********************************************************
        /// <summary>
        ///  Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false;
        public static readonly string CurrencyIconStatic = "$";
        public static readonly string CurrencyCodeStatic = "USD";
        public static readonly string CurrencyFundingPriceStatic = "$";

        //Profile settings
        //*********************************************************
        public static bool ShowGift = true;
        public static bool ShowWallet = true; 
        public static bool ShowGoPro = true;  
        public static bool ShowWithdrawals = true;

        /// <summary>
        /// The default value comes from the site .. in case it is not available, it is taken from these values
        /// </summary>
        public static string WeeklyPrice = "3"; 
        public static string MonthlyPrice = "8"; 
        public static string YearlyPrice = "89"; 
        public static string LifetimePrice = "259"; 

        //Native Post settings
        //*********************************************************
        public static int AvatarPostSize = 60;
        public static int ImagePostSize = 200;
        public static string PostApiLimitOnScroll = "22";

        //Get post in background >> 1 Min = 60 Sec
        public static long RefreshPostSeconds = 60000;  
        public static string PostApiLimitOnBackground = "12"; 

        public static bool AutoPlayVideo = true;
         
        public static bool EmbedPlayTubePostType = true;
        public static bool EmbedDeepSoundPostType = true;
        public static VideoPostTypeSystem EmbedFacebookVideoPostType = VideoPostTypeSystem.Link; 
        public static VideoPostTypeSystem EmbedVimeoVideoPostType = VideoPostTypeSystem.Link; 
        public static VideoPostTypeSystem EmbedPlayTubeVideoPostType = VideoPostTypeSystem.Link; 
        public static VideoPostTypeSystem EmbedTikTokVideoPostType = VideoPostTypeSystem.Link; 
        public static bool ShowSearchForPosts = true; 
        public static bool EmbedLivePostType = true;
         
        //new posts users have to scroll back to top
        public static bool ShowNewPostOnNewsFeed = true; 
        public static bool ShowAddPostOnNewsFeed = false; 
        public static bool ShowCountSharePost = true; 

        /// <summary>
        /// Post Privacy
        /// ShowPostPrivacyForAllUser = true : all posts user have icon Privacy 
        /// ShowPostPrivacyForAllUser = false : just my posts have icon Privacy (default)
        /// </summary>
        public static bool ShowPostPrivacyForAllUser = false; 

        public static bool ShowFullScreenVideoPost = true;
         
        //UsersPages
        public static bool ShowProUsersMembers = true;
        public static bool ShowPromotedPages = true;
        public static bool ShowTrendingHashTags = true;
        public static bool ShowLastActivities = true;

        public static bool ShowUserPoint = true;

        //Add Post
        public static bool ShowGalleryImage = true;
        public static bool ShowGalleryVideo = true;
        public static bool ShowMention = true;
        public static bool ShowLocation = true;
        public static bool ShowFeelingActivity = true;
        public static bool ShowFeeling = true;
        public static bool ShowListening = true;
        public static bool ShowPlaying = true;
        public static bool ShowWatching = true;
        public static bool ShowTraveling = true;
        public static bool ShowGif = true;
        public static bool ShowFile = true;
        public static bool ShowMusic = true;
        public static bool ShowPolls = true;
        public static bool ShowColor = true;

        public static bool ShowAnonymousPrivacyPost = true; 

        //Boost 
        public static bool ShowAdvertisingPost = true;  

        //Settings Page >> General Account
        public static bool ShowSettingsGeneralAccount = true;
        public static bool ShowSettingsAccount = true;
        public static bool ShowSettingsSocialLinks = true;
        public static bool ShowSettingsPassword = true;
        public static bool ShowSettingsBlockedUsers = true;
        public static bool ShowSettingsDeleteAccount = true;
        public static bool ShowSettingsTwoFactor = true; 
        public static bool ShowSettingsManageSessions = true;  
        public static bool ShowSettingsVerification = true;
         
        public static bool ShowSettingsSocialLinksFacebook = true; //#New
        public static bool ShowSettingsSocialLinksTwitter = true; //#New
        public static bool ShowSettingsSocialLinksGoogle = true; //#New
        public static bool ShowSettingsSocialLinksVkontakte = true; //#New
        public static bool ShowSettingsSocialLinksLinkedin = true; //#New
        public static bool ShowSettingsSocialLinksInstagram = true; //#New
        public static bool ShowSettingsSocialLinksYouTube = true; //#New

        //Settings Page >> Privacy
        public static bool ShowSettingsPrivacy = true;
        public static bool ShowSettingsNotification = true;

        //Settings Page >> Tell a Friends (Earnings)
        public static bool ShowSettingsInviteFriends = true;

        public static bool ShowSettingsShare = true;
        public static bool ShowSettingsMyAffiliates = true;
         
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// Just replace it with this 5 lines of code
        /// <uses-permission android:name="android.permission.READ_CONTACTS" />
        /// <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" />
        /// <uses-permission android:name="android.permission.SEND_SMS" />
        /// </summary>
        public static bool InvitationSystem = false;

        public static int LimitGoProPlansCountsTo = 4;

        //Settings Page >> Help && Support
        public static bool ShowSettingsHelpSupport = true;

        public static bool ShowSettingsHelp = true;
        public static bool ShowSettingsReportProblem = true;
        public static bool ShowSettingsAbout = true;
        public static bool ShowSettingsPrivacyPolicy = true;
        public static bool ShowSettingsTermsOfUse = true;

        public static bool ShowSettingsRateApp = true; 
        public static int ShowRateAppCount = 5; 
         
        public static bool ShowSettingsUpdateManagerApp = false; 

        public static bool ShowSettingsInvitationLinks = true; 
        public static bool ShowSettingsMyInformation = true; 

        public static bool ShowSuggestedUsersOnRegister = true;

        public static bool ImageCropping = true;

        //Set Theme Tab
        //*********************************************************
        public static bool SetTabDarkTheme = false;
        public static MoreTheme MoreTheme = MoreTheme.BeautyTheme; 
        public static bool SetTabOnButton = true;

        //Bypass Web Errors  
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static bool RenderPriorityFastPostLoad = false;

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = false; 

        public static bool ShowPaypal = true; 
        public static bool ShowBankTransfer = true; 
        public static bool ShowCreditCard = true;

        //********************************************************* 
        public static bool ShowCashFree = true;  

        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static string CashFreeCurrency = "INR";  

        //********************************************************* 

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 24 
        /// </summary>
        public static bool ShowRazorPay = false; 

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static string RazorPayCurrency = "USD";  
         
        public static bool ShowPayStack = true;  
        public static bool ShowPaySera = false;  //#Next Version  
        //********************************************************* 



        public static SystemApiGetLastChat LastChatSystem = SystemApiGetLastChat.Old; //#New 

        // Stickers Packs Settings >>
        //*********************************************************
        public static int StickersOnEachRow = 3;
        public static string StickersBarColor = "#efefef";
        public static string StickersBarColorDark = "#282828";

        public static bool ShowStickerStack0 = true;
        public static bool ShowStickerStack1 = true;
        public static bool ShowStickerStack2 = true;
        public static bool ShowStickerStack3 = true;
        public static bool ShowStickerStack4 = true;
        public static bool ShowStickerStack5 = true;
        public static bool ShowStickerStack6 = false;

        // Video/Audio Call Settings >>
        //*********************************************************
        public static bool EnableAudioVideoCall = true;

        public static bool EnableAudioCall = true;
        public static bool EnableVideoCall = true;

        public static SystemCall UseLibrary = SystemCall.Twilio; //#New




        //ChatWindow_Activity >>
        //*********************************************************
        //if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        //Just replace it with this 5 lines of code
        /*
         <uses-permission android:name="android.permission.READ_CONTACTS" />
         <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" /> 
         <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" /> 
         <uses-permission android:name="android.permission.GET_ACCOUNTS" />
         <uses-permission android:name="android.permission.SEND_SMS" />
         */
        public static bool ShowButtonContact = false;
        //public static bool InvitationSystem = false;  //Invite friends section 
        /////////////////////////////////////

        public static bool ShowButtonCamera = true;
        public static bool ShowButtonImage = true;
        public static bool ShowButtonVideo = true;
        public static bool ShowButtonAttachFile = true;
        public static bool ShowButtonColor = true;
        public static bool ShowButtonStickers = true;
        public static bool ShowButtonMusic = true;
        public static bool ShowButtonGif = true;
        public static bool ShowButtonLocation = true; //#New

        public static bool ShowMusicBar = false;
        public static bool OpenVideoFromApp = true;  //#New
        public static bool OpenImageFromApp = true; //#New

        //Tabbed_Main_Page >>
        //*********************************************************
        public static bool ShowTitleUsername = false;

        // Chat Group >>
        //********************************************************* 
        public static bool EnableChatGroup = true;
        public static bool EnableChatPage = false;

        //Record Sound Style & Text
        ///*********************************************************
        public static bool ShowButtonRecordSound = true;

        //Last_Messages Page >>
        ///*********************************************************
        public static bool ShowOnlineOfflineMessage = false;

        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds

        //Options chat heads (Bubbles) 
        //*********************************************************
        //Always , Hide , FullScreen
        public static string DisplayModeSettings = "Always";//#New

        //Default , Left  , Right , Nearest , Fix , Thrown
        public static string MoveDirectionSettings = "Right";//#New

        //Circle , Rectangle
        public static string ShapeSettings = "Circle";//#New

        // Last position
        public static bool IsUseLastPosition = true; //#New
    }
}