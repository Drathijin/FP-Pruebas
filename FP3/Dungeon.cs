//Ricardo Sulbarán Socas
//Nicolás Pastore Burgos el cruck
using System;
using System.IO;
using Listas;

namespace Dungeon
{
    // posibles direcciones
    public enum Direction { north, south, east, west };

    class Map
    {
        struct Enemy
        {
            public string name, description;
            public int attHP, attATK;
        }

        // lugares del mapa
        struct Dungeon
        {
            public string name, description;
            public bool exit; // es salida?
            public int[] doors; // vector de 4 componentes con el lugar
                                // al norte, sur, este y oeste respectivamente
                                // -1 si no hay conexion
            public Lista enemiesInDungeon; // lista de enteros, indices al vector de enemigos
        }

        Dungeon[] dungeons; // vector de lugares del mapa
        Enemy[] enemies; // vector de enemigos del juego
        int nDungeons, nEnemies; // numero de lugares y numero de enemigos del mapa

        public Map(int numDungeons, int numEnemies)
        {
            nDungeons = numDungeons;
            nEnemies = numEnemies;
            dungeons = new Dungeon[nDungeons];
            enemies = new Enemy[nEnemies];
        }

        /// <summary>
        /// Inicializa el mapa desde el archivo de texto file
        /// </summary>
        /// <param name="file"></param>
        public void ReadMap(string file)
        {
            StreamReader entrada;

            // guarda la linea que lee para poder mostrarla en los errores
            int readPointer = 0;

            try
            {
                entrada = new StreamReader(file);
            }
            catch (Exception e)
            {
                throw new Exception("ERROR: no encontrado el mapa\n" + e.Message);
            }
            while (!entrada.EndOfStream)
            {
                string[] linea = entrada.ReadLine().Split(' ');
                readPointer++;
                switch (linea[0].ToLower())
                {
                    case "dungeon":
                        CreateDungeon(linea, entrada, ref readPointer);
                        break;

                    case "door":
                        CreateDoor(linea, readPointer);
                        break;

                    case "enemy":
                        CreateEnemy(linea, readPointer);
                        break;

                    case "":
                        break;

                    default:
                        string error = "";
                        for(int i = 0; i < linea.Length; i++)
                        {
                            error += linea[i] + " ";
                        }
                        entrada.Close();
                        throw new Exception("Formato inválido : Item incorrecto." +
                            "\nLinea " + readPointer + ": " + error + "\n");
                }
            }
            entrada.Close();

            // previene de que haya un mapa sin puertas en la primera sala
            // soluciona el problema de que no hayan añadido un " al final de la ultima descripcion
            bool doorError = true;
            for(int i = 0; i < dungeons[0].doors.Length; i++)
            {
                if (dungeons[0].doors[i] != -1) doorError = false;
            }
            if (doorError) throw new Exception("Primera dungeon no tiene puertas." +
                "\nCompruebe que cada item este bien espaciado entre si.\n");
        }

        /// <summary>
        /// Lee del archivo y crea una sala en el mapa
        /// </summary>
        /// <param name="linea"></param>
        /// <param name="entrada"></param>
        private void CreateDungeon(string[] linea, StreamReader entrada, ref int pointer)
        {
            int numDung;
            try
            {
                numDung = int.Parse(linea[1]);  //el número de la dungeon que toca
                if(numDung < 0 || numDung >= nDungeons)
                {
                    //numDung = int.Parse("ERROR");
                    string error = "";
                    for (int i = 0; i < linea.Length; i++)
                    {
                        error += linea[i] + " ";
                    }
                    throw new Exception("Formato inválido : Dungeon no tiene un numero correcto asociado.\n" +
                        "Linea " + pointer + ": " + error + "\n");

                    // frozaba el error porque quería evitar repetir código
                }
            }
            catch
            {
                string error = "";
                for (int i = 0; i < linea.Length; i++)
                {
                    error += linea[i] + " ";
                }
                throw new Exception("Formato inválido : Dungeon no tiene un numero correcto asociado.\n" +
                    "Linea " + pointer + ": " + error + "\n");
            }
            dungeons[numDung].name = linea[2];  //guarda el nombre
            dungeons[numDung].exit = IsDungExit(linea, pointer);  //guarda el booleano salida
            dungeons[numDung].description = ReadDescription(entrada, ref pointer);  //guarda la descripción
            dungeons[numDung].doors = new int[] { -1, -1, -1, -1 };
            dungeons[numDung].enemiesInDungeon = new Lista();
        }

