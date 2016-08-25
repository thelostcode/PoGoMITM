using System.Threading.Tasks;
using PoGo_Proxy.Models;

namespace PoGo_Proxy.Dumpers
{
    public interface IDataDumper
    {
        Task Dump<T>(T context);
    }
}
