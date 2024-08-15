using System.Reflection;

namespace AspNetCore.InMemoryCache.Utilities
{
    public static class Mapper
    {
        public static TTarget MapTo<TSource, TTarget>(this TSource source) where TTarget : class
        {
            if (source == null) return null;

            // Record türlerinde parametreli yapıcı kullanmamız gerektiği için
            // reflection ile türü oluşturmak yerine her bir özellik için bir değer alıp
            // yapıcı parametrelerine geçireceğiz.
            var srcProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var targetCtor = typeof(TTarget).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault();

            if (targetCtor == null) throw new InvalidOperationException("No public constructor found.");

            var ctorParams = targetCtor.GetParameters();
            var paramValues = new object[ctorParams.Length];

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var param = ctorParams[i];
                var sourceProp = srcProperties.FirstOrDefault(p => p.Name == param.Name);
                if (sourceProp != null)
                {
                    paramValues[i] = sourceProp.GetValue(source);
                }
                else
                {
                    paramValues[i] = param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null;
                }
            }

            return (TTarget)targetCtor.Invoke(paramValues);
        }

        public static List<TTarget> MapTo<TSource, TTarget>(this IEnumerable<TSource> source) where TTarget : class
        {
            if (source == null) return new List<TTarget>();

            return source.Select(s => s.MapTo<TSource, TTarget>()).ToList();
        }

    }
}
