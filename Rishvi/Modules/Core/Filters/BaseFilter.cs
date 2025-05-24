using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rishvi.Modules.Core.Filters
{
    public abstract class BaseFilter<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        protected IQueryable<TEntity> Query;
        protected TDto Dto;

        protected BaseFilter(IQueryable<TEntity> query, TDto dto)
        {
            Query = query;
            Dto = dto;
        }

        public IQueryable<TEntity> FilteredQuery()
        {
            if (Dto == null)
            {
                return Query;
            }

            var filterMethodInfo = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            var dtoProperties = typeof(TDto).GetProperties();

            // from base class properties
            var ignoreProperties = new[] { "Action", "Ids", "Page", "Size", "SortColumn", "SortType" };

            foreach (var propertyInfo in dtoProperties.Where(w => !ignoreProperties.Contains(w.Name)))
            {
                var typePassed = false;

                if (propertyInfo.PropertyType == typeof(string))
                {
                    typePassed = StringMethodCall(propertyInfo);
                }
                else if (propertyInfo.PropertyType == typeof(int?) ||
                         propertyInfo.PropertyType == typeof(decimal?) ||
                         propertyInfo.PropertyType == typeof(bool?) ||
                         IsNullableEnum(propertyInfo.PropertyType) ||
                         propertyInfo.PropertyType == typeof(DateTime?) ||
                         propertyInfo.PropertyType == typeof(List<int?>))
                {
                    typePassed = NullableMethodCall(propertyInfo);
                }

                if (!typePassed)
                {
                    continue;
                }

                var method = filterMethodInfo.FirstOrDefault(w => w.Name == propertyInfo.Name);

                if (method != null)
                {
                    method.Invoke(this, null);
                }
            }

            return Query;
        }

        private bool StringMethodCall(PropertyInfo propertyInfo)
        {
            var value = Convert.ToString(propertyInfo.GetValue(Dto, null));

            return !string.IsNullOrEmpty(value);
        }

        private bool NullableMethodCall(PropertyInfo propertyInfo)
        {
            var value = propertyInfo.GetValue(Dto, null);

            return value != null;
        }

        private static bool IsNullableEnum(Type t)
        {
            var u = Nullable.GetUnderlyingType(t);

            return (u != null) && u.IsEnum;
        }
    }
}