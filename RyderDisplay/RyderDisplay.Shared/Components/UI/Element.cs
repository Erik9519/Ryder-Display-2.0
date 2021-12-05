using RyderDisplay.Components.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace RyderDisplay.Components.UI
{
    public class Element : RyderClient.Callback
    {
        protected string id = "null";
        protected Element refElement = null;
        protected float[] pos = new float[2], size = new float[2];
        protected short alignment;

        public string getId() { return id; }

        public float[] getPosition() { return pos; }

        public float[] getSize() { return size; }

        public virtual void OnReceive(string cmd, object json) { /* Ignore by default */ }

        // Utility Functions
        protected float[] getAllignedPos()
        {
            // Start position
            float[] newPos = new float[2] { 0, 0 };
            // Horizontal alignment
            if (this.alignment == 9 || this.alignment == 6 || this.alignment == 3)
                newPos[0] -= this.size[0];          // Right
            else if (this.alignment == 8 || this.alignment == 5 || this.alignment == 2)
                newPos[0] -= this.size[0] / 2;      // Center
            // Vertical alignment
            if (this.alignment == 1 || this.alignment == 2 || this.alignment == 3)
                newPos[1] -= this.size[1];          // Bottom
            else if (this.alignment == 4 || this.alignment == 5 || this.alignment == 6)
                newPos[1] -= this.size[1] / 2;      // Center

            // Process relative positioning
            for (short i = 0; i < 2; i++)
            {
                if (this.refElement != null)
                    newPos[i] += this.refElement.getSize()[i] / 100f * this.pos[i] + this.refElement.getPosition()[i];
                else
                    newPos[i] += this.pos[i];
            }

            return newPos;
        }

        protected static Windows.UI.Color getColorFromHex(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }
    }
}
