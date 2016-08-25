using System;
using System.Threading.Tasks;
using PoGo_Proxy.Models;
using PoGo_Proxy.MongoDB;

namespace PoGo_Proxy.Dumpers
{
    public class MongoDataDumper : IDataDumper
    {
        public async Task Dump<T>(T context)
        {
            var collection = MongoHelper.GetCollection<T>();
            try
            {
                await collection.InsertOneAsync(context);
            }
            catch (Exception)
            {
            }
        }
    }
}