namespace callbet.Domain.Entities
{
    public enum UserRole { Customer, Professional, Admin }
    public enum UserStatus { PendingVerification, Active, Suspended }
    public enum PricingType { Fixed, Hourly }
    public enum JobStatus { Draft, Assigned, InProgress, CompletedPendingApproval, Closed, Cancelled }
    public enum PaymentStatus { Pending, HeldInEscrow, ReleasedToWorker }
}