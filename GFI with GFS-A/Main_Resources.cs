using Android.Support.V7.App;

namespace GFI_with_GFS_A
{
    public partial class Main : AppCompatActivity
    {
        readonly int[] MainMenuButtonIds =
{
            Resource.Id.DBSubMenuMainButton,
            Resource.Id.OldGFDMainButton,
            Resource.Id.MDMainButton,
            Resource.Id.GFDInfoMainButton,
            Resource.Id.ExtraMainButton,
            Resource.Id.SettingMainButton
        };
        readonly int[] MainMenuButtonBackgroundIds =
        {
            Resource.Drawable.Main_DBMenuSelector,
            Resource.Drawable.Main_OldGFDSelector,
            Resource.Drawable.Main_MDSelector,
            Resource.Drawable.Main_GFDInfoSelector,
            Resource.Drawable.Main_ExtraSelector,
            Resource.Drawable.Main_SettingSelector
        };
        readonly int[] MainMenuButtonBackgroundIds_Orange =
        {
            Resource.Drawable.Main_DBMenuSelector_Orange,
            Resource.Drawable.Main_OldGFDSelector_Orange,
            Resource.Drawable.Main_MDSelector_Orange,
            Resource.Drawable.Main_GFDInfoSelector_Orange,
            Resource.Drawable.Main_ExtraSelector_Orange,
            Resource.Drawable.Main_SettingSelector_Orange
        };
        readonly string[] MainMenuButtonText =
        {
            ETC.Resources.GetString(Resource.String.Main_MainMenu_DBMenu),
            ETC.Resources.GetString(Resource.String.Main_MainMenu_OldGFD),
            ETC.Resources.GetString(Resource.String.Main_MainMenu_MDSupport),
            ETC.Resources.GetString(Resource.String.Main_MainMenu_GFDInfo),
            ETC.Resources.GetString(Resource.String.Main_MainMenu_Extras),
            ETC.Resources.GetString(Resource.String.Main_MainMenu_Setting)
        };

        readonly int[] DBSubMenuButtonIds =
        {
            Resource.Id.DollDBButton,
            Resource.Id.EquipDBButton,
            Resource.Id.FairyDBButton,
            Resource.Id.EnemyDBButton,
            Resource.Id.FSTDBButton
        };
        readonly int[] DBSubMenuButtonBackgroundIds =
        {
            Resource.Drawable.DBSub_DollDBSelector,
            Resource.Drawable.DBSub_EquipDBSelector,
            Resource.Drawable.DBSub_FairyDBSelector,
            Resource.Drawable.DBSub_EnemyDBSelector,
            Resource.Drawable.DBSub_FSTDBSelector
        };
        readonly int[] DBSubMenuButtonBackgroundIds_Orange =
        {
            Resource.Drawable.DBSub_DollDBSelector_Orange,
            Resource.Drawable.DBSub_EquipDBSelector_Orange,
            Resource.Drawable.DBSub_FairyDBSelector_Orange,
            Resource.Drawable.DBSub_EnemyDBSelector_Orange,
            Resource.Drawable.DBSub_FSTDBSelector_Orange
        };
        readonly string[] DBSubMenuButtonText =
        {
            ETC.Resources.GetString(Resource.String.Main_DBMenu_DollDB),
            ETC.Resources.GetString(Resource.String.Main_DBMenu_EquipDB),
            ETC.Resources.GetString(Resource.String.Main_DBMenu_FairyDB),
            ETC.Resources.GetString(Resource.String.Main_DBMenu_EnemyDB),
            ETC.Resources.GetString(Resource.String.Main_DBMenu_FSTDB)
        };

        readonly int[] ExtraMenuButtonIds =
        {
            Resource.Id.EventExtraButton,
            Resource.Id.GFNewsExtraButton,
            Resource.Id.CalcExtraButton,
            Resource.Id.AreaTipExtraButton,
            Resource.Id.GuideBookViewerExtraButton,
            Resource.Id.ProductSimulatorExtraButton,
            Resource.Id.StoryExtraButton,
            Resource.Id.CartoonExtraButton,
            Resource.Id.GFOSTPlayerExtraButton
        };
        readonly int[] ExtraMenuButtonBackgroundIds =
        {
            Resource.Drawable.Extra_EventSelector,
            Resource.Drawable.Extra_GFNewsSelector,
            Resource.Drawable.Extra_CalcSelector,
            Resource.Drawable.Extra_AreaTipSelector,
            Resource.Drawable.Extra_GuideBookSelector,
            Resource.Drawable.Extra_ProductSimulatorSelector,
            Resource.Drawable.Extra_StorySelector,
            Resource.Drawable.Extra_CartoonSelector,
            Resource.Drawable.Extra_OSTPlayerSelector
        };
        readonly int[] ExtraMenuButtonBackgroundIds_Orange =
        {
            Resource.Drawable.Extra_EventSelector_Orange,
            Resource.Drawable.Extra_GFNewsSelector_Orange,
            Resource.Drawable.Extra_CalcSelector_Orange,
            Resource.Drawable.Extra_AreaTipSelector_Orange,
            Resource.Drawable.Extra_GuideBookSelector_Orange,
            Resource.Drawable.Extra_ProductSimulatorSelector_Orange,
            Resource.Drawable.Extra_StorySelector_Orange,
            Resource.Drawable.Extra_CartoonSelector_Orange,
            Resource.Drawable.Extra_OSTPlayerSelector_Orange
        };
        readonly string[] ExtraMenuButtonText =
        {
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_Event),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_OfficialNotification),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_Calc),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_AreaTip),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_GuideBookViewer),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_ProductSimulator),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_Story),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_Cartoon),
            ETC.Resources.GetString(Resource.String.Main_ExtraMenu_GFOSTPlayer)
        };
    }
}