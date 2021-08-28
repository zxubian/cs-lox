namespace cslox
{
    public interface ILoxInstance
    {
        object Get(Token name);
        object Set(Token name, object value);
    }
}