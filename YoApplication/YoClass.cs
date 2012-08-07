using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace YoApplication
{
    public class YoClass
    {
        private List<string[]> baseWords = new List<string[]>();
        private List<ReplaceWord> exceptionWords = new List<ReplaceWord>();
        private string chars = "йцукенгшщзхъфывапролджэячсмитьбю";
        private List<string>[] words = new List<string>[0];
        private int[][] inds = null;
        public YoClass()
        {
            baseWords = Properties.Resources._base.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Split(' ')).ToList();// File.ReadAllLines(@"Resource/basesE.constu", Encoding.Default).ToList();
            exceptionWords = Properties.Resources.exceptions.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(a => new ReplaceWord { Word = a.Split(' ')[0], Index = int.Parse(a.Split(' ')[1]) }).ToList();
            words = chars.Select(a => baseWords.Where(q => q[0].StartsWith(a.ToString())).Select(q => q[0]).ToList()).ToArray();
            inds = chars.Select(a => baseWords.Where(q => q[0].StartsWith(a.ToString())).Select(q => int.Parse(q[1])).ToArray()).ToArray();
        }

        public string PasteLetter(string text)
        {
            int count = 0;
            var arr = text.GetBlocks().ToList();
            for (var i = 0; i < arr.Count; i++)
            {
                if (chars.Contains(arr[i].ToLower()[0]))
                {
                    var ind = 0;
                    try
                    {
                        ind = words[chars.ToList().IndexOf(arr[i].ToLower()[0])].IndexOf(arr[i].ToLower());
                    }
                    catch (Exception ee)
                    { }
                    if (ind != -1)
                    {
                        var index = inds[chars.ToList().IndexOf(arr[i].ToLower()[0])][ind];
                        arr[i] = arr[i].Remove(index, 1).Insert(index, arr[i][index] == 'е' ? "ё" : "Ё");// = lettersE[chars.ToList().IndexOf(arr[i].ToLower()[0])][ind];
                        count++;
                    }
                }
            }

            var str = string.Join("", arr.ToArray());
            return str;
        }

        public bool IsContainsException(string text, out List<ReplaceWord> wordsExceptions)
        {
            var arr = text.GetBlocks().ToList();
            wordsExceptions = new List<ReplaceWord>();
            for (var i = 0; i < arr.Count; i++)
            {
                if (chars.Contains(arr[i].ToLower()[0]))
                {
                    var excWords = exceptionWords.SingleOrDefault(q => q.Word == arr[i].ToLower());
                    if (!wordsExceptions.Contains(excWords) && excWords.Word != null)
                        wordsExceptions.Add(excWords);
                }
            }
            return wordsExceptions.Count != 0;
        }

        public string PasteLetterExceptions(string text, ReplaceWord word)
        {
            var arr = text.GetBlocks().ToList();
            for (var i = 0; i < arr.Count; i++)
            {
                if (chars.Contains(arr[i].ToLower()[0]))
                {
                    if (arr[i].ToLower() == word.Word)
                        arr[i] = arr[i].Remove(word.Index, 1).Insert(word.Index, arr[i][word.Index] == 'е' ? "ё" : "Ё");
                }
            }
            var str = string.Join("", arr.ToArray());
            return str;
        }
    }

    public struct ReplaceWord : IEquatable<ReplaceWord>
    {
        public string Word;
        public int Index;

        public bool Equals(ReplaceWord other)
        {
            return this.Word == other.Word;
        }
    }
}
