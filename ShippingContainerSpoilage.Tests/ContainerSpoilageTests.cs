using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using ShippingContainerSpoilage.WebApi;
using ShippingContainerSpoilage.WebApi.Controllers;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.Tests
{
	[TestFixture]
    public class ContainerSpoilageTests
    {
        [TestCase(30, true)]
        [TestCase(-30, false)]
        public void TripCreation_NegativeSpoilDurationInvalid(int spoilDuration, bool isValid)
        {
            // Arrange
            var tripCreationDetails = new TripCreationDetails
            {
                Name = "NZ to GB",
                SpoilDuration = spoilDuration,
                SpoilTemperature = 26.7m
            };
            var containerSpoilage = new ContainerSpoilage(null);

            // Act
            var validation = containerSpoilage.ValidateTripCreationDetails(tripCreationDetails);
            Assert.AreEqual(isValid, validation.isValid);
            Assert.AreEqual(isValid, string.IsNullOrEmpty(validation.errorMessage));
        }        
        
        [TestCase("NZ to GB", true)]
        [TestCase(null, false)]
        public void TripCreation_UnsetNameInvalid(string name, bool isValid)
        {
            // Arrange
            var tripCreationDetails = new TripCreationDetails
            {
                Name = name,
                SpoilDuration = 30,
            };
            var containerSpoilage = new ContainerSpoilage(null);

            // Act
            var validation = containerSpoilage.ValidateTripCreationDetails(tripCreationDetails);
            
            // Assert
            Assert.AreEqual(isValid, validation.isValid);
            Assert.AreEqual(isValid, string.IsNullOrEmpty(validation.errorMessage));
        }

        [TestCase(1000, true)]
        [TestCase(-1000, false)]
        public void ContainerCreation_NegativeProductCountInvalid(int productCount, bool isValid)
        {
            // Arrange
            var containerDetails = new ContainerCreationDetails
            {
                Id = Guid.NewGuid().ToString(),
                ProductCount = productCount
            };
            var containerSpoilage = new ContainerSpoilage(null);

            // Act
            var validation = containerSpoilage.ValidateContainerCreationDetails(containerDetails);

            // Assert
            Assert.AreEqual(isValid, validation.isValid);
            Assert.AreEqual(isValid, string.IsNullOrEmpty(validation.errorMessage));
        }

        [TestCase("1234", true)]
        [TestCase(null, false)]
        public void ContainerCreation_UnsetIdInvalid(string id, bool isValid)
        {
            // Arrange
            var containerDetails = new ContainerCreationDetails
            {
                Id = id,
                ProductCount = 1000
            };
            var containerSpoilage = new ContainerSpoilage(null);

            // Act
            var validation = containerSpoilage.ValidateContainerCreationDetails(containerDetails);

            // Assert
            Assert.AreEqual(isValid, validation.isValid);
            Assert.AreEqual(isValid, string.IsNullOrEmpty(validation.errorMessage));
        }

        [Test]
        public void GetsTrip()
        {
            // Arrange
            var tripId = 123456L;
            var dalfacade = MockRepository.GenerateMock<IDalFacade>();
            var tripDetails = new TripWithSpoilDetails
            {
                Id = tripId,
                SpoilDuration = 3,
                SpoilTemperature = 27m
            };
            var container1 = new ContainerCreationDetails
            {
                ProductCount = 1035,
                Measurements = new[]
                {
                    new TemperatureRecord {Time = new DateTime(2018, 6, 26, 12, 0, 0), Value = 24.6m},
                    new TemperatureRecord {Time = new DateTime(2018, 6, 26, 12, 1, 1), Value = 26.2m},
                    new TemperatureRecord {Time = new DateTime(2018, 6, 26, 12, 3, 9), Value = 27.3m},
                }
            };
            var container2 = new ContainerCreationDetails
            {
                ProductCount = 1039,
                Measurements = new[]
                {
                    new TemperatureRecord {Time = new DateTime(2018, 6, 26, 12, 30, 2), Value = 29.6m},
                    new TemperatureRecord {Time = new DateTime(2018, 6, 26, 12, 35, 7), Value = 26.2m},
                    new TemperatureRecord {Time = new DateTime(2018, 6, 26, 12, 36, 5), Value = 22.3m},
                }
            };
            dalfacade.Stub(x => x.TryGetTripDetails(tripId, out _))
                .OutRef(tripDetails)
                .Return(true);
            dalfacade.Stub(x => x.GetContainers(tripId)).Return(new[] {container1, container2});
            var containerSpoilage = new ContainerSpoilage(dalfacade);

            // Act
            var trip = containerSpoilage.GetTrip(tripId.ToString());

            // Assert
            Assert.AreEqual(tripId, trip.Id);
            Assert.AreEqual(2, trip.ContainerCount);
            Assert.AreEqual(29.60m, trip.MaxTemperature);
            var roundedMeanTemperature = decimal.Round(GetMeanTemperature(container1, container2), 2);
            Assert.AreEqual(roundedMeanTemperature, trip.MeanTemperature);
            Assert.AreEqual(1, trip.SpoiledContainerCount);
            Assert.AreEqual(1039, trip.SpoiledProductCount);
        }

        private decimal GetMeanTemperature(ContainerCreationDetails container1, ContainerCreationDetails container2)
        {
            var measurements = container1.Measurements.Concat(container2.Measurements);
            return measurements.Average(measurement => measurement.Value);
        }
    }
}
