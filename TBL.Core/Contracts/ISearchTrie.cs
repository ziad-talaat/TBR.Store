using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Contracts
{
    public interface ISearchTrie
    {
        void GetAllWords(string prefix, List<(string Word, int ClickCount)> words);
        void AutoComplete(string prefix, List<(string Word, int ClickCount)> words);
        void Insert(string str,int clickCount, int idx = 0);
        bool Remove(string str, int idx = 0);
        void UpdateCount(string str, int clickCount, int idx = 0);
    }
}
