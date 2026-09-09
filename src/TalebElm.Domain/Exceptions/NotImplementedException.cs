using System;

namespace TalebElm.Domain.Exceptions;

public class NotImplementedException : DomainException
{
    public NotImplementedException(string message) : base(message)
    {}
}
