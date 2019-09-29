using System.Collections.Generic;
using AutoMapper;

namespace Ether.Core.Extensions
{
    public static class IMapperExtensions
    {
        public static IEnumerable<TDestination> MapCollection<TDestination>(this IMapper mapper, object source)
        {
            return mapper.Map<IEnumerable<TDestination>>(source);
        }
    }
}
