using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Verse;

namespace ModManager
{
    static class ManagerUI
    {
        public const int SearchBarHeight = 20;

        static public void DrawModFilter(Rect rect, ModFilter filter)
        {
            Rect searchFiled = new Rect(rect);
            searchFiled.width -= (SearchBarHeight * 2) + 4;
            filter.filter = Widgets.TextField(searchFiled, filter.filter);

            Rect clearButton = new Rect(rect);
            clearButton.xMin = clearButton.xMax - SearchBarHeight;

            Color color = GetActiveColor(!string.IsNullOrEmpty(filter.filter));

            GUI.enabled = !string.IsNullOrEmpty(filter.filter);
            if (Widgets.ButtonImage(clearButton, Resources.Status_Cross, color))
            {
                filter.filter = "";
            }
            GUI.enabled = true;

            Rect showButton = new Rect(clearButton);
            showButton.x -= SearchBarHeight + 2;
            Texture2D texture = filter.HideNotMatching ? Resources.EyeClosed : Resources.EyeOpen;
            if (Widgets.ButtonImage(showButton, texture))
            {
                filter.HideNotMatching = !filter.HideNotMatching;
            }
        }

        static public Color GetActiveColor(bool state)
        {
            return state ? Color.white : Color.black;
        }

    }
}
