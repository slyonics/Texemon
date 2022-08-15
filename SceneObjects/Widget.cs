using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Texemon.Models;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Widgets;


namespace Texemon.SceneObjects
{
    public enum Alignment
    {
        Anchored,
        Relative,
        Absolute,
        Cascading,
        Vertical,
        ReverseVertical,
        Stretch,
        Left,
        Right,
        Center,
        BottomRight,
        Bottom,
        BottomLeft
    }

    public abstract class Widget : Overlay
    {
        protected const float WIDGET_START_DEPTH = 0.15f;
        protected const float WIDGET_DEPTH_OFFSET = -0.001f;
        protected const float WIDGET_PEER_DEPTH_OFFSET = -0.0005f;

        protected const int DEFAULT_TRANSITION_LENGTH = 250;

        public const int DEFAULT_TOOLTIP_DELAY = 400;
        private const int TOOLTIP_DESPAWN_RANGE = 32;
        private const int TOOLTIP_DESPAWN_DELTA = 8;

        protected static Assembly assembly = Assembly.GetAssembly(typeof(Widget));

        protected Rectangle bounds;
        private Vector2 anchor;
        protected virtual Rectangle Bounds { get => bounds; set => bounds = value; }

        protected Vector2[] layoutOffset = new Vector2[Enum.GetValues(typeof(Alignment)).Length];
        protected Alignment Alignment { get; set; } = Alignment.Absolute;
        protected int horizontalCenterAdjust;
        protected Rectangle currentWindow;

        protected Widget parent;

        protected TransitionController transition = null;


        protected int tooltipTime;
        protected Tooltip tooltipWidget;
        protected Vector2 tooltipOrigin;

        protected bool mousedOver;

        protected ModelProperty<bool> enableBinding;
        protected ModelProperty<bool> visibleBinding;
        protected ModelProperty<Color> colorBinding;
        protected ModelProperty<string> tooltipBinding;
        protected ModelProperty<string> fontBinding;

        public Widget()
            : base()
        {

        }

        public Widget(Widget iParent, float widgetDepth)
        {
            parent = iParent;
            Depth = widgetDepth;
        }

        public void LoadXml(XmlNode xmlNode)
        {
            LoadAttributes(xmlNode);

            ApplyAlignment();

            LoadChildren(xmlNode.ChildNodes, Depth + WIDGET_DEPTH_OFFSET);
        }

        protected virtual void ParseAttribute(string attributeName, string attributeValue)
        {
            PropertyInfo property = GetType().GetProperty(attributeName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null) return;

            object propertyValue = property.GetValue(this);
            switch (propertyValue)
            {
                case bool: property.SetValue(this, bool.Parse(attributeValue)); break;
                case int: property.SetValue(this, ParseInt(attributeValue)); break;
                case float: property.SetValue(this, float.Parse(attributeValue)); break;
                case string: property.SetValue(this, ParseString(attributeValue)); break;
                case Microsoft.Xna.Framework.Color: property.SetValue(this, Graphics.ParseHexcode(attributeValue)); break;
                case Vector2:
                {
                    string[] tokens = attributeValue.Split(',');
                    property.SetValue(this, new Vector2(ParseInt(tokens[0]), ParseInt(tokens[1])));
                    break;
                }
                case Rectangle:
                {
                    string[] tokens = attributeValue.Split(',');
                    property.SetValue(this, new Rectangle(ParseInt(tokens[0]), ParseInt(tokens[1]), ParseInt(tokens[2]), ParseInt(tokens[3])));
                    break;
                }

                default:
                if (propertyValue is Enum)
                {
                    Type type = propertyValue.GetType();
                    property.SetValue(this, Enum.Parse(type, attributeValue));
                }
                break;
            }
        }

