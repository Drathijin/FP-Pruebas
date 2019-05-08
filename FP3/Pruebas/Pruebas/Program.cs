using System;
using System.IO;

namespace Dungeon
{
    public enum Mode
    {
        Menu, Edit, Editing, Playing, Exit 
    }
    static class MainClass
    {
        const int INITAMRECO = 10; // tamaño del record inicial
        const int DUNGEONS = 10; // numero de salas
        const int ENEMIES = 3; // numero de enemigos
        static string mapFileClass = ""; // nombre del mapa (usado en Save)
        static Mode mode = 0;
        static string dirMainMenu = "./Archivos/MainMenu.txt";
        static string dirEditorMenu = "./Archivos/EditorMenu.txt";
        static string dirEdittingMenu = "./Archivos/EdittingMenu.txt";


        /// <summary>
        /// Guarda todas las acciones válidas del jugador en un array de strings
        /// </summary>
        struct Record
        {
            public string[] actions;
            public int tamActions;
        }

        /// <summary>
        /// guarda las puertas creadas por el editor
        /// </summary>
        struct Doors
        {
            public string[] doorsVec;
            public int tam;
        }

        /// <summary>
        /// Añade una accion al record
        /// </summary>
        /// <param name="entrada"></param>
        /// <param name="rec"></param>
        static void AddAction(string entrada, ref Record rec)
        {
            // si se sale del array, duplica su tamaño
            if (rec.tamActions >= rec.actions.Length)
            {
                Record temp = rec;
                rec.actions = new string[temp.actions.Length * 2];
                for (int i = 0; i < temp.actions.Length; i++)
                {
                    rec.actions[i] = temp.actions[i];
                }
            }
            rec.actions[rec.tamActions] = entrada;
            rec.tamActions++;
        }

        /// <summary>
        /// Elimina la ultima accion
        /// </summary>
        static void EraseAction(ref Record rec)
        {
            if(rec.tamActions > 0)
                rec.tamActions--;
        }

        /// <summary>
        /// Procesa el input del usuario en el juego
        /// </summary>
        /// <param name="com"></param>
        /// <param name="p"></param>
        /// <param name="m"></param>
        static void ProcesaInput(string com, Player p, Map m, ref Record rec)
        {
            Console.Clear();
            mode = Mode.Playing;
            try
            {
                string[] entrada = com.Split(' ');
                switch (entrada[0].ToLower())
                {
                    case "go":
                        if (p.Move(m, (Direction)Enum.Parse(typeof(Direction), entrada[1])))
                        {
                            AddAction(com, ref rec);
                            // si hay enemigos en la sala te atacan
                            if (EnemiesAttackPlayer(m, p))
                                EscribeEnColor("You have been attacked!!!\n", ConsoleColor.Red);
                        }
                        break;

                    case "attack":
                        AddAction(com, ref rec);
                        int killedEnemies = PlayerAttackEnemies(m, p);
                        EscribeEnColor("You have killed " + killedEnemies + " enemy/ies\n", ConsoleColor.Green);
                        // si hay enemigos en la sala te atacan
                        if (EnemiesAttackPlayer(m, p))
                            EscribeEnColor("You have been attacked!!!\n", ConsoleColor.Red);
                        // si no te atacan y no has matado a un enemigo, es que has atacado a la nada
                        else if (killedEnemies <= 0)
                            EraseAction(ref rec);
                        break;

                    case "enemies":
                        Console.WriteLine(m.GetEnemiesInfo(p.GetPosition()));
                        break;
                    case "info":
                        Console.Write(m.GetDungeonInfo(p.GetPosition()) + "\n" + m.GetMoves(p.GetPosition()));
                        break;
                    case "stats":
                        Console.WriteLine(p.PrintStats());
                        break;
                    case "quit":
                        mode = Mode.Exit;
                        break;
                    case "menu":
                        mode = Mode.Menu;
                        break;
                    case "help":
                        Console.Write(
@"help:          to see the list of commands.
go direction:  to go in that direction.
attack:        to attack all the enemies in the room.
enemies:       to show the enemies in the current room.
info:          to see the information about the room.
stats:         to see the player stats.
refresh:       cleans the screen.
commands file: read from file a sequence of instructions.
save file:     save progress in file.
menu:          goes back to the Main menu.
quit:          to exit the program.

");
                        break;
                    case "commands":
                        ReadCommands(entrada[1], m, p, ref rec);
                        break;
                    case "save":
                        SaveGame(entrada[1], rec);
                        break;
                    case "refresh":
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine("Input introducido no valido.\nEscriba help para ver los posibles comandos.\n");
                        break;
                }
            }
            catch
            {
                Console.WriteLine("Input introducido no valido.\nEscriba help para ver los posibles comandos.\n");
            }
        }

        /// <summary>
        /// Enemigos de una sala atacan al jugador. True si lo han atacado
        /// </summary>
        /// <param name="m"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        static bool EnemiesAttackPlayer(Map m, Player p)
        {
            if (m.GetNumEnemies(p.GetPosition()) > 0)
            {
                p.ReceiveDamage(m.ComputeDungeonDamage(p.GetPosition()));
                return true;
            }
            else return false;
        }

        /// <summary>
        /// El jugador ataca a los enemigos en una sala y devuelve el numero de enemigos asesinados
        /// </summary>
        /// <param name="m"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        static int PlayerAttackEnemies(Map m, Player p)
        {
            return m.AttackEnemiesInDungeon(p.GetPosition(), p.GetATK());
        }

        /// <summary>
        /// Main del juego. Aqui comienza la partida
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            MainMenu();
        }

