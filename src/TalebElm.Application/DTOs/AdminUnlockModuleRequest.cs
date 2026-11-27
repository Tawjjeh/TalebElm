namespace TalebElm.Application.DTOs;

/// <summary>
/// Request for an Admin to manually unlock a Module for a learner.
/// </summary>
/// <param name="LearnerId">The learner to unlock the Module for (must be a Student).</param>
/// <param name="ModuleId">The Module to unlock.</param>
public record AdminUnlockModuleRequest(Guid LearnerId, Guid ModuleId);
