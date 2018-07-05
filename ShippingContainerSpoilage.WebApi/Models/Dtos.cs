using System;

namespace ShippingContainerSpoilage.WebApi.Models
{
    public class TripCreationDetails
    {
        public string Name { get; set; }
        public decimal SpoilTemperature { get; set; }
        public int SpoilDuration { get; set; }
    }

    public class Trip
    {
        public long Id { get; set; }
        public int ContainerCount { get; set; }
        public decimal MaxTemperature { get; set; }
        public decimal MeanTemperature { get; set; }
        public int SpoiledContainerCount { get; set; }
        public int SpoiledProductCount { get; set; }

        public string GetETag()
        {
            return $"\"{ContainerCount}-{MaxTemperature}-{MeanTemperature}-{SpoiledContainerCount}-{SpoiledProductCount}\"";
        }
    }

    public class TripWithSpoilDetails : Trip
    {
        public decimal SpoilTemperature { get; set; }
        public int SpoilDuration { get; set; }
    }

    public class ContainerCreationDetails
    {
        public string Id { get; set; }
        public int ProductCount { get; set; }
        public TemperatureRecord[] Measurements { get; set; }
    }

    public class TemperatureRecord
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
    }
}
