using System;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.WebApi.Controllers
{
    public class ContainerSpoilageController : ApiController
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ContainerSpoilage"].ConnectionString;
        
        [HttpPost, Route("trips"), ResponseType(typeof(string))]
        public HttpResponseMessage CreateTrip([FromBody]object tripCreationDetailsString)
        {
            try
            {
                var containerSpoilage = new ContainerSpoilage(new DalFacade(connectionString));
                if (!TryGetTripDetails(tripCreationDetailsString, out var tripCreationDetails, out var invalidDetailsResponse))
                {
                    return invalidDetailsResponse;
                }
                var validation = containerSpoilage.ValidateTripCreationDetails(tripCreationDetails);
                if (!validation.isValid)
                {
                    return Responses.InvalidTripDetailsResponse(validation.errorMessages);
                }
                var tripId = containerSpoilage.CreateTrip(tripCreationDetails);
                return Responses.SuccessfulTripCreationResponse(tripId);
            }
            catch (Exception e)
            {
                return Responses.InternalErrorResponse(e);
            }
        }

        [HttpPost, Route("trips/containers"), ResponseType(typeof(string))]
        public HttpResponseMessage CreateContainer(string tripIdString, [FromBody]object containerCreationDetailsString)
        {
            try
            {
                if (!long.TryParse(tripIdString, out var tripId))
                {
                    return Responses.TripNotFoundResponse();
                }
                var containerSpoilage = new ContainerSpoilage(new DalFacade(connectionString));
                if (!TryGetContainerDetails(containerCreationDetailsString, out var containerCreationDetails, out var invalidDetailsResponse))
                {
                    return invalidDetailsResponse;
                }
                var validation = containerSpoilage.ValidateContainerCreationDetails(containerCreationDetails);
                
                if (!validation.isValid)
                {
                    return Responses.InvalidContainerDetailsResponse(validation.errorMessages);
                }
                if (!containerSpoilage.TripExists(tripId))
                {
                    return Responses.TripNotFoundResponse();
                }

                containerSpoilage.CreateContainer(tripId, containerCreationDetails);
                return Responses.SuccessfulContainerCreationResponse();
            }
            catch (Exception e)
            {
                return Responses.InternalErrorResponse(e);
            }
        }

        [HttpGet, Route("trips/{tripId}"), ResponseType(typeof(string))]
        public HttpResponseMessage GetTrip(string tripId)
        {
            try
            {
                if (!long.TryParse(tripId, out var tripIdNum))
                {
                    return Responses.TripNotFoundResponse();
                }
                var containerSpoilage = new ContainerSpoilage(new DalFacade(connectionString));
                if (!containerSpoilage.TripExists(tripIdNum))
                {
                    return Responses.TripNotFoundResponse();
                }
                var trip = containerSpoilage.GetTrip(tripIdNum);
                return Responses.SuccessfullyRetrievedTripResponse(trip);
            }
            catch (Exception e)
            {
                return Responses.InternalErrorResponse(e);
            }
        }

        private static bool TryGetTripDetails(object tripCreationDetailsString, out TripCreationDetails tripCreationDetails, out HttpResponseMessage httpResponseMessage)
        {
            tripCreationDetails = null;
            httpResponseMessage = null;
            try
            {
                tripCreationDetails = JsonConvert.DeserializeObject<TripCreationDetails>(tripCreationDetailsString.ToString());
            }
            catch (Exception e)
            {
                httpResponseMessage = Responses.InvalidTripDetailsResponse(e.ToString());
                return false;
            }

            return true;
        }

        private static bool TryGetContainerDetails(object containerCreationDetailsString, out ContainerCreationDetails containerCreationDetails, out HttpResponseMessage httpResponseMessage)
        {
            containerCreationDetails = null;
            httpResponseMessage = null;
            try
            {
                containerCreationDetails = JsonConvert.DeserializeObject<ContainerCreationDetails>(containerCreationDetailsString.ToString());
            }
            catch (Exception e)
            {
                httpResponseMessage = Responses.InvalidContainerDetailsResponse(e.ToString());
                return false;
            }

            return true;
        }
    }
}
