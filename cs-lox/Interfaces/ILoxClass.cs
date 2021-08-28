namespace cslox
{
    public interface ILoxClass
    {
        bool TryFindMethod(string name, out LoxFunction method);
        string Name { get; }
    }
}