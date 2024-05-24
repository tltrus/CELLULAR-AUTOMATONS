using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Sandpiles
{
    // Based on #107 — Sandpiles https://thecodingtrain.com/challenges/107-sandpiles
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        System.Windows.Threading.DispatcherTimer timer1, timer2;
        WriteableBitmap wb;
        int width, height;

        class Col
        {
            public byte R;
            public byte G;
            public byte B;
        }

        int[,] sandpiles, nextpiles;
        Col defaultColor = new Col() { R = 255, G = 0, B = 0 };
        Col[] colors = {
            new Col() { R = 255, G = 255, B = 0 },
            new Col() { R = 0, G = 185, B = 63 },
            new Col() { R = 0, G = 104, B = 255 },
            new Col() { R = 122, G = 0, B = 229 }
        };


        public MainWindow()
        {
            InitializeComponent();

            width = (int)g.Width;
            height = (int)g.Height;
            wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            g.Source = wb;

            timer1 = new System.Windows.Threading.DispatcherTimer();
            timer1.Tick += new EventHandler(timer1Tick);
            timer1.Interval = new TimeSpan(0, 0, 0, 0, 5);

            timer2 = new System.Windows.Threading.DispatcherTimer();
            timer2.Tick += new EventHandler(timer2Tick);
            timer2.Interval = new TimeSpan(0, 0, 0, 0, 50);

            sandpiles = new int[height, width];
            nextpiles = new int[height, width];

            sandpiles[height / 2, width / 2] = 1000000;

            timer1.Start();
            timer2.Start();
        }

        private void timer1Tick(object sender, EventArgs e) => Topple();
        private void timer2Tick(object sender, EventArgs e) => DrawFast();

        private void Topple()
        {
            Buffer.BlockCopy(sandpiles, 0, nextpiles, 0, sizeof(int) * sandpiles.Length);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int num = sandpiles[y, x];
                    if (num >= 4)
                    {
                        /*
                         * Здесь специально работаем с nextpiles, чтобы картинка расширялась симметрично.
                         * Смысл: пробегаем по циклу в sandpiles, а изменяем nextpiles. Т.к. если будем менять в sandpiles, то нарушим структуру массива.
                         */
                        nextpiles[y, x] -= 4;
                        if (x + 1 < width) nextpiles[y, x + 1]++;
                        if (x - 1 >= 0) nextpiles[y, x - 1]++;
                        if (y + 1 < height) nextpiles[y + 1, x]++;
                        if (y - 1 >= 0) nextpiles[y - 1, x]++;
                    }
                }
            Swap(ref sandpiles, ref nextpiles);
        }

        private void DrawFast()
        {
            try
            {
                wb.Lock();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var num = sandpiles[y, x];
                        var col = defaultColor;
                        if (num == 0)
                            col = colors[0];
                        else
                        if (num == 1)
                            col = colors[1];
                        else
                        if (num == 2)
                            col = colors[2];
                        else
                        if (num == 3)
                            col = colors[3];

                        unsafe
                        {
                            // Get a pointer to the back buffer.
                            IntPtr pBackBuffer = wb.BackBuffer;

                            // Find the address of the pixel to draw.
                            pBackBuffer += y * wb.BackBufferStride;
                            pBackBuffer += x * 4;

                            // {B, G, R, Apha}
                            int color = BitConverter.ToInt32(new byte[] { col.B, col.G, col.R, 255 }, 0);

                            // Assign the color data to the pixel.
                            *((int*)pBackBuffer) = color;
                        }

                        // Specify the area of the bitmap that changed.
                        wb.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                    }
                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                wb.Unlock();
            }
        }

        private void Draw()
        {
            // Присваивание цвета пикселю
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    var num = sandpiles[y, x];
                    var col = defaultColor;
                    if (num == 0)
                        col = colors[0];
                    else
                    if (num == 1)
                        col = colors[1];
                    else
                    if (num == 2)
                        col = colors[2];
                    else
                    if (num == 3)
                        col = colors[3];
                        
                    wb.SetPixel(x, y, col.R, col.G, col.B);
                }
        }

        private void Swap(ref int[,] a, ref int[,]b)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var temp = a[y, x];
                    a[y, x] = b[y, x];
                    b[y, x] = temp;
                }
        }
        private double Constrain(double n, double low, double high) => Math.Max(Math.Min(n, high), low);
    }
}
