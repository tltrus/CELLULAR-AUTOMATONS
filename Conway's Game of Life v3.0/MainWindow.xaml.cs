using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoL
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        int oldnumcell = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            for (int y = 0; y < World.rows; y++)
            {
                for (int x = 0; x < World.cols; x++)
                {
                    World.Map[y, x] = new Cells(false, World.white);
                    World.NewMap[y, x] = new Cells(false, World.white);
                }
            }

            Files.FileRead();


            switch (World.botColor)
            {
                case World.red:
                    recRed.StrokeThickness = 4;
                    recGreen.StrokeThickness = 1;
                    recBlue.StrokeThickness = 1;
                    recYellow.StrokeThickness = 1;
                    break;
                case World.green:
                    recRed.StrokeThickness = 1;
                    recGreen.StrokeThickness = 4;
                    recBlue.StrokeThickness = 1;
                    recYellow.StrokeThickness = 1;
                    break;
                case World.blue:
                    recRed.StrokeThickness = 1;
                    recGreen.StrokeThickness = 1;
                    recBlue.StrokeThickness = 4;
                    recYellow.StrokeThickness = 1;
                    break;
                case World.yellow:
                    recRed.StrokeThickness = 1;
                    recGreen.StrokeThickness = 1;
                    recBlue.StrokeThickness = 1;
                    recYellow.StrokeThickness = 4;
                    break;
                default:
                    break;
            }

            FormDrawMap();          // Отрисовка карты
        }

        private void FormDrawMap()
        {
            canvas.Children.Clear();    // Очистка canvas от нарисованного
            World.Paint(World.Map, canvas);
        }

        private void timerTick(object sender, EventArgs e)
        {
            BotControl();
        }

        // Основной цикл -------------------------------------
        private void BotControl()
        {
            bool noBotOnMap = false; // для остановки таймера, если на карте не будет ни одного бота

            for (var y = 0; y < World.rows; y++)
            {
                for (var x = 0; x < World.cols; x++)
                {
                    if (World.Map[y, x].Bot) noBotOnMap = true; // в каждом цикле проверяем наличие бота и сохраняем true, если находим

                    var alive_now = World.Map[y, x].Bot;
                    string alive_color = "";

                    int number_neighbors = L(x, y - 1, ref alive_color) + L(x, y + 1, ref alive_color) + L(x - 1, y - 1, ref alive_color) + L(x - 1, y, ref alive_color) + L(x - 1, y + 1, ref alive_color) + L(x + 1, y - 1, ref alive_color) + L(x + 1, y, ref alive_color) + L(x + 1, y + 1, ref alive_color);

                    if (number_neighbors == 2 && alive_now || (number_neighbors == 3))
                    {
                        World.NewMap[y, x].Bot = true;
                        World.NewMap[y, x].Color = alive_color;
                    }
                }
            }

            // Копирование массива из новой карты в старую
            for (int y = 0; y < World.rows; y++)
            {
                for (int x = 0; x < World.cols; x++)
                {
                    World.Map[y, x].Bot = World.NewMap[y, x].Bot;
                    World.Map[y, x].Color = World.NewMap[y, x].Color;
                }
                    
            }

            if (!noBotOnMap) timer.Stop(); // останавливаем таймер, если на карте нет ни одного бота

            FormDrawMap();          // Отрисовка карты

            resetMap(World.NewMap);

            lblAge.Content = World.age++;
        }

        private void resetMap(Cells[,] m)
        {
            for (int y = 0; y < World.rows; y++)
            {
                for (int x = 0; x < World.cols; x++)
                {
                    m[y, x].Bot = false;
                    m[y, x].Color = World.white;
                }
            }
        }

        private int L(int x, int y, ref string ass)
        {
            if (x >= 0 && x < World.cols && y >= 0 && y < World.rows)
            {
                if (World.Map[y, x].Bot)
                {
                    string a1 = "FFFFFF";
                    string a2 = World.Map[y, x].Color.Remove(0, 1);

                    int dec1 = Convert.ToInt32(a1, 16);
                    int dec2 = Convert.ToInt32(a2, 16);
                    int aa = dec1 & dec2;
                    ass = "#" + aa.ToString("X");
                    return 1;
                }
            }
            return 0;
        }

        void CellBotOnOff(MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);

            int X = (int)p.X / World.cellWidth;
            int Y = (int)p.Y / World.cellWidth;

            if ((X > 29) || (Y > 24)) return; // завершаем работу если нажали за пределы сетки


            if (World.Map[Y, X].Bot)
            {
                // Если при нажатии на клетку цвет кисти отличается от цвета клетки
                if (World.Map[Y, X].Color != World.botColor)
                {
                    World.Map[Y, X].Color = World.botColor;    // перекрашиваем цвет клетки
                }
                else
                {
                    // если цвет кисти тот же, то просто сбрасываем клетку
                    World.Map[Y, X].Bot = false;            // сбрасываем клетку
                    World.Map[Y, X].Color = World.white;    // 
                }
            }
            else
            {
                // если нажатие на пустую клетку, то рисуется новая клетка
                World.Map[Y, X].Bot = true;
                World.Map[Y, X].Color = World.botColor;
            }

            FormDrawMap();
        }


        /// <summary>
        /// СОБЫТИЯ НАЖАТИЯ НА КАНВАС
        /// </summary>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(canvas);

                int X = (int)p.X / World.cellWidth;
                int Y = (int)p.Y / World.cellWidth;

                int index = (Y * World.cols) + X;

                if (index != oldnumcell)
                {
                    CellBotOnOff(e);
                    oldnumcell = index; // запоминаем индекс клетки
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point p = e.GetPosition(canvas);

            int X = (int)p.X / World.cellWidth;
            int Y = (int)p.Y / World.cellWidth;

            oldnumcell = (Y * World.cols) + X;

            CellBotOnOff(e);
        }


        /// <summary>
        /// СОБЫТИЯ УПРАВЛЯЮЩИХ КНОПОК
        /// </summary>
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content != "Stop")
            {
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += new EventHandler(timerTick);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 50);

                timer.Start();

                btnStart.Content = "Stop";
                lblAge.Content = World.age = 0;
            }
            else
            {
                timer.Stop();
                btnStart.Content = "Start";
            }

        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            resetMap(World.Map);
            World.Paint(World.Map, canvas);
        } // Очистка сетки

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Files.SaveToFile();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            Files.FileRead();

            FormDrawMap();          // Отрисовка карты
        }

        private void BtnStep_Click(object sender, RoutedEventArgs e)
        {
            BotControl();
        }


        /// <summary>
        /// СОБЫТИЯ ЦВЕТНЫХ КНОПОК
        /// </summary>
        private void RecRed_MouseDown(object sender, MouseButtonEventArgs e)
        {
            World.botColor = World.red;

            recRed.StrokeThickness = 4;
            recGreen.StrokeThickness = 1;
            recBlue.StrokeThickness = 1;
            recYellow.StrokeThickness = 1;
        }

        private void RecGreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            World.botColor = World.green;

            recRed.StrokeThickness = 1;
            recGreen.StrokeThickness = 4;
            recBlue.StrokeThickness = 1;
            recYellow.StrokeThickness = 1;
        }

        private void RecBlue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            World.botColor = World.blue;

            recRed.StrokeThickness = 1;
            recGreen.StrokeThickness = 1;
            recBlue.StrokeThickness = 4;
            recYellow.StrokeThickness = 1;
        }

        private void RecYellow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            World.botColor = World.yellow;

            recRed.StrokeThickness = 1;
            recGreen.StrokeThickness = 1;
            recBlue.StrokeThickness = 1;
            recYellow.StrokeThickness = 4;
        }

    }
}
