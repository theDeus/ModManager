using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.ModList
{
    [Serializable]
    public class ListElement
    {
        protected string _name;
        public string Name => _name;

        public int LoadOrder = 0;

        public ModList modList; 

        public ListElement(int order = 0)
        {
            LoadOrder = order;
        }

        public virtual bool Filter(ModFilter filter)
        {
            if (String.IsNullOrEmpty(filter.filter))
                return true;

            return Name.ToLower().Contains(filter.filter.ToLower());
        }
    }
}
