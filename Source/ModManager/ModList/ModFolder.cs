using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModManager.ModList
{
    [Serializable]
    public class ModFolder : ListElement
    {
        List<ListElement> _contents = new List<ListElement>();

        public ReadOnlyCollection<ListElement> Contents => _contents.AsReadOnly();

        public bool open = true;

        public ModFolder(string name)
        {
            this._name = name;
        }

        public override bool Filter(ModFilter filter)
        {
            if (base.Filter(filter))
                return true;

            foreach (var item in Contents)
            {
                if (item.Filter(filter))
                    return true;
            }

            return false;
        }

        public void Sort()
        {
            _contents = _contents
                .OrderBy(b => b.LoadOrder)
                .ToList();
        }

        public void Clear()
        {
            _contents.Clear();
        }

        /// <summary>
        /// Adds an element to the contents of this folder
        /// </summary>
        /// <param name="i">The index where we need to insert the element. If -1 it inserts at the end</param>
        /// <param name="element">The element we want to insert</param>
        public void Add(int i, ListElement element)
        {
            element.modList = this.modList;
            if (i == -1)
            {
                _contents.Add(element);
            }
            else
            {
                _contents.Insert(i, element);
            }
        }

        public void Remove(ListElement element)
        {
            _contents.Remove(element);
        }
    }
}
