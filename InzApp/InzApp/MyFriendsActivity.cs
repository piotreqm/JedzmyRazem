using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.Preferences;
using Android.Runtime;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/MyFriendsLabel", MainLauncher = false)]
    public class MyFriendsActivity : Activity
    {
        private ImageButton mAddFriendButton;
        private ListView mMyFriendsList;
        private List<FriendListItem> mItems;
        private AdapterView.AdapterContextMenuInfo info;
        private HttpClient client;
        private HttpResponseMessage apiMessage;

        private ISharedPreferences preferences;
        private AuthenticationToken token;

        private ConnectivityManager connectivityManager;
        private NetworkInfo activeNetworkInfo;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MyFriends);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));

            mAddFriendButton = FindViewById<ImageButton>(Resource.Id.addFriendImageButton);
            mMyFriendsList = FindViewById<ListView>(Resource.Id.MyFriendsListView);
            mItems = new List<FriendListItem>();
            RegisterForContextMenu(mMyFriendsList);
            GetMyFriends();

            mAddFriendButton.Click += (object sender, EventArgs e) =>
            {
                StartActivityForResult(typeof(FindFriendToAddActivity), 2);
            };

        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id != Resource.Id.MyFriendsListView) return;

            info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            menu.SetHeaderTitle(Resource.String.ContextMenuTitle);
            var menuItems = Resources.GetStringArray(Resource.Array.ContextMenu);
            for (var i = 0; i < menuItems.Length; i++)
            {
                menu.Add(Menu.None, i, i, menuItems[i]);
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 0:
                    CallDeleteFriendFromFriendList();
                    return true;
                default:
                    return false;
            }
        }

        private async void CallDeleteFriendFromFriendList()
        {
            connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
            activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
            {
                Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long).Show();
                return;
            }

            apiMessage = await DeleteFriendFromFriendList(mItems[(int)info.Id].Id);
            if (apiMessage.IsSuccessStatusCode)
            {
                Toast.MakeText(this, GetString(Resource.String.FriendDeleteOkInfo), ToastLength.Long).Show();
                GetMyFriends();
            }
            else if (apiMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure),ToastLength.Long).Show();
                Finish();
                StartActivity(typeof(LoginActivity));
            }
            else Toast.MakeText(this, GetString(Resource.String.FriendDeleteNotOkInfo), ToastLength.Long).Show();
        }

        private async Task<HttpResponseMessage> DeleteFriendFromFriendList(long friendToDelete)
        {
            var url = string.Format(GetString(Resource.String.ApiLink) 
                + "/api/friendlist/deletefriendlistitem?friendlistitemid={0}", friendToDelete);
            client = new HttpClient();
            if (token == null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

            var response = await client.DeleteAsync(new Uri(url));
            return response;
        }

        private async void GetMyFriends()
        {
            var traveler = JsonConvert.DeserializeObject<Traveler>(preferences.GetString("traveler", null));
            connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
            activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (traveler == null || token == null)
            {
                Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long).Show();
                Finish();
                StartActivity(typeof(LoginActivity));
            }
            else if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo),ToastLength.Long).Show();
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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode != 2) return;
            switch (resultCode)
            {
                case Result.Ok:
                    FinishActivity(2);
                    Toast.MakeText(this, GetString(Resource.String.FriendAddOkInfo), ToastLength.Long).Show();
                    GetMyFriends();
                    break;
                case Result.Canceled:
                    FinishActivity(2);
                    Toast.MakeText(this, GetString(Resource.String.FriendAddCanceledInfo), ToastLength.Long).Show();
                    break;
                case (Result)409:
                    FinishActivity(2);
                    Toast.MakeText(this, GetString(Resource.String.FoundFriendAmbiguous), ToastLength.Long).Show();
                    GetMyFriends();
                    break;
                default:
                    Toast.MakeText(this, GetString(Resource.String.FriendAddNotOkInfo), ToastLength.Long).Show();
                    break;
            }
        }
    }
}