using Model.Event;
using System.Collections.Immutable;
using System.Text;

namespace Service.Connection.DataAccess.Query
{
	public class QueryBuilder
	{
		private StringBuilder query = new StringBuilder();
		private List<string> selectColumns = new List<string>();
		private List<object> parameters = new List<object>();
		private string fromTable;
		private List<string> joins = new List<string>();
		private List<string> whereConditions = new List<string>();
		private string orderBy;
		private int? limit;
		private int? offset;

		private static readonly ImmutableDictionary<string, string> propertyColumnDictionary = InitializePropertyColumnDictionary();

		private static ImmutableDictionary<string, string> InitializePropertyColumnDictionary()
		{
			return new Dictionary<string, string>
			{
				{ $"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EmployeeId)}", $"{ClassTableName[nameof(StorableObjectEvent)]}.employee_id" },
				{ $"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", $"{ClassTableName[nameof(StorableObjectEvent)]}.id" },
				{ $"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", $"{ClassTableName[nameof(StorableObjectEvent)]}.event_type" },
				{ $"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.DateTime)}", $"{ClassTableName[nameof(StorableObjectEvent)]}.\"date\"" },
				{ $"{nameof(AdditionEvent)}.{nameof(AdditionEvent.Id)}", $"{ClassTableName[nameof(AdditionEvent)]}.event_id" },
				{ $"{nameof(AdditionEvent)}.{nameof(AdditionEvent.LocationId)}", $"{ClassTableName[nameof(AdditionEvent)]}.location_id" },
				{ $"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", $"{ClassTableName[nameof(SentEvent)]}.departure_id" },
				{ $"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", $"{ClassTableName[nameof(SentEvent)]}.destination_id" },
				{ $"{nameof(SentEvent)}.{nameof(SentEvent.Comment)}", $"{ClassTableName[nameof(SentEvent)]}.dispatch_info" },
				{ $"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.Id)}", $"{ClassTableName[nameof(ArrivedEvent)]}.arrival_event_id" },
				{ $"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.Comment)}", $"{ClassTableName[nameof(ArrivedEvent)]}.arrival_info" },
				{ $"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.SentEventId)}", $"{ClassTableName[nameof(ArrivedEvent)]}.dispatch_event_id" },
				{ $"{nameof(ReservedEvent)}.{nameof(ReservedEvent.Comment)}", $"{ClassTableName[nameof(ReservedEvent)]}.reserve_start_info" },
				{ $"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}", $"{ClassTableName[nameof(ReservedEvent)]}.location_id" },
				{ $"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.Id)}", $"{ClassTableName[nameof(ReserveEndedEvent)]}.end_event_id" },
				{ $"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.Comment)}", $"{ClassTableName[nameof(ReserveEndedEvent)]}.reserve_end_info" },
				{ $"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.ReserveEventId)}", $"{ClassTableName[nameof(ReserveEndedEvent)]}.start_event_id" },
				{ $"{nameof(DecomissionedEvent)}.{nameof(DecomissionedEvent.Id)}", $"{ClassTableName[nameof(DecomissionedEvent)]}.event_id" },
				{ $"{nameof(DecomissionedEvent)}.{nameof(DecomissionedEvent.Comment)}", $"{ClassTableName[nameof(DecomissionedEvent)]}.reason" }
			}.ToImmutableDictionary();
		}

		public static ImmutableDictionary<string, string> PropertyToColumnName => propertyColumnDictionary;

		private static readonly ImmutableDictionary<string, string> classTableDictionary = InitializeClassTableDictionary();

		private static ImmutableDictionary<string, string> InitializeClassTableDictionary()
		{
			return new Dictionary<string, string>
			{
				{ $"{nameof(StorableObjectEvent)}", "public.\"event\"" },
				{ $"{nameof(AdditionEvent)}", "\"event\".addition" },
				{ $"{nameof(SentEvent)}", "\"event\".delivery" },
				{ $"{nameof(ArrivedEvent)}", "\"event\".delivery" },
				{ $"{nameof(ReservedEvent)}", "\"event\".reservation" },
				{ $"{nameof(ReserveEndedEvent)}", "\"event\".reservation" },
				{ $"{nameof(DecomissionedEvent)}", "\"event\".decomission" },
			}.ToImmutableDictionary();
		}

		public static ImmutableDictionary<string, string> ClassTableName => classTableDictionary;


		public static string ToColumnName(string fullpropertyName)
		{
			return $"{nameof(DecomissionedEvent)}.{nameof(DecomissionedEvent.Id)}";
		}

		public QueryBuilder Select(string[] columns)
		{
			selectColumns.AddRange(columns);
			return this;
		}

		public QueryBuilder From(string table)
		{
			fromTable = table;
			return this;
		}

		public QueryBuilder Join(string table, string condition)
		{
			joins.Add($"JOIN {table} ON {condition}");
			return this;
		}

