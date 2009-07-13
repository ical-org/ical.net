using System.Diagnostics;
using System.Reflection;

namespace System.Data { public class X { }  }

namespace SilverlightAdapter {
    public static class AppDomainStub {
        public static Assembly[] GetAssemblies(this System.AppDomain app) {
            Debug.Assert(false);
            return new Assembly[] {
                // Just return the ones we know...
                Assembly.GetCallingAssembly(), 
                Assembly.GetExecutingAssembly()
            };
        }
    }
}
