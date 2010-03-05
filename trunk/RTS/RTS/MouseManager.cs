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
    public class MouseManager
    {
        private StratGame game;
        //private MouseState mstate;
        //private KeyboardState kstate;
        public Boolean mousePressed { get; private set; }
        public int startMouseX      { get; private set; }
        public int startMouseY      { get; private set; }
        public int mouseX           { get; private set; }
        public int mouseY           { get; private set; }

        public MouseManager(StratGame g)
        {
            game = g;
            mousePressed = false;
            startMouseX = Program.NaN;
            startMouseY = Program.NaN;
            mouseX = Program.NaN;
            mouseY = Program.NaN;
        }

        public void Update(GameTime gameTime, MouseState mstate, KeyboardState kstate)
        {
            mouseX = mstate.X;
            mouseY = mstate.Y;

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
                        game.selected.Clear();
                    selectUnitBox(startMouseX, startMouseY, mouseX, mouseY, kstate);
                    startMouseX = Program.NaN;
                    startMouseY = Program.NaN;
                }
                mousePressed = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            /************* Draw Mouse ***********************/
            spriteBatch.Draw(game.textures.mouse, new Vector2(mouseX - 16, mouseY - 16), Color.White);
        }

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
                    game.selected.Clear();                       // can select multiple units if ctrl is held
                foreach (Unit unit in game.friendly)
                {
                    if (unit.dead) continue;
                    /*if (mouseX <= unit.location.X + (unit.image.Width / 2) &&
                        mouseX >= unit.location.X - (unit.image.Width / 2) &&
                        mouseY <= unit.location.Y + (unit.image.Height / 2) &&
                        mouseY >= unit.location.Y - (unit.image.Height / 2))*/
                    if (Vector2.Distance(new Vector2(mouseX, mouseY), unit.location) <= unit.image.Width / 2)
                    {
                        if (ctrlHeld && game.selected.Contains(unit))
                            game.selected.Remove(unit);      // Can de-select units with Control
                        else
                            game.selected.Add(unit);
                        break;
                    }
                }
            }
            else
            {
                int x = 275;
                int y = Program.SCREENHEIGHT - Program.HUDHEIGHT + 35;
                foreach (Unit unit in game.selected)
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
                            game.selected.Remove(unit);      // Can de-select units with Control
                        else
                        {
                            game.selected.Clear();
                            game.selected.Add(unit);
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
                foreach (Unit enemyUnit in game.enemy)
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
                foreach (Unit selectedUnit in game.selected)
                {
                    selectedUnit.AIon = false;
                    if (selectedUnit.dead) continue;
                    if (kstate.IsKeyDown(Keys.LeftShift) || kstate.IsKeyDown(Keys.RightShift))
                    {
                        if (target == null)
                        {
                            if (selectedUnit.destination.X == Program.NaN)
                            {
                                selectedUnit.destination.X = mouseX;
                                selectedUnit.destination.Y = mouseY;
                            }
                            else selectedUnit.targetQueue.Enqueue(new Coordinate(mouseX, mouseY));
                        }
                        else if (selectedUnit.currentTarget == null && selectedUnit.destination.X == Program.NaN)
                            selectedUnit.currentTarget = target;
                        else
                            selectedUnit.targetQueue.Enqueue(target);
                    }
                    else
                    {
                        selectedUnit.targetQueue.Clear();
                        selectedUnit.currentTarget = target;
                        if (target == null)
                        {
                            selectedUnit.destination.X = mouseX;
                            selectedUnit.destination.Y = mouseY;
                        }
                    }
                }
            }
            mousePressed = true;
        }

        private void selectUnitBox(int startX, int startY, int finishX, int finishY, KeyboardState kstate)
        {
            foreach (Unit unit in game.friendly)
            {
                // If this unit is dead, forget about it, and if we are doing Add-to-Selected (CTRL), 
                // we only want to add the unit if it is not already selected.
                if (unit.dead || ((kstate.IsKeyDown(Keys.LeftControl)
                              || kstate.IsKeyDown(Keys.RightControl)) && game.selected.Contains(unit)))
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
                    game.selected.Add(unit);
                }
            }
        }

        public void drawSelectBox(SpriteBatch spriteBatch)
        {
            if (startMouseX == Program.NaN) return;
            if (startMouseX < mouseX)
            {
                for (int i = startMouseX; i <= mouseX; i += 3)
                {
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(i, startMouseY), Color.Green);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(i, mouseY), Color.Green);
                }
            }
            else
            {
                for (int i = mouseX; i <= startMouseX; i += 3)
                {
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(i, startMouseY), Color.Green);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(i, mouseY), Color.Green);
                }
            }
            if (startMouseY < mouseY)
            {
                for (int j = startMouseY; j <= mouseY; j += 3)
                {
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(startMouseX, j), Color.Green);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(mouseX, j), Color.Green);
                }
            }
            else
            {
                for (int j = mouseY; j <= startMouseY; j += 3)
                {
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(startMouseX, j), Color.Green);
                    spriteBatch.Draw(game.textures.blankBoxSmall, new Vector2(mouseX, j), Color.Green);
                }
            }
        }
    }
}
