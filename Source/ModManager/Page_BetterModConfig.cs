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
        public Rect windowRect = new Rect(0, 0, Screen.width, Screen.height);

        private Vector2 availableListScroll;
        private Vector2 activeListScroll;

        public ModFilter availableFilter = new ModFilter();
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


        private Vector2 detailsScroll;


        private static Page_BetterModConfig _instance;
        public static Page_BetterModConfig Instance => _instance;

        public Page_BetterModConfig()
        {
            _instance = this;
            closeOnAccept = false;
            closeOnCancel = true;
            doCloseButton = false;
            doCloseX = true;

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
                    folder.Contents.Add(new ModInfo($"Mod in folder no.{i} - {j}", $"Developer {i}"));
                }
                AvailableMods.Add(folder);

            }
            */
            ModListManager.LoadMods();
        }



        public override Vector2 InitialSize => new Vector2(Screen.width, Screen.height);
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
            GUI.Label(label, "Mod Manager");


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
                    //ModListManager.AvailableMods.root.Contents.Insert(draggedFrom, dragged);
                    ModListManager.AvailableMods.root.Add(draggedFrom, dragged);
                    dragged = null;

                }
            }

        }

        void ModList(Rect rect, ModList.ModList list, ModFilter filter, ref Vector2 scroll)
        {

            //Mod List info 
            Rect modInfoRect = new Rect(rect.x, rect.y, rect.width, ModListInfoCardHeight);
            DrawModListInfoCard(modInfoRect, list);

            //Mod search bar
            Rect modSearchRect = UIHelpers.GetNextDown(modInfoRect, ManagerUI.SearchBarHeight);
            ManagerUI.DrawModFilter(modSearchRect, filter);

            //#################################
            //Mod scrolling area
            //#################################
            float scrollHeight = rect.height - ModListInfoCardHeight - ManagerUI.SearchBarHeight - 40;
            Rect scrollArea = UIHelpers.GetNextDown(modSearchRect, scrollHeight, 2);

            float innerHeight = ((ModButtonHeight + ModCardSpacing) * list.ModCount) + 10;
            Rect scrollInner = new Rect(0, 0, ModButtonWidth, innerHeight);

            Widgets.DrawBoxSolid(scrollArea, Color.gray);

            // Begin the ScrollView
            scroll = GUI.BeginScrollView(scrollArea, scroll, scrollInner, false, true);

            CardDropZone(scrollInner, list.root, filter);

            // End the ScrollView
            GUI.EndScrollView();

            //Bottom menu
            Rect bottomMenu = UIHelpers.GetNextDown(scrollArea, 30, 10);
            Widgets.DrawBoxSolid(bottomMenu, Color.black);


            Rect button = new Rect(bottomMenu);
            button.width = button.height;
            if (Widgets.ButtonImage(button, Resources.delete_empty, ManagerUI.GetActiveColor(true)))
            {
                ModListManager.CleanActiveModsList();
                selected = null;
            }



        }

        

        private void DrawModListInfoCard(Rect modInfoRect, ModList.ModList list)
        {
            //       Mod pack name
            //
            //                      # of mods


            GUI.Box(modInfoRect, list.root.Name);

            Rect modCountRect = new Rect(modInfoRect);
            modCountRect.width -= 5;

            //EditorGUI.DrawRect(modCountRect, Color.red);
            TextAnchor old = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.Label(modCountRect, $"{list.ModCount} mods");
            GUI.skin.label.alignment = old;

        }


        void CardDropZone(Rect rect, ModFolder list, ModFilter filter)
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
                if (list.Contents.Count < 1)
                    list.Add(0, dragged);
                else
                    list.Add(list.Contents.Count, dragged);
                dragged = null;
            }
        }

        private void DrawTreeElements(ModFolder list, ModFilter filter, ref Rect modRect)
        {
            for (int j = 0; j < list.Contents.Count; j++)
            {
                ListElement item = list.Contents[j];

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
                            list.Add(j, dragged);
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

                        DrawTreeElements(modFolder, filter, ref modRect);

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
                DrawModPreviewImage(prewImg, mod);

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
                    Rect inner = new Rect(tabPanel);
                    inner.xMin += 10;
                    inner.xMax -= 20;
                    inner.height = Text.CalcHeight(mod.Description, inner.width);


                    detailsScroll = GUI.BeginScrollView(tabPanel, detailsScroll, inner, false, true);

                    GUI.Label(inner, mod.Description);
                    
                    GUI.EndScrollView();
                }
                else if (tabSelected == Tabs.Versions)
                {
                    GUILayout.BeginArea(tabPanel);
                    GUILayout.BeginVertical();

                    GUILayout.Label("Versions Available:");

                    GUI.enabled = mod.modList.Active;

                    List<string> text = new List<string>();
                    for (var i = 0; i < mod.versions.Count; i++)
                    {
                        var versionInfo = mod.versions[i];

                        bool status = versionInfo.active;

                        status = GUILayout.Toggle(status,
                            $"{versionInfo.version} for: {versionInfo.targetGameVersion} at: {versionInfo.path}");
                        if (status != versionInfo.active)
                            mod.SetActiveVersion(status, i);


                        //mod.SetActiveVersion(true, selected);

                        //GUILayout.Label();
                    }

                    GUI.enabled = true;

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
            }

        }

        void DrawModPreviewImage(Rect previewImage, ModInfo mod)
        {
            GUI.DrawTexture(previewImage, mod.PreviewVersionInfo.ModMeta.PreviewImage, ScaleMode.ScaleToFit);
        }

    }
}