        /// <summary>
        ///  Lee del archivo y crea una puerta entre dos salas
        /// </summary>
        /// <param name="linea"></param>
        private void CreateDoor(string[] linea, int pointer)
        {
            string oposite = "";
            switch (linea[4].ToLower())
            {
                case "north":
                    oposite = "south";
                    break;
                case "south":
                    oposite = "north";
                    break;
                case "east":
                    oposite = "west";
                    break;
                case "west":
                    oposite = "east";
                    break;
                default:
                    string error = "";
                    for (int i = 0; i < linea.Length; i++)
                    {
                        error += linea[i] + " ";
                    }
                    throw new Exception("Formato inválido: Puerta no tiene una direccion valida.\n" +
                        "Linea " + pointer + ": " + error + "\n");
            }
            int origen, destino;
            try
            {
                origen = int.Parse(linea[3]);
                destino = int.Parse(linea[6]);
            }
            catch
            {
                string error = "";
                for (int i = 0; i < linea.Length; i++)
                {
                    error += linea[i] + " ";
                }
                throw new Exception("Formato inválido: Puerta no tiene un origen o un destino validos.\n" +
                    "Linea " + pointer + ": " + error + "\n");
            }
            
            // crea una puerta de ida y de vuelta
            dungeons[origen].doors[(int)Enum.Parse(typeof(Direction), linea[4].ToLower())] = destino;
            dungeons[destino].doors[(int)Enum.Parse(typeof(Direction), oposite)] = origen;
        }

        /// <summary>
        /// Lee del archivo y crea un enemigo en una sala y en la lista global
        /// </summary>
        /// <param name="linea"></param>
        private void CreateEnemy(string[] linea, int pointer)
        {
            int numEnem;
            try
            {
                numEnem = int.Parse(linea[1]);
            }
            catch
            {
                string error = "";
                for (int i = 0; i < linea.Length; i++)
                {
                    error += linea[i] + " ";
                }
                throw new Exception("Formato inválido : Enemigo no tiene un numero correcto asociado.\n" +
                                    "Linea " + pointer + ": " + error + "\n");
            }
            enemies[numEnem].name = linea[2];
            enemies[numEnem].description = "";
            try
            {
                enemies[numEnem].attHP = int.Parse(linea[3]);
                enemies[numEnem].attATK = int.Parse(linea[4]);
                dungeons[int.Parse(linea[6])].enemiesInDungeon.insertaFin(numEnem);
            }
            catch
            {
                string error = "";
                for (int i = 0; i < linea.Length; i++)
                {
                    error += linea[i] + " ";
                }
                throw new Exception("Formato inválido : Enemigo no tiene valores validos de HP, ATK o de la dungeon en la que esta.\n" +
                                    "Linea " + pointer + ": " + error + "\n");
            }
        }

        /// <summary>
        /// Lee del archivo la descripcion de una sala
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        private string ReadDescription(StreamReader entrada, ref int pointer)
        {
            string linea = "";
            string parrafo = " ";
            bool endOfDesc = false;
            entrada.Read();
            while (!entrada.EndOfStream && !endOfDesc)
            {
                linea = entrada.ReadLine();
                pointer++;
                int i = linea.Length - 1;
                while (i >= 0 && !endOfDesc)
                {
                    if (linea[i] == '\"')
                    {
                        endOfDesc = true;
                        string temp = linea.Remove(i);
                        linea = temp;
                    }
                    i--;
                }
                parrafo += "\n" + linea;
            }
            return parrafo;
        }

        /// <summary>
        /// Lee del archivo y comprueba si la sala es salida
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private bool IsDungExit(string[] linea, int pointer)
        {
            if (linea[3].ToLower() == "exit") return true;

            else if (linea[3].ToLower() == "noexit") return false;

            else
            {
                string error = "";
                for (int i = 0; i < linea.Length; i++)
                {
                    error += linea[i] + " ";
                }
                throw new Exception("Formato inválido : Error salida.\nLinea " + pointer + ": " + error
                    + "\nSolo puede ser exit o noExit\n");
            }
        }

        /// <summary>
        /// Devuelve un string con toda la informacion de una sala
        /// </summary>
        /// <param name="dung"></param>
        /// <returns></returns>
        public string GetDungeonInfo(int dung)
        {
            return dungeons[dung].name + ":" + dungeons[dung].description + "\nExit: " + dungeons[dung].exit;
        }

