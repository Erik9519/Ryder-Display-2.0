using System;
using System.Collections.Generic;
using System.Text;

namespace RyderDisplay.Components.UI.Dynamic
{
    class DynamicElement : Element
    {
        protected List<string> path = null;
        protected object val = null;
        protected float minVal, maxVal;
        protected bool hasMin = false, hasMax = false;

        public void setMetricPath(List<string> path) { this.path = path; }

        protected static object getValInJson(List<string> path, object json)
        {
            if (path != null)
            {
                Dictionary<string, object> data = (Dictionary<string, object>)json;
                for (int i = 0; i < path.Count; i++)
                {
                    if (data.ContainsKey(path[i]))
                    {
                        if (i < path.Count - 1)
                            data = (Dictionary<string, object>)data[path[i]];
                        else
                            return data[path[i]];
                    }
                    else { break; }
                }
            }
            return null;
        }

        protected static object enforceBounds(bool hasMin, bool hasMax, double minVal, double maxVal, object val)
        {
            if (val.GetType() != typeof(string))
            {
                if (val.GetType() == typeof(float) || val.GetType() == typeof(double))
                {
                    // Float or Double value type
                    val = hasMin ? ((double)val < minVal ? (double)minVal : (double)val) : (double)val;
                    val = hasMax ? ((double)val > maxVal ? (double)maxVal : (double)val) : (double)val;

                }
                else
                {
                    // Int value type
                    val = hasMin ? ((long)val < minVal ? (long)minVal : (long)val) : (long)val;
                    val = hasMax ? ((long)val > maxVal ? (long)maxVal : (long)val) : (long)val;
                }
            }
            return val;
        }
    }
}
