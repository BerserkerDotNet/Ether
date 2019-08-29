namespace Ether.Redux.Interfaces
{
    public interface IActionResolver
    {
        T Resolve<T>();
    }
}
