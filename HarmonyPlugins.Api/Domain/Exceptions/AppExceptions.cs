namespace HarmonyPlugins.Api.Domain.Exceptions;

// Convertidas em respostas HTTP pelo AppExceptionHandler (404, 409 e 400).
public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class BusinessRuleException(string message) : Exception(message);
