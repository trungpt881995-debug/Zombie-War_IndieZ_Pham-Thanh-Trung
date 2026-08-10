using System.Collections.Generic;

namespace GeneralCore.AnalyticsDiagnostics
{
    public interface IGameLogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Exception(System.Exception exception);
    }

    public interface IAnalyticsService
    {
        void Track(string eventName, IReadOnlyDictionary<string, object> parameters = null);
    }
}
