﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class DataGrid : Widget
    {
        public dynamic Binding { get; private set; }

        private XmlNode dataTemplate;

        public DataGrid(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Binding":
                        Binding = LookupCollectionBinding(xmlAttribute.Value);
                        Binding.SubscribeModelChanged(new ChangeCallback(Binding_ModelChanged));
                        Binding.SubscribeCollectionChanged(new ChangeCallback(Binding_CollectionChanged));
                        break;
                }
            }

            dataTemplate = xmlNode.ChildNodes[0];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Transitioning && !terminated)
            {
                foreach (Widget widget in childList)
                {
                    if (widget.Visible && IsChildVisible(widget))
                        widget.Draw(spriteBatch);
                }

                tooltip?.Draw(spriteBatch);
            }

        }


        private void Binding_CollectionChanged()
        {
            Binding_ModelChanged();
        }

        private void Binding_ModelChanged()
        {
            for (int i = 0; i < layoutOffset.Length; i++) layoutOffset[i] = new Vector2();
            foreach (Widget widget in childList) widget.Terminate();
            childList.Clear();

            foreach (var modelProperty in Binding)
            {
                Widget childWidget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + ".SceneObjects.Widgets." + dataTemplate.Name, false, BindingFlags.CreateInstance, null, new object[] { this, depth + WIDGET_DEPTH_OFFSET }, null, null);
                AddChild(childWidget, dataTemplate);
            }
        }

        public override void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            foreach (var modelProperty in Binding)
            {
                Widget childWidget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + ".SceneObjects.Widgets." + dataTemplate.Name, false, BindingFlags.CreateInstance, null, new object[] { this, widgetDepth + WIDGET_DEPTH_OFFSET }, null, null);
                AddChild(childWidget, dataTemplate);
            }
        }

        public bool IsChildVisible(Widget child)
        {
            return (child.OuterBounds.Bottom - scrollOffset.Y < InnerBounds.Bottom + InnerMargin.Y) &&
                (child.OuterBounds.Top - scrollOffset.Y >= InnerBounds.Top + InnerMargin.Y);
        }

        public void ScrollUp()
        {
            scrollOffset.Y -= childList.Last().OuterBounds.Height;
        }

        public void ScrollDown()
        {
            scrollOffset.Y += childList.Last().OuterBounds.Height;
        }

        private Vector2 scrollOffset;

        public override Vector2 Position
        {
            get
            {
                return base.Position - scrollOffset;
            }
        }
    }
}