        /// <summary>
        /// Devuelve los posibles movimientos en la sala dung
        /// </summary>
        /// <param name="dung"></param>
        /// <returns></returns>
        public string GetMoves(int dung)
        {
            string dirs = "\n";
            for (int i = 0; i < dungeons[dung].doors.Length; i++)
            {
                int pos = dungeons[dung].doors[i];
                if (pos >= 0)
                {
                    dirs += (Direction)i + ":\t " + dungeons[pos].name + "\n";
                }
            }
            dirs += "\n";
            return dirs;
        }

        /// <summary>
        /// Devuelve el numero de enemigos en una sala
        /// </summary>
        /// <param name="dung"></param>
        /// <returns></returns>
        public int GetNumEnemies (int dung)
        {
            return dungeons[dung].enemiesInDungeon.cuentaEltos();
        }

        /// <summary>
        /// Devuelve la informacion de un enemigo
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public string GetEnemyInfo(int en, int index)
        {
            return index + ": " + "Enemy" + en + " \"" + enemies[en].name + "\" HP " + 
                    enemies[en].attHP + " ATK " + enemies[en].attATK + "\n";
        }

        /// <summary>
        /// Devuelve la informacion de un enemigo (usado para Save) (en desuso)
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public string GetEnemyInfo(int en)
        {
            return "enemy " + en + " stats " +
                    enemies[en].attHP + " " + enemies[en].attATK;
        }

        /// <summary>
        /// Devuelve la informacion de todos los enemigos en una sala
        /// </summary>
        /// <param name="dung"></param>
        /// <returns></returns>
        public string GetEnemiesInfo(int dung)
        {
            string enems = "";

            for(int i = 0; i < GetNumEnemies(dung); i++)
            {
                enems += GetEnemyInfo(dungeons[dung].enemiesInDungeon.nEsimo(i), i);
            }
            if (enems != "")
                return enems;
            else return "There are no enemies in the room\n";
        }

        /// <summary>
        /// Devuelve el valor de ATK del enemigo
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public int GetEnemyATK(int en)
        {
            return enemies[en].attATK;
        }

