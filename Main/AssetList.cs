namespace Texemon.Main
{
    public enum GameView
    {
        ConversationScene_ConversationView,
        CreditsScene_CreditsView,
        MapScene_ItemView,
        MapScene_MapView,
        MapScene_MenuView,
        MatchScene_GameOverView,
        MatchScene_GameView,
        MatchScene_ItemView,
        TitleScene_SettingsView,
        TitleScene_TitleView,

        None = -1
    }

    public enum GameSound
    {
        a_mainmenuconfirm,
        a_mainmenuselection,
        Back,
        block_dissapear,
        block_junk_break,
        block_match,
        block_swap,
        Bounce,
        clear,
        combo,
        Confirm,
        Counter,
        Cursor,
        damange_blue,
        damange_cyan,
        damange_generic,
        damange_green,
        damange_red,
        damange_yellow,
        dialogue_auto_scroll,
        enemy_action,
        enemy_encounter,
        Error,
        game_over,
        menu_cursor_change,
        menu_select,
        move_selection_cursor,
        Pickup,
        Selection,
        shink,
        Talk,
        tink,
        wall_bump,
        wall_enter,

        None = -1
    }

    public enum GameMusic
    {
        SMP_BTL,
        SMP_DUN,
        SMP_TNS,
        SMP_TTL,

        None = -1
    }

    public enum GameData
    {
        ConversationData,
        EnemyData,
        ItemData,

        None = -1
    }

    public enum GameShader
    {
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
        BlueBlock,
        CyanBlock,
        GreenBlock,
        JunkBlock,
        MatchBlock,
        MatchCursor,
        MatchTiles,
        MiniMap,
        RedBlock,
        SpellBlue,
        SpellCyan,
        SpellGreen,
        SpellRed,
        SpellYellow,
        YellowBlock,
        YouAreHere,
        Actors_Alien1,
        Actors_Alien2,
        Actors_Alien3,
        Actors_Blank,
        Actors_Principal,
        Actors_Student,
        Actors_Teacher,
        Background_Ponsona,
        Background_Splash,
        Particles_attackOrb_blue,
        Particles_attackOrb_cyan,
        Particles_attackOrb_green,
        Particles_attackOrb_red,
        Particles_attackOrb_yellow,
        Particles_chain_indicator,
        Particles_combo_indicator,
        Particles_DamageDigits,
        Particles_GunSpark,
        Particles_Slash,
        Particles_Star,
        Particles_ui_digits,
        Particles_ui_immune_indicator,
        Particles_ui_resist_indicator,
        Particles_ui_weak_indicator,
        Walls_BlackboardClassroomWall,
        Walls_Blank,
        Walls_ClassroomFloor,
        Walls_ClassroomWall,
        Walls_DoorClassroomWall,
        Walls_DoorFoyerWall,
        Walls_DoubleDoorFoyerWall,
        Walls_Foundry_C_Base,
        Walls_Foundry_C_Lit,
        Walls_Foundry_D_Base,
        Walls_Foundry_D_BaseDark,
        Walls_Foundry_D_SPC,
        Walls_Foundry_F_Base,
        Walls_Foundry_F_Lit,
        Walls_Foundry_W_Base,
        Walls_Foundry_W_BaseFlat,
        Walls_Foundry_W_Dark,
        Walls_Foundry_W_DarkFlat,
        Walls_Foundry_W_DarkPipes,
        Walls_Foundry_W_DarkPipesB,
        Walls_FoyerFloor,
        Walls_LightCeiling,
        Walls_LockerFoyerWall,
        Walls_Office_C_Base,
        Walls_Office_C_Hole,
        Walls_Office_C_HoleB,
        Walls_Office_C_HoleC,
        Walls_Office_C_Light,
        Walls_Office_D_Base,
        Walls_Office_D_Overgrown,
        Walls_Office_D_SPC,
        Walls_Office_F_Base,
        Walls_Office_F_Petals,
        Walls_Office_F_PetalsB,
        Walls_Office_W_Base,
        Walls_Office_W_PL1,
        Walls_Office_W_W1,
        Walls_Office_W_W2,
        Walls_PlainCeiling,
        Walls_PlainFoyerWall,
        Walls_WindowClassroomWall,
        Walls_WindowFoyerWall,
        Widgets_Buttons_GamePanel,
        Widgets_Buttons_GamePanelOpaque,
        Widgets_Buttons_GamePanelSelected,
        Widgets_Buttons_iconplate_square1_black_25,
        Widgets_Buttons_iconplate_square1_brown_25,
        Widgets_Buttons_iconplate_square1_white_25,
        Widgets_Buttons_iconplate_square2_black_25,
        Widgets_Buttons_iconplate_square2_brown_25,
        Widgets_Buttons_iconplate_square2_white_25,
        Widgets_Buttons_kw_buttonF_black_h69,
        Widgets_Buttons_kw_buttonF_black_on_h69,
        Widgets_Buttons_kw_buttonF_blue_h69,
        Widgets_Buttons_kw_buttonF_blue_on_h69,
        Widgets_Buttons_kw_buttonF_green_h69,
        Widgets_Buttons_kw_buttonF_green_on_h69,
        Widgets_Buttons_kw_buttonF_orange_h69,
        Widgets_Buttons_kw_buttonF_orange_on_h69,
        Widgets_Buttons_kw_buttonF_pink_h69,
        Widgets_Buttons_kw_buttonF_pink_on_h69,
        Widgets_Buttons_kw_buttonF_white_h69,
        Widgets_Buttons_kw_buttonF_white_on_h69,
        Widgets_Buttons_sabwindowE_pink_66,
        Widgets_Buttons_sabwindowE_white_66,
        Widgets_Buttons_sabwinndou_oter10_90,
        Widgets_Buttons_sabwinndou_oter6_wite_90,
        Widgets_Buttons_sabwinndou_oter9_90,
        Widgets_Buttons_sabwinndou_other10_45,
        Widgets_Buttons_sabwinndou_other6_white_45,
        Widgets_Buttons_sabwinndou_other7_45,
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
        Widgets_Icons_icon_bow_25,
        Widgets_Icons_icon_robe_25,
        Widgets_Images_Proceed,
        Widgets_Images_Settings,
        Widgets_Images_SwapAction,
        Widgets_Textplate_GamePanel,
        Widgets_Textplate_GamePanelOpaque,
        Widgets_Textplate_textplateD_black_h69,
        Widgets_Textplate_textplateD_blue_h69,
        Widgets_Textplate_textplateD_green_h69,
        Widgets_Textplate_textplateD_orange_h69,
        Widgets_Textplate_textplateD_pink_h69,
        Widgets_Textplate_textplateD_white_h69,
        Widgets_Windows_GamePanel,
        Widgets_Windows_GamePanelOpaque,
        Widgets_Windows_Main,
        Widgets_Windows_mainwindowH_black_240,
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
        City,
        Class1,
        Class2,
        Class3,
        Class4,
        Class5,
        Class6,
        Class7,
        Classroom1,
        Classroom2,
        Classroom3,
        Classroom4,
        Classroom5,
        Classroom6,
        Classroom7,
        Foundry,
        Office,
        School,
        SchoolFoyer,
        SchoolOrigin,
        Walls,
        Tilesets_FDE_Blacksmith,
        Tilesets_FDE_Marshland,
        Tilesets_FDE_Winter_Village,
        Tilesets_FD_Caves,
        Tilesets_FD_City,
        Tilesets_FD_Desert,
        Tilesets_FD_Dungeon,
        Tilesets_FD_Forest,
        Tilesets_FD_Grasslands,
        Tilesets_FD_Interior,
        Tilesets_FD_Mountains,
        Tilesets_FD_Village,

        None = -1
    }

}
