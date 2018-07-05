using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using ShippingContainerSpoilage.WebApi;
using ShippingContainerSpoilage.WebApi.Controllers;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.Tests
{
    public class ContainerSpoilageControllerTests
    {
        [Test]
        public void CreateTrip_ValidatesTripDetails()
        {
            // Arrange
            var tripCreationDetails = new TripCreationDetails();
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.ValidateTripCreationDetails(tripCreationDetails))
                .Return((false, "error"));
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.CreateTrip(tripCreationDetails);

            // Assert
            Assert.AreEqual(typeof(BadRequestErrorMessageResult), response.GetType());
        }

        [Test]
        public void CreatesTrip()
        {
            // Arrange
            var tripCreationDetails = new TripCreationDetails();
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.ValidateTripCreationDetails(tripCreationDetails))
                .Return((true, ""));
            containerSpoilage.Stub(x => x.CreateTrip(tripCreationDetails)).Return(1234);
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.CreateTrip(tripCreationDetails);

            // Assert
            Assert.AreEqual(typeof(CreatedNegotiatedContentResult<Trip>), response.GetType());
        }

        [Test]
        public void CreateContainer_ChecksTripExists()
        {
            // Arrange
            var containerCreationDetails = new ContainerCreationDetails();
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.TripExists("1234")).Return(false);
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.CreateContainer("1234", containerCreationDetails);

            // Assert
            Assert.AreEqual(typeof(NotFoundResult), response.GetType());
        }

        [Test]
        public void CreateContainer_ValidatesContainer()
        {
            // Arrange
            var containerCreationDetails = new ContainerCreationDetails();
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.TripExists("1234")).Return(true);
            containerSpoilage.Stub(x => x.ValidateContainerCreationDetails(containerCreationDetails))
                .Return((false, "error"));
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.CreateContainer("1234", containerCreationDetails);

            // Assert
            Assert.AreEqual(typeof(BadRequestErrorMessageResult), response.GetType());
        }

        [Test]
        public void CreatesContainer()
        {
            // Arrange
            var containerCreationDetails = new ContainerCreationDetails();
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.TripExists("1234")).Return(true);
            containerSpoilage.Stub(x => x.ValidateContainerCreationDetails(containerCreationDetails))
                .Return((true, ""));
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.CreateContainer("1234", containerCreationDetails);

            // Assert
            Assert.AreEqual(typeof(CreatedNegotiatedContentResult<ContainerCreationDetails>), response.GetType());
        }

        [Test]
        public void GetTrip_NotFound()
        {
            // Arrange
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.TripExists("1234")).Return(false);
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.GetTrip("1234");

            // Assert
            Assert.AreEqual(typeof(NotFoundResult), response.GetType());
        }

        [Test]
        public void GetsTrips()
        {
            // Arrange
            var containerSpoilage = MockRepository.GenerateMock<IContainerSpoilage>();
            containerSpoilage.Stub(x => x.TripExists("1234")).Return(true);
            containerSpoilage.Stub(x => x.GetTrip("1234")).Return(new Trip());
            var controller = new ContainerSpoilageController(containerSpoilage);

            // Act
            var response = controller.GetTrip("1234");

            // Assert
            Assert.AreEqual(typeof(OkResultWithWeakETag<Trip>), response.GetType());
        }
    }
}
