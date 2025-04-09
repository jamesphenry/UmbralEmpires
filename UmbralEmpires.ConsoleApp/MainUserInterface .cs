// Add this if missing
using Terminal.Gui;
// Other necessary using statements...
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UmbralEmpires.Application.Persistence;
using UmbralEmpires.Application.Services;
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UmbralEmpires.ConsoleApp.UI;

public class MainUserInterface : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly Guid _currentPlayerId;
    private readonly Guid _homeBaseId;
    private readonly StatusBar? _statusBar; // Store reference to the StatusBar

    // UI Element References... (as before)
    private Label? coordsLabel, energyLabel, popLabel, areaLabel;
    private Label? constrLabel, prodLabel, resrchLabel, creditsLabel;
    private FrameView? _statusFrame;
    private ListView? _structureListView;
    private ListView? _buildOptionsListView;
    private ListView? _queueListView;
    private Button? _buildButton;

    // Constructor accepts StatusBar
    public MainUserInterface(IServiceProvider serviceProvider, Guid playerId, Guid baseId, StatusBar? statusBar) : base(" Umbral Empires ")
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<MainUserInterface>();
        _currentPlayerId = playerId;
        _homeBaseId = baseId;
        _statusBar = statusBar; // Assign passed StatusBar

        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        SetupControls();
        SetupGameLoopTimer();
        TriggerInitialDataLoad();
    }

    private void SetupControls()
    {
        // --- Frames ---
        _statusFrame = new FrameView("Base Status") { /* ... Pos/Size ... */ };
        var queueFrame = new FrameView("Construction Queue") { /* ... Pos/Size ... */ };
        var structuresFrame = new FrameView("Structures") { /* ... Pos/Size ... */ };

        // --- Status Frame Content ---
        creditsLabel = new Label("Credits: ?") { X = 1, Y = 0 }; /* ... other labels ... */
        coordsLabel = new Label("Coords: ?") { X = 1, Y = 1 }; energyLabel = new Label("Energy: ? / ?") { X = 1, Y = 2 }; popLabel = new Label("Pop:    ? / ?") { X = 1, Y = 3 }; areaLabel = new Label("Area:   ? / ?") { X = 1, Y = 4 }; constrLabel = new Label("Constr: ? /hr") { X = 1, Y = 6 }; prodLabel = new Label("Prod:   ? /hr") { X = 1, Y = 7 }; resrchLabel = new Label("Resrch: ? /hr") { X = 1, Y = 8 };
        _statusFrame.Add(creditsLabel, coordsLabel, energyLabel, popLabel, areaLabel, constrLabel, prodLabel, resrchLabel);

        // --- Structures Frame Content ---
        structuresFrame.Add(new Label(1, 0, "Existing:"));
        _structureListView = new ListView() { /* ... Pos/Size ... */ };
        structuresFrame.Add(_structureListView);

        // *** CORRECTED Label Creation and Positioning ***
        var buildOptionsTitleLabel = new Label("Build Options: [Cost] [Time]")
        { // Create with text only
            X = 1
            // Y position will be set relative to the ListView below
        };
        // Set Y relative to the structure list view AFTER the list view is created and added
        buildOptionsTitleLabel.Y = Pos.Bottom(_structureListView);
        structuresFrame.Add(buildOptionsTitleLabel); // Add the correctly positioned label

        // Make sure the Build Options ListView is positioned below this new label
        _buildOptionsListView = new ListView()
        {
            X = 1,
            Y = Pos.Bottom(buildOptionsTitleLabel), // Position below the title label
            Width = Dim.Fill(1),
            Height = Dim.Fill(2)
        };
        // The rest of the _buildOptionsListView setup and adding it remains the same...
        structuresFrame.Add(_buildOptionsListView);

        // The Button position calculation should also be relative to the _buildOptionsListView now
        _buildButton.Y = Pos.Bottom(_buildOptionsListView) + 1; // Position below the list

        _buildOptionsListView = new ListView() { /* ... Pos/Size ... */ };
        structuresFrame.Add(_buildOptionsListView);

        _buildButton = new Button("(B)uild")
        { // Create with text only
            X = Pos.Center(),                 // Center horizontally is fine
            IsDefault = true
            // Y position set AFTER _buildOptionsListView exists
        };
        // Set Y relative to the build options list view
        _buildButton.Y = Pos.Bottom(_buildOptionsListView) + 1;
        structuresFrame.Add(_buildButton);
        _buildButton.Clicked += BuildButton_Clicked;
        structuresFrame.Add(_buildButton);
        // *** FIX 2: Ensure handler signature matches event (parameterless Action) ***
        _buildButton.Clicked += BuildButton_Clicked;

        // --- Queue Frame Content ---
        _queueListView = new ListView() { /* ... Pos/Size ... */ };
        queueFrame.Add(_queueListView);

        // --- Add Frames ---
        this.Add(_statusFrame, structuresFrame, queueFrame);
    }

    // Inside MainUserInterface.cs

    // Correct Signature for Button.Clicked (void)
    private void BuildButton_Clicked()
    {
        // 1. Check if an item is selected in the build options list
        // Also check Source is not null and index is valid
        if (_buildOptionsListView?.SelectedItem >= 0 &&
            _buildOptionsListView.Source?.Count > _buildOptionsListView.SelectedItem)
        {
            // 2. Get the string representation of the selected item
            var selectedText = _buildOptionsListView.Source.ToList()[_buildOptionsListView.SelectedItem]?.ToString() ?? "";

            // 3. Parse the StructureType from the string
            //    WARNING: This parsing is fragile! It depends heavily on the format:
            //    "[X] StructureType (LTarget) [Cost C] [Time]"
            var parts = selectedText.Trim().Split(' '); // Trim leading space first
            StructureType structureToBuild = default;
            bool parseSuccess = false;
            if (parts.Length > 1)
            {
                // The StructureType name should be the first element after splitting
                parseSuccess = Enum.TryParse<StructureType>(parts[0], out structureToBuild);
                // If the first element might be 'X', try the second
                if (!parseSuccess && parts[0] == "X" && parts.Length > 2)
                {
                    parseSuccess = Enum.TryParse<StructureType>(parts[1], out structureToBuild);
                }
            }

            if (parseSuccess)
            {
                _logger.LogInformation("Build button clicked, attempting to queue {Structure}", structureToBuild);
                SetStatusMessage($"Attempting to queue {structureToBuild}..."); // Immediate UI feedback

                // 4. Call Construction Service asynchronously in background (fire and forget Task.Run)
                _ = Task.Run(async () => {
                    QueueResult result;
                    try
                    {
                        // Create a DI scope for the background task to resolve services
                        using var scope = _serviceProvider.CreateScope();
                        var constructionService = scope.ServiceProvider.GetRequiredService<IConstructionService>();

                        // Call the service method
                        result = await constructionService.QueueStructureAsync(_currentPlayerId, _homeBaseId, structureToBuild);
                    }
                    catch (Exception ex) // Catch exceptions during service call
                    {
                        _logger.LogError(ex, "Exception calling ConstructionService for {Structure}", structureToBuild);
                        result = new QueueResult(false, "Error occurred during queue attempt.");
                    }

                    // 5. Update status bar on the UI thread with the result message
                    SetStatusMessage($"Status: {result.Message}");

                    // The UI will update automatically on the next tick if the queue changed & SaveChanges succeeds.
                });
            }
            else
            {
                // Parsing failed
                _logger.LogWarning("Could not parse StructureType from selected build option: {SelectedText}", selectedText);
                SetStatusMessage($"Status: Error parsing selection!");
            }
        }
        else
        {
            // No item selected
            SetStatusMessage($"Status: No build option selected.");
        }
    }

    private void SetupGameLoopTimer()
    {
        TimeSpan tickInterval = TimeSpan.FromSeconds(1);
        double gameHoursPerRealSecond = 1.0;
        Terminal.Gui.Application.MainLoop.AddTimeout(tickInterval, (mainLoop) => {
            _ = Task.Run(async () => { /* ... Tick Logic ... */ });
            // *** FIX 3: Correct Return Value for Timer Continuation ***
            return true; // Keep timer running
        });
    }

    private void TriggerInitialDataLoad() { /* ... (as before) ... */ }
    private async Task UpdateUIData() { /* ... (as before, using .Text and explicit null checks) ... */ }

    // *** FIX 4: Corrected SetStatusMessage using passed StatusBar reference ***
    private void SetStatusMessage(string message)
    {
        Terminal.Gui.Application.MainLoop.Invoke(() => {
            // Assumes the message item is the second item (index 1) in the StatusBar
            if (_statusBar?.Items?.Length > 1)
            { // Check _statusBar and Items array
                _statusBar.Items[1].Title = message ?? ""; // Update the correct item's Title (use empty string if message is null)
                _statusBar.SetNeedsDisplay(); // Redraw the status bar itself
            }
        });
    }

    // FormatTime Helper (ensure final return)
    private static string FormatTime(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds <= 0) return "0s";
        if (timeSpan.TotalHours >= 1) return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes:D2}m {timeSpan.Seconds:D2}s";
        if (timeSpan.TotalMinutes >= 1) return $"{timeSpan.Minutes}m {timeSpan.Seconds:D2}s";
        return $"{Math.Max(1, Math.Ceiling(timeSpan.TotalSeconds))}s";
    }

} // End of class