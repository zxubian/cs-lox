namespace cslox
{
    public class Return : System.Exception
    {
        public readonly object Value;

        public Return(object value) 
        {
            Value = value;
        }
    }
}