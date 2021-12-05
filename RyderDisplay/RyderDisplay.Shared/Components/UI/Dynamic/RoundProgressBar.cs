using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace RyderDisplay.Components.UI.Dynamic
{
    class RoundProgressBar : DynamicElement
    {
        private Image image;
        // Internal
        private SKBitmap bmpBg;
        private SKBitmap bmpFg;
        private SKBitmap bmpPr;
        private SKBitmap bmp;
        private SKPaint paint = new SKPaint();
        private bool reDraw = true;
        private float range, progress = 0f;
        private float arc_ofst;
        private float[] ofst = new float[] { 0, 0 };
        private float overlapAngle = 1f, eraserOverlap = 1.5f, midAngle, halfSweepAngle;
        // Settings
        private float startAngle = 150f, sweepAngle = 240f;     // Start angle and end angle
        private short fillDir = 0;                              // Fill Direction (-1 = counter-clockwise, 0 = center-out, 1 = clockwise)
        private byte[] caps = new byte[] { 1, 2 };              // Cap (0 = None, 1 = Round, 2 = Square)
        private int bgT = 25, brT = 6, fgT = 23;                // Thickness of background, border, foreground
        private SKColor bgCol = SKColor.Parse("#292929"),       // Colors
                        brCol = SKColor.Parse("#78a6f0"), 
                        fgCol = SKColor.Parse("#4287f5");  

        public RoundProgressBar(Page page, string id, Element refElement, float[] pos, float size, short alignment)
        {
            this.id = id;
            this.refElement = refElement;
            this.pos = pos;
            this.size = new float[] { size, size };
            this.alignment = alignment;
            this.maxVal = 100; this.minVal = 0;
            this.paint.Style = SKPaintStyle.Stroke; this.paint.IsAntialias = true;

            // Create Canvas
            this.bmpBg  = new SKBitmap((int)this.size[0], (int)this.size[1]);
            this.bmpFg  = new SKBitmap((int)this.size[0], (int)this.size[1]);
            this.bmpPr  = new SKBitmap((int)this.size[0], (int)this.size[1]);
            this.bmp    = new SKBitmap((int)this.size[0], (int)this.size[1]);
            this.image  = new Image();
            //// Compute position
            float[] newPos = this.getAllignedPos();
            this.image.SetValue(Canvas.LeftProperty, newPos[0]);
            this.image.SetValue(Canvas.TopProperty, newPos[1]);
            //// Compute parameters
            this.arc_ofst = (bgT + brT) / 2f;
            this.range = this.maxVal - this.minVal;
            this.halfSweepAngle = this.sweepAngle / 2f;
            this.midAngle = this.startAngle + this.halfSweepAngle;
            float radius = size - (bgT + brT) / 2f;
            //// Compute offsets required when using end caps
            for (byte i = 0; i < 2; i++)
            {
                if (this.caps[i] != 0)
                    this.ofst[i] = -((bgT + brT) * 180f) / ((float)Math.PI * radius);
            }

            ((Panel)page.Content).Children.Add(this.image);
        }

        public override void OnReceive(string cmd, object json)
        {
            // Retrieve value
            this.val = DynamicElement.getValInJson(this.path, json);
            if (this.val == null) return;

            // Process metric bounds if applicable
            DynamicElement.enforceBounds(this.hasMin, this.hasMax, this.minVal, this.maxVal, this.val);

            float dofst = this.arc_ofst * 2f;
            SKCanvas canvas;
            // Check if re-draw needed
            if (this.reDraw)
            {
                // Draw arc with 2 half arcs such that separate end caps can be used
                this.reDraw = false;
                // Draw background bitmap
                canvas = new SKCanvas(this.bmpBg); canvas.Clear();
                this.paint.BlendMode = SKBlendMode.SrcOver;
                short dir;
                //// Draw border
                this.paint.Color = this.brCol;
                this.paint.StrokeWidth = this.bgT + this.brT;
                dir = -1;
                for (short i = 0; i < 2; i ++)
                {
                    this.paint.StrokeCap = (SKStrokeCap)this.caps[i];
                    canvas.DrawArc(
                        new SKRect(this.arc_ofst, this.arc_ofst, (int)this.size[0] - dofst, (int)this.size[1] - dofst),
                        this.midAngle - this.overlapAngle * dir, (this.halfSweepAngle + this.ofst[i] + this.overlapAngle * 2) * dir, false, this.paint
                    );
                    dir += 2;
                }
                //// Draw background
                this.paint.Color = this.bgCol;
                this.paint.StrokeWidth = this.bgT;
                dir = -1;
                for (short i = 0; i < 2; i++)
                {
                    this.paint.StrokeCap = (SKStrokeCap)this.caps[i];
                    canvas.DrawArc(
                        new SKRect(this.arc_ofst, this.arc_ofst, (int)this.size[0] - dofst, (int)this.size[1] - dofst),
                        this.midAngle - this.overlapAngle * dir, (this.halfSweepAngle + this.ofst[i] + this.overlapAngle * 2.5f) * dir, false, this.paint
                    );
                    dir += 2;
                }

                // Draw foreground bitmap
                canvas = new SKCanvas(this.bmpFg); canvas.Clear();
                this.paint.Color = this.fgCol;
                this.paint.StrokeWidth = this.fgT;
                dir = -1;
                for (short i = 0; i < 2; i++)
                {
                    this.paint.StrokeCap = (SKStrokeCap)this.caps[i];
                    canvas.DrawArc(
                        new SKRect(this.arc_ofst, this.arc_ofst, (int)this.size[0] - dofst, (int)this.size[1] - dofst),
                        this.midAngle - this.overlapAngle * dir, (this.halfSweepAngle + this.ofst[i] + this.overlapAngle * 2) * dir, false, this.paint
                    );
                    dir += 2;
                }
                //// Default
                this.paint.StrokeCap = SKStrokeCap.Butt;
                this.paint.BlendMode = SKBlendMode.Clear;
                this.paint.StrokeWidth = this.bgT + this.brT;
            }

            // Update Progress Bar
            this.progress = 1f / this.range * ((float)(long)this.val - this.minVal);
            canvas = new SKCanvas(this.bmpPr); canvas.Clear();
            canvas.DrawBitmap(this.bmpFg, 0, 0);
            if (this.fillDir != 0)
            {
                float startAngle = this.fillDir > 0 ? this.sweepAngle * this.progress : this.sweepAngle * (1f - this.progress);
                startAngle += this.startAngle;
                float sweepAngle = this.fillDir > 0 ? this.sweepAngle - this.sweepAngle * this.progress : -(this.sweepAngle - this.sweepAngle * this.progress);
                canvas.DrawArc(
                    new SKRect(this.arc_ofst, this.arc_ofst, (int)this.size[0] - dofst, (int)this.size[1] - dofst),
                    startAngle - this.eraserOverlap * this.fillDir,
                    sweepAngle + this.eraserOverlap * 2f * this.fillDir,
                    false, this.paint
                );
            }
            else
            {
                float sweepAngle = this.halfSweepAngle * this.progress;
                canvas.DrawArc(
                    new SKRect(this.arc_ofst, this.arc_ofst, (int)this.size[0] - dofst, (int)this.size[1] - dofst),
                    this.midAngle + sweepAngle - this.eraserOverlap * 0.5f, this.halfSweepAngle - sweepAngle + this.eraserOverlap * 1.5f, false, this.paint
                );
                canvas.DrawArc(
                    new SKRect(this.arc_ofst, this.arc_ofst, (int)this.size[0] - dofst, (int)this.size[1] - dofst),
                    this.midAngle - sweepAngle + this.eraserOverlap * 0.5f, -(this.halfSweepAngle - sweepAngle + this.eraserOverlap * 1.5f), false, this.paint
                );
            }

            // Convert SKBitmap to Bitmap
            canvas = new SKCanvas(this.bmp);
            canvas.DrawBitmap(this.bmpBg, 0, 0);
            canvas.DrawBitmap(this.bmpPr, 0, 0);
            var bitmap = new Windows.UI.Xaml.Media.Imaging.WriteableBitmap((int)this.size[0], (int)this.size[1]);
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            { stream.Write(this.bmp.Bytes, 0, this.bmp.ByteCount); }

            // Push update to UI
            _ = this.image.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() => { this.image.Source = bitmap; }));
        }
    }
}
