
namespace Publisher
{
    internal class User
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            if (this.Name == null)
            {
                throw new NullReferenceException(nameof(Name));
            }

            return $"{this.Guid} - {this.Name}";
        }
    }
}