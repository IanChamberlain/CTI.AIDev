using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;

namespace CTI.AIDev.Common
{
    public static class GuardExtensions
    {
        public static T NotNull<T>(
            this T? value,
            ILogger logger,
            [CallerArgumentExpression("value")] string? paramName = null)
            where T : class
        {
            if (value is null)
            {
                logger.LogError("Argument '{Param}' was null", paramName);
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        public static string NotNullOrEmpty(
            this string? value,
            ILogger logger,
            [CallerArgumentExpression("value")] string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("Argument '{Param}' was null or empty", paramName);
                throw new ArgumentException("Value cannot be null or empty", paramName);
            }

            return value;
        }

        public static ILogger<TCategory> NotNull<TCategory>(
            this ILogger<TCategory>? logger,
            [CallerArgumentExpression("logger")] string? paramName = null)
        {
            if (logger is null)
            {
                NullLogger.Instance.LogError("Argument '{Param}' was null", paramName);
                throw new ArgumentNullException(paramName);
            }

            return logger;
        }

        public static ILogger NotNull(
            this ILogger? logger,
            [CallerArgumentExpression("logger")] string? paramName = null)
        {
            if (logger is null)
            {
                NullLogger.Instance.LogError("Argument '{Param}' was null", paramName);
                throw new ArgumentNullException(paramName);
            }

            return logger;
        }
    }
}
