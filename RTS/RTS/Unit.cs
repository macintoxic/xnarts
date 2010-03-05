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
    public class Unit : Queueable
    {

        /*************** Unit Fields ******************************/

        StratGame gameRef;
        public Texture2D image;
        public Texture2D avatar;
        public Texture2D attack;
        public Unit currentTarget;
        public Queue<Queueable> targetQueue;

        private int max_health;
        private int health;     // Amount of damage unit can take before being destroyed
        private int power;      // Amount of damage unit can do with one attack
        private int speed;      // Distance unit can move in a turn
        private int range;      // Maximum distance between unit and its target for it to be able to attack

        private const int ACTION_TIME = 50;
        private int action_count_down = 0;
        private Color color = Color.White;

        public Boolean AIon;
        public Boolean enemy;
        public Boolean dead;
        public Vector2 location { get; set; }
        public Vector2 destination;


        /************** Constructors ********************************/

        // Generic constructor probably only to be used for testing purposes.
        public Unit(StratGame game)
        {
            gameRef = game;
            image = gameRef.textures.unit_default_image;
            avatar = gameRef.textures.unit_default_avatar;
            attack = gameRef.textures.unit_default_attack;
            health = 10;
            max_health = 10;
            power = 3;
            speed = 5;
            range = 10;
            currentTarget = null;
            AIon = false;
            enemy = false;
            dead = false;
            location = new Vector2(5, 5);
            destination = new Vector2(Program.NaN, Program.NaN);
            targetQueue = new Queue<Queueable>(15);
        }

        // More capable constructor, which allows for the setting of custom field values.
        // Field values can still be set to default by passing 'null' for objects 
        // and '-1' for integers.
        public Unit(StratGame game, Texture2D img, Texture2D avt, Texture2D atk,
                    int hp, int pow, int spd, int rng, Boolean enem, Vector2 loc)
        {
            gameRef = game;
            if (img == null) image = gameRef.textures.unit_default_image;
            else image = img;
            if (avt == null) avatar = gameRef.textures.unit_default_avatar;
            else avatar = avt;
            if (atk == null) attack = gameRef.textures.unit_default_attack;
            else attack = atk;
            if (hp == Program.NaN) max_health = 10;
            else max_health = hp;
            health = max_health;
            if (pow == Program.NaN) power = 3;
            else power = pow;
            if (spd == Program.NaN) speed = 5;
            else speed = spd;
            if (rng == Program.NaN) range = 10;
            else range = rng;
            if (loc == null) location = new Vector2(5, 5);
            else location = loc;
            enemy = enem;
            AIon = false;
            dead = false;
            currentTarget = null;
            destination = new Vector2(Program.NaN, Program.NaN);
            targetQueue = new Queue<Queueable>();
        }


        /*************** Actions ************************************/

        // Method to call when this Unit is hit by an attack. Returns true if this unit dies.
        public Boolean hit(int enemyPower)
        {
            health -= enemyPower;
            if (health <= 0)
            {
                this.die();
                return true;
            }
            return false;
        }

        // Called when Unit runs out of health.
        private void die()
        {
            //gameRef.units.Remove(this);
            dead = true;
        }

        // moves the unit toward its destination. Returns true if the unit moves,
        // returns false if it has no destination, or is already next to the destination.
        private Boolean move() {
            if (destination.X == Program.NaN || destination.Y == Program.NaN)
                return false;
            //Boolean movedX = true;
            //Boolean movedY = true;
            /*if (destination.X < location.X && destination.X + speed < location.X)
                location.X -= speed;
            else if (destination.X > location.X && destination.X - speed > location.X)
                location.X += speed;
            else
                movedX = false;
            if (destination.Y < location.Y && destination.Y + speed < location.Y)
                location.Y -= speed;
            else if (destination.Y > location.Y && destination.Y - speed > location.Y)
                location.Y += speed;
            else
                movedY = false; */
            if (Vector2.Distance(location, destination) <= speed)
            {
                if(currentTarget != null) return true;
                updateTargetQueue();
            }
            else
            {
                location = Vector2.Lerp(location, destination, //0.05f
                    (float)speed / (float)Vector2.Distance(location, destination));
                return true;
            }

            return false;

            /*if (!movedX && !movedY)
                destination = new Vector2(Program.NaN, Program.NaN);

            return (movedX || movedY);*/
        }

        // Method for this Unit to attack another.
        private void attackUnit()
        {
            if (currentTarget == null) return;
            Boolean kill = currentTarget.hit(power);
            if (kill)
            {
                updateTargetQueue();
            }
            action_count_down = ACTION_TIME;
        }


        /*************** Updaters ***********************************/

        // Return true if we are within attacking range of our destination.
        private Boolean inRange()
        {
            if (Vector2.Distance(destination, location) <= range)
                return true;
            return false;
        }

        // Executes this unit's AI if it is an enemy or its AI is turned on.
        private void executeAI()
        {
            if (enemy)
            {
                if (currentTarget != null && !currentTarget.dead && inRange()) return;
                int closest = Program.NaN;
                Unit close = null;
                foreach (Unit unit in gameRef.friendly)
                {
                    if (unit.dead) continue;
                    int current = (int)Vector2.Distance(location, unit.location);
                    if (closest == Program.NaN || current < closest)
                    {
                        closest = current;
                        close = unit;
                    }
                }
                currentTarget = close;
            }
            else if (AIon)
            {
                if (currentTarget != null && !currentTarget.dead && inRange()) return;
                targetQueue.Clear();
                int closest = Program.NaN;
                Unit close = null;
                foreach (Unit unit in gameRef.enemy)
                {
                    if (unit.dead) continue;
                    int current = (int)Vector2.Distance(location, unit.location);
                    if (closest == Program.NaN || current < closest)
                    {
                        closest = current;
                        close = unit;
                    }
                }
                currentTarget = close;
            }
        }

        // Update the destination of this unit so it can follow a moving target.
        private void updateDestination()
        {
            if (currentTarget == null) return;
            while (currentTarget != null && currentTarget.dead)
            {
                updateTargetQueue();
            }
            if(currentTarget != null) 
            {
                destination.X = currentTarget.location.X;
                destination.Y = currentTarget.location.Y;
            }
        }

        private void updateTargetQueue()
        {
            currentTarget = null;
            destination.X = Program.NaN;
            destination.Y = Program.NaN;
            if (targetQueue.Count > 0)
            {
                while (targetQueue.Count > 0 && (currentTarget == null || currentTarget.dead))
                {
                    Queueable next = targetQueue.Dequeue();
                    destination.X = next.location.X;
                    destination.Y = next.location.Y;
                    if (next is Unit) currentTarget = (Unit)next;
                    else { currentTarget = null; break; }
                }
            }
            if (currentTarget != null && currentTarget.dead)
            {
                currentTarget = null;
                destination.X = Program.NaN;
                destination.Y = Program.NaN;
            }
        }

        // Called every cycle during draw phase
        public void Draw(GameTime gameTime, SpriteBatch spBatch)
        {
            if (dead) return;
            spBatch.Draw(image, new Vector2(location.X - (image.Width / 2),
                                            location.Y - (image.Height / 2)), color);
        }

        // Called every cycle during update phase
        public void Update(GameTime gameTime)
        {
            if (dead) return;

            if (action_count_down > 40)
                color = Color.Red;
            else
                color = Color.White;

            if (action_count_down <= 0)
            {
                executeAI();
                updateDestination();
                if (currentTarget == null || !inRange())
                    move();
                else
                    attackUnit();
            }
            else
                --action_count_down;
        }

        // Get percentage of health left (returns an int 0 - 100)
        public int healthPercentage()
        {
            if (dead) return 0;
            if (max_health <= 0 || health <= 0)
            {
                dead = true;
                return 0;
            }
            return (int)((((double)health) / ((double)max_health))*100);
            //return (int)(ratio * 100);
            //return health;
        }
    }
}
