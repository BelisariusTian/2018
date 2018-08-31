using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common
{
    public static class SerializeHelper
    {
        public static string SerializeToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T SerializeToObject<T>(string str)
        {

            return JsonConvert.DeserializeObject<T>(str);
        }



    }
}