        /// <summary>
        /// Lee de un archivo las instrucciones a seguir
        /// </summary>
        /// <param name="file"></param>
        /// <param name="map"></param>
        /// <param name="player"></param>
        static void ReadCommands(string file, Map map, Player player, ref Record rec)
        {
            StreamReader entrada;
            try
            {
                entrada = new StreamReader(file);
                mode = Mode.Playing;
            }
            catch (Exception e)
            {
                throw new Exception("Archivo no encontrado.\n" + e.Message);
            }

            while (!entrada.EndOfStream && mode==Mode.Playing)
            {
                string linea = entrada.ReadLine();
                ProcesaInput(linea, player, map, ref rec);
            }

            entrada.Close();
        }

        /// <summary>
        /// Guarda el juego en un file (en desuso)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="m"></param>
        /// <param name="p"></param>
        static void SaveGame(string file, Map m, Player p)
        {
            StreamWriter salida = new StreamWriter(file);
            salida.WriteLine(mapFileClass + "\n");
            salida.WriteLine(p.PrintStats());
            salida.WriteLine("POS " + p.GetPosition() + "\n");

            salida.WriteLine(m.GetTotalEnemiesInfo());

            salida.Close();
        }

        /// <summary>
        /// Guarda en file un record
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rec"></param>
        static void SaveGame(string file, Record rec)
        {
            StreamWriter salida = new StreamWriter(file);
            salida.WriteLine(mapFileClass + "\n");

            for (int i = 0; i < rec.tamActions; i++)
            {
                salida.WriteLine(rec.actions[i]);
            }

            salida.Close();
        }

        /// <summary>
        /// Lee un file con una save de una partida y la carga (en desuso)
        /// </summary>
        /// <param name="saveFile"></param>
        /// <param name="m"></param>
        /// <param name="p"></param>
        static void LoadSave(string mapFile, string saveFile, out Map m, out Player p)
        {
            StreamReader entrada;
            m = new Map(DUNGEONS, ENEMIES);
            p = new Player();

            int readPointer = 0;
            try
            {
                entrada = new StreamReader(saveFile);
            }
            catch
            {
                throw new Exception("No se ha encontrado el archivo con la save\n");
            }
            string mapFileSave = entrada.ReadLine();
            readPointer++;
            if (mapFile != mapFileSave)
            {
                throw new Exception("La save no coincide con el mapa cargado\n");
            }
            else
            {
                m.ReadMap(mapFileSave);
                mapFileClass = mapFile;
                int hp = 10, atk = 2, pos = 0;
                while (!entrada.EndOfStream)
                {
                    string[] linea = entrada.ReadLine().Split(' ');
                    readPointer++;
                    switch (linea[0].ToLower())
                    {
                        case "player:":
                            break;
                        case "":
                            break;
                        case "hp":
                            hp = int.Parse(linea[1]);
                            break;
                        case "atk":
                            atk = int.Parse(linea[1]);
                            break;
                        case "pos":
                            pos = int.Parse(linea[1]);
                            break;
                        case "enemy":
                            m.SetStatsEnemy(int.Parse(linea[1]), int.Parse(linea[3]), int.Parse(linea[4]));
                            break;
                        default:
                            string error = "";
                            for (int i = 0; i < linea.Length; i++)
                            {
                                error += linea[i] + " ";
                            }
                            entrada.Close();
                            throw new Exception("Formato inválido : Item incorrecto en la Save." +
                                "\nLinea " + readPointer + ": " + error);
                    }
                }
                p = new Player(pos, hp, atk);
            }
        }

