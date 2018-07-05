using System;
using System.Collections.Generic;
using System.Linq;
using ShippingContainerSpoilage.WebApi.Controllers;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.WebApi
{
    public interface IContainerSpoilage
    {
        (bool isValid, string errorMessage) ValidateTripCreationDetails(TripCreationDetails tripCreationDetails);
        (bool isValid, string errorMessage) ValidateContainerCreationDetails(ContainerCreationDetails containerCreationDetails);
        long CreateTrip(TripCreationDetails tripCreationDetails);
        bool TripExists(string tripIdString);
        void CreateContainer(string tripId, ContainerCreationDetails containerCreationDetails);
        Trip GetTrip(string tripId);
    }

    public class ContainerSpoilage : IContainerSpoilage
    {
        private readonly IDalFacade dalFacade;

        public ContainerSpoilage(IDalFacade dalFacade)
        {
            this.dalFacade = dalFacade;
        }

        public (bool isValid, string errorMessage) ValidateTripCreationDetails(TripCreationDetails tripCreationDetails)
        {
            var errorMessage = "";
            var isValid = true;

            if (tripCreationDetails == null)
            {
                return (false, "Invalid trip creation details. ");
            }

            if (tripCreationDetails.SpoilDuration < 0)
            {
                errorMessage += "Negative spoil duration is invalid. ";
                isValid = false;
            }

            if (string.IsNullOrEmpty(tripCreationDetails.Name))
            {
                errorMessage += "Trip name must be set. ";
                isValid = false;
            }

            return (isValid, errorMessage);
        }

        public (bool isValid, string errorMessage) ValidateContainerCreationDetails(ContainerCreationDetails containerCreationDetails)
        {
            var errorMessage = "";
            var isValid = true;

            if (containerCreationDetails == null)
            {
                return (false, "Invalid container creation details");
            }

            if (string.IsNullOrEmpty(containerCreationDetails.Id))
            {
                errorMessage += "Container id must be set. ";
                isValid = false;
            }

            if (containerCreationDetails.ProductCount < 0)
            {
                errorMessage += "Negative product count is invalid. ";
                isValid = false;
            }

            return (isValid, errorMessage);
        }

        public long CreateTrip(TripCreationDetails tripCreationDetails)
        {
            return dalFacade.AddTrip(tripCreationDetails);
        }

        public bool TripExists(string tripIdString)
        {
            if (!long.TryParse(tripIdString, out var tripId))
            {
                return false;
            }
            return dalFacade.TryGetTripDetails(tripId, out _);
        }

        public void CreateContainer(string tripIdString, ContainerCreationDetails containerCreationDetails)
        {
            var tripId = long.Parse(tripIdString);
            dalFacade.AddContainer(tripId, containerCreationDetails);
        }

        public Trip GetTrip(string tripIdString)
        {
            var tripId = long.Parse(tripIdString);
            var tripFound = dalFacade.TryGetTripDetails(tripId, out var trip);
            var containers = dalFacade.GetContainers(tripId).ToArray();
            if (!containers.Any())
            {
                return trip;
            }
            trip.ContainerCount = containers.Length;
            trip.MaxTemperature = decimal.Round(containers.Max(x => x.Measurements.Max(y => y.Value)), 2);
            var measurements = new List<TemperatureRecord>();
            foreach (var measurementList in containers.Select(x => x.Measurements))
            {
                measurements.AddRange(measurementList);
            }
            trip.MeanTemperature = decimal.Round(measurements.Average(x => x.Value), 2);
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
