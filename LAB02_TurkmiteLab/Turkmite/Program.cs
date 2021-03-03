using System;
using OpenCvSharp;

namespace TurkMite
{
    class Program
    {
        static void Main()
        {
            Mat img = new Mat(200, 200, MatType.CV_8UC3, new Scalar(0, 0, 0));
            var turkmite = new StateTurkmite(img);
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

        class StateTurkmite : TurkmiteBase
        {
            public StateTurkmite(Mat image) : base(image)
            {
                stored_StateA = new StateA(this);
                stored_StateB = new StateB(this);
                stored_StateC = new StateC(this);

                CurrentState = stored_StateA;
            }

            readonly private Vec3b black = new Vec3b(0, 0, 0);
            readonly private Vec3b white = new Vec3b(255, 255, 255);
            readonly private Vec3b red = new Vec3b(0, 0, 255);

            private StateBase currentState;
            private StateBase CurrentState
            {
                get
                {
                    return currentState;
                }
                set
                {
                    currentState = value;
                    currentState.Enter();
                }
            }

            private readonly StateA stored_StateA;
            private readonly StateB stored_StateB;
            private readonly StateC stored_StateC;

            public override int PreferredIterationCount => 20000;

            protected override (Vec3b newColor, int deltaDirection) GetNextColorAndUpdateDirection(Vec3b currentColor)
            {
                if (currentColor == black)
                    return currentState.Black();
                else if (currentColor == white)
                    return currentState.White();
                else
                    return currentState.Red();
            }


            abstract class StateBase
            {
                public StateBase(StateTurkmite turkmite)
                {
                    Turkmite = turkmite;
                }

                readonly protected Vec3b black = new Vec3b(0, 0, 0);
                readonly protected Vec3b white = new Vec3b(255, 255, 255);
                readonly protected Vec3b red = new Vec3b(0, 0, 255);

                public StateTurkmite Turkmite { get; }

                public abstract void Enter();

                public abstract (Vec3b newColor, int deltaDirection) Black();
                public abstract (Vec3b newColor, int deltaDirection) White();
                public abstract (Vec3b newColor, int deltaDirection) Red();
            }


            class StateA : StateBase
            {
                public StateA(StateTurkmite turkmite) : base(turkmite) { }
                public override void Enter()
                {
                    blackCounter = 0;
                }
                public override (Vec3b newColor, int deltaDirection) Black()
                {
                    blackCounter++;
                    if (blackCounter == 3)
                    {
                        Turkmite.CurrentState = Turkmite.stored_StateB;
                        return (white, 2);
                    }
                    else
                    {
                        return (black, 0);
                    }
                }

                public override (Vec3b newColor, int deltaDirection) Red()
                {
                    return (red, 0);
                }

                public override (Vec3b newColor, int deltaDirection) White()
                {
                    return (white, 0);
                }

                int blackCounter = 0;
            }

            class StateB : StateBase
            {
                public StateB(StateTurkmite turkmite) : base(turkmite) { }

                public override void Enter()
                {
                    return;
                }

                public override (Vec3b newColor, int deltaDirection) Black()
                {
                    return (white, 1);
                }

                public override (Vec3b newColor, int deltaDirection) White()
                {
                    return (red, -1);
                }

                public override (Vec3b newColor, int deltaDirection) Red()
                {
                    Turkmite.CurrentState = Turkmite.stored_StateC;
                    return (black, 0);
                }


            }

            class StateC : StateBase
            {
                public StateC(StateTurkmite turkmite) : base(turkmite) { }

                public override void Enter()
                {
                    counter = 0;
                }

                public override (Vec3b newColor, int deltaDirection) Black()
                {
                    counter++;
                    if (counter == 1)
                    {
                        return (red, 0);
                    }
                    else if (counter == 5)
                    {
                        Turkmite.CurrentState = Turkmite.stored_StateA;
                    }

                    return (white, 1);
                }

                public override (Vec3b newColor, int deltaDirection) White()
                {
                    counter++;
                    if (counter == 1)
                    {
                        return (red, 0);
                    }
                    else if (counter == 5)
                    {
                        Turkmite.CurrentState = Turkmite.stored_StateA;
                    }

                    return (red, -1);
                }

                public override (Vec3b newColor, int deltaDirection) Red()
                {
                    counter++;
                    if (counter == 1)
                    {
                        return (red, 0);
                    }
                    else if (counter == 5)
                    {
                        Turkmite.CurrentState = Turkmite.stored_StateB;
                    }

                    return (black, -1);
                }

                int counter = 0;
            }
        }
    }
}
