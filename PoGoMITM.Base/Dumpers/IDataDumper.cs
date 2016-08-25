using System.Threading.Tasks;

namespace PoGoMITM.Base.Dumpers
{
    public interface IDataDumper
    {
        Task Dump<T>(T context);
    }
}
