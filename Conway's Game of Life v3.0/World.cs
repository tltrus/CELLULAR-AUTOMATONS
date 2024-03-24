using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;


namespace GoL
{
    class World
    {
        public static int rows = 25;
        public static int cols = 30;

        public static int age = 1;

        public static Cells[,] Map = new Cells[rows, cols];
        public static Cells[,] NewMap = new Cells[rows, cols];

        public static int cellWidth = 15;
        public static int fieldTop = 16;
        public static int fieldLeft = 16;
        public static int offs_x = 0;
        public static int offs_y = 0;

        static BrushConverter converter = new BrushConverter();
        
        public const string red = "#FFFF1111";
        public const string green = "#FF11FF11";
        public const string blue = "#FF1111FF";
        public const string yellow = "#FFFFFF11";
        public const string white = "#FFFFFFFF";

        public static string botColor = green;

        public static void Paint(Cells[,] map, Canvas canvas)
        {
            // cols
            for (int y = 0; y < World.rows; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // rows
                for (int x = 0; x < World.cols; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;

                    Rectangle cell = new Rectangle()
                    {
                        Width = cellWidth,
                        Height = cellWidth,
                        Fill = Brushes.Gray,
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.1
                    };

                    // Cell is empty
                    if (map[y, x].Bot == false)
                    {
                        cell.Fill = Brushes.White;
                    }
                    else
                    // Cell is bot
                    if (map[y, x].Bot == true)
                    {
                        cell.Fill = converter.ConvertFromString(map[y, x].Color) as Brush;
                    }
                    // --------------------------

                    canvas.Children.Add(cell);

                    Canvas.SetLeft(cell, offs_x);
                    Canvas.SetTop(cell, offs_y);
                }
            }
            
        }
    }

    class Cells
    {
        public bool Bot { get; set; }
        public string Color { get; set; }

        public Cells(bool bot, string color)
        {
            Bot = bot;
            Color = color;
        }
    }
}

