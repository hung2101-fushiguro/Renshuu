using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services;

public class ImportService
{
    public const int MaxSubjectLength = 200;
    public const int MaxContentLength = 2000;
    public const int MaxOptionLength = 500;
    public const int MaxExplanationLength = 2000;
    public const int MaxPassageTitleLength = 300;
    public const int MaxPassageContentLength = 20000;

    private static readonly Regex PassageMarker = new(
        @"^\s*\[\s*(?:BÀI\s*ĐỌC|BAI\s*DOC)\s*\]\s*[:：]?\s*(.*)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex QuestionMarker = new(
        @"^\s*\[\s*(?:CÂU\s*HỎI|CAU\s*HOI)\s*\]\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EndPassageMarker = new(
        @"^\s*\[\s*(?:HẾT\s*BÀI|HET\s*BAI)\s*\]\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Dictionary<string, string> ColumnAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["subject"] = "subject",
        ["môn"] = "subject",
        ["mon"] = "subject",
        ["môn học"] = "subject",
        ["mon hoc"] = "subject",
        ["content"] = "content",
        ["câu hỏi"] = "content",
        ["cau hoi"] = "content",
        ["nội dung"] = "content",
        ["noi dung"] = "content",
        ["a"] = "a",
        ["optiona"] = "a",
        ["đáp án a"] = "a",
        ["dap an a"] = "a",
        ["b"] = "b",
        ["optionb"] = "b",
        ["đáp án b"] = "b",
        ["dap an b"] = "b",
        ["c"] = "c",
        ["optionc"] = "c",
        ["đáp án c"] = "c",
        ["dap an c"] = "c",
        ["d"] = "d",
        ["optiond"] = "d",
        ["đáp án d"] = "d",
        ["dap an d"] = "d",
        ["correct"] = "correct",
        ["correctoption"] = "correct",
        ["đáp án"] = "correct",
        ["dap an"] = "correct",
        ["đáp án đúng"] = "correct",
        ["dap an dung"] = "correct",
        ["explanation"] = "explanation",
        ["giải thích"] = "explanation",
        ["giai thich"] = "explanation",
    };

    private readonly AppDbContext _db;

    public ImportService(AppDbContext db) => _db = db;

    public static List<ParsedQuestion> ParsePaste(string? subjectName, string? text)
    {
        var results = new List<ParsedQuestion>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return results;
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');

        string? content = null, a = null, b = null, c = null, d = null, correct = null, explanation = null;
        var optionStyle = "ABCD";
        var index = 0;

        void Reset()
        {
            content = a = b = c = d = correct = explanation = null;
            optionStyle = "ABCD";
        }

        void Flush()
        {
            if (content is null && a is null && b is null && c is null && d is null && correct is null)
            {
                Reset();
                return;
            }

            index++;
            results.Add(BuildQuestion(index, subjectName, content, a, b, c, d, correct, explanation, optionStyle));
            Reset();
        }

        bool TryAddOptions(string line)
        {
            if (TrySplitInlineOptions(line, out var inlineStyle, out var inlineValues, out var inlinePrefix))
            {
                if (a is not null || b is not null || c is not null || d is not null)
                {
                    Flush();
                }

                if (content is null && !string.IsNullOrWhiteSpace(inlinePrefix))
                {
                    content = CleanQuestionNumber(inlinePrefix);
                }

                if (content is null)
                {
                    return false;
                }

                a = inlineValues[0];
                b = inlineValues[1];
                c = inlineValues[2];
                d = inlineValues[3];
                optionStyle = inlineStyle;
                return true;
            }

            var optionMatch = Regex.Match(line, @"^([A-Da-d])\s*[\.\)\:]\s*(.*)$");
            if (!optionMatch.Success)
            {
                optionMatch = Regex.Match(line, @"^([1-4])\s*[\.\)\:]\s*(.*)$");
            }

            if (!optionMatch.Success)
            {
                return false;
            }

            var key = optionMatch.Groups[1].Value.ToUpperInvariant();
            var value = optionMatch.Groups[2].Value.Trim();
            var isNumeric = key[0] is >= '1' and <= '4';
            var mappedKey = isNumeric ? ((char)('A' + (key[0] - '1'))).ToString() : key;

            if (content is null)
            {
                return false;
            }

            if (mappedKey == "A" && a is not null)
            {
                Flush();
            }

            if (content is null)
            {
                return false;
            }

            switch (mappedKey)
            {
                case "A": a = value; break;
                case "B": b = value; break;
                case "C": c = value; break;
                case "D": d = value; break;
            }

            optionStyle = isNumeric ? "1234" : "ABCD";
            return true;
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                Flush();
                continue;
            }

            if (TryAddOptions(line))
            {
                continue;
            }

            var answerMatch = Regex.Match(
                line,
                @"^(?:Đáp án|Dap an|Answer)\s*[:\.]?\s*([A-Da-d1-4])\b",
                RegexOptions.IgnoreCase);
            if (answerMatch.Success)
            {
                var answer = answerMatch.Groups[1].Value.ToUpperInvariant();
                correct = answer[0] is >= '1' and <= '4'
                    ? ((char)('A' + (answer[0] - '1'))).ToString()
                    : answer;
                continue;
            }

            var explanationMatch = Regex.Match(
                line,
                @"^(?:Giải thích|Giai thich|Explanation)\s*[:\.]?\s*(.*)$",
                RegexOptions.IgnoreCase);
            if (explanationMatch.Success)
            {
                var value = explanationMatch.Groups[1].Value.Trim();
                explanation = string.IsNullOrEmpty(explanation) ? value : explanation + " " + value;
                continue;
            }

            line = CleanQuestionNumber(line);

            if (content is null)
            {
                content = line;
            }
            else if (explanation is not null)
            {
                explanation += " " + line;
            }
            else
            {
                content += "\n" + line;
            }
        }

