using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Runtime;
using Android.Widget;
using Android.OS;
using Android.Preferences;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true)]
    public class LoginActivity : Activity 
    {
        private EditText mEmailField;
        private EditText mPassField;
        private Button mLoginButton;
        private Button mRegisterButton;
        private Button mForgottenPwButton;

        private string email;
        private string password;
        private HttpClient client;
        private AuthenticationToken token;

        // Shared preferences file for storing bearer token for whole app
        private ISharedPreferences preferences;
        private ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);

            // Shared preferences file for storing bearer token for whole app - initialization:
            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            editor = preferences.Edit();

            mEmailField = base.FindViewById<EditText>(Resource.Id.emailField);
            mPassField = base.FindViewById<EditText>(Resource.Id.passField);
            mLoginButton = base.FindViewById<Button>(Resource.Id.loginButton);
            mRegisterButton = base.FindViewById<Button>(Resource.Id.registerButton);
            mForgottenPwButton = base.FindViewById<Button>(Resource.Id.forgottenPwButton);

            mLoginButton.Click += MLoginButton_Click;
            mRegisterButton.Click += MRegisterButton_Click;
            mForgottenPwButton.Click += MForgottenPwButton_Click;
            
        }

        private async void MLoginButton_Click(object sender, EventArgs e)
        {
            if (!mEmailField.Text.Equals("") && !mPassField.Text.Equals(""))
            {
                email = mEmailField.Text;
                password = mPassField.Text;

                var url = GetString(Resource.String.SecureApiLink) + "/token";
                client = new HttpClient();

                var loadingMessage = ProgressDialog.Show(this, GetString(Resource.String.LoginProgressTitle),
                    GetString(Resource.String.LoginProgressContent), true, false);
                var xFormData = $"grant_type=password&username={email}&password={password}";
                var content = new StringContent(xFormData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
                var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                {
                    loadingMessage.Dismiss();
                    Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo),ToastLength.Long).Show();
                    return;
                } 
                
                var response = await client.PostAsync(new Uri(url), content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonToken = JObject.Parse(responseContent);
                    token = new AuthenticationToken
                    {
                        AccessToken = jsonToken["access_token"].ToString(),
                        TokenType = jsonToken["token_type"].ToString(),
                        ExpiresIn = Convert.ToInt64(jsonToken["expires_in"]),
                        Expires = DateTime.UtcNow.AddSeconds(Convert.ToDouble(jsonToken["expires_in"])),
                        UserId = jsonToken["userId"].ToString()
                    };
                    editor.PutString("token", JsonConvert.SerializeObject(token));
                    editor.Apply();

                    url = string.Format(GetString(Resource.String.ApiLink)
                                        + "/api/traveler/gettravelerbyuserid?userid={0}", token.UserId);
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
                    response = await client.GetAsync(new Uri(url));
                    loadingMessage.Dismiss();
                    if (response.IsSuccessStatusCode)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                        editor.PutString("traveler", responseContent);
                        editor.Apply();
                        StartActivityForResult(typeof (MainActivity), 11);
                    }
                    else
                        Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long)
                            .Show();
                }
                else
                {
                    loadingMessage.Dismiss();
                    Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure),ToastLength.Long).Show();
                }
            }
            else Toast.MakeText(this, GetString(Resource.String.LoginFieldsNoDataInfo), ToastLength.Long).Show();
        }
        private void MRegisterButton_Click(object sender, EventArgs e)
        {
            StartActivityForResult(typeof(RegisterActivity), 4);
        }
        private void MForgottenPwButton_Click(object sender, EventArgs e)
        {
            StartActivityForResult(typeof(ForgottenPwActivity), 5);
        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();
            Finish();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case 4:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            FinishActivity(requestCode);
                            Toast.MakeText(this, GetString(Resource.String.RegisterUserOkInfo), ToastLength.Long).Show();
                            break;
                        case Result.Canceled:
                            FinishActivity(requestCode);
                            Toast.MakeText(this, GetString(Resource.String.RegisterUserCanceledInfo), ToastLength.Long).Show();
                            break;
                        default:
                            Toast.MakeText(this, GetString(Resource.String.RegisterUserNotOkInfo), ToastLength.Long).Show();
                            break;
                    }
                    break;
                case 5:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            FinishActivity(requestCode);
                            Toast.MakeText(this, GetString(Resource.String.ForgottenPwRequestOkInfo), ToastLength.Long).Show();
                            break;
                        case Result.Canceled:
                            FinishActivity(requestCode);
                            Toast.MakeText(this, GetString(Resource.String.ForgottenPwRequestCanceledInfo), ToastLength.Long).Show();
                            break;
                        default:
                            Toast.MakeText(this, GetString(Resource.String.ForgottenPwRequestNotOkInfo), ToastLength.Long).Show();
                            break;
                    }
                    break;
                case 11:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            FinishActivity(11);
                            Toast.MakeText(this, GetString(Resource.String.LogoutConfirmationLoggedOutMessage), ToastLength.Long).Show();
                            break;
                    }
                    break;
            }
        }
    }
}