        /// <summary>
        /// Daña al enemigo si existe y devuelve un booleano si has matado o no al enemigo
        /// </summary>
        /// <param name="dung"></param>
        /// <param name="damage"></param>
        /// <param name="en"></param>
        /// <returns></returns>
        public bool MakeDamageEnemy(int dung, int damage, int en)
        {
            //int enemyPos = dungeons[dung].enemiesInDungeon.nEsimo(en);

            if (en >= 0 && en < nEnemies)
            {
                enemies[en].attHP -= damage;

                if (enemies[en].attHP <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ataca a los enemigos en una sala y devuelve el numero de enemigos asesinados
        /// </summary>
        /// <param name="dung"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public int AttackEnemiesInDungeon(int dung, int damage)
        {
            int kills = 0;

            for (int i = 0; i < GetNumEnemies(dung); i++)
            {
                if(MakeDamageEnemy(dung, damage, dungeons[dung].enemiesInDungeon.nEsimo(i)))
                {
                    dungeons[dung].enemiesInDungeon.borraElto(dungeons[dung].enemiesInDungeon.nEsimo(i));

                    kills++;
                }
            }

            return kills;
        }

        /// <summary>
        /// Calcula el daño que te hacen los enemigos en una sala
        /// </summary>
        /// <param name="dung"></param>
        /// <returns></returns>
        public int ComputeDungeonDamage(int dung)
        {
            int damage = 0;

            for(int i = 0; i < GetNumEnemies(dung); i++)
            {
                damage += GetEnemyATK(dungeons[dung].enemiesInDungeon.nEsimo(i));
            }

            return damage;
        }

        /// <summary>
        /// Devuelve la sala desde pl con la direccion dir. Devuelve -1 en caso de error
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public int Move(int pl, Direction dir)
        {
            return dungeons[pl].doors[(int)dir];
        }

        /// <summary>
        /// Devuelve true si es salida y false si no
        /// </summary>
        /// <param name="dung"></param>
        /// <returns></returns>
        public bool isExit(int dung)
        {
            return dungeons[dung].exit;
        }

        /// <summary>
        /// Devuelve los stats de todos los enemigos del juego (en desuso)
        /// </summary>
        /// <returns></returns>
        public string GetTotalEnemiesInfo()
        {
            string list = "";
            for(int i = 0; i <enemies.Length; i++)
            {
                list += GetEnemyInfo(i) + "\n";
            }
            return list;
        }

        /// <summary>
        /// Establece el HP y ATK del enemigo en (en desuso)
        /// </summary>
        /// <param name="en"></param>
        public void SetStatsEnemy(int en, int hp, int atk)
        {
            enemies[en].attHP = hp;
            enemies[en].attATK = atk;
        }

        /// <summary>
        /// Devuelve nDungeons
        /// </summary>
        /// <returns></returns>
        public int GetNDungeons()
        {
            return nDungeons;
        }

        /// <summary>
        /// Devuelve nEnemies
        /// </summary>
        /// <returns></returns>
        public int GetNEnemies()
        {
            return nEnemies;
        }

        /// <summary>
        /// Establece nDungeons a un nuevo valor
        /// </summary>
        /// <param name="newNDung"></param>
        public void SetNDungeons(int newNDung)
        {
            nDungeons = newNDung;
        }

        /// <summary>
        /// Establece nEnemeies a un nuevo valor
        /// </summary>
        /// <param name="newNEnemeies"></param>
        public void setNEnemies(int newNEnemeies)
        {
            nEnemies = newNEnemeies;
        }

        /// <summary>
        /// Establece los parametros en la dungeon index. Exit solo lee "yes" o "no"
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newName"></param>
        /// <param name="newDescription"></param>
        /// <param name="exit"></param>
        public void SetDungeon(int index, string newName, string newDescription, string exit)
        {
            dungeons[index].name = newName;
            dungeons[index].description = newDescription;
            if (exit.ToLower() == "yes")
            {
                dungeons[index].exit = true;
            }
            else dungeons[index].exit = false;
            dungeons[index].doors = new int []{ -1, -1, -1, -1 };
            dungeons[index].enemiesInDungeon = new Lista();
        }

        /// <summary>
        /// Establece una puerta entre las salas origin y goal con direccion direction
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="goal"></param>
        /// <param name="direction"></param>
        public void SetDoor(int origin, int goal, int direction)
        {
            dungeons[origin].doors[direction] = goal;
            switch (direction)
            {
                case 0:
                    direction = 1;
                    break;
                case 1:
                    direction = 0;
                    break;
                case 2:
                    direction = 3;
                    break;
                case 3:
                    direction = 2;
                    break;
            }
            dungeons[goal].doors[direction] = origin;
        }

        /// <summary>
        /// Establece un enemigo con todos sus parametros
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newName"></param>
        /// <param name="newDescription"></param>
        /// <param name="newHP"></param>
        /// <param name="newATK"></param>
        /// <param name="location"></param>
        public void SetEnemy(int index, string newName, string newDescription, int newHP, int newATK, int location)
        {
            enemies[index].name = newName;
            enemies[index].description = newDescription;
            enemies[index].attHP = newHP;
            enemies[index].attATK = newATK;
            dungeons[location].enemiesInDungeon.insertaFin(index);
        }

        /// <summary>
        /// Devuelve un string con todas las puertas codificadas como en los archivos .map
        /// </summary>
        /// <param name="nameFile"></param>
        public string GetTotalDoorsFromFile (string nameFile)
        {
            StreamReader entrada = new StreamReader(nameFile);
            try
            {
                entrada = new StreamReader(nameFile);
            }
            catch
            {
                throw new Exception("Formato inválido: no se ha encontrado el mapa con las puertas");
            }

            string[] linea;
            string totalDoors = "";
            int pointer = 0;
            while (!entrada.EndOfStream)
            {
                linea = entrada.ReadLine().Split(' ');
                try
                {
                    if (linea[0] == "door")
                    {
                        totalDoors += "\n" + linea[0] + " " + linea[1] + " " + linea[2] + " " + linea[3] + " " + linea[4] + " " + linea[5] + " " + linea[6];
                    }
                }
                catch
                {
                    string error = "";
                    for (int i = 0; i < linea.Length; i++)
                    {
                        error += linea[i] + " ";
                    }
                    throw new Exception("Formato inválido: la puerta no tiene un formato válido.\n" +
                        "Linea " + pointer + ": " + error + "\n");
                }
                pointer++;
            }

            entrada.Close();
            return totalDoors;
        }
    }   
}