        Flush();
        return results;
    }

    public static List<ParsedPassage> ParseReadingPassages(string? subjectName, string? text)
    {
        var results = new List<ParsedPassage>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return results;
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');

        string? title = null;
        var passageLines = new List<string>();
        var questionLines = new List<string>();
        var errors = new List<string>();
        var state = 0;
        var hasQuestionMarker = false;
        var passageIndex = 0;
        var questionIndex = 0;

        void Reset()
        {
            title = null;
            passageLines = new List<string>();
            questionLines = new List<string>();
            errors = new List<string>();
            hasQuestionMarker = false;
            state = 0;
        }

        void FinalizePassage()
        {
            if (state == 0)
            {
                return;
            }

            passageIndex++;
            var content = string.Join("\n", passageLines).Trim();
            var parsed = ParsePaste(subjectName, string.Join("\n", questionLines));
            var questions = new List<ParsedQuestion>();
            foreach (var question in parsed)
            {
                questionIndex++;
                questions.Add(question with { Index = questionIndex });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                errors.Add("Thiếu nội dung bài đọc.");
            }
            else if (content.Length > MaxPassageContentLength)
            {
                errors.Add($"Nội dung bài đọc quá dài (tối đa {MaxPassageContentLength} ký tự).");
            }

            if (title is not null && title.Length > MaxPassageTitleLength)
            {
                errors.Add($"Tiêu đề bài đọc quá dài (tối đa {MaxPassageTitleLength} ký tự).");
            }

            if (!hasQuestionMarker)
            {
                errors.Add("Thiếu dòng [CÂU HỎI].");
            }

            if (questions.Count == 0)
            {
                errors.Add("Bài đọc chưa có câu hỏi nào.");
            }

            results.Add(new ParsedPassage(passageIndex, title, content, questions, errors));
            Reset();
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var passageMatch = PassageMarker.Match(line);
            if (passageMatch.Success)
            {
                FinalizePassage();
                title = passageMatch.Groups[1].Value.Trim();
                if (title.Length == 0)
                {
                    title = null;
                }

                state = 1;
                continue;
            }

            if (QuestionMarker.IsMatch(line))
            {
                if (state == 1)
                {
                    state = 2;
                    hasQuestionMarker = true;
                }

                continue;
            }

            if (EndPassageMarker.IsMatch(line))
            {
                FinalizePassage();
                continue;
            }

            if (state == 1)
            {
                passageLines.Add(line);
            }
            else if (state == 2)
            {
                questionLines.Add(line);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                errors.Add("Nội dung nằm ngoài [BÀI ĐỌC] đã bị bỏ qua.");
                state = 1;
                passageLines.Add(line);
            }
        }

        FinalizePassage();
        return results;
    }

    public static List<ParsedQuestion> ParseCsv(string text)
    {
        var rows = ParseCsvRows(text);
        if (rows.Count < 2)
        {
            throw new FormatException("File CSV cần có dòng tiêu đề và ít nhất một dòng dữ liệu.");
        }

        var header = rows[0];
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Length; i++)
        {
            var name = header[i].Trim().TrimStart('\uFEFF');
            if (ColumnAliases.TryGetValue(name, out var canonical) && !columnMap.ContainsKey(canonical))
            {
                columnMap[canonical] = i;
            }
        }

        var required = new[] { "subject", "content", "a", "b", "c", "d", "correct" };
        var missing = required.Where(r => !columnMap.ContainsKey(r)).ToList();
        if (missing.Count > 0)
        {
            throw new FormatException(
                "Thiếu cột bắt buộc trong file CSV: " + string.Join(", ", missing)
                + ". Cần các cột: Subject, Content, A, B, C, D, Correct, Explanation.");
        }

        string? Get(string[] row, string key) =>
            columnMap.TryGetValue(key, out var index) && index < row.Length ? row[index] : null;

        var items = new List<ParsedQuestion>();
        for (var r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            items.Add(BuildQuestion(r, Get(row, "subject"), Get(row, "content"),
                Get(row, "a"), Get(row, "b"), Get(row, "c"), Get(row, "d"),
                Get(row, "correct"), Get(row, "explanation"), Get(row, "optionstyle")));
        }

        return items;
    }

    public static List<ParsedQuestion> ParseJson(string text)
    {
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("questions", out var questions)
            && questions.ValueKind == JsonValueKind.Array)
        {
            root = questions;
        }

        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("File JSON phải là một mảng câu hỏi (hoặc object có thuộc tính \"questions\").");
        }

        var items = new List<ParsedQuestion>();
        var index = 0;
        foreach (var element in root.EnumerateArray())
        {
            index++;
            if (element.ValueKind != JsonValueKind.Object)
            {
                items.Add(BuildQuestion(index, null, null, null, null, null, null, null, null)
                    with { Errors = new List<string> { "Câu hỏi không phải là object JSON." } });
                continue;
            }

            string? Get(params string[] aliases)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (aliases.Any(alias => string.Equals(property.Name, alias, StringComparison.OrdinalIgnoreCase)))
                    {
                        return property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString()
                            : property.Value.ToString();
                    }
                }

                return null;
            }

            items.Add(BuildQuestion(index,
                Get("subject", "mon", "môn", "monhoc", "môn học", "tenmon", "tên môn"),
                Get("content", "cauhoi", "câu hỏi", "noidung", "nội dung"),
                Get("a", "optiona", "optionA", "dapana", "đáp án a"),
                Get("b", "optionb", "optionB", "dapanb", "đáp án b"),
                Get("c", "optionc", "optionC", "dapanc", "đáp án c"),
                Get("d", "optiond", "optionD", "dapand", "đáp án d"),
                Get("correct", "correctoption", "dapan", "đáp án", "dapandung", "đáp án đúng"),
                Get("explanation", "giaithich", "giải thích"),
                Get("optionstyle", "option_style", "style", "kiểu đáp án", "kieu dap an")));
        }

        return items;
    }

    public async Task<HashSet<int>> FindExistingDuplicatesAsync(IReadOnlyList<ParsedQuestion> items)
    {
        var duplicates = new HashSet<int>();
        var valid = items.Where(i => i.IsValid).ToList();
        if (valid.Count == 0)
        {
            return duplicates;
        }

        var subjectMap = await GetSubjectMapAsync(valid);
        var subjectIds = subjectMap.Values.Select(s => s.Id).Distinct().ToList();
        if (subjectIds.Count == 0)
        {
            return duplicates;
        }

        var existing = await _db.Questions
            .Where(q => subjectIds.Contains(q.SubjectId))
            .Select(q => new { q.SubjectId, q.Content })
            .ToListAsync();
        var existingKeys = new HashSet<string>(existing.Select(x => DuplicateKey(x.SubjectId, x.Content)));

        foreach (var item in valid)
        {
            if (!subjectMap.TryGetValue(item.Subject!.Trim(), out var subject))
            {
                continue;
            }

            if (existingKeys.Contains(DuplicateKey(subject.Id, item.Content!)))
            {
                duplicates.Add(item.Index);
            }
        }

        return duplicates;
    }

    public async Task<SaveResult> SaveAsync(IReadOnlyList<ParsedQuestion> items)
    {
        var valid = items.Where(i => i.IsValid).ToList();
        var invalidCount = items.Count - valid.Count;
        if (valid.Count == 0)
        {
            return new SaveResult(0, 0, invalidCount);
        }

        var subjectMap = await GetSubjectMapAsync(valid);
        var newSubjects = valid
            .Select(i => i.Subject!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => !subjectMap.ContainsKey(name))
            .Select(name => new Subject { Name = name })
            .ToList();

        if (newSubjects.Count > 0)
        {
            _db.Subjects.AddRange(newSubjects);
            await _db.SaveChangesAsync();
            foreach (var subject in newSubjects)
            {
                subjectMap[subject.Name] = subject;
            }
        }

        var subjectIds = subjectMap.Values.Select(s => s.Id).Distinct().ToList();
        var existing = await _db.Questions
            .Where(q => subjectIds.Contains(q.SubjectId))
            .Select(q => new { q.SubjectId, q.Content })
            .ToListAsync();
        var seen = new HashSet<string>(existing.Select(x => DuplicateKey(x.SubjectId, x.Content)));

        var added = 0;
        var duplicates = 0;
        foreach (var item in valid)
        {
            var subject = subjectMap[item.Subject!.Trim()];
            if (!seen.Add(DuplicateKey(subject.Id, item.Content!)))
            {
                duplicates++;
                continue;
            }

            _db.Questions.Add(new Question
            {
                SubjectId = subject.Id,
                Content = item.Content!.Trim(),
                OptionA = item.OptionA!.Trim(),
                OptionB = item.OptionB!.Trim(),
                OptionC = item.OptionC!.Trim(),
                OptionD = item.OptionD!.Trim(),
                CorrectOption = item.Correct!.Trim().ToUpperInvariant(),
                OptionStyle = item.OptionStyle,
                Explanation = string.IsNullOrWhiteSpace(item.Explanation) ? null : item.Explanation.Trim(),
            });
            added++;
        }

        await _db.SaveChangesAsync();
        return new SaveResult(added, duplicates, invalidCount);
    }

    public async Task<ReadingSaveResult> SaveReadingPassagesAsync(IReadOnlyList<ParsedPassage> passages)
    {
        var invalid = 0;
        var validQuestions = new List<ParsedQuestion>();
        foreach (var passage in passages)
        {
            if (passage.Errors.Count > 0)
            {
                invalid += passage.Questions.Count;
                continue;
            }

            foreach (var question in passage.Questions)
            {
                if (question.IsValid)
                {
                    validQuestions.Add(question);
                }
                else
                {
                    invalid++;
                }
            }
        }

        if (validQuestions.Count == 0)
        {
            return new ReadingSaveResult(0, 0, 0, invalid);
        }

        var subjectMap = await GetSubjectMapAsync(validQuestions);
        var newSubjects = validQuestions
            .Select(i => i.Subject!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => !subjectMap.ContainsKey(name))
            .Select(name => new Subject { Name = name })
            .ToList();

        if (newSubjects.Count > 0)
        {
            _db.Subjects.AddRange(newSubjects);
            await _db.SaveChangesAsync();
            foreach (var subject in newSubjects)
            {
                subjectMap[subject.Name] = subject;
            }
        }

        var subjectIds = subjectMap.Values.Select(s => s.Id).Distinct().ToList();
        var existing = await _db.Questions
            .Where(q => subjectIds.Contains(q.SubjectId))
            .Select(q => new { q.SubjectId, q.Content })
            .ToListAsync();
        var seen = new HashSet<string>(existing.Select(x => DuplicateKey(x.SubjectId, x.Content)));

        var passagesAdded = 0;
        var questionsAdded = 0;
        var duplicates = 0;

        foreach (var passage in passages)
        {
            if (passage.Errors.Count > 0 || string.IsNullOrWhiteSpace(passage.Content))
            {
                continue;
            }

            var valid = passage.Questions
                .Where(q => q.IsValid && !string.IsNullOrWhiteSpace(q.Subject))
                .ToList();
            if (valid.Count == 0)
            {
                continue;
            }

            var subject = subjectMap[valid[0].Subject!.Trim()];
            var questions = new List<Question>();
            foreach (var item in valid)
            {
                if (!seen.Add(DuplicateKey(subject.Id, item.Content!)))
                {
                    duplicates++;
                    continue;
                }

                questions.Add(new Question
                {
                    SubjectId = subject.Id,
                    Content = item.Content!.Trim(),
                    OptionA = item.OptionA!.Trim(),
                    OptionB = item.OptionB!.Trim(),
                    OptionC = item.OptionC!.Trim(),
                    OptionD = item.OptionD!.Trim(),
                    CorrectOption = item.Correct!.Trim().ToUpperInvariant(),
                    OptionStyle = item.OptionStyle,
                    Explanation = string.IsNullOrWhiteSpace(item.Explanation) ? null : item.Explanation.Trim(),
                });
            }

            if (questions.Count == 0)
            {
                continue;
            }

            _db.ReadingPassages.Add(new ReadingPassage
            {
                SubjectId = subject.Id,
                Title = string.IsNullOrWhiteSpace(passage.Title) ? null : passage.Title.Trim(),
                Content = passage.Content.Trim(),
                Questions = questions,
            });

            passagesAdded++;
            questionsAdded += questions.Count;
        }

        await _db.SaveChangesAsync();
        return new ReadingSaveResult(passagesAdded, questionsAdded, duplicates, invalid);
    }

    public static ParsedQuestion BuildQuestion(
        int index,
        string? subject,
        string? content,
        string? a, string? b, string? c, string? d,
        string? correct,
        string? explanation,
        string? optionStyle = null)
    {
        var errors = new List<string>();

        subject = subject?.Trim();
        content = content?.Trim();
        a = a?.Trim();
        b = b?.Trim();
        c = c?.Trim();
        d = d?.Trim();
        correct = string.IsNullOrWhiteSpace(correct) ? null : correct.Trim().ToUpperInvariant();
        explanation = explanation?.Trim();
        optionStyle = optionStyle == "1234" ? "1234" : "ABCD";

        if (string.IsNullOrEmpty(subject))
        {
            errors.Add("Thiếu tên môn học.");
        }
        else if (subject.Length > MaxSubjectLength)
        {
            errors.Add($"Tên môn học quá dài (tối đa {MaxSubjectLength} ký tự).");
        }

        if (string.IsNullOrEmpty(content))
        {
            errors.Add("Thiếu nội dung câu hỏi.");
        }
        else if (content.Length > MaxContentLength)
        {
            errors.Add($"Nội dung quá dài (tối đa {MaxContentLength} ký tự).");
        }

        void CheckOption(string? value, string key)
        {
            if (string.IsNullOrEmpty(value))
            {
                errors.Add($"Thiếu đáp án {key}.");
            }
            else if (value.Length > MaxOptionLength)
            {
                errors.Add($"Đáp án {key} quá dài (tối đa {MaxOptionLength} ký tự).");
            }
        }

        CheckOption(a, "A");
        CheckOption(b, "B");
        CheckOption(c, "C");
        CheckOption(d, "D");

        if (correct is null || correct.Length != 1 || !"ABCD".Contains(correct[0]))
        {
            errors.Add("Đáp án đúng phải là A, B, C hoặc D.");
        }

        if (explanation is not null && explanation.Length > MaxExplanationLength)
        {
            errors.Add($"Giải thích quá dài (tối đa {MaxExplanationLength} ký tự).");
        }

        return new ParsedQuestion(index, subject, content, a, b, c, d, correct, explanation, errors)
        {
            OptionStyle = optionStyle,
        };
    }

    private static string CleanQuestionNumber(string line) =>
        Regex.Replace(
            line,
            @"^(?:\[\s*\d+\s*\]|[（(]\s*\d+\s*[）)]|\d+\s*[\.\)])\s*",
            string.Empty);

    private static bool TrySplitInlineOptions(
        string line,
        out string style,
        out string[] values,
        out string prefix)
    {
        style = "ABCD";
        values = Array.Empty<string>();
        prefix = string.Empty;

        var matches = Regex.Matches(line, @"(?<!\S)([1-4A-Da-d])\s*[\.\)\:]\s*");
        if (matches.Count != 4)
        {
            return false;
        }

        var keys = matches
            .Select(m => m.Groups[1].Value.ToUpperInvariant())
            .ToList();
        var isNumeric = keys.All(k => k[0] is >= '1' and <= '4');
        var isLetters = keys.All(k => k[0] is >= 'A' and <= 'D');

        if (isNumeric && string.Concat(keys) != "1234")
        {
            return false;
        }

        if (isLetters && string.Concat(keys) != "ABCD")
        {
            return false;
        }

        if (!isNumeric && !isLetters)
        {
            return false;
        }

        var parts = new string[4];
        for (var i = 0; i < 4; i++)
        {
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : line.Length;
            parts[i] = line[start..end].Trim();
        }

        prefix = line[..matches[0].Index].Trim();
        style = isNumeric ? "1234" : "ABCD";
        values = parts;
        return true;
    }

    private async Task<Dictionary<string, Subject>> GetSubjectMapAsync(IReadOnlyList<ParsedQuestion> validItems)
    {
        var names = validItems
            .Select(i => i.Subject!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var subjects = await _db.Subjects
            .Where(s => names.Contains(s.Name))
            .ToListAsync();

        var map = new Dictionary<string, Subject>(StringComparer.OrdinalIgnoreCase);
        foreach (var subject in subjects)
        {
            map[subject.Name] = subject;
        }

        return map;
    }

    private static string DuplicateKey(int subjectId, string content) =>
        subjectId + "::" + content.Trim().ToLowerInvariant();

    private static List<string[]> ParseCsvRows(string text)
    {
        var rows = new List<string[]>();
        var fields = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        builder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    builder.Append(character);
                }
            }
            else
            {
                switch (character)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        fields.Add(builder.ToString());
                        builder.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        fields.Add(builder.ToString());
                        builder.Clear();
                        rows.Add(fields.ToArray());
                        fields.Clear();
                        break;
                    default:
                        builder.Append(character);
                        break;
                }
            }
        }

        if (builder.Length > 0 || fields.Count > 0)
        {
            fields.Add(builder.ToString());
            rows.Add(fields.ToArray());
        }

        return rows.Where(row => row.Any(field => !string.IsNullOrWhiteSpace(field))).ToList();
    }
}

