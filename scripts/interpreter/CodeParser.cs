using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class CodeParser
{
    public static List<Command> Parse(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new FormatException("Помилка компіляції: порожній код. Відсутня точка входу int main() або бібліотека <moving>.");
        }

        string cleanCode = StripComments(code);

        // Перевірка на наявність бібліотеки <moving> або <moving.h>
        if (!Regex.IsMatch(cleanCode, @"#include\s*<moving(?:\.h)?>", RegexOptions.IgnoreCase))
        {
            throw new FormatException("Помилка компіляції: відсутня точка входу int main() або бібліотека <moving>.");
        }

        // Перевірка на наявність int main()
        Match mainMatch = Regex.Match(cleanCode, @"\bint\s+main\s*\(\s*\)", RegexOptions.IgnoreCase);
        if (!mainMatch.Success)
        {
            throw new FormatException("Помилка компіляції: відсутня точка входу int main() або бібліотека <moving>.");
        }

        // Витягуємо вміст main() всередині фігурних дужок { ... }
        int mainOpenBrace = cleanCode.IndexOf('{', mainMatch.Index);
        if (mainOpenBrace == -1)
        {
            throw new FormatException("Помилка синтаксису: відсутня відкриваюча дужка '{' для int main().");
        }

        int mainCloseBrace = FindMatchingBrace(cleanCode, mainOpenBrace);
        if (mainCloseBrace == -1)
        {
            throw new FormatException("Помилка синтаксису: відсутня закриваюча дужка '}' для int main().");
        }

        string mainBody = cleanCode.Substring(mainOpenBrace + 1, mainCloseBrace - mainOpenBrace - 1);
        return ParseStatements(mainBody);
    }

    private static string StripComments(string input)
    {
        // Видалення блочних коментарів /* ... */
        string noBlockComments = Regex.Replace(input, @"/\*.*?\*/", "", RegexOptions.Singleline);
        // Видалення однорядкових коментарів // ...
        string noLineComments = Regex.Replace(noBlockComments, @"//.*$", "", RegexOptions.Multiline);
        return noLineComments;
    }

    private static int FindMatchingBrace(string text, int openBracePos)
    {
        int depth = 0;
        for (int i = openBracePos; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static List<Command> ParseStatements(string body)
    {
        var commands = new List<Command>();
        int pos = 0;

        while (pos < body.Length)
        {
            // Пропускаємо пробіли, таби, переноси рядків
            while (pos < body.Length && char.IsWhiteSpace(body[pos]))
            {
                pos++;
            }

            if (pos >= body.Length) break;

            string remaining = body.Substring(pos);

            // Ігноруємо return 0; або return;
            Match returnMatch = Regex.Match(remaining, @"^\breturn\b\s*[^;]*;");
            if (returnMatch.Success)
            {
                pos += returnMatch.Length;
                continue;
            }

            // Перевіряємо цикл for (int i = 0; i < N; i++) { ... }
            Match forMatch = Regex.Match(remaining, @"^\bfor\s*\(\s*int\s+[a-zA-Z_]\w*\s*=\s*\d+\s*;\s*[a-zA-Z_]\w*\s*<\s*(\d+)\s*;\s*[^)]+\)");
            if (forMatch.Success)
            {
                int count = int.Parse(forMatch.Groups[1].Value);
                int loopHeaderEnd = pos + forMatch.Length;

                int loopOpenBrace = body.IndexOf('{', loopHeaderEnd);
                if (loopOpenBrace == -1)
                {
                    throw new FormatException("Помилка синтаксису: відсутня відкриваюча дужка '{' для циклу for.");
                }

                int loopCloseBrace = FindMatchingBrace(body, loopOpenBrace);
                if (loopCloseBrace == -1)
                {
                    throw new FormatException("Помилка синтаксису: відсутня закриваюча дужка '}' для циклу for.");
                }

                string loopBody = body.Substring(loopOpenBrace + 1, loopCloseBrace - loopOpenBrace - 1);
                var bodyCommands = ParseStatements(loopBody);

                for (int i = 0; i < count; i++)
                {
                    commands.AddRange(bodyCommands);
                }

                pos = loopCloseBrace + 1;
                continue;
            }

            // Парсимо звичайні команди rover.*
            Match commandMatch = Regex.Match(remaining, @"^\brover\s*\.\s*(move|turn_left|turn_right)\b\s*(\(\s*\))?", RegexOptions.IgnoreCase);
            if (commandMatch.Success)
            {
                int cmdEndPos = pos + commandMatch.Length;
                
                // Перевіряємо наявність крапки з комою ';'
                int searchPos = cmdEndPos;
                while (searchPos < body.Length && char.IsWhiteSpace(body[searchPos]))
                {
                    searchPos++;
                }

                if (searchPos >= body.Length || body[searchPos] != ';')
                {
                    string actionName = commandMatch.Value;
                    throw new FormatException($"Синтаксична помилка: відсутній символ ';' після '{actionName}'");
                }

                string action = commandMatch.Groups[1].Value.ToLower();
                switch (action)
                {
                    case "move":
                        commands.Add(new Command(CommandType.Move));
                        break;
                    case "turn_left":
                        commands.Add(new Command(CommandType.TurnLeft));
                        break;
                    case "turn_right":
                        commands.Add(new Command(CommandType.TurnRight));
                        break;
                }

                pos = searchPos + 1; // Пропускаємо ';'
                continue;
            }

            // Якщо нічого не підішло, знаходимо невідомий рядок/токен
            int nextSemi = body.IndexOf(';', pos);
            int nextBrace = body.IndexOf('}', pos);
            int endErrPos = body.Length;
            if (nextSemi != -1) endErrPos = Math.Min(endErrPos, nextSemi + 1);
            if (nextBrace != -1) endErrPos = Math.Min(endErrPos, nextBrace);

            string unknownToken = body.Substring(pos, Math.Max(1, endErrPos - pos)).Trim();
            throw new FormatException($"Невідома команда або синтаксична помилка: '{unknownToken}'");
        }

        return commands;
    }
}
