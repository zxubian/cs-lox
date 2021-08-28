namespace cslox.Types
{
    public abstract class LoxInstanceBase<T> : ILoxInstance where T: ILoxClass
    {
        protected readonly T @class;
        protected LoxInstanceBase(T @class)
        {
            this.@class = @class;
        }
        public abstract object Get(Token name);

        public abstract object Set(Token name, object value);

        public override string ToString() => $"{@class.Name} instance";
    }
}