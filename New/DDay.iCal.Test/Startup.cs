using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace DDay.iCal.Test
{
    public class Startup
    {
        [STAThread]
        static public void Main(string[] args)
        {
            NUnit.Gui.AppEntry.Main(new string[]
            {
                Assembly.GetExecutingAssembly().Location, 
                "/run"
            });
        }
    }
}
