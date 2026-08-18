using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Exceptions;

public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email)
        : base($"A user with email '{email}' already exists.")
    {
    }
}
