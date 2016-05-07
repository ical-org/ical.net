using System;
using System.Reflection;

namespace ical.NET.UnitTests
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            NUnit.Gui.AppEntry.Main(new string[]
            {
                Assembly.GetExecutingAssembly().Location
            });
        }
    }
}
