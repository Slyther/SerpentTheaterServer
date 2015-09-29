using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class Utilities
    {
        public static string PadElementsInLines(List<string[]> lines, int padding = 1)
        {
            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (var i = 0; i < numElements; i++)
            {
                maxValues[i] = lines.Max(x => (x.Length > i + 1 && x[i] != null ? x[i].Length : 0)) + padding;
            }
            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;
                for (var i = 0; i < line.Length; i++)
                {
                    var value = line[i];
                    if (value != null)
                    {
                        sb.Append(value.PadRight(maxValues[i]));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
