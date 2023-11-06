using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogHelper
{
    public abstract class FileAnaylzer
    {
        public abstract bool CanDoAnaylsis(string path);
        public abstract string Name();

        public abstract bool FoundPotentialData();
        public abstract void DoAnaylsis(FileStream stream);

        public abstract void DisplayData();
    }
}
