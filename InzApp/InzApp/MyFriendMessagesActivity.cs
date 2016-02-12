using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Net;
using Android.Preferences;
using Newtonsoft.Json;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/FriendMessagesLabel", MainLauncher = false)]
    public class MyFriendMessagesActivity : Activity
    {
        private TextView mMyFriendMessagesTitle;
        private ListView mMyFriendMessages;

        private Traveler friend;
        private List<Information> mItems;
        private HttpClient client;

        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MyFriendMessages);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            mMyFriendMessagesTitle = FindViewById<TextView>(Resource.Id.MyFriendMessagesTitleTextView);
            mMyFriendMessages = FindViewById<ListView>(Resource.Id.myFriendMessagesListView);

            friend = JsonConvert.DeserializeObject<Traveler>(Intent.GetStringExtra("Friend"));
            if (friend.Id != 0)
            {
                mItems = new List<Information>();
                GetMyFriendMessages(friend.Id);
                mMyFriendMessagesTitle.Text =
                    string.Format(GetString(Resource.String.MyFriendMessagesTitle) + " {0} {1}", friend.Name, friend.Surname);
            }
            else Toast.MakeText(this, GetString(Resource.String.LoadingMessageNotOkInfo), ToastLength.Long).Show();            
        }

        private async void GetMyFriendMessages(long friendId)
        {
            var token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));
            var connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (token == null)
            {
                Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long).Show();
                Finish();
                StartActivity(typeof(LoginActivity));
            }
            else if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long).Show();
            else
            {
                var url = string.Format(GetString(Resource.String.ApiLink)
                                        + "/api/information/getinformation?travelerid={0}", friendId);
                client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                var loadingMessage = ProgressDialog.Show(this, GetString(Resource.String.LoadingMessageTitle),
                    GetString(Resource.String.LoadingMessageContent), true, false);
                var response = await client.GetAsync(new Uri(url));
                loadingMessage.Dismiss();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    mItems.Clear();
                    var friendInformations = JsonConvert.DeserializeObject<List<Information>>(content);
                    foreach (var friendInformation in friendInformations)
                    {
                        mItems.Add(new Information
                        {
                            Id = friendInformation.Id,
                            TravelerId = friendInformation.TravelerId,
                            Time = friendInformation.Time,
                            StartPlace = friendInformation.StartPlace,
                            EndPlace = friendInformation.EndPlace
                        });
                    }

                    var adapter = new MyInformationsListViewAdapter(this, mItems);
                    mMyFriendMessages.Adapter = adapter;
                }
                else
                    Toast.MakeText(this,
                        string.Format("{0} {1} " + GetString(Resource.String.LoadingFriendMessageNotFoundInfo),
                            friend.Name, friend.Surname), ToastLength.Long).Show();
            }
        }
    }
}