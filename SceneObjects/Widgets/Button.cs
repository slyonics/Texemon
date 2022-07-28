using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class Button : Widget
    {
        private MethodInfo buttonEvent;
        private string eventParameter;

        private NinePatch buttonFrame;
        private string style;
        private string pushedStyle;
        private string disabledStyle;
        private bool clicking;
        private GameSound clickSound = GameSound.menu_select;

        private ModelProperty<string> actionParameterBinding;

        public bool Radio { get; private set; } = false;
        private bool selected = false;

        public Button(Widget iParent, float widgetDepth)
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
                    case "Style": style = "Buttons_" + xmlAttribute.Value; break;
                    case "PushedStyle": pushedStyle = "Buttons_" + xmlAttribute.Value; break;
                    case "DisabledStyle": disabledStyle = "Buttons_" + xmlAttribute.Value; break;
                    case "Action": buttonEvent = GetParent<ViewModel>().GetType().GetMethod(xmlAttribute.Value); break;
                    case "ActionParameter": eventParameter = xmlAttribute.Value; break;
                    case "Radio": Radio = bool.Parse(xmlAttribute.Value); break;
                    case "Selected": selected = bool.Parse(xmlAttribute.Value); break;
                    case "Sound": clickSound = string.IsNullOrEmpty(xmlAttribute.Value) ? GameSound.None : (GameSound)Enum.Parse(typeof(GameSound), xmlAttribute.Value); break;

                    case "Binding":
                        actionParameterBinding = LookupBinding<string>(xmlAttribute.Value); actionParameterBinding.ModelChanged += ActionParameterBinding_ModelChanged;
                        if (actionParameterBinding.Value != null) ActionParameterBinding_ModelChanged();
                        break;

                    case "SoftBinding":
                        eventParameter = LookupSoftBinding<string>(xmlAttribute.Value);
                        break;
                }
            }

            if (style != null) buttonFrame = new NinePatch(style, depth);

            if (!enabled) buttonFrame?.SetSprite(disabledStyle);
            else if (selected) buttonFrame?.SetSprite(pushedStyle);
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            if (buttonFrame != null)
                buttonFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Color drawColor = (Enabled) ? color : new Color(120, 120, 120, 255);
            buttonFrame.FrameColor = drawColor;
            buttonFrame?.Draw(spriteBatch, Position);
        }

        protected override void EnableBinding_ModelChanged()
        {
            if (buttonFrame == null)
                enabled = enableBinding.Value;
            else
                Enabled = enableBinding.Value;
        }

        public void ActionParameterBinding_ModelChanged()
        {
            eventParameter = actionParameterBinding.Value;
        }

        public override Widget GetWidgetAt(Vector2 mousePosition)
        {
            if (!currentWindow.Contains(mousePosition - Position)) return null;

            return this;
        }

        public override void StartLeftClick(Vector2 mousePosition)
        {
            if (!enabled) return;

            //Audio.PlaySound(GameSound.a_mainmenuselection);

            clicking = true;
            buttonFrame?.SetSprite(pushedStyle);
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            if (!enabled) return;

            clicking = false;
            buttonFrame?.SetSprite(style);

            if (otherWidget == this)
            {
                Activate();

                if (Radio)
                {
                    RadioSelect();
                }
                else EndMouseOver();

                Audio.PlaySound(clickSound);
            }
        }

        public void Activate()
        {
            string[] parameters = (String.IsNullOrEmpty(eventParameter)) ? null : new string[] { eventParameter };
            buttonEvent?.Invoke(GetParent<ViewModel>(), parameters);
        }

        public void RadioSelect()
        {
            foreach (Widget peer in parent.ChildList)
            {
                Button buttonPeer = peer as Button;
                if (buttonPeer != null && buttonPeer.Radio) buttonPeer.UnSelect();
            }

            selected = true;
            buttonFrame?.SetSprite(pushedStyle);
        }

        public override void StartRightClick(Vector2 mousePosition)
        {
            base.StartRightClick(mousePosition);

            if (clicking && !selected) buttonFrame?.SetSprite(style);
            clicking = false;
        }

        public override void StartMouseOver()
        {
            if (!enabled) return;

            base.StartMouseOver();

            if (clicking) buttonFrame?.SetSprite(pushedStyle);
        }

        public override void EndMouseOver()
        {
            if (!enabled) return;

            base.EndMouseOver();

            if (clicking && !selected) buttonFrame?.SetSprite(style);
        }

        public void UnSelect()
        {
            //if (!enabled) return;

            selected = false;
            buttonFrame?.SetSprite(style);
        }

        public override bool Enabled
        {
            set
            {
                base.Enabled = value;
                if (base.Enabled) buttonFrame?.SetSprite(style);
                else if (disabledStyle != null) buttonFrame?.SetSprite(disabledStyle);
            }
        }
    }
}
