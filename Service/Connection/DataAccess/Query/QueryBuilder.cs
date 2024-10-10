using Model;
using Model.Event;
using System.Collections;
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
		private bool isInitialized = false;

		public bool IsInitialized { get =>  isInitialized; }
		public List<object> Parameters { get => parameters; }

		private static readonly ImmutableDictionary<string, string> tableSchemaName = InitializeTableSchemaDictionary();

		private static ImmutableDictionary<string, string> InitializeTableSchemaDictionary()
		{
			return new Dictionary<string, string>
			{
				{ $"{nameof(StorableObjectEvent)}", "public" },
				{ $"{nameof(AdditionEvent)}", "\"event\"" },
				{ $"{nameof(SentEvent)}", "\"event\"" },
				{ $"{nameof(ArrivedEvent)}", "\"event\"" },
				{ $"{nameof(ReservedEvent)}", "\"event\"" },
				{ $"{nameof(ReserveEndedEvent)}", "\"event\"" },
				{ $"{nameof(DecomissionedEvent)}", "\"event\"" },
				{ $"{nameof(DataChangedEvent)}", "\"event\"" },
				{ $"{nameof(Employee)}", "public" },
			}.ToImmutableDictionary();
		}

		public static ImmutableDictionary<string, string> TableSchemaName => tableSchemaName;

		private static readonly ImmutableDictionary<string, string> classTableDictionary = InitializeClassTableDictionary();

		private static ImmutableDictionary<string, string> InitializeClassTableDictionary()
		{
			return new Dictionary<string, string>
			{
				{ $"{nameof(StorableObjectEvent)}", "\"event\"" },
				{ $"{nameof(AdditionEvent)}", "addition" },
				{ $"{nameof(SentEvent)}", "delivery" },
				{ $"{nameof(ArrivedEvent)}", "delivery" },
				{ $"{nameof(ReservedEvent)}", "reservation" },
				{ $"{nameof(ReserveEndedEvent)}", "reservation" },
				{ $"{nameof(DecomissionedEvent)}", "decomission" },
                { $"{nameof(DataChangedEvent)}", "data_changed" },
                { $"{nameof(Employee)}", "employee" },
			}.ToImmutableDictionary();
		}

		public static ImmutableDictionary<string, string> ClassTableName => classTableDictionary;

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

				{ $"{nameof(SentEvent)}.{nameof(SentEvent.Id)}", $"{ClassTableName[nameof(SentEvent)]}.dispatch_event_id" },
				{ $"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", $"{ClassTableName[nameof(SentEvent)]}.departure_id" },
				{ $"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", $"{ClassTableName[nameof(SentEvent)]}.destination_id" },
				{ $"{nameof(SentEvent)}.{nameof(SentEvent.Comment)}", $"{ClassTableName[nameof(SentEvent)]}.dispatch_info" },

				{ $"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.Id)}", $"{ClassTableName[nameof(ArrivedEvent)]}.arrival_event_id" },
				{ $"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.Comment)}", $"{ClassTableName[nameof(ArrivedEvent)]}.arrival_info" },
				{ $"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.SentEventId)}", $"{ClassTableName[nameof(ArrivedEvent)]}.dispatch_event_id" },

				{ $"{nameof(ReservedEvent)}.{nameof(ReservedEvent.Id)}", $"{ClassTableName[nameof(ReservedEvent)]}.start_event_id" },
				{ $"{nameof(ReservedEvent)}.{nameof(ReservedEvent.Comment)}", $"{ClassTableName[nameof(ReservedEvent)]}.reserve_start_info" },
				{ $"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}", $"{ClassTableName[nameof(ReservedEvent)]}.location_id" },

				{ $"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.Id)}", $"{ClassTableName[nameof(ReserveEndedEvent)]}.end_event_id" },
				{ $"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.Comment)}", $"{ClassTableName[nameof(ReserveEndedEvent)]}.reserve_end_info" },
				{ $"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.ReserveEventId)}", $"{ClassTableName[nameof(ReserveEndedEvent)]}.start_event_id" },

				{ $"{nameof(DecomissionedEvent)}.{nameof(DecomissionedEvent.Id)}", $"{ClassTableName[nameof(DecomissionedEvent)]}.event_id" },
				{ $"{nameof(DecomissionedEvent)}.{nameof(DecomissionedEvent.Comment)}", $"{ClassTableName[nameof(DecomissionedEvent)]}.reason" },
				{ $"{nameof(DecomissionedEvent)}.{nameof(DecomissionedEvent.LocationId)}", $"{ClassTableName[nameof(DecomissionedEvent)]}.location_id" },

                { $"{nameof(DataChangedEvent)}.{nameof(DataChangedEvent.Id)}", $"{ClassTableName[nameof(DataChangedEvent)]}.event_id" },
                { $"{nameof(DataChangedEvent)}.{nameof(DataChangedEvent.Comment)}", $"{ClassTableName[nameof(DataChangedEvent)]}.comment" },

                { $"{nameof(Employee)}.{nameof(Employee.Id)}", $"{ClassTableName[nameof(Employee)]}.id" },
				{ $"{nameof(Employee)}.{nameof(Employee.Fullname)}", $"{ClassTableName[nameof(Employee)]}.fullname" },
				{ $"{nameof(Employee)}.{nameof(Employee.Email)}", $"{ClassTableName[nameof(Employee)]}.email" },
				{ $"{nameof(Employee)}.{nameof(Employee.Username)}", $"{ClassTableName[nameof(Employee)]}.username" }
			}.ToImmutableDictionary();
		}

		public static ImmutableDictionary<string, string> PropertyToColumnName => propertyColumnDictionary;

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

		public QueryBuilder LeftJoin(string table, string condition)
		{
			joins.Add($"LEFT JOIN {table} ON {condition}");
			return this;
		}

		private string GenerateWhereCondition(string propertyName, string comparison, object? value)
		{
			CheckConditionCorrectnes(propertyName, comparison, value);

			if (value == null)
			{
                return $"{PropertyToColumnName[propertyName]} {comparison} NULL";
            }
			else
			if (value is IEnumerable enumerableValue && value is not string)
			{
				if (!value.GetType().IsArray)
				{
					throw new ArgumentException("Use Array as parametr in query");
				}
				parameters.Add(enumerableValue);
				return $"{PropertyToColumnName[propertyName]} = ANY(@{parameters.Count - 1})";
			}
			else
			{
				parameters.Add(value);
				return $"{PropertyToColumnName[propertyName]} {comparison} @{parameters.Count - 1}";
			}
		}

		public QueryBuilder Where(string propertyName, string comparison, object? value)
		{
			
			if (whereConditions.Count > 0)
			{
				whereConditions.Add($"AND {GenerateWhereCondition(propertyName, comparison, value)}");
			}
			else
			{
				whereConditions.Add(GenerateWhereCondition(propertyName, comparison, value));
			}

			return this;
		}

		public QueryBuilder OrWhere(string propertyName, string comparison, object? value)
		{
			if (whereConditions.Count > 0)
			{
				whereConditions.Add($"OR {GenerateWhereCondition(propertyName, comparison, value)}");
			}
			else
			{
				whereConditions.Add(GenerateWhereCondition(propertyName, comparison, value));
			}

			return this;
		}

		private void CheckConditionCorrectnes(string propertyName, string comparison, object? value)
		{
			if (!IsComparisonOperatorValid(comparison))
			{
				throw new ArgumentException("Invalid comparison operator", nameof(comparison));
			}

			if ((comparison != "=") && value is IEnumerable)
			{
                throw new ArgumentException("If value is collection use ANY or =", nameof(value));
            }

			if (comparison == "LIKE" && value is not string)
			{
				throw new ArgumentException("Value must be a string for LIKE operator", nameof(value));
			}

			if (comparison != "IS" && comparison != "IS NOT" && value is null)
			{
				throw new ArgumentException("Invalid comparison operator for NULL value", nameof(comparison));
			}

			if ((comparison == "IS" || comparison == "IS NOT") && value is not null)
			{
				throw new ArgumentException("Value is not null for NULL comparison", nameof(comparison));
			}
		}

		private static readonly HashSet<string> validComparisonOperators = new HashSet<string>
		{
			"=", "!=", "<", ">", "<=", ">=", "LIKE", "IS", "IS NOT", "ANY"
		};

		private bool IsComparisonOperatorValid(string comparison)
		{
			return validComparisonOperators.Contains(comparison);
		}

		public QueryBuilder OrderBy(string propertyName, bool descending = false)
		{
			orderBy = $"ORDER BY {PropertyToColumnName[propertyName]} {(descending ? "DESC" : "ASC")}";
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

		public QueryBuilder LazyInit<T>()
		{
			if (isInitialized)
				return this;

			var className = typeof(T).Name;

			switch (className)
			{
				case nameof(StorableObjectEvent):
					Select([
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.EmployeeId)),
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id)),
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.EventType)),
						GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.DateTime))
						]);
					From(GetFullTableName<StorableObjectEvent>());
					break;
				case nameof(AdditionEvent):
					this.LazyInit<StorableObjectEvent>();
					Select([GetColumnName<AdditionEvent>(nameof(AdditionEvent.LocationId))]);
					Join(GetFullTableName<AdditionEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<AdditionEvent>(nameof(AdditionEvent.Id))}");
					whereConditions.RemoveAll(s => s.Contains("event_type"));
					Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.Addition);
					break;
				case nameof(SentEvent):
					this.LazyInit<StorableObjectEvent>();
					Select([
						GetColumnName<SentEvent>(nameof(SentEvent.Comment)),
						GetColumnName<SentEvent>(nameof(SentEvent.DepartureId)),
						GetColumnName<SentEvent>(nameof(SentEvent.DestinationId))
						]);
					Join(GetFullTableName<SentEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<SentEvent>(nameof(SentEvent.Id))}");
					whereConditions.RemoveAll(s => s.Contains("event_type"));
					Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.Sent);
					break;
				case nameof(ArrivedEvent):
					this.LazyInit<StorableObjectEvent>();
					Select([
						GetColumnName<ArrivedEvent>(nameof(ArrivedEvent.Comment)),
						GetColumnName<SentEvent>(nameof(SentEvent.Id))
						]);
					Join(GetFullTableName<ArrivedEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<ArrivedEvent>(nameof(ArrivedEvent.Id))}");
					whereConditions.RemoveAll(s => s.Contains("event_type"));
					Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.Arrived);
					break;
				case nameof(ReservedEvent):
					this.LazyInit<StorableObjectEvent>();
					Select([
						GetColumnName<ReservedEvent>(nameof(ReservedEvent.Comment)),
						GetColumnName<ReservedEvent>(nameof(ReservedEvent.LocationId))
						]);
					Join(GetFullTableName<ReservedEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<ReservedEvent>(nameof(ReservedEvent.Id))}");
					whereConditions.RemoveAll(s => s.Contains("event_type"));
					Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.Reserved);
					break;
				case nameof(ReserveEndedEvent):
					this.LazyInit<StorableObjectEvent>();
					Select([
						GetColumnName<ReserveEndedEvent>(nameof(ReserveEndedEvent.Comment)),
						GetColumnName<ReservedEvent>(nameof(ReservedEvent.Id))
						]);
					Join(GetFullTableName<ReserveEndedEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<ReserveEndedEvent>(nameof(ReserveEndedEvent.Id))}");
					whereConditions.RemoveAll(s => s.Contains("event_type"));
					Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.ReserveEnded);
					break;
				case nameof(DecomissionedEvent):
					this.LazyInit<StorableObjectEvent>();
					Select([
						GetColumnName<DecomissionedEvent>(nameof(DecomissionedEvent.Comment)),
						GetColumnName<DecomissionedEvent>(nameof(DecomissionedEvent.LocationId))
						]);
					Join(GetFullTableName<DecomissionedEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<DecomissionedEvent>(nameof(DecomissionedEvent.Id))}");
					whereConditions.RemoveAll(s => s.Contains("event_type"));
					Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.Decommissioned);
					break;
                case nameof(DataChangedEvent):
                    this.LazyInit<StorableObjectEvent>();
                    Select([
                        GetColumnName<DataChangedEvent>(nameof(DataChangedEvent.Comment))
                        ]);
                    Join(GetFullTableName<DataChangedEvent>(), $"{GetColumnName<StorableObjectEvent>(nameof(StorableObjectEvent.Id))} = {GetColumnName<DataChangedEvent>(nameof(DataChangedEvent.Id))}");
                    whereConditions.RemoveAll(s => s.Contains("event_type"));
                    Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EventType)}", "=", (int)EventType.DataChanged);
                    break;
                case nameof(Employee):
					Select([
						GetColumnName<Employee>(nameof(Employee.Id)),
						GetColumnName<Employee>(nameof(Employee.Fullname)),
						GetColumnName<Employee>(nameof(Employee.Username)),
						GetColumnName<Employee>(nameof(Employee.Email)),
						]);
					From(GetFullTableName<Employee>());
					break;
				default:
					throw new NotImplementedException();
			}
			isInitialized = true;
			return this;
		}

		public string GetColumnName<T>(string propertyName)
		{
			return PropertyToColumnName[$"{typeof(T).Name}.{propertyName}"];
		}

		public string GetFullTableName<T>()
		{
			return $"{TableSchemaName[typeof(T).Name]}.{ClassTableName[typeof(T).Name]}";
		}

		public QueryBuilder ClearTables()
		{
			selectColumns.Clear();
			fromTable = "";
			joins.Clear();
			query.Clear();
			isInitialized = false;
			return this;
		}
	}
}
