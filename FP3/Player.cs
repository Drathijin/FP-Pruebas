using System;

namespace Dungeon
{
    class Player
    {
        const int HP = 10;
        const int ATKPLAYER = 2;
        const int INITIALPOS = 0;

        int pos; // posicion del jugador en el mapa
        int health, damage;

        /// <summary>
        /// Inicializa la posicion del Player a INITIALPOS, y HP y ATK a las constantes
        /// </summary>
        public Player()
        {
            pos = INITIALPOS;
            health = HP;
            damage = ATKPLAYER;
        }

        /// <summary>
        /// Inicializa player a los parametros dados (en desuso)
        /// </summary>
        /// <param name="posit"></param>
        /// <param name="hp"></param>
        /// <param name="atk"></param>
        public Player(int posit, int hp, int atk)
        {
            pos = posit;
            health = hp;
            damage = atk;
        }

        /// <summary>
        /// Devuelve la posicion del player
        /// </summary>
        /// <returns></returns>
        public int GetPosition()
        {
            return pos;
        }

        /// <summary>
        /// True si el HP del jugador > 0, false en caso contrario
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            if (health > 0)
                return true;
            else return false;
        }

        /// <summary>
        /// Devuelve los stats del jugador
        /// </summary>
        /// <returns></returns>
        public string PrintStats()
        {
            return "Player: HP " + health + " ATK " + damage + "\n";
        }

        /// <summary>
        /// Devuelve el ATK del jugador
        /// </summary>
        /// <returns></returns>
        public int GetATK()
        {
            return damage;
        }

        /// <summary>
        /// Recive daño el jugador y comprueba si muere
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public bool ReceiveDamage(int damage)
        {
            health -= damage;
            return IsAlive();
        }

        /// <summary>
        /// Mueve al jugador en el mapa m con la direccion dir y devuelve true si ha podido moverse
        /// </summary>
        /// <param name="m"></param>
        /// <param name="dir"></param>
        public bool Move(Map m, Direction dir)
        {
            int goal = m.Move(pos, dir);
            if (goal > -1)
            {
                pos = goal;
                return true;
            }
            return false;
        }

        /// <summary>
        /// True si el jugador esta en una salida
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public bool atExit(Map map)
        {
            return map.isExit(pos);
        }
    }
}