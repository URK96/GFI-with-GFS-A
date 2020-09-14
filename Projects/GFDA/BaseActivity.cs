using Android.App;
using Android.Content;
using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;

namespace GFDA
{
    [Activity(Label = "BaseActivity")]
    public class BaseAppCompatActivity : AppCompatActivity
    {
        protected override void AttachBaseContext(Context @base)
        {
            if (!ETC.hasBasicInit)
            {
                ETC.BasicInitializeApp(@base);
            }

            base.AttachBaseContext(ETC.baseContext);
        }
    }

    public class BaseFragmentActivity : FragmentActivity
    {
        protected override void AttachBaseContext(Context @base)
        {
            if (!ETC.hasBasicInit)
            {
                ETC.BasicInitializeApp(@base);
            }

            base.AttachBaseContext(ETC.baseContext);
        }
    }

    public class BaseActivity : Activity
    {
        protected override void AttachBaseContext(Context @base)
        {
            if (!ETC.hasBasicInit)
            {
                ETC.BasicInitializeApp(@base);
            }

            base.AttachBaseContext(ETC.baseContext);
        }
    }
}