using System.ComponentModel;
using System.Reflection;
using Humanizer;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.Core.Helpers
{
    public static class Enumeration
    {
        public static List<IdNameDto> GetAll<TEnum>(LetterCasing letterCasing = LetterCasing.Title) where TEnum : struct
        {
            var enumerationType = typeof(TEnum);

            if (!enumerationType.IsEnum)
            {
                throw new ArgumentException("Enumeration type is expected.");
            }

            var dictionary = new Dictionary<Guid, string>();

            foreach (Guid value in Enum.GetValues(enumerationType))
            {
                var name = Enum.GetName(enumerationType, value);
                dictionary.Add(value, name);
            }

            var myList = dictionary.ToList();

            myList.Sort(
                (pair1, pair2) => string.Compare(pair1.Value, pair2.Value, StringComparison.Ordinal)
            );

            var list = new List<IdNameDto>();
            myList.ForEach(item =>
            {
                list.Add(new IdNameDto { Id = item.Key, Name = item.Value.Humanize(letterCasing) });
            });

            return list;
        }

        public static List<IdNameDescriptionDto> GetDescriptionAll<TEnum>(LetterCasing letterCasing = LetterCasing.Title) where TEnum : struct
        {
            var enumerationType = typeof(TEnum);

            if (!enumerationType.IsEnum)
            {
                throw new ArgumentException("Enumeration type is expected.");
            }

            var dictionary = new Dictionary<int, string>();

            foreach (int value in Enum.GetValues(enumerationType))
            {
                var name = enumerationType.GetEnumName(value);

                var memInfo = enumerationType.GetMember(name);

                var descriptionAttribute = memInfo[0]
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;

                dictionary.Add(value, descriptionAttribute != null ? descriptionAttribute.Description : name);
            }

            var myList = dictionary.ToList();

            myList.Sort(
                (pair1, pair2) => string.Compare(pair1.Value, pair2.Value, StringComparison.Ordinal)
            );

            var list = new List<IdNameDescriptionDto>();
            myList.ForEach(item => list.Add(new IdNameDescriptionDto { Id = item.Key, Description = item.Value }));

            return list;
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    return retVal;
                }

                retVal++;
            }

            return -1;
        }

        ///<summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }

        public static List<IdNameDto> GetValues<T>()
        {
            var values = new List<IdNameDto>();
            foreach (var itemType in Enum.GetValues(typeof(T)))
            {
                //For each value of this enumeration, add a new EnumValue instance
                values.Add(new IdNameDto()
                {
                    Name = Enum.GetName(typeof(T), itemType).Humanize(LetterCasing.Title),
                    Id = (Guid)itemType
                });
            }

            return values;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes != null && attributes.Length > 0
                    ? attributes[0].Description
                    : value.ToString();
        }

        public static Dictionary<string, int> GetEnumDictionary<T>()
        {
            var keyPairsDictionary = new Dictionary<string, int>();
            foreach (var name in Enum.GetNames(typeof(T)))
            {
                //For each value of this enumeration, add a new EnumValue instance
                if (!keyPairsDictionary.ContainsKey(name))
                {
                    keyPairsDictionary.Add(name, (int)Enum.Parse(typeof(T), name));
                }
            }

            return keyPairsDictionary;
        }
    }
}
