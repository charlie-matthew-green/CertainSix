using System;
using System.Collections.Generic;
using System.Linq;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.WebApi.Controllers
{
    public class ContainerSpoilage 
    {
        private readonly IDalFacade dalFacade;

        public ContainerSpoilage(IDalFacade dalFacade)
        {
            this.dalFacade = dalFacade;
        }

        public (bool isValid, IEnumerable<string> errorMessages) ValidateTripCreationDetails(TripCreationDetails tripCreationDetails)
        {
            var responses = new List<string>();
            var isValid = true;

            if (tripCreationDetails.SpoilDuration < 0)
            {
                responses.Add("Negative spoil duration is invalid");
                isValid = false;
            }

            if (string.IsNullOrEmpty(tripCreationDetails.Name))
            {
                responses.Add("Trip name must be set");
                isValid = false;
            }

            return (isValid, responses);
        }

        public (bool isValid, IEnumerable<string> errorMessages) ValidateContainerCreationDetails(ContainerCreationDetails containerCreationDetails)
        {
            var responses = new List<string>();
            var isValid = true;

            if (string.IsNullOrEmpty(containerCreationDetails.Id))
            {
                responses.Add("Container id must be set");
                isValid = false;
            }

            if (containerCreationDetails.ProductCount < 0)
            {
                responses.Add("Negative product count is invalid");
                isValid = false;
            }

            return (isValid, responses);
        }

        public long CreateTrip(TripCreationDetails tripCreationDetails)
        {
            return dalFacade.AddTrip(tripCreationDetails);
        }

        public bool TripExists(long tripId)
        {
            return dalFacade.TryGetTripDetails(tripId, out _);
        }

        public void CreateContainer(long tripId, ContainerCreationDetails containerCreationDetails)
        {
            dalFacade.AddContainer(tripId, containerCreationDetails);
        }

        public Trip GetTrip(long tripId)
        {
            var tripFound = dalFacade.TryGetTripDetails(tripId, out var trip);
            var containers = dalFacade.GetContainers(tripId).ToArray();
            if (!containers.Any())
            {
                return trip;
            }
            trip.ContainerCount = containers.Length;
            trip.MaxTemperature = containers.Max(x => x.Measurements.Max(y => y.Value));
            var measurements = new List<TemperatureRecord>();
            foreach (var measurementList in containers.Select(x => x.Measurements))
            {
                measurements.AddRange(measurementList);
            }
            trip.MeanTemperature = measurements.Average(x => x.Value);
            var spoilageStats = GetSpoilageStatistics(containers, measurements, trip);
            trip.SpoiledContainerCount = spoilageStats.spoiledContainers;
            trip.SpoiledProductCount = spoilageStats.spoiledProducts;
            return trip;
        }

        private (int spoiledContainers, int spoiledProducts) GetSpoilageStatistics(ContainerCreationDetails[] containers, List<TemperatureRecord> measurements,
            TripWithSpoilDetails trip)
        {
            var spoiledContainers = 0;
            var spoiledProducts = 0;
            foreach (var container in containers)
            {
                var orderedMeasurements = container.Measurements.OrderBy(x => x.Time).ToArray();
                var firstTime = orderedMeasurements.First(x => x.Time != DateTime.MinValue).Time;
                var lastTime = orderedMeasurements.Last().Time;

                // any missing times are assumed worst case where temperature is above safe level
                var numMissingMeasurements = (int) (lastTime - firstTime).TotalMinutes + 1 - orderedMeasurements.Count();
                var measuredAboveLimit = orderedMeasurements.Count(x => x.Value >= trip.SpoilTemperature);
                var minutesAboveLimit = numMissingMeasurements + measuredAboveLimit;
                if (minutesAboveLimit >= trip.SpoilDuration)
                {
                    spoiledContainers++;
                    spoiledProducts += container.ProductCount;
                }
            }

            return (spoiledContainers, spoiledProducts);
        }
    }
}
