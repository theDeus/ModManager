using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ModManager
{
    class UIHelpers
    {

        public static Rect GetNextDown(Rect prew, float heigth, float space = 0)
        {
            prew.y += prew.height + space;
            prew.height = heigth;

            return prew;
        }

        public static Rect GetNextRight(Rect prew, float width, float space = 0)
        {
            prew.x += prew.width + space;
            prew.width = width;

            return prew;
        }

    }
}
