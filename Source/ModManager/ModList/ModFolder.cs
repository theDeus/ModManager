using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.ModList
{
    [Serializable]
    public class ModFolder : ListElement
    {
        public List<ListElement> contents =  new List<ListElement>();

        public bool open = true;

        public ModFolder(string name)
        {
            this._name = name;
        }

        public override bool Filter(ModFilter filter)
        {
            if (base.Filter(filter))
                return true;

            foreach (var item in contents)
            {
                if (item.Filter(filter))
                    return true;
            }

            return false;
        }
    }
}
