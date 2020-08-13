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

        public bool activeity { get; private set; }

        public ModList(bool active)
        {
            root.modList = this;
            this.activeity = active;
        }

        //TODO: cash?
        public int ModCount => CountMods(this.root);

        public static int CountMods(ModFolder list)
        {
            int count = list.Contents.Count;
            foreach (var element in list.Contents)
            {
                if (element is ModFolder folder)
                    count += CountMods(folder);
            }

            return count;
        }

        public void Add(ListElement element)
        {
            element.modList = this;

            //root.Contents.Add(element);
            root.Add(-1, element);
            if (element is ModInfo mod && activeity)
            {
                mod.ActivateDefault();
            }
        }

        /// <summary>
        /// Called when reloading the mod list
        /// </summary>
        public void Clear()
        {
            root.Clear();
        }

        public void Sort()
        {
            root.Sort();
        }

        public List<ModMetaData> ModMetaData => GetModMetaDataList(this.root);

        List<ModMetaData> GetModMetaDataList(ModFolder list)
        {
            List<ModMetaData> data = new List<ModMetaData>();
            foreach (var element in list.Contents)
            {
                if (element is ModInfo mod)
                {
                    data.AddRange(mod.versions.Select(m => m.ModMeta));
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
