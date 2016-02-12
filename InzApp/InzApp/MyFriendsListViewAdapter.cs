using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using Android.Graphics;

namespace InzApp
{
    class MyFriendsListViewAdapter : BaseAdapter<FriendListItem>
    {
        public List<FriendListItem> mItems;
        private Context mContext;

        public MyFriendsListViewAdapter(Context context, List<FriendListItem> items)
        {
            mContext = context;
            mItems = items;
        }

        public override int Count
        {
            get
            {
                return mItems.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override FriendListItem this[int position]
        {
            get
            {
                return mItems[position];
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.MyFriendsListViewRow, null);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.nameTextView);
            txtName.Text = mItems[position].Friend.Name;
            
            TextView txtSurname = row.FindViewById<TextView>(Resource.Id.surnameTextView);
            txtSurname.Text = mItems[position].Friend.Surname;

            TextView txtDoB = row.FindViewById<TextView>(Resource.Id.dateOfBirthTextView);
            var DoB = mItems[position].Friend.DateOfBirth;
            if (DoB == new DateTime(DoB.Year, DateTime.Now.Month, DateTime.Now.Day))
            {
                txtDoB.SetTextColor(Color.Green);
                txtDoB.Text = DoB.ToString("d MMMM yyyy");
            }
            else txtDoB.Text = DoB.ToString("d MMMM yyyy");
            
            return row;
        }

    }
}