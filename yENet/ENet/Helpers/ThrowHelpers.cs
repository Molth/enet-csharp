using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Provides helper methods for validating arguments and
    ///     throwing standard exceptions with consistent messaging.
    /// </summary>
    internal static unsafe class ThrowHelpers
    {
        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="argument" /> has not been initialized.
        /// </summary>
        /// <param name="argument">The value to validate.</param>
        /// <param name="paramName">The name of the parameter that is null.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNotCreated(bool argument, ExceptionArgument paramName)
        {
            if (!argument)
                ThrowArgumentNullException(paramName);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="argument" /> is null.
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull(void* argument, ExceptionArgument paramName)
        {
            if (argument == null)
                throw new ArgumentNullException(GetArgumentName(paramName), SR.ArgumentNull_MustBeNotNull);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="argument" /> is null.
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument" /> corresponds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(T? argument, ExceptionArgument paramName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(GetArgumentName(paramName), SR.ArgumentNull_MustBeNotNull);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> if the provided delegate is not <see langword="null" />
        ///     and does not reference a static method.
        /// </summary>
        /// <param name="argument">The delegate to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentException">Thrown when the delegate is non-null and its method is not static.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfMethodIsNotStatic(Delegate? argument, ExceptionArgument paramName)
        {
            if (argument != null && !argument.Method.IsStatic)
                throw new ArgumentException(SR.Argument_MustBeStatic, GetArgumentName(paramName));
        }

        /// <summary>
        ///     Throws an <see cref="OverflowException" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowOverflowException() => throw new OverflowException();

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> for null arguments.
        /// </summary>
        /// <param name="paramName">The name of the parameter that is null.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullException(ExceptionArgument paramName) => throw new ArgumentNullException(GetArgumentName(paramName));

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> for null arguments.
        /// </summary>
        /// <param name="paramName">The name of the parameter that is null.</param>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentExceptionException(ExceptionArgument paramName) => throw new ArgumentException(GetArgumentName(paramName));

        /// <summary>
        ///     Throws a <see cref="SocketException" /> with the specified socket error code.
        /// </summary>
        /// <param name="socketError">The socket error code to include in the exception.</param>
        /// <exception cref="SocketException">Always thrown with the provided error code.</exception>
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowSocketException(SocketError socketError) => throw new SocketException((int)socketError);

        /// <summary>
        ///     Returns the argument name string associated with the specified <see cref="ExceptionArgument" /> value.
        /// </summary>
        /// <param name="argument">The <see cref="ExceptionArgument" /> value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? GetArgumentName(ExceptionArgument argument) => argument switch
        {
            ExceptionArgument.address => "address",
            ExceptionArgument.handle => "handle",
            ExceptionArgument.option => "option",
            ExceptionArgument.peerCount => "peerCount",
            _ => null
        };
    }
}