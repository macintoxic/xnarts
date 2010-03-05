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
    public class HUD
    {
        private StratGame game;

        public HUD(StratGame g)
        {
            game = g;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            /**************** Draw the HUD's background **************************/
            for (int i = 0; i <= Program.SCREENWIDTH; i += 10)
            {
                if (i == 250 || i == Program.SCREENWIDTH - 250)
                {
                    spriteBatch.Draw(game.textures.HUD_line,
                        new Vector2(i, Program.SCREENHEIGHT - Program.HUDHEIGHT), Color.White);
                }
                else
                {
                    spriteBatch.Draw(game.textures.HUD_background,
                        new Vector2(i, Program.SCREENHEIGHT - Program.HUDHEIGHT), Color.White);
                }
            }
            /**************** Draw selected Units' avatars ***********************/
            drawSelectedAvatars(spriteBatch);

            /****************** Draw Arrow Controls ******************************/
            spriteBatch.Draw(game.textures.keys_up,
                new Vector2(Program.SCREENWIDTH - 45, Program.SCREENHEIGHT - 90), Color.White);
            spriteBatch.Draw(game.textures.keys_down,
                new Vector2(Program.SCREENWIDTH - 45, Program.SCREENHEIGHT - 45), Color.White);
            spriteBatch.DrawString(game.font, "Attack Nearest",
                new Vector2(Program.SCREENWIDTH - 154, Program.SCREENHEIGHT - 83), Color.Black);
            spriteBatch.DrawString(game.font, "Stop",
                new Vector2(Program.SCREENWIDTH - 83, Program.SCREENHEIGHT - 42), Color.Black);

            /****************** Draw Select Controls *****************************/
            spriteBatch.Draw(game.textures.keys_shift,
                new Vector2(4, Program.SCREENHEIGHT - 124), Color.White);
            spriteBatch.Draw(game.textures.keys_ctrl,
                new Vector2(4, Program.SCREENHEIGHT - 85), Color.White);
            spriteBatch.Draw(game.textures.keys_space,
                new Vector2(4, Program.SCREENHEIGHT - 44), Color.White);
            spriteBatch.DrawString(game.font, "Drag-Select / EnQueue",
                new Vector2(85, Program.SCREENHEIGHT - 118), Color.Black);
            spriteBatch.DrawString(game.font, "Add / Remove Unit",
                new Vector2(84, Program.SCREENHEIGHT - 79), Color.Black);
            spriteBatch.DrawString(game.font, "Select All",
                new Vector2(129, Program.SCREENHEIGHT - 38), Color.Black);
        }

        public void drawSplashScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(game.font, "Press 'Enter' to start...",
                new Vector2(Program.SCREENWIDTH / 2, Program.SCREENHEIGHT / 2), Color.White);
        }

        // Post: Returns false if the splash screen is done
        public bool updateSplashScreen(KeyboardState kstate)
        {
            if (kstate.IsKeyDown(Keys.Enter))
            {
                game.postSplashInitialize();
                return false;
            }
            return true;
        }

        private void drawSelectedAvatars(SpriteBatch spriteBatch)
        {
            int x = 263;
            int y = Program.SCREENHEIGHT - Program.HUDHEIGHT + 20;
            foreach (Unit selectedUnit in game.selected)
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
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 10, y + 60), Color.Red);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 10, y + 57), Color.Red);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 10, y + 63), Color.Red);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 7, y + 60), Color.Red);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 13, y + 60), Color.Red);
                }
                else if (selectedUnit.currentTarget == null && selectedUnit.destination.X == Program.NaN)
                {
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 30, y + 60), Color.Brown);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 27, y + 60), Color.Brown);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 33, y + 60), Color.Brown);
                }
                if (selectedUnit.AIon)
                {
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 50, y + 60), Color.Blue);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 50, y + 57), Color.Blue);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 50, y + 63), Color.Blue);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 47, y + 60), Color.Blue);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + 53, y + 60), Color.Blue);
                }
                for (int i = 0; i <= selectedUnit.healthPercentage() / 2; i += 5)
                {
                    if (i < 10)
                        spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + i + 5, y + 2), Color.Green);
                    else if (i <= 25)
                        spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + i + 5, y + 2), Color.SeaGreen);
                    //else if (i <= 30)
                    //    spriteBatch.Draw(select, new Vector2(x + i + 5, y + 2), Color.YellowGreen);
                    else if (i <= 35)
                        spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + i + 5, y + 2), Color.MediumTurquoise);
                    else
                        spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(x + i + 5, y + 2), Color.SpringGreen);
                }
                x += 70;
            }
        }
    }
}
