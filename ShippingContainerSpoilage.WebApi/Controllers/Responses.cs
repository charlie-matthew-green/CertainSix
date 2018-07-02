using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.WebApi.Controllers
{
    public static class Responses
    {
        public static HttpResponseMessage SuccessfulTripCreationResponse(long tripId)
        {
            var response = new HttpResponseMessage();
            response.Headers.Add("Description", "Successful trip creation");
            response.StatusCode = HttpStatusCode.Created;
            response.Headers.Location = new Uri($"/trips/{tripId}", UriKind.Relative);
            return response;
        }

        public static HttpResponseMessage InvalidTripDetailsResponse(string errorMessage)
        {
            return InvalidTripDetailsResponse(new[] {errorMessage});
        }

        public static HttpResponseMessage InvalidTripDetailsResponse(IEnumerable<string> errorMessages)
        {
            var response = new HttpResponseMessage();
            var errorString = errorMessages.Any() ? $" - {String.Join("; ", errorMessages)}" : String.Empty;
            response.Headers.Add("Description", $"Invalid creation details{errorString}");
            response.StatusCode = HttpStatusCode.BadRequest;
            return response;
        }

        public static HttpResponseMessage InvalidContainerDetailsResponse(string errorMessage)
        {
            return InvalidContainerDetailsResponse(new []{errorMessage});
        }

        public static HttpResponseMessage InvalidContainerDetailsResponse(IEnumerable<string> errorMessages)
        {
            var response = new HttpResponseMessage();
            response.Headers.Add("Description", "Invalid creation details");
            response.StatusCode = HttpStatusCode.BadRequest;
            return response;
        }

        public static HttpResponseMessage SuccessfulContainerCreationResponse()
        {
            var response = new HttpResponseMessage();
            response.Headers.Add("Description", "Successful container creation");
            response.StatusCode = HttpStatusCode.Created;
            return response;
        }

        public static HttpResponseMessage TripNotFoundResponse()
        {
            var response = new HttpResponseMessage();
            response.Headers.Add("Description", "Trip not found");
            response.StatusCode = HttpStatusCode.NotFound;
            return response;
        }

        public static HttpResponseMessage SuccessfullyRetrievedTripResponse(Trip trip)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(trip), Encoding.Unicode, "application/json");
            response.Headers.Add("Description", "Successful operation");
            response.StatusCode = HttpStatusCode.Created;
            response.Headers.Add("ETag", trip.GetETag());
            return response;
        }

        public static HttpResponseMessage InternalErrorResponse(Exception e)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(e.ToString());
            response.Headers.Add("Description", "Internal server error");
            response.StatusCode = HttpStatusCode.InternalServerError;
            return response;
        }
    }
}