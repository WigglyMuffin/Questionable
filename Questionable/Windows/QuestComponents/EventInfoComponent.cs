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

    private List<IQuestInfo> _cachedActiveSeasonalQuests = new();
    private DateTime _cachedAtUtc = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(5);

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

    public bool ShouldDraw
    {
        get
        {
            if (!_configuration.General.ShowIncompleteSeasonalEvents)
                return false;
            UpdateCacheIfNeeded();
            return _cachedActiveSeasonalQuests.Count > 0;
        }
    }

    public void Draw()
    {
        UpdateCacheIfNeeded();

        // Collect active seasonal quests and group them by the folder (event) name if available
        var activeQuests = _cachedActiveSeasonalQuests;
        var groups = activeQuests.GroupBy(q =>
        {
            if (_questRegistry.TryGetQuestFolderName(q.QuestId, out var folder) && !string.IsNullOrEmpty(folder))
                return folder!;
            return q.Name;
        });

        foreach (var group in groups)
        {
            // pick the earliest expiry among group members to show as the group's expiry
            DateTime endsAt = group.Select(q =>
                (q as QuestInfo)?.SeasonalQuestExpiry
                ?? (q is UnlockLinkQuestInfo uli ? uli.QuestExpiry : (DateTime?)null)
                ?? DateTime.MaxValue)
                                  .DefaultIfEmpty(DateTime.MaxValue)
                                  .Min();

            // If all unlock-link quests in the group share the same patch, surface it for display
            var patches = group
                .Select(q => (q as UnlockLinkQuestInfo)?.Patch)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToList();
            string? patch = patches.Count == 1 ? patches[0] : null;

            var eventQuest = new EventQuest(group.Key, group.Select(q => q.QuestId).ToList(), endsAt, patch);
            DrawEventQuest(eventQuest);
        }
    }

    private void DrawEventQuest(EventQuest eventQuest)
    {
        string displayName = eventQuest.Name;
        if (!string.IsNullOrEmpty(eventQuest.Patch))
            displayName = $"{displayName} [{eventQuest.Patch}]";

        if (eventQuest.EndsAtUtc != DateTime.MaxValue)
        {
            string time = (eventQuest.EndsAtUtc - DateTime.UtcNow).Humanize(
                precision: 1,
                culture: CultureInfo.InvariantCulture,
                minUnit: TimeUnit.Minute,
                maxUnit: TimeUnit.Day);
            ImGui.Text($"{displayName} ({time})");
        }
        else
            ImGui.Text(displayName);

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
        UpdateCacheIfNeeded();

        return _cachedActiveSeasonalQuests
            .Where(q => (q as QuestInfo)?.SeasonalQuestExpiry is DateTime expiry && expiry >= DateTime.UtcNow
                     || (q is UnlockLinkQuestInfo uli && uli.QuestExpiry is DateTime uExpiry && uExpiry >= DateTime.UtcNow))
            .Select(q => q.QuestId)
            .Where(ShouldShowQuest);
    }

    private bool ShouldShowQuest(ElementId elementId) => !_questFunctions.IsQuestComplete(elementId) &&
                                                         !_questFunctions.IsQuestUnobtainable(elementId);

    private sealed record EventQuest(string Name, List<ElementId> QuestIds, DateTime EndsAtUtc, string? Patch);

    // Replaced original GetActiveSeasonalQuests with cached implementation that refreshes occasionally.
    private IEnumerable<IQuestInfo> GetActiveSeasonalQuestsNoCache()
    {
        // Only refresh the minimal set: iterate over known quest ids once per refresh interval.
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

    private void UpdateCacheIfNeeded()
    {
        if (DateTime.UtcNow - _cachedAtUtc < _cacheDuration)
            return;

        // Refresh cache
        _cachedActiveSeasonalQuests = GetActiveSeasonalQuestsNoCache().ToList();
        _cachedAtUtc = DateTime.UtcNow;
    }
}
