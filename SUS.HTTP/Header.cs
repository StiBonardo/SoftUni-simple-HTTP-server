namespace SUS.HTTP
{
    public class Header
    {
        public Header(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public Header(string headerLine)
        {
            var args = headerLine.Split(new string[] { ": " }, 2, System.StringSplitOptions.None);
            this.Name = args[0];
            this.Value = args[1];
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{this.Name}: {this.Value}";
        }
    }
}