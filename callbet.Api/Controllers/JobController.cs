using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Jobs.Commands;
using callbet.Application.Jobs.Queries;

namespace callbet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/job")]
public class JobController(IMediator mediator) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateJob([FromBody] JobDto dto)
    {
        var id = await mediator.Send(new CreateJobCommand(dto));
        return Ok(new { Message = "Job created", Id = id });
    }

    [HttpPut("assign/{jobId}")]
    public async Task<IActionResult> AssignProfessional(Guid jobId, [FromBody] Guid professionalId)
    {
        var id = await mediator.Send(new AssignProfessionalCommand(jobId, professionalId));
        return Ok(new { Message = "Professional assigned", Id = id });
    }

    [HttpPut("start/{jobId}")]
    public async Task<IActionResult> StartJob(Guid jobId)
    {
        var id = await mediator.Send(new StartJobCommand(jobId));
        return Ok(new { Message = "Job started", Id = id });
    }

    [HttpPut("complete/{jobId}")]
    public async Task<IActionResult> CompleteJob(Guid jobId)
    {
        var id = await mediator.Send(new CompleteJobCommand(jobId));
        return Ok(new { Message = "Job completed, pending approval", Id = id });
    }

    [HttpPut("close/{jobId}")]
    public async Task<IActionResult> CloseJob(Guid jobId)
    {
        var id = await mediator.Send(new CloseJobCommand(jobId));
        return Ok(new { Message = "Job closed", Id = id });
    }

    [HttpGet("professional/{professionalId}")]
    public async Task<IActionResult> GetJobsForProfessional(Guid professionalId)
    {
        var result = await mediator.Send(new GetJobsForProfessionalQuery(professionalId));
        return Ok(result);
    }

    [HttpGet("payments/professional/{professionalId}")]
    public async Task<IActionResult> GetPaymentsForProfessional(Guid professionalId)
    {
        var result = await mediator.Send(new GetPaymentsForProfessionalQuery(professionalId));
        return Ok(result);
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentDto dto)
    {
        var id = await mediator.Send(new CreatePaymentCommand(dto));
        return Ok(new { Message = "Payment created", Id = id });
    }

    [HttpPut("payment/escrow/{jobId}")]
    public async Task<IActionResult> HoldPayment(Guid jobId)
    {
        var id = await mediator.Send(new HoldPaymentCommand(jobId));
        return Ok(new { Message = "Payment held in escrow", Id = id });
    }

    [HttpPut("payment/release/{jobId}")]
    public async Task<IActionResult> ReleasePayment(Guid jobId)
    {
        var id = await mediator.Send(new ReleasePaymentCommand(jobId));
        return Ok(new { Message = "Payment released to worker", Id = id });
    }

    [HttpPost("review")]
    public async Task<IActionResult> CreateReview([FromBody] ReviewDto dto)
    {
        var id = await mediator.Send(new CreateReviewCommand(dto));
        return Ok(new { Message = "Review created", Id = id });
    }

    [HttpPost("review/reply")]
    public async Task<IActionResult> ReplyToReview([FromBody] ReviewReplyDto dto)
    {
        var id = await mediator.Send(new ReplyToReviewCommand(dto));
        return Ok(new { Message = "Reply added", Id = id });
    }
}
