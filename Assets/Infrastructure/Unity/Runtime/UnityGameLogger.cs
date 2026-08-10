using GeneralCore.AnalyticsDiagnostics;
using UnityEngine;

namespace ZombieWar.Infrastructure.Unity
{
    public sealed class UnityGameLogger : IGameLogger
    {
        public void Info(string message) => Debug.Log(message);
        public void Warning(string message) => Debug.LogWarning(message);
        public void Error(string message) => Debug.LogError(message);
        public void Exception(System.Exception exception) => Debug.LogException(exception);
    }
}
