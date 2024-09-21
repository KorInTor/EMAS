using Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IStorableObject
    {
        public int Id { get; set; }
        public string ShortInfo { get; }
    }

	public static class StorableObjectExtensions
	{
		public static Dictionary<StorableObjectType, List<IStorableObject>> GroupByType(this IEnumerable<IStorableObject> items)
		{
			return items
				.GroupBy(item => item.GetEnumType())
				.ToDictionary(group => group.Key, group => group.ToList());
		}

		public static StorableObjectType GetEnumType(this IStorableObject item)
		{
			if (item is Equipment)
			{
				return StorableObjectType.Equipment;
			}
			else if (item is MaterialPiece)
			{
				return StorableObjectType.Material;
			}
			else
			{
				throw new ArgumentException("Unknown type of IStorableObject");
			}
		}

	}
}
