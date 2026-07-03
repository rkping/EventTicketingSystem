using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketing.Domain.Exceptions;

public class NotEnoughTicketsException : DomainException
{
    public NotEnoughTicketsException(string message) : base(message) { }
}
