using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using Android.Net;
using Android.Preferences;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/AddFriendLabel", MainLauncher = false)]
    public class AddFriendActivity : Activity
    {
        private Button mFriendAdd;
        private ListView mAddFriendList;

        private HttpClient client;
        private List<Traveler> mItems;
        private FriendListItem newFriendListItem;
        private long mItemId;

        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.AddFriend);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            mAddFriendList = FindViewById<ListView>(Resource.Id.addFriendListView);
            mFriendAdd = FindViewById<Button>(Resource.Id.friendAddButton);

            mItems = new List<Traveler>();

            var loadingMessage = ProgressDialog.Show(this, GetString(Resource.String.SearchingForFriendTitle),
                GetString(Resource.String.SearchingForFriendContent), true, false);
            var getContentFoundFriend = Intent.GetStringExtra("Content");
            mItems.Clear();
            var foundFriends = JsonConvert.DeserializeObject<List<Traveler>>(getContentFoundFriend);
            foreach (var foundFriend in foundFriends)
            {
                mItems.Add(new Traveler
                {
                    Id = foundFriend.Id,
                    Name = foundFriend.Name,
                    Surname = foundFriend.Surname,
                    DateOfBirth = foundFriend.DateOfBirth
                });
            }
            
            var adapter = new AddFriendListViewAdapter(this, mItems);
            mAddFriendList.Adapter = adapter;
            loadingMessage.Dismiss();

            mFriendAdd.Click += async (object sender, EventArgs e) =>
            {
                if (mItemId == 0)
                    Toast.MakeText(this, GetString(Resource.String.FoundFriendNotSelectedInfo), ToastLength.Long).Show();
                else
                {
                    var traveler = JsonConvert.DeserializeObject<Traveler>(preferences.GetString("traveler", null));
                    var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                    var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                    if (traveler == null)
                    {
                        Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long)
                            .Show();
                        Finish();
                        StartActivity(typeof(LoginActivity));
                    }
                    else if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                        Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long).Show();
                    else
                    {
                        newFriendListItem = new FriendListItem
                        {
                            OwnerId = traveler.Id,
                            FriendId = mItemId
                        };
                        var response = await AddFriendToFriendList(newFriendListItem);
                        if (response.IsSuccessStatusCode)
                        {
                            SetResult(Result.Ok);
                            Finish();
                        }
                        else if (response.StatusCode == HttpStatusCode.Conflict)
                        {
                            SetResult((Result) 409);
                            Finish();
                        }
                    }
                }
            };

            mAddFriendList.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                mItemId = mItems[e.Position].Id;
                Toast.MakeText(this, string.Format(GetString(Resource.String.FoundFriendSelectedInfo) + " {0}", mItems[e.Position].Name), ToastLength.Long).Show();
            };
        }

        private async Task<HttpResponseMessage> AddFriendToFriendList(FriendListItem newFriendListItem)
        {
            var token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));
            if (token == null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var url = GetString(Resource.String.ApiLink) + "/api/friendlist/addFriendListItem";
            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

            var json = JsonConvert.SerializeObject(newFriendListItem);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await client.PostAsync(new Uri(url), content);

            return response;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SetResult(Result.Canceled);
        }
    }
}