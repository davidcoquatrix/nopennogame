namespace NPNG.Domain.Entities;

/// <summary>
/// Emplacement générique 1-ou-2 nombres pour une catégorie de score structuré : <c>A</c> seul pour
/// <see cref="Enums.CategoryInputShape.FreeValue"/> et <see cref="Enums.CategoryInputShape.Achieved"/>
/// (0/1), <c>A</c>×<c>B</c> pour <see cref="Enums.CategoryInputShape.MultiplierByCount"/>.
/// </summary>
public readonly record struct CategoryValue(int A, int B = 0);
