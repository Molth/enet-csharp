using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Provides helper methods for validating arguments and
    ///     throwing standard exceptions with consistent messaging.
    /// </summary>
    internal static class ThrowHelpers
    {
        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> indicating that the host has already been started.
        /// </summary>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowHostAlreadyStartedException() => throw new ArgumentException(SR.Argument_HostAlreadyStarted);

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> for null arguments.
        /// </summary>
        /// <param name="paramName">The name of the parameter that is null.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullException(ExceptionArgument paramName) => throw new ArgumentNullException(GetArgumentName(paramName));

        /// <summary>
        ///     Returns the argument name string associated with the specified <see cref="ExceptionArgument" /> value.
        /// </summary>
        /// <param name="argument">The <see cref="ExceptionArgument" /> value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? GetArgumentName(ExceptionArgument argument) => argument switch
        {
            _ => null
        };
    }
}