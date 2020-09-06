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
    //We can have multiple versions installed either a local and a steam version
    //Or multiple local


    public class ModInfo: ListElement
    {

        public List<ModVersionInfo> versions = new List<ModVersionInfo>();
        public string author { get; private set; }
        public string id { get; private set; }


        /// <summary>
        /// Shows if any of the versions are enabled
        /// </summary>
        public bool Active => versions.Any(v => v.active);

        /// <summary>
        /// The active version of this mod
        /// </summary>
        public ModVersionInfo ActiveVersion => versions.Find(v => v.active);

        /// <summary>
        /// Returns the active version info (or first in the list if none active).
        /// </summary>
        public ModVersionInfo PreviewVersionInfo => (Active) ? versions.Find(m => m.active) : versions[0];

        /// <summary>
        /// The description for this mod
        /// </summary>
        /// It is the description of the prewiew mod
        public string Description => PreviewVersionInfo.ModMeta.Description;




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

            //this.active = versions.Any(v => v.active);
        }

        public override bool Equals(object obj)
        {
            if(obj is ModInfo other)
            {
                return other.id == this.id;
            }
            return false;
        }

        public override void SetActive(bool act)
        {
            if (act)
                ActivateDefault();
            else if (ActiveVersion != null)
                ActiveVersion.active = false;
        }

        public override bool GetActive() => Active;


        public void ActivateDefault()
        {
            var currentActiveVersion = versions.Find(v => v.active);
            if (currentActiveVersion == null)
            {
                //We activate the steam version by default or the first one if there is no steam version
                var nextActiveVersion = versions.Find(v => v.source == ModSource.Steam);
                if (nextActiveVersion == null)
                    nextActiveVersion = versions[0];

                nextActiveVersion.active = true;
            }
        }

        public void SetActiveVersion(bool status, int i)
        {
            var currentActiveVersion = versions.Find(v => v.active);
            if(currentActiveVersion != null)
                currentActiveVersion.active = false;

            versions[i].active = status;
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

            if (active)
            {
                Verse.Log.Message($"{data.Name} {data.Active} {data.RootDir.FullName}");
            }
        }
    }

    public enum ModSource
    {
        Folder,
        Steam
    }
}
