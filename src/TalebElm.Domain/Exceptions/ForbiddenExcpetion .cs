
namespace TalebElm.Domain.Exceptions;

public class ForbiddenExcpetion : DomainException
{
    public ForbiddenExcpetion(string message) : base(message) {}
}
