using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Net;
using Android.Preferences;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/MyMessagesLabel", MainLauncher = false)]
    public class MyMessagesActivity : Activity
    {
        private ImageButton mNewMsgButton;

        private List<Information> mItems;
        private ListView mMyMessageList;
        private AdapterView.AdapterContextMenuInfo info;
        private HttpClient client;

        private ISharedPreferences preferences;
        private AuthenticationToken token;

        private ConnectivityManager connectivityManager;
        private NetworkInfo activeNetworkInfo;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MyMessages);
            
            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));
            
            mNewMsgButton = FindViewById<ImageButton>(Resource.Id.newMsgImageButton);
            mMyMessageList = FindViewById<ListView>(Resource.Id.myMessagesListView);
            RegisterForContextMenu(mMyMessageList);
            mItems = new List<Information>();
            
            GetMyMessages();

            mNewMsgButton.Click += (object sender, EventArgs e) =>
            {
                StartActivityForResult(typeof(NewMessageActivity), 1);
            };

        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id != Resource.Id.myMessagesListView) return;

            info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            menu.SetHeaderTitle(GetString(Resource.String.ContextMenuTitle));
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
                    CallDeleteMessage();
                    return true;
                default:
                    return false;
            }
        }

        private async void CallDeleteMessage()
        {
            connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
            activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
            {
                Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo),ToastLength.Long).Show();
                return;
            }

            var apiMessage = await DeleteMessage(mItems[(int)info.Id].Id);
            if (apiMessage.IsSuccessStatusCode)
            {
                Toast.MakeText(this, GetString(Resource.String.MessageDeleteOkInfo), ToastLength.Long).Show();
                GetMyMessages();
            }
            else if (apiMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure),ToastLength.Long).Show();
                Finish();
                StartActivity(typeof(LoginActivity));
            }
            else Toast.MakeText(this, GetString(Resource.String.MessageDeleteNotOkInfo), ToastLength.Long).Show(); 
        }

        private async Task<HttpResponseMessage> DeleteMessage(long messageToDeleteId)
        {
            var url = string.Format(GetString(Resource.String.ApiLink)
                      + "/api/information/deleteinformation?messagetodeleteId={0}", messageToDeleteId);
            client = new HttpClient();
            if (token == null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
            
            var response = await client.DeleteAsync(new Uri(url));
            return response;
        }

        private async void GetMyMessages()
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
                                        + "/api/information/getinformation?travelerid={0}", traveler.Id);
                client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                var loadingMessage = ProgressDialog.Show(this, GetString(Resource.String.LoadingMessageTitle),
                    GetString(Resource.String.LoadingMessageContent), true, false);
                var response = await client.GetAsync(new Uri(url));
                loadingMessage.Dismiss();

                mItems.Clear();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var informations = JsonConvert.DeserializeObject<List<Information>>(content);
                    foreach (var information in informations)
                    {
                        mItems.Add(new Information
                        {
                            Id = information.Id,
                            TravelerId = information.TravelerId,
                            Time = information.Time,
                            StartPlace = information.StartPlace,
                            EndPlace = information.EndPlace
                        });
                    }
                }
                else
                    Toast.MakeText(this, GetString(Resource.String.LoadingMessageNotFoundInfo), ToastLength.Long).Show();

                var adapter = new MyInformationsListViewAdapter(this, mItems);
                mMyMessageList.Adapter = adapter;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode != 1) return;
            switch (resultCode)
            {
                case Result.Ok:
                    FinishActivity(1);
                    Toast.MakeText(this, GetString(Resource.String.NewMessageAddOkInfo), ToastLength.Long).Show();
                    GetMyMessages();
                    break;
                case Result.Canceled:
                    FinishActivity(1);
                    Toast.MakeText(this, GetString(Resource.String.NewMessageAddCanceledInfo), ToastLength.Long).Show();
                    break;
                default:
                    Toast.MakeText(this, GetString(Resource.String.NewMessageAddNotOkInfo), ToastLength.Long).Show();
                    break;
            }
        }
    }
}