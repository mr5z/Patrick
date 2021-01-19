namespace Patrick.Models
{
    class GistModel
    {
        public GistModel(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; }
        public string Content { get; }
        public string? Description { get; set; }
    }
}
