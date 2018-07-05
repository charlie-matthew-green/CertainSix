using System.Web.Http;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.WebApi.Controllers
{
    public class ContainerSpoilageController : ApiController
    {
        private readonly IContainerSpoilage containerSpoilage;

        public ContainerSpoilageController(IContainerSpoilage containerSpoilage)
        {
            this.containerSpoilage = containerSpoilage;
        }

        [HttpPost, Route("trips")]
        public IHttpActionResult CreateTrip(TripCreationDetails tripCreationDetails)
        {
            var validation = containerSpoilage.ValidateTripCreationDetails(tripCreationDetails);
            if (!validation.isValid)
            {
                return BadRequest(validation.errorMessage);
            }
            var tripId = containerSpoilage.CreateTrip(tripCreationDetails);
            return Created<Trip>($"/trips/{tripId}", null);
        }

        [HttpPost, Route("trips/containers")]
        public IHttpActionResult CreateContainer(string tripId, ContainerCreationDetails containerCreationDetails)
        {
            if (!containerSpoilage.TripExists(tripId))
            {
                return NotFound();
            }
            var validation = containerSpoilage.ValidateContainerCreationDetails(containerCreationDetails);
            if (!validation.isValid)
            {
                return BadRequest(validation.errorMessage);
            }
            containerSpoilage.CreateContainer(tripId, containerCreationDetails);
            return Created<ContainerCreationDetails>("", null);
        }

        [HttpGet, Route("trips/{tripId}")]
        public IHttpActionResult GetTrip(string tripId)
        {
            if (!containerSpoilage.TripExists(tripId))
            {
                return NotFound();
            }
            var trip = containerSpoilage.GetTrip(tripId);
            return new OkResultWithWeakETag<Trip>(trip, trip.GetETag(), this);
        }
    }
}
