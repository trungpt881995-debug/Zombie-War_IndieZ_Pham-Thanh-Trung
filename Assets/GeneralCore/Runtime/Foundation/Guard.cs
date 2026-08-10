using System;

namespace GeneralCore.Foundation
{
    public static class Guard
    {
        public static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null) throw new ArgumentNullException(parameterName);
            return value;
        }

        public static string NotNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
            return value;
        }
    }
}
