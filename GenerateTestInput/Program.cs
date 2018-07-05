using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShippingContainerSpoilage.WebApi.Models;

namespace GenerateTestInput
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpClient = new HttpClient();
            var random = new Random(0);
            var container1 = new ContainerCreationDetails
            {
                Id = GetContainerId(),
                ProductCount = 15000,
            };
            var container2 = new ContainerCreationDetails
            {
                Id = GetContainerId(),
                ProductCount = 18000
            };
            var measurementsContainer1 = new List<TemperatureRecord>();
            var measurementsContainer2 = new List<TemperatureRecord>();
            for (var i = 0; i < 7200; i++)
            {
                measurementsContainer1.Add(new TemperatureRecord
                {
                    Time = DateTime.UtcNow + TimeSpan.FromMinutes(i),
                    Value = GetRandomTemperature(random)
                });
                measurementsContainer2.Add(new TemperatureRecord
                {
                    Time = DateTime.UtcNow + TimeSpan.FromMinutes(i),
                    Value = GetRandomTemperature(random)
                });
            }

            container1.Measurements = measurementsContainer1.ToArray();
            container2.Measurements = measurementsContainer2.ToArray();

            var container1json = JsonConvert.SerializeObject(container1);
            var container2json = JsonConvert.SerializeObject(container2);

            var url = "http://shippingcontainerspoilagewebapi.azurewebsites.net/trips/containers?tripId=1";
            var result1 = httpClient.PostAsync(url, new StringContent(container1json, Encoding.UTF8, "application/json")).Result;
            var result2 = httpClient.PostAsync(url, new StringContent(container2json, Encoding.UTF8, "application/json")).Result;
        }

        private static decimal GetRandomTemperature(Random random)
        {
            return Math.Round((decimal) random.NextDouble() * (50 - 3) + 3, 2);
        }

        private static string GetContainerId()
        {
            Thread.Sleep(1000);
            var seconds = (long)(DateTime.UtcNow - DateTime.MinValue).TotalSeconds;
            return seconds.ToString();
        }
    }
}
