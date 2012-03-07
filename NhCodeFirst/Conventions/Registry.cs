using System.Collections.Generic;

namespace NhCodeFirst.Conventions
{
    public class Registry<T>
    {
        public Registry()
        {
            defaults = new List<T>();
        }

        private readonly List<T> defaults;
        public void Insert(T t)
        {
            defaults.Insert(0, t);
        }

        public List<T> Rules
        {
            get { return new List<T>(defaults); }
        }

    }
}