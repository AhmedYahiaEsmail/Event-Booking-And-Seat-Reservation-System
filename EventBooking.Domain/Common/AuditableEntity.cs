using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Domain.Common;

public abstract class AuditableEntity
{
    public Guid Id { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
}