        public virtual void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                switch (xmlAttribute.Name)
                {
                    default:
                        ParseAttribute(xmlAttribute.Name, xmlAttribute.Value); break;

                    case "EnableBinding":
                        enableBinding = LookupBinding<bool>(xmlAttribute.Value);
                        enableBinding.ModelChanged += EnableBinding_ModelChanged;
                        EnableBinding_ModelChanged();
                        break;

                    case "VisibleBinding":
                        visibleBinding = LookupBinding<bool>(xmlAttribute.Value);
                        visibleBinding.ModelChanged += VisibleBinding_ModelChanged;
                        VisibleBinding_ModelChanged();
                        break;

                    case "ColorBinding":
                        colorBinding = LookupBinding<Color>(xmlAttribute.Value); colorBinding.ModelChanged += ColorBinding_ModelChanged;
                        ColorBinding_ModelChanged();
                        break;

                    case "TooltipSoftBinding": Tooltip = LookupSoftBinding<string>(xmlAttribute.Value); break;
                    case "TooltipBinding":
                        tooltipBinding = LookupBinding<string>(xmlAttribute.Value); tooltipBinding.ModelChanged += TooltipBinding_ModelChanged;
                        if (tooltipBinding.Value != null) TooltipBinding_ModelChanged();
                        break;

                    case "FontBinding":
                        fontBinding = LookupBinding<string>(xmlAttribute.Value);
                        fontBinding.ModelChanged += FontBinding_ModelChanged;
                        if (fontBinding.Value != null) FontBinding_ModelChanged();
                        break;
                }
            }
        }

        public virtual void ApplyAlignment()
        {
            switch (Alignment)
            {
                case Alignment.Anchored:
                    currentWindow = bounds;

                    break;

                case Alignment.Cascading:
                    if ((int)parent.layoutOffset[(int)Alignment].X + bounds.Width > parent.InnerBounds.Width)
                    {
                        parent.AdjustLayoutOffset(Alignment, new Vector2(-parent.layoutOffset[(int)Alignment].X, bounds.Height));
                    }

                    currentWindow.X = parent.InnerBounds.Left + (int)parent.layoutOffset[(int)Alignment].X + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Top + (int)parent.layoutOffset[(int)Alignment].Y + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(bounds.X + bounds.Width, 0));

                    break;

                case Alignment.Vertical:
                    currentWindow.X += (parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2) + bounds.X;
                    currentWindow.Y = (parent.InnerBounds.Top + (int)parent.layoutOffset[(int)Alignment].Y) + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(0, bounds.Y + bounds.Height));
                    break;

                case Alignment.ReverseVertical:
                    currentWindow.X += parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2;
                    currentWindow.Y = parent.InnerBounds.Bottom - (int)parent.layoutOffset[(int)Alignment].Y - bounds.Height;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(Alignment, new Vector2(0, -bounds.Height - bounds.Y));
                    break;

                case Alignment.Center:
                    currentWindow.X = (parent.InnerBounds.Center.X - bounds.Width / 2) + bounds.X;
                    currentWindow.Y = (parent.InnerBounds.Center.Y - bounds.Height / 2) + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;
                    break;

                case Alignment.Absolute:
                    currentWindow = bounds;
                    if (parent != null)
                    {
                        currentWindow.X += parent.InnerBounds.X;
                        currentWindow.Y += parent.InnerBounds.Y;
                    }
                    break;

                case Alignment.Relative:
                    currentWindow = bounds;
                    currentWindow.X += parent.InnerBounds.X;
                    currentWindow.Y += parent.InnerBounds.Y;
                    break;

                case Alignment.Stretch:
                    bounds = currentWindow = parent.InnerBounds;
                    break;

                case Alignment.BottomRight:
                    currentWindow.X = parent.InnerBounds.Right + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Bottom + bounds.Y;
                    break;

                case Alignment.Bottom:
                    currentWindow.X += parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2;
                    currentWindow.Y = parent.InnerBounds.Bottom - bounds.Height;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;
                    break;

                case Alignment.BottomLeft:
                    currentWindow.X = parent.InnerBounds.Left + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Bottom - bounds.Height + bounds.Y;
                    break;
            }
        }

        protected virtual int ParseInt(string token)
        {
            if (token[0] == '-') return -ParseInt(token.Remove(0, 1));
            else if (token[0] == '$')
            {
                switch (token)
                {
                    case "$ScreenWidth": return CrossPlatformGame.ScreenWidth;
                    case "$ScreenHeight": return CrossPlatformGame.ScreenHeight;
                    case "$CenterX": return CrossPlatformGame.ScreenWidth / 2;
                    case "$CenterY": return CrossPlatformGame.ScreenHeight / 2;
                    case "$Top": return 0;
                    case "$Bottom": return CrossPlatformGame.ScreenHeight;
                    default: throw new Exception();
                }
            }

            return int.Parse(token);
        }

        protected virtual string ParseString(string token)
        {
            if (String.IsNullOrEmpty(token)) return "";
            else if (token[0] != '$') return token;

            string[] tokens = token.Split('.');
            if (tokens.Length == 1)
            {
                return "ERROR";
            }
            else
            {
                switch (tokens[0])
                {
                    case "$PlayerProfile": return (GameProfile.PlayerProfile.GetType().GetProperty(tokens[1]).GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value;
                }
            }

            return GetParent<ViewModel>().ParseString(token);
        }

        public virtual void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            float depthOffset = 0;
            foreach (XmlNode node in nodeList)
            {
                Widget widget;
                switch (node.Name)
                {
                    case "LineBreak":
                        if (node.Attributes != null && node.Attributes["Height"] != null)
                        {
                            int offset = int.Parse(node.Attributes["Height"].Value);
                            layoutOffset[(int)Alignment.Cascading] = new Vector2(0, layoutOffset[(int)Alignment.Cascading].Y + offset);
                            layoutOffset[(int)Alignment.Vertical] = new Vector2(0, layoutOffset[(int)Alignment.Vertical].Y + offset);
                        }
                        else
                        {
                            layoutOffset[(int)Alignment.Cascading] = new Vector2(0, layoutOffset[(int)Alignment.Cascading].Y + ChildList.Last().bounds.Height);
                            layoutOffset[(int)Alignment.Vertical] = new Vector2(0, layoutOffset[(int)Alignment.Vertical].Y + ChildList.Last().bounds.Height);
                        }
                        continue;

                    default:
                        widget = (Widget)assembly.CreateInstance(CrossPlatformGame.GAME_NAME + ".SceneObjects.Widgets." + node.Name, false, BindingFlags.CreateInstance, null, new object[] { this, widgetDepth + depthOffset }, null, null);
                        break;
                }

                AddChild(widget, node);
                depthOffset += WIDGET_PEER_DEPTH_OFFSET;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Closed && !ChildList.Any(x => x.Transitioning))
                Terminate();

            if (!Visible) return;

            foreach (Widget widget in ChildList) widget.Update(gameTime);

            if (mousedOver && !Transitioning && tooltipWidget == null && Input.DeltaMouseGame.Length() < 1 && Tooltip != "")
            {
                tooltipTime += gameTime.ElapsedGameTime.Milliseconds;
                if (tooltipTime >= TooltipDelay)
                {
                    tooltipWidget = new Tooltip(Input.MousePosition, Tooltip);
                    tooltipTime = 0;
                    tooltipOrigin = new Vector2(tooltipWidget.InnerBounds.X, tooltipWidget.InnerBounds.Y);
                }
            }

            if (tooltipWidget != null)
            {
                tooltipWidget.Update(gameTime);

                if (Vector2.Distance(tooltipOrigin, new Vector2(tooltipWidget.InnerBounds.X, tooltipWidget.InnerBounds.Y)) > TOOLTIP_DESPAWN_RANGE || Input.DeltaMouseGame.Length() > TOOLTIP_DESPAWN_DELTA)
                    DeleteTooltip();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Transitioning && !terminated)
            {
                foreach (Widget widget in ChildList)
                {
                    if (widget.Visible)
                        widget.Draw(spriteBatch);
                }

                tooltipWidget?.Draw(spriteBatch);
            }
        }

        public virtual void AddChild(Widget widget, XmlNode node)
        {
            ChildList.Add(widget);
            widget.LoadXml(node);
        }

        public virtual Widget GetWidgetAt(Vector2 mousePosition)
        {
            if (!currentWindow.Contains(mousePosition - Position)) return null;
            foreach (Widget child in ChildList)
            {
                Widget widget = child.GetWidgetAt(mousePosition);
                if (widget != null) return widget;
            }

            return this;
        }

        public T GetParent<T>() where T : Widget
        {
            if (parent == null) return null;
            if (parent is T) return parent as T;
            return parent.GetParent<T>();
        }

        public Widget GetDescendent(Widget superParent)
        {
            if (parent == superParent) return this;

            return parent.GetDescendent(superParent);
        }

        public virtual T GetWidget<T>(string widgetName) where T : Widget
        {
            foreach (Widget child in ChildList)
            {
                if (child.Name == widgetName) return child as T;
                else
                {
                    Widget result = child.GetWidget<T>(widgetName);
                    if (result != null) return result as T;
                }
            }

            return null;
        }

        public ModelProperty<T> LookupBinding<T>(string bindingName)
        {
            string[] tokens = bindingName.Split('.');

            object dataContext;
            switch (tokens[0])
            {
                case "PlayerProfile":
                    dataContext = GameProfile.PlayerProfile;
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                case "DataGrid":
                    dataContext = GetParent<DataGrid>().Binding[0];
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                default:
                    dataContext = GetParent<ViewModel>();
                    break;
            }

            while (tokens.Length > 1)
            {
                dataContext = dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext);
                tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
            }

            return dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext) as ModelProperty<T>;
        }

        public T LookupSoftBinding<T>(string bindingName)
        {
            string[] tokens = bindingName.Split('.');

            object dataContext;
            switch (tokens[0])
            {
                case "PlayerProfile":
                    dataContext = GameProfile.PlayerProfile;
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                case "DataGrid":
                    DataGrid parent = GetParent<DataGrid>();
                    dataContext = parent.Binding[parent.ChildList.IndexOf(GetDescendent(parent))];
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                default:
                    dataContext = GetParent<ViewModel>();
                    break;
            }

            while (tokens.Length > 1)
            {
                dataContext = dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext);
                tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
            }

            return (T)dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext);
        }

        public dynamic LookupCollectionBinding(string bindingName)
        {
            string[] tokens = bindingName.Split('.');

            object dataContext;
            switch (tokens[0])
            {
                case "PlayerProfile":
                    dataContext = GameProfile.PlayerProfile;
                    tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
                    break;

                default:
                    dataContext = GetParent<ViewModel>();
                    break;
            }

            while (tokens.Length > 1)
            {
                dataContext = dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext);
                tokens = tokens.TakeLast(tokens.Length - 1).ToArray();
            }

            return dataContext.GetType().GetProperty(tokens[0]).GetValue(dataContext);
        }

        public void AdjustLayoutOffset(Alignment alignment, Vector2 offset)
        {
            layoutOffset[(int)alignment] += offset;
        }

        public virtual void StartLeftClick(Vector2 mousePosition) { }
        public virtual void StartRightClick(Vector2 mousePosition) { }
        public virtual void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget) { }
        public virtual void EndRightClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget) { }
        public virtual void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget) { }
        public virtual void RightClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget) { }
        public virtual void LoseFocus() { }

        public virtual void StartMouseOver()
        {
            mousedOver = true;
        }

        public virtual void EndMouseOver()
        {
            mousedOver = false;
            DeleteTooltip();
        }

        private void DeleteTooltip()
        {
            tooltipWidget = null;
            tooltipTime = 0;
        }

        protected virtual void EnableBinding_ModelChanged()
        {
            Enabled = enableBinding.Value;
        }

        private void VisibleBinding_ModelChanged()
        {
            Visible = visibleBinding.Value;
        }

        public void ColorBinding_ModelChanged()
        {
            Color = colorBinding.Value;
        }

        public void TooltipBinding_ModelChanged()
        {
            Tooltip = tooltipBinding.Value;
        }

        private void FontBinding_ModelChanged()
        {
            Font = (GameFont)Enum.Parse(typeof(GameFont), (string)fontBinding.Value);
        }

        public override void Terminate()
        {
            base.Terminate();

            foreach (Widget widget in ChildList) widget.Terminate();
        }

        public virtual void Close()
        {
            Closed = true;
            foreach (Widget widget in ChildList) widget.Close();

            if (!ChildList.Any(x => x.Transitioning)) Terminate();
        }

        public List<Widget> ChildList { get; protected set; } = new List<Widget>();

        public virtual Vector2 Position
        {
            get
            {
                if (Alignment == Alignment.Anchored) return Anchor;
                else if (parent != null) return parent.Position;
                else return Vector2.Zero;
            }
        }
        protected Vector2 Anchor { get => anchor; set { anchor = value; Alignment = Alignment.Anchored; } }
        public Rectangle OuterBounds { get => currentWindow; set => currentWindow = value; }
        public Rectangle InnerMargin { get; protected set; } = new Rectangle();
        public Rectangle InnerBounds { get => new Rectangle(currentWindow.Left + InnerMargin.Left, currentWindow.Top + InnerMargin.Top, currentWindow.Width - InnerMargin.Left - InnerMargin.Width, currentWindow.Height - InnerMargin.Top - InnerMargin.Height); }
        public Vector2 AbsolutePosition { get => Position + new Vector2(currentWindow.X, currentWindow.Y); }

        public string Name { get; protected set; } = "Widget";
        protected GameFont Font { get; set; } = GameFont.Tooltip;
        public Color Color { get; protected set; } = Color.White;
        protected float Depth { get; set; } = 1.0f;
        public virtual bool Visible { get; set; } = true;
        public virtual bool Enabled { get; set; } = true;
        

        


        public int TooltipDelay { get; protected set; } = DEFAULT_TOOLTIP_DELAY;
        public string Tooltip { get; protected set; } = "";

        public bool Transitioning { get => transition != null; }

        public bool Closed { get; protected set; } = false;
        public override bool Terminated { get => terminated && !Transitioning; }
    }
}
