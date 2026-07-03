using MediatR;
using Microsoft.AspNetCore.Mvc;
using EventTicketing.Application.Events;
using EventTicketing.Application.Events.commands.CreateEvent;
using EventTicketing.Application.Events.Queries;
using EventTicketing.Application.Events.Queries.GetEventById;
using EventTicketing.Application.Events.Commands.UpdateEvent;
using TicketingSystem.Application.Events.Commands.DeleteEvent;
using EventTicketing.Application.Common;

namespace EventTicketing.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly ISender _sender;

        public EventsController(ISender sender) => _sender = sender;

        [HttpPost]
        public async Task<ActionResult<EventDetailsResponse>> Create(CreateEventRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateEventCommand(
                request.Name,
                request.Description,
                request.Venue,
                request.EventDate,
                request.EventTime,
                request.TotalCapacity,
                request.PricingTiers);

            var result = await _sender.Send(command, cancellationToken);
            //return this.ToCreatedActionResult(result, nameof(GetById), new { eventId = result.Value?.Id });
            return CreatedAtAction(nameof(GetById), new { eventId = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EventSummaryResponse>>> List(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(new ListEventsQuery(pageNumber, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{eventId:guid}")]
        public async Task<ActionResult<EventDetailsResponse>> GetById(Guid eventId, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetEventByIdQuery(eventId), cancellationToken);
            return Ok(result);
        }

        [HttpPut("{eventId:guid}")]
        public async Task<ActionResult<EventDetailsResponse>> Update(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateEventCommand(
                eventId,
                request.Name,
                request.Description,
                request.Venue,
                request.EventDate,
                request.EventTime,
                request.TotalCapacity);

            var result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{eventId:guid}")]
        public async Task<IActionResult> Delete(Guid eventId, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeleteEventCommand(eventId), cancellationToken);
            return Ok();
        }
    }
}
