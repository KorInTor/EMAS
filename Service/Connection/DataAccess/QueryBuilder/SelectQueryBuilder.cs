using Model.Event;
using Npgsql;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Service.Connection.DataAccess.QueryBuilder
{
    public class SelectQueryBuilder
    {
        private StringBuilder query = new StringBuilder();

        private static Dictionary<string, string> propertyColumnDictionaries;

        public static Dictionary<string, string> PropertyColumnDictionaries
        {
            get
            {
                if (propertyColumnDictionaries is null)
                {
                    propertyColumnDictionaries = [];

                    propertyColumnDictionaries.Add(GetFullPropertyName<StorableObjectEvent>(x => x.EmployeeId), "employee_id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<StorableObjectEvent>(x => x.Id), "id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<StorableObjectEvent>(x => x.EventType), "event_type");
                    propertyColumnDictionaries.Add(GetFullPropertyName<StorableObjectEvent>(x => x.DateTime), "\"date\"");

                    propertyColumnDictionaries.Add(GetFullPropertyName<AdditionEvent>(x => x.Id), "event_id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<AdditionEvent>(x => x.LocationId), "location_id");

                    propertyColumnDictionaries.Add(GetFullPropertyName<SentEvent>(x => x.DepartureId), "departure_id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<SentEvent>(x => x.DestinationId), "destination_id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<SentEvent>(x => x.Comment), "dispatch_info");

                    propertyColumnDictionaries.Add(GetFullPropertyName<ArrivedEvent>(x => x.Id), "arrival_event_id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<ArrivedEvent>(x => x.Comment), "arrival_info");
                    propertyColumnDictionaries.Add(GetFullPropertyName<ArrivedEvent>(x => x.SentEventId), "dispatch_event_id");

                    propertyColumnDictionaries.Add(GetFullPropertyName<ReservedEvent>(x => x.Comment), "reserve_start_info");
                    propertyColumnDictionaries.Add(GetFullPropertyName<ReservedEvent>(x => x.LocationId), "location_id");

                    propertyColumnDictionaries.Add(GetFullPropertyName<ReserveEndedEvent>(x => x.Id), "end_event_id");
                    propertyColumnDictionaries.Add(GetFullPropertyName<ReserveEndedEvent>(x => x.Comment), "reserve_end_info");
                    propertyColumnDictionaries.Add(GetFullPropertyName<ReserveEndedEvent>(x => x.ReserveEventId), "start_event_id");
                }

                return propertyColumnDictionaries;
            }
        }

        private List<BaseCondition> _conditionsList = [];

        private static Dictionary<Type, string> typeTableDict;

        public static Dictionary<Type, string> TypeTableDict
        {
            get
            {
                if (typeTableDict is null)
                {
                    typeTableDict = [];
                    typeTableDict.Add(typeof(StorableObjectEvent), "public.\"event\"");
                    typeTableDict.Add(typeof(AdditionEvent), "\"event\".addition");
                    typeTableDict.Add(typeof(SentEvent), "\"event\".delivery");
                    typeTableDict.Add(typeof(ArrivedEvent), "\"event\".delivery");
                    typeTableDict.Add(typeof(ReservedEvent), "\"event\".reservation");
                    typeTableDict.Add(typeof(ReserveEndedEvent), "\"event\".reservation");
                }
                return typeTableDict;
            }
        }

        private Type assignedType;

        public SelectQueryBuilder(Type type, IEnumerable<BaseCondition> conditions)
        {
            assignedType = type;

            _conditionsList = conditions.ToList();
        }

        private void BuildBlankSelectQuery(Type type)
        {
            List<string> columns = [];
            string table = string.Empty;
            string specialCase = string.Empty;

            if (typeof(StorableObjectEvent).IsAssignableFrom(type))
            {
                table = TypeTableDict[typeof(StorableObjectEvent)];

                var eventMappings = new Dictionary<Type, (string columns, string tableName, string eventIdColumnName)>
                {
                    { typeof(AdditionEvent), ("a.location_id", "\"event\".addition as a", "a.event_id") },
                    { typeof(SentEvent), ("d.dispatch_info, d.departure_id, d.destination_id", "\"event\".delivery as d", "d.dispatch_event_id") },
                    { typeof(ArrivedEvent), ("d.arrival_info, d.dispatch_event_id","\"event\".delivery as d","d.arrival_event_id") },
                    { typeof(ReservedEvent), ("r.reserve_start_info ,r.location_id","\"event\".reservation as r","r.start_event_id") },
                    { typeof(ReserveEndedEvent), ("r.reserve_end_info ,r.start_event_id","\"event\".reservation as r","r.end_event_id") },
                };

                columns.Add("employee_id");
                columns.Add("id");
                columns.Add("event_type");
                columns.Add("\"date\"");

                if (eventMappings.TryGetValue(type, out var mapping))
                {
                    columns.Add(mapping.columns);
                    specialCase = $"JOIN {mapping.tableName} ON {mapping.eventIdColumnName} = id";
                }
            }

            if (table == string.Empty)
                throw new ArgumentException("This type is not suported");

            query.Append($"SELECT {string.Join(", ", columns)} FROM {table} {specialCase}");

            
        }

        public NpgsqlCommand Build(NpgsqlConnection connection)
        {
            BuildBlankSelectQuery(assignedType);

            var command = new NpgsqlCommand(query.ToString(), connection);

            AddConditions(command);

            command.CommandText = query.ToString();

            return command;
        }

        private void AddConditions(NpgsqlCommand command)
        {
            bool isFirstCondition = true;

            foreach (var condition in _conditionsList)
            {
                if (!isFirstCondition)
                {
                    query.Append(" AND ");
                }
                else
                {
                    query.Append(" WHERE ");
                }
                if (condition is CompareCondition compareCondition)
                {
                    if (compareCondition.value is IEnumerable enumerable)
                    {
                        //compareCondition.value = enumerable.Cast<object>().ToArray();
                        query.Append(PropertyColumnDictionaries[compareCondition.propertyName] + " " + CompareCondition.ComparisonToString(compareCondition.comparisonType) + " ANY(@" + PropertyColumnDictionaries[compareCondition.propertyName] + ")");
                    }
                    else
                    {
                        query.Append(PropertyColumnDictionaries[compareCondition.propertyName] + " " + CompareCondition.ComparisonToString(compareCondition.comparisonType) + " @" + PropertyColumnDictionaries[compareCondition.propertyName]);
                    }

                    command.Parameters.AddWithValue("@" + PropertyColumnDictionaries[compareCondition.propertyName], compareCondition.value);

                }
                if (condition is MaxCondition)
                {
                    query.Append($"{PropertyColumnDictionaries[condition.propertyName]} = (SELECT MAX({PropertyColumnDictionaries[condition.propertyName]}) FROM {TypeTableDict[assignedType]})");
                }
                if (condition is NullCondition nullCondition)
                {
                    string x = nullCondition.IsNull ? "" : "not";
                    query.Append($"{PropertyColumnDictionaries[nullCondition.propertyName]} is {x} null");
                }

                isFirstCondition = false;
            }

        }

        public static string GetFullPropertyName<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression? member = expression.Body as MemberExpression;
            if (member == null)
            {
                UnaryExpression? unary = expression.Body as UnaryExpression;
                if (unary != null)
                {
                    member = unary.Operand as MemberExpression;
                }
            }

            return $"{typeof(T).Name}.{member.Member.Name}";
        }
    }

    public enum Comparison
    {
        Equal,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        NotEqual
    }

    public class CompareCondition : BaseCondition
    {
        public Comparison comparisonType;

        public object value;

        public CompareCondition(string propertyName, Comparison comparisonType, object value) : base(propertyName)
        {
            this.comparisonType = comparisonType;
            this.value = value;
        }

        public static string ComparisonToString(Comparison comparison)
        {
            string comparisonOperator = comparison switch
            {
                Comparison.Equal => "=",
                Comparison.GreaterThan => ">",
                Comparison.LessThan => "<",
                Comparison.GreaterThanOrEqual => ">=",
                Comparison.LessThanOrEqual => "<=",
                Comparison.NotEqual => "<>",
                _ => throw new ArgumentException("Invalid comparison type")
            };

            return comparisonOperator;
        }
    }

    public class MaxCondition : BaseCondition
    {
        public MaxCondition(string propertyName) : base(propertyName)
        {

        }
    }

    public class NullCondition : BaseCondition
    {
        public bool IsNull;

        public NullCondition(string propertyName, bool isNull) : base(propertyName)
        {
            IsNull = isNull;
        }
    }

    public class BaseCondition
    {
        public string propertyName;

        public BaseCondition(string propertyName)
        {
            this.propertyName = propertyName;
        }
    }
}
