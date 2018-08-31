using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DALFactory
{
   public partial class AbstractFactory
    {
       private static object CreateInstance(string dalAssemblyPath, string fullClassName)
       {
           var assembly = Assembly.Load(dalAssemblyPath);
           return assembly.CreateInstance(fullClassName);
       }
    }
}
