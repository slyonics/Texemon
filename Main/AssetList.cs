namespace Texemon.Main
{
    public enum GameView
    {
        BattleScene_BattleView,
        BattleScene_CommandView,
        BattleScene_TargetView,
        ConversationScene_ConversationView,
        ConversationScene_ConversationView2,
        ConversationScene_ConversationView3,
        CreditsScene_CreditsView,
        IntroScene_SelectionView,
        IntroScene_TechNameView,
        ShopScene_ConstructView,
        ShopScene_HeroNameView,
        TitleScene_SettingsView,
        TitleScene_TitleView,

        None = -1
    }

    public enum GameSound
    {
        a_mainmenuconfirm,
        a_mainmenuselection,
        Back,
        Blip,
        block_dissapear,
        block_junk_break,
        block_match,
        block_swap,
        Bounce,
        Chest,
        clear,
        combo,
        Confirm,
        Construct,
        Counter,
        Cursor,
        damange_blue,
        damange_cyan,
        damange_generic,
        damange_green,
        damange_red,
        damange_yellow,
        dialogue_auto_scroll,
        Encounter,
        EnemyDeath,
        enemy_action,
        enemy_encounter,
        Error,
        Fireball,
        game_over,
        GetItem,
        Heal,
        JoinParty,
        Laser,
        menu_cursor_change,
        menu_select,
        move_selection_cursor,
        Pickup,
        Selection,
        shink,
        Slash,
        Talk,
        Thunder,
        tink,
        wall_bump,
        wall_enter,

        None = -1
    }

    public enum GameMusic
    {
        Battle,
        NewDestinations,
        Selection,
        Wilderness,

        None = -1
    }

    public enum GameData
    {
        AbilityData,
        ClassData,
        ConversationData,
        EncounterData,
        EnemyData,
        HeroData,
        ItemData,
        ShopData,

        None = -1
    }

    public enum GameShader
    {
        BattleEnemy,
        BattlePlayer,
        ColorFade,
        Default,
        HeatDistortion,
        Portrait,
        Wall,
        WallPlus,

        None = -1
    }

    public enum GameSprite
    {
        Enter,
        Gamepad,
        Keyboard,
        mighty_palette,
        Actors_Android,
        Actors_Base,
        Actors_BlowtorchDrone,
        Actors_DeflectorDrone,
        Actors_DroneShadow,
        Actors_FutureChest,
        Actors_Inventor,
        Actors_RepairDrone,
        Actors_Sentinel,
        Actors_Snake,
        Actors_TaserDrone,
        Background_Blank,
        Background_Splash,
        Background_Title,
        Background_Trees,
        Enemies_ArcherF,
        Enemies_ArcherM,
        Enemies_Bee,
        Enemies_Beholder,
        Enemies_Cockatrice,
        Enemies_Commando,
        Enemies_DarkKnight,
        Enemies_Demon,
        Enemies_Diety,
        Enemies_Dragon,
        Enemies_Drone,
        Enemies_EarthSpirit,
        Enemies_Fairy,
        Enemies_FighterF,
        Enemies_FighterM,
        Enemies_FireSpirit,
        Enemies_Ghost,
        Enemies_GiantCrab,
        Enemies_Goblin,
        Enemies_Golem,
        Enemies_Griffin,
        Enemies_Guard,
        Enemies_Harpy,
        Enemies_Hawk,
        Enemies_Hydra,
        Enemies_Medusa,
        Enemies_Minotaur,
        Enemies_NinjaF,
        Enemies_NinjaM,
        Enemies_Ogre,
        Enemies_Owl,
        Enemies_Phoenix,
        Enemies_PoisonFlower,
        Enemies_Robot,
        Enemies_Samurai,
        Enemies_Scorpion,
        Enemies_Security,
        Enemies_SeekerBot,
        Enemies_Shogun,
        Enemies_Skeleton,
        Enemies_Slime,
        Enemies_Snake,
        Enemies_Squid,
        Enemies_Succubus,
        Enemies_ThiefF,
        Enemies_ThiefM,
        Enemies_Turtle,
        Enemies_Vampire,
        Enemies_WaterSpirit,
        Enemies_WhiteWizF,
        Enemies_WhiteWizM,
        Enemies_WindSpirit,
        Enemies_Wisp,
        Enemies_WizardF,
        Enemies_WizardM,
        Enemies_Wolf,
        Particles_Bash,
        Particles_BlueHeal,
        Particles_DamageDigits,
        Particles_Electric,
        Particles_Flame,
        Particles_GreenHeal,
        Particles_Gunshot,
        Particles_Shock,
        Particles_Slash,
        Particles_Smoke,
        Particles_Sparks,
        Particles_Star,
        Tiles_ColorTileA1,
        Tiles_ColorTileA2,
        Tiles_ColorTileA4,
        Tiles_ColorTileA5,
        Tiles_ColorTileB,
        Tiles_ColorTileC,
        Tiles_ColorTileD,
        Widgets_Blank,
        Widgets_LabelGlow,
        Widgets_MagicFrame,
        Widgets_MagicFrameSelected,
        Widgets_MagicLabel,
        Widgets_MagicSelected,
        Widgets_MagicWindow,
        Widgets_TechFrame,
        Widgets_TechFrameSelected,
        Widgets_TechLabel,
        Widgets_TechSelected,
        Widgets_TechWindow,
        Widgets_Buttons_ClearPanel,
        Widgets_Buttons_GamePanel,
        Widgets_Buttons_GamePanelOpaque,
        Widgets_Buttons_GamePanelSelected,
        Widgets_Buttons_Panel,
        Widgets_Buttons_SelectedPanel,
        Widgets_Buttons_Technology,
        Widgets_Gauges_gaugeF_bar_black_h45,
        Widgets_Gauges_gaugeF_bar_blue_h66,
        Widgets_Gauges_gaugeF_bar_pink_h45,
        Widgets_Gauges_gaugeF_bar_pink_h66,
        Widgets_Gauges_gaugeF_bar_white_h66,
        Widgets_Gauges_gaugeF_bg_h45,
        Widgets_Gauges_gaugeF_bg_h66,
        Widgets_Gauges_gaugeF_frame_h45,
        Widgets_Gauges_gaugeF_frame_h66,
        Widgets_Gauges_scrollbarA_gold_w24,
        Widgets_Gauges_ui_healthBar,
        Widgets_Gauges_ui_healthBarBackground,
        Widgets_Gauges_ui_healthBarInside,
        Widgets_Gauges_ui_thumbSlider,
        Widgets_Icons_Armor,
        Widgets_Icons_Blank,
        Widgets_Icons_Chest,
        Widgets_Icons_Defend,
        Widgets_Icons_Delay,
        Widgets_Icons_Fire,
        Widgets_Icons_Flee,
        Widgets_Icons_Gun,
        Widgets_Icons_Heal,
        Widgets_Icons_Lightning,
        Widgets_Icons_Shield,
        Widgets_Icons_Staff,
        Widgets_Icons_Sword,
        Widgets_Images_Proceed,
        Widgets_Images_Settings,
        Widgets_Images_SwapAction,
        Widgets_Textplate_ClearPanel,
        Widgets_Textplate_GamePanel,
        Widgets_Textplate_GamePanelOpaque,
        Widgets_Textplate_Panel,
        Widgets_Windows_ClearPanel,
        Widgets_Windows_GamePanel,
        Widgets_Windows_GamePanelOpaque,
        Widgets_Windows_Main,
        Widgets_Windows_mainwindowH_black_240,
        Widgets_Windows_Panel,
        Widgets_Windows_sabwindowA_black_45,
        Widgets_Windows_sabwindowA_black_66,
        Widgets_Windows_sabwindowA_blue_45,
        Widgets_Windows_sabwindowA_green_45,
        Widgets_Windows_sabwindowA_orange_45,
        Widgets_Windows_sabwindowA_pink_45,
        Widgets_Windows_sabwindowA_white_45,
        Widgets_Windows_Spiky,

        None = -1
    }

    public enum GameMap
    {
        HomeLab,
        Junkyard,
        TechHomeworld,
        Test,
        Tilesets_ColorTileA1,
        Tilesets_ColorTileA2,
        Tilesets_ColorTileA4,
        Tilesets_ColorTileA5,
        Tilesets_ColorTileB,
        Tilesets_ColorTileC,
        Tilesets_ColorTileD,

        None = -1
    }

}
