using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace MyProject.Views.Controls
{
    public class AdvancedChartView : SKCanvasView
    {
        public string Source { get { return (string)GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }
        public static readonly BindableProperty SourceProperty = BindableProperty.Create("Source", typeof(string), typeof(AdvancedChartView), "");

        List<int> ChartSource = new List<int>();
        List<int> DataSource = new List<int>();
        int index { get; set; } = 0;
        int viewCount { get; set; } = 60;
        Point LastPt = new Point(0, 0);

        public AdvancedChartView()
        {
            BackgroundColor = Color.FromHex("#6628292D");
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            PaintSurface += OnPaintSurface;

            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += Pinch_PinchUpdated;
            var pan = new PanGestureRecognizer();
            pan.PanUpdated += Pan_PanUpdated;
            GestureRecognizers.Add(pinch);
            GestureRecognizers.Add(pan);

            #region Generate ChartData
            Random srand = new Random();
            for (int i = 0; i <= 500; i++)
            {
                DataSource.Add(srand.Next(50, 200));
            } 
            #endregion
            ChartSource = DataSource.Skip(index).Take(viewCount).ToList();
        }

        private void Pan_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            //Swipe View
            try
            {
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        LastPt = new Point(e.TotalX, e.TotalY);
                        break;
                    case GestureStatus.Running:
                        if (LastPt.X == e.TotalX)
                            return;
                        var moveVolume = (int)Math.Abs((LastPt.X - e.TotalX) / 5);
                        if (LastPt.X < e.TotalX)
                        {
                            index+= moveVolume;
                        }
                        else
                        {
                            index-= moveVolume;
                            index = Math.Max(index, 0);
                        }
                        LastPt = new Point(e.TotalX, e.TotalY);
                        ChartSource = DataSource.Skip(index).Take(viewCount).ToList();
                        this.InvalidateSurface();
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Pinch_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            //Zoom View
            try
            {
                if (e.Scale < 1)
                {
                    viewCount= (int)(viewCount*1.05);
                    viewCount = Math.Min(120, viewCount);
                }
                if (e.Scale > 1)
                {
                    viewCount= (int)(viewCount / 1.05);
                    viewCount = Math.Max(20, viewCount);
                }
                ChartSource = DataSource.Skip(index).Take(viewCount).ToList();
                this.InvalidateSurface();
            }
            catch (Exception ex)
            {

            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            Debug.WriteLine("OnPaintSurface");
            if (!ChartSource.Any())
                return;

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(); //Clear View

            Debug.WriteLine($"CanvasSize [Width]{CanvasSize.Width}  [Height]{CanvasSize.Height}");

            #region DrawView
            int LineCount = viewCount;
            float spacing = CanvasSize.Width / 200;
            float strokeWidth = (CanvasSize.Width - LineCount * spacing) / LineCount;

            var chartPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Orange,
                StrokeWidth = strokeWidth,
            };
            var fontPaint = new SKPaint
            {
                TextSize = 20f,
                Color = Color.Black.ToSKColor(),
                TextAlign = SKTextAlign.Center,
                IsStroke = false
            };

            var posX = CanvasSize.Width - (strokeWidth / 2) - spacing;
            var avg = ChartSource.Average();
            foreach (var data in ChartSource)
            {
                chartPaint.Color = data > avg ? Color.Green.ToSKColor() : data < avg ? Color.Red.ToSKColor() : Color.White.ToSKColor();
                canvas.DrawLine(posX, CanvasSize.Height - 20, posX, CanvasSize.Height - (float)(CanvasSize.Height / (ChartSource.Max() * 1.15) * data), chartPaint);
                canvas.DrawText(data.ToString(), posX, CanvasSize.Height - (float)(CanvasSize.Height / (ChartSource.Max() * 1.15) * data), fontPaint);

                posX = posX - strokeWidth - spacing;
            }
            canvas.DrawLine(0, CanvasSize.Height - (float)(CanvasSize.Height / (DataSource.Max() * 1.15) * avg), CanvasSize.Width, CanvasSize.Height - (float)(CanvasSize.Height / (DataSource.Max() * 1.15) * avg), new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 2,
            });
            #endregion
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == SourceProperty.PropertyName)
            {
                Debug.WriteLine("OnPropertyChanged SourceProperty");
                this.InvalidateSurface();
            }
        }
    }
}
