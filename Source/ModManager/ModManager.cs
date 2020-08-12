//#define DEBUG_PROFILE

using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ModManager
{
    public class ModManager: Mod
    {
        public ModManager( ModContentPack content ) : base( content )
        {
            Instance = this;
            var harmonyInstance = new Harmony( "fluffy.modmanager" );

#if DEBUG
            Harmony.DEBUG = true;
#endif
            harmonyInstance.PatchAll( Assembly.GetExecutingAssembly() );

#if DEBUG_PROFILE
            LongEventHandler.ExecuteWhenFinished( () => new Profiler( typeof( Page_BetterModConfig ).GetMethod(
                                                                          nameof( Page_BetterModConfig.DoWindowContents
                                                                          ) ) ) );
#endif
        }

        public static ModManager Instance { get; private set; }

        public override string SettingsCategory() => I18n.SettingsCategory;

        public override void DoSettingsWindowContents( Rect canvas )
        {
            base.DoSettingsWindowContents( canvas );

        }
    }
}