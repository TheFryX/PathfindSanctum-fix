using ExileCore2;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PathfindSanctum {
    public abstract class WeightSettings {

        private readonly Dictionary<string, PropertyInfo> _propertyCache;

        protected WeightSettings() {
            _propertyCache = GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(MenuAttribute), true).Any())
                .ToDictionary(p => ((MenuAttribute)p.GetCustomAttributes(typeof(MenuAttribute), true).First()).MenuName);
        }

        internal bool GetWeight(string key, out int value) {
            if (_propertyCache.TryGetValue(key, out var property)) {
                var rangeNode = property.GetValue(this, null) as RangeNode<int>;
                if (rangeNode != null) {
                    value = rangeNode.Value;
                    return true;
                }
            }
            Logger.Log.Error($"Error getting weight value! Weight with key '{key}' could not be found in settings type '{GetType().Name}'");
            value = 0;
            return false;
        }

        internal void SetWeight(string key, float value) {
            if (_propertyCache.TryGetValue(key, out var property)) {
                var rangeNode = property.GetValue(this, null) as RangeNode<int>;
                if (rangeNode != null) {
                    rangeNode.Value = (int)value;
                    return;
                }
            }
            Logger.Log.Error($"Error while setting weight value! Weight with key '{key}' could not be found in settings type '{GetType().Name}'");
        }
    }
}
