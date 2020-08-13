// Window_ModSelection.cs
// Copyright Karel Kroeze, 2018-2018

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static ModManager.Constants;
using static ModManager.Resources;

using ModManager.ModList;

namespace ModManager
{
    public enum Tabs
    {
        Details,
        Versions
    }

    [HotSwappable]
    public class Page_BetterModConfig: Page_ModsConfig
    {
        public Rect windowRect = new Rect(20, 20, 1000, 600);

        private Vector2 availableListScroll;
        private Vector2 activeListScroll;

        //public ModList.ModList AvailableMods = new ModList.ModList();
        public ModFilter availableFilter = new ModFilter();

        //public ModList.ModList ActiveMods = new ModList.ModList();
        public ModFilter activeFilter = new ModFilter();


        public ListElement selected = null;

        private ListElement dragged = null;
        private int draggedFrom;
        private bool Dragging => dragged != null;

        public Tabs tabSelected = Tabs.Details;

        float modListWidth;
        float ModButtonWidth;
        private int ModButtonHeight = 50;
        private int ModCardSpacing = 2;

        private int ModListInfoCardHeight = 60;
        private int SearchBarHeight = 20;



        private static Page_BetterModConfig _instance;
        public static Page_BetterModConfig Instance => _instance;

        public Page_BetterModConfig()
        {
            _instance = this;

            dragged = null;
            /*
            for (int i = 0; i < 5; i++)
            {
                var mod = new ModInfo($"Mod no. {i}", $"Developer {i}");

                AvailableMods.Add(mod);
            }

            for (int i = 0; i < 5; i++)
            {
                ModFolder folder = new ModFolder($"Folder {i}");
                folder.open = false;
                for (int j = 5; j < 12; j++)
                {
                    folder.contents.Add(new ModInfo($"Mod in folder no.{i} - {j}", $"Developer {i}"));
                }
                AvailableMods.Add(folder);

            }
            */
            ModListManager.LoadMods();
        }

        

        public override Vector2 InitialSize => StandardSize;
        public static Vector2 MinimumSize => StandardSize * 2 / 3f;

        public override void DoWindowContents( Rect canvas )
        {
            windowRect = canvas;
            DrawWindow(0);

        }

        public override void PostClose()
        {
            base.PostClose();
            ModListManager.FinalizeModLists();
        }

        void DrawWindow(int i)
        {
            Rect label = new Rect(
                0,
                0,
                windowRect.width,
                20
                );
            GUI.Label(label, "asdasdasdasdad");


            modListWidth = windowRect.width / 4;
            ModButtonWidth = modListWidth - 17;


            Rect availableRect = new Rect(
                20,
                40,
                modListWidth,
                windowRect.height - 50
            );

            ModList(availableRect, ModListManager.AvailableMods, availableFilter, ref availableListScroll);


            Rect activeRect = new Rect(availableRect);
            activeRect.x += modListWidth + 10;


            ModList(activeRect, ModListManager.ActiveMods, activeFilter, ref activeListScroll);


            Rect modInfoRect = new Rect(activeRect);
            modInfoRect.x += modListWidth + 10;
            modInfoRect.width = modListWidth * 2 - 60;
            DrawModInfo(modInfoRect, selected);

            // draw as mouse attachment                
            var rect = new Rect(0, 0, ModButtonWidth, ModButtonHeight);
            var pos = Event.current.mousePosition;
            rect.position = pos + new Vector2(6f, 6f);

            if (dragged != null)
            {
                DrawModCard(rect, dragged);

                if (Event.current.type == EventType.MouseUp)
                {
                    //TODO: record where the mod came form
                    ModListManager.AvailableMods.root.contents.Insert(draggedFrom, dragged);
                    dragged = null;

                }
            }

        }

        void ModList(Rect rect, ModList.ModList list, ModFilter filter, ref Vector2 scroll)
        {

            Rect modInfoRect = new Rect(
                rect.x,
                rect.y,
                rect.width,
                ModListInfoCardHeight
            );
            DrawModListInfoCard(modInfoRect, list);

            Rect modSearchRect = new Rect(modInfoRect);
            modSearchRect.y += ModListInfoCardHeight + 2;
            modSearchRect.height = 20;
            DrawModFilter(modSearchRect, filter);


            Rect scrollArea = new Rect(modSearchRect);
            scrollArea.y += 22;
            scrollArea.height = rect.height - ModListInfoCardHeight - 20;

            float innerHeight = ((ModButtonHeight + ModCardSpacing) * list.ModCount) + 10;
            Rect scrollInner = new Rect(
                0,
                0,
                ModButtonWidth,
                innerHeight
            );

            //EditorGUI.DrawRect(scrollArea, Color.gray);
            Widgets.DrawBoxSolid(scrollArea, Color.gray);

            // Begin the ScrollView
            scroll = GUI.BeginScrollView(scrollArea, scroll, scrollInner, false, true);

            CardDropZone(scrollInner, list.root.contents, filter);



            // End the ScrollView
            GUI.EndScrollView();
        }

