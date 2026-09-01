using MediatR;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Verification.Commands;
using callbet.Application.Verification.Queries;

namespace callbet.Api.Controllers
{
    [ApiController]
    [Route("api/verification")]
    public class VerificationController(IMediator mediator) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromBody] VerificationRecordDto dto)
        {
            var id = await mediator.Send(new UploadDocumentCommand(dto));
            return Ok(new { Message = "Verification record created", Id = id });
        }

        [HttpGet]
        public async Task<IActionResult> GetVerificationRecords([FromQuery] PagedRequest request)
        {
            var result = await mediator.Send(new GetVerificationRecordsQuery(request));
            return Ok(result);
        }

        [HttpPut("approve/{recordId}")]
        public async Task<IActionResult> ApproveVerification(Guid recordId)
        {
            var id = await mediator.Send(new ApproveVerificationCommand(recordId));
            return Ok(new { Message = "Verification approved", Id = id });
        }

        [HttpDelete("delete/{recordId}")]
        public async Task<IActionResult> DeleteVerification(Guid recordId)
        {
            var id = await mediator.Send(new DeleteVerificationCommand(recordId));
            return Ok(new { Message = "Verification record deleted", Id = id });
        }
    }
}
