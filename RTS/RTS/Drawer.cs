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
    public class Drawer
    {
        private StratGame game;

        public Drawer(StratGame g)
        {
            game = g;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw all the units first
            foreach (Unit unit in game.units)
                unit.Draw(gameTime, spriteBatch);

            // Now draw indicators around enemy units
            foreach (Unit enemyUnit in game.enemy)
            {
                if (enemyUnit.dead) continue;
                spriteBatch.Draw(game.textures.HUD_enemySelect,
                    new Vector2(enemyUnit.location.X - (enemyUnit.image.Width / 2),
                        enemyUnit.location.Y - (enemyUnit.image.Height / 2)), Color.White);
            }

            // Draw indicators around selected units
            foreach (Unit selectedUnit in game.selected)
            {
                if (selectedUnit.dead) continue;
                spriteBatch.Draw(game.textures.HUD_friendlySelect,
                    new Vector2(selectedUnit.location.X - (selectedUnit.image.Width / 2),
                        selectedUnit.location.Y - (selectedUnit.image.Height / 2)), Color.White);
            }
        }
    }
}
