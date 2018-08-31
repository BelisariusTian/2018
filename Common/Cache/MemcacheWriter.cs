using Memcached.ClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Cache
{
    public class MemcacheWriter : ICacheWriter
    {
        private static readonly MemcachedClient mc = null;
        static MemcacheWriter()
        {
            string strAppMemcache = System.Configuration.ConfigurationManager.AppSettings["MemcachedServerList"];
            if (string.IsNullOrEmpty(strAppMemcache))
            {
                throw new Exception("Mencache配置文件未配置！");
            }
            string[] serverlist = strAppMemcache.Split(',');

            //初始化池
            SockIOPool pool = SockIOPool.GetInstance();
            pool.SetServers(serverlist);

            pool.InitConnections = 3;
            pool.MinConnections = 3;
            pool.MaxConnections = 5;

            pool.SocketConnectTimeout = 1000;
            pool.SocketTimeout = 3000;

            pool.MaintenanceSleep = 30;
            pool.Failover = true;

            pool.Nagle = false;
            pool.Initialize();

            // 获得客户端实例
            mc = new MemcachedClient();
            mc.EnableCompression = false;
        }
        public void AddCache(string key, object value, DateTime expDate)
        {
            mc.Add(key, value, expDate);
        }

        public void AddCache(string key, object value)
        {
            mc.Add(key, value);
        }

        public object GetCache(string key)
        {
            return mc.Get(key);
        }


        public T GetCache<T>(string key)
        {
            return (T)mc.Get(key);
        }

		public void RemoveCache(string key)
		{
			mc.Delete(key);
		}

		public void SetCache(string key, object value, DateTime expDate)
        {
            mc.Set(key, value, expDate);
        }

        public void SetCache(string key, object value)
        {
            mc.Set(key, value);
        }
    }
}
