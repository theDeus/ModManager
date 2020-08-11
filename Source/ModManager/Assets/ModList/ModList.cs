using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ModList
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
    }
}
