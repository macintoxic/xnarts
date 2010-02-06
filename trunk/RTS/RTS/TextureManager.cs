using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace RTS
{
    public class TextureManager
    {
        #region TEXTURE_SECTION
        /*********************/
        public Texture2D unit_default_image     { get; private set; }
        public Texture2D unit_default_attack    { get; private set; }
        public Texture2D unit_default_avatar    { get; private set; }
        public Texture2D keys_up                { get; private set; }
        public Texture2D keys_down              { get; private set; }
        public Texture2D keys_shift             { get; private set; }
        public Texture2D keys_ctrl              { get; private set; }
        public Texture2D keys_space             { get; private set; }
        public Texture2D HUD_background         { get; private set; }
        public Texture2D HUD_line               { get; private set; }
        public Texture2D HUD_friendlySelect     { get; private set; }
        public Texture2D HUD_enemySelect        { get; private set; }
        public Texture2D mouse                  { get; private set; }
        public Texture2D blankBoxSmall          { get; private set; }
        /*********************/
        #endregion

        public TextureManager()
        {
            unit_default_image    = null;
            unit_default_attack   = null;
            unit_default_avatar   = null;
            keys_up               = null;
            keys_down             = null;
            keys_shift            = null;
            keys_ctrl             = null;
            keys_space            = null;
            HUD_background        = null;
            HUD_line              = null;
            HUD_friendlySelect    = null;
            HUD_enemySelect       = null;
            mouse                 = null;
            blankBoxSmall         = null;
        }

        public void LoadContent(ContentManager Content)
        {
            unit_default_image      = Content.Load<Texture2D>("Basic_Unit");
            unit_default_avatar     = Content.Load<Texture2D>("HUD/Basic_Avatar");
            blankBoxSmall           = Content.Load<Texture2D>("Select");
            keys_up                 = Content.Load<Texture2D>("Keys/UpKey");
            keys_down               = Content.Load<Texture2D>("Keys/DownKey");
            keys_shift              = Content.Load<Texture2D>("Keys/ShiftKey");
            keys_ctrl               = Content.Load<Texture2D>("Keys/CtrlKey");
            keys_space              = Content.Load<Texture2D>("Keys/SpaceKey");
            HUD_background          = Content.Load<Texture2D>("HUD/HUDBack");
            HUD_line                = Content.Load<Texture2D>("HUD/HUDBack2");
            HUD_friendlySelect      = Content.Load<Texture2D>("Friendly_Select");
            HUD_enemySelect         = Content.Load<Texture2D>("Enemy_Select");
            mouse                   = Content.Load<Texture2D>("HUD/Mouse");
            blankBoxSmall           = Content.Load<Texture2D>("Select");
        }
    }
}
