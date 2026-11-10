using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Shared;

public record SubjectOptionItem(int Id, string Name, string? FolderName, int? QuestionCount = null);

public record SubjectOptionsView(IReadOnlyList<SubjectOptionItem> Items, int? SelectedId = null)
{
    public static SubjectOptionsView From(IEnumerable<Subject> subjects, int? selectedId = null) =>
        new(subjects
            .Select(s => new SubjectOptionItem(s.Id, s.Name, s.Folder?.Name))
            .ToList(), selectedId);

    public static SubjectOptionsView FromSummaries(IEnumerable<SubjectSummary> subjects, int? selectedId = null) =>
        new(subjects
            .Select(s => new SubjectOptionItem(s.Id, s.Name, s.FolderName, s.QuestionCount))
            .ToList(), selectedId);
}
