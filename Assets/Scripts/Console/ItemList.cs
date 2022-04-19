/**
 * @brief List Item in PC Mode
 * @email dbdongbo@vip.qq.com
*/

#if !WITH_OUT_CHEAT_CONSOLE && (UNITY_STANDALONE || UNITY_EDITOR)
using Assets.Scripts.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Console
{
    public interface IItemListStyle
    {
        GUIStyle itemStyle { get; }
        GUIStyle selectedItemStyle { get; }
        
        int itemHeight { get; }
    }

    public abstract class ItemListStyle : IItemListStyle
    {
        protected GUIStyle ListItemStyle;
        protected GUIStyle SelectedListItemStyle;
        protected int ListItemHeight;

        public GUIStyle itemStyle { get { return ListItemStyle; } }
        public GUIStyle selectedItemStyle { get { return SelectedListItemStyle; } }

        public int itemHeight { get { return ListItemHeight; } }

        public ItemListStyle()
        {
            ListItemHeight = 20;   
        }
    }

    class ItemListStyleDefault : ItemListStyle
    {
        public ItemListStyleDefault()
        {
            ListItemStyle = new GUIStyle();
            ListItemStyle.normal.textColor = Color.white;
            Texture2D texInit = new Texture2D(1, 1);
            texInit.SetPixel(0, 0, Color.white);
            texInit.Apply();
            ListItemStyle.hover.background = texInit;
            ListItemStyle.onHover.background = texInit;
            ListItemStyle.hover.textColor = Color.black;
            ListItemStyle.onHover.textColor = Color.black;
            ListItemStyle.padding = new RectOffset(4, 4, 4, 4);

            SelectedListItemStyle = new GUIStyle();
            SelectedListItemStyle.normal.textColor = Color.red;
            SelectedListItemStyle.hover.background = texInit;
            SelectedListItemStyle.onHover.background = texInit;
            SelectedListItemStyle.hover.textColor = Color.red;
            SelectedListItemStyle.onHover.textColor = Color.red;
            SelectedListItemStyle.padding = new RectOffset(4, 4, 4, 4);

            ListItemHeight = 20;
        }
    }

    public interface IListItem
    {
        string name { get; }
    }

    public class ItemList
    {
        protected List<IListItem> Items { get; set; }

        protected int SelectedIndex { get; private set; }

        protected bool bVisible;

        private Rect ListBoxRect;
        private IItemListStyle Style = null;

        public delegate void SelectionEventHandler(int InIndex);
        public event SelectionEventHandler SelectedEvent;
        public event SelectionEventHandler ClickEvent;

        public ItemList()
        {
            bVisible = true;
            SelectedIndex = 0;
        }

        public bool isVisiable
        {
            get
            {
                return bVisible;
            }
            set
            {
                bVisible = value;
            }
        }

        public bool hasValidSelection
        {
            get
            {
                return SelectedIndex >= 0 && SelectedIndex < Items.Count;
            }
        }

        public int selectedIndex
        {
            get
            {
                return SelectedIndex;
            }
        }

        public String SelectedName
        {
            get
            {
                return SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex].name : "";
            }
        }

        public IListItem SelectedItem
        {
            get
            {
                return SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;
            }
        }

        public void Reset(List<IListItem> InNewItems)
        {
            bool bHasChanged = Items == null;
            int SmartSelectionIndex = -1;

            if (!bHasChanged && Items != null && InNewItems != null && hasValidSelection )
            {
                string val = SelectedName;

                DebugHelper.Assert(!string.IsNullOrEmpty(val));

                for( int i= 0; i<InNewItems.Count; ++i )
                {
                    if( InNewItems[i].name.Equals(val, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        SmartSelectionIndex = i;
                        break;
                    }
                }
            }

            Items = InNewItems;

            if (SmartSelectionIndex != -1)
            {
                SelectedIndex = SmartSelectionIndex;
            }
            else
            {
                SelectedIndex = Items != null && Items.Count > 0 ? 0 : -1;
            }
        }

        public void TryMoveSelection( int InDelta )
        {
            int NewSelection = SelectedIndex + InDelta;

            if( Items != null && NewSelection >= 0 && NewSelection < Items.Count && NewSelection != SelectedIndex )
            {
                SelectedIndex = NewSelection;
                
                TriggerSelectedEvent();
            }
        }

        public void ResetSelection( int InIndex )
        {
            if( Items != null && InIndex >= 0 && InIndex < Items.Count )
            {
                SelectedIndex = InIndex;
            }
        }

        private void InitDefaultStyle()
        {
            if( Style == null )
            {
                Style = new ItemListStyleDefault();
            }
        }

        public void ResetStyle()
        {

        }

        public void OnGUI( Rect InBoundRect )
        {
            InitDefaultStyle();

            DebugHelper.Assert(Style != null);

            DrawDropDown(InBoundRect);
        }

        protected void DrawDropDown(Rect InBoundRect)
        {
            if (bVisible && Items != null)
            {
                ListBoxRect = new Rect(InBoundRect);
                ListBoxRect.y  = ListBoxRect.y + ListBoxRect.height;
                ListBoxRect.height = Items.Count * Style.itemHeight;
                
                GUI.Box(ListBoxRect, "");
                
                for (int i = 0; i < Items.Count; i++)
                {
                    Rect ListButtonRect = new Rect(ListBoxRect);
                    ListButtonRect.y = ListButtonRect.y + Style.itemHeight * i;
                    ListButtonRect.height = Style.itemHeight;

                    var StyleSelection = SelectedIndex == i ? Style.selectedItemStyle : Style.itemStyle;

                    if (GUI.Button(
                        ListButtonRect,
                        new GUIContent(Items[i].name, Items[i].name),
                        StyleSelection
                        )
                        )
                    {
                        SelectedIndex = i;
                        
                        TriggerSelectedEvent();

                        if( ClickEvent != null )
                        {
                            ClickEvent(SelectedIndex);
                        }
                    }
                }
            }

        }

        private void TriggerSelectedEvent()
        {
            if (SelectedEvent != null)
            {
                SelectedEvent(SelectedIndex);
            }
        }
    }
}
#endif