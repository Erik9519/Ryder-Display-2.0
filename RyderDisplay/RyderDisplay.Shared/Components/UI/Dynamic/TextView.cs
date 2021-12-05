using RyderDisplay.Components.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RyderDisplay.Components.UI.Dynamic
{
    class TextView : DynamicElement
    {
        private TextBlock label;
        // Settings
        private bool showVal = true;
        private string format = "{0}";

        public TextView(Page page, string id, Element refElement, float[] pos, short alignment) {
            this.id = id;
            this.refElement = refElement;
            this.pos = pos;
            this.alignment = alignment;
 
            // Create Label
            this.label = new TextBlock();
            ((Panel)page.Content).Children.Add(this.label);
        }

        #region Font setters
        public void setFontFamily(string family) { label.FontFamily = new Windows.UI.Xaml.Media.FontFamily(family); }

        public void setFontStyle(int style) { label.FontStyle = (Windows.UI.Text.FontStyle)style; }

        public void setFontWeight(int weight) { label.FontWeight = new Windows.UI.Text.FontWeight((ushort)weight); }

        public void setFontSize(int fontSize) { label.FontSize = fontSize; }

        public void setFontColor(string hex) {
            SolidColorBrush brush = new SolidColorBrush(Element.getColorFromHex(hex));
            label.Foreground = brush;
        }

        public void setStringFormat(string format) { this.format = format; }
        #endregion

        #region Metric setters
        public void setMetricValueVisibility(bool display) { this.showVal = display; }

        public void setMetricValueMin(float min) { this.hasMin = true; this.minVal = min; }

        public void setMetricValueMax(float max) { this.hasMax = true; this.maxVal = max; }
        #endregion

        public override void OnReceive(string cmd, object json)
        {
            // Retrieve value
            this.val = DynamicElement.getValInJson(this.path, json);
            if (this.val != null)
            {
                // Process metric bounds if applicable
                DynamicElement.enforceBounds(this.hasMin, this.hasMax, this.minVal, this.maxVal, this.val);
            }

            // UI update
            _ = this.label.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                // Update label text and retrieve TextBox size
                this.label.Text = String.Format(this.format, this.val);
                this.label.Measure(new Windows.Foundation.Size(9999, 9999));
                this.size[0] = (float)this.label.ActualWidth; this.size[1] = (float)this.label.ActualHeight;

                // Compute positioning
                float[] newPos = this.getAllignedPos();

                // Apply new position
                this.label.SetValue(Canvas.LeftProperty, newPos[0]);
                this.label.SetValue(Canvas.TopProperty, newPos[1]);
            }));
        }
    }
}
