using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace LA
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        int state = 0, y = 0, x = 0, size = 4;
        bool isWhite;
        WriteableBitmap wb;

        string red = "#FFFF0000";
        string white = "#FFFFFFFF";
        string gray = "#FF808080";
        string pixel;

        int age;

        public MainWindow()
        {
            InitializeComponent();

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            wb = new WriteableBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Bgra32, null);
            wb.FillRectangle(0, 0, (int)image.Width, (int)image.Height, Colors.Gray);
            image.Source = wb;


            x = (int)(image.Width / 2);
            y = (int)(image.Height / 2);


            lblwhite.Content = '\u250c'.ToString() + '\u2192'.ToString();
            lblred.Content = '\u2190'.ToString() + '\u2510'.ToString();
        }

        private void timerTick(object sender, EventArgs e) => Draw();

        private void Draw()
        {
            if (x < 0)
            {
                timer.Stop();
                btnStart.Content = "Start";
            }
            
            if (y >= 0)
            {
                pixel = wb.GetPixel(x, y).ToString();
            }
            else 
                pixel = gray;

            if (pixel == white || pixel == gray)
            {
                isWhite = true;
                wb.FillRectangle(x, y, x + size, y + size, Colors.Red);
            }
            else if (pixel == red)
            {
                isWhite = false;
                wb.FillRectangle(x, y, x + size, y + size, Colors.White);
            }

            //Turn
            if (isWhite) state += 1;
                else
                  state -= 1;

            if (state > 3) state = 0;
            if (state < 0) state = 3;


            //Movement
            switch (state)
            {
                case 0:
                {
                    y -= size;
                }
                break;
                case 1:
                {
                    x -= size; 
                }
                break;
                case 2:
                {
                    y += size;
                }
                break;
                case 3:
                {
                    x += size;
                }
                break;
            }
            lblAge.Content = age++;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
                btnStart.Content = "Stop";
            }
            else
            {
                timer.Stop();
                btnStart.Content = "Start";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            lblAge.Content = "0";
            age = 0;
            wb.FillRectangle(0, 0, (int)image.Width, (int)image.Height, Colors.Gray);
        }


        private void BtnStep_Click(object sender, RoutedEventArgs e) => Draw();
    }
}
