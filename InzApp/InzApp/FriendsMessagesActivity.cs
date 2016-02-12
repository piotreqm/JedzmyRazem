using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Android.Content;
using Android.Net;
using Android.Preferences;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/FriendsMessagesLabel", MainLauncher = false)]
    public class FriendsMessagesActivity : Activity
    {
        private ListView mMyFriendsList;
        private List<FriendListItem> mItems;
        private HttpClient client;

        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FriendsMessages);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            
            mMyFriendsList = FindViewById<ListView>(Resource.Id.myFriendMessagesListView);
            mItems = new List<FriendListItem>();
             
            GetMyFriends();

            mMyFriendsList.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                var myFriendMessagesActivityIntent = new Intent(this, typeof(MyFriendMessagesActivity));
                var putExtraContent = JsonConvert.SerializeObject(mItems[e.Position].Friend);
                myFriendMessagesActivityIntent.PutExtra("Friend", putExtraContent);
                StartActivity(myFriendMessagesActivityIntent);
            };
        }

        private async void GetMyFriends()
        {
            var traveler = JsonConvert.DeserializeObject<Traveler>(preferences.GetString("traveler", null));
            var token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));
            var connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (traveler == null || token == null)
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
                                        + "/api/friendlist/getfriendlist?ownerId={0}", traveler.Id);
                client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                var loadingMessage = ProgressDialog.Show(this, GetString(Resource.String.LoadingFriendListTitle),
                    GetString(Resource.String.LoadingFriendListContent), true, false);
                var response = await client.GetAsync(new Uri(url));
                loadingMessage.Dismiss();

                mItems.Clear();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var friendListItems = JsonConvert.DeserializeObject<List<FriendListItem>>(content);
                    foreach (var friendListItem in friendListItems)
                    {
                        mItems.Add(new FriendListItem
                        {
                            Id = friendListItem.Id,
                            OwnerId = friendListItem.OwnerId,
                            FriendId = friendListItem.FriendId,
                            Owner = friendListItem.Owner,
                            Friend = friendListItem.Friend
                        });
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                    Toast.MakeText(this, GetString(Resource.String.LoadingFriendListEmptyInfo), ToastLength.Long).Show();
                else
                    Toast.MakeText(this, GetString(Resource.String.LoadingFriendListNotOkInfo), ToastLength.Long).Show();

                var adapter = new MyFriendsListViewAdapter(this, mItems);
                mMyFriendsList.Adapter = adapter;
            }
        }
    }
}