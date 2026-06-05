namespace Payments.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Commands.ProcessPayment;
using Payments.Application.Queries.GetPaymentByOrderId;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(request.OrderId, request.CustomerId, request.Amount, request.Currency);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(new { paymentId = result.Value }) : BadRequest(result.Error);
    }
}

public record ProcessPaymentRequest(Guid OrderId, Guid CustomerId, decimal Amount, string Currency);
