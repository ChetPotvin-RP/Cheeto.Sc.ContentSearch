using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace Cheeto.Sc.ContentSearch
{
    public static class IIndexableExtensions
    {
        public static string GetFieldValueAsString(this IIndexable indexable, string fieldName)
        {
            var field = indexable.GetFieldByName(fieldName);

            return field != null ? field.Value as string : string.Empty;
        }

        public static Item ToItem(this IIndexable indexable)
        {
            var indexableItem = indexable as SitecoreIndexableItem;

            return indexableItem?.Item;
        }

        public static IEnumerable<IIndexable> AsIndexables(this IEnumerable<Item> items)
        {
            return items?.Where(x => x != null).Select(x => new SitecoreIndexableItem(x));
        }
    }
}
