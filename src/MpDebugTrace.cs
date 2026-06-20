using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpDebugTrace
    {
        private const string LogPath = @"C:\Users\at747\source\repos\MultiplayerСlientSideOptimization\debug-ec8eb1.log";

        internal static void Log(string hypothesisId, string location, string message, string dataJson)
        {
            // #region agent log
            try
            {
                var sb = new StringBuilder(384);
                sb.Append("{\"sessionId\":\"ec8eb1\",\"hypothesisId\":\"")
                    .Append(hypothesisId)
                    .Append("\",\"location\":\"")
                    .Append(Escape(location))
                    .Append("\",\"message\":\"")
                    .Append(Escape(message))
                    .Append("\",\"timestamp\":")
                    .Append(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    .Append(",\"data\":")
                    .Append(string.IsNullOrEmpty(dataJson) ? "{}" : dataJson)
                    .Append("}\n");
                File.AppendAllText(LogPath, sb.ToString());
            }
            catch
            {
            }
            // #endregion
        }

        internal static string F(float v) =>
            v.ToString("F3", CultureInfo.InvariantCulture);

        internal static string I(int v) =>
            v.ToString(CultureInfo.InvariantCulture);

        internal static string L(long v) =>
            v.ToString(CultureInfo.InvariantCulture);

        internal static string B(bool v) =>
            v ? "true" : "false";

        internal static string EscapePath(string s) => Escape(s ?? string.Empty);

        private static string Escape(string s) =>
            string.IsNullOrEmpty(s) ? string.Empty : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
