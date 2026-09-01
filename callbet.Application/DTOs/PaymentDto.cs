using System;
using callbet.Domain.Entities;

namespace callbet.Application.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}