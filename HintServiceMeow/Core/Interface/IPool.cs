namespace HintServiceMeow.Core.Interface
{
    internal interface IPool<T>
    {
        T Rent();

        void Return(T item);
    }
}
