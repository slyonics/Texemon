﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.Scenes.TitleScene
{
    public class SettingsViewModel : ViewModel
    {
        public SettingsViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.CutsceneLevel, viewName)
        {

        }

        public void ToggleFullscreen()
        {
            DisplayMode.Value = (DisplayMode.Value == "Windowed" ? "Fullscreen" : "Windowed");
        }

        public void Apply()
        {
            Settings.SetProgramSetting<float>("SoundVolume", SoundVolume.Value);
            Settings.SetProgramSetting<float>("MusicVolume", MusicVolume.Value);
            Audio.ApplySettings();

            bool newFullscreen = DisplayMode.Value == "Fullscreen";
            bool oldFullscreen = Settings.GetProgramSetting<bool>("Fullscreen");
            Settings.SetProgramSetting<bool>("Fullscreen", DisplayMode.Value == "Fullscreen");
            if (newFullscreen != oldFullscreen)
            {
                CrossPlatformGame.GameInstance.ApplySettings();
                (parentScene as TitleScene).ResetSettings();
            }
        }

        public void Back()
        {
            Close();
        }

        public ModelProperty<string> DisplayMode { get; set; } = new ModelProperty<string>(Settings.GetProgramSetting<bool>("Fullscreen") ? "Fullscreen" : "Windowed");
        public ModelProperty<float> SoundVolume { get; set; } = new ModelProperty<float>(Settings.GetProgramSetting<float>("SoundVolume"));
        public ModelProperty<float> MusicVolume { get; set; } = new ModelProperty<float>(Settings.GetProgramSetting<float>("MusicVolume"));
    }
}
