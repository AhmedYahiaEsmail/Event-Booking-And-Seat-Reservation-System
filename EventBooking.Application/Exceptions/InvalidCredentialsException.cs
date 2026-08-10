using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}
