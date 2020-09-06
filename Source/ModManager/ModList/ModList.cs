using RimWorld;
using RimWorld.Planet;

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

        /// <summary>
        /// If the mods in this list are active
        /// </summary>
        /// 
        /// Weather this modlist is the active mods list.
        /// if so than mods added to this list wee be activated.
        public bool Active { get; private set; }

        public ModList(bool active, string name)
        {
            root.modList = this;
            this.Active = active;

            root.SetName(name);
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

        public void SetActive(bool state)
        {
            Active = state;

            foreach (var item in this.root.Contents)
            {
                item.SetActive(Active);
            }
        }

        public void Add(ListElement element)
        {
            element.modList = this;

            //root.Contents.Add(element);
            root.Add(-1, element);
            if (element is ModInfo mod && Active)
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

        /// <summary>
        /// Merges an other mod list into this.
        /// </summary>
        /// <param name="list">The list to merge</param>
        /// It matches the folders and merges them if available
        public void Merge(ModList list)
        {
            foreach (var item in list.root.Contents)
            {
                if (item is ModInfo mod)
                {
                    this.root.Add(-1, mod);
                }
                else if (item is ModFolder folder)
                {
                    ModFolder other_folder = (ModFolder)this.root.Contents.Where(i =>
                    {
                        return i is ModFolder fol
                            && fol.Name == folder.Name;
                    }).First();

                    if(other_folder!=null)
                    foreach (var c in folder.Contents)
                    {
                        other_folder.Add(-1, c);
                    }
                }
            }
        }

        public void Remove(ModList list)
        {
            foreach (var item in list.root.Contents)
            {
                this.root.Remove(item);
            }
        }

        public List<ModMetaData> ModMetaData => GetModMetaDataList(this.root);

        List<ModMetaData> GetModMetaDataList(ModFolder list)
        {
            List<ModMetaData> data = new List<ModMetaData>();
            foreach (var element in list.Contents)
            {
                if (element is ModInfo mod)
                {
                    data.Add(mod.ActiveVersion.ModMeta);
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
