using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Humanizer;
using Humanizer.Localisation;
using Questionable.Controller;
using Questionable.Data;
using Questionable.Functions;
using Questionable.Model;
using Questionable.Model.Questing;

namespace Questionable.Windows.QuestComponents;

internal sealed class EventInfoComponent
{
    private readonly QuestData _questData;
    private readonly QuestRegistry _questRegistry;
    private readonly QuestFunctions _questFunctions;
    private readonly UiUtils _uiUtils;
    private readonly QuestController _questController;
    private readonly QuestTooltipComponent _questTooltipComponent;
    private readonly Configuration _configuration;

    public EventInfoComponent(QuestData questData,
        QuestRegistry questRegistry,
        QuestFunctions questFunctions,
        UiUtils uiUtils,
        QuestController questController,
        QuestTooltipComponent questTooltipComponent,
        Configuration configuration)
    {
        _questData = questData;
        _questRegistry = questRegistry;
        _questFunctions = questFunctions;
        _uiUtils = uiUtils;
        _questController = questController;
        _questTooltipComponent = questTooltipComponent;
        _configuration = configuration;
    }

    public bool ShouldDraw => _configuration.General.ShowIncompleteSeasonalEvents && GetActiveSeasonalQuests().Any();

    public void Draw()
    {
        // Collect active seasonal quests and group them by the folder (event) name if available
        var activeQuests = GetActiveSeasonalQuests().ToList();
        var groups = activeQuests.GroupBy(q =>
        {
            if (_questRegistry.TryGetQuestFolderName(q.QuestId, out var folder) && !string.IsNullOrEmpty(folder))
                return folder!;
            return q.Name;
        });

        foreach (var group in groups)
        {
            // pick the earliest expiry among group members to show as the group's expiry
            DateTime endsAt = group.Select(q => (q as QuestInfo)?.SeasonalQuestExpiry ?? (q is UnlockLinkQuestInfo uli ? uli.QuestExpiry : (DateTime?)null) ?? DateTime.MaxValue)
                                  .DefaultIfEmpty(DateTime.MaxValue)
                                  .Min();
            var eventQuest = new EventQuest(group.Key, group.Select(q => q.QuestId).ToList(), endsAt);
            DrawEventQuest(eventQuest);
        }
    }

    private void DrawEventQuest(EventQuest eventQuest)
    {
        if (eventQuest.EndsAtUtc != DateTime.MaxValue)
        {
            string time = (eventQuest.EndsAtUtc - DateTime.UtcNow).Humanize(
                precision: 1,
                culture: CultureInfo.InvariantCulture,
                minUnit: TimeUnit.Minute,
                maxUnit: TimeUnit.Day);
            ImGui.Text($"{eventQuest.Name} ({time})");
        }
        else
            ImGui.Text(eventQuest.Name);

        List<ElementId> startableQuests = eventQuest.QuestIds.Where(x =>
                _questRegistry.IsKnownQuest(x) &&
                _questFunctions.IsReadyToAcceptQuest(x) &&
                x != _questController.StartedQuest?.Quest.Id &&
                x != _questController.NextQuest?.Quest.Id)
            .ToList();
        foreach (var questId in eventQuest.QuestIds)
        {
            if (_questFunctions.IsQuestComplete(questId))
                continue;

            using (ImRaii.PushId($"##EventQuestSelection{questId}"))
            {
                string questName = _questData.GetQuestInfo(questId).Name;
                if (startableQuests.Contains(questId) &&
                    _questRegistry.TryGetQuest(questId, out Quest? quest))
                {
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.Play))
                    {
                        _questController.SetNextQuest(quest);
                        _questController.Start("SeasonalEventSelection");
                    }

                    bool hovered = ImGui.IsItemHovered();

                    ImGui.SameLine();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(questName);
                    hovered |= ImGui.IsItemHovered();

                    if (hovered)
                        _questTooltipComponent.Draw(quest.Info);
                }
                else
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX());

                    var style = _uiUtils.GetQuestStyle(questId);
                    if (_uiUtils.ChecklistItem(questName, style.Color, style.Icon, ImGui.GetStyle().FramePadding.X))
                        _questTooltipComponent.Draw(_questData.GetQuestInfo(questId));
                }
            }
        }
    }

    public IEnumerable<ElementId> GetCurrentlyActiveEventQuests()
    {
        return GetActiveSeasonalQuests()
            .Where(q => (q as QuestInfo)?.SeasonalQuestExpiry is DateTime expiry && expiry >= DateTime.UtcNow
                     || (q is UnlockLinkQuestInfo uli && uli.QuestExpiry is DateTime uExpiry && uExpiry >= DateTime.UtcNow))
            .Select(q => q.QuestId)
            .Where(ShouldShowQuest);
    }

    private bool ShouldShowQuest(ElementId elementId) => !_questFunctions.IsQuestComplete(elementId) &&
                                                         !_questFunctions.IsQuestUnobtainable(elementId);

    private sealed record EventQuest(string Name, List<ElementId> QuestIds, DateTime EndsAtUtc);

    private IEnumerable<IQuestInfo> GetActiveSeasonalQuests()
    {
        var allQuestIds = _questRegistry.GetAllQuestIds();
        foreach (var questId in allQuestIds)
        {
            if (!_questData.TryGetQuestInfo(questId, out var q))
                continue;

            if (_questFunctions.IsQuestComplete(q.QuestId) || _questFunctions.IsQuestUnobtainable(q.QuestId))
                continue;

            if (q is UnlockLinkQuestInfo uli)
            {
                if (uli.QuestExpiry is DateTime uExpiry && uExpiry > DateTime.UtcNow)
                {
                    yield return q;
                    continue;
                }

                // no future expiry -> skip
                continue;
            }

            if (q is QuestInfo qi)
            {
                if (qi.SeasonalQuestExpiry is DateTime expiry && expiry > DateTime.UtcNow)
                {
                    yield return q;
                    continue;
                }

                if (qi.IsSeasonalQuest && qi.SeasonalQuestExpiry is null)
                {
                    yield return q;
                    continue;
                }
            }
        }
    }
}
