using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public static class EnvironmentHelper
    {
        public static T Read<T>() where T : new()
        {
            var props = typeof(T).GetProperties();

            if (props.Any(p => p.PropertyType != typeof(string)))
            {
                throw new ArgumentException("T must contain only string properties");
            }

            var res = new T();

            foreach (var item in props)
            {
                var shi = Regex.Replace(item.Name, @"[A-Z]", s => "_" + s.ToString().ToLower()).TrimStart('_');
                var v = Environment.GetEnvironmentVariable(shi);
                if (string.IsNullOrEmpty(v))
                {
                    throw new ApplicationException($"Environment variable {shi} is not defined");
                }
                item.SetValue(res, v);
            }

            return res;
        }
    }
}
