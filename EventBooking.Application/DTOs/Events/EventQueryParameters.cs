using EventBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.DTOs.Events;

public record EventQueryParameters(
    string? SearchTerm = null,
    EventStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int PageNumber = 1,
    int PageSize = 10);