        private void DrawModFilter(Rect rect, ModFilter filter)
        {
            Rect searchFiled = new Rect(rect);
            searchFiled.width -= (SearchBarHeight * 2) + 4;
            filter.filter = Widgets.TextField(searchFiled, filter.filter);

            Rect clearButton = new Rect(rect);
            clearButton.xMin = clearButton.xMax - SearchBarHeight;

            Color color = string.IsNullOrEmpty(filter.filter) ? Color.black : Color.white;

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

        private void DrawModListInfoCard(Rect modInfoRect, ModList.ModList list)
        {

            GUI.Box(modInfoRect, list.root.Name);

            Rect modCountRect = new Rect(modInfoRect);
            modCountRect.yMin = modCountRect.yMax - SearchBarHeight;
            modCountRect.width -= 5;

            //EditorGUI.DrawRect(modCountRect, Color.red);
            TextAnchor old = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.Label(modCountRect, $"{list.ModCount} mods");
            GUI.skin.label.alignment = old;

        }


        void CardDropZone(Rect rect, List<ListElement> list, ModFilter filter)
        {
            Rect modRect = new Rect(
                rect.xMin,
                rect.yMin + 10,
                rect.width,
                ModButtonHeight
            );

            DrawTreeElements(list, filter, ref modRect);

            if (dragged != null && Event.current.type == EventType.MouseUp)
            {
                if (list.Count < 1)
                    list.Insert(0, dragged);
                else
                    list.Insert(list.Count, dragged);
                dragged = null;
            }
        }

        private void DrawTreeElements(List<ListElement> list, ModFilter filter, ref Rect modRect)
        {
            for (int j = 0; j < list.Count; j++)
            {
                ListElement item = list[j];

                if (!item.Filter(filter))
                {
                    if (filter.HideNotMatching)
                    {
                        continue;
                    }
                    else
                    {
                        GUI.color = Color.gray;
                    }
                }



                Rect dropRect = modRect;
                dropRect.height += ModCardSpacing;


                if (Mouse.IsOver(dropRect))
                {
                    //GUI.color = Color.green;

                    if (Event.current.type == EventType.MouseDrag)
                    {
                        if (dragged == null)
                        {
                            dragged = item;
                            draggedFrom = j;
                            list.Remove(item);
                        }
                    }
                    else if (Event.current.type == EventType.MouseDown && !Dragging)
                    {
                        selected = item;
                    }
                    if (Event.current.type == EventType.MouseUp)
                    {
                        if (dragged != null)
                        {
                            list.Insert(j, dragged);
                            dragged = null;
                        }
                        else if (item is ModFolder folder)
                        {
                            folder.open = !folder.open;
                        }
                    }
                }

                DrawModCard(modRect, item);


                // GUI.color = Color.white;

                //Insert indicator
                if (Mouse.IsOver(modRect) && dragged != null)
                {
                    //EditorGUI.DrawRect(modRect, Color.red);

                    Rect separator = modRect;
                    separator.height = ModCardSpacing;
                    Widgets.DrawLine(new Vector2(separator.x, separator.y), new Vector2(separator.xMax, separator.y),
                        Color.white, ModCardSpacing);
                }

                modRect.y += ModButtonHeight + ModCardSpacing;

                if (item is ModFolder modFolder)
                {
                    if (modFolder.open)
                    {
                        modRect.x += 20;

                        DrawTreeElements(modFolder.contents, filter, ref modRect);

                        modRect.x -= 20;
                    }
                }

                GUI.color = Color.white;
            }
        }

        void DrawModCard(Rect rect, ListElement mod)
        {
            if (selected == mod)
            {
                //EditorGUI.DrawRect(rect, Color.cyan);
                Widgets.DrawBoxSolid(rect, Color.cyan);
            }

            if (mod is ModInfo modInfo)
                GUI.Box(rect, $"{modInfo.Name}");

            if (mod is ModFolder folder)
                GUI.Box(rect, $"{folder.Name}");

            //GUI.skin.box.normal.background = old;

        }


        void DrawModInfo(Rect rect, ListElement element)
        {
            //rect.xMin += 5;
            if (element == null)
                return;

            Rect prewImg = new Rect(rect);
            prewImg.height = (prewImg.height / 2) - 50;
            //EditorGUI.DrawRect(prewImg, Color.black);
            Widgets.DrawBoxSolid(prewImg, Color.black);
            //GUI.DrawTexture(prewImg, mod_prew, ScaleMode.ScaleToFit);

            Rect modData = new Rect(prewImg);
            modData.y += prewImg.height + 5;
            modData.height = 50;
            //EditorGUI.DrawRect(modData, Color.black);
            Widgets.DrawBoxSolid(modData, Color.black);

            Rect nameRect = new Rect(modData);
            nameRect.x += 5;
            nameRect.height = 20;
            GUI.Label(nameRect, $"{element.Name}");

            if (element is ModInfo mod)
            {
                nameRect.y += 20;
                GUI.Label(nameRect, $"Author: {mod.author}");

                Rect tabButton = new Rect(modData);
                tabButton.y += modData.height;
                tabButton.height = 20;

                GUIContent detailsButtonContent = new GUIContent("Details");
                tabButton.width = 150;
                if (GUI.Button(tabButton, "Details"))
                    tabSelected = Tabs.Details;

                tabButton.x += 150;
                if (GUI.Button(tabButton, "Versions"))
                    tabSelected = Tabs.Versions;

                Rect tabPanel = new Rect(rect);
                tabPanel.height = (rect.height / 2) - 30;
                tabPanel.y += rect.height / 2 + 30;

                //EditorGUI.DrawRect(tabPanel, Color.gray);
                Widgets.DrawBoxSolid(tabPanel, Color.gray);
                if (tabSelected == Tabs.Details)
                {

                }
                else if (tabSelected == Tabs.Versions)
                {
                    GUILayout.BeginArea(tabPanel);
                    GUILayout.BeginVertical();

                    GUILayout.Label("Versions Available:");

                    bool hasActive = false;
                    foreach (var versionInfo in mod.versions)
                    {
                        versionInfo.active = GUILayout.Toggle(versionInfo.active,
                            $"{versionInfo.version} for: {versionInfo.targetGameVersion} at: {versionInfo.path}");
                        
                        if (hasActive)
                        {
                            versionInfo.active = false;
                        }
                        hasActive = versionInfo.active;
                        //GUILayout.Label();
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
            }

        }


    }
}