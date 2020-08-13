#nullable enable

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Assertions.Must;

using Verse;

namespace ModManager.ModList
{
    [Serializable]
    public class ModInfo: ListElement
    {
        //We can have multiple versions installed either a local and a steam
        //Or multiple local
        public List<ModVersionInfo> versions = new List<ModVersionInfo>();
        public string author { get; private set; }
        public string id { get; private set; }

        public bool active { get; private set; }

        public ModInfo(string name, string author, int order): base(order)
        {
            this._name = name;
            this.author = author;
        }

        public ModInfo(List<ModMetaData> data)
        {
            if (data.NullOrEmpty())
                return;

            ModMetaData modMeta = data.Find(m => m.Active);
            if (modMeta == null)
            {
                modMeta = data[0];
            }

            _name = modMeta.Name;
            author = modMeta.Author;
            id = modMeta.PackageIdNonUnique;

            LoadOrder = modMeta.LoadOrder();

            
            foreach (var ver in data)
            {
                versions.Add(new ModVersionInfo(ver));
            }

            this.active = versions.Any(v => v.active);
        }

        public void SetActive(bool status)
        {
            ModVersionInfo activeVersion = versions.Find(v => v.active);
            
            if (status && activeVersion == null)
            {
                //We activate the steam version by default or the first one if there is no steam version
                activeVersion = versions.Find(v => v.source == ModSource.Steam);
                if (activeVersion != null)
                    activeVersion = versions[0];

                activeVersion!.active = true;
            }
            else
            {
                if (activeVersion != null)
                    activeVersion.active = false;
            }
        }
    }

    public class ModVersionInfo
    {
        public ModMetaData ModMeta;

        public string version;
        public string path;
        public string targetGameVersion;

        public ModSource source;

        public bool active
        {
            set
            {
                ModMeta.Active = value;
            }
            get
            {
                return ModMeta.Active;
            }
        }

        public ModVersionInfo(ModMetaData data)
        {
            ModMeta = data;
            version = "Read from manifest I guess";
            path = data.RootDir.FullName;
            targetGameVersion = data.TargetVersion;

            source = data.OnSteamWorkshop ? ModSource.Steam : ModSource.Folder;
        }
    }

    public enum ModSource
    {
        Folder,
        Steam
    }
}
