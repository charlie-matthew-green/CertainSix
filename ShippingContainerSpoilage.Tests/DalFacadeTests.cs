using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using ShippingContainerSpoilage.WebApi.Controllers;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.Tests
{
    [TestFixture, Category("Database")]
    public class DalFacadeTests
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ContainerSpoilage"].ConnectionString;

        [SetUp]
        [TearDown]
        public void ClearTables()
        {
            var commandText = "delete from Trips; delete from Containers; delete from [Temperature Records];";
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(commandText, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        [Test]
        public void CreatesAndGetsTripCreationDetails()
        {
            // Arrange
            var dalFacade = new DalFacade(connectionString);
            var tripCreationDetails = new TripCreationDetails
            {
                Name = "NZ to GB",
                SpoilDuration = 30,
                SpoilTemperature = 28.3m
            };

            // Act
            var tripId = dalFacade.AddTrip(tripCreationDetails);
            var tripFound = dalFacade.TryGetTripDetails(tripId, out var trip);

            // Assert
            Assert.AreEqual(true, tripFound);
            Assert.AreEqual(tripId, trip.Id);
            Assert.AreEqual(tripCreationDetails.SpoilDuration, trip.SpoilDuration);
            Assert.AreEqual(tripCreationDetails.SpoilTemperature, trip.SpoilTemperature);
        }

        [Test]
        public void CreatesAndGetsContainers()
        {
            // Arrange
            var dalFacade = new DalFacade(connectionString);
            var container = new ContainerCreationDetails
            {
                Id = Guid.NewGuid().ToString(),
                ProductCount = 10_000,
                Measurements = new []
                {
                    new TemperatureRecord { Time = new DateTime(2018, 6, 26, 12, 0, 0), Value = 24.6m},
                    new TemperatureRecord { Time = new DateTime(2018, 6, 26, 12, 1, 1), Value = 26.2m},
                    new TemperatureRecord { Time = new DateTime(2018, 6, 26, 12, 3, 9), Value = 27.3m},
                }
            };
            var tripCreationDetails = new TripCreationDetails
            {
                Name = "NZ to GB",
                SpoilDuration = 30,
                SpoilTemperature = 28.3m
            };

            // Act
            var tripId = dalFacade.AddTrip(tripCreationDetails);
            dalFacade.AddContainer(tripId, container);
            var containers = dalFacade.GetContainers(tripId);

            // Assert
            Assert.AreEqual(1, containers.Count());
            var retrievedContainer = containers.First();
            Assert.AreEqual(container.Id, retrievedContainer.Id);
            Assert.AreEqual(container.ProductCount, retrievedContainer.ProductCount);
            Assert.AreEqual(3, retrievedContainer.Measurements.Length);
            var measurements = retrievedContainer.Measurements;
            Assert.AreEqual(1, measurements.Count(x => x.Time == container.Measurements[0].Time && x.Value == container.Measurements[0].Value));
            Assert.AreEqual(1, measurements.Count(x => x.Time == container.Measurements[1].Time && x.Value == container.Measurements[1].Value));
            Assert.AreEqual(1, measurements.Count(x => x.Time == container.Measurements[2].Time && x.Value == container.Measurements[2].Value));
        }
    }
}
