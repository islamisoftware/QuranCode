using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Model;

public enum Direction { Right, Up, Left, Down };
public enum DrawingShape { Spiral, SquareSpiral, Square, HGoldenRect, VGoldenRect, Cube, HGoldenCube, VGoldenCube };

public static class Drawing
{
    private static string s_drawings_directory = "Drawings";
    static Drawing()
    {
        if (!Directory.Exists(s_drawings_directory))
        {
            Directory.CreateDirectory(s_drawings_directory);
        }
    }

    public static void DrawValues(Bitmap bitmap, List<long> values, Color color, DrawingShape shape)
    {
        if (values != null)
        {
            int count = values.Count;

            int width = 0;
            int height = 0;

            switch (shape)
            {
                case DrawingShape.Spiral:
                    {
                        DrawValuesSpiral(bitmap, values, color);
                        return;
                    }
                case DrawingShape.SquareSpiral:
                    {
                        DrawValuesSquareSpiral(bitmap, values, color);
                        return;
                    }
                case DrawingShape.Square:
                case DrawingShape.Cube:
                    {
                        width = (int)Math.Sqrt(count + 1);
                        height = width;
                    }
                    break;
                case DrawingShape.HGoldenRect:
                case DrawingShape.HGoldenCube:
                    {
                        width = (int)Math.Sqrt((count * Numbers.PHI) + 1);
                        height = (int)Math.Sqrt((count / Numbers.PHI) + 1);
                    }
                    break;
                case DrawingShape.VGoldenRect:
                case DrawingShape.VGoldenCube:
                    {
                        width = (int)Math.Sqrt((count / Numbers.PHI) + 1);
                        height = (int)Math.Sqrt((count * Numbers.PHI) + 1);
                    }
                    break;
            }

            double normalizer = 1.0;
            long max_value = long.MinValue;
            foreach (long value in values)
            {
                if (max_value < value)
                {
                    max_value = value;
                }
            }
            if (max_value > 0)
            {
                normalizer = (255.0 / max_value);
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                if (graphics != null)
                {
                    int dx = bitmap.Width / width;
                    int dy = bitmap.Height / height;
                    //int x_shift = (bitmap.Width - dx * width) / 2;
                    //int y_shift = (bitmap.Height - dy * height) / 2;

                    graphics.Clear(Color.Black); // set background
                    for (int i = 0; i < count; i++)
                    {
                        // draw point at new location in color shaded by numerology value
                        int r = (int)((values[i] * normalizer) * (color.R / 255.0));
                        int g = (int)((values[i] * normalizer) * (color.G / 255.0));
                        int b = (int)((values[i] * normalizer) * (color.B / 255.0));
                        Color value_color = Color.FromArgb(r, g, b);
                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            //graphics.FillRectangle(brush, new Rectangle(x_shift + (i % width) * dx, y_shift + (i / width) * dy, dx, dy));
                            graphics.FillRectangle(brush, new Rectangle((i % width) * dx, (i / width) * dy, dx, dy));
                        }
                    }
                }
            }
        }
    }
    private static void DrawValuesSpiral(Bitmap bitmap, List<long> values, Color color)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                int count = values.Count;
                int width = (int)Math.Sqrt(count) + 1;
                int height = width;
                int dx = 1;
                int dy = 1;
                int x_shift = (bitmap.Width - dx * width) / 2;
                int y_shift = (bitmap.Height - dy * height) / 2;
                int x = (bitmap.Width - x_shift) / 2;
                int y = (bitmap.Height - y_shift) / 2;

                if (values != null)
                {
                    double normalizer = 1.0;
                    long max_value = long.MinValue;
                    foreach (long value in values)
                    {
                        if (max_value < value)
                        {
                            max_value = value;
                        }
                    }
                    if (max_value > 0)
                    {
                        normalizer = (255.0 / max_value);
                    }

                    PointF[] points = new PointF[count];
                    float angle = 0.0F;
                    float scale = 0.0F;
                    for (int i = 0; i < count; i++)
                    {
                        angle = (float)((-2 * Math.PI * i) / (x + y));
                        scale = (float)i / (count / 1.0F);

                        points[i].X = (float)(x * (1 + scale * Math.Cos(angle))) + x_shift / 2;
                        points[i].Y = (float)(y * (1 + scale * Math.Sin(angle))) + y_shift / 2;

                        // draw point at new location in color shaded by numerology value
                        int r = (int)((values[i] * normalizer) * (color.R / 255.0));
                        int g = (int)((values[i] * normalizer) * (color.G / 255.0));
                        int b = (int)((values[i] * normalizer) * (color.B / 255.0));
                        Color value_color = Color.FromArgb(r, g, b);
                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, points[i].X, points[i].Y, dx, dy);
                        }
                    }
                }
            }
        }
    }
    private static void DrawValuesSquareSpiral(Bitmap bitmap, List<long> values, Color color)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                int count = values.Count;
                int width = (int)Math.Sqrt(count) + 1;
                int height = width;
                int dx = 1;
                int dy = 1;
                int x_shift = (bitmap.Width - dx * width) / 2;
                int y_shift = (bitmap.Height - dy * height) / 2;
                int x = (bitmap.Width - x_shift) / 2;
                int y = (bitmap.Height - y_shift) / 2;

                Direction direction = Direction.Right;
                int steps = 1;
                int steps_in_directoin = steps;

                if (values != null)
                {
                    double normalizer = 1.0;
                    long max_value = long.MinValue;
                    foreach (long value in values)
                    {
                        if (max_value < value)
                        {
                            max_value = value;
                        }
                    }
                    if (max_value > 0)
                    {
                        normalizer = (255.0 / max_value);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        // draw point at new location in color shaded by numerology value
                        int r = (int)((values[i] * normalizer) * (color.R / 255.0));
                        int g = (int)((values[i] * normalizer) * (color.G / 255.0));
                        int b = (int)((values[i] * normalizer) * (color.B / 255.0));
                        Color value_color = Color.FromArgb(r, g, b);
                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, x, y, dx, dy);
                        }

                        // has direction finished?
                        if (steps == 0)
                        {
                            // change direction
                            switch (direction)
                            {
                                case Direction.Right: { direction = Direction.Up; } break;
                                case Direction.Up: { direction = Direction.Left; steps_in_directoin++; } break;
                                case Direction.Left: { direction = Direction.Down; } break;
                                case Direction.Down: { direction = Direction.Right; steps_in_directoin++; } break;
                            }
                            steps = steps_in_directoin;
                        }

                        // move one step in current direction
                        switch (direction)
                        {
                            case Direction.Right: x += dx; break;
                            case Direction.Up: y -= dy; break;
                            case Direction.Left: x -= dx; break;
                            case Direction.Down: y += dy; break;
                        }

                        // one step done
                        steps--;
                    }
                }
            }
        }
    }
    //private static void DrawValuesCube(Bitmap bitmap, List<long> values, Color color)
    //{
    //    using (Graphics graphics = Graphics.FromImage(bitmap))
    //    {
    //        if (graphics != null)
    //        {
    //            graphics.Clear(Color.Black); // set background
    //            int count = values.Count;
    //            int width = (int)Math.Pow(count + 1, 1.0 / 3.0);
    //            int height = width;
    //            int layers = width;

    //            if (values != null)
    //            {
    //                double normalizer = 1.0;
    //                long max_value = long.MinValue;
    //                foreach (long value in values)
    //                {
    //                    if (max_value < value)
    //                    {
    //                        max_value = value;
    //                    }
    //                }
    //                if (max_value > 0)
    //                {
    //                    normalizer = (255.0 / max_value);
    //                }

    //                if (graphics != null)
    //                {
    //                    graphics.Clear(Color.Black); // set background
    //                    for (int n = 0; n < layers; n++)
    //                    {
    //                        for (int i = 0; i < (width * height); i++)
    //                        {
    //                            int dx = 1;
    //                            int dy = 1;
    //                            int x_shift = (bitmap.Width - dx * width) / 2;
    //                            int y_shift = (bitmap.Height - dy * height) / 2;

    //                            // draw point at new location in color shaded by numerology value
    //                            int r = (int)((values[n * (width * height) + i] * normalizer) * (color.R / 255.0));
    //                            int g = (int)((values[n * (width * height) + i] * normalizer) * (color.G / 255.0));
    //                            int b = (int)((values[n * (width * height) + i] * normalizer) * (color.B / 255.0));
    //                            Color value_color = Color.FromArgb(r, g, b);
    //                            using (SolidBrush brush = new SolidBrush(value_color))
    //                            {
    //                                graphics.FillRectangle(brush, new Rectangle(x_shift + (i % width) * dx, y_shift + (i / width) * dy, dx, dy));
    //                            }
    //                        }

    //                        // wait before drawing next layer
    //                        Thread.Sleep(100);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    public static void DrawValues(Bitmap bitmap, List<long> values, Color color1, Color color2, Color color3, DrawingShape shape)
    {
        if (values != null)
        {
            int count = values.Count;

            int width = 0;
            int height = 0;

            switch (shape)
            {
                case DrawingShape.Spiral:
                    {
                        DrawValuesSpiral(bitmap, values, color1, color2, color3);
                        return;
                    }
                case DrawingShape.SquareSpiral:
                    {
                        DrawValuesSquareSpiral(bitmap, values, color1, color2, color3);
                        return;
                    }
                case DrawingShape.Square:
                case DrawingShape.Cube:
                    {
                        width = (int)Math.Sqrt(count + 1);
                        height = width;
                    }
                    break;
                case DrawingShape.HGoldenRect:
                case DrawingShape.HGoldenCube:
                    {
                        width = (int)Math.Sqrt((count * Numbers.PHI) + 1);
                        height = (int)Math.Sqrt((count / Numbers.PHI) + 1);
                    }
                    break;
                case DrawingShape.VGoldenRect:
                case DrawingShape.VGoldenCube:
                    {
                        width = (int)Math.Sqrt((count / Numbers.PHI) + 1);
                        height = (int)Math.Sqrt((count * Numbers.PHI) + 1);
                    }
                    break;
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                if (graphics != null)
                {
                    graphics.Clear(Color.Black); // set background
                    for (int i = 0; i < count; i++)
                    {
                        int dx = bitmap.Width / width;
                        int dy = bitmap.Height / height;
                        //int x_shift = (bitmap.Width - dx * width) / 2;
                        //int y_shift = (bitmap.Height - dy * height) / 2;

                        Color value_color = Color.Black;
                        if (values[i] == 3)
                        {
                            value_color = color3;
                        }
                        else if (values[i] == 2)
                        {
                            value_color = color2;
                        }
                        else if (values[i] == 1)
                        {
                            value_color = color1;
                        }
                        else if (values[i] == 0)
                        {
                            value_color = Color.Black;
                        }
                        else
                        {
                            continue;
                        }

                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            //graphics.FillRectangle(brush, new Rectangle(x_shift + (i % width) * dx, y_shift + (i / width) * dy, dx, dy));
                            graphics.FillRectangle(brush, new Rectangle((i % width) * dx, (i / width) * dy, dx, dy));
                        }
                    }
                }
            }
        }
    }
    private static void DrawValuesSpiral(Bitmap bitmap, List<long> values, Color color1, Color color2, Color color3)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                if (values != null)
                {
                    int count = values.Count;
                    int width = (int)Math.Sqrt(count) + 1;
                    int height = width;
                    int dx = 1;
                    int dy = 1;
                    int x_shift = (bitmap.Width - dx * width) / 2;
                    int y_shift = (bitmap.Height - dy * height) / 2;
                    int x = (bitmap.Width - x_shift) / 2;
                    int y = (bitmap.Height - y_shift) / 2;

                    PointF[] points = new PointF[count];
                    float angle = 0.0F;
                    float scale = 0.0F;
                    for (int i = 0; i < count; i++)
                    {
                        angle = (float)((-2 * Math.PI * i) / (x + y));
                        scale = (float)i / (count / 1.0F);

                        points[i].X = (float)(x * (1 + scale * Math.Cos(angle))) + x_shift / 2;
                        points[i].Y = (float)(y * (1 + scale * Math.Sin(angle))) + y_shift / 2;

                        Color value_color = Color.Black;
                        if (values[i] == 3)
                        {
                            value_color = color3;
                        }
                        else if (values[i] == 2)
                        {
                            value_color = color2;
                        }
                        else if (values[i] == 1)
                        {
                            value_color = color1;
                        }
                        else if (values[i] == 0)
                        {
                            value_color = Color.Black;
                        }
                        else
                        {
                            continue;
                        }

                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, points[i].X, points[i].Y, dx, dy);
                        }
                    }
                }
            }
        }
    }
    private static void DrawValuesSquareSpiral(Bitmap bitmap, List<long> values, Color color1, Color color2, Color color3)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            if (graphics != null)
            {
                graphics.Clear(Color.Black); // set background
                int count = values.Count;
                int width = (int)Math.Sqrt(count) + 1;
                int height = width;
                int dx = 1;
                int dy = 1;
                int x_shift = (bitmap.Width - dx * width) / 2;
                int y_shift = (bitmap.Height - dy * height) / 2;
                int x = (bitmap.Width - x_shift) / 2;
                int y = (bitmap.Height - y_shift) / 2;

                Direction direction = Direction.Right;
                int steps = 1;
                int steps_in_directoin = steps;

                if (values != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Color value_color = Color.Black;
                        if (values[i] == 3)
                        {
                            value_color = color3;
                        }
                        else if (values[i] == 2)
                        {
                            value_color = color2;
                        }
                        else if (values[i] == 1)
                        {
                            value_color = color1;
                        }
                        else if (values[i] == 0)
                        {
                            value_color = Color.Black;
                        }
                        else
                        {
                            continue;
                        }

                        using (SolidBrush brush = new SolidBrush(value_color))
                        {
                            graphics.FillRectangle(brush, x, y, dx, dy);
                        }

                        // has direction finished?
                        if (steps == 0)
                        {
                            // change direction
                            switch (direction)
                            {
                                case Direction.Right: { direction = Direction.Up; } break;
                                case Direction.Up: { direction = Direction.Left; steps_in_directoin++; } break;
                                case Direction.Left: { direction = Direction.Down; } break;
                                case Direction.Down: { direction = Direction.Right; steps_in_directoin++; } break;
                            }
                            steps = steps_in_directoin;
                        }

                        // move one step in current direction
                        switch (direction)
                        {
                            case Direction.Right: x += dx; break;
                            case Direction.Up: y -= dy; break;
                            case Direction.Left: x -= dx; break;
                            case Direction.Down: y += dy; break;
                        }

                        // one step done
                        steps--;
                    }
                }
            }
        }
    }

    private const int WIDTH = 1000;
    private const int HEIGHT = 1000;
    public static void SaveDrawing(String filename, Bitmap bitmap)
    {
        if (bitmap != null)
        {
            try
            {
                filename = s_drawings_directory + "/" + filename;
                bitmap.Save(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
    public static void GenerateAndSaveAllPrimeDrawings(Color color1, Color color2, Color color3)
    {
        GeneratePrimeBitmaps(NumberType.Prime, color1, color2, color3);
        GeneratePrimeBitmaps(NumberType.AdditivePrime, color1, color2, color3);
        GeneratePrimeBitmaps(NumberType.PurePrime, color1, color2, color3);
        GenerateAllPrimeBitmaps(color1, color2, color3);

        GeneratePrimeSpiralBitmaps(NumberType.Prime, color1, color2, color3);
        GeneratePrimeSpiralBitmaps(NumberType.AdditivePrime, color1, color2, color3);
        GeneratePrimeSpiralBitmaps(NumberType.PurePrime, color1, color2, color3);
        GenerateAllPrimeSpiralBitmaps(color1, color2, color3);

        GeneratePrimeSquareSpiralBitmaps(NumberType.Prime, color1, color2, color3);
        GeneratePrimeSquareSpiralBitmaps(NumberType.AdditivePrime, color1, color2, color3);
        GeneratePrimeSquareSpiralBitmaps(NumberType.PurePrime, color1, color2, color3);
        GenerateAllPrimeSquareSpiralBitmaps(color1, color2, color3);

        System.Diagnostics.Process.Start(s_drawings_directory);
    }

    public static void GeneratePrimeBitmaps(NumberType number_type, Color color1, Color color2, Color color3)
    {        
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color prime_color = Color.Black;
                switch (number_type)
                {
                    case NumberType.Prime:
                        prime_color = color1;
                        break;
                    case NumberType.AdditivePrime:
                        prime_color = color2;
                        break;
                    case NumberType.PurePrime:
                        prime_color = color3;
                        break;
                    default:
                        prime_color = Color.Black;
                        break;
                }

                int max = WIDTH * HEIGHT;
                int prime_count = 0;
                for (int i = 0; i < max; i++)
                {
                    int x = (i % WIDTH);
                    int y = (i / WIDTH);
                    if (Numbers.IsNumberType((i + 1), number_type))
                    {
                        bitmap.SetPixel(x, y, prime_color);
                        prime_count++;
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                }

                String filename = String.Format(number_type.ToString() + "_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, prime_count);
                SaveDrawing(filename, bitmap);
            }
        }
    }
    public static void GeneratePrimeSpiralBitmaps(NumberType number_type, Color color1, Color color2, Color color3)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color prime_color = Color.Black;
                switch (number_type)
                {
                    case NumberType.Prime:
                        prime_color = color1;
                        break;
                    case NumberType.AdditivePrime:
                        prime_color = color2;
                        break;
                    case NumberType.PurePrime:
                        prime_color = color3;
                        break;
                    default:
                        prime_color = Color.Black;
                        break;
                }

                int x = (bitmap.Width) / 2;
                int y = (bitmap.Height) / 2;

                int prime_count = 0;
                int max = WIDTH * HEIGHT;
                PointF[] points = new PointF[max];
                float angle = 0.0F;
                float scale = 0.0F;
                for (int i = 0; i < max; i++)
                {
                    angle = (float)((-2 * Math.PI * i) / (x + y));
                    scale = (float)i / (max / 1.0F);

                    points[i].X = (float)(x * (1 + scale * Math.Cos(angle)));
                    points[i].Y = (float)(y * (1 + scale * Math.Sin(angle)));
                    if (Numbers.IsNumberType((i + 1), number_type))
                    {
                        bitmap.SetPixel((int)points[i].X, (int)points[i].Y, prime_color);
                        prime_count++;
                    }
                    else
                    {
                        bitmap.SetPixel((int)points[i].X, (int)points[i].Y, Color.Black);
                    }
                }

                String filename = String.Format(number_type.ToString() + "Spiral_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, prime_count);
                SaveDrawing(filename, bitmap);
            }
        }
    }
    public static void GeneratePrimeSquareSpiralBitmaps(NumberType number_type, Color color1, Color color2, Color color3)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color prime_color = Color.Black;
                switch (number_type)
                {
                    case NumberType.Prime:
                        prime_color = color1;
                        break;
                    case NumberType.AdditivePrime:
                        prime_color = color2;
                        break;
                    case NumberType.PurePrime:
                        prime_color = color3;
                        break;
                    default:
                        prime_color = Color.Black;
                        break;
                }

                // initialize first step
                int x = (WIDTH / 2) - 1;
                int y = (HEIGHT / 2);
                Direction direction = Direction.Right;
                int steps = 1;
                int remaining_steps = steps;

                int max = WIDTH * HEIGHT;
                int prime_count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsNumberType((i + 1), number_type))
                    {
                        bitmap.SetPixel(x, y, prime_color);
                        prime_count++;
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }

                    // has direction finished?
                    if (remaining_steps == 0)
                    {
                        // change direction
                        switch (direction)
                        {
                            case Direction.Right: { direction = Direction.Up; } break;
                            case Direction.Up: { direction = Direction.Left; steps++; } break;
                            case Direction.Left: { direction = Direction.Down; } break;
                            case Direction.Down: { direction = Direction.Right; steps++; } break;
                        }
                        remaining_steps = steps;
                    }

                    // move one step in current direction
                    switch (direction)
                    {
                        case Direction.Right: x += 1; break;
                        case Direction.Up: y -= 1; break;
                        case Direction.Left: x -= 1; break;
                        case Direction.Down: y += 1; break;
                    }

                    // one step done
                    remaining_steps--;
                }

                String filename = String.Format(number_type.ToString() + "SquareSpiral_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, prime_count);
                SaveDrawing(filename, bitmap);
            }
        }
    }

    public static void GenerateAllPrimeBitmaps(Color color1, Color color2, Color color3)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                Color color = Color.Black;
                int max = WIDTH * HEIGHT;
                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsPurePrime(i + 1)) color = color3;
                    else if (Numbers.IsAdditivePrime(i + 1)) color = color2;
                    else if (Numbers.IsPrime(i + 1)) color = color1;
                    else color = Color.Black;
                    count += (color != Color.Black) ? 1 : 0;

                    int x = (i % WIDTH);
                    int y = (i / WIDTH);
                    bitmap.SetPixel(x, y, color);
                }

                String filename = String.Format("AllPrimes_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, count);
                SaveDrawing(filename, bitmap);
            }
        }
    }
    public static void GenerateAllPrimeSpiralBitmaps(Color color1, Color color2, Color color3)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                int x = (bitmap.Width) / 2;
                int y = (bitmap.Height) / 2;

                int max = WIDTH * HEIGHT;
                int count = 0;
                PointF[] points = new PointF[max];
                float angle = 0.0F;
                float scale = 0.0F;
                for (int i = 0; i < max; i++)
                {
                    Color color = Color.Black;
                    if (Numbers.IsPurePrime(i + 1)) color = color3;
                    else if (Numbers.IsAdditivePrime(i + 1)) color = color2;
                    else if (Numbers.IsPrime(i + 1)) color = color1;
                    else color = Color.Black;
                    count += (color != Color.Black) ? 1 : 0;

                    angle = (float)((-2 * Math.PI * i) / (x + y));
                    scale = (float)i / (max / 1.0F);

                    points[i].X = (float)(x * (1 + scale * Math.Cos(angle)));
                    points[i].Y = (float)(y * (1 + scale * Math.Sin(angle)));
                    bitmap.SetPixel((int)points[i].X, (int)points[i].Y, color);
                }

                String filename = String.Format("AllPrimeSpiral_{0:000}x{1:000}_{2:0000}.bmp", bitmap.Width, bitmap.Height, count);
                SaveDrawing(filename, bitmap);
            }
        }
    }
    public static void GenerateAllPrimeSquareSpiralBitmaps(Color color1, Color color2, Color color3)
    {
        using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb))
        {
            if (bitmap != null)
            {
                // initialize first step
                int x = (WIDTH / 2) - 1;
                int y = (HEIGHT / 2);
                Direction direction = Direction.Right;
                int steps = 1;
                int remaining_steps = steps;

                Color color = Color.Black;
                int max = WIDTH * HEIGHT;
                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsPurePrime(i + 1)) color = color3;
                    else if (Numbers.IsAdditivePrime(i + 1)) color = color2;
                    else if (Numbers.IsPrime(i + 1)) color = color1;
                    else color = Color.Black;
                    count += (color != Color.Black) ? 1 : 0;

                    bitmap.SetPixel(x, y, color);

                    // has direction finished?
                    if (remaining_steps == 0)
                    {
                        // change direction
                        switch (direction)
                        {
                            case Direction.Right: { direction = Direction.Up; } break;
                            case Direction.Up: { direction = Direction.Left; steps++; } break;
                            case Direction.Left: { direction = Direction.Down; } break;
                            case Direction.Down: { direction = Direction.Right; steps++; } break;
                        }
                        remaining_steps = steps;
                    }

                    // move one step in current direction
                    switch (direction)
                    {
                        case Direction.Right: x += 1; break;
                        case Direction.Up: y -= 1; break;
                        case Direction.Left: x -= 1; break;
                        case Direction.Down: y += 1; break;
                    }

                    // one step done
                    remaining_steps--;
                }

                String filename = String.Format("AllPrimeSquareSpiral_{0:000}x{1:000}_{2:0000}.bmp", WIDTH, HEIGHT, count);
                SaveDrawing(filename, bitmap);
            }
        }
    }
}
