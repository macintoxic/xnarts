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
        //ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont font;

        public Texture2D unitImage;
        public Texture2D unitAttack;
        public Texture2D unitAvatar;
        public Texture2D friendlySelect;
        public Texture2D enemySelect;

        #region PRIVATE_TEXTURES
        /***********************/
        private Texture2D select;
        private Texture2D upArrow;
        private Texture2D downArrow;
        private Texture2D shiftKey;
        private Texture2D ctrlKey;
        private Texture2D spaceKey;
        private Texture2D HUD;
        private Texture2D HUDLine;
        private Texture2D mouse;
        /***********************/
        #endregion

        //public List<Unit> startingUnits;   // May want to use at some point to clean up dead units in "units"
        public List<Unit> units;
        public List<Unit> friendly;
        public List<Unit> enemy;
        public List<Unit> selected;

        private Boolean pause;
        private Boolean splashScreen;
        private Boolean mousePressed;
        private int startMouseX;
        private int startMouseY;
        private int mouseX;
        private int mouseY;

        public StratGame()
        {
            graphics = new GraphicsDeviceManager(this);
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
            mousePressed = false;
            startMouseX = Program.NaN;
            startMouseY = Program.NaN;
            mouseX = Program.NaN;
            mouseY = Program.NaN;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            unitImage = Content.Load<Texture2D>("Basic_Unit");
            friendlySelect = Content.Load<Texture2D>("Friendly_Select");
            enemySelect = Content.Load<Texture2D>("Enemy_Select");
            select = Content.Load<Texture2D>("Select");
            upArrow = Content.Load<Texture2D>("Keys/UpKey");
            downArrow = Content.Load<Texture2D>("Keys/DownKey");
            shiftKey = Content.Load<Texture2D>("Keys/ShiftKey");
            ctrlKey = Content.Load<Texture2D>("Keys/CtrlKey");
            spaceKey = Content.Load<Texture2D>("Keys/SpaceKey");
            HUD = Content.Load<Texture2D>("HUD/HUDBack");
            HUDLine = Content.Load<Texture2D>("HUD/HUDBack2");
            mouse = Content.Load<Texture2D>("HUD/Mouse");
            unitAvatar = Content.Load<Texture2D>("HUD/Basic_Avatar");
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

            mouseX = mstate.X;
            mouseY = mstate.Y;

            if (splashScreen)
            {
                updateSplashScreen(kstate);
                return;
            }

            if (!pause && kstate.IsKeyDown(Keys.P)) pause = true;
            if (pause && kstate.IsKeyDown(Keys.Enter)) { pause = false; return; }
            if (pause || gameOver()) return;
            
            // Units are selected or deselected when the left mouse button is pressed
            if (mstate.LeftButton == ButtonState.Pressed && !mousePressed)
            {
                leftMousePress(mstate, kstate);
            }
            else if (mstate.RightButton == ButtonState.Pressed && !mousePressed)
            {                           // Targets are selected for current units with the right mouse button.
                rightMousePress(mstate, kstate);
            }
            else if (mousePressed &&
                     mstate.LeftButton != ButtonState.Pressed && 
                     mstate.RightButton != ButtonState.Pressed)
            {                               // if this 'else if' is kept in, can't drag units around
                if (startMouseX != Program.NaN)
                {
                    if (!kstate.IsKeyDown(Keys.RightControl) && !kstate.IsKeyDown(Keys.LeftControl))
                        selected.Clear();
                    selectUnitBox(startMouseX, startMouseY, mouseX, mouseY, kstate);
                    startMouseX = Program.NaN;
                    startMouseY = Program.NaN;
                }
                mousePressed = false;
            }

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
                drawSplashScreen();
                spriteBatch.End();
                return;
            }

            // Draw all the units first
            foreach (Unit unit in units)
                unit.Draw(gameTime, spriteBatch);
            
            // Now draw indicators around enemy units
            foreach (Unit enemyUnit in enemy)
            {
                if (enemyUnit.dead) continue;
                spriteBatch.Draw(enemySelect,
                    new Vector2(enemyUnit.location.X - (enemyUnit.image.Width / 2),
                        enemyUnit.location.Y - (enemyUnit.image.Height / 2)), Color.White);
            }

            // Draw indicators around selected units
            foreach (Unit selectedUnit in selected)
            {
                if (selectedUnit.dead) continue;
                spriteBatch.Draw(friendlySelect,
                    new Vector2(selectedUnit.location.X - (selectedUnit.image.Width / 2),
                        selectedUnit.location.Y - (selectedUnit.image.Height / 2)), Color.White);
            }
            
            // Deal with the mouse and the HUD and stuff
            if (startMouseX != Program.NaN)
                drawSelectBox();

            drawHUD();

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

            spriteBatch.End();
            /*** SpriteBatch Ends ***/

            base.Draw(gameTime);
        }


        /****************** Mouse Click Update Helpers **********************************/

        private void leftMousePress(MouseState mstate, KeyboardState kstate)
        {
            Boolean ctrlHeld = kstate.IsKeyDown(Keys.LeftControl) || kstate.IsKeyDown(Keys.RightControl);
            if (kstate.IsKeyDown(Keys.LeftShift) || kstate.IsKeyDown(Keys.RightShift))
            {
                startMouseX = mouseX;
                startMouseY = mouseY;
            }
            else if (mouseY <= Program.SCREENHEIGHT - Program.HUDHEIGHT)
            {
                if (!ctrlHeld)
                    selected.Clear();                       // can select multiple units if ctrl is held
                foreach (Unit unit in friendly)
                {
                    if (unit.dead) continue;
                    if (mouseX <= unit.location.X + (unit.image.Width / 2) &&
                        mouseX >= unit.location.X - (unit.image.Width / 2) &&
                        mouseY <= unit.location.Y + (unit.image.Height / 2) &&
                        mouseY >= unit.location.Y - (unit.image.Height / 2))
                    {
                        if (ctrlHeld && selected.Contains(unit))
                            selected.Remove(unit);      // Can de-select units with Control
                        else
                            selected.Add(unit);
                        break;
                    }
                }
            }
            else
            {
                int x = 275;
                int y = Program.SCREENHEIGHT - Program.HUDHEIGHT + 35;
                foreach (Unit unit in selected)
                {
                    if (unit.dead) continue;
                    if (x >= Program.SCREENWIDTH - 275)
                    {
                        x = 275;
                        y += 70;
                    }
                    if (mouseX < x + 64 && mouseX > x &&
                        mouseY < y + 64 && mouseY > y)
                    {
                        if (ctrlHeld)
                            selected.Remove(unit);      // Can de-select units with Control
                        else
                        {
                            selected.Clear();
                            selected.Add(unit);
                        }
                        break;
                    }
                    x += 70;
                }
            }
            mousePressed = true;
        }

        private void rightMousePress(MouseState mstate, KeyboardState kstate)
        {
            if (mouseY <= Program.SCREENHEIGHT - Program.HUDHEIGHT)
            {
                Unit target = null;
                foreach (Unit enemyUnit in enemy)
                {
                    if (enemyUnit.dead) continue;
                    /*if (mouseX <= enemyUnit.location.X + (enemyUnit.image.Width / 2) &&
                        mouseX >= enemyUnit.location.X - (enemyUnit.image.Width / 2) &&
                        mouseY <= enemyUnit.location.Y + (enemyUnit.image.Height / 2) &&
                        mouseY >= enemyUnit.location.Y - (enemyUnit.image.Height / 2))*/
                    if (Vector2.Distance(new Vector2(mouseX, mouseY),
                    new Vector2(enemyUnit.location.X, enemyUnit.location.Y)) <= enemyUnit.image.Height / 2 + 1)
                    {
                        target = enemyUnit;
                    }
                }
                foreach (Unit selectedUnit in selected)
                {
                    selectedUnit.AIon = false;
                    if (selectedUnit.dead) continue;
                    if (target != null)
                    {
                        if (kstate.IsKeyDown(Keys.LeftShift) || kstate.IsKeyDown(Keys.RightShift))
                        {
                            if (selectedUnit.currentTarget == null)
                                selectedUnit.currentTarget = target;
                            else
                                selectedUnit.targetQueue.Enqueue(target);
                        }
                        else
                        {
                            selectedUnit.targetQueue.Clear();
                            selectedUnit.currentTarget = target;
                        }
                    }
                    else
                    {
                        selectedUnit.targetQueue.Clear();
                        selectedUnit.currentTarget = null;
                        selectedUnit.destination = new Vector2(mouseX, mouseY);
                    }
                }
            }
            mousePressed = true;
        }

        private void selectUnitBox(int startX, int startY, int finishX, int finishY, KeyboardState kstate)
        {
            foreach (Unit unit in friendly)
            {
                // If this unit is dead, forget about it, and if we are doing Add-to-Selected (CTRL), 
                // we only want to add the unit if it is not already selected.
                if (unit.dead || ((kstate.IsKeyDown(Keys.LeftControl)
                              || kstate.IsKeyDown(Keys.RightControl)) && selected.Contains(unit)))
                    continue;

                // If our box includes the unit, add it to selected.
                int locXmin = (int)unit.location.X - (unit.image.Width / 2);
                int locXmax = (int)unit.location.X + (unit.image.Width / 2);
                int locYmin = (int)unit.location.Y - (unit.image.Height / 2);
                int locYmax = (int)unit.location.Y + (unit.image.Height / 2);
                if ((locXmin <= startX && locXmax >= finishX && locYmin <= startY && locYmax >= finishY) ||
                    (locXmin <= startX && locXmax >= finishX && locYmax >= startY && locYmin <= finishY) ||
                    (locXmax >= startX && locXmin <= finishX && locYmin <= startY && locYmax >= finishY) ||
                    (locXmax >= startX && locXmin <= finishX && locYmax >= startY && locYmin <= finishY))
                {
                    selected.Add(unit);
                }
            }
        }


        /******************** Drawing Helpers *******************************************/

        private void drawSelectBox()
        {
            if (startMouseX < mouseX)
            {
                for (int i = startMouseX; i <= mouseX; i += 3)
                {
                    spriteBatch.Draw(select, new Vector2(i, startMouseY), Color.Green);
                    spriteBatch.Draw(select, new Vector2(i, mouseY), Color.Green);
                }
            }
            else
            {
                for (int i = mouseX; i <= startMouseX; i += 3)
                {
                    spriteBatch.Draw(select, new Vector2(i, startMouseY), Color.Green);
                    spriteBatch.Draw(select, new Vector2(i, mouseY), Color.Green);
                }
            }
            if (startMouseY < mouseY)
            {
                for (int j = startMouseY; j <= mouseY; j += 3)
                {
                    spriteBatch.Draw(select, new Vector2(startMouseX, j), Color.Green);
                    spriteBatch.Draw(select, new Vector2(mouseX, j), Color.Green);
                }
            }
            else
            {
                for (int j = mouseY; j <= startMouseY; j += 3)
                {
                    spriteBatch.Draw(select, new Vector2(startMouseX, j), Color.Green);
                    spriteBatch.Draw(select, new Vector2(mouseX, j), Color.Green);
                }
            }
        }

        private void drawHUD()
        {
            /**************** Draw the HUD's background **************************/
            for (int i = 0; i <= Program.SCREENWIDTH; i += 10)
            {
                if (i == 250 || i == Program.SCREENWIDTH - 250)
                    spriteBatch.Draw(HUDLine, new Vector2(i, Program.SCREENHEIGHT - Program.HUDHEIGHT), Color.White);
                else
                    spriteBatch.Draw(HUD, new Vector2(i, Program.SCREENHEIGHT - Program.HUDHEIGHT), Color.White);
            }

            /**************** Draw selected Units' avatars ***********************/
            drawSelectedAvatars();

            /****************** Draw Arrow Controls ******************************/
            spriteBatch.Draw(upArrow,
                new Vector2(Program.SCREENWIDTH - 45, Program.SCREENHEIGHT - 90), Color.White);
            spriteBatch.Draw(downArrow,
                new Vector2(Program.SCREENWIDTH - 45, Program.SCREENHEIGHT - 45), Color.White);
            spriteBatch.DrawString(font, "Attack Nearest",
                new Vector2(Program.SCREENWIDTH - 154, Program.SCREENHEIGHT - 83), Color.Black);
            spriteBatch.DrawString(font, "Stop",
                new Vector2(Program.SCREENWIDTH - 83, Program.SCREENHEIGHT - 42), Color.Black);

            /****************** Draw Select Controls *****************************/
            spriteBatch.Draw(shiftKey,
                new Vector2(4, Program.SCREENHEIGHT - 124), Color.White);
            spriteBatch.Draw(ctrlKey,
                new Vector2(4, Program.SCREENHEIGHT - 85), Color.White);
            spriteBatch.Draw(spaceKey,
                new Vector2(4, Program.SCREENHEIGHT - 44), Color.White);
            spriteBatch.DrawString(font, "Drag-Select / EnQueue",
                new Vector2(85, Program.SCREENHEIGHT - 118), Color.Black);
            spriteBatch.DrawString(font, "Add / Remove Unit",
                new Vector2(84, Program.SCREENHEIGHT - 79), Color.Black);
            spriteBatch.DrawString(font, "Select All",
                new Vector2(129, Program.SCREENHEIGHT - 38), Color.Black);

            /************* Draw Mouse ***********************/
            spriteBatch.Draw(mouse, new Vector2(mouseX - 16, mouseY - 16), Color.White);
        }

        private void drawSelectedAvatars()
        {
            int x = 263;
            int y = Program.SCREENHEIGHT - Program.HUDHEIGHT + 20;
            foreach (Unit selectedUnit in selected)
            {
                if (selectedUnit.dead) continue;
                if (x >= Program.SCREENWIDTH - 275)
                {
                    x = 263;
                    y += 72;
                }
                spriteBatch.Draw(selectedUnit.avatar, new Vector2(x, y), null, Color.White, 0.0f,
                    new Vector2(0, 0), 1.0f, new SpriteEffects(), 0.0f);
                if (selectedUnit.targetQueue.Count > 0)
                {
                    spriteBatch.Draw(select, new Vector2(x + 10, y + 60), Color.Red);
                    spriteBatch.Draw(select, new Vector2(x + 10, y + 57), Color.Red);
                    spriteBatch.Draw(select, new Vector2(x + 10, y + 63), Color.Red);
                    spriteBatch.Draw(select, new Vector2(x + 7, y + 60), Color.Red);
                    spriteBatch.Draw(select, new Vector2(x + 13, y + 60), Color.Red);
                }
                else if (selectedUnit.currentTarget == null && selectedUnit.destination.X == Program.NaN)
                {
                    spriteBatch.Draw(select, new Vector2(x + 30, y + 60), Color.Brown);
                    spriteBatch.Draw(select, new Vector2(x + 27, y + 60), Color.Brown);
                    spriteBatch.Draw(select, new Vector2(x + 33, y + 60), Color.Brown);
                }
                if (selectedUnit.AIon)
                {
                    spriteBatch.Draw(select, new Vector2(x + 50, y + 60), Color.Blue);
                    spriteBatch.Draw(select, new Vector2(x + 50, y + 57), Color.Blue);
                    spriteBatch.Draw(select, new Vector2(x + 50, y + 63), Color.Blue);
                    spriteBatch.Draw(select, new Vector2(x + 47, y + 60), Color.Blue);
                    spriteBatch.Draw(select, new Vector2(x + 53, y + 60), Color.Blue);
                }
                for (int i = 0; i <= selectedUnit.healthPercentage() / 2; i += 5)
                {
                    if (i < 10)
                        spriteBatch.Draw(select, new Vector2(x + i + 5, y + 2), Color.Green);
                    else if (i <= 25)
                        spriteBatch.Draw(select, new Vector2(x + i + 5, y + 2), Color.SeaGreen);
                    //else if (i <= 30)
                    //    spriteBatch.Draw(select, new Vector2(x + i + 5, y + 2), Color.YellowGreen);
                    else if (i <= 35)
                        spriteBatch.Draw(select, new Vector2(x + i + 5, y + 2), Color.MediumTurquoise);
                    else
                        spriteBatch.Draw(select, new Vector2(x + i + 5, y + 2), Color.SpringGreen);
                }
                x += 70;
            }
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

        private void updateSplashScreen(KeyboardState kstate)
        {
            if (kstate.IsKeyDown(Keys.Enter))
            {
                splashScreen = false;
                postSplashInitialize();
            }
        }

        private void drawSplashScreen()
        {
            spriteBatch.DrawString(font, "Press 'Enter' to start...", 
                new Vector2(Program.SCREENWIDTH / 2, Program.SCREENHEIGHT / 2), Color.White);
        }

        private void postSplashInitialize()
        {
            units = new List<Unit>();
            selected = new List<Unit>();
            friendly = new List<Unit>();
            enemy = new List<Unit>();

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
