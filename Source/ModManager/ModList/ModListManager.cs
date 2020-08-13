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
        public static ModList AvailableMods = new ModList();

        public static ModList ActiveMods = new ModList();


        public static void LoadMods()
        {
            AvailableMods.Clear();
            ActiveMods.Clear();

            // create all the buttons
            foreach (var mods in ModLister.AllInstalledMods.GroupBy(m => m.PackageIdNonUnique))
            {
                ModInfo inf = new ModInfo(mods.ToList());
                if (inf.active)
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


            ModsConfig.SetActiveToList(active);

            //ModsConfig.Save();
        }
    }
}
