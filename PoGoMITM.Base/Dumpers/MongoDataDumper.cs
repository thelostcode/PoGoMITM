using System;
using System.Threading.Tasks;
using PoGoMITM.Base.MongoDB;

namespace PoGoMITM.Base.Dumpers
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