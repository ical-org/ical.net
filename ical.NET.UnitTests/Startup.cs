using System;
using System.Reflection;
using NUnit.Gui;

namespace ical.NET.UnitTests
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppEntry.Main(new[]
            {
                Assembly.GetExecutingAssembly().Location
            });
        }
    }
}
