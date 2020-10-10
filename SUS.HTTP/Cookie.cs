namespace SUS.HTTP
{
    public class Cookie
    {
        public Cookie(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public Cookie(string cookieLine)
        {
            var args = cookieLine.Split(new char[] { '=' }, 2);
            this.Name = args[0];
            this.Value = args[1];
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{this.Name}={this.Value}";
        }
    }
}