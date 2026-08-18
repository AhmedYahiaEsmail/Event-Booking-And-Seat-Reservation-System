using EventBooking.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
