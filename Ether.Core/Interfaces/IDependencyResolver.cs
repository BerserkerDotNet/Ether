namespace Ether.Core.Interfaces
{
    public interface IDependencyResolver
    {
        T Resolve<T>() 
            where T : class;
    }
}