        /// <summary>
        /// Lee un file con un record que funciona como save y carga la partida
        /// </summary>
        /// <param name="saveFile"></param>
        /// <param name="rec"></param>
        /// <param name="m"></param>
        /// <param name="p"></param>
        static void LoadSave(string saveFile, ref Record rec, out Map m, out Player p)
        {
            StreamReader entrada;
            m = new Map(DUNGEONS, ENEMIES);
            p = new Player();
            try
            {
                entrada = new StreamReader(saveFile);
            }
            catch
            {
                throw new Exception("No se ha encontrado el archivo con la save\n");
            }

            string linea = entrada.ReadLine();
            try
            {
                m.ReadMap(linea);
            }
            catch (Exception e)
            {
                throw new Exception("Error al cargar el mapa desde la save:\n" + e.Message);
            }

            mapFileClass = linea;

            while (!entrada.EndOfStream)
            {
                linea = entrada.ReadLine();
                if (linea != "")
                    AddAction(linea, ref rec);
            }

            entrada.Close();

            try
            {
                EnemiesAttackPlayer(m, p);
                ReadCommands(saveFile, m, p, ref rec);
            }
            catch (Exception e)
            {
                throw new Exception("Error al ejecutar las acciones de la save\n" + e.Message);
            }
        }

        /// <summary>
        /// Menu principal del juego
        /// </summary>
        static void MainMenu()
        {
            //Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            while (mode != Mode.Exit)
            {
                Console.SetCursorPosition(0, 0);                
                EscribeMainMenu();
                Prompt();
                try
                {
                    ProcesaInputMenu();
                }
                catch (Exception e)
                {
                    // 31 para el primero
                    // 16 para el segundo
                    // 18 para el tercero
                    Console.SetCursorPosition(0, 18);
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void EscribeMainMenu()
        {
            StreamReader reader = new StreamReader(dirMainMenu);
            EscribeEnColor(reader.ReadToEnd(), ConsoleColor.DarkYellow);
            reader.Close();
        }

        /// <summary>
        /// Procesa el input del menu
        /// </summary>
        static void ProcesaInputMenu()
        {
            mode = Mode.Menu;
            string[] entrada = Console.ReadLine().Split(' ');
            try
            {
                switch (entrada[0].ToLower())
                {
                    case "new":
                        NewGame(entrada[1]);
                        break;
                    case "load":
                        LoadGame(entrada[1]);
                        break;
                    case "editor":
                        mode = Mode.Edit;
                        EditorMenu();
                        break;
                    case "quit":
                        mode = Mode.Exit;
                        break;
                    case "refresh":
                        Console.Clear();
                        break;
                    case "":
                        break;
                    default:
                        throw new Exception("Formato Inválido: Input incoherente.\n");
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                throw new Exception("Ha habido un error al cargar la opcion:\n" + e.Message);
            }
        }

        /// <summary>
        /// Crea una nueva partida
        /// </summary>
        /// <param name="file"></param>
        static void NewGame(string file)
        {
            Console.Clear();
            Console.WriteLine("Loading...");
            Map map = new Map(DUNGEONS, ENEMIES);
            Player player = new Player();
            Record rec = new Record { actions = new string[INITAMRECO], tamActions = 0 };

            try
            {
                map.ReadMap(file);
            }
            catch (Exception e)
            {
                throw new Exception("Ha habido un error en el nivel:\n" + e.Message);
            }

            mapFileClass = file;
            Game(map, player, ref rec);
        }

        /// <summary>
        /// Carga una partida a partir de una save
        /// </summary>
        /// <param name="saveFile"></param>
        static void LoadGame(string saveFile)
        {
            Console.Clear();
            Console.WriteLine("Loading...");
            Map map;
            Player player;
            Record rec = new Record { actions = new string[INITAMRECO], tamActions = 0 };
            try
            {
                LoadSave(saveFile, ref rec, out map, out player);
            }
            catch (Exception e)
            {
                throw new Exception("Error al cargar la save:\n" + e.Message);
            }
            Game(map, player, ref rec);
        }

        /// <summary>
        /// Juego
        /// </summary>
        /// <param name="map"></param>
        /// <param name="player"></param>
        /// <param name="rec"></param>
        static void Game(Map map, Player player, ref Record rec)
        {
            Console.Clear();
            mode = Mode.Playing;

            // si hay enemigos en la sala inicial te atacan
            ProcesaInput("help", player, map, ref rec);
            if (EnemiesAttackPlayer(map, player))
            {
                EscribeEnColor("You have been attacked!!!\n", ConsoleColor.Red);
            }

            while (!EndGame(map, player) && mode == Mode.Playing)
            {
                EscribeEnColor(MapName(mapFileClass), ConsoleColor.Cyan);
                Console.Write(" >");
                string entrada = Console.ReadLine();

                ProcesaInput(entrada, player, map, ref rec);
            }
            Console.Clear();
        }

        private static void EscribeEnColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        static string MapName(string name) 
        {
            string ret = "";
            ret += char.ToUpper(name[0]);
            int i = 1;
            while(i < name.Length && name[i] != '.')
            {
                ret += name[i];
                i++;
            }
            return ret;

        }

        /// <summary>
        /// Comprueba si se ha terminado la partida
        /// </summary>
        /// <param name="map"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        static bool EndGame(Map map, Player player)
        {
            // si sales del mapa
            if (player.atExit(map))
            {
                Console.Write(map.GetDungeonInfo(player.GetPosition()));
                Console.WriteLine("\n\nYOU WIN!!!\n");
                Prompt();
                Console.ReadLine();
                return true;
            }
            // si mueres
            else if (!player.IsAlive())
            {
                Console.Write("You have died.\n");
                Console.WriteLine("YOU LOSE!!!\n");
                Prompt();
                Console.ReadLine();
                return true;
            }
            else return false;
        }

        private static void Prompt()
        {
            EscribeEnColor(mode.ToString(), ConsoleColor.Cyan);
            Console.Write(" >");
        }

        /// <summary>
        /// Pregunta si quieres editar un nivel o crear uno nuevo
        /// </summary>
        static void EditorMenu()
        {
            Console.Clear();
            mode = Mode.Edit;
            while (mode == Mode.Edit)
            {
                Console.SetCursorPosition(0, 0);
                //Console.WriteLine("Editor:\n");

                EscribeEditorMenu();

                Prompt();
                try
                {
                    ProcesaInputMenuEditor();
                }
                catch (Exception e)
                {
                    // 18 para el primero
                    Console.SetCursorPosition(0, 18);
                    Console.WriteLine(e.Message);
                }
            }
            Console.Clear();
        }

        private static void EscribeEditorMenu()
        {
            StreamReader reader = new StreamReader(dirEditorMenu);
            EscribeEnColor(reader.ReadToEnd(), ConsoleColor.DarkYellow);
            reader.Close();
        }

        /// <summary>
        /// Procesa el input para el menu del editor
        /// </summary>
        static void ProcesaInputMenuEditor()
        {
            mode = Mode.Edit;
            string[] entrada = Console.ReadLine().Split(' ');
            try
            {
                switch (entrada[0].ToLower())
                {
                    case "new":
                        Map m = CreateMap();
                        Doors doors = new Doors { doorsVec = new string[100], tam = 0 };
                        Editor(m, doors, entrada[1]);
                        break;
                    case "edit":
                        Map map = LoadMap(entrada[1]);
                        Doors doors2 = LoadDoors(map, entrada[1]);
                        Editor(map, doors2, entrada[1]);
                        break;
                    case "menu":
                        mode = Mode.Menu;
                        break;
                    case "quit":
                        mode = Mode.Exit;
                        break;
                    case "refresh":
                        Console.Clear();
                        break;
                    case "":
                        break;
                    default:
                        throw new Exception("Formato inválido: Input incoherente");
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                throw new Exception("Ha habido un error durante la ejecucion del editor:\n" + e.Message);
            }
        }

        /// <summary>
        /// Creador y editor de mapas
        /// </summary>
        /// <param name="m"></param>
        /// <param name="saveFile"></param>
        static void Editor(Map m,Doors doors, string saveFile)
        {
            Console.Clear();

            mode = Mode.Editing;
            while (mode == Mode.Editing)
            {
                Console.SetCursorPosition(0, 0);
                EscribeEdittingMenu(saveFile);

                Prompt();
                try
                {
                    ProcesaInputEditing(m, ref doors);
                }
                catch (Exception e)
                {
                    // 18 para el primero
                    Console.SetCursorPosition(0, 29);
                    Console.WriteLine(e.Message);
                }
            }
            Console.Clear();
        }

        private static void EscribeEdittingMenu(string saveFile)
        {
            //59 24
            StreamReader reader = new StreamReader(dirEdittingMenu);
            char i = ' ';
            string line = "";
            while (i != '{')
            {
                i =(char)(reader.Read());
                if(i != '{') line+=i;
            }
            reader.Read();
            EscribeEnColor(line, ConsoleColor.DarkYellow);
            EscribeEnColor(saveFile+"\n", ConsoleColor.Green);
            EscribeEnColor(reader.ReadToEnd(), ConsoleColor.DarkYellow);

            reader.Close();
        }

        /// <summary>
        /// Procesa el input del editor
        /// </summary>
        /// <param name="m"></param>
        static void ProcesaInputEditing(Map m, ref Doors doors)
        {
            mode = Mode.Editing;
            string[] entrada = Console.ReadLine().Split(' ');
            try
            {
                switch (entrada[0].ToLower())
                {
                    case "new":
                        switch (entrada[1].ToLower())
                        {
                            case "dungeon":
                                AddDungeon(m);
                                break;
                            case "door":
                                AddDoors(m, ref doors);
                                break;
                            case "enemy":
                                AddEnemy(m);
                                break;
                            default:
                                throw new Exception("Formato Inválido: Item no válido\n");
                        }
                        break;
                    case "edit":
                        switch (entrada[1].ToLower())
                        {
                            case "dungeon":
                                break;
                            case "enemy":
                                break;
                            default:
                                throw new Exception("Formato Inválido: Item no válido\n");
                        }
                        break;
                    case "delete":
                        switch (entrada[1].ToLower())
                        {
                            case "dungeon":
                                break;
                            case "enemy":
                                break;
                            default:
                                throw new Exception("Formato Inválido: Item no válido\n");
                        }
                        break;
                    case "random":
                        break;
                    case "show":
                        Show(m);
                        break;
                    case "save":
                        break;
                    case "menu":
                        switch (entrada[1].ToLower())
                        {
                            case "main":
                                mode = Mode.Menu;
                                break;
                            case "editor":
                                mode = Mode.Edit;
                                break;
                            default:
                                throw new Exception("Formato Inválido: Menu inexistente");
                        }
                        break;
                    case "quit":
                        mode = Mode.Exit;
                        break;
                    case "refresh":
                        Console.Clear();
                        break;
                    case "":
                        break;
                    default:
                        throw new Exception("Formato inválido: Input incoherente");
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                throw new Exception("Ha habido error durante la ejecucion del editor:\n" + e.Message);
            }
            Console.Clear();
        }

        /// <summary>
        /// Carga un mapa desde nameFile para editarlo desde el editor
        /// </summary>
        /// <param name="nameFile"></param>
        /// <returns></returns>
        static Map LoadMap(string nameFile)
        {
            Map m = new Map(DUNGEONS, ENEMIES);
            try
            {
                m.ReadMap(nameFile);
            }
            catch (Exception e)
            {
                throw new Exception("Ha habido un error al cargar el mapa:\n" + e.Message);
            }
            return m;
        }

        /// <summary>
        /// Carga las puertas del archivo 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="nameFile"></param>
        /// <returns></returns>
        static Doors LoadDoors(Map m, string nameFile)
        {
            Doors doors = new Doors { doorsVec = new string[100], tam = 0 };
            string[] doorsStr = m.GetTotalDoorsFromFile(nameFile).Split('\n');
            for(int i = 0; i < doorsStr.Length; i++)
            {
                doors.doorsVec[i] = doorsStr[i];
                doors.tam++;
            }
            return doors;
        }

        /// <summary>
        /// Crea un mapa sin parametros iniciales
        /// </summary>
        static Map CreateMap()
        {
            Map m = new Map(DUNGEONS, ENEMIES);
            m.SetNDungeons(0);
            m.setNEnemies(0);
            return m;
        }

        /// <summary>
        /// Crea un mapa con paramteros iniciales
        /// </summary>
        /// <param name="nIniDungeons"></param>
        /// <param name="nIniEnemies"></param>
        static Map CreateMap(int nIniDungeons, int nIniEnemies)
        {
            Map m = new Map(nIniDungeons, nIniEnemies);
            m.SetNDungeons(0);
            m.setNEnemies(0);
            return m;
        }

        /// <summary>
        /// Añade una Dungeon al final de dungeons[] de m
        /// </summary>
        /// <param name="m"></param>
        static void AddDungeon(Map m)
        {
            Console.Clear();
            Console.WriteLine("Type the name of the room:\n");
            Prompt();
            string name = Console.ReadLine();

            int descLines = AskForInt("How many lines does the description have?\n");

            string description = "\n";
            for (int i = 0; i < descLines; i++)
            {
                Console.Clear();
                Console.WriteLine("Type the line " + (i + 1) + ":\n");
                Prompt();
                string linea = Console.ReadLine();
                description += linea + "\n";
            }

            string exit;
            do
            {
                Console.Clear();
                Console.WriteLine("Is the room an exit?\nAnswer only yes or no:\n");
                Prompt();
                exit = Console.ReadLine();
            } while (exit.ToLower() != "yes" && exit.ToLower() != "no");

            m.SetDungeon(m.GetNDungeons(), name, description, exit);
            m.SetNDungeons(m.GetNDungeons() + 1);
        }

        /// <summary>
        /// Edita una dungeon
        /// </summary>
        /// <param name="m"></param>
        static void EditDungeon(Map m)
        {
            Console.Clear();
            int index;
            try
            {
                index = AskForInt(m.GetNDungeons(), "Type the index of the dungeon");
            }
            catch (Exception e)
            {
                throw new Exception("Error: editando sobre lista vacia:\n" + e.Message);
            }
            bool endEditing = false;

            Console.Clear();
            while (!endEditing)
            {
                Console.WriteLine("Current dungeon:\n");
                Console.WriteLine(PrintDungeon(m, index));
            }

        }

        /// <summary>
        /// Añade una puerta en m
        /// </summary>
        /// <param name="m"></param>
        static void AddDoors(Map m, ref Doors doors)
        {
            if (m.GetNDungeons() < 1)
                throw new Exception("Error: no se pueden crear una puerta sin al menos una dungeon\n");

            int origin = AskForInt(m.GetNDungeons(), "Type the index of the origin room:\n");

            int goal = AskForInt(m.GetNDungeons(), "Type the index of the goal room:\n");

            int direction = AskForInt(4, "Type the index of the direction of the door from origin:\n" +
                "0: North\n1: South\n2: East\n3: West\n");

            doors.doorsVec[doors.tam] = "door " + doors.tam + " dungeon " + origin + " " + (Direction)direction + " dungeon " + goal;
            doors.tam++;

            m.SetDoor(origin, goal, direction);
        }

        /// <summary>
        /// Añade un enemigo
        /// </summary>
        /// <param name="m"></param>
        static void AddEnemy(Map m)
        {
            Console.Clear();

            if (m.GetNDungeons() < 1)
                throw new Exception("Error: no se pueden crear un enemigo sin al menos una dungeon\n");

            int dungeon = AskForInt(m.GetNDungeons(), "Type the index of the dungeon in which the enemy is:\n");

            Console.WriteLine("Type the name of the enemy:\n");
            Prompt();
            string name = Console.ReadLine();

            string description = "";

            int hp = AskForInt("Type the HP of the enemy:\n");
            int atk = AskForInt("Type the ATK of the enemy:\n");

            m.SetEnemy(m.GetNEnemies(), name, description, hp, atk, dungeon);
            m.setNEnemies(m.GetNEnemies() + 1);
        }

        /// <summary>
        /// Pide un entero positivo con un maximo max no incluido y un string que muestra lo que se pide
        /// </summary>
        /// <param name="max"></param>
        /// <param name="whatAsk"></param>
        /// <returns></returns>
        static int AskForInt(int max, string whatAsk)
        {
            int num = -1;
            Console.Clear();
            do
            {
                Console.WriteLine(whatAsk);
                Prompt();
                try
                {
                    num = int.Parse(Console.ReadLine());
                    Console.Clear();
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Error: not an int.");
                }
            } while (num <= -1 || num >= max);
            return num;
        }

        /// <summary>
        /// Pide un entero postivo con un string de que es lo que se pide
        /// </summary>
        /// <param name="whatAsk"></param>
        /// <returns></returns>
        static int AskForInt(string whatAsk)
        {
            int num = -1;
            Console.Clear();
            do
            {
                Console.WriteLine(whatAsk);
                Prompt();
                try
                {
                    num = int.Parse(Console.ReadLine());
                    Console.Clear();
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Error: not an int.");
                }
            } while (num <= -1);
            return num;
        }

        /// <summary>
        /// Muestra el estado del mapa m
        /// </summary>
        /// <param name="m"></param>
        static void Show(Map m)
        {
            Console.Clear();
            Console.WriteLine("Estado actual del mapa:\n");
            Console.WriteLine(PrintAllDungeons(m) + PrintAllEnemies(m));
            Prompt();
            Console.ReadLine();
        }

        /// <summary>
        /// Devuelve el string con todas las Dungeons del mapa m
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        static string PrintAllDungeons(Map m)
        {
            string dung = "Dungeons:\n\n";
            for (int i = 0; i < m.GetNDungeons(); i++)
            {
                dung += PrintDungeon(m, i);
            }
            dung += "\n";
            return dung;
        }

        /// <summary>
        /// Devuelve un string con la informacion de la dungeon index del mapa m
        /// </summary>
        /// <param name="m"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static string PrintDungeon(Map m, int index)
        {
            return "dungeon" + index + ": " + m.GetDungeonInfo(index) + "\n" + m.GetMoves(index) + "\n";
        }

        /// <summary>
        /// Devuelve un string con todos los enemigos de un mapa m
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        static string PrintAllEnemies(Map m)
        {
            string enem = "Enemies:\n\n";
            for (int i = 0; i < m.GetNEnemies(); i++)
            {
                enem += m.GetEnemyInfo(i, i);
            }
            enem += "\n";
            return enem;
        }
    }
}


// DUDAS:
// 1 he puesto directions a minusculas para mejorar la lectura (cambio de la especificacion)
// 2 cambie el nombre de la clase Main porque daba errores
// 3 endGame out ref entre metodos, bien?
// 4 Exception especificas?
// 5 IsExit() y isExit()... problemas? no es sobrecarga verdad?
// 6 Solucion puertas al no tener espacio inicial (solucion meh) (si no hay saltos de linea) -> Problemas con la descripcion (ahora solo da problemas si no pones " final)
// 7 endGame salir de Game()? ReadCommands() (solucion meh, porque ese boolean no hace nada, es solo para evitar hacer una sobrecarga)
// 8 forzar un error para que lance excepcion? (solucionado duplicando el error)
// 9 AddDoor() y AddEnemy() hacen la comprobacion? o mejor Procesa? (lo hacen los dos metodos ahora)
// 10 const de DUNGEONS, ENEMIES, HP y ATK. cambiar la codificacion del mapa para que cada mapa pueda modificarlo? -> IMPORTANTE
// 11 modificado Player.Move() para poder guardar o no un registro

// RECORDATORIOS: 
// 1 no olvidar borrar las sobrecargas en desuso

// COMENTARIOS:
// 1 una sala puede tener una puerta que lleve a ella, y crea una puerta detras que tambien va a ella misma
// cuidado si se pisan las puertas, no explota, pero se pierde el camino de vuelta