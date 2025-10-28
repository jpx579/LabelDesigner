namespace LabelDesigner.Models
{
    public class LabelModel
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Dpi { get; set; } = 300;
        public List<ElementInfo> LabelElements { get; set; } = new();
        public ElementInfo? FindElement(string name) => LabelElements.FirstOrDefault(e => e.Name == name);
    }
}
