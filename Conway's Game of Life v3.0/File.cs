using System;
using System.Linq;
using System.IO;

namespace GoL
{
    class Files
    {
        private static int[] mind = new int[64];

        public static void FileRead()
        {
            if (!File.Exists("data.txt"))
            {
                CreateFile();
            }

            string[] readText = File.ReadAllLines("data.txt");

            int z = 0;
            for (int i = 0; i < World.rows; i++)
            {
                for (int j = 0; j < World.cols; j++)
                {
                    if (readText[z++] == "1")
                    {
                        World.Map[i, j].Bot = true;
                        World.Map[i, j].Color = World.green;
                    } else
                    {
                        World.Map[i, j].Bot = false;
                        World.Map[i, j].Color = World.white;
                    }
                }
            }
        }

        private static void CreateFile()
        {
            File.Create("data.txt").Close();
            File.WriteAllLines("data.txt", new string[World.rows * World.cols]);
        }

        public static void SaveToFile()
        {
            int z = 0;
            string[] arr = new string[World.rows * World.cols];
            for (int i = 0; i < World.rows; i++)
                for (int j = 0; j < World.cols; j++)
                {
                    arr[z++] = (World.Map[i, j].Bot) ? "1" : "0"; // Cheking, cell is empty or not
                }
            
            if (!File.Exists("data.txt")) File.Create("data.txt").Close();

            File.WriteAllLines("data.txt", arr);
        }
    }
}
