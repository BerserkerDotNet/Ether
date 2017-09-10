using System.Threading.Tasks;

namespace Ether.Interfaces
{ 
    public interface IVSTSClient
    {
        Task<T> ExecuteGet<T>(string url);
        Task<T> ExecutePost<T>(string url, object payload);
        Task<T> ExecutePost<T>(string url, string payload);
    }
}