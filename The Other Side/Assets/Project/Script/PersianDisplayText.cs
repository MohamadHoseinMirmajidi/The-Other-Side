using System;
using System.Collections.Generic;
using System.Text;
using TMPro;


    public static class PersianDisplayText
    {
        public static void Apply(TMP_Text target, string value)
        {
            if (target == null) return;
            if (target is RTLTextMeshPro rtl) rtl.SetRtlText(value);
            else { target.isRightToLeftText = false; target.text = Prepare(value); }
        }
        private readonly struct Forms
        {
            public Forms(char isolated, char final, char initial, char medial, bool joinsNext = true)
            { Isolated = isolated; Final = final; Initial = initial; Medial = medial; JoinsNext = joinsNext; }
            public char Isolated { get; }
            public char Final { get; }
            public char Initial { get; }
            public char Medial { get; }
            public bool JoinsNext { get; }
        }

        private static readonly Dictionary<char, Forms> Glyphs = new Dictionary<char, Forms>
        {
            ['آ'] = new Forms('\ufe81', '\ufe82', '\ufe81', '\ufe82', false),
            ['ا'] = new Forms('\ufe8d', '\ufe8e', '\ufe8d', '\ufe8e', false), ['ب'] = new Forms('\ufe8f', '\ufe90', '\ufe91', '\ufe92'),
            ['پ'] = new Forms('\ufb56', '\ufb57', '\ufb58', '\ufb59'), ['ت'] = new Forms('\ufe95', '\ufe96', '\ufe97', '\ufe98'),
            ['ث'] = new Forms('\ufe99', '\ufe9a', '\ufe9b', '\ufe9c'), ['ج'] = new Forms('\ufe9d', '\ufe9e', '\ufe9f', '\ufea0'),
            ['چ'] = new Forms('\ufb7a', '\ufb7b', '\ufb7c', '\ufb7d'), ['ح'] = new Forms('\ufea1', '\ufea2', '\ufea3', '\ufea4'),
            ['خ'] = new Forms('\ufea5', '\ufea6', '\ufea7', '\ufea8'), ['د'] = new Forms('\ufea9', '\ufeaa', '\ufea9', '\ufeaa', false),
            ['ذ'] = new Forms('\ufeab', '\ufeac', '\ufeab', '\ufeac', false), ['ر'] = new Forms('\ufead', '\ufeae', '\ufead', '\ufeae', false),
            ['ز'] = new Forms('\ufeaf', '\ufeb0', '\ufeaf', '\ufeb0', false), ['ژ'] = new Forms('\ufb8a', '\ufb8b', '\ufb8a', '\ufb8b', false),
            ['س'] = new Forms('\ufeb1', '\ufeb2', '\ufeb3', '\ufeb4'), ['ش'] = new Forms('\ufeb5', '\ufeb6', '\ufeb7', '\ufeb8'),
            ['ص'] = new Forms('\ufeb9', '\ufeba', '\ufebb', '\ufebc'), ['ض'] = new Forms('\ufebd', '\ufebe', '\ufebf', '\ufec0'),
            ['ط'] = new Forms('\ufec1', '\ufec2', '\ufec3', '\ufec4'), ['ظ'] = new Forms('\ufec5', '\ufec6', '\ufec7', '\ufec8'),
            ['ع'] = new Forms('\ufec9', '\ufeca', '\ufecb', '\ufecc'), ['غ'] = new Forms('\ufecd', '\ufece', '\ufecf', '\ufed0'),
            ['ف'] = new Forms('\ufed1', '\ufed2', '\ufed3', '\ufed4'), ['ق'] = new Forms('\ufed5', '\ufed6', '\ufed7', '\ufed8'),
            ['ک'] = new Forms('\ufb8e', '\ufb8f', '\ufb90', '\ufb91'), ['گ'] = new Forms('\ufb92', '\ufb93', '\ufb94', '\ufb95'),
            ['ل'] = new Forms('\ufedd', '\ufede', '\ufedf', '\ufee0'), ['م'] = new Forms('\ufee1', '\ufee2', '\ufee3', '\ufee4'),
            ['ن'] = new Forms('\ufee5', '\ufee6', '\ufee7', '\ufee8'), ['و'] = new Forms('\ufeed', '\ufeee', '\ufeed', '\ufeee', false),
            ['ه'] = new Forms('\ufee9', '\ufeea', '\ufeeb', '\ufeec'), ['ی'] = new Forms('\ufbfc', '\ufbfd', '\ufbfe', '\ufbff')
        };

        public static string Prepare(string value)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            value = NormalizeCharacters(value);
            if (ContainsPersian(value) == false) return value;
            var lines = value.Replace("\r\n", "\n").Split('\n');
            for (var index = 0; index < lines.Length; index++) lines[index] = ShapeAndReverse(lines[index]);
            return string.Join("\n", lines);
        }

        private static string ShapeAndReverse(string value)
        {
            var shaped = value.ToCharArray();
            for (var index = 0; index < shaped.Length; index++)
            {
                if (Glyphs.TryGetValue(value[index], out var forms) == false) continue;
                var joinsPrevious = index > 0 && CanJoinNext(value[index - 1]) && Glyphs.ContainsKey(value[index]);
                var joinsNext = index + 1 < value.Length && forms.JoinsNext && Glyphs.ContainsKey(value[index + 1]);
                shaped[index] = joinsPrevious ? (joinsNext ? forms.Medial : forms.Final) : (joinsNext ? forms.Initial : forms.Isolated);
            }
            Array.Reverse(shaped);
            var result = new StringBuilder(shaped.Length);
            for (var index = 0; index < shaped.Length;)
            {
                if (char.IsDigit(shaped[index]) == false) { result.Append(shaped[index++]); continue; }
                var end = index; while (end < shaped.Length && char.IsDigit(shaped[end])) end++;
                for (var digit = end - 1; digit >= index; digit--) result.Append(shaped[digit]);
                index = end;
            }
            return result.ToString();
        }

        private static bool CanJoinNext(char character) => Glyphs.TryGetValue(character, out var forms) && forms.JoinsNext;

        private static string NormalizeCharacters(string value)
        {
            return value
                .Replace('ي', 'ی')
                .Replace('ى', 'ی')
                .Replace('ك', 'ک');
        }

        private static bool ContainsPersian(string value)
        {
            foreach (var character in value) if (Glyphs.ContainsKey(character)) return true;
            return false;
        }
    }

