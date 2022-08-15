using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Texemon.Main;
using Texemon.SceneObjects.Controllers;

namespace Texemon.SceneObjects.Widgets
{
    public class Panel : Widget
    {
        private enum TransitionType
        {
            None,
            Shrink,
            Expand,
            FadeIn,
            FadeOut
        }

        private enum ResizeType
        {
            None,
            Width,
            Height,
            Both
        }

        private NinePatch panelFrame;

        private TransitionType transitionIn;
        private TransitionType transitionOut;

        private Rectangle startWindow;
        private Rectangle endWindow;
        private Color startColor;
        private Color endColor;

        private ResizeType resizeType = ResizeType.None;

        public Panel(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            bool flatframe = false;

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "FlatFrame": flatframe = bool.Parse(xmlAttribute.Value); break;
                    case "Style": panelFrame = new NinePatch("Windows_" + xmlAttribute.Value, Depth, flatframe); panelFrame.FrameColor = Color; break;
                    case "TransitionIn": transitionIn = (TransitionType)Enum.Parse(typeof(TransitionType), xmlAttribute.Value); break;
                    case "TransitionOut": transitionOut = (TransitionType)Enum.Parse(typeof(TransitionType), xmlAttribute.Value); break;
                    case "Resize": resizeType = (ResizeType)Enum.Parse(typeof(ResizeType), xmlAttribute.Value); break;
                }
            }
        }

        public override void LoadChildren(XmlNodeList nodeList, float widgetDepth)
        {
            base.LoadChildren(nodeList, widgetDepth);

            TransitionIn();

            if (panelFrame != null) panelFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if ((!Transitioning || transitionIn != TransitionType.Expand) && !terminated)
            {
                foreach (Widget widget in ChildList)
                {
                    if (widget.Visible)
                        widget.Draw(spriteBatch);
                }

                tooltipWidget?.Draw(spriteBatch);
            }

            panelFrame?.Draw(spriteBatch, Position);
        }

        public override void Close()
        {
            base.Close();

            TransitionOut();
        }

        public void TransitionIn()
        {
            switch (transitionIn)
            {
                case TransitionType.Expand:
                    endWindow = currentWindow;
                    currentWindow = startWindow = new Rectangle((int)currentWindow.Center.X, (int)currentWindow.Center.Y, 0, 0);
                    transition = new TransitionController(TransitionDirection.In, DEFAULT_TRANSITION_LENGTH);
                    transition.UpdateTransition += UpdateTransition;
                    transition.FinishTransition += FinishTransition;
                    GetParent<ViewModel>().ParentScene.AddController(transition);
                    break;

                    /*
                case TransitionType.FadeIn:
                    endWindow = startWindow = currentWindow;
                    startColor = new Color(0, 0, 0, 0);
                    endColor = new Color(255, 255, 255, 255);
                    transitionState = TransitionState.In;
                    break;*/
            }
        }

        public void TransitionOut()
        {
            switch (transitionOut)
            {
                case TransitionType.Shrink:
                    endWindow = currentWindow;
                    startWindow = new Rectangle((int)currentWindow.Center.X, (int)currentWindow.Center.Y, 0, 0);
                    transition = new TransitionController(TransitionDirection.Out, DEFAULT_TRANSITION_LENGTH);
                    transition.UpdateTransition += UpdateTransition;
                    transition.FinishTransition += FinishTransition;
                    GetParent<ViewModel>().ParentScene.AddController(transition);
                    break;
            }
        }

        private void UpdateTransition(float transitionProgress)
        {
            currentWindow = Extensions.Lerp(startWindow, endWindow, transitionProgress);

            if (panelFrame != null) panelFrame.Bounds = currentWindow;
        }

        private void FinishTransition(TransitionDirection transitionDirection)
        {
            transition = null;
            if (Closed) Terminate();
        }

        public void Resize(int width, int height)
        {
            if (resizeType == ResizeType.Both || resizeType == ResizeType.Height)
            {
                currentWindow.Height = height + InnerMargin.Y + InnerMargin.Height;
                endWindow.Height = currentWindow.Height;
            }
        }
    }
}
