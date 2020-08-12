using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace ModManager.ModList
{
    [Serializable]
    public class ModList
    {
        public ModFolder root = new ModFolder("Root");

        //TODO: cash?
        public int ModCount => CountMods(this.root);

        public static int CountMods(ModFolder list)
        {
            int count = list.contents.Count;
            foreach (var element in list.contents)
            {
                if (element is ModFolder folder)
                    count += CountMods(folder);
            }

            return count;
        }

        public void Add(ListElement element)
        {
            root.contents.Add(element);
        }

        /// <summary>
        /// Called when reloading the mod list
        /// </summary>
        public void Clear()
        {
            root.contents.Clear();
        }

        public void Sort()
        {
            root.contents = root.contents
                .OrderBy(b => b.LoadOrder)
                .ToList();
        }

        public List<ModMetaData> ModMetaData => GetModMetaDataList(this.root);

        List<ModMetaData> GetModMetaDataList(ModFolder list)
        {
            List<ModMetaData> data = new List<ModMetaData>();
            foreach (var element in list.contents)
            {
                if (element is ModInfo mod)
                {
                    data.AddRange(mod.version.Select(m => m.ModMeta));
                }
                else if (element is ModFolder folder)
                {
                    GetModMetaDataList(folder);
                }
            }
            return data;
        }
    }
}
