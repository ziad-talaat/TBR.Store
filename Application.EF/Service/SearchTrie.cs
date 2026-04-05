using TBL.Core.Contracts;

namespace TBL.EF.Service
{
    public class SearchTrie : ISearchTrie
    {
        private Dictionary<char, SearchTrie> child = new();
        private bool isLeaf;
        private int clickedCount = 0;

        public void Insert(string str, int clickCount, int idx = 0)
        {
            str = str.ToLower().Trim();
            if (idx == str.Length)
            {
                isLeaf = true;
                clickedCount = clickCount;
            }
            else
            {
                char cur = str[idx];
                if (!child.ContainsKey(cur))
                    child[cur] = new SearchTrie();
                child[cur].Insert(str, clickCount, ++idx);
            }
        }
        public void UpdateCount(string str, int clickCount, int idx = 0)
        {
            str = str.ToLower().Trim();
            if (idx == str.Length)
            {
                if (isLeaf)
                    clickedCount = clickCount;
                return;
            }
            else
            {
                char cur = str[idx];
                if (!child.ContainsKey(cur))
                    return;

                child[cur].UpdateCount(str, clickCount, ++idx);
            }
        }

        public bool Remove(string str, int idx = 0)
        {
            str = str.ToLower().Trim();
            if (idx == str.Length)
            {
                if (!isLeaf) return false;
                isLeaf = false;
                return child.Count == 0;
            }
            char cur = str[idx];
            if (!child.ContainsKey(cur)) return false;
            bool shouldDelete = child[cur].Remove(str, idx + 1);
            if (shouldDelete)
            {
                child.Remove(cur);
                return !isLeaf && child.Count == 0;
            }
            return false;
        }

        public void GetAllWords(string prefix, List<(string Word, int ClickCount)> words)
        {
            if (isLeaf)
                words.Add((prefix, clickedCount));
            foreach (var kvp in child)
                kvp.Value.GetAllWords(prefix + kvp.Key, words);
        }

        public void AutoComplete(string prefix, List<(string Word, int ClickCount)> words)
        {
            if (string.IsNullOrEmpty(prefix)) return;
            prefix = prefix.ToLower().Trim();
            SearchTrie cur = this;
            foreach (char c in prefix)
            {
                if (!cur.child.ContainsKey(c)) return;
                cur = cur.child[c];
            }
            cur.GetAllWords(prefix, words);
        }
    }
}