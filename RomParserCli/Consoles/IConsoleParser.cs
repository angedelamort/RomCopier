using System.Collections.Generic;

namespace RomCopier.Consoles
{
    public interface IConsoleParser
    {
        void Init(string countriesString);
        List<string> Filter(List<string> files);
    }
}
