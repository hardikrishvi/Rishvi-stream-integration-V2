using System.Collections.Generic;
using System.Linq;
using LinnworksMacroHelpers.Interfaces;

namespace LinnworksMacroHelpers.Classes.Email
{
    internal static class EmailHelper
    {
        internal static void Validate<TItem>(this IEnumerable<TItem> items) where TItem : class, IValidation
        {
            if (items?.Any() == true)
            {
                foreach (var item in items)
                {
                    item.Validate();
                }
            }
        }
    }
}