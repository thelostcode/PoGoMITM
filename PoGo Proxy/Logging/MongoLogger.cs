using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using PoGo_Proxy.Models;
using PoGo_Proxy.MongoDB;

namespace PoGo_Proxy.Logging
{
    public class MongoLogger : ILogger
    {
        public async Task Log(PoGoWebRequest webRequest)
        {
            var collection = MongoHelper.GetCollection<PoGoWebRequest>();
            try
            {
                await collection.InsertOneAsync(webRequest);
            }
            catch (Exception)
            {
            }
        }
    }
}