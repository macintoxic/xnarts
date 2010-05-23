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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class StratGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public TextureManager textures;
        public MouseManager mouse;
        public Drawer drawer;
        public HUD hud;

        //ContentManager content;
        private SpriteBatch spriteBatch;
        public SpriteFont font { get; private set; }

        //public List<Unit> startingUnits;   // May want to use at some point to clean up dead units in "units"
        public List<Unit> units;
        public List<Unit> friendly;
        public List<Unit> enemy;
        public List<Unit> selected;

        private bool pause;
        private bool splashScreen;



        public StratGame()
        {
            graphics = new GraphicsDeviceManager(this);
            textures = new TextureManager();
            hud = new HUD(this);
            Content.RootDirectory = "Content";
            
            //Program.SCREENWIDTH = Window.ClientBounds.Width;
            //Program.SCREENHEIGHT = Window.ClientBounds.Height;
            graphics.PreferredBackBufferWidth = Program.SCREENWIDTH;
            graphics.PreferredBackBufferHeight = Program.SCREENHEIGHT;
            //graphics.IsFullScreen = true;
            splashScreen = true;
            //content = new ContentManager(Services);
            //Window.AllowUserResizing = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            pause = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            textures.LoadContent(Content);
            font = Content.Load<SpriteFont>("Fonts/font1");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mstate = Mouse.GetState();
            KeyboardState kstate = Keyboard.GetState();
            if ((kstate.IsKeyDown(Keys.RightControl) || 
                 kstate.IsKeyDown(Keys.LeftControl)) && kstate.IsKeyDown(Keys.Q))
                this.Exit();

            if (splashScreen)
            {
                splashScreen = hud.updateSplashScreen(kstate);
                return;
            }

            mouse.Update(gameTime, mstate, kstate);

            if (!pause && kstate.IsKeyDown(Keys.P)) pause = true;
            if (pause && kstate.IsKeyDown(Keys.Enter)) { pause = false; return; }
            if (pause || gameOver()) return;

            // Space selects all friendly units
            if (kstate.IsKeyDown(Keys.Space))
            {
                selected.Clear();
                foreach (Unit unit in friendly)
                    selected.Add(unit);
            }

            foreach (Unit unit in units)
                unit.Update(gameTime);

            handleAI(kstate);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            /*** SpriteBatch Begins ***/
            spriteBatch.Begin();

            if (splashScreen)
            {
                hud.drawSplashScreen(spriteBatch);
                spriteBatch.End();
                return;
            }

            drawer.Draw(gameTime, spriteBatch);
            
            // Deal with the mouse and the HUD and stuff
            mouse.drawSelectBox(spriteBatch);

            hud.Draw(gameTime, spriteBatch);

            if (gameOver())
            {
                spriteBatch.DrawString(font, "GAME OVER", 
                    new Vector2(Program.SCREENWIDTH / 2 - 65, Program.SCREENHEIGHT / 2 - 40), 
                    Color.Red, 0.0f, new Vector2(0, 0), 1.5f, new SpriteEffects(), 0.0f);
            }
            else if (pause)
            {
                spriteBatch.DrawString(font, "PAUSED",
                    new Vector2(Program.SCREENWIDTH / 2 - 50, Program.SCREENHEIGHT / 2 - 61),
                    Color.Blue, 0.0f, new Vector2(0, 0), 1.5f, new SpriteEffects(), 0.0f);
                spriteBatch.DrawString(font, "Press 'enter' to resume game...",
                    new Vector2(Program.SCREENWIDTH / 2 - 111, Program.SCREENHEIGHT / 2 - 10), Color.PowderBlue);
            }

            mouse.Draw(spriteBatch);

            spriteBatch.End();
            /*** SpriteBatch Ends ***/

            base.Draw(gameTime);
        }


        /****************** Update Helpers **********************************************/

        private void handleAI(KeyboardState kstate)
        {
            if (kstate.IsKeyDown(Keys.Down))
            {
                foreach (Unit unit in selected)
                {
                    unit.AIon = false;
                    unit.currentTarget = null;
                    unit.targetQueue.Clear();
                    unit.destination = new Vector2(Program.NaN, Program.NaN);
                }
            }
            else if (kstate.IsKeyDown(Keys.Up))
            {
                foreach (Unit unit in selected)
                    unit.AIon = true;
            }
        }

        private Boolean gameOver()
        {
            foreach (Unit unit in friendly)
            {
                if (!unit.dead) return false;
            }
            return true;
        }
        

        /********* Splash Screen and Initializer Helpers ********************************/

        public void postSplashInitialize()
        {
            units = new List<Unit>();
            selected = new List<Unit>();
            friendly = new List<Unit>();
            enemy = new List<Unit>();

            mouse = new MouseManager(this);
            drawer = new Drawer(this);

            for (int i = 50; i <= 400; i += 50)
            {
                Unit unit = new Unit(this, null,
                    null, null, 20, 1, 2, 50, false, new Vector2(200, i));
                units.Add(unit);
                friendly.Add(unit);
                unit = new Unit(this, null,
                    null, null, 20, 1, 2, 50, true, new Vector2(Program.SCREENWIDTH - 200, i));
                units.Add(unit);
                enemy.Add(unit);
            }
        }
    }
}
