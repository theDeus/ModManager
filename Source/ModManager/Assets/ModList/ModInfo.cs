using Assets.ModList;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ModList
{
    [Serializable]
    public class ModInfo: ListElement
    {
        //We can have multiple versions installed either a local and a steam
        //Or multiple local
        public List<ModVersionInfo> version = new List<ModVersionInfo>();
        public string author { get; private set; }
        public string id { get; private set; }
        public string targetGameVersion { get; private set; }
        

        public ModInfo(string name, string author)
        {
            this._name = name;
            this.author = author;
        }

        public void AddVersion(string ver, string path)
        {
            this.version.Add(new ModVersionInfo(ver, path));
        }
    }

    public struct ModVersionInfo
    {
        public string version;
        public string path;

        public ModVersionInfo(string ver, string p)
        {
            this.version = ver;
            this.path = p;
        }
    }
}
