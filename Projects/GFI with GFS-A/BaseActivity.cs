using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "BaseActivity")]
    public class BaseAppCompatActivity : AppCompatActivity
    {
        protected override void AttachBaseContext(Context @base)
        {
            if (!ETC.hasBasicInit)
                ETC.BasicInitializeApp(@base);

            base.AttachBaseContext(ETC.baseContext);
        }
    }

    public class BaseFragmentActivity : FragmentActivity
    {
        protected override void AttachBaseContext(Context @base)
        {
            if (!ETC.hasBasicInit)
                ETC.BasicInitializeApp(@base);

            base.AttachBaseContext(ETC.baseContext);
        }
    }

    public class BaseActivity : Activity
    {
        protected override void AttachBaseContext(Context @base)
        {
            if (!ETC.hasBasicInit)
                ETC.BasicInitializeApp(@base);

            base.AttachBaseContext(ETC.baseContext);
        }
    }
}