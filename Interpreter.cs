namespace Get;

public readonly struct GetWord(string value, int start, GetWord.Type type = GetWord.Type.Word)
{
    public enum Type
    {
        FunctionCall,
        Word,
        Number,
        String,
        Expression
    }

    public readonly string value = value;
    public readonly Type type = type;

    public readonly int start = start;

    public override bool Equals(object? obj)
    {
        if (obj is Type t)
            return this == t;
        else if (obj is string s)
            return this == s;
        return false;
    }

    public override int GetHashCode() => base.GetHashCode();

    public override string ToString() => $"{type}: " + (type == Type.String ? $"\"{value}\"" : type == Type.Expression ? $"({value})" : $"{value}");

    public static bool operator ==(GetWord word, Type t) => word.type == t;
    public static bool operator !=(GetWord word, Type t) => !(word.type == t);

    public static bool operator ==(GetWord word, string s) => word.value == s;
    public static bool operator !=(GetWord word, string s) => !(word.value == s);
}

public struct GetVariable(object value, bool mutable = false)
{
    public object value = value;
    public readonly bool mutable = mutable;
}

public readonly struct GetFunction()
{

}

public class Interpreter(string text, Dictionary<string, object>? vars = null, Dictionary<string, GetFunction>? funcs = null)
{
    private readonly string[] lines = text.Split('\n');
    private readonly Dictionary<string, object> vars = vars ?? [];
    private readonly Dictionary<string, GetFunction> funcs = funcs ?? [];

    private static Func<int, int, string, (T?, string?)> ColError<T>(T? _)
    {
        return (ln, col, msg) => (default, $"error on line {ln + 1}, col {col + 1}: {msg}");
    }

    private static Func<int, string, (T?, string?)> LineError<T>(T? _)
    {
        return (ln, msg) =>
        {
            return (default, $"error on line {ln + 1}: {msg}");
        };
    }

    private static (GetWord[]?, string?) ParseLine(string line, int ln)
    {
        static bool numChar(char ch) => ch >= '0' && ch <= '9';
        static bool identChar(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || numChar(ch) || ch == '_';

        var error = ColError(Array.Empty<GetWord>());
        List<GetWord> words = [];
        GetWord.Type? collecting = null;
        bool escape = false;
        string acc = "";
        int accStart = -1;

        for (int idx = 0; idx < line.Length; idx++)
        {
            char ch = line[idx];
            if (collecting != null)
            {
                if (escape)
                {
                    switch (ch)
                    {
                        case '\\' or '"' or '\'':
                            acc += ch;
                            break;
                        case 'n':
                            acc += '\n';
                            break;
                        case 't':
                            acc += '\t';
                            break;
                        case '0':
                            acc += '\0';
                            break;
                        default:
                            return error(ln, idx, $"invalid escape character '{ch}'");
                    }
                    continue;
                }

                switch (collecting)
                {
                    case GetWord.Type.Word:
                        if (!identChar(ch))
                        {
                            words.Add(new(acc, accStart, acc.IsUpper() ? GetWord.Type.FunctionCall : (GetWord.Type)collecting));
                            acc = "";
                            accStart = -1;
                            collecting = null;
                            continue;
                        }
                        break;
                    case GetWord.Type.Number:
                        if (!numChar(ch) && !acc.Contains('.'))
                        {
                            words.Add(new(acc, accStart, (GetWord.Type)collecting));
                            acc = "";
                            accStart = -1;
                            collecting = null;
                            continue;
                        }
                        break;
                    case GetWord.Type.String:
                        if (ch == '"')
                        {
                            words.Add(new(acc, accStart, (GetWord.Type)collecting));
                            acc = "";
                            accStart = -1;
                            collecting = null;
                            continue;
                        }
                        break;
                    case GetWord.Type.Expression:
                        if (ch == ')')
                        {
                            words.Add(new(acc, accStart, (GetWord.Type)collecting));
                            acc = "";
                            accStart = -1;
                            collecting = null;
                            continue;
                        }
                        break;
                }

                if (ch == '\\' && collecting == GetWord.Type.String) escape = true;
                //Console.WriteLine($"'{ch}', {collecting}");
                acc += ch;
            }
            else
            {
                if (numChar(ch))
                {
                    collecting = GetWord.Type.Number;
                    accStart = idx;
                    idx--;
                }
                else if (identChar(ch))
                {
                    collecting = GetWord.Type.Word;
                    accStart = idx;
                    idx--;
                }
                else if (ch == '"')
                {
                    collecting = GetWord.Type.String;
                    accStart = idx;
                }
                else if (ch == '(')
                {
                    collecting = GetWord.Type.Expression;
                    accStart = idx;
                }
                else if (ch == '\'')
                    break;
                else if (!((char[])['\t', ' ']).Contains(ch))
                    return error(ln, idx, $"illegal character '{ch}'");
            }
        }

        if (collecting != null)
        {
            if (collecting == GetWord.Type.String)
                return error(ln, accStart, "unterminated string literal");
            else if (collecting == GetWord.Type.Expression)
                return error(ln, accStart, "unterminated expression");
            words.Add(new(acc, accStart, (GetWord.Type)collecting));
        }

        return ([.. words], null);
    }

    private (GetWord[][]?, string?) GenerateInfo()
    {
        var error = LineError(Array.Empty<GetWord[]>());
        List<GetWord[]> wordLines = [];

        for (int ln = 0; ln < lines.Length; ln++)
        {
            var (wordLine, err) = ParseLine(lines[ln], ln);
            if (err != null) return (null, err);
            wordLines.Add(wordLine!);
        }

        return ([.. wordLines], null);
    }

    public (object?, string?) Interpret()
    {
        var (wordLines, err) = GenerateInfo();
        if (err != null) return (null, err);
        var error = LineError(new object());

        foreach (var wordLine in wordLines!)
        {
            foreach (var word in wordLine)
                Console.WriteLine(word);
            Console.Write("\n");
        }

        /*
        for (int ln = 0; ln < wordLines!.Length; ln++)
        {
            GetWord[] line = wordLines![ln];
        }
        */

        return (null, null);
    }
}