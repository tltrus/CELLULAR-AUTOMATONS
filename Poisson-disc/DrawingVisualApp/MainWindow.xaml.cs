using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VectorOperation;

namespace DrawingVisualApp
{
    // Based on #33 — Poisson-disc Sampling https://thecodingtrain.com/challenges/33-poisson-disc-sampling
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer Drawtimer;
        Random rnd = new Random();
        double width, height;

        DrawingVisual visual;
        DrawingContext dc;

        double w;
        int cols, rows, r = 4, k = 30;
        Vector2D[] grids;
        List<Vector2D> active = new List<Vector2D>();
        List<Vector2D> ordered = new List<Vector2D>();

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();
            width = window.Width;
            height = window.Height;

            var color = Color.FromArgb(150, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
            var brush = new SolidColorBrush(color);

            Init();

            Drawtimer = new System.Windows.Threading.DispatcherTimer();
            Drawtimer.Tick += new EventHandler(DrawtimerTick);
            Drawtimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            Drawtimer.Start();
        }

        private void Init()
        {
            w = r / Math.Sqrt(2);

            // STEP 0
            cols = (int)Math.Floor(width / w);
            rows = (int)Math.Floor(height / w);

            grids = new Vector2D[cols * rows];

            for (int n = 0; n < cols * rows; n++)
                grids[n] = new Vector2D();

            // STEP 1
            var x = width / 2;
            var y = height / 2;
            int i = (int)Math.Floor(x / w);
            int j = (int)Math.Floor(y / w);
            var pos = new Vector2D(x, y);
            grids[i + j * cols] = pos;
            active.Add(pos);
        }

        private void Drawing()
        {
            for (var total = 0; total < 25; total++)
            {
                if (active.Count > 0)
                {
                    var randIndex = (int)Math.Floor((decimal)rnd.Next(active.Count));
                    var pos = active[randIndex];
                    var found = false;
                    for (var n = 0; n < k; n++)
                    {
                        var sample = new Vector2D().Random2D(rnd);
                        var m = rnd.Next(r, 2 * r);
                        sample.SetMag(m);
                        sample.Add(pos);

                        int col = (int)Math.Floor(sample.x / w);
                        int row = (int)Math.Floor(sample.y / w);

                        if (col > -1 && row > -1 && col < cols && row < rows )
                        {
                            var ok = true;
                            for (var i = -1; i <= 1; i++)
                            {
                                for (var j = -1; j <= 1; j++)
                                {
                                    int index = (col + i) + (row + j) * cols;

                                    if (index >= grids.Length - 1)
                                    {
                                        Drawtimer.Stop();
                                        break;
                                    }

                                    var neighbor = grids[index];
                                    //if (neighbor != null)
                                    //{
                                        var d = Vector2D.Dist(sample, neighbor);
                                        if (d < r)
                                        {
                                            ok = false;
                                        }
                                    //}
                                }
                            }
                            if (ok)
                            {
                                found = true;
                                var index = col + row * cols;
                                grids[index] = sample;
                                active.Add(sample);
                                ordered.Add(sample);
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        active.RemoveAt(randIndex);
                    }
                }
            }

            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                for (var i = 0; i < ordered.Count; i++)
                {
                    var c = (byte)(i % 360);

                    var color = Color.FromRgb(c, c, c);
                    var brush = new SolidColorBrush(color);

                    Point p = new Point(ordered[i].x, ordered[i].y);
                    var radius = r * 0.5;
                    dc.DrawEllipse(brush, null, p, radius, radius);
                }
                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void DrawtimerTick(object sender, EventArgs e) => Drawing();
    }
}
