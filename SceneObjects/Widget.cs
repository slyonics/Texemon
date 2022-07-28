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
    public abstract class Widget : Overlay
    {
        public enum Alignment
        {
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
            BottomLeft,
        }

        protected const float WIDGET_START_DEPTH = 0.15f;
        protected const float WIDGET_DEPTH_OFFSET = -0.001f;
        protected const float WIDGET_PEER_DEPTH_OFFSET = -0.0005f;

        protected const int DEFAULT_TRANSITION_LENGTH = 250;

        public const int DEFAULT_TOOLTIP_DELAY = 400;
        private const int TOOLTIP_DESPAWN_RANGE = 32;
        private const int TOOLTIP_DESPAWN_DELTA = 8;

        protected static Assembly assembly = Assembly.GetAssembly(typeof(Widget));

        protected string name;
        protected GameFont font = GameFont.TurpentineRegular;
        protected Color color = Color.White;

        protected Vector2? anchor;
        protected Rectangle bounds;
        protected int scaleX = 1;
        protected int scaleY = 1;
        protected float depth;

        protected Vector2[] layoutOffset = new Vector2[Enum.GetValues(typeof(Alignment)).Length];
        protected Alignment alignment = Alignment.Absolute;
        protected int horizontalCenterAdjust;

        protected Rectangle currentWindow;
        protected Rectangle innerMargin;

        protected Widget parent;
        protected List<Widget> childList = new List<Widget>();

        protected TransitionController transition = null;
        protected bool closed;

        protected int tooltipTime;
        public int TooltipDelay { get; set; } = DEFAULT_TOOLTIP_DELAY;
        protected string tooltipText = "";
        protected Tooltip tooltip;
        protected Vector2 tooltipOrigin;

        protected bool visible = true;
        protected bool enabled = true;
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
            depth = widgetDepth;
        }

        public void LoadXml(XmlNode xmlNode)
        {
            LoadAttributes(xmlNode);

            ApplyAlignment();

            LoadChildren(xmlNode.ChildNodes, depth + WIDGET_DEPTH_OFFSET);
        }

        public virtual void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Name": name = xmlAttribute.Value; break;
                    case "Color": color = Graphics.ParseHexcode(xmlAttribute.Value); break;
                    case "Anchor": tokens = xmlAttribute.Value.Split(','); anchor = new Vector2(ParseInt(tokens[0]), ParseInt(tokens[1])); break;
                    case "InnerMargin": tokens = xmlAttribute.Value.Split(','); innerMargin = new Rectangle(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3])); break;
                    case "Tooltip": tooltipText = xmlAttribute.Value; break;
                    case "TooltipDelay": TooltipDelay = int.Parse(xmlAttribute.Value); break;
                    case "Alignment": alignment = (Alignment)Enum.Parse(typeof(Alignment), xmlAttribute.Value); break;
                    case "Depth": depth = float.Parse(xmlAttribute.Value); break;
                    case "Visible": visible = bool.Parse(xmlAttribute.Value); break;
                    case "Enabled": enabled = bool.Parse(xmlAttribute.Value); break;
                    case "Font": font = (GameFont)Enum.Parse(typeof(GameFont), xmlAttribute.Value); break;

                    case "Bounds":
                        tokens = xmlAttribute.Value.Split(',');
                        bounds = new Rectangle(ParseInt(tokens[0]), ParseInt(tokens[1]), ParseInt(tokens[2]), ParseInt(tokens[3]));
                        if (tokens.Length == 5) scaleX = scaleY = ParseInt(tokens[4]);
                        else if (tokens.Length == 6)
                        {
                            scaleX = ParseInt(tokens[4]);
                            scaleY = ParseInt(tokens[5]);
                        }
                        break;

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

                    case "TooltipSoftBinding": tooltipText = LookupSoftBinding<string>(xmlAttribute.Value); break;
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

            if (scaleX != 1 || scaleY != 1)
            {
                int innerWidth = bounds.Width;
                int innerHeight = bounds.Height;

                bounds.X += bounds.X * (scaleX - 1);
                bounds.Y += bounds.Y * (scaleY - 1);
                bounds.Width += innerWidth * (scaleX - 1);
                bounds.Height += innerHeight * (scaleY - 1);
            }
        }

        public virtual void ApplyAlignment()
        {
            switch (alignment)
            {
                case Alignment.Cascading:
                    if ((int)parent.LayoutOffset[(int)alignment].X + bounds.Width > parent.InnerBounds.Width)
                    {
                        parent.AdjustLayoutOffset(alignment, new Vector2(-parent.LayoutOffset[(int)alignment].X, bounds.Height));
                    }

                    currentWindow.X = parent.InnerBounds.Left + (int)parent.LayoutOffset[(int)alignment].X + bounds.X;
                    currentWindow.Y = parent.InnerBounds.Top + (int)parent.LayoutOffset[(int)alignment].Y + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(alignment, new Vector2(bounds.X + bounds.Width, 0));

                    break;

                case Alignment.Vertical:
                    currentWindow.X += (parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2) + bounds.X;
                    currentWindow.Y = (parent.InnerBounds.Top + (int)parent.LayoutOffset[(int)alignment].Y) + bounds.Y;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(alignment, new Vector2(0, bounds.Y + bounds.Height));
                    break;

                case Alignment.ReverseVertical:
                    currentWindow.X += parent.OuterBounds.X - (bounds.Width - parent.OuterBounds.Width) / 2;
                    currentWindow.Y = parent.InnerBounds.Bottom - (int)parent.LayoutOffset[(int)alignment].Y - bounds.Height;
                    currentWindow.Width = bounds.Width;
                    currentWindow.Height = bounds.Height;

                    parent.AdjustLayoutOffset(alignment, new Vector2(0, -bounds.Height - bounds.Y));
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
                    currentWindow.X = parent.InnerBounds.Right;
                    currentWindow.Y = parent.InnerBounds.Bottom;
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
                    case "$MapX": return (int)(CrossPlatformGame.ScreenWidth * 3 / 4);
                    case "$MapY": return (int)(CrossPlatformGame.ScreenHeight / 2);
                    case "$Scale": return CrossPlatformGame.Scale;
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
                            layoutOffset[(int)Alignment.Cascading] = new Vector2(0, layoutOffset[(int)Alignment.Cascading].Y + childList.Last().bounds.Height);
                            layoutOffset[(int)Alignment.Vertical] = new Vector2(0, layoutOffset[(int)Alignment.Vertical].Y + childList.Last().bounds.Height);
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
            if (closed && !childList.Any(x => x.Transitioning))
                Terminate();

            if (!visible) return;

            foreach (Widget widget in childList) widget.Update(gameTime);

            if (mousedOver && !Transitioning && tooltip == null && Input.DeltaMouseGame.Length() < 1 && tooltipText != "")
            {
                tooltipTime += gameTime.ElapsedGameTime.Milliseconds;
                if (tooltipTime >= TooltipDelay)
                {
                    tooltip = new Tooltip(Input.MousePosition, tooltipText);
                    tooltipTime = 0;
                    tooltipOrigin = new Vector2(tooltip.InnerBounds.X, tooltip.InnerBounds.Y);
                }
            }

            if (tooltip != null)
            {
                tooltip.Update(gameTime);

                if (Vector2.Distance(tooltipOrigin, new Vector2(tooltip.InnerBounds.X, tooltip.InnerBounds.Y)) > TOOLTIP_DESPAWN_RANGE || Input.DeltaMouseGame.Length() > TOOLTIP_DESPAWN_DELTA)
                    DeleteTooltip();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Transitioning && !terminated)
            {
                foreach (Widget widget in childList)
                {
                    if (widget.visible)
                        widget.Draw(spriteBatch);
                }

                tooltip?.Draw(spriteBatch);
            }
        }

        public virtual void AddChild(Widget widget, XmlNode node)
        {
            childList.Add(widget);
            widget.LoadXml(node);
        }

        public virtual Widget GetWidgetAt(Vector2 mousePosition)
        {
            if (!currentWindow.Contains(mousePosition - Position)) return null;
            foreach (Widget child in childList)
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
            foreach (Widget child in childList)
            {
                if (child.name == widgetName) return child as T;
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
                    dataContext = parent.Binding[parent.childList.IndexOf(GetDescendent(parent))];
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
            tooltip = null;
            tooltipTime = 0;
        }

        protected virtual void EnableBinding_ModelChanged()
        {
            enabled = enableBinding.Value;
        }

        private void VisibleBinding_ModelChanged()
        {
            visible = visibleBinding.Value;
        }

        public void ColorBinding_ModelChanged()
        {
            color = colorBinding.Value;
        }

        public void TooltipBinding_ModelChanged()
        {
            tooltipText = tooltipBinding.Value;
        }

        private void FontBinding_ModelChanged()
        {
            font = (GameFont)Enum.Parse(typeof(GameFont), (string)fontBinding.Value);
        }

        public override void Terminate()
        {
            base.Terminate();

            foreach (Widget widget in childList) widget.Terminate();
        }

        public virtual void Close()
        {
            closed = true;
            foreach (Widget widget in childList) widget.Close();

            if (!childList.Any(x => x.Transitioning)) Terminate();
        }

        public string Name { get => name; }
        public virtual Vector2 Position
        {
            get
            {
                if (anchor.HasValue) return anchor.Value;
                else if (parent != null) return parent.Position;
                else return Vector2.Zero;
            }
        }

        public int Height { get => Text.GetStringHeight(font); }
        public Vector2[] LayoutOffset { get => layoutOffset; }
        public Rectangle OuterBounds { get => currentWindow; set => currentWindow = value; }
        public Rectangle InnerBounds { get => new Rectangle(currentWindow.Left + innerMargin.Left, currentWindow.Top + innerMargin.Top, currentWindow.Width - innerMargin.Left - innerMargin.Width, currentWindow.Height - innerMargin.Top - innerMargin.Height); }
        public Rectangle InnerMargin { get => innerMargin; }
        public Vector2 AbsolutePosition { get => Position + new Vector2(currentWindow.X, currentWindow.Y); }
        public List<Widget> ChildList { get => childList; }

        public bool Visible { get => visible; set => visible = value; }
        public virtual bool Enabled { get => enabled; set => enabled = value; }
        public bool Transitioning { get => transition != null; }
        public override bool Terminated { get => terminated && !Transitioning; }
        public bool Closed { get => closed; }
        public string TooltipText { get => tooltipText; set => tooltipText = value; }
    }
}
