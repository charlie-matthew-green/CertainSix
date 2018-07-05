using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ShippingContainerSpoilage.WebApi.Models;

namespace ShippingContainerSpoilage.WebApi.Controllers
{
    public interface IDalFacade
    {
        long AddTrip(TripCreationDetails tripCreationDetails);
        void AddContainer(long tripId, ContainerCreationDetails containerCreationDetails);
        bool TryGetTripDetails(long tripId, out TripWithSpoilDetails trip);
        IEnumerable<ContainerCreationDetails> GetContainers(long tripId);
    }

    /// Persistance is done via stored procedures rather than with an ORM due to time constraints and the fact
    /// that I'm more familiar with using Ado.Net directly. With more time it would be better to use an ORM,
    /// especially if the domain is likely to get more complicated, to reduce the amount of code manually
    /// written and have the benefit of saving objects in transactions (then don't end up with half of the
    /// object saved in the database if something goes wrong half way through)
    public class DalFacade : IDalFacade
    {
        private readonly string connectionString;

        public DalFacade(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public long AddTrip(TripCreationDetails tripCreationDetails)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("Add Trip", connection))
            {
                connection.Open();
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@name", SqlDbType.NVarChar);
                command.Parameters["@name"].Value = tripCreationDetails.Name;
                command.Parameters.Add("@spoilTemperature", SqlDbType.Decimal);
                command.Parameters["@spoilTemperature"].Value = tripCreationDetails.SpoilTemperature;
                command.Parameters.Add("@spoilDurationInMinutes", SqlDbType.BigInt);
                command.Parameters["@spoilDurationInMinutes"].Value = tripCreationDetails.SpoilDuration;
                
                return (long)command.ExecuteScalar();
            }
        }

        public void AddContainer(long tripId, ContainerCreationDetails containerCreationDetails)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("Add Container", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@tripId", SqlDbType.BigInt);
                    command.Parameters["@tripId"].Value = tripId;
                    command.Parameters.Add("@containerId", SqlDbType.NVarChar);
                    command.Parameters["@containerId"].Value = containerCreationDetails.Id;
                    command.Parameters.Add("@productCount", SqlDbType.Int);
                    command.Parameters["@productCount"].Value = containerCreationDetails.ProductCount;

                    command.ExecuteNonQuery();
                }

                foreach (var measurement in containerCreationDetails.Measurements)
                {
                    using (var command = new SqlCommand("Add Temperature Record", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@containerId", SqlDbType.NVarChar);
                        command.Parameters["@containerId"].Value = containerCreationDetails.Id;
                        command.Parameters.Add("@temperature", SqlDbType.Decimal);
                        command.Parameters["@temperature"].Value = measurement.Value;
                        command.Parameters.Add("@recordedAt", SqlDbType.DateTime);
                        command.Parameters["@recordedAt"].Value = measurement.Time;

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public bool TryGetTripDetails(long tripId, out TripWithSpoilDetails trip)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("Get Trip", connection))
            {
                connection.Open();
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@id", SqlDbType.BigInt);
                command.Parameters["@id"].Value = tripId;

                var reader = command.ExecuteReader();
                var found = TryReadTrip(reader, out trip);
                return found;
            }
        }

        public IEnumerable<ContainerCreationDetails> GetContainers(long tripId)
        {
            var containers = new List<ContainerCreationDetails>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("Get Containers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@tripId", SqlDbType.BigInt);
                    command.Parameters["@tripId"].Value = tripId;

                    var reader = command.ExecuteReader();
                    containers = ReadContainers(reader).ToList();
                    reader.Close();
                }

                foreach (var container in containers)
                {
                    using (var command = new SqlCommand("Get Temperature Records", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@containerId", SqlDbType.NVarChar);
                        command.Parameters["@containerId"].Value = container.Id;

                        var reader = command.ExecuteReader();
                        container.Measurements = ReadTemperatureRecords(reader).ToArray();
                        reader.Close();
                    }
                }
            }

            return containers;
        }

        private bool TryReadTrip(IDataReader reader, out TripWithSpoilDetails trip)
        {
            trip = null;
            while (reader.Read())
            {
                trip = new TripWithSpoilDetails();
                trip.SpoilDuration = (int)reader["Spoil Duration in Minutes"];
                trip.SpoilTemperature = (decimal)reader["Spoil Temperature"];
                trip.Id = (long) reader["Id"];
                return true;
            }
            return false;
        }

        private IEnumerable<ContainerCreationDetails> ReadContainers(IDataReader reader)
        {
            while (reader.Read())
            {
                var contatiner = new ContainerCreationDetails();
                contatiner.Id = (string)reader["Id"];
                contatiner.ProductCount = (int) reader["Product Count"];
                yield return contatiner;
            }
        }

        private IEnumerable<TemperatureRecord> ReadTemperatureRecords(IDataReader reader)
        {
            while (reader.Read())
            {
                var temperatureRecord = new TemperatureRecord();
                temperatureRecord.Time = (DateTime)reader["Recorded At"];
                temperatureRecord.Value = (decimal) reader["Temperature"];
                yield return temperatureRecord;
            }
        }
    }
}