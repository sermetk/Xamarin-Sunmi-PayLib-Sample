using System;
namespace XamarinSunmiPayLibSample
{
    public interface ILogger
    {
        void Log(string description);
        void Log(string description, Exception stackTrace);
    }
}
