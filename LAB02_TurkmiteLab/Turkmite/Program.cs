﻿using System;
using OpenCvSharp;

namespace TurkMite
{
    class Program
    {
        static void Main()
        {
            Mat img = new Mat(200, 200, MatType.CV_8UC3, new Scalar(0, 0, 0));
            var turkmite = new ThreeColorTurkmite(img);
            for (int i = 0; i < turkmite.PreferredIterationCount; i++)
            {
                turkmite.Step();
            }
            Cv2.ImShow("TurkMite", turkmite.Image);
            Cv2.WaitKey();
        }

        class OriginalTurkmite : TurkmiteBase
        {
            public override int PreferredIterationCount => 13000;
            readonly private Vec3b black = new Vec3b(0, 0, 0);
            readonly private Vec3b white = new Vec3b(255, 255, 255);

            public OriginalTurkmite(Mat image) : base(image)
            {
            }

            protected override (Vec3b newColor, int deltaDirection) GetNextColorAndUpdateDirection(Vec3b currentColor)
            {
                return (currentColor == black) ? (white, 1) : (black, -1);
            }
        }

        class ThreeColorTurkmite : TurkmiteBase
        {
            public override int PreferredIterationCount => 13000;
            readonly private Vec3b black = new Vec3b(0, 0, 0);
            readonly private Vec3b white = new Vec3b(255, 255, 255);
            readonly private Vec3b red = new Vec3b(0, 0, 255);

            public ThreeColorTurkmite(Mat image) : base(image)
            {
            }

            protected override (Vec3b newColor, int deltaDirection) GetNextColorAndUpdateDirection(Vec3b currentColor)
            {
                if (currentColor == black)
                    return (white, 1);
                else if (currentColor == white)
                    return (red, -1);
                else
                    return (black, -1);
            }
        }

        abstract class TurkmiteBase
        {
            public Mat Image { get; }
            public abstract int PreferredIterationCount { get; }
            private Mat.Indexer<Vec3b> indexer;
            private int x;
            private int y;
            protected int direction;  // 0 up, 1 right, 2 down, 3 left
            public TurkmiteBase(Mat image)
            {
                Image = image;
                x = image.Cols / 2;
                y = image.Rows / 2;
                direction = 0;
                indexer = image.GetGenericIndexer<Vec3b>();
            }

            readonly private (int x, int y)[] delta = new (int x, int y)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };

            public void Step()
            {
                int deltaDirection;
                (indexer[y, x], deltaDirection) = GetNextColorAndUpdateDirection(indexer[y, x]);
                PerformMove(deltaDirection);
            }

            private void PerformMove(int deltaDirection)
            {
                direction += deltaDirection;
                direction = (direction + 4) % 4;
                x += delta[direction].x;
                y += delta[direction].y;
                x = Math.Max(0, Math.Min(Image.Cols, x));
                y = Math.Max(0, Math.Min(Image.Rows, y));
            }

            protected abstract (Vec3b newColor, int deltaDirection) GetNextColorAndUpdateDirection(Vec3b currentColor);
        }
    }
}
