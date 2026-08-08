namespace NPNG.Domain.Entities;

/// <summary>
/// Détail de la manche pour un jeu à progression de phase (ex: Phase 10) : la phase que le joueur
/// tentait ce tour-ci, et s'il l'a validée. Persisté sur <see cref="ScoreEntry.PhaseDetail"/> pour
/// rester éditable via Time Travel, au même titre que <see cref="StructuredScoreDetail"/>.
/// </summary>
public record PhaseScoreDetail(int PhaseNumber, bool Completed);
