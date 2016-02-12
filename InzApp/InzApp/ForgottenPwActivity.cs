using System;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Text.RegularExpressions;
using System.Net.Http;
using Android.Net;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/ForgottenPwLabel", MainLauncher = false)]
    public class ForgottenPwActivity : Activity
    {
        private EditText mEmailField;
        private Button mRemindButton;

        private string mEmail;
        private HttpClient client;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ForgottenPw);

            mEmailField = FindViewById<EditText>(Resource.Id.forgottenPwEmailField);
            mRemindButton = FindViewById<Button>(Resource.Id.remindButton);

            mRemindButton.Click += async (object sender, EventArgs e) =>
            {
                if (!mEmailField.Text.Equals(""))
                {
                    if (Regex.Match(mEmailField.Text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success)
                    {
                        mEmail = mEmailField.Text;

                        var url = string.Format(GetString(Resource.String.ApiLink)
                            + "/api/account/forgotpassword?useremail={0}", mEmail);
                        client = new HttpClient();

                        var loadingMessage = ProgressDialog.Show(this,
                            GetString(Resource.String.SendingForgottenPwRequestTitle),
                            GetString(Resource.String.SendingForgottenPwRequestContent));

                        var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                        var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                        if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                        {
                            loadingMessage.Dismiss();
                            Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long).Show();
                            return;
                        }

                        var response = await client.GetAsync(new Uri(url));
                        loadingMessage.Dismiss();

                        if (response.IsSuccessStatusCode)
                            SetResult(Result.Ok);
                        Finish();
                    }
                    else Toast.MakeText(this, GetString(Resource.String.ForgottenPwEmailNotCorrectInfo), ToastLength.Long).Show();
                }
            };
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SetResult(Result.Canceled);
        }
    }
}