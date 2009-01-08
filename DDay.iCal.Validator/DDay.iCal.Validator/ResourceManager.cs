using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace DDay.iCal.Validator
{
    static internal class ResourceManager
    {
        static private System.Resources.ResourceManager _ResourceManager;

        static ResourceManager()
        {            
        }

        static private bool EnsureResourceManager()
        {
            if (_ResourceManager == null)
                _ResourceManager = System.Resources.ResourceManager.CreateFileBasedResourceManager("Messages", "Resources", null);

            return _ResourceManager != null;
        }

        static internal string GetString(string key)
        {
            if (EnsureResourceManager())
                return _ResourceManager.GetString(key, CultureInfo.CurrentCulture);
            return null;
        }
    }
}
