using HarmonyLib;

using RimWorld.Planet;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModManager.ModList
{
    public class ModListManager
    {
        public static ModList AvailableMods = new ModList(false, "Available Mods");

        public static ModList ActiveMods = new ModList(true, "Active Mods");

        

        public static void LoadMods()
        {
            AvailableMods.Clear();
            ActiveMods.Clear();

            // create all the buttons
            foreach (var mods in ModLister.AllInstalledMods.GroupBy(m => m.PackageIdNonUnique))
            {
                ModInfo inf = new ModInfo(mods.ToList());
                if (inf.Active)
                {
                    ActiveMods.Add(inf);
                }
                else
                {
                    AvailableMods.Add(inf);
                }
            }

            AvailableMods.Sort();
            ActiveMods.Sort();

        }

        public static void FinalizeModLists()
        {
            List<ModMetaData> metaDatas = ActiveMods.ModMetaData;

            List<string> active = metaDatas.FindAll(m=>m.Active).Select(m => m.PackageId).ToList();

            string a = active.StringJoin(", ");

            Debug.Log(a);

            ModsConfig.SetActiveToList(active);
            
            ModsConfig.Save();
        }

        public static void ActivateModList(ModList list)
        {
            //Unload all current mods
            AvailableMods.Merge(ActiveMods);
            ActiveMods.root.Empty();

            //Set new mod list
            ActiveMods = list;

            //Remove in use mods from available list
            AvailableMods.Remove(ActiveMods);

            ActiveMods.SetActive(true);
        }

        public static ModList GetDefaultModList()
        {
            var list = new ModList(false, "Default");

            list.Add(new ModInfo(ModLister.AllInstalledMods.Where(i => i.PackageIdNonUnique == "brrainz.harmony").ToList()));
            list.Add(new ModInfo(ModLister.AllInstalledMods.Where(i => i.PackageIdNonUnique == "ludeon.rimworld").ToList()));
            list.Add(new ModInfo(ModLister.AllInstalledMods.Where(i => i.PackageIdNonUnique == "fluffy.modmanager").ToList()));

            return list;
        }

        public static void CleanActiveModsList()
        {
            ActivateModList(GetDefaultModList());
        }


    }
}
