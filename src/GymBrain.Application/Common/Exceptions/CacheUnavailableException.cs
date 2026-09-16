namespace GymBrain.Application.Common.Exceptions;

public sealed class CacheUnavailableException(Exception? innerException = null)
    : Exception("Usage limits are temporarily unavailable. Please try again later.", innerException);
