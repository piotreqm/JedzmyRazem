using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace InzApp
{
    class MyInformationsListViewAdapter : BaseAdapter<Information>
    {
        public List<Information> mItems;
        private Context mContext;

        public MyInformationsListViewAdapter(Context context, List<Information> items)
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

        public override Information this[int position]
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
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.MyMessageListViewRow, null);
            }

            TextView txtTime = row.FindViewById<TextView>(Resource.Id.timeTextView);
            txtTime.Text = mItems[position].Time.ToString("HH:mm");

            TextView txtStart = row.FindViewById<TextView>(Resource.Id.startPlaceTextView);
            txtStart.Text = mItems[position].StartPlace;

            TextView txtEnd = row.FindViewById<TextView>(Resource.Id.endPlaceTextView);
            txtEnd.Text = mItems[position].EndPlace;

            return row;
        }

    }
}