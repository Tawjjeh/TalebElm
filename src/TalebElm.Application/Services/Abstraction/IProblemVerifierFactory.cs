using TalebElm.Domain.Enums;

namespace TalebElm.Application.Services.Abstraction;

public interface IProblemVerifierFactory
{
    IProblemVerifier GetFor(ProblemKind kind);
}
