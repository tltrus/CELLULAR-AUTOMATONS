using System;
using System.Collections.Generic;
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
using System.Threading;

namespace CA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        Random rnd = new Random();
        static int n = 13; // от 0 до 13
        static int y, x;
        static int xm1, xp1, ym1, yp1, sp1; 

        public static int height;  // Количество строк матрицы
        public static int width;   // Количество столбцов матрицы

        public static int age = 0;

        public static int[,] Map; public static int[,] NewMap;

        public int cellWidth = 3;
        public int fieldTop = 0;      // Отступ сверху
        public int fieldLeft = 0;     // Отступ слева
        public int offs_x = 0;
        public int offs_y = 0;

        Color color; // цвет заливки
        WriteableBitmap wb;

        delegate void TimerFunc(); // Объявляем делегат
        TimerFunc TimerControlFunc;

        string CyclicCAInfo = "Клетка из состояния m переходит в следующее состояние k, только если одна из соседних клеток имеет состояние k";
        string CyclicCarpet = "Мы определяем среднее значение состояния соседних клеток, добавляем сдвиг shift, маскируем маской mask (то есть выполняем логическое сложение с маской) и затем делаем так, чтобы полученное значение не превышало n-1 при помощи взятия остатка от деления";
        string CyclicMixed1 = "";
        string CyclicVenera = "Если начинать со случайно заполненного поля, то образуется два врезающихся друг в друга «потока»: вертикальный и горизонтальный.Через некоторое время один из «потоков» начинает преобладать.И в итоге остаётся только один.";
        string CyclicMixed2 = "Примем за n+1 количество состояний клетки в КА. Клетку в состоянии 0 будем считать “здоровой”, в состоянии n – больной. Промежуточные состояния отражают степень зараженности клетки – чем ближе к n, тем сильнее клетка больна. Если клетка здорова, то на следующем шаге она перейдет в состояние, зависящее от состояния окружающих ее соседей";
        string CyclicNoLimitGrow = "Клетки могут находиться в двух состояниях 0 — мёртвая, и 1 — живая. Клетка становится живой, если хотя бы один из её соседей живой; клетка, однажды став живой, остаётся живой всегда";

        // Для Коврика
        static int shift, mask;

        // Для Мешанины 2
        static int k1 = 2, k2 = 3, g = 5;
        Color[] colors = new Color[100];    // Массив цветов для Мешанины 2

        /// <summary>
        /// ИНИЦИАЛИЗАЦИЯ
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // Заполнение массива цветами для Мешанины 2
            for (double i = 0; i < 1; i += 0.01)
            {
                HSL.ColorRGB c = HSL.HSL2RGB(i, 0.5, 0.5);
                colors[(int)(i * 100)] = Color.FromRgb(c.R, c.G, c.B);
            }

            
            // Для Битмапа
            wb = new WriteableBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Bgra32, null);
            image.Source = wb;

            width = (int)image.Width / cellWidth;
            height = (int)image.Height / cellWidth;

            Map = new int[height, width];
            NewMap = new int[height, width];

            // Для коврика
            shift = 1; mask = 148;
            slidShift.Value = shift;
            slidMask.Value = mask;

            comb.SelectedIndex = 0;
            switch (comb.SelectedIndex)
            {
                case 0:
                    TimerControlFunc = ControlCyclicCA;
                    break;

                case 1:
                    TimerControlFunc = ControlCarpet;
                    break;

                case 2:
                    TimerControlFunc = ControlMixed1;
                    break;

                case 3:
                    TimerControlFunc = ControlVenera;
                        break;

                case 4:
                    TimerControlFunc = ControlMixed2;
                    break;
            }

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    NewMap[y, x] = rnd.Next(0, n + 1); // Заполняем в НьюМап чтобы потом лишний раз не делать копирование из Мапа в НьюМап

            FormDrawMap();
        }


        /// <summary>
        /// ТАЙМЕР
        /// </summary>

        private void timerTick(object sender, EventArgs e)
        {
            TimerControlFunc();

            //Array.Copy(NewMap, Map, NewMap.Length); // копирование массива
            FormDrawMap();          // Отрисовка карты
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// СОБЫТИЯ КНОПОК
        /// </summary>

        // Кнопка - Старт
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content != "Stop")
            {
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += new EventHandler(timerTick);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 10);

                timer.Start();

                btnStart.Content = "Stop";
                age = 0;
                StatusInfo.Text = "Шаг: 0";
            }
            else
            {
                timer.Stop();
                btnStart.Content = "Start";
            }
        }

        // Кнопка - ШАГ
        private void BtnStep_Click(object sender, RoutedEventArgs e)
        {
            TimerControlFunc();
            FormDrawMap();
        }

        // Кнопка - Заполнение карты рандомно
        private void BtnFillMap_Click(object sender, RoutedEventArgs e)
        {
            if (comb.SelectedIndex == 5)
            {
                // выбран автомат НЕОГРАНИЧЕННЫЙ РОСТ. У него специальное заполнение поля
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        NewMap[y, x] = 0; // Заполняем в НьюМап чтобы потом лишний раз не делать копирование из Мапа в НьюМап. 0 = мертвые клетки
                
                // Устанавливаем в центре поля одну живую
                NewMap[height / 2, width / 2] = 1;
            } 
            else
            {
                // остальные автоматы
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        NewMap[y, x] = rnd.Next(0, n + 1); // Заполняем в НьюМап чтобы потом лишний раз не делать копирование из Мапа в НьюМап
            }

            FormDrawMap();
        }

        // Событие при выборе типа автомата из выпадающего списка
        private void Comb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbl1.IsEnabled = false;
            lbl2.IsEnabled = false;
            slidShift.IsEnabled = false;
            slidMask.IsEnabled = false;

            switch (comb.SelectedIndex)
            {
                case 0:
                    TimerControlFunc = ControlCyclicCA;
                    tb.Text = CyclicCAInfo;
                    n = 13;
                    break;

                case 1:
                    TimerControlFunc = ControlCarpet;
                    tb.Text = CyclicCarpet;
                    n = 13;
                    lbl1.IsEnabled = true; 
                    lbl2.IsEnabled = true;
                    slidShift.IsEnabled = true;
                    slidMask.IsEnabled = true;
                    break;

                case 2:
                    TimerControlFunc = ControlMixed1;
                    tb.Text = CyclicMixed1;
                    n = 13;
                    break;

                case 3:
                    TimerControlFunc = ControlVenera;
                    tb.Text = CyclicVenera;
                    n = 3;
                    break;

                case 4:
                    TimerControlFunc = ControlMixed2;
                    tb.Text = CyclicMixed2;
                    n = 99;
                    break;

                case 5:
                    TimerControlFunc = ControlNoLimitGrow;
                    tb.Text = CyclicNoLimitGrow;
                    n = 99;
                    break;
            }
        }

        private void SlidShift_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            shift = (int)e.NewValue;
            lbl1.Content = shift.ToString();
        }

        private void SlidMask_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mask = (int)e.NewValue;
            lbl2.Content = mask.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ПРАВИЛА
        /// </summary>

        // Циклический клеточный автомат
        private static void RuleCyclicCA(int x, int y)
        {
            xm1 = ((x - 1) < 0) ? Map[y, width - 1] : xm1 = Map[y, x - 1];
            xp1 = ((x + 1) > width - 1) ? Map[y, 0] : Map[y, x + 1];

            ym1 = ((y - 1) < 0) ? Map[height - 1, x] : Map[y - 1, x];
            yp1 = ((y + 1) > height - 1) ? Map[0, x] : Map[y + 1, x];

            // рассчитываем следующее состояние
            sp1 = (Map[y, x] + 1) % (n + 1); // чтобы если в клетке статус n, то после mod получается 0

            // сравниваем клетки вокруг со следующим состоянием
            // если сосед = следующему состоянию текущей клетки, то сосед выигрывает
            if (xm1 == sp1 || xp1 == sp1 || ym1 == sp1 || yp1 == sp1)
                NewMap[y, x] = sp1;
            else
                NewMap[y, x] = Map[y, x];
        }

        // Ковёр
        private static void RuleCarpet(int x, int y)
        {
            xm1 = ((x - 1) < 0) ? Map[y, width - 1] : xm1 = Map[y, x - 1];
            xp1 = ((x + 1) > width - 1) ? Map[y, 0] : Map[y, x + 1];

            ym1 = ((y - 1) < 0) ? Map[height - 1, x] : Map[y - 1, x];
            yp1 = ((y + 1) > height - 1) ? Map[0, x] : Map[y + 1, x];

            var ass = (((Map[ym1, xm1] + Map[ym1, x] + Map[ym1, xp1] +
                Map[y, xm1] + Map[y, xp1] +
                Map[yp1, xm1] + Map[yp1, x] + Map[yp1, xp1]) / 8 << shift) | mask) % (n + 1);

            NewMap[y, x] = ass;
        }


        /// <summary>
        /// МЕШАНИНА 1
        /// Пусть снова клетка может находиться в состояниях 0, 1, ..., n – 1.
        /// Рассмотрим правило состоящее из трёх частей.
        /// Здоровье.Если клетка находится в состоянии 0 (т.е.здорова), то она заболевает, если несколько из её соседей больны (т.е.
        /// находятся в ненулевом состоянии).
        /// Выздоровление.Если клетка находится в состоянии n – 1, то она самовылечивается и переходит в состояние 0.
        /// Болезнь.Иначе же состояние клетки рассчитывается как среднее состояний соседних клеток плюс некоторая константа. (Если
        /// полученное число больше чем n – 1, то новым состоянием будет n – 1.)
        /// </summary>
        private static void RuleMixed1(int x, int y)
        {
            xm1= ((x - 1) + width) % width;
            xp1= (x + 1) % width;
            ym1= ((y - 1) + height) % height;
            yp1= (y + 1) % height;

            int northWestCell   = Map[ym1, xm1];
            int northCell       = Map[ym1, x];
            int northEastCell   = Map[ym1, xp1];
            int westCell        = Map[y, xm1];
            int eastCell        = Map[y, xp1];
            int southWestCell   = Map[yp1, xm1];
            int southCell       = Map[yp1, x];
            int southEastCell   = Map[yp1, xp1];



            //int s = Map[ym1, xm1] + Map[ym1, x] + Map[ym1, xp1] + Map[y, xm1] + Map[y, xp1] + Map[yp1, xm1] + Map[yp1, x] + Map[yp1, xp1];

            int sum = northWestCell + northCell + northEastCell +
                westCell + eastCell + southWestCell + southCell + southEastCell;

            if (Map[y, x] == 0) // если клетка здорова
            {
                if (sum < 5)
                {
                    NewMap[y, x] = 0;
                }
                else if (sum < 100)
                {
                    NewMap[y, x] = 2;
                }
                else
                {
                    NewMap[y, x] = 3;
                }
            }
            else if (Map[y, x] == n)
            {
                NewMap[y, x] = 0;
            }
            else
            {
                NewMap[y, x] = Math.Min(sum / 8 + 5, n);
            }
        }

        /// <summary>
        /// ПОВЕРХНОСТЬ ВЕНЕРЫ
        /// </summary>
        private static void RuleVenera(int x, int y)
        {
            xm1 = ((x - 1) + width) % width;
            xp1 = (x + 1) % width;
            ym1 = ((y - 1) + height) % height;
            yp1 = (y + 1) % height;

            int northWestCell = Map[ym1, xm1];
            int northCell = Map[ym1, x];
            int northEastCell = Map[ym1, xp1];
            int westCell = Map[y, xm1];
            int eastCell = Map[y, xp1];
            int southWestCell = Map[yp1, xm1];
            int southCell = Map[yp1, x];
            int southEastCell = Map[yp1, xp1];

            if (Map[y, x] == 0)
            {
                NewMap[y, x] = 2 * ((northWestCell % 2) ^ (northEastCell % 2)) + northCell % 2;
            }
            else if (Map[y, x] == 1)
            {
                NewMap[y, x] = 2 * ((northWestCell % 2) ^ (southWestCell % 2)) + westCell % 2;
            }
            else if (Map[y, x] == 2)
            {
                NewMap[y, x] = 2 * ((southWestCell % 2) ^ (southEastCell % 2)) + southCell % 2;
            }
            else
            {
                NewMap[y, x] = 2 * ((southEastCell % 2) ^ (northEastCell % 2)) + eastCell % 2;
            }
        }

        /// <summary>
        /// МЕШАНИНА 2
        /// </summary>
        private static void RuleMixed2(int x, int y)
        {
            xm1 = ((x - 1) + width) % width;
            xp1 = (x + 1) % width;
            ym1 = ((y - 1) + height) % height;
            yp1 = (y + 1) % height;

            int northWestCell = Map[ym1, xm1];
            int northCell = Map[ym1, x];
            int northEastCell = Map[ym1, xp1];
            int westCell = Map[y, xm1];
            int eastCell = Map[y, xp1];
            int southWestCell = Map[yp1, xm1];
            int southCell = Map[yp1, x];
            int southEastCell = Map[yp1, xp1];

            int a = 0, b = 0;

            if (northWestCell > 5 && northWestCell < n) a++; else b++;
            if (northCell > 5 && northCell < n) a++; else b++;
            if (northEastCell > 5 && northEastCell < n) a++; else b++;
            if (westCell > 5 && westCell < n) a++; else b++;
            if (eastCell > 5 && eastCell < n) a++; else b++;
            if (southWestCell > 5 && southWestCell < n) a++; else b++;
            if (southCell > 5 && southCell < n) a++; else b++;
            if (southEastCell > 5 && southEastCell < n) a++; else b++;

            int s = Map[y, x] + northWestCell + northCell + northEastCell + westCell + eastCell + southWestCell + southCell + southEastCell;


            if (Map[y, x] == n)
                NewMap[y, x] = 0;
            else 
            if (Map[y, x] == 0)
                NewMap[y, x] = Math.Min((a / k1) + (b / k2), n);
            else
                NewMap[y, x] = Math.Min((s / (a + 1)) + g, n);
        }

        /// <summary>
        /// НЕОГРАНИЧЕННЫЙ РОСТ
        /// 
        /// Правило.
        /// Живая клетка = 1
        /// Мертвая клетка = 0
        /// Изначально только одна клетка в центре экрана живая.
        /// </summary>
        private static void RuleNoLimitGrow(int x, int y)
        {
            xm1 = ((x - 1) + width) % width;
            xp1 = (x + 1) % width;
            ym1 = ((y - 1) + height) % height;
            yp1 = (y + 1) % height;

            int northWestCell = Map[ym1, xm1];
            int northCell = Map[ym1, x];
            int northEastCell = Map[ym1, xp1];
            int westCell = Map[y, xm1];
            int eastCell = Map[y, xp1];
            int southWestCell = Map[yp1, xm1];
            int southCell = Map[yp1, x];
            int southEastCell = Map[yp1, xp1];

            int sum = northWestCell + northCell + northEastCell +
                westCell + eastCell + southWestCell + southCell + southEastCell;

            // хотя бы один из соседей живой, то клетка становится живой
            if (sum == 1)
            {
                NewMap[y, x] = 1;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// МЕТОДЫ ТАЙМЕРА ДЛЯ РАЗНЫХ АВТОМАТОВ
        /// </summary>

        // Циклический автомат
        private static void ControlCyclicCA()
        {
            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                    RuleCyclicCA(x, y);
        }

        // Ковёр
        private static void ControlCarpet()
        {
            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                    RuleCarpet(x, y);
        }

        // Мешанина 1
        private static void ControlMixed1()
        {
            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                    RuleMixed1(x, y);
        }

        // Поверхность Венеры
        private static void ControlVenera()
        {
            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                    RuleVenera(x, y);
        }

        // Мешанина 2
        private static void ControlMixed2()
        {
            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                    RuleMixed2(x, y);
        }

        // Неограниченный рост
        private static void ControlNoLimitGrow()
        {
            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                    RuleNoLimitGrow(x, y);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ВЫВОД ГРАФИКИ
        /// </summary>

        // Обобщенный метод вызова функций отрисовки
        private void FormDrawMap()
        {
            StatusInfo.Text = "Шаг: " + age++.ToString();

            switch (comb.SelectedIndex)
            {
                case 0:
                    PaintCyclicCA();
                    break;

                case 1:
                    PaintCarpet();
                    break;

                case 2:
                    PaintMixed1();
                    break;

                case 3:
                    PaintVenera();
                    break;

                case 4:
                    PaintMixed2();
                    break;

                case 5:
                    PaintNoLimitGrow();
                    break;
            }

            StatusFPS.Text = "FPS: " + FPS.CalculateFrameRate().ToString();
        }

        // Циклический автомат
        private void PaintCyclicCA()
        {
            // по вертикали
            for (y = 0; y < height; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // по горизонтали
                for (x = 0; x < width; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;

                    Map[y, x] = NewMap[y, x];

                    if (Map[y, x] == 0)
                        color = (Color)ColorConverter.ConvertFromString("#fff3e0");
                    else if (Map[y, x] == 1)
                        color = (Color)ColorConverter.ConvertFromString("#ffe0b2");
                    else if (Map[y, x] == 2)
                        color = (Color)ColorConverter.ConvertFromString("#ffcc80");
                    else if (Map[y, x] == 3)
                        color = (Color)ColorConverter.ConvertFromString("#ffb74d");
                    else if (Map[y, x] == 4)
                        color = (Color)ColorConverter.ConvertFromString("#ffa726");
                    else if (Map[y, x] == 5)
                        color = (Color)ColorConverter.ConvertFromString("#ff9800");
                    else if (Map[y, x] == 6)
                        color = (Color)ColorConverter.ConvertFromString("#fb8c00");
                    else if (Map[y, x] == 7)
                        color = (Color)ColorConverter.ConvertFromString("#f57c00");
                    else if (Map[y, x] == 8)
                        color = (Color)ColorConverter.ConvertFromString("#ef6c00");
                    else if (Map[y, x] == 9)
                        color = (Color)ColorConverter.ConvertFromString("#e65100");
                    else if (Map[y, x] == 10)
                        color = (Color)ColorConverter.ConvertFromString("#d84315");
                    else if (Map[y, x] == 11)
                        color = (Color)ColorConverter.ConvertFromString("#bf360c");
                    else if (Map[y, x] == 12)
                        color = (Color)ColorConverter.ConvertFromString("#795548");
                    else if (Map[y, x] == 13)
                        color = (Color)ColorConverter.ConvertFromString("#3e2723");

                    wb.FillRectangle(offs_x, offs_y, offs_x + cellWidth, offs_y + cellWidth, color);
                }
            }
        }

        // Ковёр
        private void PaintCarpet()
        {
            // по вертикали
            for (y = 0; y < height; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // по горизонтали
                for (x = 0; x < width; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;

                    Map[y, x] = NewMap[y, x];

                    if (Map[y, x] == 0)
                        color = (Color)ColorConverter.ConvertFromString("#fff3e0");
                    else if (Map[y, x] == 1)
                        color = (Color)ColorConverter.ConvertFromString("#ffe0b2");
                    else if (Map[y, x] == 2)
                        color = (Color)ColorConverter.ConvertFromString("#ffcc80");
                    else if (Map[y, x] == 3)
                        color = (Color)ColorConverter.ConvertFromString("#ffb74d");
                    else if (Map[y, x] == 4)
                        color = (Color)ColorConverter.ConvertFromString("#ffa726");
                    else if (Map[y, x] == 5)
                        color = (Color)ColorConverter.ConvertFromString("#ff9800");
                    else if (Map[y, x] == 6)
                        color = (Color)ColorConverter.ConvertFromString("#fb8c00");
                    else if (Map[y, x] == 7)
                        color = (Color)ColorConverter.ConvertFromString("#f57c00");
                    else if (Map[y, x] == 8)
                        color = (Color)ColorConverter.ConvertFromString("#ef6c00");
                    else if (Map[y, x] == 9)
                        color = (Color)ColorConverter.ConvertFromString("#e65100");
                    else if (Map[y, x] == 10)
                        color = (Color)ColorConverter.ConvertFromString("#d84315");
                    else if (Map[y, x] == 11)
                        color = (Color)ColorConverter.ConvertFromString("#bf360c");
                    else if (Map[y, x] == 12)
                        color = (Color)ColorConverter.ConvertFromString("#795548");
                    else if (Map[y, x] == 13)
                        color = (Color)ColorConverter.ConvertFromString("#3e2723");

                    wb.FillRectangle(offs_x, offs_y, offs_x + cellWidth, offs_y + cellWidth, color);
                }
            }
        }

        // Мешанина 1
        private void PaintMixed1()
        {
            // по вертикали
            for (y = 0; y < height; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // по горизонтали
                for (x = 0; x < width; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;

                    Map[y, x] = NewMap[y, x];

                    if (Map[y, x] == 0)
                        color = (Color)ColorConverter.ConvertFromString("#fff3e0");
                    else if (Map[y, x] == 1)
                        color = (Color)ColorConverter.ConvertFromString("#ffe0b2");
                    else if (Map[y, x] == 2)
                        color = (Color)ColorConverter.ConvertFromString("#ffcc80");
                    else if (Map[y, x] == 3)
                        color = (Color)ColorConverter.ConvertFromString("#ffb74d");
                    else if (Map[y, x] == 4)
                        color = (Color)ColorConverter.ConvertFromString("#ffa726");
                    else if (Map[y, x] == 5)
                        color = (Color)ColorConverter.ConvertFromString("#ff9800");
                    else if (Map[y, x] == 6)
                        color = (Color)ColorConverter.ConvertFromString("#fb8c00");
                    else if (Map[y, x] == 7)
                        color = (Color)ColorConverter.ConvertFromString("#f57c00");
                    else if (Map[y, x] == 8)
                        color = (Color)ColorConverter.ConvertFromString("#ef6c00");
                    else if (Map[y, x] == 9)
                        color = (Color)ColorConverter.ConvertFromString("#e65100");
                    else if (Map[y, x] == 10)
                        color = (Color)ColorConverter.ConvertFromString("#d84315");
                    else if (Map[y, x] == 11)
                        color = (Color)ColorConverter.ConvertFromString("#bf360c");
                    else if (Map[y, x] == 12)
                        color = (Color)ColorConverter.ConvertFromString("#795548");
                    else if (Map[y, x] == 13)
                        color = (Color)ColorConverter.ConvertFromString("#3e2723");

                    wb.FillRectangle(offs_x, offs_y, offs_x + cellWidth, offs_y + cellWidth, color);
                }
            }
        }

        // Поверхность Венеры
        private void PaintVenera()
        {
            // по вертикали
            for (y = 0; y < height; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // по горизонтали
                for (x = 0; x < width; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;

                    Map[y, x] = NewMap[y, x];

                    if (Map[y, x] == 0)
                        color = Colors.Red;
                    else if (Map[y, x] == 1)
                        color = Colors.Blue;
                    else if (Map[y, x] == 2)
                        color = Colors.Green;
                    else if (Map[y, x] == 3)
                        color = Colors.Black;

                    wb.FillRectangle(offs_x, offs_y, offs_x + cellWidth, offs_y + cellWidth, color);
                }
            }
        }

        // Мешанина 2
        private void PaintMixed2()
        {
            // по вертикали
            for (y = 0; y < height; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // по горизонтали
                for (x = 0; x < width; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;
                    Map[y, x] = NewMap[y, x];
                    color = colors[Map[y, x]]; // подставляем цвет из массива цветов (массив создается при инициализации)

                    wb.FillRectangle(offs_x, offs_y, offs_x + cellWidth, offs_y + cellWidth, color);
                }
            }
        }

        // Неограниченный рост
        private void PaintNoLimitGrow()
        {
            // по вертикали
            for (y = 0; y < height; y++)
            {
                offs_y = (y - 1) * cellWidth + fieldTop;
                // по горизонтали
                for (x = 0; x < width; x++)
                {
                    offs_x = (x - 1) * cellWidth + fieldLeft;

                    Map[y, x] = NewMap[y, x];

                    if (Map[y, x] == 1)
                        color = (Color)ColorConverter.ConvertFromString("#000000");
                    else if (Map[y, x] == 0)
                        color = (Color)ColorConverter.ConvertFromString("#ffffff");

                    wb.FillRectangle(offs_x, offs_y, offs_x + cellWidth, offs_y + cellWidth, color);
                }
            }
        }

    }
}
