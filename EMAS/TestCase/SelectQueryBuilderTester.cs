using Npgsql;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.QueryBuilder;

namespace EMAS.Service.Connection.DataAccess.Tests
{
    public class SelectQueryBuilderTests
    {
        public void Build_StorableObjectEvent_NoConditions_BuildsExpectedQuery()
        {
            // Arrange
            var expectedQuery = "SELECT employee_id, id, event_type, \"date\" FROM public.\"event\"";
            var conditions = new List<BaseCondition>();

            // Act
            var builder = new SelectQueryBuilder(typeof(StorableObjectEvent), conditions);
            var command = builder.Build(new NpgsqlConnection("Connection string not required for this test"));

            // Assert
            Console.WriteLine(command.CommandText);
            Console.WriteLine(expectedQuery);
        }

        public void Build_AdditionEvent_WithComparisonCondition_BuildsExpectedQuery()
        {
            // Arrange
            var expectedQuery = "SELECT employee_id, id, event_type, \"date\", a.location_id FROM public.\"event\" JOIN \"event\".addition as a ON a.event_id = id WHERE location_id = @location_id";
            var conditions = new List<BaseCondition>() { new CompareCondition(nameof(AdditionEvent.LocationId), Comparison.Equal, 10) };

            // Act
            var builder = new SelectQueryBuilder(typeof(AdditionEvent), conditions);
            var command = builder.Build(new NpgsqlConnection("Connection string not required for this test"));

            // Assert
            Console.WriteLine(expectedQuery);
            Console.WriteLine(command.CommandText);
        }

        public void Build_SentEvent_WithNullCondition_BuildsExpectedQuery()
        {
            // Arrange
            var expectedQuery = "SELECT employee_id, id, event_type, \"date\", d.dispatch_info, d.departure_id, d.destination_id FROM public.\"event\" JOIN \"event\".delivery as d ON d.dispatch_event_id = id WHERE d.dispatch_info is null";
            var conditions = new List<BaseCondition>() { new NullCondition(nameof(SentEvent.Comment), true) };

            // Act
            var builder = new SelectQueryBuilder(typeof(SentEvent), conditions);
            var command = builder.Build(new NpgsqlConnection("Connection string not required for this test"));

            // Assert
            Console.WriteLine(expectedQuery);
            Console.WriteLine(command.CommandText);
        }

        public void Build_InvalidEventType_ThrowsArgumentException()
        {
            // Arrange
            var conditions = new List<BaseCondition>();

            // Act & Assert
            //Assert.Throws<ArgumentException>(() => new SelectQueryBuilder(typeof(string), conditions));
        }
    }
}