		public QueryBuilder InitFor(Type typeToInitFor)
		{
			var className = typeToInitFor.Name;

			switch (className)
			{
				case nameof(StorableObjectEvent):
					Select(
						[
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.EmployeeId)),
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id)),
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.EventType)),
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.DateTime))
						]
						);
					From(ClassTableName[nameof(className)]);
					break;
				case nameof(AdditionEvent):
					this.InitFor(typeof(StorableObjectEvent));
					Join(ClassTableName[nameof(className)], $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<AdditionEvent>(nameof(AdditionEvent.Id))}");
					break;
				case nameof(SentEvent):
					this.InitFor(typeof(StorableObjectEvent));
					Join(ClassTableName[nameof(className)], $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<SentEvent>(nameof(SentEvent.Id))}");
					break;
				case nameof(ArrivedEvent):
					this.InitFor(typeof(StorableObjectEvent));
					Join(ClassTableName[nameof(className)], $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<ArrivedEvent>(nameof(ArrivedEvent.Id))}");
					break;
				case nameof(ReservedEvent):
					this.InitFor(typeof(StorableObjectEvent));
					Join(ClassTableName[nameof(className)], $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<ReservedEvent>(nameof(ReservedEvent.Id))}");
					break;
				case nameof(ReserveEndedEvent):
					this.InitFor(typeof(StorableObjectEvent));
					Join(ClassTableName[nameof(className)], $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<ReserveEndedEvent>(nameof(ReserveEndedEvent.Id))}");
					break;
				case nameof(DecomissionedEvent):
					this.InitFor(typeof(StorableObjectEvent));
					Join(ClassTableName[nameof(className)], $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<DecomissionedEvent>(nameof(DecomissionedEvent.Id))}");
					break;
				default:
					throw new NotImplementedException();
			}
			return this;
		}

		public QueryBuilder LeftJoin(string table, string condition)
		{
			joins.Add($"LEFT JOIN {table} ON {condition}");
			return this;
		}

		public QueryBuilder Where(string propertyName, string comparison, object value)
		{
			if (!IsComparisonOperatorValid(comparison))
			{
				throw new ArgumentException("Invalid comparison operator", nameof(comparison));
			}

			if (comparison == "IN" && value is not IEnumerable<object>)
			{
				throw new ArgumentException("Value must be a collection for IN operator", nameof(value));
			}

			if (comparison == "LIKE" && value is not string)
			{
				throw new ArgumentException("Value must be a string for LIKE operator", nameof(value));
			}

			whereConditions.Add($"{PropertyToColumnName[propertyName]} {comparison} @{parameters.Count}");
			parameters.Add(value);
			return this;
		}

		public QueryBuilder AndWhere(string propertyName, string comparison, object value)
		{
			if (!IsComparisonOperatorValid(comparison))
			{
				throw new ArgumentException("Invalid comparison operator", nameof(comparison));
			}

			if (comparison == "IN" && value is not IEnumerable<object>)
			{
				throw new ArgumentException("Value must be a collection for IN operator", nameof(value));
			}

			if (comparison == "LIKE" && value is not string)
			{
				throw new ArgumentException("Value must be a string for LIKE operator", nameof(value));
			}

			if (whereConditions.Count > 0)
			{
				whereConditions.Add($"AND {PropertyToColumnName[propertyName]} {comparison} @{parameters.Count}");
			}
			else
			{
				whereConditions.Add($"{PropertyToColumnName[propertyName]} {comparison} @{parameters.Count}");
			}
			parameters.Add(value);
			return this;
		}

		public QueryBuilder OrWhere(string propertyName, string comparison, object value)
		{
			if (!IsComparisonOperatorValid(comparison))
			{
				throw new ArgumentException("Invalid comparison operator", nameof(comparison));
			}

			if (comparison == "IN" && value is not IEnumerable<object>)
			{
				throw new ArgumentException("Value must be a collection for IN operator", nameof(value));
			}

			if (comparison == "LIKE" && value is not string)
			{
				throw new ArgumentException("Value must be a string for LIKE operator", nameof(value));
			}

			if (whereConditions.Count > 0)
			{
				whereConditions.Add($"OR {PropertyToColumnName[propertyName]} {comparison} @{parameters.Count}");
			}
			else
			{
				whereConditions.Add($"{PropertyToColumnName[propertyName]} {comparison} @{parameters.Count}");
			}
			parameters.Add(value);
			return this;
		}

		private static readonly HashSet<string> validComparisonOperators = new HashSet<string>
		{
			"=", "!=", "<", ">", "<=", ">=", "LIKE", "IN"
		};

		private bool IsComparisonOperatorValid(string comparison)
		{
			return validComparisonOperators.Contains(comparison);
		}

		public QueryBuilder OrderBy(string column, bool descending = false)
		{
			orderBy = $"ORDER BY {column} {(descending ? "DESC" : "ASC")}";
			return this;
		}

		public QueryBuilder Limit(int limit)
		{
			this.limit = limit;
			return this;
		}

		public QueryBuilder Offset(int offset)
		{
			this.offset = offset;
			return this;
		}

		public string Build()
		{
			if (!selectColumns.Any())
			{
				throw new InvalidOperationException("No columns specified for SELECT.");
			}

			query.Append($"SELECT {string.Join(", ", selectColumns)} FROM {fromTable}");

			if (joins.Any())
			{
				query.Append($" {string.Join(" ", joins)}");
			}

			if (whereConditions.Any())
			{
				query.Append($" WHERE {string.Join(" ", whereConditions)}");
			}

			if (!string.IsNullOrEmpty(orderBy))
			{
				query.Append($" {orderBy}");
			}

			if (limit.HasValue)
			{
				query.Append($" LIMIT {limit.Value}");
			}

			if (offset.HasValue)
			{
				query.Append($" OFFSET {offset.Value}");
			}

			return query.ToString();
		}

		private string GetColumnName<T>(string propertyName)
		{
			return PropertyToColumnName[$"{typeof(T).Name}.{propertyName}"];
		}

	}

}
