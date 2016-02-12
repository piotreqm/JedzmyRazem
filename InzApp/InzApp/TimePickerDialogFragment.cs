using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace InzApp
{
    public class TimePickerDialogFragment : DialogFragment
    {
        private readonly Context mContext;
        private DateTime mTime;
        private readonly TimePickerDialog.IOnTimeSetListener mListener;

        public TimePickerDialogFragment(Context context, DateTime time, TimePickerDialog.IOnTimeSetListener listener)
        {
            mContext = context;
            mTime = time;
            mListener = listener;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return new TimePickerDialog(mContext, mListener, mTime.Hour, mTime.Minute, true);
        }
    }
}