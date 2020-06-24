using System;
using Xamarin.Forms;
using XamarinSunmiPayLibSample.Droid;

[assembly: Dependency(typeof(Logger))]
namespace XamarinSunmiPayLibSample.Droid
{
    public class Logger : ILogger
    {
        public void Log(string description)
        {
            System.Diagnostics.Debug.WriteLine(description);
        }
        public void Log(string description, Exception stackTrace)
        {
            System.Diagnostics.Debug.WriteLine(description + "\n" + stackTrace);
        }
    }
}