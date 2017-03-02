namespace StarlightDirector.UI.Controls.Models
{
    /// <summary>
    /// Internal representation of bars for drawing
    /// </summary>
    internal sealed class DrawingBar
    {
        public int DrawType { get; set; }
        public int Timing { get; set; }
        public double T { get; set; }
        public double Y { get; set; }
    }
}
