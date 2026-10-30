
namespace Tombaro.DrawEngine.Exceptions
{
    /// <summary>
    /// Exception thrown when a <see cref="DrawRequest"/> fails validation before the draw is executed.
    /// </summary>
    public sealed class DrawValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DrawValidationException"/> with the specified error message.
        /// </summary>
        /// <param name="message">A message describing the validation failure.</param>
        public DrawValidationException(string message) : base(message)
        {
        }
    }
}