public record ParsedQuestion(
    int Index,
    string? Subject,
    string? Content,
    string? OptionA,
    string? OptionB,
    string? OptionC,
    string? OptionD,
    string? Correct,
    string? Explanation,
    List<string> Errors)
{
    public string OptionStyle { get; init; } = "ABCD";

    public bool IsValid => Errors.Count == 0;
}

public record ParsedPassage(
    int Index,
    string? Title,
    string? Content,
    List<ParsedQuestion> Questions,
    List<string> Errors)
{
    public bool IsValid => Errors.Count == 0 && Questions.Count > 0;
}

public record SaveResult(int Added, int Duplicates, int Invalid);

public record ReadingSaveResult(int PassagesAdded, int QuestionsAdded, int Duplicates, int Invalid);

public record ImportPreview(List<ParsedQuestion> Items, HashSet<int> Duplicates)
{
    public int ValidCount => Items.Count(i => i.IsValid) - Items.Count(i => i.IsValid && Duplicates.Contains(i.Index));
    public int InvalidCount => Items.Count(i => !i.IsValid);
    public int DuplicateCount => Items.Count(i => i.IsValid && Duplicates.Contains(i.Index));
}

public record ReadingImportPreview(List<ParsedPassage> Passages, HashSet<int> Duplicates)
{
    public int QuestionCount => Passages.Sum(p => p.Questions.Count);
    public int ValidCount => Passages
        .Where(p => p.Errors.Count == 0)
        .SelectMany(p => p.Questions)
        .Count(q => q.IsValid && !Duplicates.Contains(q.Index));
    public int InvalidCount => Passages
        .Sum(p => p.Errors.Count > 0
            ? p.Questions.Count
            : p.Questions.Count(q => !q.IsValid));
    public int DuplicateCount => Passages
        .Where(p => p.Errors.Count == 0)
        .SelectMany(p => p.Questions)
        .Count(q => q.IsValid && Duplicates.Contains(q.Index));
}
