using System;
using System.Collections.Generic;
using System.Linq;

namespace AgileSqlClub.SqlPackageFilter.Extensions
{
    public class Optional<T> where T : class
    {
        internal readonly T instance = default(T);
        private Func<T> getDefault = null;

        public Optional(T instance, Func<T> getDefault = null)
        {
            this.instance = instance;
            this.getDefault = getDefault;
        }

        public Optional<T> Default(Func<T> func)
        {
            this.getDefault = func;
            return this;
        }

        public T Evaluate()
        {
            if (this.instance != default(T))
                return this.instance;
            else if (this.getDefault != null)
                return this.getDefault();
            return default(T);
        }
    }

    public static class OptionalExtensions
    {
        public static Optional<T> AsOptional<T>(this T obj) where T : class
        {
            return new Optional<T>(obj);
        }

        public static Optional<T> AsOptional<T>(this object obj, Func<object, T> func) where T : class
        {
            var value = func.Invoke(obj);
            return new Optional<T>(value);
        }

        public static Optional<T2> ValueOrDefault<T1, T2>(this Optional<T1> optional, Func<T1, T2> func) where T1 : class where T2 : class
        {
            var value = func.Invoke(optional.instance);
            return new Optional<T2>(value);
        }

        public static Optional<T> OptionalAt<T>(this Optional<IList<T>> optional, int index = 0) where T : class
        {
            return new Optional<T>(optional?.instance?.ElementAtOrDefault(index));
        }

        public static Optional<T> OptionalAt<T>(this Optional<List<T>> optional, int index = 0) where T : class
        {
            return new Optional<T>(optional?.instance?.ElementAtOrDefault(index));
        }
    }
}
