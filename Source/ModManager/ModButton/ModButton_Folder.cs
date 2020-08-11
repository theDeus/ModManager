using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Verse;

using static ModManager.Constants;
using static ModManager.Resources;
using static ModManager.Utilities;

namespace ModManager
{
    class ModButton_Folder : ModButton
    {
        public ModButton_Folder() { }

        public ModButton_Folder(string name)
        {
            //Debug.Log($"ModButton_Downloading({pfid})");
            //_identifier = pfid;
            this._name = name;
        }

        private string _name;
        public override string Name => _name;
        public override string Identifier => _name;
        public override int SortOrder => 9;

        public override bool SamePackageId(string packageId)
        {
            return Identifier == packageId;
        }

        public override bool Active { get; set; }
        public override Color Color => Color.white;

        public override void DoModButton(Rect canvas, bool alternate = false, Action clickAction = null, Action doubleClickAction = null,
            bool deemphasizeFiltered = false, string filter = null)
        {

            base.DoModButton(canvas, alternate, clickAction, doubleClickAction, deemphasizeFiltered, filter);
            canvas = canvas.ContractedBy(SmallMargin / 2f);

            /**
             * NAME                   
             * progress
             */

            var nameRect = new Rect(
                canvas.xMin,
                canvas.yMin,
                canvas.width,
                canvas.height * 3 / 5f);
            var progressRect = new Rect(
                canvas.xMin,
                nameRect.yMax,
                canvas.width,
                canvas.height * 2 / 5f);

            Widgets.Label(nameRect, Name.Truncate(nameRect.width, _modNameTruncationCache));

            if (Mouse.IsOver(nameRect) && Name != Name.Truncate(nameRect.width, _modNameTruncationCache))
                TooltipHandler.TipRegion(nameRect, Name);

            ulong done;
            ulong total;
        }

        internal override void DoModActionButtons(Rect canvas) { }

        internal override void DoModDetails(Rect canvas) { }

        public override IEnumerable<Dependency> Requirements => Manifest.EmptyRequirementList;
    }
}