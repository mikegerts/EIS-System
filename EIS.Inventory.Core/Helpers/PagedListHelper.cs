using System.Collections.Generic;
using AutoMapper;
using X.PagedList;

namespace EIS.Inventory.Core.Helpers
{
    public static class PagedListHelper
    {
        public static IPagedList<TDestination> ToMappedPagedList<TSource, TDestination>(this IPagedList<TSource> list)
        {
            var sourceList = Mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(list);
            return new StaticPagedList<TDestination>(sourceList, list.GetMetaData());
        }
    }
}
